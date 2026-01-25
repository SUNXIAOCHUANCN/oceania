using UnityEngine;
using System.Collections.Generic;

public class TextShowTrigger : MonoBehaviour
{
    [SerializeField] private List<showTextString> textStrings = new List<showTextString>();
    //[SerializeField] public int pageIndex;         // 要修改的页面索引
    [SerializeField] public string pageName; // 要修改的页面名称，如"Page1", "Page2", "Page3"
    [SerializeField] public Sprite pageImage;    // 页面图片资源
    [SerializeField] private Sprite image = null;
    [Header("剧情点设置")]
    [SerializeField] private string plotPointID; // 自身对应的剧情点ID
    [SerializeField] private List<string> requiredPlotPoints = new List<string>(); // 前置剧情点列表

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            // 检查是否满足所有前置剧情点
            if (requiredPlotPoints.Count > 0)
            {
                if (!CheckRequiredPlotPoints())
                {
                    Debug.Log($"Required plot points not met for text show: {plotPointID}");
                    return;
                }
            }
            // 查找TextShowManager并调用显示文本的方法
            TextShowManager textManager = other.GetComponent<TextShowManager>();
            if (textManager != null)
            {
                if (pageName != null && pageImage != null) {
                    textManager.SetPageName(pageName); // 改为传递页面名称
                    textManager.SetNewPage(pageImage); 
                }

                if (image != null)
                {
                    textManager.SetImage(image);
                }
                else
                {
                    textManager.SetDefaultImage();
                }
               
                textManager.ShowText(textStrings);
                // 标记自身剧情点
                if (!string.IsNullOrEmpty(plotPointID))
                {
                    MarkPlotPoint();
                }
                hasTriggered = true; // 避免重复触发
            }
            else
            {
                Debug.LogWarning("TextShowManager component not found on player!");
            }
        }
    }
    /// <summary>
    /// 检查所有前置剧情点是否已触发
    /// </summary>
    private bool CheckRequiredPlotPoints()
    {
        if (PlotProgressManager.Instance == null)
        {
            Debug.LogWarning("PlotProgressManager instance not found!");
            return requiredPlotPoints.Count == 0; // 如果没有管理器，且没有要求，则允许
        }

        foreach (string plotPoint in requiredPlotPoints)
        {
            if (!PlotProgressManager.Instance.IsPlotPointTriggered(plotPoint))
            {
                Debug.Log($"Required plot point '{plotPoint}' not triggered");
                return false;
            }
        }

        return true;
    }
    /// <summary>
    /// 标记自身剧情点为已触发
    /// </summary>
    private void MarkPlotPoint()
    {
        if (PlotProgressManager.Instance != null && !string.IsNullOrEmpty(plotPointID))
        {
            PlotProgressManager.Instance.MarkPlotPointTriggered(plotPointID);
            Debug.Log($"Text show plot point '{plotPointID}' marked as triggered");
        }
    }
}



[System.Serializable]
public class showTextString
{
    public string text; // 要显示的文本
    public bool isEnd; // 是否是最后一行文本
    public float showTime = 3f; // 显示时间（秒），0表示手动点击继续
    
    [Header("触发事件")]
    public UnityEngine.Events.UnityEvent startTextEvent; // 文本开始显示时触发
    public UnityEngine.Events.UnityEvent endTextEvent; // 文本显示结束时触发

}