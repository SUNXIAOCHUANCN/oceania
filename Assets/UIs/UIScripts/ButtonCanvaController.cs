using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonCanvaController : CanvasController
{
    [Header("Button Interaction")]
    [SerializeField] private UnityEvent onButtonClicked;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool hideAfterClick = true;
    
    [Header("Input Settings")]
    [SerializeField] private InputAction interactionAction;
    
    [Header("Button References")]
    [SerializeField] private GameObject farmButton;
    [SerializeField] private GameObject ranchButton;
    [SerializeField] private GameObject forestButton;
    [SerializeField] private GameObject pickButton;
    
    [Header("UI Controller References")]
    [SerializeField] private FarmUIController farmUIController;
    [SerializeField] private CanvasController ranchUIController;
    [SerializeField] private CanvasController forestUIController;
    [SerializeField] private CanvasController pickUIController;
    
    // 按钮组件引用
    private Button farmButtonComponent;
    private Button ranchButtonComponent;
    private Button forestButtonComponent;
    private Button pickButtonComponent;
    
    // 用于跟踪当前显示的按钮
    private string currentActiveButton = "";
    
    private void Start()
    {
        // 获取Interaction输入动作
        if (interactionAction == null)
        {
            // 尝试从PlayerInput组件获取Interaction动作
            var playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null)
            {
                var actions = playerInput.actions;
                if (actions != null)
                {
                    interactionAction = actions.FindAction("Interaction");
                }
            }
        }
        
        // 如果找不到输入动作，尝试使用playerInputActions
        if (interactionAction == null)
        {
            try
            {
                var playerInputActions = new playerInputActions();
                interactionAction = playerInputActions.player.Interaction;
            }
            catch
            {
                // 忽略错误，如果无法获取输入动作则继续运行
            }
        }
        
        // 初始化按钮状态
        HideAllButtons();
        
        // 设置按钮点击监听器
        SetupButtonListeners();
    }
    
    /// <summary>
    /// 设置按钮点击监听器
    /// </summary>
    private void SetupButtonListeners()
    {
        // 设置农场按钮监听器
        SetupButtonListener(farmButton, ref farmButtonComponent, OnFarmButtonClicked, "Farm");
        
        // 设置牧场按钮监听器
        SetupButtonListener(ranchButton, ref ranchButtonComponent, OnRanchButtonClicked, "Ranch");
        
        // 设置森林按钮监听器
        SetupButtonListener(forestButton, ref forestButtonComponent, OnForestButtonClicked, "Forest");
        
        // 设置拾取按钮监听器
        SetupButtonListener(pickButton, ref pickButtonComponent, OnPickButtonClicked, "Pick");
    }
    
    /// <summary>
    /// 为指定按钮设置点击监听器，如果缺少Button组件则自动添加
    /// </summary>
    private void SetupButtonListener(GameObject buttonObject, ref Button buttonComponent, UnityAction callback, string buttonName)
    {
        if (buttonObject == null)
        {
            Debug.LogWarning($"{buttonName} button GameObject is null");
            return;
        }
        
        // 获取或添加Button组件
        buttonComponent = buttonObject.GetComponent<Button>();
        if (buttonComponent == null)
        {
            Debug.Log($"Adding Button component to {buttonName} button GameObject");
            buttonComponent = buttonObject.AddComponent<Button>();
            
            // 确保有图形组件用于交互
            var graphic = buttonObject.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic == null)
            {
                // 尝试获取任何图形组件，如果没有则添加Image
                var image = buttonObject.GetComponent<Image>();
                if (image == null)
                {
                    // 如果没有Image，添加一个默认Image组件
                    image = buttonObject.AddComponent<Image>();
                    image.color = new Color(1, 1, 1, 0.5f); // 半透明白色
                    Debug.Log($"Added default Image component to {buttonName} button for visual feedback");
                }
            }
        }
        
        // 设置点击监听器
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(callback);
        Debug.Log($"{buttonName} button listener added");
    }
    
    private void Update()
    {
        // 检查是否按下了F键（通过Interaction输入动作）
        if (interactionAction != null && interactionAction.triggered)
        {
            // 如果按钮Canvas是可见的，则触发对应按钮的点击事件
            if (IsCanvasVisible())
            {
                // 根据当前激活的按钮类型触发相应的操作
                switch (currentActiveButton)
                {
                    case "Farm":
                        OnFarmButtonClicked();
                        break;
                    case "Ranch":
                        OnRanchButtonClicked();
                        break;
                    case "Forest":
                        OnForestButtonClicked();
                        break;
                    case "Pick":
                        OnPickButtonClicked();
                        break;
                }
            }
        }
    }
    
    /// <summary>
    /// 显示指定类型的按钮并隐藏其他按钮
    /// </summary>
    public void ShowButton(string buttonType)
    {
        HideAllButtons();
        
        switch (buttonType.ToLower())
        {
            case "farm":
                if (farmButton != null)
                {
                    farmButton.SetActive(true);
                    currentActiveButton = "Farm";
                }
                break;
            case "ranch":
                if (ranchButton != null)
                {
                    ranchButton.SetActive(true);
                    currentActiveButton = "Ranch";
                }
                break;
            case "forest":
                if (forestButton != null)
                {
                    forestButton.SetActive(true);
                    currentActiveButton = "Forest";
                }
                break;
            case "pick":
                if (pickButton != null)
                {
                    pickButton.SetActive(true);
                    currentActiveButton = "Pick";
                }
                break;
        }
        
        ShowCanvas();
    }
    
    /// <summary>
    /// 隐藏所有按钮
    /// </summary>
    public void HideAllButtons()
    {
        if (farmButton != null) farmButton.SetActive(false);
        if (ranchButton != null) ranchButton.SetActive(false);
        if (forestButton != null) forestButton.SetActive(false);
        if (pickButton != null) pickButton.SetActive(false);
        currentActiveButton = "";
    }
    
    /// <summary>
    /// 查找农场UI控制器（包括禁用的对象）
    /// </summary>
    private FarmUIController FindFarmUIController()
    {
        // 首先尝试使用序列化字段
        if (farmUIController != null)
        {
            return farmUIController;
        }
        
        // 尝试使用FindObjectOfType（默认只查找活动对象）
        FarmUIController found = FindObjectOfType<FarmUIController>();
        if (found != null)
        {
            return found;
        }
        
        // 如果找不到活动对象，尝试查找所有对象（包括禁用的）
        var allControllers = Resources.FindObjectsOfTypeAll<FarmUIController>();
        if (allControllers.Length > 0)
        {
            // 返回第一个找到的控制器
            return allControllers[0];
        }
        
        // 最后尝试按名称查找GameObject
        GameObject farmUIGameObject = GameObject.Find("FieldProductionCanva");
        if (farmUIGameObject != null)
        {
            found = farmUIGameObject.GetComponent<FarmUIController>();
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 当农场按钮被点击时调用
    /// </summary>
    public void OnFarmButtonClicked()
    {
        Debug.Log("OnFarmButtonClicked called");
        
        // 查找农场UI控制器
        FarmUIController farmUI = FindFarmUIController();
        
        if (farmUI != null)
        {
            Debug.Log($"FarmUIController found: {farmUI.gameObject.name}, active: {farmUI.gameObject.activeSelf}");
            farmUI.ShowCanvas();
            Debug.Log($"After ShowCanvas, active: {farmUI.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogError("FarmUIController not found in scene!");
        }
        
        // 触发按钮点击事件
        onButtonClicked?.Invoke();
        
        // 如果设置为点击后隐藏，则隐藏Canvas
        if (hideAfterClick)
        {
            HideCanvas();
        }
    }
    
    /// <summary>
    /// 当牧场按钮被点击时调用
    /// </summary>
    public void OnRanchButtonClicked()
    {
        // 显示牧场UI
        if (ranchUIController != null)
        {
            ranchUIController.ShowCanvas();
        }
        else
        {
            // 如果找不到直接引用，可以在这里添加默认行为
            Debug.Log("Ranch UI controller not assigned, showing default ranch UI");
        }
        
        // 触发按钮点击事件
        onButtonClicked?.Invoke();
        
        // 如果设置为点击后隐藏，则隐藏Canvas
        if (hideAfterClick)
        {
            HideCanvas();
        }
    }
    
    /// <summary>
    /// 当森林按钮被点击时调用
    /// </summary>
    public void OnForestButtonClicked()
    {
        // 显示森林UI
        if (forestUIController != null)
        {
            forestUIController.ShowCanvas();
        }
        else
        {
            // 如果找不到直接引用，可以在这里添加默认行为
            Debug.Log("Forest UI controller not assigned, showing default forest UI");
        }
        
        // 触发按钮点击事件
        onButtonClicked?.Invoke();
        
        // 如果设置为点击后隐藏，则隐藏Canvas
        if (hideAfterClick)
        {
            HideCanvas();
        }
    }
    
    /// <summary>
    /// 当拾取按钮被点击时调用
    /// </summary>
    public void OnPickButtonClicked()
    {
        // 显示拾取UI
        if (pickUIController != null)
        {
            pickUIController.ShowCanvas();
        }
        else
        {
            // 如果找不到直接引用，可以在这里添加默认行为
            Debug.Log("Pick UI controller not assigned, showing default pick UI");
        }
        
        // 触发按钮点击事件
        onButtonClicked?.Invoke();
        
        // 如果设置为点击后隐藏，则隐藏Canvas
        if (hideAfterClick)
        {
            HideCanvas();
        }
    }
    
    /// <summary>
    /// 当按钮被点击时调用（通用方法）
    /// </summary>
    public void OnButtonClick()
    {
        // 根据当前激活的按钮类型触发相应的操作
        if (currentActiveButton == "Farm")
        {
            OnFarmButtonClicked();
        }
        else if (currentActiveButton == "Ranch")
        {
            OnRanchButtonClicked();
        }
        else if (currentActiveButton == "Forest")
        {
            OnForestButtonClicked();
        }
        else if (currentActiveButton == "Pick")
        {
            OnPickButtonClicked();
        }
    }
    
    /// <summary>
    /// 显示按钮Canvas并重置状态
    /// </summary>
    public void ShowButtonCanvas()
    {
        ShowCanvas();
    }
    
    /// <summary>
    /// 隐藏按钮Canvas
    /// </summary>
    public void HideButtonCanvas()
    {
        HideCanvas();
    }
    
    /// <summary>
    /// 设置按钮点击事件监听器
    /// </summary>
    public void SetButtonClickedListener(UnityAction listener)
    {
        onButtonClicked.AddListener(listener);
    }
    
    /// <summary>
    /// 移除按钮点击事件监听器
    /// </summary>
    public void RemoveButtonClickedListener(UnityAction listener)
    {
        onButtonClicked.RemoveListener(listener);
    }
    
    /// <summary>
    /// 清除所有按钮点击事件监听器
    /// </summary>
    public void ClearButtonClickedListeners()
    {
        onButtonClicked.RemoveAllListeners();
    }
}