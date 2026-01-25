using UnityEngine;

/// <summary>
/// 角色头像映射配置 - 存储角色名与对应图片的映射关系
/// </summary>
[CreateAssetMenu(fileName = "CharacterSpriteMapping", menuName = "Dialogue/Character Sprite Mapping")]
public class CharacterSpriteMapping : ScriptableObject
{
    [System.Serializable]
    public class CharacterEntry
    {
        public string characterName;  // 角色名称，如 "基诺"
        public Sprite characterSprite; // 对应的头像图片
    }

    [Header("角色头像映射表")]
    [SerializeField] private CharacterEntry[] characterEntries;

    /// <summary>
    /// 根据角色名获取对应的头像
    /// </summary>
    public Sprite GetSprite(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
            return null;

        foreach (var entry in characterEntries)
        {
            if (entry.characterName == characterName)
            {
                return entry.characterSprite;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取所有已配置的角色名
    /// </summary>
    public string[] GetAllCharacterNames()
    {
        string[] names = new string[characterEntries.Length];
        for (int i = 0; i < characterEntries.Length; i++)
        {
            names[i] = characterEntries[i].characterName;
        }
        return names;
    }
}
