using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
public class ProductionUnit : MonoBehaviour
{
    [Header("Production Units - assign ScriptableObjects here")]
    public List<ProductionUnitScriptableObject> field = new List<ProductionUnitScriptableObject>();
    public List<ProductionUnitScriptableObject> pasture = new List<ProductionUnitScriptableObject>();
    public List<ProductionUnitScriptableObject> plantation = new List<ProductionUnitScriptableObject>();

    [Tooltip("建造牧场消耗的材料资源（如果留空则不消耗）")]
    public ResourceScriptableObject pastureBuildMaterial;
    [Tooltip("栽培林建造消耗的材料资源（如果留空则不消耗）")]
    public ResourceScriptableObject plantationBuildMaterial;

    // 月时长（秒），用于建造/开垦的 1 个月判定
    private float monthSeconds => GlobalTimeSystem.Instance != null ? GlobalTimeSystem.Instance.phaseDuration * 4f : 120f;

    private ProductionPanel productionPanel;
    private void Awake()
    {
        if (field == null) field = new List<ProductionUnitScriptableObject>();
        if (pasture == null) pasture = new List<ProductionUnitScriptableObject>();
        if (plantation == null) plantation = new List<ProductionUnitScriptableObject>();
    }

    private void Start()
    {
        if (GlobalTimeSystem.Instance == null)
        {
            Debug.LogError("未找到 GlobalTimeSystem! 请先从初始场景启动或将时间系统拖入场。");
            return;
        }

#if UNITY_2023_2_OR_NEWER
        productionPanel = Object.FindFirstObjectByType<ProductionPanel>();
#else
        productionPanel = FindObjectOfType<ProductionPanel>();
#endif
        if (productionPanel == null)
        {
            Debug.LogWarning("未找到 ProductionPanel! 生产系统无法通知 UI 刷新。");
        }
    }

