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

    [Header("Panels")]
    public GameObject productionPanel;
    public GameObject resourcePanel;
    public GameObject populationPanel;

    private void Start()
    {
        if (productionButton != null) productionButton.onClick.AddListener(() => ShowPanel(PanelType.Production));
        if (resourceButton != null) resourceButton.onClick.AddListener(() => ShowPanel(PanelType.Resource));
        if (populationButton != null) populationButton.onClick.AddListener(() => ShowPanel(PanelType.Population));

        // 默认显示生产面板（可根据需要改为隐藏所有）
        ShowPanel(PanelType.Production);
    }

    public enum PanelType { Production, Resource, Population }

    public void ShowPanel(PanelType type)
    {
        if (productionPanel != null) productionPanel.SetActive(type == PanelType.Production);
        if (resourcePanel != null) resourcePanel.SetActive(type == PanelType.Resource);
        if (populationPanel != null) populationPanel.SetActive(type == PanelType.Population);
    }
}
