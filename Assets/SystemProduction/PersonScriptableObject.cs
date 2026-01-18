using UnityEngine;

public enum PersonProfession
{
    observer,
    tamer,
    farmer,
    sailor,
    witcher,
    civilian
}

public enum PersonStatus
{
    rest,       // 休息
    onsea,      // 在海上
    infarm,     // 在农场
    inManagerFarm, // 在管理农场
    inranch,    // 在牧场
    inforest    // 在栽培林
}

[CreateAssetMenu(fileName = "NewPerson", menuName = "System/Person Data", order = 1)]
public class PersonScriptableObject : ScriptableObject
{
    [Header("基本信息")]
    public string personName;         // 人员名称
    public PersonProfession profession; // 职业
    public Sprite avatar;             // 头像
    public bool recruited = false;    // 是否已招募
    
    [Header("职业描述")]
    [TextArea]
    public string professionDescription; // 职业描述
    
    [Header("状态")]
    public PersonStatus status;       // 状态
    
    [Header("消耗属性")]
    public float monthlyCropConsumption = 0;    // 每月消耗Crop
    public float monthlyAniConsumption = 0;     // 每月消耗Ani
    public float monthlyMatConsumption = 0;     // 每月消耗Mat
    
    private void OnValidate()
    {
        // 根据职业自动设置职业描述
        SetProfessionDescription();
    }
    
    private void SetProfessionDescription()
    {
        switch (profession)
        {
            case PersonProfession.observer:
                professionDescription = "在海上航行时可以辨认东南西北";
                break;
            case PersonProfession.tamer:
                professionDescription = "携带tamer出海可以采集野生的物种";
                break;
            case PersonProfession.farmer:
                professionDescription = "在农场、牧场、栽培林管理时，生产增加20%";
                break;
            case PersonProfession.sailor:
                professionDescription = "航行时速度增加20%";
                break;
            case PersonProfession.witcher:
                professionDescription = "闲置时每个月相祈祷可以获得一定资源";
                break;
            case PersonProfession.civilian:
                professionDescription = "普通人员";
                break;
        }
    }
}