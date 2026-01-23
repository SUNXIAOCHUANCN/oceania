using UnityEngine;

/// <summary>
/// 探索系统测试脚本 - 用于调试和验证
/// </summary>
public class ExploreSystemTest : MonoBehaviour
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
        // 等待2秒，确保所有系统初始化完成
        yield return new WaitForSeconds(2f);

        Debug.Log("========== 探索系统测试 ==========");

        // 测试 1: 检查 ExploreSystem
        if (ExploreSystem.Instance == null)
        {
            Debug.LogError("❌ ExploreSystem.Instance 为空！");
        }
        else
        {
            Debug.Log("✅ ExploreSystem.Instance 存在");

            var managers = ExploreSystem.Instance.GetAllProgressTableManagers();
            Debug.Log($"✅ 找到 {managers.Count} 个 ProgressTableManager");

            if (managers.Count == 0)
            {
                Debug.LogError("❌ 没有找到任何 ProgressTableManager！");
            }
            else
            {
                foreach (var manager in managers)
                {
                    Debug.Log($"  - {manager.GetIslandName()}");
                }
            }
        }

        // 测试 2: 检查 ExploreSaveSystem
        if (ExploreSaveSystem.Instance == null)
        {
            Debug.LogError("❌ ExploreSaveSystem.Instance 为空！");
        }
        else
        {
            Debug.Log("✅ ExploreSaveSystem.Instance 存在");

            bool hasSave = ExploreSaveSystem.Instance.SaveExists();
            Debug.Log($"✅ 存档文件存在: {hasSave}");

            if (hasSave)
            {
                var saveData = ExploreSaveSystem.Instance.LoadExploreData();
                Debug.Log($"✅ 存档包含 {saveData.islandsExploreData.Count} 个岛屿");
            }
            else
            {
                Debug.Log("ℹ️  存档文件不存在（首次运行正常）");
            }
        }

        // 测试 3: 检查 GlobalSaveManager
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
        if (ExploreSystem.Instance != null && ExploreSaveSystem.Instance != null)
        {
            var data = ExploreSystem.Instance.ExportToSaveData();
            ExploreSaveSystem.Instance.SaveExploreData(data);
            Debug.Log($"✅ 强制保存完成，导出 {data.islandsExploreData.Count} 个岛屿");
        }
        else
        {
            Debug.LogError("❌ 系统未初始化");
        }
    }

    [ContextMenu("强制加载")]
    public void ForceLoad()
    {
        if (ExploreSystem.Instance != null && ExploreSaveSystem.Instance != null)
        {
            var data = ExploreSaveSystem.Instance.LoadExploreData();
            ExploreSystem.Instance.LoadFromSaveData(data);
            Debug.Log($"✅ 强制加载完成，加载 {data.islandsExploreData.Count} 个岛屿");
        }
        else
        {
            Debug.LogError("❌ 系统未初始化");
        }
    }

    [ContextMenu("删除存档")]
    public void DeleteSave()
    {
        if (ExploreSaveSystem.Instance != null)
        {
            ExploreSaveSystem.Instance.DeleteSave();
            Debug.Log("✅ 存档已删除");
        }
        else
        {
            Debug.LogError("❌ ExploreSaveSystem 未初始化");
        }
    }
}
