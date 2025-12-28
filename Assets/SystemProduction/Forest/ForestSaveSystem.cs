using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ForestSpeciesSaveData
{
    public string speciesName;
    public string speciesDescription;
    public string speciesType;  // Crop, Ani, Mat
    public string source;       // 长尾鸟岛, 十字星岛, 热火山岛, 无名花岛
    public string state;        // 驯化, 野化
    public bool unlocked;
    public float initialYield;
    public float growthPhases;
    public float decayPerPhase;
    public float leastYield;
    public float initialCropConsumption;
    public float monthlyCropConsumption;
    public float amount;
    public float nextPhaseYield;
    
    // 构造函数用于从SpeciesScriptableObject创建保存数据
    public ForestSpeciesSaveData(SpeciesScriptableObject species, float amt, float nextYield)
    {
        speciesName = species.speciesName;
        speciesDescription = species.speciesDescription;
        speciesType = species.speciesType.ToString();
        source = species.source.ToString();
        state = species.state.ToString();
        unlocked = species.unlocked;
        initialYield = species.initialYield;
        growthPhases = species.growthPhases;
        decayPerPhase = species.decayPerPhase;
        leastYield = species.leastYield;
        initialCropConsumption = species.initialCropConsumption;
        monthlyCropConsumption = species.monthlyCropConsumption;
        amount = amt;
        nextPhaseYield = nextYield;
    }
    
    // 默认构造函数
    public ForestSpeciesSaveData()
    {
    }
}

[System.Serializable]
public class ForestSaveData
{
    public string managerPersonName;
    public List<ForestSpeciesSaveData> forestDatabase = new List<ForestSpeciesSaveData>();
    public int saveVersion = 1;
}

public class ForestSaveSystem : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "forest_save.dat";
    private const int CURRENT_SAVE_VERSION = 1;
    
    public static ForestSaveSystem Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 保存森林数据
    /// </summary>
    public void SaveForestData(ForestSystem forestSystem)
    {
        ForestSaveData saveData = new ForestSaveData();
        
        // 保存管理者
        if (forestSystem.GetManager() != null)
        {
            saveData.managerPersonName = forestSystem.GetManager().personName;
        }
        
        // 保存森林数据库 - 保存所有物种信息和amount、nextPhaseYield
        foreach (ForestSpeciesData species in forestSystem.GetForestDatabase())
        {
            // 由于我们不知道具体是哪个SpeciesScriptableObject，
            // 我们只能保存变化的数据：amount和nextPhaseYield
            // 其他信息在加载时通过物种名称重新获取
            saveData.forestDatabase.Add(new ForestSpeciesSaveData()
            {
                speciesName = species.speciesName,
                amount = species.amount,
                nextPhaseYield = species.nextPhaseYield
            });
        }
        
        // 序列化并保存
        string json = JsonUtility.ToJson(saveData, true);
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        File.WriteAllText(fullPath, json);
        Debug.Log($"森林数据已保存到: {fullPath}");
    }
    
    /// <summary>
    /// 加载森林数据
    /// </summary>
    public bool LoadForestData(ForestSystem forestSystem)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        if (!File.Exists(fullPath))
        {
            Debug.Log("未找到森林存档，使用初始状态");
            return false;
        }
        
        try
        {
            string json = File.ReadAllText(fullPath);
            ForestSaveData saveData = JsonUtility.FromJson<ForestSaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("Failed to deserialize save data. JSON might be corrupted or empty.");
                return false;
            }
            
            // 处理不同版本的存档
            if (saveData.saveVersion < CURRENT_SAVE_VERSION)
            {
                Debug.Log($"升级存档版本: {saveData.saveVersion} -> {CURRENT_SAVE_VERSION}");
                // 这里可以添加版本升级逻辑
            }
            
            // 加载管理者
            if (!string.IsNullOrEmpty(saveData.managerPersonName))
            {
                PersonScriptableObject manager = FindRecruitedPersonByName(saveData.managerPersonName);
                if (manager != null)
                {
                    forestSystem.SetManager(manager);
                }
            }
            
            // 加载森林数据库
            if (saveData.forestDatabase != null)
            {
                // 使用新的方法来正确加载数据，避免操作副本的问题
                forestSystem.LoadForestDatabaseFromSave(saveData.forestDatabase);
            }
            
            Debug.Log("成功加载森林存档");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载森林存档失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool SaveExists()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        return File.Exists(fullPath);
    }
    
    /// <summary>
    /// 通过名称查找已招募的人员
    /// </summary>
    private PersonScriptableObject FindRecruitedPersonByName(string name)
    {
        if (PersonsLoader.Instance == null)
        {
            Debug.LogError("PersonsLoader.Instance is null! Cannot find recruited person by name.");
            return null;
        }
        
        List<PersonScriptableObject> recruitedPersons = PersonsLoader.Instance.GetRecruitedPersons();
        if (recruitedPersons == null)
        {
            Debug.LogWarning("GetRecruitedPersons() returned null!");
            return null;
        }
        
        return recruitedPersons.Find(p => p.personName == name);
    }
    
    /// <summary>
    /// 通过名称查找物种
    /// </summary>
    private SpeciesScriptableObject FindSpeciesByName(string name)
    {
        if (SpeciesLoader.Instance == null)
        {
            Debug.LogError("SpeciesLoader.Instance is null! Cannot find species by name.");
            return null;
        }
        
        // 从所有物种中查找
        var allSpecies = new List<SpeciesScriptableObject>();
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedCropSpecies());
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedAniSpecies());
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedMatSpecies());
        
        return allSpecies.Find(s => s.speciesName == name);
    }
}