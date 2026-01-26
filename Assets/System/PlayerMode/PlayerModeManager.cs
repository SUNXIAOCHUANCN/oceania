using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerModeManager : MonoBehaviour
{
    public static PlayerModeManager Instance { get; private set; }

    /// <summary>
    /// 位置变更事件
    /// </summary>
    public event Action<PlayerLocationState> OnLocationChanged;

    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private PlayerController playerController;

    [Header("位置触发器")]
    [SerializeField] private PlayerLocationTrigger[] locationTriggers; // 手动分配所有位置触发器

    private readonly HashSet<PlayerLocationState> activeLocations = new HashSet<PlayerLocationState>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (playerStateManager == null)
        {
            playerStateManager = PlayerStateManager.Instance;
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        // 验证触发器是否已分配
        if (locationTriggers == null || locationTriggers.Length == 0)
        {
            Debug.LogWarning("[PlayerModeManager] 未手动分配任何位置触发器！请在Inspector中分配。");
        }
        else
        {
            Debug.Log($"[PlayerModeManager] 已分配 {locationTriggers.Length} 个位置触发器");
        }

        // 通过坐标主动检测玩家位置
        StartCoroutine(InitializeLocationByPosition());
    }

    /// <summary>
    /// 通过坐标主动检测玩家所在位置
    /// </summary>
    private IEnumerator InitializeLocationByPosition()
    {
        // 等待一帧，确保玩家和触发器都已初始化
        yield return null;

        // 通过坐标检测玩家当前在哪个触发器内
        PlayerLocationState detectedLocation = CheckLocationByPosition();

        // 如果检测到了位置，直接设置
        if (detectedLocation != PlayerLocationState.海上)
        {
            activeLocations.Add(detectedLocation);
            SetLocation(detectedLocation);
            Debug.Log($"[PlayerModeManager] 主动检测到玩家位置: {detectedLocation}");
        }
        else
        {
            // 如果没有检测到任何触发器，检查是否在船上
            if (IsOnRaft())
            {
                SetLocation(PlayerLocationState.海上);
                Debug.Log("[PlayerModeManager] 玩家在船上，位置设为海上");
            }
            else if (activeLocations.Count > 0)
            {
                // 如果之前有触发器记录（OnTriggerEnter已触发），使用第一个
                foreach (var loc in activeLocations)
                {
                    SetLocation(loc);
                    break;
                }
            }
            else
            {
                // 使用默认位置（存档中的位置）
                Debug.Log("[PlayerModeManager] 使用存档中的位置");
            }
        }
    }

    /// <summary>
    /// 通过玩家坐标检测当前所在的触发器
    /// </summary>
    private PlayerLocationState CheckLocationByPosition()
    {
        if (playerController == null || locationTriggers == null)
        {
            return PlayerLocationState.海上;
        }

        Vector3 playerPosition = playerController.transform.position;

        // 遍历所有触发器，检查玩家是否在触发器范围内
        foreach (var trigger in locationTriggers)
        {
            if (trigger == null) continue;

            // 跳过海上触发器
            if (trigger.location == PlayerLocationState.海上) continue;

            // 获取触发器的碰撞体
            Collider triggerCollider = trigger.GetComponent<Collider>();
            if (triggerCollider == null || !triggerCollider.isTrigger) continue;

            // 检查玩家位置是否在触发器边界内
            if (triggerCollider.bounds.Contains(playerPosition))
            {
                Debug.Log($"[PlayerModeManager] 检测到玩家在触发器内: {trigger.location}");
                return trigger.location;
            }
        }

        return PlayerLocationState.海上;
    }

    public void OnEnterLocationTrigger(PlayerLocationState location)
    {
        activeLocations.Add(location);

        if (!IsOnRaft())
        {
            SetLocation(location);
        }
    }

    public void OnExitLocationTrigger(PlayerLocationState location)
    {
        activeLocations.Remove(location);

        // 始终更新位置，不管是否在船上
        UpdateLocationFromActiveTriggers();
    }

    public void OnBoardRaft()
    {
        SetLocation(PlayerLocationState.海上);
    }

    public void OnLeaveRaft()
    {
        UpdateLocationFromActiveTriggers();
    }

    private void UpdateLocationFromActiveTriggers()
    {
        if (activeLocations.Count > 0)
        {
            foreach (var loc in activeLocations)
            {
                SetLocation(loc);
                return;
            }
        }
        else
        {
            SetLocation(PlayerLocationState.海上);
        }
    }

    private void SetLocation(PlayerLocationState location)
    {
        if (playerStateManager == null) return;
        playerStateManager.SetCurrentLocation(location);

        // 触发位置变更事件
        OnLocationChanged?.Invoke(location);
    }

    private bool IsOnRaft()
    {
        return playerController != null && playerController.IsOnRaft();
    }
}

