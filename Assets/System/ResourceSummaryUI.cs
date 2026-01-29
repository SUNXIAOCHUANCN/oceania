using UnityEngine;
using TMPro;

/// <summary>
/// 资源摘要UI组件 - 显示每月资源变化信息
/// </summary>
public class ResourceSummaryUI : MonoBehaviour
{
    [Header("UI引用 - 当前资源")]
    [SerializeField] private TextMeshProUGUI currentCropText;
    [SerializeField] private TextMeshProUGUI currentAnimalText;
    [SerializeField] private TextMeshProUGUI currentMaterialText;

    [Header("UI引用 - 本月净增长")]
    [SerializeField] private TextMeshProUGUI currentMonthCropText;
    [SerializeField] private TextMeshProUGUI currentMonthAnimalText;
    [SerializeField] private TextMeshProUGUI currentMonthMaterialText;

    [Header("UI引用 - 环比变化")]
    [SerializeField] private TextMeshProUGUI monthOverMonthCropText;
    [SerializeField] private TextMeshProUGUI monthOverMonthAnimalText;
    [SerializeField] private TextMeshProUGUI monthOverMonthMaterialText;

    [Header("颜色设置")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;

    private void Start()
    {
        DebugTool.LogResourceSummary("Start() 开始执行");

        // 订阅资源计算器事件
        if (ResourceManagerCalculator.Instance != null)
        {
            ResourceManagerCalculator.Instance.OnResourceSummaryUpdated += UpdateUI;

            // 立即更新一次UI
            UpdateUI(ResourceManagerCalculator.Instance.CurrentSummary);
            DebugTool.LogResourceSummary("已订阅资源计算器事件并更新UI");
        }
        else
        {
            DebugTool.LogWarning("ResourceSummaryUI", "ResourceManagerCalculator.Instance 为 null");
        }

        DebugTool.LogResourceSummary("Start() 完成");
    }

    private void OnDestroy()
    {
        DebugTool.LogResourceSummary("OnDestroy() 开始执行");

        // 取消订阅
        if (ResourceManagerCalculator.Instance != null)
        {
            ResourceManagerCalculator.Instance.OnResourceSummaryUpdated -= UpdateUI;
            DebugTool.LogResourceSummary("已取消订阅资源计算器事件");
        }

        DebugTool.LogResourceSummary("OnDestroy() 完成");
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI(ResourceManagerCalculator.MonthlyResourceSummary summary)
    {
        if (summary == null)
        {
            DebugTool.LogWarning("ResourceSummaryUI", "资源摘要为 null");
            return;
        }

        DebugTool.LogResourceSummary("UpdateUI() 更新资源摘要显示");

        // 更新当前资源
        UpdateResourceText(currentCropText, summary.currentResources.crop, isAbsolute: true);
        UpdateResourceText(currentAnimalText, summary.currentResources.animal, isAbsolute: true);
        UpdateResourceText(currentMaterialText, summary.currentResources.material, isAbsolute: true);

        // 更新本月净增长
        UpdateResourceText(currentMonthCropText, summary.currentMonthNetGrowth.crop);
        UpdateResourceText(currentMonthAnimalText, summary.currentMonthNetGrowth.animal);
        UpdateResourceText(currentMonthMaterialText, summary.currentMonthNetGrowth.material);

        // 更新环比变化
        UpdateResourceText(monthOverMonthCropText, summary.monthOverMonthChange.crop, showArrow: true);
        UpdateResourceText(monthOverMonthAnimalText, summary.monthOverMonthChange.animal, showArrow: true);
        UpdateResourceText(monthOverMonthMaterialText, summary.monthOverMonthChange.material, showArrow: true);
    }

    /// <summary>
    /// 更新单个资源文本
    /// </summary>
    private void UpdateResourceText(TextMeshProUGUI textComponent, float value, bool isAbsolute = false, bool showArrow = false)
    {
        if (textComponent == null) return;

        string displayStr = "";

        if (isAbsolute)
        {
            // 绝对值显示 - 整数位
            displayStr = Mathf.Floor(value).ToString("F0");
            textComponent.color = neutralColor;
        }
        else if (showArrow)
        {
            // 环比变化显示 (带箭头) - 小数点后1位
            string arrow = value > 0 ? "↑" : (value < 0 ? "↓" : "→");
            displayStr = $"{arrow} {Mathf.Abs(value):F1}";
            textComponent.color = value > 0 ? positiveColor : (value < 0 ? negativeColor : neutralColor);
        }
        else
        {
            // 净增长显示 (带正负号) - 小数点后1位
            string sign = value >= 0 ? "+" : "";
            displayStr = $"{sign}{value:F1}";
            textComponent.color = value > 0 ? positiveColor : (value < 0 ? negativeColor : neutralColor);
        }

        textComponent.text = displayStr;
    }

    /// <summary>
    /// 手动刷新UI (供外部调用)
    /// </summary>
    public void RefreshUI()
    {
        DebugTool.LogResourceSummary("RefreshUI() 手动刷新UI");
        if (ResourceManagerCalculator.Instance != null)
        {
            ResourceManagerCalculator.Instance.CalculateResourceSummary();
            DebugTool.LogResourceSummary("已触发资源摘要计算");
        }
    }
}
