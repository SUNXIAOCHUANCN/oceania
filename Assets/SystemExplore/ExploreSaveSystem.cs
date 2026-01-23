using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 探索系统存档管理器 - 负责探索数据的保存和加载
/// </summary>
public class ExploreSaveSystem : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "explore_save.dat";
    private const int CURRENT_SAVE_VERSION = 1;

    public static ExploreSaveSystem Instance { get; private set; }

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
    /// 保存探索数据
    /// </summary>
    public void SaveExploreData(ExploreSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("[ExploreSaveSystem] 尝试保存空数据");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            string fullPath = GetSaveFilePath();

            File.WriteAllText(fullPath, json);
            Debug.Log($"[ExploreSaveSystem] 探索数据已保存到: {fullPath}");
            Debug.Log($"[ExploreSaveSystem] 保存了 {saveData.islandsExploreData.Count} 个岛屿的数据");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExploreSaveSystem] 保存探索数据失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载探索数据
    /// </summary>
    public ExploreSaveData LoadExploreData()
    {
        string fullPath = GetSaveFilePath();

        if (!File.Exists(fullPath))
        {
            Debug.Log("[ExploreSaveSystem] 未找到探索存档，返回新数据");
            return new ExploreSaveData();
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            ExploreSaveData saveData = JsonUtility.FromJson<ExploreSaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("[ExploreSaveSystem] 存档反序列化失败，返回新数据");
                return new ExploreSaveData();
            }

            // 版本升级逻辑
            if (saveData.saveVersion < CURRENT_SAVE_VERSION)
            {
                Debug.Log($"[ExploreSaveSystem] 升级存档版本: {saveData.saveVersion} -> {CURRENT_SAVE_VERSION}");
                saveData = UpgradeSaveData(saveData);
            }

            Debug.Log($"[ExploreSaveSystem] 成功加载探索存档，包含 {saveData.islandsExploreData.Count} 个岛屿的数据");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExploreSaveSystem] 加载探索存档失败: {e.Message}");
            return new ExploreSaveData();
        }
    }

    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool SaveExists()
    {
        return File.Exists(GetSaveFilePath());
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public void DeleteSave()
    {
        string fullPath = GetSaveFilePath();
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log("[ExploreSaveSystem] 探索存档已删除");
        }
    }

    /// <summary>
    /// 获取存档文件路径
    /// </summary>
    private string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    /// <summary>
    /// 升级存档版本
    /// </summary>
    private ExploreSaveData UpgradeSaveData(ExploreSaveData oldData)
    {
        // 未来版本升级逻辑
        // 例如：if (oldData.saveVersion == 1) { /* 迁移到版本2 */ }

        oldData.saveVersion = CURRENT_SAVE_VERSION;
        return oldData;
    }
}
