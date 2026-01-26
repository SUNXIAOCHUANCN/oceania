using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 线索保存数据
/// </summary>
[System.Serializable]
public class ClueSaveData
{
    public string clueName;        // 线索名称（唯一标识符）
    public bool isUnlocked;        // 解锁状态

    public ClueSaveData() { }

    public ClueSaveData(string name, bool unlocked)
    {
        clueName = name;
        isUnlocked = unlocked;
    }
}

/// <summary>
/// 秘密保存数据
/// </summary>
[System.Serializable]
public class SecretSaveData
{
    public string secretName;      // 秘密名称（唯一标识符）
    public bool isUnlocked;        // 解锁状态

    public SecretSaveData() { }

    public SecretSaveData(string name, bool unlocked)
    {
        secretName = name;
        isUnlocked = unlocked;
    }
}

/// <summary>
/// 信物保存数据
/// </summary>
[System.Serializable]
public class TokenSaveData
{
    public string tokenName;       // 信物名称（唯一标识符）
    public bool isUnlocked;        // 解锁状态

    public TokenSaveData() { }

    public TokenSaveData(string name, bool unlocked)
    {
        tokenName = name;
        isUnlocked = unlocked;
    }
}

/// <summary>
/// 物种解锁状态（用于探索系统）
/// </summary>
[System.Serializable]
public class SpeciesUnlockSaveData
{
    public string speciesName;     // 物种名称（唯一标识符）
    public bool isUnlocked;        // 解锁状态

    public SpeciesUnlockSaveData() { }

    public SpeciesUnlockSaveData(string name, bool unlocked)
    {
        speciesName = name;
        isUnlocked = unlocked;
    }
}

/// <summary>
/// 单个岛屿的探索进度
/// </summary>
[System.Serializable]
public class IslandExploreSaveData
{
    public string islandName;                  // 岛屿名称（SpeciesSource枚举转字符串）

    // 解锁状态列表
    public List<ClueSaveData> unlockedClues;           // 已解锁的线索
    public List<SecretSaveData> unlockedSecrets;       // 已解锁的秘密（通常只有1个）
    public List<TokenSaveData> unlockedTokens;         // 已解锁的信物（通常只有1个）
    public List<SpeciesUnlockSaveData> unlockedSpecies;// 已解锁的物种

    public IslandExploreSaveData()
    {
        unlockedClues = new List<ClueSaveData>();
        unlockedSecrets = new List<SecretSaveData>();
        unlockedTokens = new List<TokenSaveData>();
        unlockedSpecies = new List<SpeciesUnlockSaveData>();
    }

    public IslandExploreSaveData(string island) : this()
    {
        islandName = island;
    }
}

/// <summary>
/// 探索系统全局存档数据
/// </summary>
[System.Serializable]
public class ExploreSaveData
{
    // 所有岛屿的探索进度
    public List<IslandExploreSaveData> islandsExploreData;

    // 存档版本（用于未来升级）
    public int saveVersion = 1;

    public ExploreSaveData()
    {
        islandsExploreData = new List<IslandExploreSaveData>();
    }

    /// <summary>
    /// 获取指定岛屿的探索数据，如果不存在则创建
    /// </summary>
    public IslandExploreSaveData GetOrCreateIslandData(string islandName)
    {
        IslandExploreSaveData islandData = islandsExploreData.Find(d => d.islandName == islandName);

        if (islandData == null)
        {
            islandData = new IslandExploreSaveData(islandName);
            islandsExploreData.Add(islandData);
        }

        return islandData;
    }

    /// <summary>
    /// 检查指定岛屿是否有探索数据
    /// </summary>
    public bool HasIslandData(string islandName)
    {
        return islandsExploreData.Exists(d => d.islandName == islandName);
    }
}
