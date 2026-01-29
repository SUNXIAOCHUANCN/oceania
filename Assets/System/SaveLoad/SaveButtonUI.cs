using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档按钮 UI - 玩家点击按钮进行存档
/// </summary>
public class SaveButtonUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Button saveButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button quitButton;
    [SerializeField] private float statusDisplayDuration = 2f;

    private void Start()
    {
        // 自动查找按钮（如果未手动赋值）
        if (saveButton == null)
            saveButton = GetComponentInChildren<Button>();

        // 绑定保存按钮事件
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    /// <summary>
    /// 保存按钮点击
    /// </summary>
    private void OnSaveButtonClicked()
    {
        if (GlobalSaveManager.Instance != null)
        {
            GlobalSaveManager.Instance.SaveAllData();
            ShowStatus("游戏已保存！", Color.green);
        }
        else
        {
            ShowStatus("保存失败：GlobalSaveManager 不存在", Color.red);
        }
    }

    /// <summary>
    /// 显示状态信息
    /// </summary>
    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            StartCoroutine(ShowStatusCoroutine(message, color));
        }
        else
        {
            Debug.Log($"[SaveButtonUI] {message}");
        }
    }

    private System.Collections.IEnumerator ShowStatusCoroutine(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;

        // 如果状态文字默认是隐藏的，显示它
        if (!statusText.gameObject.activeSelf)
            statusText.gameObject.SetActive(true);

        yield return new WaitForSeconds(statusDisplayDuration);

        // 隐藏状态文字
        statusText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // 清理事件监听
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("退出游戏");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
