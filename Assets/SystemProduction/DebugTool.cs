using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 统一的日志工具类，支持带颜色的日志输出和文件写入
/// </summary>
public static class DebugTool
{
    private static string logFilePath;
    private static bool isInitialized = false;

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    private static void Initialize()
    {
        if (isInitialized) return;

        // 设置日志文件路径
        string folderPath = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(folderPath, $"GameLog_{timestamp}.txt");

        // 写入文件头
        WriteToFile("========================================");
        WriteToFile($"游戏日志 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteToFile("========================================\n");

        isInitialized = true;
    }

    /// <summary>
    /// 写入日志到文件
    /// </summary>
    private static void WriteToFile(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(logFilePath))
            {
                Initialize();
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            File.AppendAllText(logFilePath, $"[{timestamp}] {message}\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"写入日志文件失败: {e.Message}");
        }
    }

    /// <summary>
    /// 带颜色的日志输出（支持数值格式化）
    /// </summary>
    /// <param name="scriptName">脚本名称</param>
    /// <param name="colorHex">颜色十六进制代码</param>
    /// <param name="message">日志消息</param>
    /// <param name="args">数值参数</param>
    public static void Log(string scriptName, string colorHex, string message, params object[] args)
    {
        // 格式化消息
        string formattedMessage = args != null && args.Length > 0
            ? string.Format(message, args)
            : message;

        // 构建带颜色的Unity日志
        string coloredMessage = $"<color=#{colorHex}>[{scriptName}]</color>: {formattedMessage}";

        // 输出到Unity控制台
        Debug.Log(coloredMessage);

        // 写入到文件（去除颜色标签）
        string plainMessage = $"[{scriptName}]: {formattedMessage}";
        WriteToFile(plainMessage);
    }

    /// <summary>
    /// 普通日志（默认白色）
    /// </summary>
    public static void Log(string scriptName, string message, params object[] args)
    {
        Log(scriptName, "FFFFFF", message, args);
    }

    /// <summary>
    /// 警告日志（黄色）
    /// </summary>
    public static void LogWarning(string scriptName, string message, params object[] args)
    {
        // 格式化消息
        string formattedMessage = args != null && args.Length > 0
            ? string.Format(message, args)
            : message;

        // Unity警告日志
        Debug.LogWarning($"[{scriptName}]: {formattedMessage}");

        // 写入到文件
        string plainMessage = $"[WARNING] [{scriptName}]: {formattedMessage}";
        WriteToFile(plainMessage);
    }

    /// <summary>
    /// 错误日志（红色）
    /// </summary>
    public static void LogError(string scriptName, string message, params object[] args)
    {
        // 格式化消息
        string formattedMessage = args != null && args.Length > 0
            ? string.Format(message, args)
            : message;

        // Unity错误日志
        Debug.LogError($"[{scriptName}]: {formattedMessage}");

        // 写入到文件
        string plainMessage = $"[ERROR] [{scriptName}]: {formattedMessage}";
        WriteToFile(plainMessage);
    }

    /// <summary>
    /// 农场系统专用日志（绿色 #00FF00）
    /// </summary>
    public static void LogFarm(string message, params object[] args)
    {
        Log("FarmSystem", "00FF00", message, args);
    }

    /// <summary>
    /// 森林系统专用日志（黄色 #FFFF00）
    /// </summary>
    public static void LogForest(string message, params object[] args)
    {
        Log("ForestSystem", "FFFF00", message, args);
    }

    /// <summary>
    /// 牧场系统专用日志（蓝色 #00BFFF）
    /// </summary>
    public static void LogRanch(string message, params object[] args)
    {
        Log("RanchSystem", "00BFFF", message, args);
    }

    /// <summary>
    /// 资源管理器专用日志（白色 #FFFFFF）
    /// </summary>
    public static void LogResource(string message, params object[] args)
    {
        Log("ResourceManager", "FFFFFF", message, args);
    }

    /// <summary>
    /// 资源摘要UI专用日志（青色 #00CED1）
    /// </summary>
    public static void LogResourceSummary(string message, params object[] args)
    {
        Log("ResourceSummaryUI", "00CED1", message, args);
    }

    /// <summary>
    /// 资源计算器专用日志（紫色 #9370DB）
    /// </summary>
    public static void LogResourceCalculator(string message, params object[] args)
    {
        Log("ResourceCalculator", "9370DB", message, args);
    }

    /// <summary>
    /// 获取日志文件路径
    /// </summary>
    public static string GetLogFilePath()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        return logFilePath;
    }
}
