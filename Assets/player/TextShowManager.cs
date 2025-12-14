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

    private List<showTextString> textList;
    private int currentTextIndex = 0;
    private bool isShowingText = false;
    private bool waitingForContinue = false;

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

    public void ShowText(List<showTextString> textToShow)
    {
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
            playerAnimator.Play("Idle");

            playerAnimator.enabled = false;
        }

        // 显示鼠标光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isShowingText = true;
        StartCoroutine(DisplayTextSequence());
    }

    private IEnumerator DisplayTextSequence()
    {
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

            currentTextIndex++;

            // 如果是最后一段文本，结束显示
            if (currentText.isEnd)
            {
                break;
            }
        }

        HideText();
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