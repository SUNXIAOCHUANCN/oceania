using System;
using System.Collections.Generic;
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

        if (!IsOnRaft())
        {
            UpdateLocationFromActiveTriggers();
        }
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

