using UnityEngine;
using TMPro;

public class SecretDisplay : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI secretTitleText;
    [SerializeField] private TextMeshProUGUI secretDescriptionText;

    [Header("默认内容")]
    [SerializeField] private string lockedTitle = "秘密";
    [SerializeField] private string lockedDescription = "???";

    [Header("颜色设置")]
    [SerializeField] private Color unlockedTitleColor = Color.white;
    [SerializeField] private Color lockedTitleColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    [SerializeField] private Color unlockedDescriptionColor = Color.white;
    [SerializeField] private Color lockedDescriptionColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

    private SecretScriptableObject currentSecret;

    public void Initialize(SecretScriptableObject secret)
    {
        currentSecret = secret;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentSecret == null)
        {
            Debug.LogWarning("SecretDisplay: 当前秘密为空");
            SetLockedState();
            return;
        }

        bool isUnlocked = currentSecret.isUnlocked;

        // 更新标题
        if (secretTitleText != null)
        {
            secretTitleText.text = isUnlocked ? currentSecret.secretName : lockedTitle;
            secretTitleText.color = isUnlocked ? unlockedTitleColor : lockedTitleColor;
        }

        // 更新描述
        if (secretDescriptionText != null)
        {
            secretDescriptionText.text = isUnlocked ? currentSecret.secretContent : lockedDescription;
            secretDescriptionText.color = isUnlocked ? unlockedDescriptionColor : lockedDescriptionColor;
        }
    }

    private void SetLockedState()
    {
        if (secretTitleText != null)
        {
            secretTitleText.text = lockedTitle;
            secretTitleText.color = lockedTitleColor;
        }

        if (secretDescriptionText != null)
        {
            secretDescriptionText.text = lockedDescription;
            secretDescriptionText.color = lockedDescriptionColor;
        }
    }

    public void RefreshDisplay()
    {
        UpdateDisplay();
    }

    public void SetSecret(SecretScriptableObject secret)
    {
        currentSecret = secret;
        UpdateDisplay();
    }

    public SecretScriptableObject GetCurrentSecret()
    {
        return currentSecret;
    }

    public bool IsUnlocked()
    {
        return currentSecret != null && currentSecret.isUnlocked;
    }
}