using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRole", menuName = "Systems/Population/New Role Data")]
public class RoleScriptableObject : ScriptableObject
{
    [Header("角色信息")]
    public string roleName;         // e.g., "驯化师", "水手"
    public string description;      // 职业的介绍或背景故事

    public int count = 0;               // 该职业的人口数

    [Header("基础属性 - 初始配置")]
    // 消耗：主动扩充该类职业时消耗的资源（例如：10 薯蓣）
    public List<ResourceCost> RecruitmentCosts = new List<ResourceCost>();

    [Header("周期性消耗（多资源）")]
    // 每个职业每周期可能消耗多种资源，使用 ResourceCost 列表表示
    public List<ResourceCost> durationalConsumption = new List<ResourceCost>();

    [Header("功能属性 - 职业加成")]
    // 驯化师属性
    [Tooltip("如果该角色是驯化师，能带回变种")]
    public bool canBringBackVariant = false;

    // 水手属性
    [Tooltip("如果是水手，减少航行的时间（30%）")]
    public float sailingTimeSave = 0f;

    // 巫师属性
    [Tooltip("如果是祭司，每月祈祷获得资源")]
    public bool MonthlyPrayforResource = false;

    // 观星属性
    [Tooltip("如果是观星，航行时可以指示东西南北")]
    public bool canPointoutDirection = false;

    [Tooltip("如果是农民，管理生产时获得加成")]
    public bool ManagementBonus = false;

}
