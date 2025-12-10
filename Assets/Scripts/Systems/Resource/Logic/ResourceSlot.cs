using System;

[Serializable] // 序列化，方便在 Inspector 里查看调试
public class ResourceSlot
{
    public ResourceScriptableObject data;      // 引用静态数据 (是什么?)
    public int amount;           // 当前拥有数量
    public float qualityMultiplier; // 基因质量 (1.0 = 100%, 越低产量越少)

    public ResourceSlot(ResourceScriptableObject resourceData)
    {
        data = resourceData;
        amount = 0;
        qualityMultiplier = 1.0f; // 初始为完美基因
    }

    // 计算当前一次能产出多少
    public int GetCurrentYield()
    {
        return UnityEngine.Mathf.FloorToInt(data.baseYield * qualityMultiplier);
    }

    // 发生退化
    public void Degrade()
    {
        qualityMultiplier -= data.degradationRate;
        if (qualityMultiplier < 0.1f) qualityMultiplier = 0.1f; // 最低保留10%产量
    }

    // 获得变种 (恢复基因)
    public void RestoreGene()
    {
        qualityMultiplier = 1.0f; // 或者根据变种等级提升
    }
}