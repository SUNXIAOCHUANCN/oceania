using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueCanvasController : CanvasController
{
    [Header("Dialogue UI Elements")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button option1Button;
    [SerializeField] private Button option2Button;

    private UnityAction onOption1Selected;
    private UnityAction onOption2Selected;

    /// <summary>
    /// 设置对话文本内容
    /// </summary>
    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }

    /// <summary>
    /// 清空对话文本
    /// </summary>
    public void ClearDialogueText()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }

    /// <summary>
    /// 显示选项按钮
    /// </summary>
    public void ShowOptionButtons(string option1Text, string option2Text)
    {
        if (option1Button != null && option2Button != null)
        {
            option1Button.gameObject.SetActive(true);
            option2Button.gameObject.SetActive(true);
            
            option1Button.interactable = true;
            option2Button.interactable = true;
            
            option1Button.GetComponentInChildren<TMP_Text>().text = option1Text;
            option2Button.GetComponentInChildren<TMP_Text>().text = option2Text;
        }
    }

    /// <summary>
    /// 隐藏选项按钮
    /// </summary>
    public void HideOptionButtons()
    {
        if (option1Button != null && option2Button != null)
        {
            option1Button.gameObject.SetActive(false);
            option2Button.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 设置选项按钮的点击事件
    /// </summary>
    public void SetOptionButtonListeners(UnityAction onOption1, UnityAction onOption2)
    {
        if (option1Button != null && option2Button != null)
        {
            // 移除现有的监听器
            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            
            // 添加新的监听器
            onOption1Selected = onOption1;
            onOption2Selected = onOption2;
            
            option1Button.onClick.AddListener(() => 
            {
                onOption1Selected?.Invoke();
                ClearOptionButtonListeners();
            });
            
            option2Button.onClick.AddListener(() => 
            {
                onOption2Selected?.Invoke();
                ClearOptionButtonListeners();
            });
        }
    }

    /// <summary>
    /// 清空选项按钮的监听器
    /// </summary>
    public void ClearOptionButtonListeners()
    {
        if (option1Button != null && option2Button != null)
        {
            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            onOption1Selected = null;
            onOption2Selected = null;
        }
    }

    /// <summary>
    /// 获取Canvas是否可见
    /// </summary>
    public bool IsDialogueCanvasVisible()
    {
        return IsCanvasVisible();
    }
}