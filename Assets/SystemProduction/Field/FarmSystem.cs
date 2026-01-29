using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FarmSystem : MonoBehaviour
{
    [Header("田地配置")]
    [SerializeField] private List<FieldUnit> fields = new List<FieldUnit>();
    [SerializeField] private int totalFields = 15;

    [Header("生产配置")]
    [SerializeField] private float farmerBonusMultiplier = 1.2f; // 农民管理者加成倍率
    [SerializeField] private float noManagerMultiplier = 1f; // 无管理者时的倍率

    [Header("UI引用")]
    [SerializeField] private FarmUIController uiController;

    public PersonScriptableObject Manager { get; private set; }
    public float CurrentMonthProduction { get; private set; }
    public float NextMonthExpectedYield { get; private set; }

    public UnityEvent<float> OnProductionCalculated = new UnityEvent<float>();
    public UnityEvent<PersonScriptableObject> OnManagerChanged = new UnityEvent<PersonScriptableObject>();

    private void Awake()
    {
        DebugTool.LogFarm("Awake() 开始执行");
        InitializeFields();
        DebugTool.LogFarm("Awake() 完成");
    }

    private void Start()
    {
        DebugTool.LogFarm("Start() 开始执行");

        // 尝试加载存档
        if (FarmSaveSystem.Instance.SaveExists())
        {
            DebugTool.LogFarm("检测到存档文件，开始加载");
            FarmSaveSystem.Instance.LoadFarmData(this);
            DebugTool.LogFarm("存档加载完成");
        }
        else
        {
            DebugTool.LogFarm("未检测到存档文件，使用新游戏数据");
        }

        // 计算下月预计产量
        CalculateNextMonthExpectedYield();

        // 更新UI
        UpdateUI();

        DebugTool.LogFarm("Start() 完成");
    }

    private void OnEnable()
    {
        DebugTool.LogFarm("OnEnable() 开始执行");

        // 订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += HandlePhaseChange;
            DebugTool.LogFarm("成功订阅月相变化事件");
        }
        else
        {
            DebugTool.LogWarning("FarmSystem", "OnEnable时 GlobalTimeSystem.Instance 为 null");
        }

        DebugTool.LogFarm("OnEnable() 完成");
    }

    private void OnDisable()
    {
        DebugTool.LogFarm("OnDisable() 开始执行");

        // 取消订阅月相变化事件
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= HandlePhaseChange;
            DebugTool.LogFarm("成功取消订阅月相变化事件");
        }

        DebugTool.LogFarm("OnDisable() 完成");
    }

    /// <summary>
    /// 初始化田地列表
    /// </summary>
    private void InitializeFields()
    {
        DebugTool.LogFarm("InitializeFields() 开始执行，当前田地数量: {0}", fields.Count);

        if (fields.Count == 0)
        {
            // 创建15个田地
            for (int i = 0; i < totalFields; i++)
            {
                GameObject fieldObj = new GameObject($"Field_{i}");
                fieldObj.transform.SetParent(transform);
                FieldUnit field = fieldObj.AddComponent<FieldUnit>();
                fields.Add(field);
            }
            DebugTool.LogFarm("初始化了 {0} 个田地", totalFields);
        }
        else
        {
            DebugTool.LogFarm("田地已存在，跳过初始化");
        }

        DebugTool.LogFarm("InitializeFields() 完成");
    }

    /// <summary>
    /// 处理月相变化
    /// </summary>
    private void HandlePhaseChange(GlobalTimeSystem.MoonPhase phase, int phaseCount)
    {
        DebugTool.LogFarm("========================================");
        DebugTool.LogFarm("HandlePhaseChange() 开始执行，月相: {0}, 阶段数: {1}", phase, phaseCount);

        // 第一阶段：获取本月产量
        CurrentMonthProduction = 0;

        DebugTool.LogFarm("开始遍历 {0} 个田地", fields.Count);
        foreach (FieldUnit field in fields)
        {
            field.HandlePhaseChange();
            // 通知田地"农场已读取产量"
            float yield = field.GetAndLockCurrentYield();
            CurrentMonthProduction += yield;
            field.UpdateNPY();
            DebugTool.LogFarm("田地产量: {0:F2}, 累计产量: {1:F2}", yield, CurrentMonthProduction);
        }

        // 应用管理者加成
        float finalProduction = CurrentMonthProduction;
        DebugTool.LogFarm("应用加成前: {0:F2}, 管理者: {1}", finalProduction, Manager?.personName ?? "无");

        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            finalProduction *= farmerBonusMultiplier;
            DebugTool.LogFarm("应用农民管理者加成 {0:F2}, 最终产量: {1:F2}", farmerBonusMultiplier, finalProduction);
        }
        else if (Manager == null)
        {
            finalProduction *= noManagerMultiplier;
            DebugTool.LogFarm("无管理者加成 {0:F2}, 最终产量: {1:F2}", noManagerMultiplier, finalProduction);
        }
        else
        {
            DebugTool.LogFarm("管理者非农民职业: {0}, 职业: {1}, 不应用加成", Manager.personName, Manager.profession);
        }

        // 第二阶段：获取下月预计产量
        CalculateNextMonthExpectedYield();

        // 通知UI更新
        UpdateUI();

        // 触发生产事件（通知 ResourceManagerCalculator 进行资源修改）
        OnProductionCalculated?.Invoke(finalProduction);
        DebugTool.LogFarm("触发生产事件: {0:F2}", finalProduction);

        DebugTool.LogFarm("HandlePhaseChange() 完成");
    }

    /// <summary>
    /// 计算下个月预计产量
    /// </summary>
    public void CalculateNextMonthExpectedYield()
    {
        DebugTool.LogFarm("CalculateNextMonthExpectedYield() 开始执行");

        NextMonthExpectedYield = 0;
        int unlockedFields = 0;
        int producingFields = 0;

        foreach (FieldUnit field in fields)
        {
            if (field.IsUnlocked && field.CurrentCrop != null && field.IsProducing)
            {
                NextMonthExpectedYield += field.CurrentNPY;
                producingFields++;
            }
            if (field.IsUnlocked)
            {
                unlockedFields++;
            }
        }

        DebugTool.LogFarm("下月预计产量计算: 解锁田地 {0}/{1}, 生产田地 {2}, 原始产量 {3:F2}",
            unlockedFields, fields.Count, producingFields, NextMonthExpectedYield);

        // 应用管理者加成
        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            NextMonthExpectedYield *= farmerBonusMultiplier;
            DebugTool.LogFarm("应用农民管理者加成 {0:F2}, 预计产量: {1:F2}", farmerBonusMultiplier, NextMonthExpectedYield);
        }
        else if (Manager == null)
        {
            NextMonthExpectedYield *= noManagerMultiplier;
            DebugTool.LogFarm("无管理者加成 {0:F2}, 预计产量: {1:F2}", noManagerMultiplier, NextMonthExpectedYield);
        }

        DebugTool.LogFarm("CalculateNextMonthExpectedYield() 完成，最终预计产量: {0:F2}", NextMonthExpectedYield);
    }

    /// <summary>
    /// 设置管理者
    /// </summary>
    public bool SetManager(PersonScriptableObject person)
    {
        DebugTool.LogFarm("SetManager() 开始执行，设置管理者: {0}", person?.personName ?? "null");

        if (person == null || !person.recruited ||
            (person.status != PersonStatus.rest && person.status != PersonStatus.infarm))
        {
            DebugTool.LogFarm("SetManager 失败: 人员不符合条件, recruited={0}, status={1}",
                person?.recruited ?? false, person?.status ?? PersonStatus.rest);
            return false;
        }

        // 如果已有管理者，先移除
        if (Manager != null)
        {
            DebugTool.LogFarm("移除旧管理者: {0}", Manager.personName);
            RemoveManager();
        }

        Manager = person;
        // 使用PersonManager统一管理人员状态
        PersonManager.Instance.ChangePersonStatus(person, PersonStatus.inManagerFarm);
        DebugTool.LogFarm("新管理者已设置: {0}, 职业: {1}", person.personName, person.profession);

        // 重新计算预计产量
        CalculateNextMonthExpectedYield();
        UpdateUI();

        OnManagerChanged?.Invoke(person);

        DebugTool.LogFarm("SetManager() 完成");
        return true;
    }

    /// <summary>
    /// 移除管理者
    /// </summary>
    public void RemoveManager()
    {
        DebugTool.LogFarm("RemoveManager() 开始执行，当前管理者: {0}", Manager?.personName ?? "无");

        if (Manager != null)
        {
            // 使用PersonManager统一管理人员状态
            PersonManager.Instance.ChangePersonStatus(Manager, PersonStatus.rest);
            DebugTool.LogFarm("管理者已移除: {0}", Manager.personName);
            Manager = null;

            // 重新计算预计产量
            CalculateNextMonthExpectedYield();
            UpdateUI();

            OnManagerChanged?.Invoke(null);
        }

        DebugTool.LogFarm("RemoveManager() 完成");
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        DebugTool.LogFarm("UpdateUI() 开始执行");

        if (uiController != null)
        {
            uiController.UpdateFarmInfo(
                CurrentMonthProduction,
                NextMonthExpectedYield,
                Manager
            );
            DebugTool.LogFarm("UI已更新: 本月产量 {0:F2}, 下月预计 {1:F2}, 管理者 {2}",
                CurrentMonthProduction, NextMonthExpectedYield, Manager?.personName ?? "无");
        }
        else
        {
            DebugTool.LogWarning("FarmSystem", "uiController 为 null，无法更新UI");
        }

        DebugTool.LogFarm("UpdateUI() 完成");
    }

    // 供存档系统使用的公共方法
    public List<FieldUnit> GetFields()
    {
        DebugTool.LogFarm("GetFields() 调用，返回 {0} 个田地", fields.Count);
        return fields;
    }

    public PersonScriptableObject GetManager()
    {
        DebugTool.LogFarm("GetManager() 调用，返回管理者: {0}", Manager?.personName ?? "无");
        return Manager;
    }
}
