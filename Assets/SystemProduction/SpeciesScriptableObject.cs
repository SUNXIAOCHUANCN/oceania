using UnityEngine;

public enum SpeciesType
{
    Crop,   // 作物
    Ani,    // 动物
    Mat     // 材料
}

public enum SpeciesSource
{
    长尾鸟岛,
    十字星岛,
    热火山岛,
    无名花岛
}

public enum SpeciesState
{
    驯化,
    野化
}

[CreateAssetMenu(fileName = "NewSpecies", menuName = "System/Production/Species Data", order = 1)]
public class SpeciesScriptableObject : ScriptableObject
{
    [Header("基本信息")]
    public string speciesName;        // 物种名称
    public SpeciesType speciesType;   // 物种类型
    public string speciesDescription; // 描述
    public Sprite icon;               // 图标
    public SpeciesSource source;      // 来源
    public SpeciesState state;        // 野化状态
    public bool unlocked = false;     // 是否解锁

    [Header("产量属性")]
    public float initialYield;        // 初始产量
    public float growthPhases;        // 生长阶段数
    public float decayPerPhase;          // 每个phase衰退产量
    public float leastYield;             // 最低产量
    [Header("消耗属性")]
    public float initialCropConsumption;      // 初始消耗谷物
    public float monthlyCropConsumption;      // 每个月相消耗谷物

    // 只读属性：下个月相产量，初始时等于initialYield
    [Header("只读属性")]
    [SerializeField]
    private float _nextPhaseYield;
    
    public float nextPhaseYield
    {
        get { return _nextPhaseYield; }
    }

    private void OnEnable()
    {
        // 在加载时设置_nextPhaseYield为initialYield
        _nextPhaseYield = initialYield;
    }
}