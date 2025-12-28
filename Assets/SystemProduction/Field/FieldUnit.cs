using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;



public class FieldUnit : MonoBehaviour
{
    [Header("状态")]
    [SerializeField] private bool isUnlocked = false;
    [SerializeField] private int cultivationProgress = 0;
    [SerializeField] private float currentNPY = 0;
    
    [Header("开垦工人")]
    [SerializeField] private PersonScriptableObject currentWorker;
    
    [Header("作物信息")]
    [SerializeField] private SpeciesScriptableObject currentCrop;
    [SerializeField] private int growthPhaseCount = 0;
    
    public bool IsUnlocked => isUnlocked;
    public PersonScriptableObject CurrentWorker => currentWorker;
    public SpeciesScriptableObject CurrentCrop => currentCrop;
    public float CurrentNPY => currentNPY;
    public int GrowthPhaseCount => growthPhaseCount;
    public bool IsProducing => isUnlocked && currentCrop != null && growthPhaseCount >= currentCrop.growthPhases;
    
    public UnityEvent<SpeciesScriptableObject> OnCropPlanted = new UnityEvent<SpeciesScriptableObject>();
    public UnityEvent<float> OnYieldUpdated = new UnityEvent<float>();
    
    // 用于处理"农场已读取产量"的临时变量
    private bool yieldLocked = false;
    private float yieldWhenLocked = 0;

    // 新增：记录每种作物在该田地上最后种植时的NPY
    private Dictionary<string, float> cropLastNPY = new Dictionary<string, float>();
    
    /// <summary>
    /// 开垦田地
    /// </summary>
    public bool StartCultivation()
    {
        if (isUnlocked)
        {
            return false;
        }
        
        cultivationProgress = 1;
        yieldLocked = false;
        return true;
    }
    
    /// <summary>
    /// 设置开垦工人
    /// </summary>
    /// <param name="worker">工人</param>
    public void SetWorker(PersonScriptableObject worker)
    {
        currentWorker = worker;
        Debug.Log($"田地 {name} 设置开垦工人: {worker?.personName}");
    }
    
    /// <summary>
    /// 清除开垦工人
    /// </summary>
    public void ClearWorker()
    {
        if (currentWorker != null)
        {
            Debug.Log($"田地 {name} 清除开垦工人: {currentWorker.personName}");
            currentWorker = null;
        }
    }
    
    /// <summary>
    /// 完成开垦
    /// </summary>
    public void CompleteCultivation()
    {
        // 如果有关联的工人，将其状态重置为休息
        if (currentWorker != null && PersonManager.Instance != null)
        {
            PersonManager.Instance.ChangePersonStatus(currentWorker, PersonStatus.rest);
            Debug.Log($"田地 {name} 开垦完成，工人 {currentWorker.personName} 状态重置为休息");
        }
        
        isUnlocked = true;
        cultivationProgress = 0;
        
        // 清除工人引用
        ClearWorker();
    }
    
    /// <summary>
    /// 种植作物
    /// </summary>
    public bool PlantCrop(SpeciesScriptableObject crop, int growthPhaseCount = 0)
    {
        if (!isUnlocked || crop == null || crop.speciesType != SpeciesType.Crop)
        {
            return false;
        }
        
        // 如果当前有作物，保存其NPY到字典
        if (currentCrop != null)
        {
            cropLastNPY[currentCrop.speciesName] = currentNPY;
        }
        
        // 检查字典中是否有该作物的记录
        string cropName = crop.speciesName;
        if (cropLastNPY.TryGetValue(cropName, out float lastNPY))
        {
            // 使用上次记录的NPY
            currentNPY = lastNPY;
            Debug.Log($"田地 {name} 种植 {cropName}，使用上次记录的NPY: {currentNPY}");
        }
        else
        {
            // 使用初始产量
            currentNPY = crop.initialYield;
            Debug.Log($"田地 {name} 首次种植 {cropName}，使用初始NPY: {currentNPY}");
        }
        
        currentCrop = crop;
        this.growthPhaseCount = growthPhaseCount;
        
        yieldLocked = false;
        
        OnCropPlanted?.Invoke(crop);
        return true;
    }
    
    /// <summary>
    /// 通知田地：农场已读取产量
    /// </summary>
    public float GetAndLockCurrentYield()
    {
        if (yieldLocked || !IsProducing)
        {
            return 0;
        }
        
        // 锁定当前产量，防止重复计算
        yieldWhenLocked = currentNPY;
        yieldLocked = true;
        
        return yieldWhenLocked;
    }
    
    /// <summary>
    /// 通知田地：已更新NPY
    /// </summary>
    public void UpdateNPY()
    {
        if (!yieldLocked || !IsProducing)
        {
            return;
        }
        
        // 更新NPY
        currentNPY = Mathf.Max(
            currentNPY - currentCrop.decayPerPhase, 
            currentCrop.leastYield
        );
        
        // 增加生长阶段
        growthPhaseCount++;
        
        // 保存当前作物的NPY到字典
        if (currentCrop != null)
        {
            cropLastNPY[currentCrop.speciesName] = currentNPY;
        }
        
        yieldLocked = false;
        OnYieldUpdated?.Invoke(currentNPY);
    }
    
    /// <summary>
    /// 处理月相变化
    /// </summary>
    public void HandlePhaseChange()
    {
        Debug.Log($"Field HandlePhaseChange - isUnlocked: {isUnlocked}, hasCrop: {currentCrop != null}");
        if (!isUnlocked)
        {
            // 如果正在开垦，增加进度
            if (cultivationProgress > 0)
            {
                cultivationProgress++;
                if (cultivationProgress >= 2)
                {
                    CompleteCultivation();
                }
            }
        }
        else if (currentCrop != null)
        {
            // 如果有作物，增加生长阶段
            growthPhaseCount++;
            Debug.Log($"Growth phase increased to: {growthPhaseCount}");
        }
    }
    
    // 供存档系统使用的公共方法
    public void SetUnlocked(bool unlocked) => isUnlocked = unlocked;
    public void SetCultivationProgress(int progress) => cultivationProgress = progress;
    public void SetGrowthPhaseCount(int count) => growthPhaseCount = count;
    public void SetCurrentNPY(float npy) => currentNPY = npy;
    // 新增：供存档系统使用的作物NPY字典方法
    public Dictionary<string, float> GetCropLastNPY() => cropLastNPY;
    
    public void SetCropLastNPY(Dictionary<string, float> newCropLastNPY)
    {
        cropLastNPY = newCropLastNPY;
        Debug.Log($"田地 {name} 加载了作物NPY记录，共 {cropLastNPY.Count} 条记录");
    }
}