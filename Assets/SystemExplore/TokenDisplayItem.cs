using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TokenDisplayItem : MonoBehaviour
{
    [Header("UI元素")]
    public Image image;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    public void UpdateDisplay(TokenScriptableObject token, bool isUnlocked)
    {
        if (isUnlocked)
        {
            // 已解锁：显示实际内容
            if (image != null && token.icon != null)
            {
                image.sprite = token.icon;
                image.enabled = true;
            }
            else if (image != null)
            {
                image.enabled = false; // 如果没有图片则隐藏图片组件
            }

            if (nameText != null)
            {
                nameText.text = token.tokenName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = token.tokenDescription;
            }
        }
        else
        {
            // 未解锁：显示问号
            if (image != null)
            {
                image.sprite = null;
                image.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = "???";
            }

            if (descriptionText != null)
            {
                descriptionText.text = "???";
            }
        }
    }
}