using UnityEngine;
using System.Collections.Generic;

public class TextShowTrigger : MonoBehaviour
{
    [SerializeField] private List<showTextString> textStrings = new List<showTextString>();
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 查找TextShowManager并调用显示文本的方法
            TextShowManager textManager = other.GetComponent<TextShowManager>();
            if (textManager != null)
            {
                textManager.ShowText(textStrings);
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