using UnityEngine;
using System;

public class PlayerLocationManager : MonoBehaviour
{
    public static PlayerLocationManager Instance { get; private set; }

    /// <summary>
    /// 玩家位置状态
    /// </summary>
    public enum LocationState
    {
        长尾鸟岛,
        十字星岛,
        海上
    }

    /// <summary>
    /// 位置变更事件
    /// </summary>
    public event Action<LocationState> OnLocationChanged;

    [Header("玩家信息")]
    [SerializeField] private Transform playerTransform;

    [Header("位置触发器")]
    [SerializeField] private Collider 长尾鸟岛Trigger;
    [SerializeField] private Collider 十字星岛Trigger;

    private LocationState currentLocation = LocationState.海上;

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
        if (playerTransform == null)
        {
            Debug.LogWarning("[PlayerLocationManager] Player Transform 未设置！");
            return;
        }

        // 每帧检测玩家位置
        LocationState newLocation = DetectLocation();

        // 如果位置改变，更新并触发事件
        if (newLocation != currentLocation)
        {
            currentLocation = newLocation;
            OnLocationChanged?.Invoke(currentLocation);
        }
    }

    /// <summary>
    /// 检测玩家当前在哪个位置
    /// </summary>
    private LocationState DetectLocation()
    {
        Vector3 playerPosition = playerTransform.position;

        // 优先检测长尾鸟岛
        if (长尾鸟岛Trigger != null && 长尾鸟岛Trigger.bounds.Contains(playerPosition))
        {
            return LocationState.长尾鸟岛;
        }

        // 检测十字星岛
        if (十字星岛Trigger != null && 十字星岛Trigger.bounds.Contains(playerPosition))
        {
            return LocationState.十字星岛;
        }

        // 都不在，就是海上
        return LocationState.海上;
    }

    /// <summary>
    /// 获取当前位置状态
    /// </summary>
    public LocationState GetCurrentLocation()
    {
        return currentLocation;
    }
}