    private void Update()
    {
        if (GlobalTimeSystem.Instance == null) return;
        float now = GlobalTimeSystem.Instance.TotalElapsedTime;

        CheckProductionCompletion(field, now);
        CheckProductionCompletion(pasture, now);
        CheckProductionCompletion(plantation, now);
        // --- 自动检查：若某单元处于建设状态（canProduction==false 且 workerAssignedStart 已设置），
        // 当分配工人数 >= requiredWorkers 且建设时长 >= 1 个月时，完成建设并允许生产。
        System.Action<List<ProductionUnitScriptableObject>> checkConstruction = (list) =>
        {
            for (int i = 0; i < list.Count; i++)
            {
                var unit = list[i];
                if (unit == null) continue;
                if (unit.canProduction) continue; // 已可生产
                float start = unit.workerAssignedStart;
                if (start < 0f) continue; // 无建设开始时间
                if (unit.Worker != null)
                {
                    if (now - start >= monthSeconds)
                    {
                        unit.canProduction = true;
                        Debug.Log($"[{GlobalTimeSystem.Instance.GetFormattedTotalTime()}] 单元建设完成并允许生产。");
                        unit.Worker.currentStatus = Person.Status.Free;
                        unit.Worker = null;
                    }
                }
            }
        };

        checkConstruction(field);
        checkConstruction(pasture);
        checkConstruction(plantation);

        // 田地队列
        for (int i = 0; i < field.Count; i++)
        {
            var unit = field[i];
            if (unit == null) continue;
            if (!unit.canProduction) continue;
            var crop = unit.Resource;
            if (crop == null) continue;

            float plantStart = unit.ProductionStart;
            if (plantStart < 0f)
            {
                unit.ProductionStart = now;
                plantStart = now;
            }
            
            float progress = now - plantStart;
            if (progress >= crop.growthTime)
            {
                var slot = ResourceManager.Instance.GetResourceSlot(crop.resourceName);
                if (slot != null)
                {
                    float yield = slot.GetCurrentYield();
                    if (unit.hasFarmer) yield *= 1.2f;
                    ResourceManager.Instance.AddResource(crop.resourceName, yield);
                    Debug.Log($"[{GlobalTimeSystem.Instance.GetFormattedTotalTime()}] 田地单元 {i} 收获: {crop.resourceName} (+{yield})");
                }

                unit.ProductionStart = -1f;
                
                if(unit.autoReplant && crop != null)
                {
                    unit.ProductionStart = now;
                }
            }
        }

        // 牧场队列
        for (int i = 0; i < pasture.Count; i++)
        {
            var unit = pasture[i];
            if (unit == null) continue;
            if (!unit.canProduction) continue;
            var livestock = unit.Resource;
            if (livestock == null) continue;
            // 判断牧场单元是否有管理员存在
            if (unit.Manager == null) continue;    

            float lastProduce = unit.lastProduce;
            if (lastProduce < 0f) 
            { 
                unit.lastProduce = now; 
                lastProduce = now; 
            }
            
            float since = now - lastProduce;
            if (since >= livestock.growthTime)
            {
                var slot = ResourceManager.Instance.GetResourceSlot(livestock.resourceName);
                float feedNeed = 0;
                float quality = 1f;
                if (slot != null) quality = slot.qualityMultiplier;
                if (livestock.baseConsumption > 0)
                {
                    feedNeed = livestock.baseConsumption / quality;
                }

                bool hasFeed = true;
                if (feedNeed > 0) hasFeed = ResourceManager.Instance.ConsumeResource(ResourceCategory.Crop, feedNeed);
                if (!hasFeed)
                {
                    Debug.LogWarning($"牧场单元 {i}：饲料不足，跳过本次产出（需要 {feedNeed} 作物）");
                }
                else
                {
                    float yield = slot.GetCurrentYield();
                    if (unit.hasFarmer) yield *= 1.2f;
                    ResourceManager.Instance.AddResource(livestock.resourceName, yield);
                    Debug.Log($"[{GlobalTimeSystem.Instance.GetFormattedTotalTime()}] 牧场单元产出: {livestock.resourceName} (+{yield}) 消耗饲料 {feedNeed}");
                }

                unit.lastProduce = now;
            }
        }

        // 栽培林队列（按月产出）
        for (int i = 0; i < plantation.Count; i++)
        {
            var unit = plantation[i];
            if (unit == null) continue;
            if (!unit.canProduction) continue;
            var material = unit.Resource;
            if (material == null) continue;

            float lastYield = unit.lastYield;
            if (lastYield < 0f)
            {
                unit.lastYield = GlobalTimeSystem.Instance.TotalElapsedTime;
                lastYield = GlobalTimeSystem.Instance.TotalElapsedTime;
            }

            float sinceLast = now - lastYield;
            if (sinceLast >= monthSeconds)
            {
                bool matured = true;
                if (now - unit.ProductionStart < material.growthTime) matured = false;
                ResourceSlot slot = ResourceManager.Instance.GetResourceSlot(material.resourceName);
                if (slot != null)
                {
                    float yield = slot.GetCurrentYield();
                    if (!matured) yield *= 0.5f; 
                    if (unit.hasFarmer) yield *= 1.2f;
                    ResourceManager.Instance.AddResource(material.resourceName, yield);
                    Debug.Log($"[{GlobalTimeSystem.Instance.GetFormattedTotalTime()}] 栽培林单元产出: {material.resourceName} (+{yield}) {(matured?"成熟":"生长期")} ");
                }
                unit.lastYield = now;
            }
        }
    }

    // === 操作接口：对三类生产单元队列进行配置 ===

    // 设定田地单元的作物
    public bool PlantCropInField(int index, ResourceScriptableObject crop, bool autoReplant = true)
    {
        if (index < 0 || index >= field.Count) return false;
        var unit = field[index];
        if (unit == null) return false;
        if (!unit.canProduction) 
        {
            Debug.Log($"当前生产单元 {unit.unitID} 无法进行生产！");
            return false;
        }
        unit.Resource = crop;
        unit.autoReplant = autoReplant;
        unit.ProductionStart = -1f;
        Debug.Log($"当前生产单元 {unit.unitID} 已开始种植作物 {crop.resourceName}。");
        return true;
    }

    // 为要建设的田地单元分配工人
    public bool AssignWorkerToFieldUnit(int index, Person worker)
    {
        if (index < 0 || index >= field.Count) return false;
        var unit = field[index];
        if (unit == null) return false;
        if (unit.canProduction) return false;
        unit.Worker = worker;
        return true;
    }

    // 开始开垦田地（标记为开始时间）
    public bool StartBuildField(int index)
    {
        if (index < 0 || index >= field.Count) return false;
        var unit = field[index];
        if (unit == null) return false;
        if (unit.canProduction) return false;
        if (unit.Worker == null) return false;
        unit.workerAssignedStart  = GlobalTimeSystem.Instance.TotalElapsedTime;
        unit.Worker.currentStatus = Person.Status.Working;
        return true;
    }

