using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Canvas Controller")]
    [SerializeField] private DialogueCanvasController dialogueCanvasController;
    
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float turnSpeed = 2f;

    private List<dialogueString> dialogueList;
    private CharacterSpriteMapping characterMapping;
    private Transform currentNPCTransform;  // 当前对话的NPC Transform
    private NPCAnimationController currentNPCAnimator;  // 当前NPC的动画控制器

    [Header("Player")]
    [SerializeField] private PlayerController firstPlayerController;
    private Transform playerCamera;

    private int currentDialogueIndex = 0;
    private bool optionSelected = false;

    void Start()
    {
        playerCamera = Camera.main.transform;
        // 确保对话开始时Canvas是隐藏的
        if (dialogueCanvasController != null)
        {
            dialogueCanvasController.HideCanvas();
        }
    }

    public void DialogueStart(List<dialogueString> textToPrint, Transform NPC, Sprite dialogueImage, CharacterSpriteMapping characterMapping = null)
    {
        if (dialogueCanvasController == null)
        {
            Debug.LogError("DialogueCanvasController not assigned!");
            return;
        }
        if (dialogueImage != null)
        {
            dialogueCanvasController.SetDialogueImage(dialogueImage);
        }
        else
        {
            // 如果没有指定图片，查找默认的"example"图片
            dialogueCanvasController.SetDefaultDialogueImage();
        }

        // 保存角色头像映射配置和NPC引用
        this.characterMapping = characterMapping;
        this.currentNPCTransform = NPC;

        // 尝试获取NPC的动画控制器
        Debug.Log($"[DialogueManager] 尝试获取NPC的动画控制器，NPC Transform: {NPC?.name}");
        currentNPCAnimator = NPC?.GetComponent<NPCAnimationController>();

        // 如果没找到，尝试在父对象中查找（处理Animator在子对象上的情况）
        if (currentNPCAnimator == null && NPC != null)
        {
            currentNPCAnimator = NPC.GetComponentInParent<NPCAnimationController>();
            if (currentNPCAnimator != null)
            {
                Debug.Log($"在父对象中找到NPCAnimationController: {currentNPCAnimator.gameObject.name}");
            }
        }

        // 调试信息
        if (currentNPCAnimator != null)
        {
            Debug.Log($"成功找到NPCAnimationController，GameObject: {currentNPCAnimator.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"未找到NPCAnimationController！NPC Transform: {NPC?.name}");
        }

        // 显示对话Canvas
        dialogueCanvasController.ShowCanvas();
        firstPlayerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(TurnCameraTowardsNPC(NPC));

        dialogueList = textToPrint;
        currentDialogueIndex = 0;
        optionSelected = false;

        // 清空文本、名称和按钮
        dialogueCanvasController.ClearDialogueText();
        dialogueCanvasController.ClearSpeakerName();  // 清空角色名称
        dialogueCanvasController.HideOptionButtons();
        dialogueCanvasController.ClearOptionButtonListeners();

        StartCoroutine(PrintDialogue());
    }

    private IEnumerator TurnCameraTowardsNPC(Transform NPC)
    {
        Quaternion startRotation = playerCamera.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(NPC.position - playerCamera.position);

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            playerCamera.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * turnSpeed;
            yield return null;
        }
        playerCamera.rotation = targetRotation;
    }

    private IEnumerator PrintDialogue()
    {
        while (currentDialogueIndex < dialogueList.Count)
        {
            dialogueString line = dialogueList[currentDialogueIndex];
            line.startDialogueEvent?.Invoke();

            // 播放NPC手势动画（如果NPC有动画控制器）
            if (currentNPCAnimator != null)
            {
                Debug.Log($"[DialogueManager] 调用PlayRandomGesture，NPC: {currentNPCTransform?.name}");
                currentNPCAnimator.PlayRandomGesture();
            }
            else
            {
                Debug.Log($"[DialogueManager] NPC没有动画控制器，跳过手势播放");
            }

            // 如果当前行指定了说话者，切换到对应的头像和显示名称
            if (!string.IsNullOrEmpty(line.speakerName))
            {
                // 显示角色名称
                dialogueCanvasController.SetSpeakerName(line.speakerName);

                // 切换头像（如果有配置角色映射）
                if (characterMapping != null)
                {
                    Sprite speakerSprite = characterMapping.GetSprite(line.speakerName);
                    if (speakerSprite != null)
                    {
                        dialogueCanvasController.SetDialogueImage(speakerSprite);
                    }
                }
            }

            if (line.isQuestion)
            {
                yield return StartCoroutine(TypeText(line.text));
                
                // 通过CanvasController显示选项按钮
                dialogueCanvasController.ShowOptionButtons(line.answerOption1, line.answerOption2);
                
                // 设置按钮监听器
                dialogueCanvasController.SetOptionButtonListeners(
                    () => HandleOptionSelected(line.option1IndexJump),
                    () => HandleOptionSelected(line.option2IndexJump)
                );

                yield return new WaitUntil(() => optionSelected);
            }
            else
            {
                yield return StartCoroutine(TypeText(line.text));
            }
            
            line.endDialogueEvent?.Invoke();
            optionSelected = false;
        }
        
        DialogueStop();
    }

    private void HandleOptionSelected(int indexJump)
    {
        optionSelected = true;
        dialogueCanvasController.HideOptionButtons();
        dialogueCanvasController.ClearOptionButtonListeners();
        currentDialogueIndex = indexJump;
    }

    private IEnumerator TypeText(string text)
    {
        dialogueCanvasController.ClearDialogueText();
        string currentText = "";
        
        foreach (char letter in text.ToCharArray())
        {
            currentText += letter;
            dialogueCanvasController.SetDialogueText(currentText);
            yield return new WaitForSeconds(typingSpeed);
        }
        
        if (!dialogueList[currentDialogueIndex].isQuestion)
        {
            yield return new WaitUntil(() => Mouse.current?.leftButton.wasPressedThisFrame ?? false);
        }
        
        currentDialogueIndex++;
        
        if (dialogueList[currentDialogueIndex - 1].isEnd)
        {
            DialogueStop();
            yield break;
        }
    }

    private void DialogueStop()
    {
        StopAllCoroutines();
        dialogueCanvasController.ClearDialogueText();
        dialogueCanvasController.ClearSpeakerName();  // 清空角色名称
        dialogueCanvasController.HideOptionButtons();
        dialogueCanvasController.ClearOptionButtonListeners();
        dialogueCanvasController.HideCanvas();

        firstPlayerController.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 可以添加一些更新逻辑，但保持简单
    }
}