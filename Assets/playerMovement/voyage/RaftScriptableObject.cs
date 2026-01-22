using UnityEngine;
using System.Collections.Generic;

public enum RaftStatus
{
    Active,     // 正在运行
    Inactive    // 不在运行
}

[CreateAssetMenu(fileName = "NewRaft", menuName = "System/Raft Data", order = 2)]
public class RaftScriptableObject : ScriptableObject
{
    [Header("基本信息")]
    public string raftName;         // 船只名称
    public GameObject raftPrefab;   // 船只预制体
    public Sprite icon;             // 船只图标
    
    [Header("载人能力")]
    public int capacity;            // 最大携带人数
    public PersonScriptableObject person1;  // 携带人1
    public PersonScriptableObject person2;  // 携带人2
    
    [Header("航行持续时间")]
    public int durationInPhases = 1; // 持续几个月相（默认1个月相）
    
    [Header("资源消耗")]
    public float cropConsumption = 0;   // 消耗Crop
    public float aniConsumption = 0;    // 消耗Ani
    public float matConsumption = 0;    // 消耗Mat
    
    [Header("状态")]
    public RaftStatus status;         // 船只状态
    
    [Header("描述")]
    [TextArea]
    public string description;        // 船只描述
    
    private void OnValidate()
    {
        // 确保容量不为负数
        if (capacity < 0)
            capacity = 0;
        
        // 确保持续时间不为负数
        if (durationInPhases < 0)
            durationInPhases = 0;
    }
}