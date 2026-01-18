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

    public void DialogueStart(List<dialogueString> textToPrint, Transform NPC)
    {
        if (dialogueCanvasController == null)
        {
            Debug.LogError("DialogueCanvasController not assigned!");
            return;
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

        // 清空文本和按钮
        dialogueCanvasController.ClearDialogueText();
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