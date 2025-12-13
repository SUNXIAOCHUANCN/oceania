using UnityEngine;
using System;

[Serializable]
public class Person
{
    // 运行时数据
    public string uniqueId;     // 每个人的唯一ID
    public string personName;   // e.g., "阿力", "莎娜"

    [Header("职位引用")]
    public RoleScriptableObject role;         // 指向职业模板数据

    // 状态机（未来用于任务系统）
    public enum Status { Free, Sailing, Managing, Working }
    public Status currentStatus = Status.Free;

    // 对话属性 (初级实现：简单的一句话)
    public string dialogueLine = "我们有很多故事可以讲。";

    public Person(RoleScriptableObject roleTemplate, string name)
    {
        this.uniqueId = Guid.NewGuid().ToString(); // 确保ID唯一
        this.role = roleTemplate;
        this.personName = name;
    }

    // --- 核心方法：用于人口管理 ---

    // 外部系统调用此方法来获取角色的加成
    public float GetSailingBonus()
    {
        return role.sailingTimeSave;
    }

    // 驯化师是否能带回变种
    public bool CanBringBackVariant()
    {
        return role.canBringBackVariant;
    }

    // 获取巫师减少风暴几率的加成
    public bool MonthlyPrayforResource()
    {
        return role.MonthlyPrayforResource;
    }

    // 观星者是否能指示方向
    public bool CanPointOutDirection()
    {
        return role.canPointoutDirection;
    }

    public bool ManagementBonus()
    {
        return role.ManagementBonus;
    }

    // 外部系统可以调用此方法来触发对话（例如：点击NPC时）
    public string StartDialogue()
    {
        // 未来可以拓展为复杂的对话树
        return $"{personName} ({role.roleName}): \"{dialogueLine}\"";
    }
}