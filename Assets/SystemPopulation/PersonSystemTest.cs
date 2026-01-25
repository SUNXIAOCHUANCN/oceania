using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 人口系统测试脚本 - 用于调试和验证
/// </summary>
public class PersonSystemTest : MonoBehaviour
{
    [Header("测试选项")]
    [SerializeField] private bool autoTestOnStart = true;

    private void Start()
    {
        if (autoTestOnStart)
        {
            StartCoroutine(TestAfterDelay());
        }
    }

    private System.Collections.IEnumerator TestAfterDelay()
    {
        // 等待3秒，确保所有系统都已初始化并加载存档
        yield return new WaitForSeconds(3f);

        Debug.Log("========== 人口系统测试 ==========");

        // 测试 1: 检查 PersonManager
        if (PersonManager.Instance == null)
        {
            Debug.LogError("❌ PersonManager.Instance 为空！");
        }
        else
        {
            Debug.Log("✅ PersonManager.Instance 存在");

            var allPersons = PersonManager.Instance.GetAllPersons();
            var recruitedPersons = PersonManager.Instance.GetRecruitedPersons();

            Debug.Log($"✅ 总人数: {allPersons.Count}");
            Debug.Log($"✅ 已招募: {recruitedPersons.Count}");

            if (recruitedPersons.Count > 0)
            {
                Debug.Log("  已招募人员列表:");
                foreach (var person in recruitedPersons)
                {
                    Debug.Log($"    - {person.personName} ({person.status})");
                }
            }
        }

        // 测试 2: 检查 PersonSaveSystem
        if (PersonSaveSystem.Instance == null)
        {
            Debug.LogError("❌ PersonSaveSystem.Instance 为空！");
        }
        else
        {
            Debug.Log("✅ PersonSaveSystem.Instance 存在");

            bool hasSave = PersonSaveSystem.Instance.SaveExists();
            Debug.Log($"✅ 存档文件存在: {hasSave}");

            if (hasSave)
            {
                var saveData = PersonSaveSystem.Instance.LoadPopulationData();
                Debug.Log($"✅ 存档包含 {saveData.personsData.Count} 个人员");

                int recruitedInSave = 0;
                foreach (var personData in saveData.personsData)
                {
                    if (personData.isRecruited) recruitedInSave++;
                }
                Debug.Log($"✅ 存档中已招募: {recruitedInSave}");
            }
            else
            {
                Debug.Log("ℹ️  存档文件不存在（首次运行正常）");
            }
        }

        // 测试 3: 检查 PersonsLoader
        if (PersonsLoader.Instance == null)
        {
            Debug.LogError("❌ PersonsLoader.Instance 为空！");
        }
        else
        {
            Debug.Log("✅ PersonsLoader.Instance 存在");
        }

        // 测试 4: 检查 GlobalSaveManager
        if (GlobalSaveManager.Instance == null)
        {
            Debug.LogError("❌ GlobalSaveManager.Instance 为空！");
        }
        else
        {
            Debug.Log("✅ GlobalSaveManager.Instance 存在");
        }

        Debug.Log("========== 测试完成 ==========");
    }

    [ContextMenu("手动测试")]
    public void ManualTest()
    {
        StartCoroutine(TestAfterDelay());
    }

    [ContextMenu("强制保存")]
    public void ForceSave()
    {
        if (PersonManager.Instance != null && PersonSaveSystem.Instance != null)
        {
            var data = PersonManager.Instance.ExportToSaveData();
            PersonSaveSystem.Instance.SavePopulationData(data);
            Debug.Log($"✅ 强制保存完成，导出 {data.personsData.Count} 个人员");
        }
        else
        {
            Debug.LogError("❌ 系统未初始化");
        }
    }

    [ContextMenu("强制加载")]
    public void ForceLoad()
    {
        if (PersonManager.Instance != null && PersonSaveSystem.Instance != null)
        {
            var data = PersonSaveSystem.Instance.LoadPopulationData();
            PersonManager.Instance.LoadFromSaveData(data);
            Debug.Log($"✅ 强制加载完成，加载 {data.personsData.Count} 个人员");
        }
        else
        {
            Debug.LogError("❌ 系统未初始化");
        }
    }

    [ContextMenu("删除存档")]
    public void DeleteSave()
    {
        if (PersonSaveSystem.Instance != null)
        {
            PersonSaveSystem.Instance.DeleteSave();
            Debug.Log("✅ 存档已删除");
        }
        else
        {
            Debug.LogError("❌ PersonSaveSystem 未初始化");
        }
    }

    [ContextMenu("显示所有人员状态")]
    public void ShowAllPersonStatus()
    {
        if (PersonManager.Instance == null)
        {
            Debug.LogError("❌ PersonManager 未初始化");
            return;
        }

        var allPersons = PersonManager.Instance.GetAllPersons();
        Debug.Log($"========== 人员状态 (共 {allPersons.Count} 人) ==========");

        foreach (var person in allPersons)
        {
            string status = person.recruited ? $"已招募 - {person.status}" : "未招募";
            Debug.Log($"  {person.personName}: {status}");
        }

        Debug.Log("================================");
    }
}
