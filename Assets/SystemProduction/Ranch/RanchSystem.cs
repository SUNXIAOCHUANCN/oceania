using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class RanchSpeciesData
{
    public string speciesName;
    public float amount;
    public float nextPhaseYield;
    
    public RanchSpeciesData(string name, float amt)
    {
        speciesName = name;
        amount = amt;
        nextPhaseYield = 0f; // 将在初始化时设置
    }
    
    public RanchSpeciesData(string name, float amt, float nextYield)
    {
        speciesName = name;
        amount = amt;
        nextPhaseYield = nextYield;
    }
}

public class RanchSystem : MonoBehaviour
{
    [Header("牧场数据库")]
    [SerializeField] private List<RanchSpeciesData> ranchDatabase = new List<RanchSpeciesData>();
    
    [Header("UI引用")]
    [SerializeField] private RanchUIController uiController;
    
    [Header("生产配置")]
    [SerializeField] private SpeciesType validSpeciesType = SpeciesType.Ani; // 参与计算的物种类型
    [SerializeField] private float farmerBonusMultiplier = 1.2f; // 农民管理者加成倍率
    [SerializeField] private float noManagerMultiplier = 1f; // 无管理者时的倍率
    
    [Header("数量限制")]
    private const int MAX_TOTAL_AMOUNT = 15; // 所有物种总数量上限
    private int currentTotalAmount = 0; // 当前总数量
    public float CurrentMonthDecay { get; private set; } // 本月所有动物的总退化量
    
    public PersonScriptableObject Manager { get; private set; }
    public float CurrentMonthProduction { get; private set; }
    public float NextMonthExpectedYield { get; private set; }
    public float CurrentMonthCropConsumption { get; private set; }
    public int CurrentTotalAmount => currentTotalAmount; // 当前总数量（只读）
    public int MaxTotalAmount => MAX_TOTAL_AMOUNT; // 最大总数量（只读）
    
    public UnityEvent<float> OnProductionCalculated = new UnityEvent<float>();
    public UnityEvent<float> OnCropConsumptionCalculated = new UnityEvent<float>();
    public UnityEvent<PersonScriptableObject> OnManagerChanged = new UnityEvent<PersonScriptableObject>();
    public UnityEvent<int> OnTotalAmountChanged = new UnityEvent<int>(); // 总数量变化事件
    
    private void Start()
    {
        Debug.Log($"存档加载前数据库大小: {ranchDatabase.Count}");
        // 尝试加载存档
        if (RanchSaveSystem.Instance.SaveExists())
        {
            Debug.Log("检测到存档文件，正在加载...");
            Debug.Log($"存档加载前数据库大小: {ranchDatabase.Count}");
            RanchSaveSystem.Instance.LoadRanchData(this);
            Debug.Log($"存档加载后数据库大小: {ranchDatabase.Count}"); // 可能为0
            AddAllUnlockedAnimalsToDatabase();
            Debug.Log($"调用AddAllUnlockedAnimalsToDatabase后数据库大小: {ranchDatabase.Count}");
        }
        else
        {
            Debug.Log("未检测到存档文件，初始化空的数据库");
            // 如果没有存档，初始化空的数据库
            InitializeEmptyDatabase();
            
            // 自动添加所有已解锁的动物物种到数据库
            //AddAllUnlockedAnimalsToDatabase();
        }
        
        // 计算下月预计产量
        CalculateNextMonthExpectedYield();
        
        // 更新总数量
        UpdateTotalAmount();
        
        // 尝试自动查找UI控制器
        if (uiController == null)
        {
            uiController = FindObjectOfType<RanchUIController>();
            if (uiController == null)
            {
                Debug.LogWarning("未找到RanchUIController，UI更新功能将不可用");
            }
            else
            {
                Debug.Log("已自动找到RanchUIController");
            }
        }
        
        // 更新UI
        UpdateUI();
        
        // 添加调试信息：显示当前数据库状态
        Debug.Log($"RanchSystem启动完成，当前数据库状态:");
        Debug.Log($"  - 数据库大小: {ranchDatabase.Count}");
        Debug.Log($"  - 当前总数量: {currentTotalAmount}");
        Debug.Log($"  - 最大总数量: {MAX_TOTAL_AMOUNT}");
        
        if (ranchDatabase.Count > 0)
        {
            foreach (var data in ranchDatabase)
            {
                Debug.Log($"  - 物种: {data.speciesName}, 数量: {data.amount}, 下月产量: {data.nextPhaseYield}");
            }
        }
        else
        {
            Debug.Log("  - 数据库为空，没有预加载任何物种");
        }
    }
    
