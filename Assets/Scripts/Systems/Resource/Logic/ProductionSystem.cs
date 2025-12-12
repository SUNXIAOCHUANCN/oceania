using UnityEngine;
using System.Collections;

public class ProductionUnit : MonoBehaviour
{
    [Header("配置")]
    public ResourceScriptableObject currentCrop; // 当前种的什么
    public bool autoProduce = true; // 初级目标：自动种植

    private float timer = 0f;
    private bool isProducing = false;

    private void Start()
    {
        if (currentCrop != null && autoProduce)
        {
            StartProduction();
        }
    }

    private void Update()
    {
        if (isProducing)
        {
            timer += Time.deltaTime;

            // 这里可以添加进度条UI更新逻辑

            if (timer >= currentCrop.growthTime)
            {
                Harvest();
            }
        }
    }

    public void StartProduction()
    {
        timer = 0f;
        isProducing = true;
        Debug.Log($"开始种植: {currentCrop.resourceName}");
    }

    // 收获逻辑
    private void Harvest()
    {
        // 1. 获取当前资源的状态（查一下基因质量）
        ResourceSlot slot = ResourceManager.Instance.GetResourceSlot(currentCrop.resourceName);

        if (slot != null)
        {
            // 2. 计算产量
            int yieldAmount = slot.GetCurrentYield();

            // 3. 加到总库存
            ResourceManager.Instance.AddResource(currentCrop.resourceName, yieldAmount);

            // 4. 触发退化 (关键机制)
            slot.Degrade();
            Debug.Log($"{currentCrop.resourceName} 收获了! 产量: {yieldAmount}, 基因质量下降为: {slot.qualityMultiplier}");
        }

        // 5. 自动开始下一轮
        StartProduction();
    }

    // 玩家切换作物的逻辑 (对应需求：切换作物理度作废)
    public void ChangeCrop(ResourceScriptableObject newCrop)
    {
        if (currentCrop == newCrop) return;

        currentCrop = newCrop;
        // 进度作废，重置
        StopAllCoroutines();
        StartProduction();
        Debug.Log("更换作物，进度重置");
    }
}