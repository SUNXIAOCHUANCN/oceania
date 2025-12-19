using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance { get; private set; }

    [Header("配置")]
    public List<RoleScriptableObject> availableRoles; // 所有可招募的角色模板

    [Header("状态 - 运行时人口")]
    // 追踪所有人口单位
    public List<Person> allPeople = new List<Person>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        if (GlobalTimeSystem.Instance != null)
        {
            // 订阅时间系统事件，用于周期性消耗资源
            GlobalTimeSystem.Instance.OnNewCycle += OnNewCycle_ConsumeResources;
        }

        // --- 初级目标实现：初始化固定人口 ---
        if (allPeople.Count == 0 && availableRoles.Count > 0)
        {
            // 随便初始化 3 个不同职业的人口
            AddPerson(availableRoles[0], "Akira"); // 驯化师
            AddPerson(availableRoles[1], "Kaia");  // 水手
            AddPerson(availableRoles[2], "Zola");  // 巫师
            AddPerson(availableRoles[4], "Rin");   // 观星者
        }
    }

    private void OnDestroy()
    {
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnNewCycle -= OnNewCycle_ConsumeResources;
        }
    }

    // --- 核心方法：人口管理 ---

    // 主动招募新的人口
    public int RecruitNewPerson(RoleScriptableObject role, int recruitCount)
    {
        if (role == null || recruitCount <= 0) return 0;

        // 1. 检查资源是否足够 (需要与 ResourceManager 交互)
        if (!CanAffordRecruitment(role, recruitCount))
        {
            Debug.LogWarning($"资源不足，无法招募 {recruitCount} 个 {role.roleName}。请检查资源！");
            return 0;
        }

        // 2. 消耗复合资源
        if (!ConsumeRecruitmentCost(role, recruitCount))
        {
            // 如果资源检查通过了，但消耗失败，通常是逻辑错误
            Debug.LogError($"资源消耗失败，但之前检查通过了！");
            return 0;
        }

        // 3. 循环招募并添加到人口列表
        int successful = 0;
        for (int i = 0; i < recruitCount; i++)
        {
            string newName = GetRandomName(); // 假设你有一个获取随机名字的方法
            AddPerson(role, newName);
            successful++;
        }

        Debug.Log($"成功招募 {successful} 个 {role.roleName}。");
        return successful;
    }

    // 检查需要用于招募的资源是否足够
    private bool CanAffordRecruitment(RoleScriptableObject role, int count)
    {
        // 假设 ResourceManager 已经挂载并初始化
        if (ResourceManager.Instance == null) 
        {
            Debug.LogWarning("ResourceManager 实例未找到，无法进行资源交互！");
            return false;
        }
        // 聚合每种资源的总需求量（以防相同资源在列表中多次出现）
        Dictionary<string, int> needed = new Dictionary<string, int>();
        foreach (var cost in role.RecruitmentCosts)
        {
            int total = cost.amount * count;
            if (needed.ContainsKey(cost.resourceName)) needed[cost.resourceName] += total;
            else needed.Add(cost.resourceName, total);
        }

        foreach (var kv in needed)
        {
            var slot = ResourceManager.Instance.GetResourceSlot(kv.Key);
            float have = slot != null ? slot.amount : 0;
            if (slot == null || have < kv.Value)
            {
                Debug.Log($"招募失败！招募 {count} 名 {role.roleName} 需要 {kv.Key} 数量为：{kv.Value} ，您当前只有 {have})");
                return false;
            }
        }
        return true;
    }

    // 消耗招募人口所需要的资源
    private bool ConsumeRecruitmentCost(RoleScriptableObject role, int count)
    {
        if (ResourceManager.Instance == null) return false;
        // 聚合后一次性消耗，避免重复对同一资源多次调用
        Dictionary<string, int> toConsume = new Dictionary<string, int>();
        foreach (var cost in role.RecruitmentCosts)
        {
            int total = cost.amount * count;
            if (toConsume.ContainsKey(cost.resourceName)) toConsume[cost.resourceName] += total;
            else toConsume.Add(cost.resourceName, total);
        }

        bool ok = true;
        foreach (var kv in toConsume)
        {
            if (!ResourceManager.Instance.ConsumeResource(kv.Key, kv.Value))
            {
                Debug.LogError($"招募失败！消耗资源 {kv.Key} 数量 {kv.Value} 失败！");
                ok = false;
            }
        }
        return ok;
    }

    private string GetRandomName()
    {
        return "Unit_" + allPeople.Count;
    }

    private void AddPerson(RoleScriptableObject role, string name)
    {
        allPeople.Add(new Person(role, name));
        role.count += 1;
        // 可以在这里触发事件通知 UI 更新人口列表
    }

    // 查找特定职业的人口 (例如：出海时需要找驯化师)
    public List<Person> GetPeopleByRole(RoleScriptableObject role)
    {
        return allPeople.Where(p => p.role == role).ToList();
    }

    // --- 周期性消耗逻辑，每个周期都需要消耗与人口相关的资源 ---
    private void OnNewCycle_ConsumeResources(int cycleCount)
    {
        Debug.Log($"--- 开始 {cycleCount} 周期人口资源消耗 ---");

        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning("ResourceManager 实例未找到，无法进行人口消耗。");
            return;
        }

        // 计算在岛上的人数（出海人员不消耗岛上资源）
        int islandPeople = 0;
        foreach (var p in allPeople)
        {
            if (p.currentStatus != Person.Status.Sailing) islandPeople++;
        }

        // 基础固定消耗（每人在文档中的消耗）：pc:3, pl:2, pm:1
        int needCropTotal = 3 * islandPeople;
        int needLivestockTotal = 2 * islandPeople;
        int needMaterialTotal = 1 * islandPeople;

        // 聚合职业额外的周期性消耗为按类别的需求
        Dictionary<ResourceCategory, int> namedNeedsByCategory = new Dictionary<ResourceCategory, int>();
        foreach (var role in availableRoles)
        {
            int rcnt = Mathf.Max(0, role.count);
            if (rcnt == 0 || role.durationalConsumption == null) continue;
            foreach (var rc in role.durationalConsumption)
            {
                ResourceCategory cat = ResourceManager.Instance.GetCategoryByResourceName(rc.resourceName);
                int need = rc.amount * rcnt;
                if (namedNeedsByCategory.ContainsKey(cat)) namedNeedsByCategory[cat] += need;
                else namedNeedsByCategory.Add(cat, need);
            }
        }

        // 计算每个类别的总需求 = 基础需求 + 职业额外需求
        int totalNeedCrop = needCropTotal + (namedNeedsByCategory.ContainsKey(ResourceCategory.Crop) ? namedNeedsByCategory[ResourceCategory.Crop] : 0);
        int totalNeedLivestock = needLivestockTotal + (namedNeedsByCategory.ContainsKey(ResourceCategory.Livestock) ? namedNeedsByCategory[ResourceCategory.Livestock] : 0);
        int totalNeedMaterial = needMaterialTotal + (namedNeedsByCategory.ContainsKey(ResourceCategory.Material) ? namedNeedsByCategory[ResourceCategory.Material] : 0);

        // 检查每个类别是否足够
        bool okAll = true;
        if (ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Crop) < totalNeedCrop)
        {
            Debug.LogError($"周期人口作物类资源不足！需要 {totalNeedCrop}，可用 {ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Crop)}");
            okAll = false;
        }
        if (ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Livestock) < totalNeedLivestock)
        {
            Debug.LogError($"周期人口牲畜类资源不足！需要 {totalNeedLivestock}，可用 {ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Livestock)}");
            okAll = false;
        }
        if (ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Material) < totalNeedMaterial)
        {
            Debug.LogError($"周期人口材料类资源不足！需要 {totalNeedMaterial}，可用 {ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Material)}");
            okAll = false;
        }

        if (!okAll)
        {
            Debug.LogWarning("资源不足以满足本周期全部人口消耗，暂不扣除资源。可实现部分配给或后果逻辑。");
            return;
        }

        // 扣除职业额外需求（按类别）优先
        foreach (var kv in namedNeedsByCategory)
        {
            bool ok = ResourceManager.Instance.ConsumeResource(kv.Key, kv.Value);
            if (ok) Debug.Log($"扣除职业额外消耗 {kv.Key} 数量 {kv.Value}");
            else Debug.LogError($"扣除职业额外消耗失败 {kv.Key} 数量 {kv.Value}");
        }

        // 扣除基础类别需求
        if (needCropTotal > 0) ResourceManager.Instance.ConsumeResource(ResourceCategory.Crop, needCropTotal);
        if (needLivestockTotal > 0) ResourceManager.Instance.ConsumeResource(ResourceCategory.Livestock, needLivestockTotal);
        if (needMaterialTotal > 0) ResourceManager.Instance.ConsumeResource(ResourceCategory.Material, needMaterialTotal);

        Debug.Log($"周期人口消耗完成：人数(岛上)={islandPeople}，pc={needCropTotal}，pl={needLivestockTotal}，pm={needMaterialTotal}");
    }
}