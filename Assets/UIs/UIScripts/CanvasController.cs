using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public enum CanvasType
    {
        SystemCanvas,
        ButtonCanvas,
        DialogueCanvas,
        FieldProductionCanvas,
        RanchProductionCanvas,
        ForestProductionCanvas,
        PopulationCanvas,
        ExploreCanvas,
        VoyageCanvas,
        InteractCanvas
    }
    
    [SerializeField] private CanvasType canvasType;
    [SerializeField] private bool startHidden = true;
    
    public CanvasType Type => canvasType;
    
    protected virtual void Start()
    {
        // 根据设置初始化显示状态
        if (startHidden)
        {
            HideCanvas();
        }
        else
        {
            ShowCanvas();
        }
    }
    
    /// <summary>
    /// 显示这个Canvas
    /// </summary>
    public virtual void ShowCanvas()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏这个Canvas
    /// </summary>
    public virtual void HideCanvas()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 切换Canvas的显示状态
    /// </summary>
    public void ToggleCanvas()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
    /// <summary>
    /// 检查Canvas当前是否显示
    /// </summary>
    public bool IsCanvasVisible()
    {
        return gameObject.activeSelf;
    }
}