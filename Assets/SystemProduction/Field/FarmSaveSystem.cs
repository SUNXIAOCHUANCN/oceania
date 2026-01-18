using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class FieldSaveData
{
    public bool isUnlocked;
    public string cropSpeciesName;
    public int growthPhaseCount;
    public float currentNPY;
    public int cultivationProgress;
    // 新增：作物NPY记录字典的序列化形式
    public List<string> cropNPYKeys = new List<string>();
    public List<float> cropNPYValues = new List<float>();
}

[System.Serializable]
public class FarmSaveData
{
    public string managerPersonName;
    public List<FieldSaveData> fields = new List<FieldSaveData>();
    public int saveVersion = 1;
}

public class FarmSaveSystem : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "farm_save.dat";
    private const int CURRENT_SAVE_VERSION = 1;
    
    public static FarmSaveSystem Instance { get; private set; }
    
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
    /// 保存农场数据
    /// </summary>
    public void SaveFarmData(FarmSystem farmSystem)
    {
        FarmSaveData saveData = new FarmSaveData();
        
        // 保存管理者
        if (farmSystem.GetManager() != null)
        {
            saveData.managerPersonName = farmSystem.GetManager().personName;
        }
        
        // 保存所有田地数据
        foreach (FieldUnit field in farmSystem.GetFields())
        {
            FieldSaveData fieldData = new FieldSaveData
            {
                isUnlocked = field.IsUnlocked,
                growthPhaseCount = field.GrowthPhaseCount,
                currentNPY = field.CurrentNPY,
                cultivationProgress = field.GetType().GetField("cultivationProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(field) as int? ?? 0
            };
            
            // 保存作物NPY字典
            var cropLastNPY = field.GetCropLastNPY();
            foreach (var kvp in cropLastNPY)
            {
                fieldData.cropNPYKeys.Add(kvp.Key);
                fieldData.cropNPYValues.Add(kvp.Value);
            }
            
            if (field.CurrentCrop != null)
            {
                fieldData.cropSpeciesName = field.CurrentCrop.speciesName;
            }
            
            saveData.fields.Add(fieldData);
        }
        
        // 序列化并保存
        string json = JsonUtility.ToJson(saveData, true);
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        File.WriteAllText(fullPath, json);
        Debug.Log($"农场数据已保存到: {fullPath}");
    }
    
    /// <summary>
    /// 加载农场数据
    /// </summary>
    public bool LoadFarmData(FarmSystem farmSystem)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        if (!File.Exists(fullPath))
        {
            Debug.Log("未找到农场存档，使用初始状态");
            return false;
        }
        
        try
        {
            string json = File.ReadAllText(fullPath);
            FarmSaveData saveData = JsonUtility.FromJson<FarmSaveData>(json);
            
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
                    farmSystem.SetManager(manager);
                }
            }
            
            // 检查字段数据
            if (saveData.fields == null)
            {
                Debug.LogWarning("Save data fields is null. No field data to load.");
                return true; // 返回true，因为没有田地数据可以加载不是错误
            }
            
            // 检查农场系统田地
            var farmFields = farmSystem.GetFields();
            if (farmFields == null)
            {
                Debug.LogError("FarmSystem.GetFields() returned null!");
                return false;
            }
            
            // 加载田地数据
            for (int i = 0; i < Mathf.Min(saveData.fields.Count, farmSystem.GetFields().Count); i++)
            {
                FieldUnit field = farmSystem.GetFields()[i];
                FieldSaveData fieldData = saveData.fields[i];
                
                // 检查字段数据
                if (fieldData == null)
                {
                    Debug.LogWarning($"Field data at index {i} is null. Skipping.");
                    continue;
                }
                
                                field.SetUnlocked(fieldData.isUnlocked);
                field.SetCultivationProgress(fieldData.cultivationProgress);
                
                // 加载作物NPY字典
                if (fieldData.cropNPYKeys != null && fieldData.cropNPYValues != null && 
                    fieldData.cropNPYKeys.Count == fieldData.cropNPYValues.Count)
                {
                    Dictionary<string, float> loadedCropLastNPY = new Dictionary<string, float>();
                    for (int j = 0; j < fieldData.cropNPYKeys.Count; j++)
                    {
                        loadedCropLastNPY[fieldData.cropNPYKeys[j]] = fieldData.cropNPYValues[j];
                    }
                    field.SetCropLastNPY(loadedCropLastNPY);
                }
                
                if (fieldData.isUnlocked)
                {
                    // 设置作物
                    if (!string.IsNullOrEmpty(fieldData.cropSpeciesName))
                    {
                        SpeciesScriptableObject crop = FindUnlockedCropByName(fieldData.cropSpeciesName);
                        if (crop != null)
                        {
                            field.PlantCrop(crop, fieldData.growthPhaseCount);
                        }
                    }
                    
                    // 设置NPY
                    field.SetCurrentNPY(fieldData.currentNPY);
                }
            }
            
            Debug.Log("成功加载农场存档");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载农场存档失败: {e.Message}");
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
    /// 通过名称查找已解锁的作物
    /// </summary>
    private SpeciesScriptableObject FindUnlockedCropByName(string name)
    {
        if (SpeciesLoader.Instance == null)
        {
            Debug.LogError("SpeciesLoader.Instance is null! Cannot find unlocked crop by name.");
            return null;
        }
        
        List<SpeciesScriptableObject> unlockedCrops = SpeciesLoader.Instance.GetUnlockedCropSpecies();
        if (unlockedCrops == null)
        {
            Debug.LogWarning("GetUnlockedCropSpecies() returned null!");
            return null;
        }
        
        return unlockedCrops.Find(c => c.speciesName == name);
    }
}