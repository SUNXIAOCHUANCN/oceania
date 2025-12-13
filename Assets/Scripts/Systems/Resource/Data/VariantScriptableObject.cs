using UnityEngine;
public enum VariantType
{
    Tamed, // 驯化变种 (高产快衰)
    Wild   // 野生变种 (低产慢衰)
}

[CreateAssetMenu(fileName = "NewVariant", menuName = "Systems/Resource/New Variant Data")]

public class VariantScriptableObject : ResourceScriptableObject
{
    [Header("--- 变种系统专属配置 ---")]
    [Tooltip("变种类型：决定了获取方式（贸易岛 vs 野生岛）及UI标签")]
    public VariantType variantType;

    [Tooltip("关联的基础物种。例如：'良薯蓣' 和 '野薯蓣' 都应该关联到 '薯蓣(原始种)'。用于UI归类或逻辑查找。")]
    public ResourceScriptableObject originalSpecies;
    public bool unlocked = false;    // 该变种是否已解锁

    public string GetFullName()
    {
        string prefix = variantType == VariantType.Tamed ? "驯化" : "野生";
        return $"{prefix} - {resourceName}";
    }
}
