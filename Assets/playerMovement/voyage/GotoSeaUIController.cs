using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 用于ESC键关闭（可选）

public class GotoSeaUIController : CanvasController
{
    [Header("船预制体设置")]
    [SerializeField] private GameObject shipPrefab1; // 第一种船
    [SerializeField] private GameObject shipPrefab2; // 第二种船
    [SerializeField] private GameObject shipPrefab3; // 第三种船

    [Header("按钮设置")]
    [SerializeField] private Button shipButton1; // 第一种船按钮
    [SerializeField] private Button shipButton2; // 第二种船按钮
    [SerializeField] private Button shipButton3; // 第三种船按钮
    [SerializeField] private Button closeButton; // 关闭按钮（拖入你的CloseButton即可）

    [Header("生成设置")]
    [SerializeField] private float spawnOffsetY = 0f; // Y轴偏移

    private PlayerController playerController;
    private voyagetrigger voyageTrigger; // 触发器引用

    private void Start()
    {
        Debug.Log($"[GotoSeaUIController] Start() 被调用，GameObject: {gameObject.name}, 当前状态: {gameObject.activeSelf}");

        // 绑定选船按钮事件
        if (shipButton1 != null) shipButton1.onClick.AddListener(() => OnShipSelected(1));
        else Debug.LogWarning("[GotoSeaUIController] shipButton1 未设置！");

        if (shipButton2 != null) shipButton2.onClick.AddListener(() => OnShipSelected(2));
        else Debug.LogWarning("[GotoSeaUIController] shipButton2 未设置！");

        if (shipButton3 != null) shipButton3.onClick.AddListener(() => OnShipSelected(3));
        else Debug.LogWarning("[GotoSeaUIController] shipButton3 未设置！");

        // 绑定关闭按钮事件（核心：拖入按钮即生效）
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);
        else Debug.LogWarning("[GotoSeaUIController] closeButton 未设置！请拖入关闭按钮");

        // 初始隐藏UI
        gameObject.SetActive(true);
        HideCanvas();
        Debug.Log($"[GotoSeaUIController] Start() 完成，GameObject 状态: {gameObject.activeSelf}");
    }

    // 每帧检测ESC键（可选：按ESC关闭UI）
    private void Update()
    {
        // 只有UI显示时，按ESC才触发关闭
        if (gameObject.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnCloseButtonClicked();
        }
    }

    // 设置玩家控制器
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }

    // 设置触发器引用
    public void SetVoyageTrigger(voyagetrigger trigger)
    {
        voyageTrigger = trigger;
    }

    // 显示UI
    public override void ShowCanvas()
    {
        Debug.Log($"[GotoSeaUIController] ShowCanvas() 被调用，当前状态: {gameObject.activeSelf}");
        base.ShowCanvas();
        // 解锁鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[GotoSeaUIController] 鼠标已解锁");
    }

    // 隐藏UI
    public override void HideCanvas()
    {
        base.HideCanvas();
        // 恢复鼠标锁定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 核心：关闭UI时通知触发器重置状态（避免误触发）
        if (voyageTrigger != null)
        {
            voyageTrigger.OnUIClosed();
            Debug.Log("[GotoSeaUIController] 已通知触发器重置状态");
        }
    }

    // 选船逻辑（不变）
    private Transform spawnPoint; // 从voyagetrigger获取的生成点

    /// <summary>
    /// 设置生成点
    /// </summary>
    public void SetSpawnPoint(Transform selectedSpawnPoint)
    {
        spawnPoint = selectedSpawnPoint;
    }
    
    private void OnShipSelected(int shipType)
    {
        GameObject selectedPrefab = shipType switch
        {
            1 => shipPrefab1,
            2 => shipPrefab2,
            3 => shipPrefab3,
            _ => null
        };

        if (selectedPrefab == null)
        {
            Debug.LogError($"船类型 {shipType} 的预制体未设置！");
            return;
        }

        // 生成位置
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject shipInstance = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        // 获取船的控制器
        RaftController raftController = shipInstance.GetComponentInChildren<RaftController>();
        if (raftController == null)
        {
            Debug.LogError($"生成的船 {shipInstance.name} 没有 RaftController 组件！");
            return;
        }

        // 玩家上船
        if (playerController != null)
        {
            playerController.ForceBoardRaft(raftController);
            Debug.Log($"已生成船类型 {shipType}，玩家已上船");
        }
        else
        {
            Debug.LogError("PlayerController 未设置！");
        }

        // 选船后关闭UI
        HideCanvas();
    }

    // 获取生成位置（从voyagetrigger获取的生成点）
    private Vector3 GetSpawnPosition()
    {
        if (spawnPoint != null) return spawnPoint.position + Vector3.up * spawnOffsetY;
        if (voyageTrigger != null) return voyageTrigger.transform.position + Vector3.up * spawnOffsetY;
        if (playerController != null) return playerController.transform.position + playerController.transform.forward * 5f + Vector3.up * spawnOffsetY;
        return Vector3.zero;
    }

    
    private void OnCloseButtonClicked()
    {
        Debug.Log("[GotoSeaUIController] 点击关闭按钮，隐藏UI");
        HideCanvas(); // 调用隐藏UI方法，自动重置触发器状态
    }
}
