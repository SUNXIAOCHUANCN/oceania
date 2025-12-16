using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 简单的资源面板脚本：展示 knownResources 列表与三类资源总量
public class ResourcePanel : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform listContainer; // 用于动态创建行（可选）
    public GameObject listRowPrefab;    // 每一行的预制（可包含 Text/TMP）

    public Text cropsAmountText;
    public Text livestockAmountText;
    public Text materialAmountText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (ResourceManager.Instance == null) return;

        // 更新三类资源总量
        cropsAmountText.text = ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Crop).ToString("F1");
        livestockAmountText.text = ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Livestock).ToString("F1");
        materialAmountText.text = ResourceManager.Instance.GetCategoryAmount(ResourceCategory.Material).ToString("F1");

        // 列表展示已知资源（简单文本行）
        if (listContainer == null) return;
        // 清空已有行
        for (int i = listContainer.childCount - 1; i >= 0; i--) Destroy(listContainer.GetChild(i).gameObject);

        foreach (var res in ResourceManager.Instance.knownResources)
        {
            if (res == null) continue;
            if (listRowPrefab != null)
            {
                var go = Instantiate(listRowPrefab, listContainer);
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = res.resourceName;
            }
            else
            {
                var row = new GameObject("ResRow");
                row.transform.SetParent(listContainer, false);
                var txt = row.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.text = res.resourceName;
                txt.color = Color.black;
                txt.fontSize = 14;
            }
        }
    }
}