    // 为田地单元设置管理人员
    public void SetFieldManager(int index, Person manager)
    {
        if (index < 0 || index >= field.Count) return;
        var unit = field[index];
        if (unit == null) return;
        unit.Manager = manager;

        if (manager.role.roleName == "农民")
        {
            unit.hasFarmer = true;
        }

        manager.currentStatus = Person.Status.Managing;
    }

    // 为田地单元撤销管理人员
    public void RemoveFieldManager(int index)
    {
        if (index < 0 || index >= field.Count) return;
        var unit = field[index];
        if (unit == null) return;
        unit.Manager.currentStatus = Person.Status.Free;
        unit.Manager = null;
        unit.hasFarmer = false;
    }

    // 设定牧场单元的牲畜
    public bool AssignLivestockToPasture(int index, ResourceScriptableObject livestock)
    {
        if (index < 0 || index >= pasture.Count) return false;
        var unit = pasture[index];
        if (unit == null) return false;
        unit.Resource = livestock;
        unit.ProductionStart = GlobalTimeSystem.Instance.TotalElapsedTime;
        return true;
    }

    // 为要建设的牧场单元分配工人
    public bool AssignWorkerToPastureUnit(int index, Person worker)
    {
        if (index < 0 || index >= pasture.Count) return false;
        var unit = pasture[index];
        if (unit == null) return false;
        if (unit.canProduction) return false;
        unit.Worker = worker;
        return true;
    }

    // 开始建造牧场（消耗材料并标记为开始时间）
    public bool StartBuildPasture(int index)
    {
        if (index < 0 || index >= pasture.Count) return false;
        var unit = pasture[index];
        if (unit == null) return false;
        if (unit.canProduction) return false;
        if (unit.Worker == null) return false;
        if (pastureBuildMaterial != null)
        {
            if (!ResourceManager.Instance.ConsumeResource(pastureBuildMaterial.resourceName, unit.requiredMaterial))
            {
                Debug.LogWarning("建造牧场需要材料不足");
                return false;
            }
        }
        // 标记为正在建设（设置 ProductionStart，实际完成由分配工人 + 1 个月触发）
        unit.workerAssignedStart = GlobalTimeSystem.Instance.TotalElapsedTime;
        unit.Worker.currentStatus = Person.Status.Working;
        return true;
    }

    // 为牧场单元设置管理人员
    public void SetPastureManager(int index, Person manager)
    {
        if (index < 0 || index >= pasture.Count) return;
        var unit = pasture[index];
        if (unit == null) return;
        unit.Manager = manager;

        if (manager.role.roleName == "农民")
        {
            unit.hasFarmer = true;
        }

        manager.currentStatus = Person.Status.Managing;
    }

    // 为牧场单元撤销管理人员
    public void RemovePastureManager(int index)
    {
        if (index < 0 || index >= pasture.Count) return;
        var unit = pasture[index];
        if (unit == null) return;
        unit.Manager.currentStatus = Person.Status.Free;
        unit.Manager = null;
        unit.hasFarmer = false;
    }

    // 设定栽培林单元的材料
    public bool ProduceMaterialInPlantation(int index, ResourceScriptableObject material)
    {
        if (index < 0 || index >= plantation.Count) return false;
        var unit = plantation[index];
        if (unit == null) return false;
        if (!unit.canProduction) return false;
        unit.Resource = material;
        unit.ProductionStart = GlobalTimeSystem.Instance.TotalElapsedTime;
        return true;
    }

    // 为要建设的栽培林单元分配工人
    public bool AssignWorkerToPlantationUnit(int index, Person worker)
    {
        if (index < 0 || index >= plantation.Count) return false;
        var unit = plantation[index];
        if (unit == null) return false;
        if (unit.canProduction) return false;
        unit.Worker = worker;
        return true;
    }

    // 开始种植栽培林（消耗材料并标记为开始时间）
    public bool StartBuildPlantation(int index)
    {
        if (index < 0 || index >= plantation.Count) return false;
        var unit = plantation[index];
        if (unit == null) return false;
        if (unit.canProduction) return false;
        if (unit.Worker == null) return false;
        if (plantationBuildMaterial != null)
        {
            if (!ResourceManager.Instance.ConsumeResource(plantationBuildMaterial.resourceName, unit.requiredMaterial))
            {
                Debug.LogWarning("种植栽培林所需材料不足");
                return false;
            }
        }
        unit.workerAssignedStart = GlobalTimeSystem.Instance.TotalElapsedTime;
        unit.Worker.currentStatus = Person.Status.Working;
        return true;
    }

