using UnityEngine;

/// <summary>
/// 航海系统管理器 - 统一管理航海相关逻辑
/// </summary>
public class VoyageSystemManager : MonoBehaviour
{
    public static VoyageSystemManager Instance { get; private set; }

    [Header("船只资源消耗配置")]
    [SerializeField] private ShipResourceCost[] shipCosts;
    [SerializeField] private bool ignoreResourceCost = false; // 临时忽略资源消耗（用于测试）

    [Header("安全重生点")]
    [SerializeField] private Transform safeRespawnPoint; // 玩家在船上退出时的重生点

    [Header("下船配置")]
    [SerializeField] private float leaveRaftOffset = 2f; // 下船时距离船的距离

    [Header("调试设置")]
    [SerializeField] private bool showDebugLog = true;

    [Header("UI引用")]
    [SerializeField] private ButtonCanvaController buttonCanvaController;

    // 当前船只信息（用于存档）
    private RaftController currentRaft;
    private int currentShipType = 0;
    private bool isOnRaft = false;

    // 下船按钮状态跟踪
    private bool wasNearIsland = false;

    public bool IsOnRaft => isOnRaft;
    public RaftController CurrentRaft => currentRaft;
    public int CurrentShipType => currentShipType;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        // 只有在船上才检测
        if (!isOnRaft || currentRaft == null) return;

        // 检测船只是否靠近岛屿
        bool isNearIsland = CheckRaftNearIsland();

