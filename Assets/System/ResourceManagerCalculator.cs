using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源计算器 - 集中计算每月的资源变化
/// </summary>
public class ResourceManagerCalculator : MonoBehaviour
{
    public static ResourceManagerCalculator Instance { get; private set; }

    [Header("系统引用")]
    [SerializeField] private FarmSystem farmSystem;
    [SerializeField] private ForestSystem forestSystem;
    [SerializeField] private RanchSystem ranchSystem;
    [SerializeField] private PersonManager personManager;

    // 资源变化数据
    [System.Serializable]
    public class ResourceChange
    {
        public float crop;
        public float animal;
        public float material;

        public ResourceChange(float c = 0, float a = 0, float m = 0)
        {
            crop = c;
            animal = a;
            material = m;
        }

        public static ResourceChange operator +(ResourceChange a, ResourceChange b)
        {
            return new ResourceChange(a.crop + b.crop, a.animal + b.animal, a.material + b.material);
        }

        public static ResourceChange operator -(ResourceChange a, ResourceChange b)
        {
            return new ResourceChange(a.crop - b.crop, a.animal - b.animal, a.material - b.material);
        }

        public override string ToString()
        {
            return $"Crop: {crop:F2}, Animal: {animal:F2}, Material: {material:F2}";
        }
    }

    /// <summary>
    /// 每月资源变化摘要
    /// </summary>
    [System.Serializable]
    public class MonthlyResourceSummary
    {
        public ResourceChange currentMonthNetGrowth;      // 本月净增长
        public ResourceChange nextMonthNetGrowth;        // 下月净增长
        public ResourceChange monthOverMonthChange;      // 环比变化 (下月-本月)
        public ResourceChange currentResources;          // 当前资源量

        public MonthlyResourceSummary() { }

        public MonthlyResourceSummary(ResourceChange current, ResourceChange next, ResourceChange currentRes)
        {
            currentMonthNetGrowth = current;
            nextMonthNetGrowth = next;
            monthOverMonthChange = next - current;
            currentResources = currentRes;
        }

        public void UpdateCurrentResources(ResourceChange newResources)
        {
            currentResources = newResources;
        }
    }

    public MonthlyResourceSummary CurrentSummary { get; private set; }
    public event Action<MonthlyResourceSummary> OnResourceSummaryUpdated;

    /// <summary>
    /// 生产系统产量事件 (crop, ani, mat)
    /// </summary>
    public event Action<float, float, float> OnProductionDataCalculated;

