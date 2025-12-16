using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 贸易面板：列出所有未获得的野生变种，并提供交易/驯化操作（简化实现）
public class TradePanel : MonoBehaviour
{
    public RectTransform listContainer;
    public GameObject rowPrefab; // 包含 Text + Button(s)

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (listContainer == null || rowPrefab == null) return;
        for (int i = listContainer.childCount - 1; i >= 0; i--) Destroy(listContainer.GetChild(i).gameObject);
        if (ResourceManager.Instance == null) return;

        // 找出所有 knownResources 中的 VariantScriptableObject，如果玩家尚未拥有则列出
        foreach (var r in ResourceManager.Instance.knownResources)
        {
            if (r is VariantScriptableObject v)
            {
                var slot = ResourceManager.Instance.GetResourceSlot(v.resourceName);
                float have = slot != null ? slot.amount : 0f;
                if (have > 0f) continue;

                if (rowPrefab != null)
                {
                    var go = Instantiate(rowPrefab, listContainer);
                    var txt = go.GetComponentInChildren<Text>();
                    if (txt != null) txt.text = v.resourceName + " (" + v.variantType + ")";

                    var btns = go.GetComponentsInChildren<Button>();
                    if (btns.Length > 0)
                    {
                        btns[0].onClick.AddListener(() => OnTameClicked(v));
                    }
                }
                else
                {
                    var row = new GameObject("VariantRow");
                    row.transform.SetParent(listContainer, false);
                    var txt = row.AddComponent<Text>();
                    txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    txt.text = v.resourceName + " (" + v.variantType + ")";
                    txt.color = Color.black;
                    txt.fontSize = 14;
                }
            }
        }
    }

    private void OnTameClicked(VariantScriptableObject wild)
    {
        if (VariantManager.Instance == null)
        {
            Debug.LogWarning("VariantManager 未就绪，无法驯化");
            return;
        }
        bool ok = VariantManager.Instance.TameWildVariant(wild);
        if (ok) Refresh();
    }
}
