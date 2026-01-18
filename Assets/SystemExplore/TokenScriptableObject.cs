using UnityEngine;

[CreateAssetMenu(fileName = "NewToken", menuName = "System/Explore/Token Data", order = 2)]
public class TokenScriptableObject : ScriptableObject
{
    [Header("信物信息")]
    public string tokenName;          // 信物名称
    public SpeciesSource relatedIsland; // 信物所属岛屿
    public Sprite icon;               // 信物图标
    
    [Header("状态信息")]
    public bool isUnlocked = false;   // 信物是否解锁
}