using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单控制器 - 管理开始场景的UI交互
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI按钮")]
    [SerializeField] private GameObject newGameButton;
    [SerializeField] private GameObject continueGameButton;
    [SerializeField] private GameObject quitButton;

    void Start()
    {
        // 检查是否有存档，启用/禁用"继续游戏"按钮
        bool hasSave = GlobalSaveManager.Instance.HasAnySaveData();
        if (continueGameButton != null)
        {
            continueGameButton.SetActive(hasSave);
        }

        Debug.Log($"[MainMenuController] 检测到存档: {hasSave}");
    }

    /// <summary>
    /// 新游戏按钮点击事件
    /// </summary>
    public void OnNewGameClicked()
    {
        Debug.Log("[MainMenuController] 点击新游戏");

        // 删除所有存档
        if (GlobalSaveManager.Instance != null)
        {
            GlobalSaveManager.Instance.DeleteAllSaveData();
            Debug.Log("[MainMenuController] 所有存档已删除");

            // 加载游戏场景
            Time.timeScale = 1f;
            GlobalTimeSystem.Instance.Resume();
            SceneManager.LoadScene("MainIsland");
        }
        else
        {
            Debug.LogError("[MainMenuController] GlobalSaveManager.Instance 为 null！");
        }
    }

    /// <summary>
    /// 继续游戏按钮点击事件
    /// </summary>
    public void OnContinueGameClicked()
    {
        Debug.Log("[MainMenuController] 点击继续游戏");

        // 直接加载游戏场景（GlobalSaveManager会自动加载存档）
        Time.timeScale = 1f;
        GlobalTimeSystem.Instance.Resume();
        SceneManager.LoadScene("MainIsland");
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
}
