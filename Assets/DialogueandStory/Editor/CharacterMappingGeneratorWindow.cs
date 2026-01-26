using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// 角色头像映射配置生成器 - 快速创建GlobalCharacterMapping
/// </summary>
public class CharacterMappingGeneratorWindow : EditorWindow
{
    private string spriteFolderPath = "Assets/Resources/CharacterSprites";
    private string saveFolderPath = "Assets/DialogueandStory/CharacterMappings";
    private string mappingName = "GlobalCharacterMapping";
    private Sprite[] foundSprites = new Sprite[0];
    private bool autoExtractNames = true;

    [MenuItem("Tools/对话系统/角色头像映射生成器")]
    public static void ShowWindow()
    {
        GetWindow<CharacterMappingGeneratorWindow>("角色头像映射生成器");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("角色头像映射配置生成器", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "这个工具可以帮助你快速创建全局角色头像映射配置。\n\n" +
            "1. 将所有角色头像放在一个文件夹中\n" +
            "2. 选择文件夹路径\n" +
            "3. 点击生成，自动创建CharacterSpriteMapping配置",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // 文件夹路径配置
        EditorGUILayout.LabelField("文件夹配置", EditorStyles.boldLabel);
        spriteFolderPath = EditorGUILayout.TextField("头像文件夹路径", spriteFolderPath);
        saveFolderPath = EditorGUILayout.TextField("保存配置文件路径", saveFolderPath);

        EditorGUILayout.Space(5);

        // 配置名称
        mappingName = EditorGUILayout.TextField("配置文件名称", mappingName);

        EditorGUILayout.Space(5);

        // 选项
        autoExtractNames = EditorGUILayout.Toggle("从文件名自动提取角色名", autoExtractNames);

        if (!autoExtractNames)
        {
            EditorGUILayout.HelpBox(
                "关闭自动提取后，你需要手动为每个Sprite指定角色名。",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space(10);

        // 扫描按钮
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("扫描头像文件", GUILayout.Height(30)))
            {
                ScanSprites();
            }

            if (GUILayout.Button("浏览文件夹", GUILayout.Width(100)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择头像文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 转换为相对路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        spriteFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    ScanSprites();
                }
            }
        }

        EditorGUILayout.Space(10);

        // 显示找到的Sprite
        if (foundSprites.Length > 0)
        {
            EditorGUILayout.LabelField($"找到 {foundSprites.Length} 个头像文件:", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (Sprite sprite in foundSprites)
                {
                    string extractedName = autoExtractNames ? ExtractCharacterName(sprite.name) : sprite.name;
                    EditorGUILayout.LabelField($"📄 {sprite.name} → [{extractedName}]", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space(10);

            // 生成按钮
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("生成角色映射配置", GUILayout.Height(35)))
            {
                GenerateMapping();
            }
            GUI.backgroundColor = Color.white;
        }
        else if (spriteFolderPath != "Assets/Resources/CharacterSprites")
        {
            EditorGUILayout.HelpBox("未找到任何Sprite文件，请检查文件夹路径", MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        // 预设模板
        EditorGUILayout.LabelField("快速创建模板", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "你的游戏有这些角色：\n" +
            "基诺、拉拉、库拉奶奶、玛莱、阿斯特拉、莉法、艾拉、玛拉、孩子们\n\n" +
            "建议：将头像文件命名为角色名（中文或拼音），如：\n" +
            "- 基诺.png 或 Jinuo.png\n" +
            "- 拉拉.png 或 Lala.png",
            MessageType.None
        );

        if (GUILayout.Button("创建推荐的文件夹结构"))
        {
            CreateRecommendedFolderStructure();
        }
    }

    /// <summary>
    /// 扫描文件夹中的所有Sprite
    /// </summary>
    private void ScanSprites()
    {
        if (!AssetDatabase.IsValidFolder(spriteFolderPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件夹不存在: {spriteFolderPath}", "确定");
            foundSprites = new Sprite[0];
            return;
        }

        // 加载文件夹中的所有Sprite
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolderPath });
        foundSprites = new Sprite[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            foundSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        if (foundSprites.Length == 0)
        {
            Debug.LogWarning($"在文件夹 {spriteFolderPath} 中未找到任何Sprite");
        }
        else
        {
            Debug.Log($"扫描完成，找到 {foundSprites.Length} 个Sprite");
        }

        Repaint();
    }

    /// <summary>
    /// 从文件名提取角色名
    /// </summary>
    private string ExtractCharacterName(string fileName)
    {
        // 移除扩展名
        string name = Path.GetFileNameWithoutExtension(fileName);

        // 如果文件名包含下划线，取第一部分作为角色名
        // 例如: "基诺_Happy.png" → "基诺"
        if (name.Contains("_"))
        {
            name = name.Split('_')[0];
        }

        return name;
    }

    /// <summary>
    /// 生成CharacterSpriteMapping配置文件
    /// </summary>
    private void GenerateMapping()
    {
        if (foundSprites.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "没有找到任何Sprite文件", "确定");
            return;
        }

        // 确保保存文件夹存在
        if (!AssetDatabase.IsValidFolder(saveFolderPath))
        {
            string parent = Path.GetDirectoryName(saveFolderPath);
            string folderName = Path.GetFileName(saveFolderPath);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EditorUtility.DisplayDialog("错误", $"保存路径的父文件夹不存在: {parent}", "确定");
                return;
            }

            AssetDatabase.CreateFolder(parent, folderName);
            AssetDatabase.Refresh();
        }

        // 创建CharacterSpriteMapping实例
        CharacterSpriteMapping mapping = ScriptableObject.CreateInstance<CharacterSpriteMapping>();

        // 使用反射设置私有字段
        var field = typeof(CharacterSpriteMapping).GetField("characterEntries",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            // 创建角色条目数组
            var entries = new System.Collections.Generic.List<CharacterSpriteMapping.CharacterEntry>();

            foreach (Sprite sprite in foundSprites)
            {
                string characterName = autoExtractNames ? ExtractCharacterName(sprite.name) : sprite.name;

                entries.Add(new CharacterSpriteMapping.CharacterEntry
                {
                    characterName = characterName,
                    characterSprite = sprite
                });
            }

            field.SetValue(mapping, entries.ToArray());
        }

        // 保存为Asset
        string assetPath = $"{saveFolderPath}/{mappingName}.asset";
        AssetDatabase.CreateAsset(mapping, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功",
            $"角色映射配置已创建！\n\n" +
            $"路径: {assetPath}\n" +
            $"包含 {foundSprites.Length} 个角色",
            "确定");

        // 选中创建的资源
        Selection.activeObject = mapping;
        EditorGUIUtility.PingObject(mapping);

        Debug.Log($"[角色头像映射生成器] 成功创建配置文件: {assetPath}");
    }

    /// <summary>
    /// 创建推荐的文件夹结构
    /// </summary>
    private void CreateRecommendedFolderStructure()
    {
        // 创建Resources/CharacterSprites文件夹
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources/CharacterSprites"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "CharacterSprites");
        }

        // 创建DialogueandStory/CharacterMappings文件夹
        if (!AssetDatabase.IsValidFolder("Assets/DialogueandStory/CharacterMappings"))
        {
            AssetDatabase.CreateFolder("Assets/DialogueandStory", "CharacterMappings");
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("创建完成",
            "已创建以下文件夹结构：\n\n" +
            "📁 Assets/Resources/CharacterSprites/\n" +
            "   → 将角色头像图片放在这里\n\n" +
            "📁 Assets/DialogueandStory/CharacterMappings/\n" +
            "   → 配置文件将保存在这里\n\n" +
            "接下来：\n" +
            "1. 将角色头像图片放入 CharacterSprites 文件夹\n" +
            "2. 确保图片的Texture Type设置为 Sprite (2D and UI)\n" +
            "3. 点击[扫描头像文件]按钮\n" +
            "4. 点击[生成角色映射配置]",
            "确定");

        // 在Project窗口中选中CharacterSprites文件夹
        Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/CharacterSprites");
        if (folder != null)
        {
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }
    }
}
