using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个人员的存档数据
/// </summary>
[System.Serializable]
public class PersonSaveData
{
    public string personName;           // 人员名称（唯一标识符）
    public bool isRecruited;            // 是否已招募
    public string statusName;           // 当前状态（PersonStatus枚举转字符串）

    public PersonSaveData() { }

    public PersonSaveData(string name, bool recruited, PersonStatus status)
    {
        personName = name;
        isRecruited = recruited;
        statusName = status.ToString();
    }
}

/// <summary>
/// 人口系统全局存档数据
/// </summary>
[System.Serializable]
public class PopulationSaveData
{
    // 所有人员的存档数据
    public List<PersonSaveData> personsData;

    // 存档版本（用于未来升级）
    public int saveVersion = 1;

    public PopulationSaveData()
    {
        personsData = new List<PersonSaveData>();
    }

    /// <summary>
    /// 查找或创建人员的存档数据
    /// </summary>
    public PersonSaveData GetOrCreatePersonData(string personName)
    {
        PersonSaveData personData = personsData.Find(p => p.personName == personName);

        if (personData == null)
        {
            personData = new PersonSaveData();
            personData.personName = personName;
            personData.isRecruited = false;
            personData.statusName = PersonStatus.rest.ToString();
            personsData.Add(personData);
        }

        return personData;
    }

    /// <summary>
    /// 检查是否有人员数据
    /// </summary>
    public bool HasPersonData(string personName)
    {
        return personsData.Exists(p => p.personName == personName);
    }

    /// <summary>
    /// 获取已招募的人员名称列表
    /// </summary>
    public List<string> GetRecruitedPersonNames()
    {
        List<string> recruitedNames = new List<string>();
        foreach (var personData in personsData)
        {
            if (personData.isRecruited)
            {
                recruitedNames.Add(personData.personName);
            }
        }
        return recruitedNames;
    }
}