    // 为栽培林单元设置管理人员
    public void SetPlantationManager(int index, Person manager)
    {
        if (index < 0 || index >= plantation.Count) return;
        var unit = plantation[index];
        if (unit == null) return;
        unit.Manager = manager;

        if (manager.role.roleName == "农民")
        {
            unit.hasFarmer = true;
        }

        manager.currentStatus = Person.Status.Managing;
    }

    // 为栽培林单元撤销管理人员
    public void RemovePlantationFarmer(int index)
    {
        if (index < 0 || index >= plantation.Count) return;
        var unit = plantation[index];
        if (unit == null) return;
        unit.Manager.currentStatus = Person.Status.Free;
        unit.Manager = null;
        unit.hasFarmer = false;
    }

    private void CheckProductionCompletion(List<ProductionUnitScriptableObject> units, float currentTime)
    {
        bool needRefresh = false;
    
        // 从后往前遍历列表，以防未来需要移除元素
        for(int i = units.Count - 1; i >= 0; i--)
        {
            var unit = units[i];
        
            // 确保单元在生产中
            if (unit == null || unit.Resource == null || unit.ProductionStart <= 0f) continue;

            float elapsedTime = currentTime - unit.ProductionStart;
            float growthTime = unit.Resource.growthTime;

            if (elapsedTime >= growthTime)
            {
                // --- 生产完成结算逻辑 ---
            
                // 1. 产出资源 (假设 ResourceManager.Instance 存在且产出逻辑放在这里)
                if (ResourceManager.Instance != null)
                {
                    // TODO: 在此处实现实际的资源增加逻辑
                    Debug.Log($"[{unit.category}] {unit.unitID} 生产完成，产出 {unit.Resource.resourceName}");
                }

                if (unit.autoReplant) {
                
                    // 自动生产: 立即重置时间，开始新一轮生产
                    unit.ProductionStart = currentTime; 
                    // 人员保持不变，资源保持不变
                
                    // 仅需确保 Manager/Worker 状态是 Working/Managing
                    if (unit.Manager != null) unit.Manager.currentStatus = Person.Status.Working;
                    if (unit.Worker != null) unit.Worker.currentStatus = Person.Status.Working;
                
                    needRefresh = true; // 状态变化，通知 UI
                }
                else
                {
                    // **解决问题 1 & 2：非自动生产，进行状态清理，解除锁定**
                
                    // 2. 清除数据
                    unit.Resource = null;             // 清除资源
                    unit.ProductionStart = 0f;        // 进度归零
                
                    // 3. 释放 Manager 和 Worker
                    if (unit.Manager != null) 
                    {
                        unit.Manager.currentStatus = Person.Status.Free;
                        unit.Manager = null;
                    }
                    if (unit.Worker != null) 
                    {
                        unit.Worker.currentStatus = Person.Status.Free;
                        unit.Worker = null;
                    }
                
                    needRefresh = true; // 状态变化，通知 UI
                }
            }
        }
    // 如果有任何单元状态发生变化，通知 UI 刷新
        if (needRefresh && productionPanel != null)
        {
            productionPanel.RefreshSelectedList(); 
        }
    }

    public bool StartConstruction(ProductionUnitScriptableObject unit, ProductionPanel.ViewCategory category)
    {
        if (unit == null) return false;
        switch (category)
        {
            case ProductionPanel.ViewCategory.Field:
                int index = 0;
                for(int i = 0; i < field.Count; i++)
                {
                    if (field[i] == unit)
                    {
                        index = i;
                        break;
                    }
                }
                StartBuildField(index);
                break;
            case ProductionPanel.ViewCategory.Pasture:
                index = 0;
                for(int i = 0; i < pasture.Count; i++)
                {
                    if (pasture[i] == unit)
                    {
                        index = i;
                        break;
                    }
                }
                StartBuildPasture(index);
                break;
            case ProductionPanel.ViewCategory.Plantation:
                index = 0;
                for(int i = 0; i < plantation.Count; i++)
                {
                    if (plantation[i] == unit)
                    {
                        index = i;
                        break;
                    }
                }
                StartBuildPlantation(index);
                break;
        }
        return true;
    }

    

    // 查询方法
    public int GetClearedFieldCount() { int c = 0; foreach (var u in field) if (u != null && u.canProduction) c++; return c; }
    public int GetBuiltPastureCount() { int c = 0; foreach (var u in pasture) if (u != null && u.canProduction) c++; return c; }
    public int GetPlantedPlantationCount() { int c = 0; foreach (var u in plantation) if (u != null && u.canProduction) c++; return c; }


}