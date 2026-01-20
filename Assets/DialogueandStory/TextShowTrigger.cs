using UnityEngine;
using System.Collections.Generic;

public class TextShowTrigger : MonoBehaviour
{
    [SerializeField] private List<showTextString> textStrings = new List<showTextString>();
    //[SerializeField] public int pageIndex;         // 要修改的页面索引
    [SerializeField] public string pageName; // 要修改的页面名称，如"Page1", "Page2", "Page3"
    [SerializeField] public Sprite image;    // 页面图片资源
    [SerializeField] private string plotPointID = "PlotPoint1";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            // 查找TextShowManager并调用显示文本的方法
            TextShowManager textManager = other.GetComponent<TextShowManager>();
            if (textManager != null)
            {
                //Debug.Log("Find TextShowManager");
                //textManager.SetPageIndex(pageIndex);
                textManager.SetPageName(pageName); // 改为传递页面名称
                textManager.SetNewPage(image);
                //Debug.Log("Call textManager.ShowText");
                textManager.ShowText(textStrings);
                // 通知 PlotProgressManager 该 PlotPoint 已触发
                if (PlotProgressManager.Instance != null)
                {
                    PlotProgressManager.Instance.MarkPlotPointTriggered(plotPointID);
                }
                hasTriggered = true; // 避免重复触发
            }
            else
            {
                Debug.LogWarning("TextShowManager component not found on player!");
            }
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