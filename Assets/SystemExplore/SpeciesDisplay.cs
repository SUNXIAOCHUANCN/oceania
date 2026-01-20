using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeciesDisplay : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Image speciesImage;
    [SerializeField] private TextMeshProUGUI speciesNameText;
    [SerializeField] private TextMeshProUGUI speciesDescriptionText;
    [SerializeField] private TextMeshProUGUI speciesTypeText;

    [Header("默认内容")]
    [SerializeField] private Sprite defaultLockedSprite;
    [SerializeField] private string lockedNamePrefix = "???";
    [SerializeField] private string lockedDescription = "???";
    [SerializeField] private string typeFormat = "类型: {0}";

    private SpeciesScriptableObject currentSpecies;

    public void Initialize(SpeciesScriptableObject species, Sprite defaultSprite, string lockedPrefix, string lockedDesc)
    {
        if (species == null)
        {
            Debug.LogError("SpeciesDisplay: 传入的SpeciesScriptableObject为空");
            return;
        }

        currentSpecies = species;
        defaultLockedSprite = defaultSprite;
        lockedNamePrefix = lockedPrefix;
        lockedDescription = lockedDesc;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentSpecies == null) return;

        bool isUnlocked = currentSpecies.unlocked;

        // 更新图像
        if (speciesImage != null)
        {
            if (isUnlocked && currentSpecies.icon != null)
            {
                speciesImage.sprite = currentSpecies.icon;
                speciesImage.color = Color.white;
            }
            else
            {
                speciesImage.sprite = defaultLockedSprite;
                speciesImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 灰色半透明
            }
        }

        // 更新名称
        if (speciesNameText != null)
        {
            if (isUnlocked)
            {
                speciesNameText.text = currentSpecies.speciesName;
                speciesNameText.color = Color.white;
            }
            else
            {
                // 显示类型信息
                string typeName = GetTypeName(currentSpecies.speciesType);
                speciesNameText.text = $"{lockedNamePrefix} ({typeName})";
                speciesNameText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            }
        }

        // 更新描述
        if (speciesDescriptionText != null)
        {
            if (isUnlocked)
            {
                speciesDescriptionText.text = currentSpecies.speciesDescription;
                speciesDescriptionText.color = Color.white;
            }
            else
            {
                speciesDescriptionText.text = lockedDescription;
                speciesDescriptionText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            }
        }

        // 更新类型文本
        if (speciesTypeText != null)
        {
            string typeName = GetTypeName(currentSpecies.speciesType);
            speciesTypeText.text = string.Format(typeFormat, typeName);
            
            // 根据解锁状态设置颜色
            speciesTypeText.color = isUnlocked ? new Color(0.8f, 0.9f, 1f, 1f) : new Color(0.6f, 0.6f, 0.6f, 0.8f);
        }
    }

    private string GetTypeName(SpeciesType speciesType)
    {
        switch (speciesType)
        {
            case SpeciesType.Crop:
                return "作物";
            case SpeciesType.Ani:
                return "动物";
            case SpeciesType.Mat:
                return "材料";
            default:
                return speciesType.ToString();
        }
    }

    public void RefreshDisplay()
    {
        if (currentSpecies != null)
        {
            UpdateDisplay();
        }
    }

    public void SetSpecies(SpeciesScriptableObject species)
    {
        currentSpecies = species;
        UpdateDisplay();
    }

    public SpeciesScriptableObject GetCurrentSpecies()
    {
        return currentSpecies;
    }

    public bool IsUnlocked()
    {
        return currentSpecies != null && currentSpecies.unlocked;
    }
}