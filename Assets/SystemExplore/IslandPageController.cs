using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class IslandPageController : MonoBehaviour
{
    [Header("物种展示区域")]
    [SerializeField] private Transform speciesDisplayContainer;
    [SerializeField] private GameObject speciesDisplayPrefab;

    [Header("秘密展示区域")]
    [SerializeField] private GameObject secretArea;
    [SerializeField] private TextMeshProUGUI secretTitleText;
    [SerializeField] private TextMeshProUGUI secretDescriptionText;

    [Header("信物展示区域")]
    [SerializeField] private GameObject tokenArea;
    [SerializeField] private Button tokenUnlockButton;
    [SerializeField] private TextMeshProUGUI tokenNameText;
    [SerializeField] private TextMeshProUGUI tokenDescriptionText;
    [SerializeField] private Image tokenImage;

    [Header("默认显示内容")]
    [SerializeField] private Sprite defaultSpeciesSprite;
    [SerializeField] private string lockedSpeciesNamePrefix = "???";
    [SerializeField] private string lockedSpeciesDescription = "???";
    [SerializeField] private string lockedSecretTitle = "秘密";
    [SerializeField] private string lockedSecretDescription = "???";
    [SerializeField] private string lockedTokenName = "???";
    [SerializeField] private string lockedTokenDescription = "???";
    [SerializeField] private Sprite lockedTokenSprite;

    private ProgressTableManager currentManager;
    private List<SpeciesDisplay> speciesDisplays = new List<SpeciesDisplay>();

    public void UpdatePageContent(ProgressTableManager manager)
    {
        if (manager == null)
        {
            Debug.LogError("IslandPageController: 传入的ProgressTableManager为空");
            return;
        }

        currentManager = manager;

        // 更新物种展示
        UpdateSpeciesDisplays();

        // 更新秘密展示
        UpdateSecretDisplay();

        // 更新信物展示
        UpdateTokenDisplay();
    }

    private void UpdateSpeciesDisplays()
    {
        if (currentManager == null) return;

        // 清除现有展示
        ClearSpeciesDisplays();

        // 获取绑定的物种列表
        List<SpeciesScriptableObject> boundSpecies = currentManager.GetBoundSpecies();
        if (boundSpecies == null || boundSpecies.Count == 0)
        {
            Debug.LogWarning("IslandPageController: 没有绑定的物种");
            return;
        }

        // 创建物种展示项
        foreach (SpeciesScriptableObject species in boundSpecies)
        {
            CreateSpeciesDisplay(species);
        }
    }

    private void ClearSpeciesDisplays()
    {
        foreach (SpeciesDisplay display in speciesDisplays)
        {
            if (display != null && display.gameObject != null)
                Destroy(display.gameObject);
        }
        speciesDisplays.Clear();
    }

    private void CreateSpeciesDisplay(SpeciesScriptableObject species)
    {
        if (speciesDisplayPrefab == null || speciesDisplayContainer == null)
        {
            Debug.LogError("IslandPageController: 物种展示预制体或容器未设置");
            return;
        }

        // 实例化展示项
        GameObject displayObj = Instantiate(speciesDisplayPrefab, speciesDisplayContainer);
        SpeciesDisplay display = displayObj.GetComponent<SpeciesDisplay>();
        
        if (display == null)
        {
            Debug.LogError("IslandPageController: 物种展示预制体上缺少SpeciesDisplay组件");
            Destroy(displayObj);
            return;
        }

        // 配置展示项
        display.Initialize(species, defaultSpeciesSprite, lockedSpeciesNamePrefix, lockedSpeciesDescription);
        speciesDisplays.Add(display);
    }

    private void UpdateSecretDisplay()
    {
        if (secretArea == null) return;

        SecretScriptableObject secret = currentManager?.GetBoundSecret();
        
        if (secret == null)
        {
            // 没有秘密，隐藏区域
            secretArea.SetActive(false);
            return;
        }

        secretArea.SetActive(true);

        if (secret.isUnlocked)
        {
            // 秘密已解锁
            if (secretTitleText != null)
                secretTitleText.text = secret.secretName;
            
            if (secretDescriptionText != null)
                secretDescriptionText.text = secret.secretContent;
        }
        else
        {
            // 秘密未解锁
            if (secretTitleText != null)
                secretTitleText.text = lockedSecretTitle;
            
            if (secretDescriptionText != null)
                secretDescriptionText.text = lockedSecretDescription;
        }
    }

    private void UpdateTokenDisplay()
    {
        if (tokenArea == null) return;

        TokenScriptableObject token = currentManager?.GetBoundToken();
        
        if (token == null)
        {
            // 没有信物，隐藏区域
            tokenArea.SetActive(false);
            return;
        }

        tokenArea.SetActive(true);

        // 检查是否可以解锁信物
        bool canUnlockToken = currentManager.IsIslandComplete();
        
        // 配置解锁按钮
        if (tokenUnlockButton != null)
        {
            tokenUnlockButton.gameObject.SetActive(!token.isUnlocked && canUnlockToken);
            tokenUnlockButton.onClick.RemoveAllListeners();
            tokenUnlockButton.onClick.AddListener(OnTokenUnlockButtonClicked);
        }

        if (token.isUnlocked)
        {
            // 信物已解锁
            if (tokenNameText != null)
                tokenNameText.text = token.tokenName;
            
            if (tokenDescriptionText != null)
                tokenDescriptionText.text = token.tokenDescription;
            
            if (tokenImage != null && token.icon != null)
                tokenImage.sprite = token.icon;
        }
        else
        {
            // 信物未解锁
            if (tokenNameText != null)
                tokenNameText.text = lockedTokenName;
            
            if (tokenDescriptionText != null)
                tokenDescriptionText.text = lockedTokenDescription;
            
            if (tokenImage != null)
                tokenImage.sprite = lockedTokenSprite;
        }
    }

    private void OnTokenUnlockButtonClicked()
    {
        if (currentManager == null) return;

        TokenScriptableObject token = currentManager.GetBoundToken();
        if (token == null) return;

        // 尝试解锁信物
        bool success = currentManager.UnlockToken();
        
        if (success)
        {
            Debug.Log($"信物解锁成功: {token.tokenName}");
            // 更新显示
            UpdateTokenDisplay();
        }
        else
        {
            Debug.LogWarning($"信物解锁失败: {token.tokenName}");
        }
    }

    public void RefreshDisplay()
    {
        if (currentManager != null)
        {
            UpdatePageContent(currentManager);
        }
    }
}