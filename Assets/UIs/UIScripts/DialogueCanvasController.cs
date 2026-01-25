using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueCanvasController : CanvasController
{
    [Header("Dialogue UI Elements")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText;  // 角色名称文本
    [SerializeField] private Image image;
    [SerializeField] private Button option1Button;
    [SerializeField] private Button option2Button;

    [Header("Default Image Settings")]
    [SerializeField] private Sprite defaultImage; // 可选的默认图片
    [SerializeField] private string defaultImageName = "example"; // 默认查找的图片名字

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
    /// 设置说话者的名字
    /// </summary>
    public void SetSpeakerName(string speakerName)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
        }
    }

    /// <summary>
    /// 清空说话者的名字
    /// </summary>
    public void ClearSpeakerName()
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = "";
        }
    }

    /// <summary>
    /// 设置对话图片
    /// </summary>
    public void SetDialogueImage(Sprite image)
    {
        if (this.image == null)
        {
            Debug.LogError("DialogueImage is not assigned in DialogueCanvasController!");
            return;
        }
        if (image != null)
        {
            this.image.sprite = image;
            this.image.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 设置默认对话图片
    /// </summary>
    public void SetDefaultDialogueImage()
    {
        if (image != null)
        {
            // 如果有预设的默认图片，使用它
            if (defaultImage != null)
            {
                image.sprite = defaultImage;
                image.gameObject.SetActive(true);
                return;
            }

            // 否则在Resources文件夹中查找名为"example"的图片
            Sprite foundImage = Resources.Load<Sprite>(defaultImageName);
            if (foundImage != null)
            {
                image.sprite = foundImage;
                image.gameObject.SetActive(true);
            }
            else
            {
                // 如果找不到任何图片，隐藏图片组件
                image.gameObject.SetActive(false);
                Debug.LogWarning($"Default image '{defaultImageName}' not found in Resources folder.");
            }
        }
    }

    /// <summary>
    /// 清空对话图片
    /// </summary>
    public void ClearDialogueImage()
    {
        if (image != null)
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
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