    /// <summary>
    /// 消费系统消耗事件 (crop, ani, mat)
    /// </summary>
    public event Action<float, float, float> OnConsumptionDataCalculated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 自动查找系统引用
        FindSystems();
    }

    private void Start()
    {
        // 初始化时计算一次
        CalculateResourceSummary();
    }

    private void OnEnable()
    {
        // 订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += OnPhaseChanged;
        }

        // 订阅生产系统事件
        SubscribeToProductionEvents();
    }

    private void OnDisable()
    {
        // 取消订阅
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= OnPhaseChanged;
        }

        // 取消订阅生产系统事件
        UnsubscribeFromProductionEvents();
    }

    private void OnPhaseChanged(GlobalTimeSystem.MoonPhase phase, int phaseCount)
    {
        // 每个月相变化时重新计算
        CalculateResourceSummary();
    }

    /// <summary>
    /// 自动查找系统引用
    /// </summary>
    private void FindSystems()
    {
        if (farmSystem == null)
            farmSystem = FindObjectOfType<FarmSystem>();
        if (forestSystem == null)
            forestSystem = FindObjectOfType<ForestSystem>();

        if (ranchSystem == null)
            ranchSystem = FindObjectOfType<RanchSystem>();
        if (personManager == null)
            personManager = PersonManager.Instance;

        if(farmSystem==null){
            Debug.LogError("❌ FarmSystem 未找到！");
        }
        if (forestSystem == null)
            Debug.LogError("❌ ForestSystem 未找到！");
        if (ranchSystem == null)
            Debug.LogError("❌ RanchSystem 未找到！");
        if (personManager == null)
            Debug.LogError("❌ PersonManager 未找到！");
    }

    /// <summary>
    /// 计算本月资源净增长
    /// </summary>
    public ResourceChange CalculateCurrentMonthNetGrowth()
    {
        Debug.Log("========================================");
        Debug.Log("📊 开始计算本月资源净增长");
        Debug.Log("========================================");

        ResourceChange total = new ResourceChange();

        // 记录各系统的贡献
        float farmCropProduction = 0f;
        float forestMatProduction = 0f;
        float forestCropConsumption = 0f;
        float ranchAniProduction = 0f;
        float ranchCropConsumption = 0f;
        float populationCropConsumption = 0f;
        float populationAniConsumption = 0f;
        float populationMatConsumption = 0f;

        // 1. 农场生产 (Crop +)
        if (farmSystem != null)
        {
            farmCropProduction = farmSystem.CurrentMonthProduction;
            total.crop += farmCropProduction;
            Debug.Log($"🌾 农场系统:");
            Debug.Log($"   ├─ Crop 生产: +{farmCropProduction:F2}");
        }

        // 2. 森林生产 (Mat +) 和消耗 (Crop -)
        if (forestSystem != null)
        {
            forestMatProduction = forestSystem.CurrentMonthProduction;
            forestCropConsumption = forestSystem.CurrentMonthCropConsumption;
            total.material += forestMatProduction;
            total.crop -= forestCropConsumption;
            Debug.Log($"🌲 森林系统:");
            Debug.Log($"   ├─ Mat 生产: +{forestMatProduction:F2}");
            Debug.Log($"   └─ Crop 消耗: -{forestCropConsumption:F2}");
        }

        // 3. 牧场生产 (Ani +) 和消耗 (Crop -)
        if (ranchSystem != null)
        {
            ranchAniProduction = ranchSystem.CurrentMonthProduction;
            ranchCropConsumption = ranchSystem.CurrentMonthCropConsumption;
            total.animal += ranchAniProduction;
            total.crop -= ranchCropConsumption;
            Debug.Log($"🐄 牧场系统:");
            Debug.Log($"   ├─ Ani 生产: +{ranchAniProduction:F2}");
            Debug.Log($"   └─ Crop 消耗: -{ranchCropConsumption:F2}");
        }

        // 4. 人口消耗 (Crop -, Ani -, Mat -)
        if (personManager != null)
        {
            var recruitedPersons = personManager.GetRecruitedPersons();
            Debug.Log($"👥 人口系统 ({recruitedPersons.Count} 人):");

            if (recruitedPersons.Count > 0)
            {
                for (int i = 0; i < recruitedPersons.Count; i++)
                {
                    var person = recruitedPersons[i];
                    bool isLast = (i == recruitedPersons.Count - 1);
                    string prefix = isLast ? "   └─" : "   ├─";

                    Debug.Log($"{prefix} [{person.personName}] Crop:{person.monthlyCropConsumption:F2} Ani:{person.monthlyAniConsumption:F2} Mat:{person.monthlyMatConsumption:F2}");

                    total.crop -= person.monthlyCropConsumption;
                    total.animal -= person.monthlyAniConsumption;
                    total.material -= person.monthlyMatConsumption;

                    populationCropConsumption += person.monthlyCropConsumption;
                    populationAniConsumption += person.monthlyAniConsumption;
                    populationMatConsumption += person.monthlyMatConsumption;
                }
            }

            // 输出总计
            Debug.Log($"   📊 总计消耗:");
            Debug.Log($"      ├─ Crop: -{populationCropConsumption:F2}");
            Debug.Log($"      ├─ Ani: -{populationAniConsumption:F2}");
            Debug.Log($"      └─ Mat: -{populationMatConsumption:F2}");
        }

        // 输出汇总
        Debug.Log("========================================");
        Debug.Log("📈 本月资源变化汇总:");
        Debug.Log($"----------------------------------------");
        Debug.Log($"🌾 Crop (作物):");
        Debug.Log($"   ├─ 生产: +{farmCropProduction:F2} (来自农场)");
        float totalCropConsumption = forestCropConsumption + ranchCropConsumption + populationCropConsumption;
        Debug.Log($"   ├─ 消耗: -{totalCropConsumption:F2} (森林:{forestCropConsumption:F2} + 牧场:{ranchCropConsumption:F2} + 人口:{populationCropConsumption:F2})");
        Debug.Log($"   └─ 净增长: {(total.crop >= 0 ? "+" : "")}{total.crop:F2}");

        Debug.Log($"🐄 Ani (动物):");
        Debug.Log($"   ├─ 生产: +{ranchAniProduction:F2} (来自牧场)");
        Debug.Log($"   ├─ 消耗: -{populationAniConsumption:F2} (来自人口)");
        Debug.Log($"   └─ 净增长: {(total.animal >= 0 ? "+" : "")}{total.animal:F2}");

        Debug.Log($"🪵 Mat (材料):");
        Debug.Log($"   ├─ 生产: +{forestMatProduction:F2} (来自森林)");
        Debug.Log($"   ├─ 消耗: -{populationMatConsumption:F2} (来自人口)");
        Debug.Log($"   └─ 净增长: {(total.material >= 0 ? "+" : "")}{total.material:F2}");
        Debug.Log("========================================");

        return total;
    }

    /// <summary>
    /// 计算下月资源净增长
    /// </summary>
    public ResourceChange CalculateNextMonthNetGrowth()
    {
        Debug.Log("========================================");
        Debug.Log("📊 开始计算下月资源净增长");
        Debug.Log("========================================");

        ResourceChange total = new ResourceChange();

        // 记录各系统的贡献
        float farmCropProduction = 0f;
        float forestMatProduction = 0f;
        float forestCropConsumption = 0f;
        float ranchAniProduction = 0f;
        float ranchCropConsumption = 0f;
        float populationCropConsumption = 0f;
        float populationAniConsumption = 0f;
        float populationMatConsumption = 0f;

        // 1. 农场预计产量 (Crop +)
        if (farmSystem != null)
        {
            farmCropProduction = farmSystem.NextMonthExpectedYield;
            float managerBonus = 1f;
            string managerInfo = "无管理者";
            if (farmSystem.Manager != null && farmSystem.Manager.profession == PersonProfession.farmer)
            {
                managerBonus = 1.2f;
                managerInfo = $"管理者: {farmSystem.Manager.personName} (农民 ×1.2)";
            }
            farmCropProduction *= managerBonus;
            total.crop += farmCropProduction;
            Debug.Log($"🌾 农场系统 [{managerInfo}]:");
            Debug.Log($"   └─ 预计 Crop 生产: +{farmCropProduction:F2}");
        }

        // 2. 森林预计产量 (Mat +) 和消耗 (Crop -)
        if (forestSystem != null)
        {
            forestMatProduction = CalculateForestNextMonthProduction();
            forestCropConsumption = CalculateForestNextMonthConsumption();

            float managerBonus = 1f;
            string managerInfo = "无管理者";
            if (forestSystem.Manager != null && forestSystem.Manager.profession == PersonProfession.farmer)
            {
                managerBonus = 1.5f;
                managerInfo = $"管理者: {forestSystem.Manager.personName} (农民 ×1.5)";
            }
            else if (forestSystem.Manager == null)
            {
                managerBonus = 1f;
                managerInfo = "无管理者 (×1.0)";
            }

            float originalProduction = forestMatProduction;
            forestMatProduction *= managerBonus;
            total.material += forestMatProduction;
            total.crop -= forestCropConsumption;

            Debug.Log($"🌲 森林系统 [{managerInfo}]:");
            Debug.Log($"   ├─ 预计 Mat 生产: +{originalProduction:F2} → 加成后 +{forestMatProduction:F2}");
            Debug.Log($"   └─ 预计 Crop 消耗: -{forestCropConsumption:F2}");
        }

        // 3. 牧场预计产量 (Ani +) 和消耗 (Crop -)
        if (ranchSystem != null)
        {
            ranchAniProduction = CalculateRanchNextMonthProduction();
            ranchCropConsumption = CalculateRanchNextMonthConsumption();

            float managerBonus = 1f;
            string managerInfo = "无管理者";
            if (ranchSystem.Manager != null && ranchSystem.Manager.profession == PersonProfession.farmer)
            {
                managerBonus = 1.2f;
                managerInfo = $"管理者: {ranchSystem.Manager.personName} (农民 ×1.2)";
            }
            else if (ranchSystem.Manager == null)
            {
                managerBonus = 0f;
                managerInfo = "无管理者 (×0.0)";
            }

            float originalProduction = ranchAniProduction;
            ranchAniProduction *= managerBonus;
            total.animal += ranchAniProduction;
            total.crop -= ranchCropConsumption;

            Debug.Log($"🐄 牧场系统 [{managerInfo}]:");
            Debug.Log($"   ├─ 预计 Ani 生产: +{originalProduction:F2} → 加成后 +{ranchAniProduction:F2}");
            Debug.Log($"   └─ 预计 Crop 消耗: -{ranchCropConsumption:F2}");
        }

        // 4. 人口消耗 (假设人口不变)
        if (personManager != null)
        {
            var recruitedPersons = personManager.GetRecruitedPersons();
            Debug.Log($"👥 人口系统 ({recruitedPersons.Count} 人):");

            if (recruitedPersons.Count > 0)
            {
                for (int i = 0; i < recruitedPersons.Count; i++)
                {
                    var person = recruitedPersons[i];
                    bool isLast = (i == recruitedPersons.Count - 1);
                    string prefix = isLast ? "   └─" : "   ├─";

                    Debug.Log($"{prefix} [{person.personName}] Crop:{person.monthlyCropConsumption:F2} Ani:{person.monthlyAniConsumption:F2} Mat:{person.monthlyMatConsumption:F2}");

                    total.crop -= person.monthlyCropConsumption;
                    total.animal -= person.monthlyAniConsumption;
                    total.material -= person.monthlyMatConsumption;

                    populationCropConsumption += person.monthlyCropConsumption;
                    populationAniConsumption += person.monthlyAniConsumption;
                    populationMatConsumption += person.monthlyMatConsumption;
                }
            }

            // 输出总计
            Debug.Log($"   📊 总计消耗:");
            Debug.Log($"      ├─ Crop: -{populationCropConsumption:F2}");
            Debug.Log($"      ├─ Ani: -{populationAniConsumption:F2}");
            Debug.Log($"      └─ Mat: -{populationMatConsumption:F2}");
        }

        // 输出汇总
        Debug.Log("========================================");
        Debug.Log("📈 下月资源变化汇总:");
        Debug.Log($"----------------------------------------");
        Debug.Log($"🌾 Crop (作物):");
        Debug.Log($"   ├─ 生产: +{farmCropProduction:F2} (来自农场)");
        float totalCropConsumption = forestCropConsumption + ranchCropConsumption + populationCropConsumption;
        Debug.Log($"   ├─ 消耗: -{totalCropConsumption:F2} (森林:{forestCropConsumption:F2} + 牧场:{ranchCropConsumption:F2} + 人口:{populationCropConsumption:F2})");
        Debug.Log($"   └─ 净增长: {(total.crop >= 0 ? "+" : "")}{total.crop:F2}");

        Debug.Log($"🐄 Ani (动物):");
        Debug.Log($"   ├─ 生产: +{ranchAniProduction:F2} (来自牧场)");
        Debug.Log($"   ├─ 消耗: -{populationAniConsumption:F2} (来自人口)");
        Debug.Log($"   └─ 净增长: {(total.animal >= 0 ? "+" : "")}{total.animal:F2}");

        Debug.Log($"🪵 Mat (材料):");
        Debug.Log($"   ├─ 生产: +{forestMatProduction:F2} (来自森林)");
        Debug.Log($"   ├─ 消耗: -{populationMatConsumption:F2} (来自人口)");
        Debug.Log($"   └─ 净增长: {(total.material >= 0 ? "+" : "")}{total.material:F2}");
        Debug.Log("========================================");

        return total;
    }

    /// <summary>
    /// 计算森林下月产量 (考虑退化)
    /// </summary>
    private float CalculateForestNextMonthProduction()
    {
        if (forestSystem == null || SpeciesLoader.Instance == null)
            return 0f;

        float total = 0f;
        var database = forestSystem.GetForestDatabase();

        foreach (var speciesData in database)
        {
            SpeciesScriptableObject species = GetSpeciesByName(speciesData.speciesName);
            if (species == null || species.speciesType != SpeciesType.Mat || !species.unlocked)
                continue;

            // 计算退化后的产量
            float decayedYield = speciesData.nextPhaseYield - species.decayPerPhase;
            decayedYield = Mathf.Max(decayedYield, species.leastYield);

            total += decayedYield * speciesData.amount;
        }

        return total;
    }

    /// <summary>
    /// 计算森林下月消耗
    /// </summary>
    private float CalculateForestNextMonthConsumption()
    {
        if (forestSystem == null || SpeciesLoader.Instance == null)
            return 0f;

        float total = 0f;
        var database = forestSystem.GetForestDatabase();

        foreach (var speciesData in database)
        {
            SpeciesScriptableObject species = GetSpeciesByName(speciesData.speciesName);
            if (species == null || species.speciesType != SpeciesType.Mat || !species.unlocked)
                continue;

            total += species.monthlyCropConsumption * speciesData.amount;
        }

        return total;
    }

    /// <summary>
    /// 计算牧场下月产量 (考虑退化)
    /// </summary>
    private float CalculateRanchNextMonthProduction()
    {
        if (ranchSystem == null || SpeciesLoader.Instance == null)
            return 0f;

        float total = 0f;
        var database = ranchSystem.GetRanchDatabase();

        foreach (var speciesData in database)
        {
            SpeciesScriptableObject species = GetSpeciesByName(speciesData.speciesName);
            if (species == null || species.speciesType != SpeciesType.Ani || !species.unlocked)
                continue;

            // 计算退化后的产量
            float decayedYield = speciesData.nextPhaseYield - species.decayPerPhase;
            decayedYield = Mathf.Max(decayedYield, species.leastYield);

            total += decayedYield * speciesData.amount;
        }

        return total;
    }

    /// <summary>
    /// 计算牧场下月消耗
    /// </summary>
    private float CalculateRanchNextMonthConsumption()
    {
        if (ranchSystem == null || SpeciesLoader.Instance == null)
            return 0f;

        float total = 0f;
        var database = ranchSystem.GetRanchDatabase();

        foreach (var speciesData in database)
        {
            SpeciesScriptableObject species = GetSpeciesByName(speciesData.speciesName);
            if (species == null || species.speciesType != SpeciesType.Ani || !species.unlocked)
                continue;

            total += species.monthlyCropConsumption * speciesData.amount;
        }

        return total;
    }

    /// <summary>
    /// 获取当前资源量
    /// </summary>
    public ResourceChange GetCurrentResources()
    {
        if (ResourceManager.Instance == null)
            return new ResourceChange();

        return new ResourceChange(
            ResourceManager.Instance.GetCropAmount(),
            ResourceManager.Instance.GetAniAmount(),
            ResourceManager.Instance.GetMatAmount()
        );
    }

    /// <summary>
    /// 计算完整的资源摘要
    /// </summary>
    public void CalculateResourceSummary()
    {
        // 确保系统引用
        FindSystems();

        // 计算本月净增长
        ResourceChange currentMonth = CalculateCurrentMonthNetGrowth();

        // 计算下月净增长
        ResourceChange nextMonth = CalculateNextMonthNetGrowth();

        // 获取当前资源
        ResourceChange currentResources = GetCurrentResources();

        // 创建摘要
        CurrentSummary = new MonthlyResourceSummary(currentMonth, nextMonth, currentResources);

        // 触发事件
        OnResourceSummaryUpdated?.Invoke(CurrentSummary);

        Debug.Log($"[ResourceManagerCalculator] 资源摘要已更新:");
        Debug.Log($"  本月净增长: {currentMonth}");
        Debug.Log($"  下月净增长: {nextMonth}");
        Debug.Log($"  环比变化: {CurrentSummary.monthOverMonthChange}");
        Debug.Log($"  当前资源: {currentResources}");
    }

    /// <summary>
    /// 通过物种名称获取物种信息
    /// </summary>
    private SpeciesScriptableObject GetSpeciesByName(string speciesName)
    {
        if (SpeciesLoader.Instance == null)
            return null;

        var allSpecies = new List<SpeciesScriptableObject>();
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedCropSpecies());
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedAniSpecies());
        allSpecies.AddRange(SpeciesLoader.Instance.GetUnlockedMatSpecies());

        return allSpecies.Find(s => s.speciesName == speciesName);
    }

    /// <summary>
    /// 公共方法: 获取当前资源摘要
    /// </summary>
    public MonthlyResourceSummary GetResourceSummary()
    {
        if (CurrentSummary == null)
        {
            CalculateResourceSummary();
        }
        return CurrentSummary;
    }

    /// <summary>
    /// 订阅生产系统事件
    /// </summary>
    private void SubscribeToProductionEvents()
    {
        FindSystems();

        // 订阅农场生产事件
        if (farmSystem != null)
        {
            farmSystem.OnProductionCalculated.AddListener(OnFarmProductionCalculated);
        }

        // 订阅森林生产事件
        if (forestSystem != null)
        {
            forestSystem.OnProductionCalculated.AddListener(OnForestProductionCalculated);
            forestSystem.OnCropConsumptionCalculated.AddListener(OnForestCropConsumptionCalculated);
        }

        // 订阅牧场生产事件
        if (ranchSystem != null)
        {
            ranchSystem.OnProductionCalculated.AddListener(OnRanchProductionCalculated);
            ranchSystem.OnCropConsumptionCalculated.AddListener(OnRanchCropConsumptionCalculated);
        }

        // 订阅人口消耗事件
        if (personManager != null)
        {
            personManager.OnPopulationConsumptionCalculated.AddListener(OnPopulationConsumptionCalculated);
        }
    }

    /// <summary>
    /// 取消订阅生产系统事件
    /// </summary>
    private void UnsubscribeFromProductionEvents()
    {
        if (farmSystem != null)
        {
            farmSystem.OnProductionCalculated.RemoveListener(OnFarmProductionCalculated);
        }

        if (forestSystem != null)
        {
            forestSystem.OnProductionCalculated.RemoveListener(OnForestProductionCalculated);
            forestSystem.OnCropConsumptionCalculated.RemoveListener(OnForestCropConsumptionCalculated);
        }

        if (ranchSystem != null)
        {
            ranchSystem.OnProductionCalculated.RemoveListener(OnRanchProductionCalculated);
            ranchSystem.OnCropConsumptionCalculated.RemoveListener(OnRanchCropConsumptionCalculated);
        }

        if (personManager != null)
        {
            personManager.OnPopulationConsumptionCalculated.RemoveListener(OnPopulationConsumptionCalculated);
        }
    }

    /// <summary>
    /// 农场产量计算回调
    /// </summary>
    private void OnFarmProductionCalculated(float production)
    {
        Debug.Log($"[ResourceManagerCalculator] 农场产量: {production}");
        if (ResourceManager.Instance != null && production > 0)
        {
            ResourceManager.Instance.AddCrop(production);
        }
        OnProductionDataCalculated?.Invoke(production, 0f, 0f);
    }

    /// <summary>
    /// 森林产量计算回调（生产 Mat）
    /// </summary>
    private void OnForestProductionCalculated(float production)
    {
        Debug.Log($"[ResourceManagerCalculator] 森林产量: {production}");
        if (ResourceManager.Instance != null && production > 0)
        {
            ResourceManager.Instance.AddMat(production);
        }
        OnProductionDataCalculated?.Invoke(0f, 0f, production);
    }

    /// <summary>
    /// 森林作物消耗计算回调
    /// </summary>
    private void OnForestCropConsumptionCalculated(float consumption)
    {
        Debug.Log($"[ResourceManagerCalculator] 森林消耗: {consumption}");
        if (ResourceManager.Instance != null && consumption > 0)
        {
            ResourceManager.Instance.AutoConsumeCrop(consumption);
        }
        OnConsumptionDataCalculated?.Invoke(consumption, 0f, 0f);
    }

    /// <summary>
    /// 牧场产量计算回调（生产 Ani）
    /// </summary>
    private void OnRanchProductionCalculated(float production)
    {
        Debug.Log($"[ResourceManagerCalculator] 牧场产量: {production}");
        if (ResourceManager.Instance != null && production > 0)
        {
            ResourceManager.Instance.AddAni(production);
        }
        OnProductionDataCalculated?.Invoke(0f, production, 0f);
    }

    /// <summary>
    /// 牧场作物消耗计算回调
    /// </summary>
    private void OnRanchCropConsumptionCalculated(float consumption)
    {
        Debug.Log($"[ResourceManagerCalculator] 牧场消耗: {consumption}");
        if (ResourceManager.Instance != null && consumption > 0)
        {
            ResourceManager.Instance.AutoConsumeCrop(consumption);
        }
        OnConsumptionDataCalculated?.Invoke(consumption, 0f, 0f);
    }

    /// <summary>
    /// 人口消耗计算回调
    /// </summary>
    private void OnPopulationConsumptionCalculated(PopulationConsumptionData consumptionData)
    {
        Debug.Log($"[ResourceManagerCalculator] 人口消耗 - Crop: {consumptionData.cropConsumption}, Ani: {consumptionData.aniConsumption}, Mat: {consumptionData.matConsumption}");
        if (ResourceManager.Instance != null)
        {
            if (consumptionData.cropConsumption > 0)
                ResourceManager.Instance.AutoConsumeCrop(consumptionData.cropConsumption);
            if (consumptionData.aniConsumption > 0)
                ResourceManager.Instance.AutoConsumeAni(consumptionData.aniConsumption);
            if (consumptionData.matConsumption > 0)
                ResourceManager.Instance.AutoConsumeMat(consumptionData.matConsumption);
        }
        OnConsumptionDataCalculated?.Invoke(consumptionData.cropConsumption, consumptionData.aniConsumption, consumptionData.matConsumption);
    }
}
