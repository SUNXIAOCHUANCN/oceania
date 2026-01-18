using UnityEngine;

[CreateAssetMenu(fileName = "NewSecret", menuName = "System/Explore/Secret Data", order = 3)]
public class SecretScriptableObject : ScriptableObject
{
    [Header("秘密信息")]
    public string secretName;          // 秘密名称
    public SpeciesSource relatedIsland; // 秘密所属岛屿
    public bool isUnlocked = false;     // 秘密是否解锁
    [TextArea(3, 10)]
    public string secretContent;       // 秘密内容
}