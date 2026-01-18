using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    private bool IsinteractionPanelOpen = false;

    private void Awake()
    {
        Instance = this;
    }
    // 初始化时默认隐藏
    void Start()
    {
        SetCursorState(false);
    }

    /// <summary>
    /// 核心逻辑：判断当前光标是否应该显示
    /// </summary>
    /// <param name="isAltPressed">传入PlayerController检测到的Alt键状态</param>
    public void UpdateCursorLogic(bool isAltPressed)
    {
        // 如果有交互面板打开，或者按下了Alt键，则显示光标
        bool shouldShow = IsinteractionPanelOpen || isAltPressed;

        if (shouldShow)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// 供交互面板调用：增加或减少强制显示光标的请求
    /// </summary>
    public void RegisterInteractionPanel(bool isOpen)
    {
        if (isOpen) IsinteractionPanelOpen = true;
        else IsinteractionPanelOpen = false;
        
        // 状态改变时立即刷新一次，防止等待下一帧Update
        UpdateCursorLogic(false); 
    }

    /// <summary>
    /// 外部接口：设置鼠标状态
    /// </summary>
    /// <param name="isVisible">true=显示鼠标(此时不能转视角), false=隐藏鼠标(锁定并可以转视角)</param>
    public void SetCursorState(bool isVisible)
    {
        RegisterInteractionPanel(isVisible);
    }
}