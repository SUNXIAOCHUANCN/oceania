using UnityEngine;

using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine.UIElements.Experimental;


public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private List<dialogueString> dialogueStrings = new List<dialogueString>();
    [SerializeField] private Transform NPCTransform;
    private bool hasSpoken = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Call OnTriggerEnter");
        if (!other.CompareTag("Player"))
        {
            Debug.Log("other's tag is not Player, but "+other.tag);
        }
        if (other.CompareTag("Player") ){
            Debug.Log("Try to call DialogueStart");
            other.gameObject.GetComponent<DialogueManager>().DialogueStart(dialogueStrings, NPCTransform);
        }
    }
}

[System.Serializable]
public class dialogueString
{
    public string text; // Represent the text that the npo says.
    public bool isEnd; // Represent if the line is the final Line for the conversation
    [Header("Branch")]
    public bool isQuestion;
    public string answerOption1;
    public string answerOption2;
    public int option1IndexJump;
    public int option2IndexJump;
    [Header("Triggered Events")]
    public UnityEvent startDialogueEvent;
    public UnityEvent endDialogueEvent;

}
