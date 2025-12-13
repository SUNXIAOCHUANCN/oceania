using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("初始资源配置")]
    // 在Inspector里把做好的 Yam, Pig, Palm 拖进去，作为初始认识的资源
    public List<ResourceScriptableObject> knownResources;

    [Header("退化配置")]
    public int degradationIntervalInMonths = 1; // 每个月会发生 1 次资源退化，效率下降
    // 用于计数自上次退化经过的月数（每次月相变化记为 1 月）
    private int monthsSinceLast = 0;

    // 运行时库存字典：资源名称 -> 运行时数据P
    private Dictionary<string, ResourceSlot> inventory = new Dictionary<string, ResourceSlot>();
    // 按类别聚合的运行时库存：仅记录三类抽象资源数量（Crop, Livestock, Material）
    private Dictionary<ResourceCategory, float> categoryInventory = new Dictionary<ResourceCategory, float>();

    // 事件：当资源变化时通知UI更新 (UI系统订阅这个事件)
    public event Action OnResourceChanged;

    private void Start()
    {
        // 订阅时间系统的事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnNewCycle += HandleNewCycle;
            GlobalTimeSystem.Instance.OnPhaseChanged += OnPhaseChanged;
        }
        else
        {
            UnityEngine.Debug.LogError("未找到 GlobalTimeSystem，无法订阅时间事件。");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        InitializeResources();
    }

    // 初始化字典
    private void InitializeResources()
    {
        foreach (var res in knownResources)
        {
            if (!inventory.ContainsKey(res.resourceName))
            {
                inventory.Add(res.resourceName, new ResourceSlot(res));
            }
        }
        // 初始化类别库存为 0
        foreach (ResourceCategory c in System.Enum.GetValues(typeof(ResourceCategory)))
        {
            if (!categoryInventory.ContainsKey(c)) categoryInventory[c] = 0;
        }
    }

    // --- 公共方法供其他系统调用 ---

    // 资源增加
    public void AddResource(string resourceName, float amount)
    {
        // 解析资源名对应的类别，并将数量加入到类别库存
        ResourceCategory cat = GetCategoryByResourceName(resourceName);
        AddResource(cat, amount);
        return ;
    }

    public void AddResource(ResourceCategory category, float amount)
    {
        if (!categoryInventory.ContainsKey(category))
        {
            categoryInventory[category] = 0;
        }
        categoryInventory[category] += amount;
        UnityEngine.Debug.Log($"获得资源(按类): {category} + {amount}, 当前: {categoryInventory[category]}");
        OnResourceChanged?.Invoke(); // 通知UI
    }

    // 资源消耗 (比如造船、人口消耗)，返回 true 代表消耗成功，false 代表资源不足
    
    public bool ConsumeResource(string resourceName, float amount)
    {
        // 解析到类别并从类别库存中扣除
        ResourceCategory cat = GetCategoryByResourceName(resourceName);
        return ConsumeResource(cat, amount);
    }

    // 按类别消耗资源
    public bool ConsumeResource(ResourceCategory category, float amount)
    {
        if (!categoryInventory.ContainsKey(category))
        {
            UnityEngine.Debug.LogWarning($"未知资源类别: {category}");
            return false;
        }
        if (categoryInventory[category] >= amount)
        {
            categoryInventory[category] -= amount;
            UnityEngine.Debug.Log($"消耗资源(按类): {category} -{amount}, 剩余: {categoryInventory[category]}");
            OnResourceChanged?.Invoke();
            return true;
        }
        UnityEngine.Debug.LogWarning($"资源不足(按类): {category}");
        return false;
    }

    // 获取特定资源的运行时数据 (用于查看退化程度等)
    public ResourceSlot GetResourceSlot(string resourceName)
    {
        if (inventory.ContainsKey(resourceName)) return inventory[resourceName];
        return null;
    }

    // 在运行时注册一个新的资源（例如变种）。如果已存在则返回 false。
    public bool RegisterResource(ResourceScriptableObject res)
    {
        if (res == null) return false;
        if (knownResources == null) knownResources = new List<ResourceScriptableObject>();
        // 按名称判重
        foreach (var r in knownResources)
        {
            if (r != null && r.resourceName == res.resourceName) return false;
        }
        knownResources.Add(res);
        if (!inventory.ContainsKey(res.resourceName))
        {
            inventory[res.resourceName] = new ResourceSlot(res);
        }
        // 确保类别键存在
        if (!categoryInventory.ContainsKey(res.category)) categoryInventory[res.category] = 0f;
        UnityEngine.Debug.Log($"已注册资源: {res.resourceName}");
        OnResourceChanged?.Invoke();
        return true;
    }

    // 注销运行时注册的资源（尽量仅用于调试/编辑器脚本）
    public bool DeregisterResource(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName)) return false;
        ResourceScriptableObject found = null;
        foreach (var r in knownResources)
        {
            if (r != null && r.resourceName == resourceName) { found = r; break; }
        }
        if (found == null) return false;
        knownResources.Remove(found);
        if (inventory.ContainsKey(resourceName)) inventory.Remove(resourceName);
        UnityEngine.Debug.Log($"已注销资源: {resourceName}");
        OnResourceChanged?.Invoke();
        return true;
    }

    // 根据资源名解析其所属类别（优先查找已知具体资源，其次尝试解析为类别缩写或名称）
    public ResourceCategory GetCategoryByResourceName(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName)) return ResourceCategory.Crop;
        // 1) 按已知资源名查找
        foreach (var res in knownResources)
        {
            if (res.resourceName == resourceName) return res.category;
        }
        UnityEngine.Debug.LogWarning($"无法解析资源名 '{resourceName}' 的类别，默认归为 Crop。");
        return ResourceCategory.Crop;
    }

    // 获取某个类别当前的聚合数量
    public float GetCategoryAmount(ResourceCategory category)
    {
        if (categoryInventory.ContainsKey(category)) return categoryInventory[category];
        return 0;
    }

    // 将指定资源的质量/效率重置为 100%（用于获得新变种或手动重置）
    public void ResetResourceQuality(string resourceName)
    {
        var slot = GetResourceSlot(resourceName);
        if (slot != null)
        {
            slot.ResetQuality();
            OnResourceChanged?.Invoke();
            UnityEngine.Debug.Log($"已重置资源 {resourceName} 的效率到 100%。");
        }
    }

    // 重置所有已知资源为完美效率
    public void ResetAllResourceQualities()
    {
        foreach (var kv in inventory)
        {
            kv.Value.ResetQuality();
        }
        OnResourceChanged?.Invoke();
        UnityEngine.Debug.Log("已重置所有资源效率到 100%。");
    }

    private void OnDestroy()
    {
        // 记得取消订阅，防止内存泄漏
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnNewCycle -= HandleNewCycle;
            GlobalTimeSystem.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    // 处理新周期（比如：每个月结余一次）
    private void HandleNewCycle(int newCycleCount)
    {
        UnityEngine.Debug.Log($"=== 新的一个月开始了 (第 {newCycleCount} 周期) ===");
        // 通知 UI 更新
        OnResourceChanged?.Invoke();
    }

    private void OnPhaseChanged(GlobalTimeSystem.MoonPhase phase)
    {
        monthsSinceLast++;
        if (monthsSinceLast >= Mathf.Max(1, degradationIntervalInMonths))
        {
            monthsSinceLast = 0;
            ApplyDegradationToAllSlots();
        }
    }

    private void ApplyDegradationToAllSlots()
    {
        if (ResourceManager.Instance == null) return;

        // 逐个对已知资源槽调用 Degrade()
        // 这里按配置的间隔月份进行退化（例如每 1 个月触发一次，则退化数月 = degradationIntervalInMonths）
        int monthsToApply = Mathf.Max(1, degradationIntervalInMonths);
        foreach (var res in ResourceManager.Instance.knownResources)
        {
            var slot = ResourceManager.Instance.GetResourceSlot(res.resourceName);
            if (slot != null)
            {
                slot.Degrade();
            }
        }

        // 触发一次资源更新通知：使用 AddResource(+0) 触发 ResourceManager 的 OnResourceChanged
        if (ResourceManager.Instance.knownResources.Count > 0)
        {
            var first = ResourceManager.Instance.knownResources[0];
            ResourceManager.Instance.AddResource(first.resourceName, 0);
        }

        UnityEngine.Debug.Log($"已按间隔 {degradationIntervalInMonths} 个月执行退化。");
    }

}