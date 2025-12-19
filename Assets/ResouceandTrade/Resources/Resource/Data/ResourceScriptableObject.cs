using UnityEngine;

public enum ResourceCategory
{
    Crop,        // 作物
    Livestock,   // 牲畜
    Material     // 材料
}

[CreateAssetMenu(fileName = "NewResource", menuName = "Systems/Resource/New Resource Data")]
public class ResourceScriptableObject : ScriptableObject
{
    [Header("基本信息")]
    public string resourceName;       // 资源名称 (e.g. "薯蓣")
    public Sprite icon;               // UI图标
    public ResourceCategory category; // 分类

    [Header("生产属性")]
    public float baseYield;        // 基础产量
    public float growthTime;     // 生长/生产所需时间 (秒)

    [Header("退化属性")]
    [Tooltip("每月退化的百分比 (0-1)，按月累加。配合 ResourceManager.degradationIntervalInMonths 使用。")]
    public float degradationRate;     // 退化速度（按月）
    [Tooltip("最低效率下限 (0-1)，例如 0.1 表示至少保留 10% 产量")]
    public float minEfficiency = 0.1f; // 最低保留百分比
    
    [Header("消耗属性（仅牲畜类适用）")]
    [Tooltip("牲畜每周期（每月）需要的作物消耗量，单位与作物资源一致（例如 pc 单位）。仅在 ResourceCategory.Livestock 时有效。")]
    public float baseConsumption = 0;
}