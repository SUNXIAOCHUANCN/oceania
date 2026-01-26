using UnityEngine;

/// <summary>
/// 信物解锁NPC - 用于酋长等可以解锁岛屿信物的特殊NPC
/// 在对话选项选择正确后调用UnlockToken()来解锁对应岛屿的信物
/// </summary>
public class TokenUnlockerNPC : MonoBehaviour
{
    [Header("探索系统配置")]
    [SerializeField] private SpeciesSource targetIsland;  // 目标岛屿（枚举）
    [SerializeField] private ProgressTableManager customProgressTableManager;  // 手动指定的管理器（可选，优先级高于targetIsland）

    [Header("调试")]
    [SerializeField] private bool showDebugLog = true;

    private ProgressTableManager cachedManager;
    private bool isManagerInitialized = false;

    #region 初始化

    private void Awake()
    {
        FindProgressTableManager();
    }

    /// <summary>
    /// 查找对应的ProgressTableManager
    /// </summary>
    private void FindProgressTableManager()
    {
        // 优先使用手动指定的管理器
        if (customProgressTableManager != null)
        {
            cachedManager = customProgressTableManager;
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 使用手动指定的ProgressTableManager: {cachedManager.GetIslandName()}");
            }
            return;
        }

        // 如果没有手动指定，通过ExploreSystem查找
        if (ExploreSystem.Instance != null)
        {
            var allManagers = ExploreSystem.Instance.GetAllProgressTableManagers();

            foreach (var manager in allManagers)
            {
                if (manager.GetIsland() == targetIsland)
                {
                    cachedManager = manager;
                    if (showDebugLog)
                    {
                        Debug.Log($"[{gameObject.name}] 自动找到ProgressTableManager: {manager.GetIslandName()}");
                    }
                    return;
                }
            }
        }

        // 如果还没找到，尝试在场景中查找
        if (cachedManager == null)
        {
            ProgressTableManager[] foundManagers = FindObjectsOfType<ProgressTableManager>();
            foreach (var manager in foundManagers)
            {
                if (manager.GetIsland() == targetIsland)
                {
                    cachedManager = manager;
                    if (showDebugLog)
                    {
                        Debug.Log($"[{gameObject.name}] 在场景中找到ProgressTableManager: {manager.GetIslandName()}");
                    }
                    return;
                }
            }
        }

        if (cachedManager == null)
        {
            Debug.LogError($"[{gameObject.name}] 未找到岛屿 '{targetIsland}' 的ProgressTableManager！\n" +
                          $"请手动指定或在场景中确保对应的ProgressTableManager存在");
        }

        isManagerInitialized = true;
    }

    #endregion

    #region 公开接口

    /// <summary>
    /// 解锁信物 - 供对话系统的UnityEvent调用
    /// </summary>
    public void UnlockToken()
    {
        // 确保管理器已初始化
        if (!isManagerInitialized || cachedManager == null)
        {
            FindProgressTableManager();
        }

        if (cachedManager == null)
        {
            Debug.LogError($"[{gameObject.name}] 无法解锁信物：ProgressTableManager未找到");
            return;
        }

        // 检查信物是否已解锁
        if (cachedManager.IsTokenUnlocked())
        {
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 信物已经解锁过了，跳过");
            }
            return;
        }

        // 解锁信物
        bool success = cachedManager.UnlockToken();

        if (success)
        {
            string islandName = cachedManager.GetIslandName();
            Debug.Log($"[{gameObject.name}] ✓ 成功解锁 '{islandName}' 的信物！");

            // 静默解锁，不显示额外提示
            // 信物会在海图志UI中显示
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 解锁信物失败");
        }
    }

    /// <summary>
    /// 检查信物是否已解锁
    /// </summary>
    public bool IsTokenUnlocked()
    {
        if (cachedManager == null)
        {
            FindProgressTableManager();
        }

        return cachedManager != null && cachedManager.IsTokenUnlocked();
    }

    /// <summary>
    /// 获取绑定的信物对象
    /// </summary>
    public TokenScriptableObject GetToken()
    {
        if (cachedManager == null)
        {
            FindProgressTableManager();
        }

        return cachedManager?.GetBoundToken();
    }

    #endregion

    #region 调试辅助

    /// <summary>
    /// 在Inspector中显示调试信息
    /// </summary>
    private string GetConfigInfo()
    {
        if (cachedManager == null)
        {
            return $"未找到管理器 (目标岛屿: {targetIsland})";
        }

        return $"岛屿: {cachedManager.GetIslandName()} | " +
               $"信物解锁: {cachedManager.IsTokenUnlocked()}";
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在Inspector中显示调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (cachedManager != null)
        {
            Gizmos.color = cachedManager.IsTokenUnlocked() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif

    #endregion
}
