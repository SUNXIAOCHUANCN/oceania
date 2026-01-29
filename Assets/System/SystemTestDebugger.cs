using UnityEngine;

/// <summary>
/// 临时调试脚本：检查生产系统的状态
/// </summary>
public class SystemTestDebugger : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("========================================");
        Debug.Log("🔍 开始检查生产系统状态");
        Debug.Log("========================================");

        // 检查 ForestSystem
        ForestSystem forestSystem = FindObjectOfType<ForestSystem>();
        if (forestSystem != null)
        {
            Debug.Log($"✅ ForestSystem 找到了");
            Debug.Log($"   - GameObject 激活状态: {forestSystem.gameObject.activeInHierarchy}");
            Debug.Log($"   - 组件启用状态: {forestSystem.enabled}");
            Debug.Log($"   - GlobalTimeSystem.Instance: {GlobalTimeSystem.Instance != null}");
        }
        else
        {
            Debug.LogError("❌ ForestSystem 未找到！可能 GameObject 被禁用或不存在");
        }

        // 检查 RanchSystem
        RanchSystem ranchSystem = FindObjectOfType<RanchSystem>();
        if (ranchSystem != null)
        {
            Debug.Log($"✅ RanchSystem 找到了");
            Debug.Log($"   - GameObject 激活状态: {ranchSystem.gameObject.activeInHierarchy}");
            Debug.Log($"   - 组件启用状态: {ranchSystem.enabled}");
            Debug.Log($"   - GlobalTimeSystem.Instance: {GlobalTimeSystem.Instance != null}");
        }
        else
        {
            Debug.LogError("❌ RanchSystem 未找到！可能 GameObject 被禁用或不存在");
        }

        // 检查 FarmSystem
        FarmSystem farmSystem = FindObjectOfType<FarmSystem>();
        if (farmSystem != null)
        {
            Debug.Log($"✅ FarmSystem 找到了");
            Debug.Log($"   - GameObject 激活状态: {farmSystem.gameObject.activeInHierarchy}");
            Debug.Log($"   - 组件启用状态: {farmSystem.enabled}");
        }
        else
        {
            Debug.LogError("❌ FarmSystem 未找到！");
        }

        // 检查 GlobalTimeSystem
        if (GlobalTimeSystem.Instance != null)
        {
            Debug.Log($"✅ GlobalTimeSystem.Instance 存在");
            Debug.Log($"   - GameObject 激活状态: {GlobalTimeSystem.Instance.gameObject.activeInHierarchy}");
            Debug.Log($"   - 组件启用状态: {GlobalTimeSystem.Instance.enabled}");
        }
        else
        {
            Debug.LogError("❌ GlobalTimeSystem.Instance 为 null！");
        }

        Debug.Log("========================================");
        Debug.Log("🔍 检查完成");
        Debug.Log("========================================");

        // 5秒后再次检查（确保所有 Start 都执行完毕）
        Invoke("DelayedCheck", 5f);
    }

    private void DelayedCheck()
    {
        Debug.Log("========================================");
        Debug.Log("🔍 延迟检查（5秒后）");
        Debug.Log("========================================");

        ForestSystem forestSystem = FindObjectOfType<ForestSystem>();
        RanchSystem ranchSystem = FindObjectOfType<RanchSystem>();

        if (forestSystem != null && GlobalTimeSystem.Instance != null)
        {
            Debug.Log($"📊 森林系统当前总数量: {forestSystem.CurrentTotalAmount}");
        }

        if (ranchSystem != null && GlobalTimeSystem.Instance != null)
        {
            Debug.Log($"📊 牧场系统当前总数量: {ranchSystem.CurrentTotalAmount}");
        }

        // 自动触发测试
        Invoke(nameof(ManualTriggerTest), 2f);
    }

    private void ManualTriggerTest()
    {
        Debug.Log("========================================");
        Debug.Log("🧪 开始手动触发月相变化测试");
        Debug.Log("========================================");

        ForestSystem forestSystem = FindObjectOfType<ForestSystem>();
        RanchSystem ranchSystem = FindObjectOfType<RanchSystem>();

        // 测试 ForestSystem
        if (forestSystem != null && GlobalTimeSystem.Instance != null)
        {
            Debug.Log("📢 手动调用 ForestSystem.HandlePhaseChange");
            try
            {
                // 通过反射调用私有方法进行测试
                var method = typeof(ForestSystem).GetMethod("HandlePhaseChange",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(forestSystem, new object[] { GlobalTimeSystem.MoonPhase.FullMoon, 1 });
                    Debug.Log("✅ ForestSystem.HandlePhaseChange 调用成功");
                }
                else
                {
                    Debug.LogError("❌ 无法找到 HandlePhaseChange 方法");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 调用 ForestSystem.HandlePhaseChange 失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("❌ ForestSystem 或 GlobalTimeSystem.Instance 为 null");
        }

        // 测试 RanchSystem
        if (ranchSystem != null && GlobalTimeSystem.Instance != null)
        {
            Debug.Log("📢 手动调用 RanchSystem.HandlePhaseChange");
            try
            {
                var method = typeof(RanchSystem).GetMethod("HandlePhaseChange",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(ranchSystem, new object[] { GlobalTimeSystem.MoonPhase.FullMoon, 1 });
                    Debug.Log("✅ RanchSystem.HandlePhaseChange 调用成功");
                }
                else
                {
                    Debug.LogError("❌ 无法找到 HandlePhaseChange 方法");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 调用 RanchSystem.HandlePhaseChange 失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("❌ RanchSystem 或 GlobalTimeSystem.Instance 为 null");
        }

        Debug.Log("========================================");
        Debug.Log("🧪 手动触发测试完成");
        Debug.Log("========================================");
    }
}
