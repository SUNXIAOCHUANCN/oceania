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

    private void Awake()
    {
        // 确保无管理者倍率不会为0（修复可能的Inspector设置错误）
        if (noManagerMultiplier == 0f)
        {
            noManagerMultiplier = 1f;
            DebugTool.LogRanch("Awake() 中修正 noManagerMultiplier 从 0 改为 1f");
        }
    }

    private void Start()
    {
        DebugTool.LogRanch("Start() 开始执行");

        // 确保订阅月相变化事件（作为 OnEnable 的备用）
        // 先取消订阅以防止重复订阅
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= HandlePhaseChange;
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += HandlePhaseChange;
            DebugTool.LogRanch("在 Start 中成功订阅月相变化事件");
        }
        else
        {
            DebugTool.LogError("RanchSystem", "Start 时 GlobalTimeSystem.Instance 仍为 null，无法订阅月相变化事件！");
        }

        DebugTool.LogRanch("存档加载前数据库大小: {0}", ranchDatabase.Count);

        // 尝试加载存档
        if (RanchSaveSystem.Instance.SaveExists())
        {
            DebugTool.LogRanch("检测到存档文件，正在加载...");
            RanchSaveSystem.Instance.LoadRanchData(this);
            DebugTool.LogRanch("存档加载后数据库大小: {0}", ranchDatabase.Count);
            AddAllUnlockedAnimalsToDatabase();
            DebugTool.LogRanch("调用AddAllUnlockedAnimalsToDatabase后数据库大小: {0}", ranchDatabase.Count);
        }
        else
        {
            DebugTool.LogRanch("未检测到存档文件，初始化空的数据库");
            // 如果没有存档，初始化空的数据库
            InitializeEmptyDatabase();
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
                DebugTool.LogWarning("RanchSystem", "未找到RanchUIController，UI更新功能将不可用");
            }
            else
            {
                DebugTool.LogRanch("已自动找到RanchUIController");
            }
        }

        // 更新UI
        UpdateUI();

        // 添加调试信息：显示当前数据库状态
        DebugTool.LogRanch("RanchSystem启动完成，当前数据库状态:");
        DebugTool.LogRanch("  - 数据库大小: {0}", ranchDatabase.Count);
        DebugTool.LogRanch("  - 当前总数量: {0}", currentTotalAmount);
        DebugTool.LogRanch("  - 最大总数量: {0}", MAX_TOTAL_AMOUNT);

        if (ranchDatabase.Count > 0)
        {
            foreach (var data in ranchDatabase)
            {
                DebugTool.LogRanch("  - 物种: {0}, 数量: {1}, 下月产量: {2}", data.speciesName, data.amount, data.nextPhaseYield);
            }
        }
        else
        {
            DebugTool.LogRanch("  - 数据库为空，没有预加载任何物种");
        }

        DebugTool.LogRanch("Start() 完成");
    }

    private void OnEnable()
    {
        DebugTool.LogRanch("OnEnable() 开始执行");

        // 订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += HandlePhaseChange;
            DebugTool.LogRanch("成功订阅月相变化事件");
        }
        else
        {
            DebugTool.LogWarning("RanchSystem", "OnEnable时 GlobalTimeSystem.Instance 为 null，将在 Start 中重新尝试订阅");
        }

        DebugTool.LogRanch("OnEnable() 完成");
    }

    private void OnDisable()
    {
        DebugTool.LogRanch("OnDisable() 开始执行");

        // 取消订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= HandlePhaseChange;
            DebugTool.LogRanch("成功取消订阅月相变化事件");
        }

        DebugTool.LogRanch("OnDisable() 完成");
    }

    private void OnApplicationQuit()
    {
        DebugTool.LogRanch("OnApplicationQuit() 开始执行");
        // 游戏退出时保存数据
        RanchSaveSystem.Instance.SaveRanchData(this);
        DebugTool.LogRanch("OnApplicationQuit() 完成");
    }

    /// <summary>
    /// 自动将所有已解锁的动物物种添加到数据库
    /// </summary>
    private void AddAllUnlockedAnimalsToDatabase()
    {
        DebugTool.LogRanch("AddAllUnlockedAnimalsToDatabase() 开始执行");

        if (SpeciesLoader.Instance == null)
        {
            DebugTool.LogError("RanchSystem", "SpeciesLoader.Instance 为空，无法加载动物物种");
            return;
        }

        var unlockedAnis = SpeciesLoader.Instance.GetUnlockedAniSpecies();
        DebugTool.LogRanch("发现 {0} 个解锁的动物物种", unlockedAnis.Count);

        int addedCount = 0;
        foreach (var ani in unlockedAnis)
        {
            // 检查物种是否已在数据库中
            if (!ranchDatabase.Any(s => s.speciesName == ani.speciesName))
            {
                DebugTool.LogRanch("正在添加动物物种: {0}", ani.speciesName);
                // 添加数量为0的物种，仅用于初始化
                AddSpeciesToDatabase(ani, 0f);
                addedCount++;
            }
            else
            {
                DebugTool.LogRanch("物种 {0} 已在数据库中，跳过", ani.speciesName);
            }
        }

        DebugTool.LogRanch("AddAllUnlockedAnimalsToDatabase 完成，添加了 {0} 个新物种", addedCount);
    }

    /// <summary>
    /// 初始化空的牧场数据库
    /// </summary>
    private void InitializeEmptyDatabase()
    {
        DebugTool.LogRanch("InitializeEmptyDatabase() 开始执行");
        ranchDatabase.Clear();
        DebugTool.LogRanch("牧场数据库已初始化为空");
    }

    /// <summary>
    /// 处理月相变化
    /// </summary>
    private void HandlePhaseChange(GlobalTimeSystem.MoonPhase phase, int phaseCount)
    {
        DebugTool.LogRanch("========================================");
        DebugTool.LogRanch("HandlePhaseChange() 开始执行，月相: {0}, 阶段数: {1}", phase, phaseCount);

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
                DebugTool.LogWarning("RanchSystem", "找不到物种: {0}", speciesData.speciesName);
                continue;
            }

            // 保护机制：检查物种类型和解锁状态
            if (species.speciesType != validSpeciesType || !species.unlocked)
            {
                continue;
            }

            // 计算产量：nextPhaseYield * amount
            float speciesProduction = speciesData.nextPhaseYield * speciesData.amount;
            DebugTool.LogRanch("物种 {0}: nextPhaseYield={1}, amount={2}, production={3}",
                speciesData.speciesName, speciesData.nextPhaseYield, speciesData.amount, speciesProduction);
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

        DebugTool.LogRanch("循环完成: totalProduction={0}, totalCropConsumption={1}, totalDecay={2}",
            totalProduction, totalCropConsumption, totalDecay);

        // 应用管理者加成
        float finalProduction = totalProduction;
        DebugTool.LogRanch("应用加成前: finalProduction={0}, Manager={1}", finalProduction, Manager?.personName ?? "无");

        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            finalProduction *= farmerBonusMultiplier;
            DebugTool.LogRanch("应用农民加成: finalProduction={0} (倍率={1})", finalProduction, farmerBonusMultiplier);
        }
        else if (Manager == null)
        {
            finalProduction *= noManagerMultiplier;
            DebugTool.LogRanch("无管理者加成: finalProduction={0} (倍率={1})", finalProduction, noManagerMultiplier);
        }
        else
        {
            DebugTool.LogRanch("管理者非农民: {0}, profession={1}, finalProduction={2}",
                Manager.personName, Manager.profession, finalProduction);
        }

        // 保存当前月的生产和消耗数据
        CurrentMonthProduction = finalProduction;
        CurrentMonthCropConsumption = totalCropConsumption;
        CurrentMonthDecay = totalDecay; // 保存本月退化量
        DebugTool.LogRanch("CurrentMonthProduction 已设置: {0}", CurrentMonthProduction);

        // 重新计算下月预计产量
        CalculateNextMonthExpectedYield();

        // 更新UI
        UpdateUI();

        // 触发事件（通知 ResourceManagerCalculator 进行资源修改）
        DebugTool.LogRanch("准备触发事件: OnProductionCalculated({0})", finalProduction);
        OnProductionCalculated?.Invoke(finalProduction);
        DebugTool.LogRanch("事件已触发");
        OnCropConsumptionCalculated?.Invoke(totalCropConsumption);

        DebugTool.LogRanch("牧场月相变化处理完成: 产量={0}, 消耗={1}", finalProduction, totalCropConsumption);

        // 检查新解锁的物种
        CheckForNewlyUnlockedSpecies();

        DebugTool.LogRanch("HandlePhaseChange() 完成");
    }

    /// <summary>
    /// 检查新解锁的动物物种并添加到数据库
    /// </summary>
    private void CheckForNewlyUnlockedSpecies()
    {
        DebugTool.LogRanch("CheckForNewlyUnlockedSpecies() 开始执行");

        if (SpeciesLoader.Instance == null)
        {
            DebugTool.LogError("RanchSystem", "SpeciesLoader.Instance 为空，无法检查新解锁物种");
            return;
        }

        var unlockedAnimals = SpeciesLoader.Instance.GetUnlockedAniSpecies();
        int addedCount = 0;
        foreach (var species in unlockedAnimals)
        {
            // 检查物种是否已在数据库中
            if (!ranchDatabase.Any(s => s.speciesName == species.speciesName))
            {
                DebugTool.LogRanch("发现新解锁的动物物种: {0}，添加到牧场数据库", species.speciesName);
                AddSpeciesToDatabase(species, 0f);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            DebugTool.LogRanch("CheckForNewlyUnlockedSpecies: 添加了 {0} 个新解锁的动物物种", addedCount);
            // 重新计算预计产量并更新UI
            CalculateNextMonthExpectedYield();
            UpdateUI();
        }

        DebugTool.LogRanch("CheckForNewlyUnlockedSpecies() 完成");
    }

    /// <summary>
    /// 计算下个月预计产量
    /// </summary>
    public void CalculateNextMonthExpectedYield()
    {
        DebugTool.LogRanch("CalculateNextMonthExpectedYield() 开始执行");

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

        DebugTool.LogRanch("下月原始预计产量: {0}", NextMonthExpectedYield);

        // 应用管理者加成
        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            NextMonthExpectedYield *= farmerBonusMultiplier;
            DebugTool.LogRanch("应用农民管理者加成: {0} (倍率={1})", NextMonthExpectedYield, farmerBonusMultiplier);
        }
        else if (Manager == null)
        {
            NextMonthExpectedYield *= noManagerMultiplier;
            DebugTool.LogRanch("无管理者加成: {0} (倍率={1})", NextMonthExpectedYield, noManagerMultiplier);
        }

        DebugTool.LogRanch("CalculateNextMonthExpectedYield() 完成，最终预计产量: {0}", NextMonthExpectedYield);
    }

    /// <summary>
    /// 更新当前总数量
    /// </summary>
    private void UpdateTotalAmount()
    {
        DebugTool.LogRanch("UpdateTotalAmount() 开始执行");

        currentTotalAmount = 0;
        foreach (RanchSpeciesData speciesData in ranchDatabase)
        {
            currentTotalAmount += Mathf.RoundToInt(speciesData.amount);
        }

        DebugTool.LogRanch("总数量更新: {0}/{1}", currentTotalAmount, MAX_TOTAL_AMOUNT);

        // 触发事件
        OnTotalAmountChanged?.Invoke(currentTotalAmount);

        DebugTool.LogRanch("UpdateTotalAmount() 完成");
    }

    /// <summary>
    /// 通过物种名称获取物种ScriptableObject
    /// </summary>
    private SpeciesScriptableObject GetSpeciesByName(string speciesName)
    {
        if (SpeciesLoader.Instance == null)
        {
            DebugTool.LogError("RanchSystem", "SpeciesLoader.Instance is null!");
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
            DebugTool.LogWarning("RanchSystem", "在所有物种中未找到名称为 '{0}' 的物种", speciesName);
        }
        else
        {
            DebugTool.LogRanch("成功找到物种: {0}", speciesName);
        }

        return result;
    }

    /// <summary>
    /// 设置管理者
    /// </summary>
    public bool SetManager(PersonScriptableObject person)
    {
        DebugTool.LogRanch("SetManager() 开始执行，设置管理者: {0}", person?.personName ?? "null");

        if (person == null || !person.recruited ||
            (person.status != PersonStatus.rest && person.status != PersonStatus.inranch))
        {
            DebugTool.LogRanch("SetManager 失败: 人员不符合条件, recruited={0}, status={1}",
                person?.recruited ?? false, person?.status ?? PersonStatus.rest);
            return false;
        }

        // 如果已有管理者，先移除
        if (Manager != null)
        {
            DebugTool.LogRanch("移除旧管理者: {0}", Manager.personName);
            RemoveManager();
        }

        Manager = person;
        // 使用PersonManager统一管理人员状态
        PersonManager.Instance.ChangePersonStatus(person, PersonStatus.inranch);

        DebugTool.LogRanch("新管理者已设置: {0}, 职业: {1}", person.personName, person.profession);

        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();

        OnManagerChanged?.Invoke(person);

        DebugTool.LogRanch("SetManager() 完成");
        return true;
    }

    /// <summary>
    /// 移除管理者
    /// </summary>
    public void RemoveManager()
    {
        DebugTool.LogRanch("RemoveManager() 开始执行，当前管理者: {0}", Manager?.personName ?? "无");

        if (Manager != null)
        {
            // 使用PersonManager统一管理人员状态
            PersonManager.Instance.ChangePersonStatus(Manager, PersonStatus.rest);
            DebugTool.LogRanch("管理者已移除: {0}", Manager.personName);
            Manager = null;

            // 重新计算预计产量
            CalculateNextMonthExpectedYield();
            UpdateUI();

            OnManagerChanged?.Invoke(null);
        }

        DebugTool.LogRanch("RemoveManager() 完成");
    }

    /// <summary>
    /// 获取所有已招募的人员（用于管理员选择）
    /// </summary>
    public List<PersonScriptableObject> GetAllRecruitedPersons()
    {
        DebugTool.LogRanch("GetAllRecruitedPersons() 开始执行");

        if (PersonManager.Instance == null)
        {
            DebugTool.LogError("RanchSystem", "PersonManager.Instance is null!");
            return new List<PersonScriptableObject>();
        }

        var persons = PersonManager.Instance.GetAllPersons();
        DebugTool.LogRanch("获取到 {0} 个已招募人员", persons.Count);

        return persons;
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        DebugTool.LogRanch("UpdateUI() 开始执行");

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
            DebugTool.LogRanch("UI已更新: 本月产量 {0}, 下月预计 {1}, 本月消耗 {2}, 管理者 {3}",
                CurrentMonthProduction, NextMonthExpectedYield, CurrentMonthCropConsumption, Manager?.personName ?? "无");
        }

        DebugTool.LogRanch("UpdateUI() 完成");
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
                DebugTool.LogWarning("RanchSystem", "UpdateUI: 未找到RanchUIController，UI更新功能将不可用");
            }
            else
            {
                DebugTool.LogRanch("成功找到RanchUIController");
            }
        }
    }

    /// <summary>
    /// 向牧场数据库添加物种
    /// </summary>
    public bool AddSpeciesToDatabase(SpeciesScriptableObject species, float amount)
    {
        DebugTool.LogRanch("AddSpeciesToDatabase() 开始执行，物种: {0}, 数量: {1}", species?.speciesName ?? "null", amount);

        if (species == null || amount < 0)
        {
            DebugTool.LogWarning("RanchSystem", "无效的物种或数量: species={0}, amount={1}", species?.speciesName ?? "null", amount);
            return false;
        }

        // 检查总数量上限
        int amountInt = Mathf.RoundToInt(amount);
        if (currentTotalAmount + amountInt > MAX_TOTAL_AMOUNT)
        {
            DebugTool.LogWarning("RanchSystem", "超过总数量上限: 当前{0}, 尝试添加{1}, 上限{2}",
                currentTotalAmount, amountInt, MAX_TOTAL_AMOUNT);
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
            DebugTool.LogRanch("更新现有物种: {0}, 新数量: {1}", species.speciesName, existingData.amount);
        }
        else
        {
            // 创建新数据
            RanchSpeciesData newData = new RanchSpeciesData(species.speciesName, amount, species.initialYield);
            ranchDatabase.Add(newData);
            DebugTool.LogRanch("添加新物种: {0}, 数量: {1}, 初始产量: {2}", species.speciesName, amount, species.initialYield);
        }

        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        UpdateTotalAmount(); // 更新总数量

        DebugTool.LogRanch("AddSpeciesToDatabase 完成: 当前数据库大小={0}, 当前总数量={1}",
            ranchDatabase.Count, currentTotalAmount);
        return true;
    }

    /// <summary>
    /// 从牧场数据库移除物种
    /// </summary>
    public bool RemoveSpeciesFromDatabase(string speciesName, float amount)
    {
        DebugTool.LogRanch("RemoveSpeciesFromDatabase() 开始执行，物种: {0}, 数量: {1}", speciesName, amount);

        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == speciesName);
        if (existingData == null)
        {
            DebugTool.LogWarning("RanchSystem", "牧场中找不到物种: {0}", speciesName);
            return false;
        }

        if (existingData.amount < amount)
        {
            DebugTool.LogWarning("RanchSystem", "数量不足: {0} 只有 {1}，无法移除 {2}", speciesName, existingData.amount, amount);
            return false;
        }

        existingData.amount -= amount;
        // 注意：即使数量为0，我们也不从数据库中移除该物种（保持只增不减的逻辑）

        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        UpdateTotalAmount(); // 更新总数量

        DebugTool.LogRanch("已从牧场移除物种: {0} x{1}, 剩余数量: {2}", speciesName, amount, existingData.amount);
        return true;
    }

    /// <summary>
    /// 增加指定物种的数量（+1）
    /// </summary>
    public bool IncrementSpeciesAmount(string speciesName)
    {
        DebugTool.LogRanch("IncrementSpeciesAmount() 开始执行，物种: {0}", speciesName);

        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == speciesName);
        if (existingData == null)
        {
            DebugTool.LogWarning("RanchSystem", "牧场中找不到物种: {0}", speciesName);
            return false;
        }

        // 检查总数量上限
        if (currentTotalAmount + 1 > MAX_TOTAL_AMOUNT)
        {
            DebugTool.LogWarning("RanchSystem", "超过总数量上限: 当前{0}, 上限{1}", currentTotalAmount, MAX_TOTAL_AMOUNT);
            return false;
        }

        existingData.amount += 1;

        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateTotalAmount(); // 更新总数量
        UpdateUI();

        DebugTool.LogRanch("已增加物种数量: {0} +1, 当前数量: {1}", speciesName, existingData.amount);
        return true;
    }

    /// <summary>
    /// 减少指定物种的数量（-1）
    /// </summary>
    public bool DecrementSpeciesAmount(string speciesName)
    {
        DebugTool.LogRanch("DecrementSpeciesAmount() 开始执行，物种: {0}", speciesName);

        RanchSpeciesData existingData = ranchDatabase.Find(data => data.speciesName == speciesName);
        if (existingData == null)
        {
            DebugTool.LogWarning("RanchSystem", "牧场中找不到物种: {0}", speciesName);
            return false;
        }

        if (existingData.amount < 1)
        {
            DebugTool.LogWarning("RanchSystem", "数量不足: {0} 只有 {1}，无法减少", speciesName, existingData.amount);
            return false;
        }

        existingData.amount -= 1;

        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();
        UpdateTotalAmount(); // 更新总数量

        DebugTool.LogRanch("已减少物种数量: {0} -1, 当前数量: {1}", speciesName, existingData.amount);
        return true;
    }

    /// <summary>
    /// 获取牧场数据库的副本
    /// </summary>
    public List<RanchSpeciesData> GetRanchDatabase()
    {
        DebugTool.LogRanch("GetRanchDatabase() 调用，返回 {0} 个项目", ranchDatabase.Count);
        return new List<RanchSpeciesData>(ranchDatabase);
    }

    /// <summary>
    /// 从保存数据加载牧场数据库（修复副本问题）
    /// </summary>
    public void LoadRanchDatabaseFromSave(List<RanchSpeciesSaveData> saveDataList)
    {
        DebugTool.LogRanch("LoadRanchDatabaseFromSave() 开始执行，加载 {0} 个项目", saveDataList?.Count ?? 0);

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
                DebugTool.LogWarning("RanchSystem", "无法找到保存的物种: {0}，跳过加载", speciesData.speciesName);
            }
        }

        DebugTool.LogRanch("从保存数据加载牧场数据库完成，共 {0} 个项目", ranchDatabase.Count);
    }

    /// <summary>
    /// 获取管理者
    /// </summary>
    public PersonScriptableObject GetManager()
    {
        DebugTool.LogRanch("GetManager() 调用，返回管理者: {0}", Manager?.personName ?? "无");
        return Manager;
    }

    /// <summary>
    /// 清空牧场数据库（用于测试）
    /// </summary>
    public void ClearDatabase()
    {
        DebugTool.LogRanch("ClearDatabase() 开始执行");
        ranchDatabase.Clear();
        CalculateNextMonthExpectedYield();
        UpdateUI();
        DebugTool.LogRanch("牧场数据库已清空");
    }
}
