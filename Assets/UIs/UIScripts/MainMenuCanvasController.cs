using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单控制器 - 管理开始场景的UI交互
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI按钮")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button quitButton;

    [Header("UI效果")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Text loadingText;

    [Header("场景名称配置")]
    [SerializeField] private string gameSceneName = "MainIsland";

    void Start()
    {
        // 确保按钮引用正确
        if (newGameButton == null || continueGameButton == null || quitButton == null)
        {
            Debug.LogError("[MainMenuController] 按钮引用未设置！请在Inspector中分配按钮");
            return;
        }

        // 检查是否有存档，启用/禁用"继续游戏"按钮
        bool hasSave = GlobalSaveManager.Instance.HasAnySaveData();
        continueGameButton.interactable = hasSave;

        // 如果没存档，可以改变按钮颜色提示
        if (!hasSave)
        {
            var colors = continueGameButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            continueGameButton.colors = colors;
        }

        // 绑定按钮事件
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueGameButton.onClick.AddListener(OnContinueGameClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        Debug.Log($"[MainMenuController] 初始化完成。检测到存档: {hasSave}");

        // 隐藏加载面板
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    /// <summary>
    /// 新游戏按钮点击事件
    /// </summary>
    /// <summary>
    /// 新游戏按钮点击事件
    /// </summary>
    public async void OnNewGameClicked()
    {
        Debug.Log("[MainMenuController] 点击新游戏");

        // 显示确认对话框（可选）
        // 这里可以直接执行，也可以添加确认对话框

        ShowLoading("正在准备新游戏...");

        // 等待一帧，确保UI更新
        await System.Threading.Tasks.Task.Delay(100);

        // 删除所有存档
        if (GlobalSaveManager.Instance != null)
        {
            GlobalSaveManager.Instance.DeleteAllSaveData();
            Debug.Log("[MainMenuController] 所有存档已删除");

            // 等待一小段时间确保文件操作完成
            await System.Threading.Tasks.Task.Delay(200);

            ShowLoading("正在加载游戏...");

            // 加载游戏场景
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("[MainMenuController] GlobalSaveManager.Instance 为 null！");
            HideLoading();
        }
    }

    /// <summary>
    /// 继续游戏按钮点击事件
    /// </summary>
    public void OnContinueGameClicked()
    {
        Debug.Log("[MainMenuController] 点击继续游戏");

        ShowLoading("正在加载存档...");

        // 直接加载游戏场景（GlobalSaveManager会自动加载存档）
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// 退出游戏按钮点击事件
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("[MainMenuController] 点击退出游戏");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    /// <summary>
    /// 显示加载界面
    /// </summary>
    private void ShowLoading(string message = "加载中...")
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingText != null)
                loadingText.text = message;
        }
    }

    /// <summary>
    /// 隐藏加载界面
    /// </summary>
    private void HideLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    void OnDestroy()
    {
        // 清理事件绑定
        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnNewGameClicked);
        if (continueGameButton != null)
            continueGameButton.onClick.RemoveListener(OnContinueGameClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
}
