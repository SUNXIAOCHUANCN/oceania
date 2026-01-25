using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 人口系统存档管理器
/// </summary>
public class PersonSaveSystem : MonoBehaviour
{
    public static PersonSaveSystem Instance { get; private set; }

    private const string SAVE_FILE_NAME = "population_save.dat";
    private const int CURRENT_SAVE_VERSION = 1;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

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
    /// 保存人口数据
    /// </summary>
    public void SavePopulationData(PopulationSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("[PersonSaveSystem] 无法保存空数据");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[PersonSaveSystem] 人口数据已保存 ({saveData.personsData.Count} 个人员)");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PersonSaveSystem] 保存失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载人口数据
    /// </summary>
    public PopulationSaveData LoadPopulationData()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("[PersonSaveSystem] 未找到人口存档，返回新数据");
            return new PopulationSaveData();
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            PopulationSaveData saveData = JsonUtility.FromJson<PopulationSaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("[PersonSaveSystem] 存档反序列化失败");
                return new PopulationSaveData();
            }

            // 版本升级逻辑
            if (saveData.saveVersion < CURRENT_SAVE_VERSION)
            {
                Debug.Log($"[PersonSaveSystem] 升级存档版本: {saveData.saveVersion} -> {CURRENT_SAVE_VERSION}");
                saveData = UpgradeSaveData(saveData);
            }

            Debug.Log($"[PersonSaveSystem] 成功加载人口存档 ({saveData.personsData.Count} 个人员)");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PersonSaveSystem] 加载人口存档失败: {e.Message}");
            return new PopulationSaveData();
        }
    }

    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool SaveExists()
    {
        return File.Exists(SaveFilePath);
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("[PersonSaveSystem] 人口存档已删除");
        }
    }

    /// <summary>
    /// 升级存档版本
    /// </summary>
    private PopulationSaveData UpgradeSaveData(PopulationSaveData oldData)
    {
        // 未来版本升级逻辑
        // 例如：v1 -> v2 添加新字段
        oldData.saveVersion = CURRENT_SAVE_VERSION;
        return oldData;
    }

    /// <summary>
    /// 获取存档文件路径（用于调试）
    /// </summary>
    public string GetSaveFilePath()
    {
        return SaveFilePath;
    }
}
