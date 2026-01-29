using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // 资源数量存储
    private Dictionary<string, float> resourceAmounts = new Dictionary<string, float>();

    // 资源耗尽事件
    public System.Action<string> OnResourceDepleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // 初始化三种资源为默认值（新存档时使用）
        resourceAmounts["Crop"] = 1000f;
        resourceAmounts["Ani"] = 1000f;  // Ani 代表 Livestock
        resourceAmounts["Mat"] = 1000f;  // Mat 代表 Material
    }

    private void Start()
    {
        // 订阅资源变化事件，检测资源是否耗尽
        // 这里使用一个简单的更新来检测资源变化
    }

    #region Crop资源管理
    // 获取Crop资源数量
    public float GetCropAmount()
    {
        return resourceAmounts.ContainsKey("Crop") ? resourceAmounts["Crop"] : 0f;
    }

    // 添加Crop资源数量
    public void AddCrop(float amount)
    {
        if (resourceAmounts.ContainsKey("Crop"))
        {
            resourceAmounts["Crop"] += amount;
        }
        else
        {
            resourceAmounts["Crop"] = amount;
        }
        Debug.Log($"Crop资源增加: +{amount}, 当前数量: {resourceAmounts["Crop"]}");
        SaveResources(); // 自动保存
        CheckResourceDepleted("Crop");
    }

    // 手动减少Crop资源数量（先检测是否足够）
    public bool ConsumeCrop(float amount)
    {
        if (resourceAmounts.ContainsKey("Crop") && resourceAmounts["Crop"] >= amount)
        {
            resourceAmounts["Crop"] -= amount;
            Debug.Log($"Crop资源消耗: -{amount}, 剩余数量: {resourceAmounts["Crop"]}");
            SaveResources(); // 自动保存
            CheckResourceDepleted("Crop");
            return true;
        }
        else
        {
            Debug.LogWarning($"Crop资源不足，需要: {amount}, 当前: {GetCropAmount()}");
            return false;
        }
    }
    
    // 自动减少Crop资源数量（直接减少，不管是否会导致负数）
    public void AutoConsumeCrop(float amount)
    {
        if (resourceAmounts.ContainsKey("Crop"))
        {
            resourceAmounts["Crop"] -= amount;
        }
        else
        {
            resourceAmounts["Crop"] = -amount;
        }
        Debug.Log($"Crop资源自动消耗: -{amount}, 剩余数量: {resourceAmounts["Crop"]}");
        SaveResources(); // 自动保存
        CheckResourceDepleted("Crop");
    }
    #endregion

    #region Ani资源管理
    // 获取Ani资源数量
    public float GetAniAmount()
    {
        return resourceAmounts.ContainsKey("Ani") ? resourceAmounts["Ani"] : 0f;
    }

    // 添加Ani资源数量
    public void AddAni(float amount)
    {
        if (resourceAmounts.ContainsKey("Ani"))
        {
            resourceAmounts["Ani"] += amount;
        }
        else
        {
            resourceAmounts["Ani"] = amount;
        }
        Debug.Log($"Ani资源增加: +{amount}, 当前数量: {resourceAmounts["Ani"]}");
        SaveResources(); // 自动保存
        CheckResourceDepleted("Ani");
    }

    // 手动减少Ani资源数量（先检测是否足够）
    public bool ConsumeAni(float amount)
    {
        if (resourceAmounts.ContainsKey("Ani") && resourceAmounts["Ani"] >= amount)
        {
            resourceAmounts["Ani"] -= amount;
            Debug.Log($"Ani资源消耗: -{amount}, 剩余数量: {resourceAmounts["Ani"]}");
            SaveResources(); // 自动保存
            CheckResourceDepleted("Ani");
            return true;
        }
        else
        {
            Debug.LogWarning($"Ani资源不足，需要: {amount}, 当前: {GetAniAmount()}");
            return false;
        }
    }
    
    // 自动减少Ani资源数量（直接减少，不管是否会导致负数）
    public void AutoConsumeAni(float amount)
    {
        if (resourceAmounts.ContainsKey("Ani"))
        {
            resourceAmounts["Ani"] -= amount;
        }
        else
        {
            resourceAmounts["Ani"] = -amount;
        }
        Debug.Log($"Ani资源自动消耗: -{amount}, 剩余数量: {resourceAmounts["Ani"]}");
        SaveResources(); // 自动保存
        CheckResourceDepleted("Ani");
    }
    #endregion

    #region Mat资源管理
    // 获取Mat资源数量
    public float GetMatAmount()
    {
        return resourceAmounts.ContainsKey("Mat") ? resourceAmounts["Mat"] : 0f;
    }

    // 添加Mat资源数量
    public void AddMat(float amount)
    {
        if (resourceAmounts.ContainsKey("Mat"))
        {
            resourceAmounts["Mat"] += amount;
        }
        else
        {
            resourceAmounts["Mat"] = amount;
        }
        Debug.Log($"Mat资源增加: +{amount}, 当前数量: {resourceAmounts["Mat"]}");
        SaveResources(); // 自动保存
        CheckResourceDepleted("Mat");
    }

    // 手动减少Mat资源数量（先检测是否足够）
    public bool ConsumeMat(float amount)
    {
        if (resourceAmounts.ContainsKey("Mat") && resourceAmounts["Mat"] >= amount)
        {
            resourceAmounts["Mat"] -= amount;
            Debug.Log($"Mat资源消耗: -{amount}, 剩余数量: {resourceAmounts["Mat"]}");
            SaveResources(); // 自动保存
            CheckResourceDepleted("Mat");
            return true;
        }
        else
        {
            Debug.LogWarning($"Mat资源不足，需要: {amount}, 当前: {GetMatAmount()}");
            return false;
        }
    }
    
    // 自动减少Mat资源数量（直接减少，不管是否会导致负数）
    public void AutoConsumeMat(float amount)
    {
        if (resourceAmounts.ContainsKey("Mat"))
        {
            resourceAmounts["Mat"] -= amount;
        }
        else
        {
            resourceAmounts["Mat"] = -amount;
        }
        Debug.Log($"Mat资源自动消耗: -{amount}, 剩余数量: {resourceAmounts["Mat"]}");
        SaveResources(); // 自动保存
        CheckResourceDepleted("Mat");
    }
    #endregion

    // 检查资源是否耗尽
    private void CheckResourceDepleted(string resourceName)
    {
        if (resourceAmounts.ContainsKey(resourceName) && resourceAmounts[resourceName] <= 0)
        {
            Debug.LogWarning($"{resourceName} 资源已耗尽！");
            OnResourceDepleted?.Invoke(resourceName); // 触发全局广播
        }
    }

    // 保存资源数据到PlayerPrefs
    private void SaveResources()
    {
        PlayerPrefs.SetFloat("CropAmount", resourceAmounts["Crop"]);
        PlayerPrefs.SetFloat("AniAmount", resourceAmounts["Ani"]);
        PlayerPrefs.SetFloat("MatAmount", resourceAmounts["Mat"]);
        PlayerPrefs.Save();
    }

    // 从PlayerPrefs加载资源数据
    private void LoadResources()
    {
        // 如果有保存的数据，使用保存的数据；否则使用默认值1000
        if (PlayerPrefs.HasKey("CropAmount"))
            resourceAmounts["Crop"] = PlayerPrefs.GetFloat("CropAmount");
        else
            resourceAmounts["Crop"] = 1000f;
            
        if (PlayerPrefs.HasKey("AniAmount"))
            resourceAmounts["Ani"] = PlayerPrefs.GetFloat("AniAmount");
        else
            resourceAmounts["Ani"] = 1000f;
            
        if (PlayerPrefs.HasKey("MatAmount"))
            resourceAmounts["Mat"] = PlayerPrefs.GetFloat("MatAmount");
        else
            resourceAmounts["Mat"] = 1000f;
    }

    // 在游戏开始时加载资源
    private void OnEnable()
    {
        LoadResources();
    }

    // 在游戏结束时保存资源
    private void OnDisable()
    {
        SaveResources();
    }

    // 在应用程序退出时保存资源
    private void OnApplicationQuit()
    {
        SaveResources();
    }
}