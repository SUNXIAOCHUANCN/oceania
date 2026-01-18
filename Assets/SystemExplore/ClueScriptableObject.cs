using UnityEngine;

[CreateAssetMenu(fileName = "NewClue", menuName = "System/Explore/Clue Data", order = 1)]
public class ClueScriptableObject : ScriptableObject
{
    [Header("线索信息")]
    public string clueName;           // 线索名称
    public SpeciesSource relatedIsland; // 线索所在岛屿
    [TextArea(3, 10)]
    public string clueText;           // 线索文字描述
    [TextArea(1, 5)]
    public string keyText;            // 线索重点文字
    
    [Header("状态信息")]
    public bool isUnlocked = false;   // 线索是否解锁
}