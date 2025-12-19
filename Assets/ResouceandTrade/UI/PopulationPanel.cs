using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

// 人口面板：左侧显示人口簿（姓名 / 职业 / 每月消耗），右侧为招募栏（列出 availableRoles 并提供招募按钮）
public class PopulationPanel : MonoBehaviour
{
    [Header("Left - People List")]
    public RectTransform listContainer;
    public GameObject rowPrefab; // 可包含 Text 组件

    [Header("Right - Recruit List")]
    public RectTransform recruitContainer;
    public GameObject recruitRowPrefab; // 可包含 Text + Button

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        RefreshPeopleList();
        RefreshRecruitList();
    }

    private void RefreshPeopleList()
    {
        if (listContainer == null) return;
        for (int i = listContainer.childCount - 1; i >= 0; i--) Destroy(listContainer.GetChild(i).gameObject);
        if (PopulationManager.Instance == null) return;

        foreach (var p in PopulationManager.Instance.allPeople)
        {
            if (p == null) continue;
            string personName = (p.personName != null) ? p.personName : "Unnamed";
            string roleName = (p.role != null) ? p.role.roleName : "NoRole";

            // 计算该人每月消耗字符串：基础 + 职业周期性消耗
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("pc:3 pl:2 pm:1");

            if (p.role != null && p.role.durationalConsumption != null && p.role.durationalConsumption.Count > 0)
            {
                sb.Append(" | 额外: ");
                var extras = p.role.durationalConsumption.Select(rc => $"{rc.resourceName}:{rc.amount}");
                sb.Append(string.Join(", ", extras));
            }

            string line = $"{personName} - {roleName} - {p.currentStatus} - {sb.ToString()}";

            if (rowPrefab != null)
            {
                var go = Instantiate(rowPrefab, listContainer);
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = line;
            }
            else
            {
                var row = new GameObject("PersonRow");
                row.transform.SetParent(listContainer, false);
                var txt = row.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.text = line;
                txt.color = Color.black;
                txt.fontSize = 14;
            }
        }
    }

    private void RefreshRecruitList()
    {
        if (recruitContainer == null) return;
        for (int i = recruitContainer.childCount - 1; i >= 0; i--) Destroy(recruitContainer.GetChild(i).gameObject);
        if (PopulationManager.Instance == null) return;

        foreach (var role in PopulationManager.Instance.availableRoles)
        {
            if (role == null) continue;
            string roleText = role.roleName ?? "Role";

            // 招募费用文本
            string costText = "Cost: ";
            if (role.RecruitmentCosts != null && role.RecruitmentCosts.Count > 0)
            {
                costText += string.Join(", ", role.RecruitmentCosts.Select(rc => $"{rc.resourceName}:{rc.amount}"));
            }
            else costText += "(none)";

            if (recruitRowPrefab != null)
            {
                var go = Instantiate(recruitRowPrefab, recruitContainer);
                var texts = go.GetComponentsInChildren<Text>();
                if (texts.Length > 0) texts[0].text = roleText;
                if (texts.Length > 1) texts[1].text = costText;

                var btn = go.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    RoleScriptableObject captured = role;
                    btn.onClick.AddListener(() => { OnRecruitClicked(captured); });
                }
            }
            else
            {
                var row = new GameObject("RecruitRow");
                row.transform.SetParent(recruitContainer, false);

                var txt = row.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.text = roleText + " - " + costText;
                txt.color = Color.black;
                txt.fontSize = 14;

                var btnGO = new GameObject("RecruitBtn");
                btnGO.transform.SetParent(row.transform, false);
                var img = btnGO.AddComponent<Image>();
                img.color = Color.grey;
                var btn = btnGO.AddComponent<Button>();
                var btnTextGO = new GameObject("Text");
                btnTextGO.transform.SetParent(btnGO.transform, false);
                var bt = btnTextGO.AddComponent<Text>();
                bt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                bt.text = "招募";
                bt.color = Color.white;

                RoleScriptableObject captured = role;
                btn.onClick.AddListener(() => { OnRecruitClicked(captured); });
            }
        }
    }

    private void OnRecruitClicked(RoleScriptableObject role)
    {
        if (PopulationManager.Instance == null || role == null) return;
        int got = PopulationManager.Instance.RecruitNewPerson(role, 1);
        if (got > 0)
        {
            Refresh();
        }
    }
}
