using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TokenDisplay : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Button unlockButton;
    [SerializeField] private TextMeshProUGUI tokenNameText;
    [SerializeField] private TextMeshProUGUI tokenDescriptionText;
    [SerializeField] private Image tokenImage;

    [Header("默认内容")]
    [SerializeField] private string lockedName = "???";
    [SerializeField] private string lockedDescription = "???";
    [SerializeField] private Sprite lockedSprite;

    [Header("颜色设置")]
    [SerializeField] private Color unlockedNameColor = Color.white;
    [SerializeField] private Color lockedNameColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    [SerializeField] private Color unlockedDescriptionColor = Color.white;
    [SerializeField] private Color lockedDescriptionColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

    private TokenScriptableObject currentToken;
    private ProgressTableManager currentManager;

    public void Initialize(TokenScriptableObject token, ProgressTableManager manager)
    {
        currentToken = token;
        currentManager = manager;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentToken == null)
        {
            Debug.LogWarning("TokenDisplay: 当前信物为空");
            SetLockedState();
            return;
        }

        bool isUnlocked = currentToken.isUnlocked;
        bool canUnlock = currentManager != null && currentManager.IsIslandComplete();

        // 更新解锁按钮
        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(!isUnlocked && canUnlock);
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }

        // 更新名称
        if (tokenNameText != null)
        {
            tokenNameText.text = isUnlocked ? currentToken.tokenName : lockedName;
            tokenNameText.color = isUnlocked ? unlockedNameColor : lockedNameColor;
        }

        // 更新描述
        if (tokenDescriptionText != null)
        {
            tokenDescriptionText.text = isUnlocked ? currentToken.tokenDescription : lockedDescription;
            tokenDescriptionText.color = isUnlocked ? unlockedDescriptionColor : lockedDescriptionColor;
        }

        // 更新图像
        if (tokenImage != null)
        {
            if (isUnlocked && currentToken.icon != null)
            {
                tokenImage.sprite = currentToken.icon;
                tokenImage.color = Color.white;
            }
            else
            {
                tokenImage.sprite = lockedSprite;
                tokenImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
        }
    }

    private void SetLockedState()
    {
        if (unlockButton != null)
            unlockButton.gameObject.SetActive(false);

        if (tokenNameText != null)
        {
            tokenNameText.text = lockedName;
            tokenNameText.color = lockedNameColor;
        }

        if (tokenDescriptionText != null)
        {
            tokenDescriptionText.text = lockedDescription;
            tokenDescriptionText.color = lockedDescriptionColor;
        }

        if (tokenImage != null && lockedSprite != null)
        {
            tokenImage.sprite = lockedSprite;
            tokenImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
    }

    private void OnUnlockButtonClicked()
    {
        if (currentManager == null || currentToken == null) return;

        bool success = currentManager.UnlockToken();
        
        if (success)
        {
            Debug.Log($"信物解锁成功: {currentToken.tokenName}");
            UpdateDisplay();
        }
        else
        {
            Debug.LogWarning($"信物解锁失败: {currentToken.tokenName}");
        }
    }

    public void RefreshDisplay()
    {
        UpdateDisplay();
    }

    public void SetToken(TokenScriptableObject token)
    {
        currentToken = token;
        UpdateDisplay();
    }

    public void SetManager(ProgressTableManager manager)
    {
        currentManager = manager;
        UpdateDisplay();
    }

    public TokenScriptableObject GetCurrentToken()
    {
        return currentToken;
    }

    public bool IsUnlocked()
    {
        return currentToken != null && currentToken.isUnlocked;
    }
}