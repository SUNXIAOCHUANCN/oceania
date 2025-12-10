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
    public int baseYield;        // 基础产量
    public float growthTime;     // 生长/生产所需时间 (秒)

    [Header("退化属性")]
    [Tooltip("每次收获后产量降低的百分比 (0-1)")]
    public float degradationRate;     // 退化速度

    [Header("价值属性")]
    public float value;               // 贸易价值
}