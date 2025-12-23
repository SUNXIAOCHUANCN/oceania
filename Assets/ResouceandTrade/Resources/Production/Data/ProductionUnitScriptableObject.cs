using UnityEngine;

public enum ProdcutionUnitCategory
{
    Field,        // 田地，用于生产作物
    Pasture,   // 牧场，用于饲养牲畜
    Plantation     // 栽培林，用于生产材料
}

[CreateAssetMenu(fileName = "NewProductionUnit", menuName = "Systems/Production/Production Unit")]
public class ProductionUnitScriptableObject : ScriptableObject
{
    public string unitID;                           // 生产单元的ID（便于辨识）

    public ProdcutionUnitCategory category;         // 生产单元类别
    public bool canProduction = false;              // 是否可以进行生产

    public ResourceScriptableObject Resource;       // 当前生产单元生产的资源（可为 null）
    public float ProductionStart = -1f;             // 生产开始时间（运行时可更新）

    public Person Manager = null;                   // 是否有管理者      
    public bool hasFarmer = false;                  // 是否有农民管理（+20%）
    public bool autoReplant = true;                 // 收获后是否自动生产
    
    [Header("Construction")]
    [Tooltip("需要多少工人同时工作才能完成建设（默认 1）")]
    public int requiredWorkers = 1;
    [Tooltip("需要消耗多少材料才能完成建设")]
    public float requiredMaterial = 1f;

    // 运行时字段：已分配工人数量 & 开始工作时间（不会序列化到磁盘）
    [System.NonSerialized]
    public Person Worker = null;
    [System.NonSerialized]
    public float workerAssignedStart = -1f;
    // 运行时记录字段（不会序列化到磁盘）
    [System.NonSerialized]
    public float lastProduce = -1f;                 // 最近一次产出时间（用于牧场）
    [System.NonSerialized]
    public float lastYield = -1f;                   // 最近一次月产时间（用于栽培林）
    public float accumulatedPlantingTime;
    public string currentPlantedResourceName;
}