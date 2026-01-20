using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class TextShowManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private GameObject textShowPanel; // 文本显示面板
    [SerializeField] private TMP_Text displayText; // 显示文本的UI
    [SerializeField] private Button continueButton; // 继续按钮

    [Header("显示设置")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果速度
    [SerializeField] private bool useTypingEffect = true; // 是否使用打字机效果

    [Header("书本控制")]
    [SerializeField] private BookPro bookPro; // 书本引用
    
    private List<showTextString> textList;
    private int currentTextIndex = 0;
    private bool isShowingText = false;
    private bool waitingForContinue = false;
    //private int PageIndex;
    private string targetPageName; // 存储目标页面名称
    private Sprite newPageSprite; 

    [Header("玩家控制")]
    [SerializeField] private PlayerController playerController; // 玩家控制器
    [SerializeField] private Animator playerAnimator; // 玩家的动画控制器
    private bool wasPlayerEnabled = true;

    void Start()
    {
        // 初始化时隐藏面板
        if (textShowPanel != null)
            textShowPanel.SetActive(false);

        // 设置继续按钮
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    public void SetPageName(string pageName)
    {
        targetPageName = pageName;
    }
    public void SetNewPage(Sprite page)
    {
        newPageSprite = page;
    }

    public void ShowText(List<showTextString> textToShow)
    {
        //Debug.Log("In ShowText");
        if (isShowingText) return;

        textList = textToShow;
        currentTextIndex = 0;

        // 显示面板
        if (textShowPanel != null)
            textShowPanel.SetActive(true);

        // 禁用玩家控制
        if (playerController != null)
        {
            wasPlayerEnabled = playerController.enabled;
            playerController.enabled = false;
        }
        if (playerAnimator != null)
        {
           // playerAnimator.Play("Idle");
            playerAnimator.enabled = false;
        }

        // 显示鼠标光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isShowingText = true;
        //Debug.Log("Call StartCoroutine");
        StartCoroutine(DisplayTextSequence());
    }

    private IEnumerator DisplayTextSequence()
    {
        //Debug.Log("In  DisplayTextSequence");
        while (currentTextIndex < textList.Count)
        {
            showTextString currentText = textList[currentTextIndex];

            // 触发开始事件
            currentText.startTextEvent?.Invoke();

            // 显示文本
            if (useTypingEffect)
            {
                yield return StartCoroutine(TypeText(currentText.text));
            }
            else
            {
                displayText.text = currentText.text;
                yield return null; // 等待一帧确保文本已更新
            }

            // 显示继续按钮并等待点击
            if (continueButton != null)
            {
                waitingForContinue = true;
                continueButton.gameObject.SetActive(true);

                // 如果是最后一段文本，按钮文本改为"结束"
                if (currentText.isEnd)
                {
                    TMP_Text buttonText = continueButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                        buttonText.text = "点击结束";
                }
                else
                {
                    TMP_Text buttonText = continueButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                        buttonText.text = "点击继续";
                }

                // 等待玩家点击继续按钮
                yield return new WaitUntil(() => !waitingForContinue);
                continueButton.gameObject.SetActive(false);
            }

            // 触发结束事件
            currentText.endTextEvent?.Invoke();

            if ( bookPro != null)
            {
                 ChangeBookPageImage(targetPageName, newPageSprite);
            }

            currentTextIndex++;

            // 如果是最后一段文本，结束显示
            if (currentText.isEnd)
            {
                break;
            }
        }

        HideText();
    }

    private void ChangeBookPageImage(string pageName, Sprite newSprite)
    {
        if (bookPro == null || bookPro.papers == null)
        {
            Debug.LogError($"BookPro或papers为空");
            return;
        }

        if (newSprite == null)
        {
            Debug.LogError($"图片为空");
            return;
        }

        bool found = false;

        // 方法1：在papers数组中查找对应名称的页面
        foreach (Paper paper in bookPro.papers)
        {
            if (paper.Front != null && paper.Front.name == pageName)
            {
                Image image = paper.Front.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = newSprite;
                    Debug.Log($"已修改页面 {pageName} 的图片");
                    found = true;
                    break;
                }
            }
        }

        // 方法2：如果方法1没找到，直接通过GameObject名称查找
        if (!found)
        {
            Transform pageTransform = bookPro.transform.Find(pageName);
            if (pageTransform != null)
            {
                Image image = pageTransform.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = newSprite;
                    Debug.Log($"通过GameObject名称修改页面 {pageName} 的图片");
                    found = true;
                }
            }
        }

        if (!found)
        {
            Debug.LogError($"未找到页面: {pageName}");

            // 打印所有可用的页面名称
            Debug.Log("可用的页面名称：");
            for (int i = 0; i < bookPro.papers.Length; i++)
            {
                if (bookPro.papers[i].Front != null)
                    Debug.Log($"- {bookPro.papers[i].Front.name}");
            }
        }

        // 强制刷新书本显示
        if (found)
        {
            bookPro.UpdatePages();
        }
    }

    private IEnumerator TypeText(string text)
    {
        displayText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            displayText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnContinueClicked()
    {
        // 点击继续按钮，继续显示下一段文本
        waitingForContinue = false;
    }

    private void HideText()
    {
        StopAllCoroutines();

        // 隐藏UI
        if (displayText != null)
            displayText.text = "";

        if (textShowPanel != null)
            textShowPanel.SetActive(false);

        // 恢复玩家控制
        if (playerController != null && wasPlayerEnabled)
        {
            playerController.enabled = true;
        }
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
        }

        // 隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 隐藏继续按钮
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        waitingForContinue = false;
        isShowingText = false;
    }

    void Update()
    {
    }
}