using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewProgressTable", menuName = "System/Explore/Progress Table", order = 4)]
public class ProgressTableScriptableObject : ScriptableObject
{
    [Header("所属岛屿")]
    public SpeciesSource island;
    
    [Header("绑定的物种")]
    public List<SpeciesScriptableObject> boundSpecies = new List<SpeciesScriptableObject>();
    
    [Header("绑定的秘密")]
    public SecretScriptableObject boundSecret;
    
    [Header("绑定的线索")]
    public List<ClueScriptableObject> boundClues = new List<ClueScriptableObject>();
    
    [Header("绑定的信物")]
    public TokenScriptableObject boundToken;
    
    [Header("UI主题设置")]
    public Color islandColor = Color.white;
    public Sprite islandIcon;
}