        // 状态变化时更新UI
        if (isNearIsland != wasNearIsland)
        {
            if (isNearIsland)
            {
                ShowLeaveRaftButton();
            }
            else
            {
                HideLeaveRaftButton();
            }
            wasNearIsland = isNearIsland;
        }
    }

    /// <summary>
    /// 尝试生成船只并上船
    /// </summary>
    public bool TrySpawnAndBoardRaft(int shipType, GameObject shipPrefab, Vector3 spawnPosition, PlayerController playerController)
    {
        if (showDebugLog) Debug.Log($"[VoyageSystemManager] 尝试生成船只类型 {shipType}");

        // 1. 检查资源是否足够（如果开启忽略，则跳过）
        if (!ignoreResourceCost)
        {
            ShipResourceCost cost = GetShipCost(shipType);
            if (cost != null)
            {
                if (!ConsumeResources(cost))
                {
                    Debug.LogWarning($"[VoyageSystemManager] 资源不足，无法生成船只。需要: Crop={cost.cropCost}, Ani={cost.aniCost}, Mat={cost.matCost}");
                    return false;
                }
                if (showDebugLog) Debug.Log($"[VoyageSystemManager] 资源检查通过，已消耗: Crop={cost.cropCost}, Ani={cost.aniCost}, Mat={cost.matCost}");
            }
        }
        else
        {
            if (showDebugLog) Debug.Log("[VoyageSystemManager] 已启用忽略资源消耗模式，跳过资源检查");
        }

        // 2. 生成船
        GameObject shipInstance = Object.Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
        RaftController raftController = shipInstance.GetComponentInChildren<RaftController>();
        if (raftController == null)
        {
            Debug.LogError($"[VoyageSystemManager] 生成的船没有 RaftController 组件！");
            Object.Destroy(shipInstance);
            return false;
        }

        // 3. 设置船只类型和唯一名称
        raftController.gameObject.name = $"Raft_ShipType{shipType}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        // 4. 玩家上船
        if (playerController != null)
        {
            playerController.ForceBoardRaft(raftController);
            SetCurrentRaft(raftController, shipType);

            // 通知PlayerModeManager更新位置状态为"海上"
            if (PlayerModeManager.Instance != null)
            {
                PlayerModeManager.Instance.OnBoardRaft();
                if (showDebugLog) Debug.Log("[VoyageSystemManager] 已通知 PlayerModeManager.OnBoardRaft()");
            }

            // 通知PlayerStateManager更新位置状态
            if (PlayerStateManager.Instance != null)
            {
                PlayerStateManager.Instance.SetCurrentLocation(PlayerLocationState.海上);
                if (showDebugLog) Debug.Log("[VoyageSystemManager] 已通知 PlayerStateManager.SetCurrentLocation(海上)");
            }

            Debug.Log($"[VoyageSystemManager] 已生成船类型 {shipType}，玩家已上船");

            // 上船成功后，隐藏出海按钮
            ButtonCanvaController btnController = Object.FindObjectOfType<ButtonCanvaController>();
            if (btnController != null)
            {
                btnController.HideAllButtons();
                if (showDebugLog) Debug.Log("[VoyageSystemManager] 上船成功，已隐藏出海按钮");
            }

            return true;
        }

        Debug.LogError("[VoyageSystemManager] PlayerController 为空，无法上船！");
        return false;
    }

    /// <summary>
    /// 玩家下船
    /// </summary>
    public void LeaveRaft()
    {
        if (!isOnRaft || currentRaft == null)
        {
            Debug.LogWarning("[VoyageSystemManager] 玩家不在船上，无法下船");
            return;
        }

        if (showDebugLog) Debug.Log("[VoyageSystemManager] 开始下船流程");

        // 1. 获取玩家控制器
        PlayerController playerController = Object.FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            // 尝试从当前船只获取（如果有引用）
            if (currentRaft != null)
            {
                // 查找所有PlayerController，找到在船上的那个
                PlayerController[] allControllers = Object.FindObjectsOfType<PlayerController>();
                foreach (var controller in allControllers)
                {
                    if (controller.IsOnRaft())
                    {
                        playerController = controller;
                        break;
                    }
                }
            }
        }

        if (playerController == null)
        {
            Debug.LogError("[VoyageSystemManager] 找不到PlayerController！");
            return;
        }

        // 先让玩家下船（这会恢复CharacterController等状态）
        playerController.ForceLeaveRaft();
        if (showDebugLog) Debug.Log("[VoyageSystemManager] 已调用 PlayerController.ForceLeaveRaft()");

        // 获取下船位置
        Vector3 raftPosition = currentRaft.transform.position;
        Vector3 leavePosition = raftPosition + currentRaft.transform.forward * leaveRaftOffset;

        if (showDebugLog) Debug.Log($"[VoyageSystemManager] 下船位置: {leavePosition}");

        // 传送玩家到下船位置（确保玩家在船的前方）
        playerController.transform.position = leavePosition;

        // 销毁船只
        string raftName = currentRaft.gameObject.name;
        Object.Destroy(currentRaft.gameObject);
        if (showDebugLog) Debug.Log($"[VoyageSystemManager] 已销毁船只: {raftName}");

        // 清空状态
        currentRaft = null;
        currentShipType = 0;
        isOnRaft = false;
        wasNearIsland = false; // 重置岛屿检测状态

        // 隐藏下船按钮
        HideLeaveRaftButton();

        // 通知PlayerModeManager更新位置状态
        if (PlayerModeManager.Instance != null)
        {
            PlayerModeManager.Instance.OnLeaveRaft();
            if (showDebugLog) Debug.Log("[VoyageSystemManager] 已通知 PlayerModeManager.OnLeaveRaft()");
        }

        Debug.Log($"[VoyageSystemManager] 玩家已下船，船只已销毁");
    }

    /// <summary>
    /// 检查是否可以下船
    /// </summary>
    public bool CanLeaveRaft()
    {
        if (!isOnRaft)
        {
            if (showDebugLog && Time.frameCount % 60 == 0)
            {
                Debug.Log("[VoyageSystemManager] CanLeaveRaft: 玩家不在船上");
            }
            return false;
        }

        if (currentRaft == null)
        {
            if (showDebugLog && Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("[VoyageSystemManager] CanLeaveRaft: currentRaft 为空");
            }
            return false;
        }

        // 方法1：检查玩家状态（如果玩家已经在岛屿触发器内）
        bool canLeaveByState = false;
        if (PlayerStateManager.Instance != null)
        {
            PlayerLocationState currentLocation = PlayerStateManager.Instance.CurrentLocation;
            canLeaveByState = currentLocation != PlayerLocationState.海上;
        }

        // 方法2：物理检测船只是否在岛屿触发器范围内
        bool canLeaveByPhysics = CheckRaftNearIsland();

        // 两种方法任一满足即可下船
        bool canLeave = canLeaveByState || canLeaveByPhysics;

        if (showDebugLog && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[VoyageSystemManager] CanLeaveRaft: 状态检测={canLeaveByState}, 物理检测={canLeaveByPhysics}, 最终={canLeave}");
        }

        return canLeave;
    }

    /// <summary>
    /// 物理检测船只是否靠近岛屿触发器
    /// </summary>
    private bool CheckRaftNearIsland()
    {
        if (currentRaft == null) return false;

        Vector3 raftPosition = currentRaft.transform.position;

        // 查找所有岛屿触发器
        PlayerLocationTrigger[] triggers = FindObjectsOfType<PlayerLocationTrigger>();
        foreach (var trigger in triggers)
        {
            // 检查是否是非海上的位置（岛屿）
            if (trigger.location == PlayerLocationState.海上) continue;

            // 获取触发器的碰撞体
            Collider triggerCollider = trigger.GetComponent<Collider>();
            if (triggerCollider == null) continue;

            // 检查船只是否在触发器范围内（使用OverlapBox检测船的位置）
            Vector3 triggerSize = triggerCollider.bounds.size;
            Vector3 triggerCenter = triggerCollider.bounds.center;

            // 检查船只中心点是否在触发器范围内（稍微扩大检测范围）
            if (triggerCollider.bounds.Contains(raftPosition))
            {
                if (showDebugLog && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[VoyageSystemManager] 检测到船只靠近岛屿: {trigger.location}");
                }
                return true;
            }

            // 或者检查船只是否靠近触发器边缘（容差检测）
            float distanceToTrigger = Vector3.Distance(raftPosition, triggerCenter);
            float triggerRadius = Mathf.Max(triggerSize.x, triggerSize.z) * 0.6f;
            if (distanceToTrigger < triggerRadius + 5f) // 额外5米容差
            {
                if (showDebugLog && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[VoyageSystemManager] 检测到船只靠近岛屿边缘: {trigger.location}, 距离={distanceToTrigger:F2}m");
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 显示下船按钮
    /// </summary>
    private void ShowLeaveRaftButton()
    {
        if (buttonCanvaController != null)
        {
            buttonCanvaController.ShowButton("leaveraft");
            if (showDebugLog) Debug.Log("[VoyageSystemManager] 显示下船按钮");
        }
        else
        {
            Debug.LogWarning("[VoyageSystemManager] ButtonCanvaController 未设置，无法显示下船按钮");
        }
    }

    /// <summary>
    /// 隐藏下船按钮
    /// </summary>
    private void HideLeaveRaftButton()
    {
        if (buttonCanvaController != null)
        {
            buttonCanvaController.HideAllButtons();
            if (showDebugLog) Debug.Log("[VoyageSystemManager] 隐藏下船按钮");
        }
    }

    /// <summary>
    /// 设置当前船只
    /// </summary>
    public void SetCurrentRaft(RaftController raft, int shipType)
    {
        currentRaft = raft;
        currentShipType = shipType;
        isOnRaft = (raft != null);

        // 重置岛屿检测状态
        wasNearIsland = false;

        if (showDebugLog)
        {
            Debug.Log($"[VoyageSystemManager] SetCurrentRaft: 船只={raft?.name}, 类型={shipType}, 在船上={isOnRaft}");
        }
    }

    /// <summary>
    /// 清空当前船只（用于加载存档时）
    /// </summary>
    public void ClearCurrentRaft()
    {
        if (showDebugLog && isOnRaft)
        {
            Debug.Log("[VoyageSystemManager] ClearCurrentRaft: 清空当前船只状态");
        }

        currentRaft = null;
        currentShipType = 0;
        isOnRaft = false;
    }

    /// <summary>
    /// 获取安全重生点
    /// </summary>
    public Transform GetSafeRespawnPoint()
    {
        return safeRespawnPoint;
    }

    /// <summary>
    /// 消耗资源
    /// </summary>
    private bool ConsumeResources(ShipResourceCost cost)
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("[VoyageSystemManager] ResourceManager 不存在！");
            return false;
        }

        // 依次尝试消耗三种资源
        bool cropOk = ResourceManager.Instance.ConsumeCrop(cost.cropCost);
        if (!cropOk)
        {
            Debug.LogWarning($"[VoyageSystemManager] Crop资源不足！需要: {cost.cropCost}, 当前: {ResourceManager.Instance.GetCropAmount()}");
            return false;
        }

        bool aniOk = ResourceManager.Instance.ConsumeAni(cost.aniCost);
        if (!aniOk)
        {
            // 回退Crop
            ResourceManager.Instance.AddCrop(cost.cropCost);
            Debug.LogWarning($"[VoyageSystemManager] Ani资源不足！需要: {cost.aniCost}, 当前: {ResourceManager.Instance.GetAniAmount()}");
            return false;
        }

        bool matOk = ResourceManager.Instance.ConsumeMat(cost.matCost);
        if (!matOk)
        {
            // 回退Crop和Ani
            ResourceManager.Instance.AddCrop(cost.cropCost);
            ResourceManager.Instance.AddAni(cost.aniCost);
            Debug.LogWarning($"[VoyageSystemManager] Mat资源不足！需要: {cost.matCost}, 当前: {ResourceManager.Instance.GetMatAmount()}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取船只资源消耗
    /// </summary>
    private ShipResourceCost GetShipCost(int shipType)
    {
        foreach (var cost in shipCosts)
        {
            if (cost.shipType == shipType)
                return cost;
        }

        if (showDebugLog)
        {
            Debug.LogWarning($"[VoyageSystemManager] 未找到船只类型 {shipType} 的资源消耗配置");
        }

        return null;
    }

    /// <summary>
    /// 获取船只资源消耗（公共方法）
    /// </summary>
    public ShipResourceCost GetShipCostPublic(int shipType)
    {
        return GetShipCost(shipType);
    }
}

/// <summary>
/// 船只资源消耗配置
/// </summary>
[System.Serializable]
public class ShipResourceCost
{
    public int shipType; // 1, 2, 3
    public float cropCost;
    public float aniCost;
    public float matCost;
}
