using System;

// 定义招募或建造时所需的单一资源和数量
[Serializable]
public class ResourceCost
{
    public string resourceName; // e.g., "薯蓣", "棕榈"
    public int amount;          // e.g., 10, 3
}