using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// DialogueTrigger的编辑器扩展 - 添加一键导入台词功能
/// </summary>
[CustomEditor(typeof(DialogueTrigger))]
public class DialogueTriggerEditor : Editor
{
    private string txtFilePath = "";
    private CharacterSpriteMapping characterMapping;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DialogueTrigger dialogueTrigger = (DialogueTrigger)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("台词自动导入工具", EditorStyles.boldLabel);

        // 角色头像映射配置
        characterMapping = (CharacterSpriteMapping)EditorGUILayout.ObjectField(
            "角色头像映射",
            characterMapping,
            typeof(CharacterSpriteMapping),
            false
        );

        // txt文件路径输入
        EditorGUILayout.LabelField("从txt文件导入台词:");
        txtFilePath = EditorGUILayout.TextField("文件路径", txtFilePath);

        // 浏览文件按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("浏览文件", GUILayout.Width(100)))
        {
            string path = EditorUtility.OpenFilePanel("选择台词文件", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(path))
            {
                txtFilePath = path;
            }
        }

        // 导入按钮
        GUI.enabled = !string.IsNullOrEmpty(txtFilePath) && characterMapping != null;
        if (GUILayout.Button("导入台词"))
        {
            ImportDialogueFromFile(dialogueTrigger, txtFilePath, characterMapping);
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        // 或者从Resources导入
        EditorGUILayout.LabelField("从Resources文件夹导入:", EditorStyles.boldLabel);
        string resourcesPath = EditorGUILayout.TextField("Resources路径", "Dialogues/文件名");

        if (GUILayout.Button("从Resources导入"))
        {
            ImportDialogueFromResources(dialogueTrigger, resourcesPath.Trim(), characterMapping);
        }

        // 帮助信息
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "台词格式示例:\n" +
            "[基诺]：你好，欢迎来到村庄！\n" +
            "[拉拉]：基诺姐姐！\n\n" +
            "注意:\n" +
            "1. 必须先创建角色头像映射配置\n" +
            "2. txt文件要放在Assets/DialogueandStory/Dialogues文件夹下",
            MessageType.Info
        );
    }

    /// <summary>
    /// 从txt文件导入台词
    /// </summary>
    private void ImportDialogueFromFile(DialogueTrigger dialogueTrigger, string filePath, CharacterSpriteMapping mapping)
    {
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {filePath}", "确定");
            return;
        }

        List<dialogueString> dialogueList = DialogueParser.ParseDialogueFile(filePath, mapping);

        if (dialogueList.Count == 0)
        {
            EditorUtility.DisplayDialog("警告", "未能解析出任何对话内容，请检查文件格式", "确定");
            return;
        }

        // 应用到DialogueTrigger
        ApplyDialogueToTrigger(dialogueTrigger, dialogueList, mapping);

        EditorUtility.DisplayDialog("成功", $"成功导入 {dialogueList.Count} 行对话", "确定");
        EditorUtility.SetDirty(dialogueTrigger);
    }

    /// <summary>
    /// 从Resources导入台词
    /// </summary>
    private void ImportDialogueFromResources(DialogueTrigger dialogueTrigger, string resourcesPath, CharacterSpriteMapping mapping)
    {
        if (mapping == null)
        {
            EditorUtility.DisplayDialog("错误", "请先设置角色头像映射配置", "确定");
            return;
        }

        List<dialogueString> dialogueList = DialogueParser.ParseFromResources(resourcesPath, mapping);

        if (dialogueList.Count == 0)
        {
            EditorUtility.DisplayDialog("警告", "未能解析出任何对话内容，请检查路径和格式", "确定");
            return;
        }

        // 应用到DialogueTrigger
        ApplyDialogueToTrigger(dialogueTrigger, dialogueList, mapping);

        EditorUtility.DisplayDialog("成功", $"成功从Resources导入 {dialogueList.Count} 行对话", "确定");
        EditorUtility.SetDirty(dialogueTrigger);
    }

    /// <summary>
    /// 将解析的对话应用到DialogueTrigger
    /// </summary>
    private void ApplyDialogueToTrigger(DialogueTrigger trigger, List<dialogueString> dialogueList, CharacterSpriteMapping mapping)
    {
        // 使用反射设置私有字段（因为dialogueStrings是private的）
        var field = typeof(DialogueTrigger).GetField("dialogueStrings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(trigger, dialogueList);
        }
        else
        {
            Debug.LogError("无法访问dialogueStrings字段");
        }
    }
}
