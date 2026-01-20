using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 海图志按钮控制器 - 控制海图志界面的打开和关闭
/// 点击按钮打开界面，再次点击关闭界面
/// </summary>
public class ProgressTableButtonController : MonoBehaviour
{
    [Header("控制模式")]
    [SerializeField] private ControlMode controlMode = ControlMode.AutoFind;
    
    [Header("手动引用（当ControlMode为Manual时使用）")]
    [SerializeField] private ProgressTableUIController progressTableController;
    
    [Header("UI反馈设置")]
    [SerializeField] private bool changeButtonText = true;
    [SerializeField] private string openText = "打开海图志";
    [SerializeField] private string closeText = "关闭海图志";
    
    private Button button;
    private Text buttonText;
    
    public enum ControlMode
    {
        AutoFind,      // 自动查找场景中的ProgressTableUIController
        Manual,        // 手动指定引用
        UseUIManager   // 通过UIManager控制
    }
    
    private void Awake()
    {
        // 获取按钮组件
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("ProgressTableButtonController: 当前GameObject上没有Button组件！");
            return;
        }
        
        // 获取按钮文本组件（可选）
        if (changeButtonText)
        {
            buttonText = button.GetComponentInChildren<Text>();
            if (buttonText == null)
            {
                Debug.LogWarning("ProgressTableButtonController: 未找到Text组件，无法更新按钮文本");
                changeButtonText = false;
            }
        }
        
        // 设置按钮点击监听
        button.onClick.AddListener(OnButtonClicked);
    }
    
    private void Start()
    {
        // 初始化按钮文本
        UpdateButtonText();
    }
    
    /// <summary>
    /// 按钮点击事件处理
    /// </summary>
    private void OnButtonClicked()
    {
        ToggleProgressTable();
    }
    
    /// <summary>
    /// 切换海图志界面的显示状态
    /// </summary>
    public void ToggleProgressTable()
    {
        bool shouldShow = !IsProgressTableVisible();
        
        if (shouldShow)
        {
            ShowProgressTable();
        }
        else
        {
            HideProgressTable();
        }
        
        // 更新按钮文本
        UpdateButtonText();
    }
    
    /// <summary>
    /// 显示海图志界面
    /// </summary>
    public void ShowProgressTable()
    {
        switch (controlMode)
        {
            case ControlMode.AutoFind:
                var controller = FindProgressTableController();
                if (controller != null)
                {
                    controller.ShowCanvas();
                }
                break;
                
            case ControlMode.Manual:
                if (progressTableController != null)
                {
                    progressTableController.ShowCanvas();
                }
                else
                {
                    Debug.LogError("ProgressTableButtonController: 手动模式下未设置ProgressTableUIController引用！");
                }
                break;
                
            case ControlMode.UseUIManager:
                if (UIManager.Instance != null)
                {
                    var canvas = UIManager.Instance.GetCanvas(CanvasController.CanvasType.ExploreCanvas);
                    if (canvas != null)
                    {
                        canvas.ShowCanvas();
                    }
                    else
                    {
                        Debug.LogWarning("ProgressTableButtonController: UIManager中未找到ExploreCanvas");
                        // 回退到自动查找
                        var fallbackController = FindProgressTableController();
                        if (fallbackController != null)
                        {
                            fallbackController.ShowCanvas();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("ProgressTableButtonController: UIManager实例不存在，使用自动查找");
                    var fallbackController = FindProgressTableController();
                    if (fallbackController != null)
                    {
                        fallbackController.ShowCanvas();
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// 隐藏海图志界面
    /// </summary>
    public void HideProgressTable()
    {
        switch (controlMode)
        {
            case ControlMode.AutoFind:
                var controller = FindProgressTableController();
                if (controller != null)
                {
                    controller.HideCanvas();
                }
                break;
                
            case ControlMode.Manual:
                if (progressTableController != null)
                {
                    progressTableController.HideCanvas();
                }
                else
                {
                    Debug.LogError("ProgressTableButtonController: 手动模式下未设置ProgressTableUIController引用！");
                }
                break;
                
            case ControlMode.UseUIManager:
                if (UIManager.Instance != null)
                {
                    var canvas = UIManager.Instance.GetCanvas(CanvasController.CanvasType.ExploreCanvas);
                    if (canvas != null)
                    {
                        canvas.HideCanvas();
                    }
                    else
                    {
                        Debug.LogWarning("ProgressTableButtonController: UIManager中未找到ExploreCanvas");
                        // 回退到自动查找
                        var fallbackController = FindProgressTableController();
                        if (fallbackController != null)
                        {
                            fallbackController.HideCanvas();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("ProgressTableButtonController: UIManager实例不存在，使用自动查找");
                    var fallbackController = FindProgressTableController();
                    if (fallbackController != null)
                    {
                        fallbackController.HideCanvas();
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// 检查海图志界面是否可见
    /// </summary>
    public bool IsProgressTableVisible()
    {
        switch (controlMode)
        {
            case ControlMode.AutoFind:
                var controller = FindProgressTableController();
                return controller != null && controller.IsCanvasVisible();
                
            case ControlMode.Manual:
                return progressTableController != null && progressTableController.IsCanvasVisible();
                
            case ControlMode.UseUIManager:
                if (UIManager.Instance != null)
                {
                    var canvas = UIManager.Instance.GetCanvas(CanvasController.CanvasType.ExploreCanvas);
                    return canvas != null && canvas.IsCanvasVisible();
                }
                break;
        }
        
        return false;
    }
    
    /// <summary>
    /// 在场景中查找ProgressTableUIController
    /// </summary>
    private ProgressTableUIController FindProgressTableController()
    {
        var controller = FindObjectOfType<ProgressTableUIController>();
        if (controller == null)
        {
            Debug.LogError("ProgressTableButtonController: 在场景中未找到ProgressTableUIController！");
            Debug.LogError("请确保：");
            Debug.LogError("1. ExploreCanva已添加到场景中");
            Debug.LogError("2. ExploreCanva上已添加ProgressTableUIController组件");
        }
        return controller;
    }
    
    /// <summary>
    /// 更新按钮文本
    /// </summary>
    private void UpdateButtonText()
    {
        if (!changeButtonText || buttonText == null) return;
        
        if (IsProgressTableVisible())
        {
            buttonText.text = closeText;
        }
        else
        {
            buttonText.text = openText;
        }
    }
    
    /// <summary>
    /// 设置控制模式
    /// </summary>
    public void SetControlMode(ControlMode mode)
    {
        controlMode = mode;
    }
    
    /// <summary>
    /// 设置ProgressTableUIController引用
    /// </summary>
    public void SetProgressTableController(ProgressTableUIController controller)
    {
        progressTableController = controller;
    }
    
    /// <summary>
    /// 设置按钮文本
    /// </summary>
    public void SetButtonTexts(string open, string close)
    {
        openText = open;
        closeText = close;
        UpdateButtonText();
    }
}