using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // 单例实例
    public static UIManager Instance { get; private set; }
    
    // Canvas控制器引用
    private CanvasController systemCanvas;
    private CanvasController buttonCanvas;
    private CanvasController dialogueCanvas;
    private CanvasController fieldProductionCanvas;
    private CanvasController ranchProductionCanvas;
    private CanvasController forestProductionCanvas;
    private CanvasController populationCanvas;
    
    // 存储所有Canvas的字典，便于快速访问
    private Dictionary<CanvasController.CanvasType, CanvasController> canvasDictionary 
        = new Dictionary<CanvasController.CanvasType, CanvasController>();
    
    private void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        // 如需跨场景保留，取消下一行的注释
        // DontDestroyOnLoad(gameObject);
        
        // 自动查找并初始化所有Canvas控制器
        InitializeCanvases();
    }

    void start()
    {
        // 显示系统Canvas
        ShowSystemCanvas();
    }
    
    /// <summary>
    /// 查找场景中所有CanvasController并初始化
    /// </summary>
    private void InitializeCanvases()
    {
        // 查找所有CanvasController（包括未激活的）
        CanvasController[] allCanvasControllers = FindObjectsOfType<CanvasController>(true);
        
        foreach (CanvasController controller in allCanvasControllers)
        {
            // 添加到字典以便快速访问
            canvasDictionary[controller.Type] = controller;
            
            // 根据类型赋值给对应的字段
            switch (controller.Type)
            {
                case CanvasController.CanvasType.SystemCanvas:
                    systemCanvas = controller;
                    break;
                case CanvasController.CanvasType.ButtonCanvas:
                    buttonCanvas = controller;
                    break;
                case CanvasController.CanvasType.DialogueCanvas:
                    dialogueCanvas = controller;
                    break;
                case CanvasController.CanvasType.FieldProductionCanvas:
                    fieldProductionCanvas = controller;
                    break;
                case CanvasController.CanvasType.RanchProductionCanvas:
                    ranchProductionCanvas = controller;
                    break;   
                case CanvasController.CanvasType.ForestProductionCanvas:
                    forestProductionCanvas = controller;
                    break; 
                case CanvasController.CanvasType.PopulationCanvas:
                    populationCanvas = controller;
                    break;  
                default:
                    Debug.LogWarning($"未知的Canvas类型: {controller.Type}，游戏对象: {controller.gameObject.name}");
                    break;
            }
        }
        
        // 检查是否所有Canvas都已找到
        LogCanvasInitializationStatus();
    }
    
    /// <summary>
    /// 记录Canvas初始化状态
    /// </summary>
    private void LogCanvasInitializationStatus()
    {
        Debug.Log("=== Canvas初始化状态 ===");
        Debug.Log($"SystemCanvas: {(systemCanvas != null ? "已找到" : "未找到")}");
        Debug.Log($"ButtonCanvas: {(buttonCanvas != null ? "已找到" : "未找到")}");
        Debug.Log($"DialogueCanvas: {(dialogueCanvas != null ? "已找到" : "未找到")}");
        Debug.Log($"FieldProductionCanvas: {(fieldProductionCanvas != null ? "已找到" : "未找到")}");
        Debug.Log($"ForestProductionCanvas: {(forestProductionCanvas != null ? "已找到" : "未找到")}");
        Debug.Log($"PopulationCanvas: {(populationCanvas != null ? "已找到" : "未找到")}");
        Debug.Log($"总计找到Canvas数量: {canvasDictionary.Count}");
        Debug.Log("========================");
    }
    
    // ========== 各个Canvas的显示/隐藏方法 ==========
    
    public void ShowSystemCanvas() => systemCanvas?.ShowCanvas();
    public void HideSystemCanvas() => systemCanvas?.HideCanvas();
    public void ToggleSystemCanvas() => systemCanvas?.ToggleCanvas();
    public bool IsSystemCanvasVisible() => systemCanvas?.IsCanvasVisible() ?? false;
    
    public void ShowButtonCanvas() => buttonCanvas?.ShowCanvas();
    public void HideButtonCanvas() => buttonCanvas?.HideCanvas();
    public void ToggleButtonCanvas() => buttonCanvas?.ToggleCanvas();
    public bool IsButtonCanvasVisible() => buttonCanvas?.IsCanvasVisible() ?? false;
    
    public void ShowDialogueCanvas() => dialogueCanvas?.ShowCanvas();
    public void HideDialogueCanvas() => dialogueCanvas?.HideCanvas();
    public void ToggleDialogueCanvas() => dialogueCanvas?.ToggleCanvas();
    public bool IsDialogueCanvasVisible() => dialogueCanvas?.IsCanvasVisible() ?? false;

    public void ShowFieldProductionCanvas() => fieldProductionCanvas?.ShowCanvas();
    public void HideFieldProductionCanvas() => fieldProductionCanvas?.HideCanvas();
    public void ToggleFieldProductionCanvas() => fieldProductionCanvas?.ToggleCanvas();
    public bool IsFieldProductionCanvasVisible() => fieldProductionCanvas?.IsCanvasVisible() ?? false;
    
    public void ShowRanchProductionCanvas() => ranchProductionCanvas?.ShowCanvas();
    public void HideRanchProductionCanvas() => ranchProductionCanvas?.HideCanvas();
    public void ToggleRanchProductionCanvas() => ranchProductionCanvas?.ToggleCanvas();
    public bool IsRanchProductionCanvasVisible() => ranchProductionCanvas?.IsCanvasVisible() ?? false;

    public void ShowForestProductionCanvas() => forestProductionCanvas?.ShowCanvas();
    public void HideForestProductionCanvas() => forestProductionCanvas?.HideCanvas();
    public void ToggleForestProductionCanvas() => forestProductionCanvas?.ToggleCanvas();
    public bool IsForestProductionCanvasVisible() => forestProductionCanvas?.IsCanvasVisible() ?? false;

    public void ShowPopulationCanvas() => populationCanvas?.ShowCanvas();
    public void HidePopulationCanvas() => populationCanvas?.HideCanvas();
    public void TogglePopulationCanvas() => populationCanvas?.ToggleCanvas();
    public bool IsPopulationCanvasVisible() => populationCanvas?.IsCanvasVisible() ?? false;
    
    /// <summary>
    /// 获取指定类型的Canvas控制器
    /// </summary>
    public CanvasController GetCanvas(CanvasController.CanvasType type)
    {
        if (canvasDictionary.TryGetValue(type, out CanvasController controller))
        {
            return controller;
        }
        return null;
    }
    
    /// <summary>
    /// 显示指定类型的Canvas
    /// </summary>
    public void ShowCanvas(CanvasController.CanvasType type)
    {
        GetCanvas(type)?.ShowCanvas();
    }
    
    /// <summary>
    /// 隐藏指定类型的Canvas
    /// </summary>
    public void HideCanvas(CanvasController.CanvasType type)
    {
        GetCanvas(type)?.HideCanvas();
    }
    
    /// <summary>
    /// 隐藏所有Canvas
    /// </summary>
    public void HideAllCanvases()
    {
        foreach (var controller in canvasDictionary.Values)
        {
            controller.HideCanvas();
        }
    }
    
    /// <summary>
    /// 显示多个Canvas（隐藏其他的）
    /// </summary>
    public void ShowOnlyCanvases(params CanvasController.CanvasType[] typesToShow)
    {
        HideAllCanvases();
        
        foreach (var type in typesToShow)
        {
            ShowCanvas(type);
        }
    }
    
    /// <summary>
    /// 检查是否有任何Canvas正在显示（除SystemCanvas外）
    /// </summary>
    public bool IsAnyCanvasVisible()
    {
        foreach (var kvp in canvasDictionary)
        {
            // SystemCanvas通常是常显的，可以排除
            if (kvp.Key != CanvasController.CanvasType.SystemCanvas && kvp.Value.IsCanvasVisible())
            {
                return true;
            }
        }
        return false;
    }
}