using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FarmSystem : MonoBehaviour
{
    [Header("田地配置")]
    [SerializeField] private List<FieldUnit> fields = new List<FieldUnit>();
    [SerializeField] private int totalFields = 15;
    
    [Header("UI引用")]
    [SerializeField] private FarmUIController uiController;
    
    public PersonScriptableObject Manager { get; private set; }
    public float CurrentMonthProduction { get; private set; }
    public float NextMonthExpectedYield { get; private set; }
    
    public UnityEvent<float> OnProductionCalculated = new UnityEvent<float>();
    public UnityEvent<PersonScriptableObject> OnManagerChanged = new UnityEvent<PersonScriptableObject>();
    
    private void Awake()
    {
        InitializeFields();
    }
    
    private void Start()
    {
        // 尝试加载存档
        if (FarmSaveSystem.Instance.SaveExists())
        {
            FarmSaveSystem.Instance.LoadFarmData(this);
        }
        
        // 计算下月预计产量
        CalculateNextMonthExpectedYield();
        
        // 更新UI
        UpdateUI();
    }
    
    private void OnEnable()
    {
        // 订阅月相变化事件
        GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += HandlePhaseChange;
    }
    
    private void OnDisable()
    {
        // 取消订阅月相变化事件
        GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= HandlePhaseChange;
    }
    
    private void OnApplicationQuit()
    {
        // 游戏退出时保存数据
        FarmSaveSystem.Instance.SaveFarmData(this);
    }
    
    /// <summary>
    /// 初始化田地列表
    /// </summary>
    private void InitializeFields()
    {
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
        }
    }
    
    /// <summary>
    /// 处理月相变化
    /// </summary>
    private void HandlePhaseChange(GlobalTimeSystem.MoonPhase phase, int phaseCount)
    {
        // 第一阶段：获取本月产量
        CurrentMonthProduction = 0;
        
        foreach (FieldUnit field in fields)
        {
            field.HandlePhaseChange();
            // 通知田地"农场已读取产量"
            float yield = field.GetAndLockCurrentYield();
            CurrentMonthProduction += yield;
            field.UpdateNPY();
        }
        
        // 应用管理者加成
        float finalProduction = CurrentMonthProduction;
        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            finalProduction *= 1.2f;
        }
        
        // 更新资源管理器
        ResourceManager.Instance.AddCrop(finalProduction);
        
        // 第二阶段：获取下月预计产量
        CalculateNextMonthExpectedYield();
        
        // 通知UI更新
        UpdateUI();
        
        // 触发生产事件
        OnProductionCalculated?.Invoke(finalProduction);
    }
    
    /// <summary>
    /// 计算下个月预计产量
    /// </summary>
    public void CalculateNextMonthExpectedYield()
    {
        NextMonthExpectedYield = 0;
        
        foreach (FieldUnit field in fields)
        {
            if (field.IsUnlocked && field.CurrentCrop != null && field.IsProducing)
            {
                NextMonthExpectedYield += field.CurrentNPY;
            }
        }
        
        // 应用管理者加成
        if (Manager != null && Manager.profession == PersonProfession.farmer)
        {
            NextMonthExpectedYield *= 1.2f;
        }
    }
    
    /// <summary>
    /// 设置管理者
    /// </summary>
    public bool SetManager(PersonScriptableObject person)
    {
        if (person == null || !person.recruited || 
            (person.status != PersonStatus.rest && person.status != PersonStatus.infarm))
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
        PersonManager.Instance.ChangePersonStatus(person, PersonStatus.inManagerFarm);
        
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
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        if (uiController != null)
        {
            uiController.UpdateFarmInfo(
                CurrentMonthProduction, 
                NextMonthExpectedYield, 
                Manager
            );
        }
    }
    
    // 供存档系统使用的公共方法
    public List<FieldUnit> GetFields() => fields;
    public PersonScriptableObject GetManager() => Manager;
}