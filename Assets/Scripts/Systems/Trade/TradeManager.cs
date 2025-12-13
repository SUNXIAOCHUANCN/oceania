using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VariantPriceEntry
{
    public ResourceCategory category;
    public float pcPrice;
    public float plPrice;
    public float pmPrice;
}
public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance { get; private set; }

    [Header("变种价格表（可在 Inspector 调整）")]
    public List<VariantPriceEntry> variantPrices = new List<VariantPriceEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        // 如果未配置，则填入文档建议的默认值
        if (variantPrices == null || variantPrices.Count == 0)
        {
            variantPrices = new List<VariantPriceEntry>
            {
                new VariantPriceEntry { category = ResourceCategory.Crop, pcPrice = 40, plPrice = 30, pmPrice = 25 },
                new VariantPriceEntry { category = ResourceCategory.Livestock, pcPrice = 60, plPrice = 50, pmPrice = 40 },
                new VariantPriceEntry { category = ResourceCategory.Material, pcPrice = 50, plPrice = 40, pmPrice = 35 }
            };
        }
    }

    private VariantPriceEntry? GetPriceEntry(VariantScriptableObject variant)
    {
        if (variant == null) return null;
        foreach (var e in variantPrices)
        {
            if (variant.category == e.category) return e;
        }
        return null;
    }

    // -------- 交易接口 --------
    // 1) 资源 -> 变种：使用原始资源进行支付，成功后注册变种并获得 1 个变种单位
    // 支付时可指定使用哪类资源支付（例如用 Crop 支付来换取 Livestock 变种）
    public bool BuyVariantWithResources(VariantScriptableObject variant, ResourceCategory payWithCategory)
    {
        if (variant == null) { Debug.LogWarning("BuyVariantWithResources: variant 为 null"); return false; }
        if (ResourceManager.Instance == null) return false;
        var entry = GetPriceEntry(variant);
        if (entry == null) { Debug.LogWarning("未找到变种价格条目"); return false; }

        float price = 0f;
        if (payWithCategory == ResourceCategory.Crop) price = entry.Value.pcPrice;
        else if (payWithCategory == ResourceCategory.Livestock) price = entry.Value.plPrice;
        else price = entry.Value.pmPrice;

        if (!ResourceManager.Instance.ConsumeResource(payWithCategory, price))
        {
            Debug.LogWarning($"购买变种失败：{variant.resourceName}，{payWithCategory} 不足（需要 {price}）。");
            return false;
        }

        if (VariantManager.Instance == null) 
        { 
            Debug.LogWarning("VariantManager 未就绪"); 
            return false; 
        }

        bool reg = VariantManager.Instance.ApplyVariantToResource(variant.originalSpecies, variant);
        if (!reg) 
        { 
            Debug.LogWarning("注册变种失败"); 
            return false; 
        }
        Debug.Log($"购买变种成功：{variant.resourceName}");
        return true;
    }
}
