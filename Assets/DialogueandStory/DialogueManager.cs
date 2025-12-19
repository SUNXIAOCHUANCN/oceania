using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueParent;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button option1Button;
    [SerializeField] private Button option2Button;
 

    [SerializeField] private float typingSpeed=0.05f;
    [SerializeField] private float turnSpeed = 2f;

    private List<dialogueString> dialogueList;

    [Header("Player")]
    [SerializeField]private PlayerController firstPlayerController;
    private Transform playerCamera;

    private int currentDialogueIndex = 0;
    private bool optionSelected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueParent.SetActive(false);
        playerCamera = Camera.main.transform;
        //firstPlayerController= GetComponent<PlayerController>();
    }

    public void DialogueStart(List<dialogueString> textToPrint, Transform NPC)
    {
        //playerRigidbody.useGravity = false;
        //Debug.Log("Call DialogueStrat");
        dialogueParent.SetActive(true);
        firstPlayerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(TurnCameraTowardsNPC(NPC));

        dialogueList = textToPrint;
        currentDialogueIndex = 0;

        DisableButtons();

        StartCoroutine(PrintDialogue());

    }

    private IEnumerator TurnCameraTowardsNPC(Transform NPC)
    {
        Quaternion startRotation = playerCamera.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(NPC.position-playerCamera.position);

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            playerCamera.rotation=Quaternion.Slerp(startRotation,targetRotation,  elapsedTime);
            elapsedTime += Time.deltaTime*turnSpeed;
            yield return null;
        }
        playerCamera.rotation= targetRotation;
    }

    private void DisableButtons()
    {
        /*
        option1Button.interactable = false;
        option2Button.interactable = false;

        option1Button.GetComponentInChildren<TMP_Text>().text = "Click to end dialogue";
        option2Button.GetComponentInChildren<TMP_Text>().text = "Click to end dialogue";
        */
        option1Button.gameObject.SetActive(false);
        option2Button.gameObject.SetActive(false);
        
    }

    private IEnumerator PrintDialogue()
    {
        while (currentDialogueIndex <dialogueList.Count)
        {
            dialogueString line = dialogueList[currentDialogueIndex];
            line.startDialogueEvent?.Invoke();

            if (line.isQuestion)
            {
                yield return StartCoroutine(TypeText(line.text));
                option1Button.gameObject.SetActive(true);
                option2Button.gameObject.SetActive(true);
                
                option1Button.interactable = true;
                option2Button.interactable = true;

                option1Button.GetComponentInChildren<TMP_Text>().text = line.answerOption1;
                option2Button.GetComponentInChildren<TMP_Text>().text = line.answerOption2;

                option1Button.onClick.AddListener(()=>HandleOptionSelected(line.option1IndexJump));
                option2Button.onClick.AddListener(() => HandleOptionSelected(line.option2IndexJump));

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
        optionSelected = true ;
        DisableButtons();

        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();

        currentDialogueIndex = indexJump;
    }
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        if (!dialogueList[currentDialogueIndex].isQuestion)
        {
            //yield return new WaitUntil(()=>Input.GetMouseButtonDown(0));
            yield return new WaitUntil(() => Mouse.current?.leftButton.wasPressedThisFrame ?? false);
        }
        currentDialogueIndex++;
        if (dialogueList[currentDialogueIndex-1].isEnd)
        {
            DialogueStop();
            yield break;
        }
        
    }

    private void DialogueStop()
    {
        StopAllCoroutines();
        dialogueText.text = "";
        dialogueParent.SetActive(false);

        firstPlayerController.enabled =true;
        //playerRigidbody.useGravity = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
