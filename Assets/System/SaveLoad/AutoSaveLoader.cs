using UnityEngine;

/// <summary>
/// 自动加载存档 - 游戏启动时自动加载存档
/// </summary>
public class AutoSaveLoader : MonoBehaviour
{
    [Header("自动加载设置")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private float loadDelay = 0.5f; // 延迟加载，确保所有系统已初始化

    void Start()
    {
        if (autoLoadOnStart)
        {
            Invoke(nameof(LoadAllData), loadDelay);
        }
    }

    private void LoadAllData()
    {
        if (GlobalSaveManager.Instance != null)
        {
            GlobalSaveManager.Instance.LoadAllData();
        }
        else
        {
            Debug.LogError("[AutoSaveLoader] GlobalSaveManager.Instance 为 null，无法加载存档");
        }
    }
}
