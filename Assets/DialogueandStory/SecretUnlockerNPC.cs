using UnityEngine;

/// <summary>
/// 秘密解锁NPC - 用于酋长等可以解锁岛屿秘密的特殊NPC
/// 在对话选项选择正确后调用UnlockSecret()来解锁对应岛屿的秘密
/// </summary>
public class SecretUnlockerNPC : MonoBehaviour
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
    /// 解锁秘密 - 供对话系统的UnityEvent调用
    /// </summary>
    public void UnlockSecret()
    {
        // 确保管理器已初始化
        if (!isManagerInitialized || cachedManager == null)
        {
            FindProgressTableManager();
        }

        if (cachedManager == null)
        {
            Debug.LogError($"[{gameObject.name}] 无法解锁秘密：ProgressTableManager未找到");
            return;
        }

        // 检查秘密是否已解锁
        if (cachedManager.IsSecretUnlocked())
        {
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 秘密已经解锁过了，跳过");
            }
            return;
        }

        // 解锁秘密
        bool success = cachedManager.UnlockSecret();

        if (success)
        {
            string islandName = cachedManager.GetIslandName();
            Debug.Log($"[{gameObject.name}] ✓ 成功解锁 '{islandName}' 的秘密！");

            // 静默解锁，不显示额外提示
            // 秘密内容会在海图志UI中显示
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 解锁秘密失败");
        }
    }

    /// <summary>
    /// 检查秘密是否已解锁
    /// </summary>
    public bool IsSecretUnlocked()
    {
        if (cachedManager == null)
        {
            FindProgressTableManager();
        }

        return cachedManager != null && cachedManager.IsSecretUnlocked();
    }

    /// <summary>
    /// 获取绑定的秘密对象
    /// </summary>
    public SecretScriptableObject GetSecret()
    {
        if (cachedManager == null)
        {
            FindProgressTableManager();
        }

        return cachedManager?.GetBoundSecret();
    }

    #endregion

    #region 调试辅助

    /// <summary>
    /// 获取当前配置的信息（用于Inspector调试）
    /// </summary>
    private string GetConfigInfo()
    {
        if (cachedManager == null)
        {
            return $"未找到管理器 (目标岛屿: {targetIsland})";
        }

        return $"岛屿: {cachedManager.GetIslandName()} | " +
               $"秘密解锁: {cachedManager.IsSecretUnlocked()}";
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在Inspector中显示调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (cachedManager != null)
        {
            Gizmos.color = cachedManager.IsSecretUnlocked() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif

    #endregion
}