    private void OnEnable()
    {
        // 订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += HandlePhaseChange;
        }
    }
    
    private void OnDisable()
    {
        // 取消订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= HandlePhaseChange;
        }
    }
    
    private void OnApplicationQuit()
    {
        // 游戏退出时保存数据
        RanchSaveSystem.Instance.SaveRanchData(this);
    }
    
    /// <summary>
    /// 自动将所有已解锁的动物物种添加到数据库
    /// </summary>
    private void AddAllUnlockedAnimalsToDatabase()
    {
        if (SpeciesLoader.Instance == null)
        {
            Debug.LogError("SpeciesLoader.Instance 为空，无法加载动物物种");
            return;
        }
        
        var unlockedAnis = SpeciesLoader.Instance.GetUnlockedAniSpecies();
        Debug.Log($"发现 {unlockedAnis.Count} 个解锁的动物物种");
        
        int addedCount = 0;
        foreach (var ani in unlockedAnis)
        {
            // 检查物种是否已在数据库中
            if (!ranchDatabase.Any(s => s.speciesName == ani.speciesName))
            {
                Debug.Log($"正在添加动物物种: {ani.speciesName}");
                // 添加数量为0的物种，仅用于初始化
                AddSpeciesToDatabase(ani, 0f);
                addedCount++;
            }
            else
            {
                Debug.Log($"物种 {ani.speciesName} 已在数据库中，跳过");
            }
        }
        
        Debug.Log($"AddAllUnlockedAnimalsToDatabase 完成，添加了 {addedCount} 个新物种");
    }
    
    /// <summary>
    /// 初始化空的牧场数据库
    /// </summary>
    private void InitializeEmptyDatabase()
    {
        ranchDatabase.Clear();
        Debug.Log("牧场数据库已初始化为空");
    }
    
    /// <summary>
    /// 处理月相变化
    /// </summary>
    private void HandlePhaseChange(GlobalTimeSystem.MoonPhase phase, int phaseCount)
    {
        float totalProduction = 0f;
        float totalCropConsumption = 0f;
        float totalDecay = 0f; // 所有动物的总退化量

        // 计算生产和消耗
        foreach (RanchSpeciesData speciesData in ranchDatabase)
        {
            // 获取物种的完整信息
            SpeciesScriptableObject species = GetSpeciesByName(speciesData.speciesName);
            if (species == null)
            {
                Debug.LogWarning($"找不到物种: {speciesData.speciesName}");
                continue;
            }

            // 保护机制：检查物种类型和解锁状态
            if (species.speciesType != validSpeciesType || !species.unlocked)
            {
                continue;
            }

            // 计算产量：nextPhaseYield * amount
            float speciesProduction = speciesData.nextPhaseYield * speciesData.amount;
            totalProduction += speciesProduction;

            // 计算消耗：monthlyCropConsumption * amount (仅对动物类型)
            if (species.speciesType == SpeciesType.Ani)
            {
                float speciesConsumption = species.monthlyCropConsumption * speciesData.amount;
                totalCropConsumption += speciesConsumption;
            }

            // 作物退化：nextPhaseYield扣除decayPerPhase，但不低于leastYield
            float newNextPhaseYield = speciesData.nextPhaseYield - species.decayPerPhase;
            speciesData.nextPhaseYield = Mathf.Max(newNextPhaseYield, species.leastYield);

            // 累加退化量：decayPerPhase * amount
            totalDecay += species.decayPerPhase * speciesData.amount;
        }

        // 应用管理者加成
        float finalProduction = totalProduction;
        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            finalProduction *= farmerBonusMultiplier;
        }
        else if (Manager == null)
        {
            finalProduction *= noManagerMultiplier;
        }

        // 保存当前月的生产和消耗数据
        CurrentMonthProduction = finalProduction;
        CurrentMonthCropConsumption = totalCropConsumption;
        CurrentMonthDecay = totalDecay; // 保存本月退化量

        // 重新计算下月预计产量
        CalculateNextMonthExpectedYield();

        // 更新UI
        UpdateUI();

        // 触发事件（通知 ResourceManagerCalculator 进行资源修改）
        OnProductionCalculated?.Invoke(finalProduction);
        OnCropConsumptionCalculated?.Invoke(totalCropConsumption);

        Debug.Log($"牧场月相变化处理完成: 产量={finalProduction}, 消耗={totalCropConsumption}");

        // 检查新解锁的物种
        CheckForNewlyUnlockedSpecies();
    }

    /// <summary>
    /// 检查新解锁的动物物种并添加到数据库
    /// </summary>
    private void CheckForNewlyUnlockedSpecies()
    {
        if (SpeciesLoader.Instance == null)
        {
            Debug.LogError("SpeciesLoader.Instance 为空，无法检查新解锁物种");
            return;
        }

        var unlockedAnimals = SpeciesLoader.Instance.GetUnlockedAniSpecies();
        int addedCount = 0;
        foreach (var species in unlockedAnimals)
        {
            // 检查物种是否已在数据库中
            if (!ranchDatabase.Any(s => s.speciesName == species.speciesName))
            {
                Debug.Log($"发现新解锁的动物物种: {species.speciesName}，添加到牧场数据库");
                AddSpeciesToDatabase(species, 0f);
                addedCount++;
            }
        }
        
        if (addedCount > 0)
        {
            Debug.Log($"CheckForNewlyUnlockedSpecies: 添加了 {addedCount} 个新解锁的动物物种");
            // 重新计算预计产量并更新UI
            CalculateNextMonthExpectedYield();
            UpdateUI();
        }
    }
    
    /// <summary>
    /// 计算下个月预计产量
    /// </summary>
    public void CalculateNextMonthExpectedYield()
    {
        NextMonthExpectedYield = 0f;
        
        foreach (RanchSpeciesData speciesData in ranchDatabase)
        {
            SpeciesScriptableObject species = GetSpeciesByName(speciesData.speciesName);
            if (species == null) continue;
            
            // 保护机制：检查物种类型和解锁状态
            if (species.speciesType != validSpeciesType || !species.unlocked)
            {
                continue;
            }
            
            NextMonthExpectedYield += speciesData.nextPhaseYield * speciesData.amount;
        }
        
        // 应用管理者加成
        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            NextMonthExpectedYield *= farmerBonusMultiplier;
        }
        else if (Manager == null)
        {
            NextMonthExpectedYield *= noManagerMultiplier;
        }
    }
    
    /// <summary>
    /// 更新当前总数量
    /// </summary>
    private void UpdateTotalAmount()
    {
        currentTotalAmount = 0;
        foreach (RanchSpeciesData speciesData in ranchDatabase)
        {
            currentTotalAmount += Mathf.RoundToInt(speciesData.amount);
        }
        
        // 触发事件
        OnTotalAmountChanged?.Invoke(currentTotalAmount);
    }
    
    /// <summary>
    /// 通过物种名称获取物种ScriptableObject
    /// </summary>
    private SpeciesScriptableObject GetSpeciesByName(string speciesName)
    {
        if (SpeciesLoader.Instance == null)
        {
            Debug.LogError("SpeciesLoader.Instance is null!");
            return null;
        }
        
        // 从所有物种中查找
        var allSpecies = new List<SpeciesScriptableObject>();
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedCropSpecies());
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedAniSpecies());
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedMatSpecies());
        
        var result = allSpecies.Find(s => s.speciesName == speciesName);
        if (result == null)
        {
            Debug.LogWarning($"在所有物种中未找到名称为 '{speciesName}' 的物种");
        }
        else
        {
            Debug.Log($"成功找到物种: {speciesName}");
        }
        
        return result;
    }
    
    /// <summary>
    /// 设置管理者
    /// </summary>
    public bool SetManager(PersonScriptableObject person)
    {
        if (person == null || !person.recruited || 
            (person.status != PersonStatus.rest && person.status != PersonStatus.inranch))
        {
            return false;
        }
        
        // 如果已有管理者，先移除
        if (Manager != null)
        {
            RemoveManager();
        }
        
        Manager = person;
        // 使用PersonManager统一管理人员状态
        PersonManager.Instance.ChangePersonStatus(person, PersonStatus.inranch);
        
        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        
        OnManagerChanged?.Invoke(person);
        return true;
    }
    
    /// <summary>
    /// 移除管理者
    /// </summary>
    public void RemoveManager()
    {
        if (Manager != null)
        {
            // 使用PersonManager统一管理人员状态
            PersonManager.Instance.ChangePersonStatus(Manager, PersonStatus.rest);
            Manager = null;
            
            // 重新计算预计产量
            CalculateNextMonthExpectedYield();
            UpdateUI();
            
            OnManagerChanged?.Invoke(null);
        }
    }
    
    /// <summary>
    /// 获取所有已招募的人员（用于管理员选择）
    /// </summary>
    public List<PersonScriptableObject> GetAllRecruitedPersons()
    {
        if (PersonManager.Instance == null)
        {
            Debug.LogError("PersonManager.Instance is null!");
            return new List<PersonScriptableObject>();
        }
        
        return PersonManager.Instance.GetAllPersons();
    }
    
    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        // 确保UI控制器存在
        EnsureUIController();
        
        if (uiController != null)
        {
            uiController.UpdateRanchInfo(
                CurrentMonthProduction,
                NextMonthExpectedYield,
                CurrentMonthCropConsumption,
                Manager
            );
        }
    }
    
    /// <summary>
    /// 确保UI控制器已初始化
    /// </summary>
    private void EnsureUIController()
    {
        if (uiController == null)
        {
            uiController = FindObjectOfType<RanchUIController>();
            if (uiController == null)
            {
                Debug.LogWarning("UpdateUI: 未找到RanchUIController，UI更新功能将不可用");
            }
        }
    }
    
    /// <summary>
    /// 向牧场数据库添加物种
    /// </summary>
    public bool AddSpeciesToDatabase(SpeciesScriptableObject species, float amount)
    {
        if (species == null || amount < 0)
        {
            Debug.LogWarning("无效的物种或数量");
            return false;
        }
        
        // 检查总数量上限
        int amountInt = Mathf.RoundToInt(amount);
        if (currentTotalAmount + amountInt > MAX_TOTAL_AMOUNT)
        {
            Debug.LogWarning($"超过总数量上限: 当前{currentTotalAmount}, 尝试添加{amountInt}, 上限{MAX_TOTAL_AMOUNT}");
            return false;
        }
        
        // 检查是否已存在
        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == species.speciesName);
        if (existingData != null)
        {
            // 更新现有数量
            existingData.amount += amount;
            // 如果是第一次添加或nextPhaseYield为0，设置初始值
            if (existingData.nextPhaseYield <= 0)
            {
                existingData.nextPhaseYield = species.initialYield;
            }
        }
        else
        {
            // 创建新数据
            RanchSpeciesData newData = new RanchSpeciesData(species.speciesName, amount, species.initialYield);
            ranchDatabase.Add(newData);
        }
        
        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        UpdateTotalAmount(); // 更新总数量
        
        Debug.Log($"已添加物种到牧场: {species.speciesName} x{amount}");
        Debug.Log($"  - 当前数据库大小: {ranchDatabase.Count}");
        Debug.Log($"  - 当前总数量: {currentTotalAmount}");
        return true;
    }
    
    /// <summary>
    /// 从牧场数据库移除物种
    /// </summary>
    public bool RemoveSpeciesFromDatabase(string speciesName, float amount)
    {
        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == speciesName);
        if (existingData == null)
        {
            Debug.LogWarning($"牧场中找不到物种: {speciesName}");
            return false;
        }
        
        if (existingData.amount < amount)
        {
            Debug.LogWarning($"数量不足: {speciesName} 只有 {existingData.amount}，无法移除 {amount}");
            return false;
        }
        
        existingData.amount -= amount;
        // 注意：即使数量为0，我们也不从数据库中移除该物种（保持只增不减的逻辑）
        
        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        UpdateTotalAmount(); // 更新总数量
        
        Debug.Log($"已从牧场移除物种: {speciesName} x{amount}");
        return true;
    }
    
    /// <summary>
    /// 增加指定物种的数量（+1）
    /// </summary>
    public bool IncrementSpeciesAmount(string speciesName)
    {
        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == speciesName);
        if (existingData == null)
        {
            Debug.LogWarning($"牧场中找不到物种: {speciesName}");
            return false;
        }
        
        // 检查总数量上限
        if (currentTotalAmount + 1 > MAX_TOTAL_AMOUNT)
        {
            Debug.LogWarning($"超过总数量上限: 当前{currentTotalAmount}, 上限{MAX_TOTAL_AMOUNT}");
            return false;
        }
        
        existingData.amount += 1;
        
        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateTotalAmount(); // 更新总数量
        UpdateUI();
        
        Debug.Log($"已增加物种数量: {speciesName} +1, 当前数量: {existingData.amount}");
        return true;
    }
    
    /// <summary>
    /// 减少指定物种的数量（-1）
    /// </summary>
    public bool DecrementSpeciesAmount(string speciesName)
    {
        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == speciesName);
        if (existingData == null)
        {
            Debug.LogWarning($"牧场中找不到物种: {speciesName}");
            return false;
        }
        
        if (existingData.amount < 1)
        {
            Debug.LogWarning($"数量不足: {speciesName} 只有 {existingData.amount}，无法减少");
            return false;
        }
        
        existingData.amount -= 1;
        
        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        UpdateTotalAmount(); // 更新总数量
        
        Debug.Log($"已减少物种数量: {speciesName} -1, 当前数量: {existingData.amount}");
        return true;
    }
    
    /// <summary>
    /// 获取牧场数据库的副本
    /// </summary>
    public List<RanchSpeciesData> GetRanchDatabase()
    {
        Debug.Log($"GetRanchDatabase called, returning {ranchDatabase.Count} items");
        return new List<RanchSpeciesData>(ranchDatabase);
    }
    
    /// <summary>
    /// 从保存数据加载牧场数据库（修复副本问题）
    /// </summary>
    public void LoadRanchDatabaseFromSave(List<RanchSpeciesSaveData> saveDataList)
    {
        // 清空当前数据库
        ranchDatabase.Clear();
        
        // 添加保存的数据
        foreach (RanchSpeciesSaveData speciesData in saveDataList)
        {
            // 通过SpeciesLoader获取原始物种信息，以确保数据完整性
            SpeciesScriptableObject originalSpecies = GetSpeciesByName(speciesData.speciesName);
            if (originalSpecies != null)
            {
                // 使用原始物种的初始值来确保数据一致性
                ranchDatabase.Add(new RanchSpeciesData(speciesData.speciesName, speciesData.amount, speciesData.nextPhaseYield));
            }
            else
            {
                Debug.LogWarning($"无法找到保存的物种: {speciesData.speciesName}，跳过加载");
            }
        }
        
        Debug.Log($"从保存数据加载牧场数据库完成，共 {ranchDatabase.Count} 个项目");
    }
    
    /// <summary>
    /// 获取管理者
    /// </summary>
    public PersonScriptableObject GetManager()
    {
        return Manager;
    }
    
    /// <summary>
    /// 清空牧场数据库（用于测试）
    /// </summary>
    public void ClearDatabase()
    {
        Debug.Log("正在清空牧场数据库...");
        ranchDatabase.Clear();
        CalculateNextMonthExpectedYield();
        UpdateUI();
        Debug.Log("牧场数据库已清空");
    }
}