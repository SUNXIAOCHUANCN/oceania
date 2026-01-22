using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 交互提示UI管理器 - 使用ButtonCanvaController显示交互提示
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    private ButtonCanvaController buttonCanvas;
    private InteractableObject currentInteractable;
    private playerInputActions controls;

    private void Awake()
    {
        controls = new playerInputActions();
    }

    private void OnEnable()
    {
        if (controls != null)
        {
            controls.player.Enable();
        }
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.player.Disable();
        }
    }

    private void Start()
    {
        // 查找ButtonCanvaController
        buttonCanvas = FindObjectOfType<ButtonCanvaController>();
        if (buttonCanvas == null)
        {
            Debug.LogError("未找到 ButtonCanvaController，请确保场景中有该组件");
        }
    }

    private void Update()
    {
        // 检测 F 键输入（使用新的Input System）
        if (currentInteractable != null && controls != null)
        {
            if (controls.player.Interaction.WasPressedThisFrame())
            {
                OnInteract();
            }
        }
    }

    /// <summary>
    /// 显示交互提示
    /// </summary>
    public void ShowPrompt(InteractableObject interactable)
    {
        currentInteractable = interactable;

        if (buttonCanvas != null)
        {
            // 使用现有的PickButton作为交互提示
            buttonCanvas.ShowButton("pick");
            Debug.Log("显示交互提示按钮");
        }
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    public void HidePrompt()
    {
        currentInteractable = null;

        if (buttonCanvas != null)
        {
            // 隐藏所有按钮
            buttonCanvas.HideAllButtons();
            buttonCanvas.HideCanvas();
            Debug.Log("隐藏交互提示按钮");
        }
    }

    /// <summary>
    /// 执行交互（公开方法供ButtonCanvaController调用）
    /// </summary>
    public void OnInteract()
    {
        if (currentInteractable != null)
        {
            Debug.Log("触发交互");
            currentInteractable.ManualInteract();
            HidePrompt();
        }
    }
}
