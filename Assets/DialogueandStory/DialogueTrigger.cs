using UnityEngine;

using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UIElements.Experimental;


public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private List<dialogueString> dialogueStrings = new List<dialogueString>();
    [SerializeField] private Transform NPCTransform;
    private bool hasSpoken = false;
    [SerializeField] private Sprite image = null;

    [Header("角色头像映射配置")]
    [SerializeField] private CharacterSpriteMapping characterMapping;

    [Header("���������")]
    [SerializeField] private string plotPointID; // ������Ӧ�ľ����ID
    [SerializeField] private List<string> requiredPlotPoints = new List<string>(); // ǰ�þ�����б�

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Call OnTriggerEnter");
        if (!other.CompareTag("Player"))
        {
            Debug.Log("other's tag is not Player, but "+other.tag);
            return;
        }
        // ����Ƿ���������ǰ�þ����
        if (requiredPlotPoints.Count > 0)
        {
            if (!CheckRequiredPlotPoints())
            {
                Debug.Log($"Required plot points not met for dialogue: {plotPointID}");
                return;
            }
        }
        // ��ȡ��ִ�жԻ�
        DialogueManager dialogueManager = other.gameObject.GetComponent<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.DialogueStart(dialogueStrings, NPCTransform, image, characterMapping);

            // ����Լ���Ӧ�ľ����
            if (!string.IsNullOrEmpty(plotPointID))
            {
                MarkPlotPoint();
            }

            hasSpoken = true; // ��ֹ�ظ�����
        }
        else
        {
            Debug.LogWarning("DialogueManager component not found on player!");
        }
    }
    private bool CheckRequiredPlotPoints()
    {
        if (PlotProgressManager.Instance == null)
        {
            Debug.LogWarning("PlotProgressManager instance not found!");
            return requiredPlotPoints.Count == 0; // ���û�й���������û��Ҫ��������
        }

        foreach (string plotPoint in requiredPlotPoints)
        {
            if (!PlotProgressManager.Instance.IsPlotPointTriggered(plotPoint))
            {
                Debug.Log($"Required plot point '{plotPoint}' not triggered");
                return false;
            }
        }

        return true;
    }
    private void MarkPlotPoint()
    {
        if (PlotProgressManager.Instance != null && !string.IsNullOrEmpty(plotPointID))
        {
            PlotProgressManager.Instance.MarkPlotPointTriggered(plotPointID);
            Debug.Log($"Dialogue plot point '{plotPointID}' marked as triggered");
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

    [Header("多角色对话（可选）")]
    public string speakerName; // 当前说话者的名字，用于动态切换头像
}