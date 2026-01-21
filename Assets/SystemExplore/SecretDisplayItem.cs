using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecretDisplayItem : MonoBehaviour
{
    [Header("UI元素")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;

    public void UpdateDisplay(SecretScriptableObject secret, bool isUnlocked)
    {
        if (isUnlocked)
        {
            // 已解锁：显示实际内容
            if (nameText != null)
            {
                nameText.text = secret.secretName;
            }

            if (contentText != null)
            {
                contentText.text = secret.secretContent;
            }
        }
        else
        {
            // 未解锁：显示问号
            if (nameText != null)
            {
                nameText.text = "???";
            }

            if (contentText != null)
            {
                contentText.text = "???";
            }
        }
    }
}