// PlotProgressManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PlotProgressManager : MonoBehaviour
{
    // 单例模式，便于全局访问
    public static PlotProgressManager Instance { get; private set; }

    // 存储所有 PlotPoint 的名称或 ID
    private HashSet<string> triggeredPlotPoints = new HashSet<string>();

    // 所有需要触发的 PlotPoint 列表（可在编辑器中设置）
    [SerializeField] private List<string> requiredPlotPoints = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 标记一个 PlotPoint 为已触发
    public void MarkPlotPointTriggered(string plotPointName)
    {
        triggeredPlotPoints.Add(plotPointName);
        Debug.Log($"PlotPoint {plotPointName} triggered. Progress: {triggeredPlotPoints.Count}/{requiredPlotPoints.Count}");
    }

    // 检查是否所有 PlotPoint 都已触发
    public bool AreAllPlotPointsTriggered()
    {
        foreach (var point in requiredPlotPoints)
        {
            if (!triggeredPlotPoints.Contains(point))
            {
                return false;
            }
        }
        return true;
    }

    // 获取当前进度（用于调试或 UI 显示）
    public int GetTriggeredCount()
    {
        return triggeredPlotPoints.Count;
    }

    public int GetTotalCount()
    {
        return requiredPlotPoints.Count;
    }
}