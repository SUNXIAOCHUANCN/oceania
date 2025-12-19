using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 管理器：管理同一 Canvas 下的面板切换
/// 在 Canvas 上创建三个 Button，拖入本脚本对应字段，然后把三个面板拖入对应字段。
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button productionButton;
    public Button resourceButton;
    public Button populationButton;
    public Button tradeButton;

    [Header("Panels")]
    public GameObject productionPanel;
    public GameObject resourcePanel;
    public GameObject populationPanel;
    public GameObject tradePanel;

    private void Start()
    {
        if (productionButton != null) productionButton.onClick.AddListener(() => ShowPanel(PanelType.Production));
        if (resourceButton != null) resourceButton.onClick.AddListener(() => ShowPanel(PanelType.Resource));
        if (populationButton != null) populationButton.onClick.AddListener(() => ShowPanel(PanelType.Population));
        if (tradeButton != null) tradeButton.onClick.AddListener(() => ShowPanel(PanelType.Trade));

        // 默认显示生产面板（可根据需要改为隐藏所有）
        ShowPanel(PanelType.Production);
    }

    public enum PanelType { Production, Resource, Population, Trade }

    public void ShowPanel(PanelType type)
    {
        if (productionPanel != null) productionPanel.SetActive(type == PanelType.Production);
        if (resourcePanel != null) resourcePanel.SetActive(type == PanelType.Resource);
        if (populationPanel != null) populationPanel.SetActive(type == PanelType.Population);
        if (tradePanel != null) tradePanel.SetActive(type == PanelType.Trade);

        // 尝试在被显示的面板上调用 Refresh 方法（如果该面板实现了 Refresh）
        GameObject shown = null;
        switch (type)
        {
            case PanelType.Production: shown = productionPanel; break;
            case PanelType.Resource: shown = resourcePanel; break;
            case PanelType.Population: shown = populationPanel; break;
            case PanelType.Trade: shown = tradePanel; break;
        }
        if (shown != null)
        {
            // 使用 SendMessage 安全调用，不要求目标一定实现该方法
            shown.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
        }
    }
}
