using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 台词解析器 - 从txt文件解析对话内容并生成dialogueString列表
/// </summary>
public class DialogueParser : MonoBehaviour
{
    /// <summary>
    /// 从txt文件解析对话内容
    /// </summary>
    /// <param name="filePath">txt文件的完整路径</param>
    /// <param name="characterMapping">角色头像映射配置</param>
    /// <returns>解析后的dialogueString列表</returns>
    public static List<dialogueString> ParseDialogueFile(string filePath, CharacterSpriteMapping characterMapping)
    {
        List<dialogueString> dialogueList = new List<dialogueString>();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"对话文件不存在: {filePath}");
            return dialogueList;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            // 跳过空行
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // 解析格式: [角色名]：对话内容
            dialogueString dialog = ParseLine(line, characterMapping);
            if (dialog != null)
            {
                dialogueList.Add(dialog);
            }
        }

        // 自动标记最后一行为结束
        if (dialogueList.Count > 0)
        {
            dialogueList[dialogueList.Count - 1].isEnd = true;
        }

        Debug.Log($"成功解析对话文件: {Path.GetFileName(filePath)}, 共 {dialogueList.Count} 行对话");
        return dialogueList;
    }

    /// <summary>
    /// 解析单行对话
    /// </summary>
    private static dialogueString ParseLine(string line, CharacterSpriteMapping characterMapping)
    {
        // 匹配格式: [角色名]：对话内容
        // 支持中文冒号和英文冒号
        Match match = Regex.Match(line, @"\[([^\]]+)\]\s*[:：]\s*(.*)");

        if (!match.Success)
        {
            // 尝试匹配简单格式（没有方括号）
            match = Regex.Match(line, @"^([^:：]+)\s*[:：]\s*(.*)");
        }

        if (!match.Success)
        {
            Debug.LogWarning($"无法解析的行: {line}");
            return null;
        }

        string characterName = match.Groups[1].Value.Trim();
        string dialogueText = match.Groups[2].Value.Trim();

        if (string.IsNullOrEmpty(dialogueText))
        {
            Debug.LogWarning($"空对话内容: {line}");
            return null;
        }

        dialogueString dialog = new dialogueString
        {
            text = dialogueText,
            isEnd = false,
            isQuestion = false,
            speakerName = characterName  // 设置说话者名字，用于动态切换头像
        };

        return dialog;
    }

    /// <summary>
    /// 从Resources文件夹加载txt文件并解析
    /// </summary>
    /// <param name="resourcesPath">Resources文件夹下的相对路径，如 "Dialogues/test"</param>
    /// <param name="characterMapping">角色头像映射配置</param>
    /// <returns>解析后的dialogueString列表</returns>
    public static List<dialogueString> ParseFromResources(string resourcesPath, CharacterSpriteMapping characterMapping)
    {
        TextAsset textFile = Resources.Load<TextAsset>(resourcesPath);
        if (textFile == null)
        {
            Debug.LogError($"在Resources中找不到文件: {resourcesPath}");
            return new List<dialogueString>();
        }

        List<dialogueString> dialogueList = new List<dialogueString>();
        string[] lines = textFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            dialogueString dialog = ParseLine(line.Trim(), characterMapping);
            if (dialog != null)
            {
                dialogueList.Add(dialog);
            }
        }

        if (dialogueList.Count > 0)
        {
            dialogueList[dialogueList.Count - 1].isEnd = true;
        }

        Debug.Log($"成功从Resources加载对话: {resourcesPath}, 共 {dialogueList.Count} 行");
        return dialogueList;
    }
}
