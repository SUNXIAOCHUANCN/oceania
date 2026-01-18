using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeUIManager : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text totalTimeText;
    public TMP_Text phasesText; // 新增显示totalPhases的文本组件
    public Image moonPhaseImage;
    public Button resetButton;

    [Header("月相 Sprite 配置")]
    public Sprite crescentSprite;
    public Sprite upQuarterSprite;
    public Sprite fullMoonSprite;
    public Sprite downQuarterSprite;

    private GlobalTimeSystem timeSystem;
    private float lastUpdateTime = -1f; // 上次更新时间（秒）
    private const float UPDATE_INTERVAL = 1f; // 每1秒更新一次

    void Awake()
    {
        timeSystem = GlobalTimeSystem.Instance;
        if (timeSystem == null)
        {
            Debug.LogError("[TimeUIManager] GlobalTimeSystem not found! Disabling UI.");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        // 初始化 UI
        RefreshTotalTime();
        RefreshMoonPhase();
        RefreshPhases(); // 初始化显示totalPhases

        // 订阅月相变化事件
        timeSystem.OnPhaseChanged += OnMoonPhaseChanged;
        timeSystem.OnPhaseChangedWithTotalPhases += OnMoonPhaseChangedWithTotalPhases;
    }

    void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnPhaseChanged -= OnMoonPhaseChanged;
            timeSystem.OnPhaseChangedWithTotalPhases -= OnMoonPhaseChangedWithTotalPhases;
        }
    }

    void Update()
    {
        // 每隔 1 秒更新一次总时间（避免每帧刷新造成视觉闪烁）
        if (timeSystem != null && !timeSystem.isPaused)
        {
            float currentSeconds = Mathf.Floor(timeSystem.TotalElapsedTime);
            if (currentSeconds != lastUpdateTime)
            {
                RefreshTotalTime();
                lastUpdateTime = currentSeconds;
            }
        }
    }

    void RefreshTotalTime()
    {
        if (totalTimeText != null && timeSystem != null)
        {
            totalTimeText.text = timeSystem.GetFormattedTotalTime();
        }
    }

    void RefreshMoonPhase()
    {
        if (moonPhaseImage == null || timeSystem == null) return;

        var phase = timeSystem.GetCurrentMoonPhase();
        moonPhaseImage.sprite = phase switch
        {
            GlobalTimeSystem.MoonPhase.Crescent => crescentSprite,
            GlobalTimeSystem.MoonPhase.UpQuarterMoon => upQuarterSprite,
            GlobalTimeSystem.MoonPhase.FullMoon => fullMoonSprite,
            GlobalTimeSystem.MoonPhase.DownQuarterMoon => downQuarterSprite,
            _ => crescentSprite
        };
    }

    void RefreshPhases()
    {
        if (phasesText != null && timeSystem != null)
        {
            phasesText.text = timeSystem.TotalPhases.ToString();
        }
    }

    void OnMoonPhaseChanged(GlobalTimeSystem.MoonPhase newPhase)
    {
        RefreshMoonPhase();
    }

    void OnMoonPhaseChangedWithTotalPhases(GlobalTimeSystem.MoonPhase newPhase, int totalPhases)
    {
        RefreshPhases(); // 当月相改变时刷新totalPhases显示
    }

    public void ButtonTest()
    {
        Debug.Log("Button Test");
    }
}