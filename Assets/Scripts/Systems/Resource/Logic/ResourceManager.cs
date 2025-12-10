using System.Collections.Generic;
using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("初始资源配置")]
    // 在Inspector里把做好的 Yam, Pig, Palm 拖进去，作为初始认识的资源
    public List<ResourceScriptableObject> knownResources;

    // 运行时库存字典：资源名称 -> 运行时数据P
    private Dictionary<string, ResourceSlot> inventory = new Dictionary<string, ResourceSlot>();

    // 事件：当资源变化时通知UI更新 (UI系统订阅这个事件)
    public event Action OnResourceChanged;

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
    }

    // --- 公共方法供其他系统调用 ---

    // 增加资源 (比如收获)
    public void AddResource(string resourceName, int amount)
    {
        if (inventory.ContainsKey(resourceName))
        {
            inventory[resourceName].amount += amount;
            Debug.Log($"获得资源: {resourceName} +{amount}, 当前: {inventory[resourceName].amount}");
            OnResourceChanged?.Invoke(); // 通知UI
        }
    }

    // 消耗资源 (比如造船、人口消耗)
    // 返回 true 代表消耗成功，false 代表资源不足
    public bool ConsumeResource(string resourceName, int amount)
    {
        if (inventory.ContainsKey(resourceName))
        {
            if (inventory[resourceName].amount >= amount)
            {
                inventory[resourceName].amount -= amount;
                Debug.Log($"消耗资源: {resourceName} -{amount}");
                OnResourceChanged?.Invoke();
                return true;
            }
        }
        Debug.LogWarning($"资源不足: {resourceName}");
        return false;
    }

    // 获取特定资源的运行时数据 (用于查看退化程度等)
    public ResourceSlot GetResourceSlot(string resourceName)
    {
        if (inventory.ContainsKey(resourceName)) return inventory[resourceName];
        return null;
    }
}