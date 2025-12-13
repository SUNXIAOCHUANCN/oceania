using System;
using UnityEngine;

[Serializable]
public class ResourceSlot
{
    public ResourceScriptableObject data;      // 引用静态数据
    public float amount;                       // 当前拥有数量
    public float qualityMultiplier;            // 基因质量 (1.0 = 100%, 越低产量越少)

    // 变种支持：当应用变种时可调整产量倍数和退化率（运行时字段）
    [NonSerialized]
    public float variantYieldMultiplier = 1f;    // 变种对基础产量的乘数（例如 驯化 +1.5）
    [NonSerialized]
    public float variantDegradationRate = -1f;   // 如果 >0 则覆盖 data.degradationRate

    public ResourceSlot(ResourceScriptableObject resourceData)
    {
        data = resourceData;
        amount = 0;
        qualityMultiplier = 1.0f; // 初始为完美基因
        variantYieldMultiplier = 1f;
        variantDegradationRate = -1f;
    }

    // 计算当前一次能产出多少（考虑基因质量和变种乘数）
    public float GetCurrentYield()
    {
        if (data == null) return 0f;
        return data.baseYield * qualityMultiplier * Mathf.Max(0.0001f, variantYieldMultiplier);
    }

    // 发生退化。按配置的退化率（若变种覆盖则使用覆盖值），每次调用视为 1 个单位（通常为 1 个月）
    public void Degrade()
    {
        if (data == null) return;
        float rate = variantDegradationRate > 0f ? variantDegradationRate : data.degradationRate;
        qualityMultiplier -= rate;
        if (qualityMultiplier < data.minEfficiency) qualityMultiplier = data.minEfficiency; // 保证最低效率
    }

    // 重置或恢复基因/效率（获得新变种时应调用）
    public void ResetQuality()
    {
        qualityMultiplier = 1.0f;
    }

    // 清除变种效果（恢复为默认行为）
    public void ClearVariantOverrides()
    {
        variantYieldMultiplier = 1f;
        variantDegradationRate = -1f;
    }
}