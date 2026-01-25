using UnityEngine;

/// <summary>
/// 探索奖励解锁NPC - 通用的解锁脚本，可以解锁秘密、信物或两者
/// 用于酋长等可以解锁岛屿探索奖励的特殊NPC
/// </summary>
public class ExploreRewardUnlockerNPC : MonoBehaviour
{
    [Header("探索系统配置")]
    [SerializeField] private SpeciesSource targetIsland;  // 目标岛屿（枚举）
    [SerializeField] private ProgressTableManager customProgressTableManager;  // 手动指定的管理器（可选，优先级高于targetIsland）

    [Header("解锁类型")]
    [SerializeField] private bool unlockSecret = false;  // 是否解锁秘密
    [SerializeField] private bool unlockToken = false;  // 是否解锁信物

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
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 解锁秘密失败");
        }
    }

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
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 解锁信物失败");
        }
    }

    /// <summary>
    /// 解锁所有选中的奖励（秘密+信物）
    /// </summary>
    public void UnlockAllRewards()
    {
        if (unlockSecret)
        {
            UnlockSecret();
        }

        if (unlockToken)
        {
            UnlockToken();
        }
    }

    #endregion

    #region 状态检查

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

#if UNITY_EDITOR
    /// <summary>
    /// 在Inspector中显示调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (cachedManager != null)
        {
            // 根据解锁状态显示颜色
            bool secretUnlocked = unlockSecret && cachedManager.IsSecretUnlocked();
            bool tokenUnlocked = unlockToken && cachedManager.IsTokenUnlocked();

            if (secretUnlocked && tokenUnlocked)
            {
                Gizmos.color = Color.green;  // 都解锁了
            }
            else if (secretUnlocked || tokenUnlocked)
            {
                Gizmos.color = Color.yellow;  // 部分解锁
            }
            else
            {
                Gizmos.color = Color.red;  // 都没解锁
            }

            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
#endif

    #endregion
}
