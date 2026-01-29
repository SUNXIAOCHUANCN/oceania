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
        DebugTool.LogResource("Awake() 开始执行");

        if (Instance != null && Instance != this)
        {
            DebugTool.LogResource("检测到重复实例，销毁当前对象");
            Destroy(this);
        }
        else
        {
            Instance = this;
            DebugTool.LogResource("ResourceManager 单例已设置");
        }

        // 初始化三种资源为默认值（新存档时使用）
        resourceAmounts["Crop"] = 1000f;
        resourceAmounts["Ani"] = 1000f;  // Ani 代表 Livestock
        resourceAmounts["Mat"] = 1000f;  // Mat 代表 Material

        DebugTool.LogResource("资源初始化完成: Crop={0}, Ani={1}, Mat={2}",
            resourceAmounts["Crop"], resourceAmounts["Ani"], resourceAmounts["Mat"]);
        DebugTool.LogResource("Awake() 完成");
    }

    private void Start()
    {
        DebugTool.LogResource("Start() 开始执行");
        // 订阅资源变化事件，检测资源是否耗尽
        // 这里使用一个简单的更新来检测资源变化
        DebugTool.LogResource("Start() 完成");
    }

    #region Crop资源管理
    // 获取Crop资源数量
    public float GetCropAmount()
    {
        float amount = resourceAmounts.ContainsKey("Crop") ? resourceAmounts["Crop"] : 0f;
        DebugTool.LogResource("GetCropAmount() 调用，返回: {0}", amount);
        return amount;
    }

    // 添加Crop资源数量
    public void AddCrop(float amount)
    {
        DebugTool.LogResource("AddCrop() 开始执行，添加数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Crop"))
        {
            resourceAmounts["Crop"] += amount;
        }
        else
        {
            resourceAmounts["Crop"] = amount;
        }

        DebugTool.LogResource("Crop资源增加: +{0}, 当前数量: {1}", amount, resourceAmounts["Crop"]);
        SaveResources(); // 自动保存
        CheckResourceDepleted("Crop");

        DebugTool.LogResource("AddCrop() 完成");
    }

    // 手动减少Crop资源数量（先检测是否足够）
    public bool ConsumeCrop(float amount)
    {
        DebugTool.LogResource("ConsumeCrop() 开始执行，消耗数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Crop") && resourceAmounts["Crop"] >= amount)
        {
            resourceAmounts["Crop"] -= amount;
            DebugTool.LogResource("Crop资源消耗: -{0}, 剩余数量: {1}", amount, resourceAmounts["Crop"]);
            SaveResources(); // 自动保存
            CheckResourceDepleted("Crop");

            DebugTool.LogResource("ConsumeCrop() 成功完成");
            return true;
        }
        else
        {
            DebugTool.LogWarning("ResourceManager", "Crop资源不足，需要: {0}, 当前: {1}", amount, GetCropAmount());
            DebugTool.LogResource("ConsumeCrop() 失败");
            return false;
        }
    }

    // 自动减少Crop资源数量（直接减少，不管是否会导致负数）
    public void AutoConsumeCrop(float amount)
    {
        DebugTool.LogResource("AutoConsumeCrop() 开始执行，消耗数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Crop"))
        {
            resourceAmounts["Crop"] -= amount;
        }
        else
        {
            resourceAmounts["Crop"] = -amount;
        }

        DebugTool.LogResource("Crop资源自动消耗: -{0}, 剩余数量: {1}", amount, resourceAmounts["Crop"]);
        SaveResources(); // 自动保存
        CheckResourceDepleted("Crop");

        DebugTool.LogResource("AutoConsumeCrop() 完成");
    }
    #endregion

    #region Ani资源管理
    // 获取Ani资源数量
    public float GetAniAmount()
    {
        float amount = resourceAmounts.ContainsKey("Ani") ? resourceAmounts["Ani"] : 0f;
        DebugTool.LogResource("GetAniAmount() 调用，返回: {0}", amount);
        return amount;
    }

    // 添加Ani资源数量
    public void AddAni(float amount)
    {
        DebugTool.LogResource("AddAni() 开始执行，添加数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Ani"))
        {
            resourceAmounts["Ani"] += amount;
        }
        else
        {
            resourceAmounts["Ani"] = amount;
        }

        DebugTool.LogResource("Ani资源增加: +{0}, 当前数量: {1}", amount, resourceAmounts["Ani"]);
        SaveResources(); // 自动保存
        CheckResourceDepleted("Ani");

        DebugTool.LogResource("AddAni() 完成");
    }

    // 手动减少Ani资源数量（先检测是否足够）
    public bool ConsumeAni(float amount)
    {
        DebugTool.LogResource("ConsumeAni() 开始执行，消耗数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Ani") && resourceAmounts["Ani"] >= amount)
        {
            resourceAmounts["Ani"] -= amount;
            DebugTool.LogResource("Ani资源消耗: -{0}, 剩余数量: {1}", amount, resourceAmounts["Ani"]);
            SaveResources(); // 自动保存
            CheckResourceDepleted("Ani");

            DebugTool.LogResource("ConsumeAni() 成功完成");
            return true;
        }
        else
        {
            DebugTool.LogWarning("ResourceManager", "Ani资源不足，需要: {0}, 当前: {1}", amount, GetAniAmount());
            DebugTool.LogResource("ConsumeAni() 失败");
            return false;
        }
    }

    // 自动减少Ani资源数量（直接减少，不管是否会导致负数）
    public void AutoConsumeAni(float amount)
    {
        DebugTool.LogResource("AutoConsumeAni() 开始执行，消耗数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Ani"))
        {
            resourceAmounts["Ani"] -= amount;
        }
        else
        {
            resourceAmounts["Ani"] = -amount;
        }

        DebugTool.LogResource("Ani资源自动消耗: -{0}, 剩余数量: {1}", amount, resourceAmounts["Ani"]);
        SaveResources(); // 自动保存
        CheckResourceDepleted("Ani");

        DebugTool.LogResource("AutoConsumeAni() 完成");
    }
    #endregion

    #region Mat资源管理
    // 获取Mat资源数量
    public float GetMatAmount()
    {
        float amount = resourceAmounts.ContainsKey("Mat") ? resourceAmounts["Mat"] : 0f;
        DebugTool.LogResource("GetMatAmount() 调用，返回: {0}", amount);
        return amount;
    }

    // 添加Mat资源数量
    public void AddMat(float amount)
    {
        DebugTool.LogResource("AddMat() 开始执行，添加数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Mat"))
        {
            resourceAmounts["Mat"] += amount;
        }
        else
        {
            resourceAmounts["Mat"] = amount;
        }

        DebugTool.LogResource("Mat资源增加: +{0}, 当前数量: {1}", amount, resourceAmounts["Mat"]);
        SaveResources(); // 自动保存
        CheckResourceDepleted("Mat");

        DebugTool.LogResource("AddMat() 完成");
    }

    // 手动减少Mat资源数量（先检测是否足够）
    public bool ConsumeMat(float amount)
    {
        DebugTool.LogResource("ConsumeMat() 开始执行，消耗数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Mat") && resourceAmounts["Mat"] >= amount)
        {
            resourceAmounts["Mat"] -= amount;
            DebugTool.LogResource("Mat资源消耗: -{0}, 剩余数量: {1}", amount, resourceAmounts["Mat"]);
            SaveResources(); // 自动保存
            CheckResourceDepleted("Mat");

            DebugTool.LogResource("ConsumeMat() 成功完成");
            return true;
        }
        else
        {
            DebugTool.LogWarning("ResourceManager", "Mat资源不足，需要: {0}, 当前: {1}", amount, GetMatAmount());
            DebugTool.LogResource("ConsumeMat() 失败");
            return false;
        }
    }

    // 自动减少Mat资源数量（直接减少，不管是否会导致负数）
    public void AutoConsumeMat(float amount)
    {
        DebugTool.LogResource("AutoConsumeMat() 开始执行，消耗数量: {0}", amount);

        if (resourceAmounts.ContainsKey("Mat"))
        {
            resourceAmounts["Mat"] -= amount;
        }
        else
        {
            resourceAmounts["Mat"] = -amount;
        }

        DebugTool.LogResource("Mat资源自动消耗: -{0}, 剩余数量: {1}", amount, resourceAmounts["Mat"]);
        SaveResources(); // 自动保存
        CheckResourceDepleted("Mat");

        DebugTool.LogResource("AutoConsumeMat() 完成");
    }
    #endregion

    // 检查资源是否耗尽
    private void CheckResourceDepleted(string resourceName)
    {
        if (resourceAmounts.ContainsKey(resourceName) && resourceAmounts[resourceName] <= 0)
        {
            DebugTool.LogWarning("ResourceManager", "{0} 资源已耗尽！当前数量: {1}", resourceName, resourceAmounts[resourceName]);
            OnResourceDepleted?.Invoke(resourceName); // 触发全局广播
        }
    }

    // 保存资源数据到PlayerPrefs
    private void SaveResources()
    {
        DebugTool.LogResource("SaveResources() 开始执行");

        PlayerPrefs.SetFloat("CropAmount", resourceAmounts["Crop"]);
        PlayerPrefs.SetFloat("AniAmount", resourceAmounts["Ani"]);
        PlayerPrefs.SetFloat("MatAmount", resourceAmounts["Mat"]);
        PlayerPrefs.Save();

        DebugTool.LogResource("资源已保存: Crop={0}, Ani={1}, Mat={2}",
            resourceAmounts["Crop"], resourceAmounts["Ani"], resourceAmounts["Mat"]);
        DebugTool.LogResource("SaveResources() 完成");
    }

    // 从PlayerPrefs加载资源数据
    private void LoadResources()
    {
        DebugTool.LogResource("LoadResources() 开始执行");

        // 如果有保存的数据，使用保存的数据；否则使用默认值1000
        if (PlayerPrefs.HasKey("CropAmount"))
        {
            resourceAmounts["Crop"] = PlayerPrefs.GetFloat("CropAmount");
            DebugTool.LogResource("从存档加载Crop资源: {0}", resourceAmounts["Crop"]);
        }
        else
        {
            resourceAmounts["Crop"] = 1000f;
            DebugTool.LogResource("使用默认值Crop资源: {0}", resourceAmounts["Crop"]);
        }

        if (PlayerPrefs.HasKey("AniAmount"))
        {
            resourceAmounts["Ani"] = PlayerPrefs.GetFloat("AniAmount");
            DebugTool.LogResource("从存档加载Ani资源: {0}", resourceAmounts["Ani"]);
        }
        else
        {
            resourceAmounts["Ani"] = 1000f;
            DebugTool.LogResource("使用默认值Ani资源: {0}", resourceAmounts["Ani"]);
        }

        if (PlayerPrefs.HasKey("MatAmount"))
        {
            resourceAmounts["Mat"] = PlayerPrefs.GetFloat("MatAmount");
            DebugTool.LogResource("从存档加载Mat资源: {0}", resourceAmounts["Mat"]);
        }
        else
        {
            resourceAmounts["Mat"] = 1000f;
            DebugTool.LogResource("使用默认值Mat资源: {0}", resourceAmounts["Mat"]);
        }

        DebugTool.LogResource("LoadResources() 完成");
    }

    // 在游戏开始时加载资源
    private void OnEnable()
    {
        DebugTool.LogResource("OnEnable() 开始执行");
        LoadResources();
        DebugTool.LogResource("OnEnable() 完成");
    }

    // 在游戏结束时保存资源
    private void OnDisable()
    {
        DebugTool.LogResource("OnDisable() 开始执行");
        SaveResources();
        DebugTool.LogResource("OnDisable() 完成");
    }

    // 在应用程序退出时保存资源
    private void OnApplicationQuit()
    {
        DebugTool.LogResource("OnApplicationQuit() 开始执行");
        SaveResources();
        DebugTool.LogResource("OnApplicationQuit() 完成");
    }
}
