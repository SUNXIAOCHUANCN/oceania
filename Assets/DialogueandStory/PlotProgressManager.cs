// PlotProgressManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class PlotProgressManager : MonoBehaviour
{
    // 单例模式，便于全局访问
    public static PlotProgressManager Instance { get; private set; }

    // 存储所有 PlotPoint 的名称或 ID
    private HashSet<string> triggeredPlotPoints = new HashSet<string>();

    // 所有需要触发的 PlotPoint 列表（可在编辑器中设置）
    [SerializeField] private List<string> requiredPlotPoints = new List<string>();

    // 保存数据的键值
    private const string PLOT_POINTS_SAVE_KEY = "PlotPointsData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlotPoints(); // 游戏开始时加载
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SavePlotPoints(); // 游戏退出时保存
    }

    /// <summary>
    /// 标记一个 PlotPoint 为已触发
    /// </summary>
    public void MarkPlotPointTriggered(string plotPointName)
    {
        if (string.IsNullOrEmpty(plotPointName))
        {
            Debug.LogWarning("Attempted to mark empty plot point name!");
            return;
        }

        if (!triggeredPlotPoints.Contains(plotPointName))
        {
            triggeredPlotPoints.Add(plotPointName);
            Debug.Log($"PlotPoint '{plotPointName}' triggered. Progress: {triggeredPlotPoints.Count}/{requiredPlotPoints.Count}");

            // 触发事件
            //OnPlotPointTriggered?.Invoke(plotPointName);
            SavePlotPoints();
        }
    }
    public void SavePlotPoints()
    {
        try
        {
            // 将HashSet转换为字符串列表，用逗号分隔
            string savedData = string.Join(",", triggeredPlotPoints);

            // 保存到 PlayerPrefs
            PlayerPrefs.SetString(PLOT_POINTS_SAVE_KEY, savedData);
            PlayerPrefs.Save();

            Debug.Log($"Plot points saved: {triggeredPlotPoints.Count} points");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save plot points: {e.Message}");
        }
    }
    /// <summary>
    /// 从 PlayerPrefs 加载剧情点
    /// </summary>
    public void LoadPlotPoints()
    {
        try
        {
            // 检查是否有保存数据
            if (!PlayerPrefs.HasKey(PLOT_POINTS_SAVE_KEY))
            {
                Debug.Log("No saved plot points found. Starting fresh.");
                triggeredPlotPoints = new HashSet<string>();
                return;
            }

            // 从 PlayerPrefs 读取数据
            string savedData = PlayerPrefs.GetString(PLOT_POINTS_SAVE_KEY);

            // 解析字符串为HashSet
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] pointsArray = savedData.Split(',');
                triggeredPlotPoints = new HashSet<string>(pointsArray);
                Debug.Log($"Plot points loaded: {triggeredPlotPoints.Count} points");
            }
            else
            {
                triggeredPlotPoints = new HashSet<string>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load plot points: {e.Message}");
            triggeredPlotPoints = new HashSet<string>();
        }
    }
    /// <summary>
    /// 删除所有保存的数据
    /// </summary>
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(PLOT_POINTS_SAVE_KEY);
        PlayerPrefs.Save();
        triggeredPlotPoints.Clear();
        Debug.Log("All plot points save data deleted.");
    }
    /// <summary>
    /// 检查指定剧情点是否已触发
    /// </summary>
    public bool IsPlotPointTriggered(string plotPointName)
    {
        return triggeredPlotPoints.Contains(plotPointName);
    }

    /// <summary>
    /// 检查是否所有剧情点都已触发
    /// </summary>
    public bool AreAllPlotPointsTriggered()
    {
        foreach (var point in requiredPlotPoints)
        {
            if (!triggeredPlotPoints.Contains(point))
            {
                Debug.Log($"Plot point '{point}' not triggered yet.");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检查是否所有指定的剧情点都已触发
    /// </summary>
    public bool ArePlotPointsTriggered(List<string> plotPoints)
    {
        foreach (var point in plotPoints)
        {
            if (!triggeredPlotPoints.Contains(point))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取当前进度（用于调试或 UI 显示）
    /// </summary>
    public int GetTriggeredCount()
    {
        return triggeredPlotPoints.Count;
    }

    /// <summary>
    /// 获取总剧情点数量
    /// </summary>
    public int GetTotalCount()
    {
        return requiredPlotPoints.Count;
    }

    /// <summary>
    /// 获取已触发的剧情点列表
    /// </summary>
    public List<string> GetTriggeredPlotPoints()
    {
        return new List<string>(triggeredPlotPoints);
    }

    /// <summary>
    /// 获取未触发的剧情点列表
    /// </summary>
    public List<string> GetUntriggeredPlotPoints()
    {
        List<string> untriggered = new List<string>();
        foreach (var point in requiredPlotPoints)
        {
            if (!triggeredPlotPoints.Contains(point))
            {
                untriggered.Add(point);
            }
        }
        return untriggered;
    }

    /// <summary>
    /// 重置所有剧情点状态（用于重新开始游戏）
    /// </summary>
    public void ResetAllPlotPoints()
    {
        triggeredPlotPoints.Clear();
        Debug.Log("All plot points have been reset.");
    }

    /// <summary>
    /// 从保存数据中恢复剧情点状态
    /// </summary>
    public void LoadPlotPoints(List<string> savedPlotPoints)
    {
        triggeredPlotPoints = new HashSet<string>(savedPlotPoints);
        Debug.Log($"Loaded {triggeredPlotPoints.Count} plot points from save data.");
    }
    
}