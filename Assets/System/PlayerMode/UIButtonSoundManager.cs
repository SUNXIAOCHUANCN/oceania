using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI按钮音效管理器 - 自动为所有按钮添加点击音效
/// 使用方法：
/// 1. 创建空GameObject，挂载此脚本
/// 2. 在Inspector中设置buttonClickSound音效文件
/// 3. 运行游戏，所有按钮点击都会有音效
/// </summary>
public class UIButtonSoundManager : MonoBehaviour
{
    public static UIButtonSoundManager Instance { get; private set; }

    [Header("音效设置")]
    [SerializeField] private AudioClip buttonClickSound;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 创建AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    void Start()
    {
        // 为场景中所有现有按钮添加音效
        AddSoundToAllButtons();

        // 监听场景加载事件，为每个新场景的按钮添加音效
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 场景加载时为新场景的按钮添加音效
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        AddSoundToAllButtons();
    }

    /// <summary>
    /// 为场景中所有按钮添加音效监听
    /// </summary>
    private void AddSoundToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true); // true包含隐藏对象

        int count = 0;
        foreach (Button button in buttons)
        {
            // 为每个按钮添加音效组件（如果还没有）
            ButtonSoundEffect soundEffect = button.GetComponent<ButtonSoundEffect>();
            if (soundEffect == null)
            {
                soundEffect = button.gameObject.AddComponent<ButtonSoundEffect>();
                count++;
            }
        }

        Debug.Log($"[UIButtonSoundManager] 已为 {count} 个按钮添加点击音效组件");
    }

    /// <summary>
    /// 播放点击音效（由ButtonSoundEffect调用）
    /// </summary>
    public void PlayClickSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    /// <summary>
    /// 设置点击音效
    /// </summary>
    public void SetClickSound(AudioClip clip)
    {
        buttonClickSound = clip;
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    public void SetVolume(float vol)
    {
        volume = Mathf.Clamp01(vol);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}

/// <summary>
/// 按钮音效组件 - 自动添加到每个按钮上
/// </summary>
public class ButtonSoundEffect : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 调用管理器播放音效
        if (UIButtonSoundManager.Instance != null)
        {
            UIButtonSoundManager.Instance.PlayClickSound();
        }
    }
}
