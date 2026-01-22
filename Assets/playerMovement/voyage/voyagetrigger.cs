using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class voyagetrigger : MonoBehaviour
{
    [Header("航海设置")]
    [SerializeField] private RaftController targetRaft;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform boardPoint;

    [Header("UI设置")]
    [SerializeField] private GotoSeaUIController gotoSeaUI;

    [Header("交互设置")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Key interactKey = Key.F;
    [SerializeField] private bool showDebugLog = true;

    private bool playerInRange = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLog) Debug.Log($"[voyagetrigger] OnTriggerEnter: {other.name}, Tag: {other.tag}, Expected Tag: {playerTag}");

        if (!other.CompareTag(playerTag))
        {
            if (showDebugLog) Debug.Log($"[voyagetrigger] 标签不匹配，跳过");
            return;
        }

        // 缓存 playerController（仅在进入范围时）
        playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning($"[voyagetrigger] 在 {other.name} 上未找到 PlayerController 组件！");
            return;
        }

        playerInRange = true;
        if (showDebugLog) Debug.Log($"[voyagetrigger] 玩家进入航海触发范围，可按 F 打开选船界面。playerInRange = {playerInRange}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 修复1：退出范围时清空 playerController 引用
        playerController = null;
        playerInRange = false;

        // 修复2：退出范围时自动隐藏UI
        if (gotoSeaUI != null && gotoSeaUI.gameObject.activeSelf)
        {
            gotoSeaUI.HideCanvas(); // 需确保 GotoSeaUIController 有 HideCanvas 方法
            if (showDebugLog) Debug.Log("[voyagetrigger] 玩家离开范围，自动隐藏选船UI");
        }

        if (showDebugLog) Debug.Log("玩家离开航海触发范围");
    }

    private void Update()
    {
        // 严格前置条件：不在范围直接返回（无任何后续逻辑）
        if (!playerInRange || playerController == null)
        {
            if (showDebugLog && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[voyagetrigger] Update: playerInRange = {playerInRange}, playerController = {playerController != null}");
            }
            return;
        }

        if (Keyboard.current == null)
        {
            if (showDebugLog && Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("[voyagetrigger] Keyboard.current 为 null，Input System 可能未初始化");
            }
            return;
        }

        if (interactKey == Key.None)
        {
            if (showDebugLog) Debug.LogWarning("[voyagetrigger] interactKey 未设置");
            return;
        }

        // 仅在玩家按下按键且在范围内时触发
        if (Keyboard.current[interactKey].wasPressedThisFrame)
        {
            if (showDebugLog) Debug.Log($"[voyagetrigger] 检测到按键 {interactKey} 被按下");
            TryOpenShipSelectionUI();
        }
    }

    private void TryOpenShipSelectionUI()
    {
        if (showDebugLog) Debug.Log("[voyagetrigger] TryOpenShipSelectionUI 被调用");

        // 双重校验：确保 playerController 不为空
        if (playerController == null)
        {
            Debug.LogError("[voyagetrigger] 未找到 PlayerController！");
            return;
        }

        // 检查玩家是否已经在船上
        if (playerController.IsOnRaft())
        {
            if (showDebugLog) Debug.Log("[voyagetrigger] 玩家已经在船上，不允许打开选船界面");
            return;
        }

        // 查找 UI 控制器
        if (gotoSeaUI == null)
        {
            if (showDebugLog) Debug.Log("[voyagetrigger] 正在查找 GotoSeaUIController...");
            gotoSeaUI = FindObjectOfType<GotoSeaUIController>();
            if (gotoSeaUI == null)
            {
                Debug.LogError("[voyagetrigger] 未找到 GotoSeaUIController！请确保场景中有名为 GotoSeaCanva 的 GameObject，并且上面附加了 GotoSeaUIController 组件。");
                return;
            }
            if (showDebugLog) Debug.Log($"[voyagetrigger] 找到 GotoSeaUIController: {gotoSeaUI.gameObject.name}");
        }

        // 设置引用并显示UI
        gotoSeaUI.SetPlayerController(playerController);
        gotoSeaUI.SetVoyageTrigger(this);

        if (showDebugLog) Debug.Log($"[voyagetrigger] 准备显示 UI，当前 Canvas 状态: {gotoSeaUI.gameObject.activeSelf}");
        gotoSeaUI.ShowCanvas();
        if (showDebugLog) Debug.Log($"[voyagetrigger] 已调用 ShowCanvas()，当前 Canvas 状态: {gotoSeaUI.gameObject.activeSelf}");
    }

    // 可选：给 GotoSeaUIController 提供的「关闭UI」回调（比如UI上的关闭按钮调用）
    public void OnUIClosed()
    {
        playerInRange = false; // 关闭UI后重置范围标记，防止误触
        playerController = null;
    }
}