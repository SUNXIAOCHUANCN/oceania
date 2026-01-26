using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 航海输入处理器 - 已弃用
/// 下船功能已迁移到 PlayerController.HandleDismountInput()
/// 此脚本保留仅为兼容性，实际功能已禁用
/// </summary>
[System.Obsolete("VoyageInputHandler已弃用，下船功能已迁移到PlayerController", true)]
public class VoyageInputHandler : MonoBehaviour
{
    [Header("输入设置")]
    [SerializeField] private playerInputActions inputActions;

    [Header("调试设置")]
    [SerializeField] private bool showDebugLog = true;

    private InputAction interactionAction;

    private void Awake()
    {
        Debug.LogWarning("[VoyageInputHandler] 此脚本已弃用！下船功能已迁移到PlayerController。请从场景中移除此组件。");

        // 禁用此组件
        this.enabled = false;
    }

    // 以下所有方法已禁用，功能已迁移到 PlayerController
    private void OnEnable() { }
    private void OnDisable() { }
    private void OnInteractionPerformed(InputAction.CallbackContext context) { }
    private void HandleLeaveRaftInput() { }
}
