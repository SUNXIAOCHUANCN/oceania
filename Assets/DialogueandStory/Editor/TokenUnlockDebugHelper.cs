using UnityEngine;
using UnityEditor;

/// <summary>
/// 调试助手 - 检查ExploreRewardUnlockerNPC的配置
/// </summary>
public class TokenUnlockDebugHelper : EditorWindow
{
    private ExploreRewardUnlockerNPC targetNPC;

    [MenuItem("Tools/对话系统/Token解锁调试助手")]
    public static void ShowWindow()
    {
        GetWindow<TokenUnlockDebugHelper>("Token解锁调试助手");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Token解锁调试助手", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // 选择目标NPC
        targetNPC = (ExploreRewardUnlockerNPC)EditorGUILayout.ObjectField(
            "目标NPC",
            targetNPC,
            typeof(ExploreRewardUnlockerNPC),
            true
        );

        EditorGUILayout.Space(10);

        if (targetNPC != null)
        {
            // 显示配置信息
            EditorGUILayout.LabelField("配置检查:", EditorStyles.boldLabel);

            // 使用反射获取私有字段
            var targetIslandField = typeof(ExploreRewardUnlockerNPC).GetField("targetIsland",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var unlockTokenField = typeof(ExploreRewardUnlockerNPC).GetField("unlockToken",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var showDebugLogField = typeof(ExploreRewardUnlockerNPC).GetField("showDebugLog",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (targetIslandField != null)
            {
                var targetIsland = targetIslandField.GetValue(targetNPC);
                EditorGUILayout.LabelField($"目标岛屿: {targetIsland}");
            }

            if (unlockTokenField != null)
            {
                var unlockToken = unlockTokenField.GetValue(targetNPC);
                bool shouldUnlock = (bool)unlockToken;
                EditorGUILayout.LabelField($"解锁Token: {(shouldUnlock ? "✓ 是" : "✗ 否")}");

                if (!shouldUnlock)
                {
                    EditorGUILayout.HelpBox("❌ Unlock Token未勾选，无法解锁信物！", MessageType.Error);
                }
            }

            if (showDebugLogField != null)
            {
                var showDebug = showDebugLogField.GetValue(targetNPC);
                bool debugEnabled = (bool)showDebug;
                EditorGUILayout.LabelField($"调试日志: {(debugEnabled ? "✓ 开启" : "✗ 关闭")}");
            }

            EditorGUILayout.Space(10);

            // 测试按钮
            EditorGUILayout.LabelField("测试功能:", EditorStyles.boldLabel);

            if (GUILayout.Button("测试查找ProgressTableManager"))
            {
                TestFindManager();
            }

            if (GUILayout.Button("测试解锁Token"))
            {
                TestUnlockToken();
            }

            if (GUILayout.Button("检查Token是否已解锁"))
            {
                CheckTokenStatus();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请在Hierarchy中选择一个带有ExploreRewardUnlockerNPC组件的对象", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // 使用说明
        EditorGUILayout.LabelField("使用说明:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. 在Hierarchy中选择酋长NPC对象\n" +
            "2. 点击上方的对象字段，拖入NPC\n" +
            "3. 检查配置是否正确\n" +
            "4. 点击测试按钮验证功能",
            MessageType.Info
        );
    }

    private void TestFindManager()
    {
        if (targetNPC == null) return;

        Debug.Log("=== 测试查找ProgressTableManager ===");

        // 尝试通过ExploreSystem查找
        if (ExploreSystem.Instance != null)
        {
            var allManagers = ExploreSystem.Instance.GetAllProgressTableManagers();
            Debug.Log($"ExploreSystem找到 {allManagers.Count} 个ProgressTableManager");

            foreach (var manager in allManagers)
            {
                Debug.Log($"  - {manager.GetIslandName()}");
            }
        }
        else
        {
            Debug.LogWarning("ExploreSystem.Instance为null");
        }

        // 尝试在场景中查找
        ProgressTableManager[] foundManagers = FindObjectsOfType<ProgressTableManager>();
        Debug.Log($"场景中共有 {foundManagers.Length} 个ProgressTableManager");
    }

    private void TestUnlockToken()
    {
        if (targetNPC == null) return;

        Debug.Log("=== 测试解锁Token ===");
        targetNPC.SendMessage("UnlockToken");
    }

    private void CheckTokenStatus()
    {
        if (targetNPC == null) return;

        Debug.Log("=== 检查Token状态 ===");

        // 使用反射调用IsTokenUnlocked
        var method = typeof(ExploreRewardUnlockerNPC).GetMethod("IsTokenUnlocked",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (method != null)
        {
            bool isUnlocked = (bool)method.Invoke(targetNPC, null);
            Debug.Log($"Token已解锁: {isUnlocked}");
        }
    }
}
