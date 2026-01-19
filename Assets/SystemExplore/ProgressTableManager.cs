using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 海图志管理器 - 管理单个岛屿的进度数据，提供解锁接口
/// </summary>
public class ProgressTableManager : MonoBehaviour
{
    [Header("数据源")]
    [SerializeField] private ProgressTableScriptableObject dataTable;

    private bool isInitialized = false;

    private void Awake()
    {
        if (dataTable == null)
        {
            Debug.LogError("ProgressTableManager: dataTable 未设置，请在Inspector中指定！");
        }
        else
        {
            Debug.Log($"ProgressTableManager 初始化: {dataTable.island}");
        }
    }

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化管理器
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        Debug.Log($"ProgressTableManager 初始化完成: {dataTable.island}");
    }

    #region 数据获取接口

    /// <summary>
    /// 获取所属岛屿
    /// </summary>
    public SpeciesSource GetIsland()
    {
        return dataTable.island;
    }

    /// <summary>
    /// 获取岛屿名称
    /// </summary>
    public string GetIslandName()
    {
        return dataTable.island.ToString();
    }

    /// <summary>
    /// 获取绑定的物种列表
    /// </summary>
    public List<SpeciesScriptableObject> GetBoundSpecies()
    {
        return new List<SpeciesScriptableObject>(dataTable.boundSpecies);
    }

    /// <summary>
    /// 获取绑定的线索列表
    /// </summary>
    public List<ClueScriptableObject> GetBoundClues()
    {
        return new List<ClueScriptableObject>(dataTable.boundClues);
    }

    /// <summary>
    /// 获取绑定的秘密
    /// </summary>
    public SecretScriptableObject GetBoundSecret()
    {
        return dataTable.boundSecret;
    }

    /// <summary>
    /// 获取绑定的信物
    /// </summary>
    public TokenScriptableObject GetBoundToken()
    {
        return dataTable.boundToken;
    }

    /// <summary>
    /// 获取岛屿颜色
    /// </summary>
    public Color GetIslandColor()
    {
        return dataTable.islandColor;
    }

    /// <summary>
    /// 获取岛屿图标
    /// </summary>
    public Sprite GetIslandIcon()
    {
        return dataTable.islandIcon;
    }

    #endregion

    #region 状态检查接口

    /// <summary>
    /// 检查物种是否已解锁
    /// </summary>
    public bool IsSpeciesUnlocked(string speciesName)
    {
        foreach (var species in dataTable.boundSpecies)
        {
            if (species.speciesName == speciesName)
            {
                return species.unlocked;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查线索是否已解锁
    /// </summary>
    public bool IsClueUnlocked(string clueName)
    {
        foreach (var clue in dataTable.boundClues)
        {
            if (clue.clueName == clueName)
            {
                return clue.isUnlocked;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查秘密是否已解锁
    /// </summary>
    public bool IsSecretUnlocked()
    {
        return dataTable.boundSecret != null && dataTable.boundSecret.isUnlocked;
    }

    /// <summary>
    /// 检查信物是否已解锁
    /// </summary>
    public bool IsTokenUnlocked()
    {
        return dataTable.boundToken != null && dataTable.boundToken.isUnlocked;
    }

    /// <summary>
    /// 获取已解锁的物种数量
    /// </summary>
    public int GetUnlockedSpeciesCount()
    {
        int count = 0;
        foreach (var species in dataTable.boundSpecies)
        {
            if (species.unlocked) count++;
        }
        return count;
    }

    /// <summary>
    /// 获取已解锁的线索数量
    /// </summary>
    public int GetUnlockedCluesCount()
    {
        int count = 0;
        foreach (var clue in dataTable.boundClues)
        {
            if (clue.isUnlocked) count++;
        }
        return count;
    }

    /// <summary>
    /// 获取岛屿解锁进度（0-1）
    /// </summary>
    public float GetIslandProgress()
    {
        int totalItems = dataTable.boundSpecies.Count + dataTable.boundClues.Count + 2; // +2 表示秘密和信物
        int unlockedItems = GetUnlockedSpeciesCount() + GetUnlockedCluesCount();
        
        if (dataTable.boundSecret != null && dataTable.boundSecret.isUnlocked) unlockedItems++;
        if (dataTable.boundToken != null && dataTable.boundToken.isUnlocked) unlockedItems++;
        
        return totalItems > 0 ? (float)unlockedItems / totalItems : 0f;
    }

    #endregion

    #region 解锁接口（供外部系统调用）

    /// <summary>
    /// 解锁物种 - 供外部系统调用
    /// </summary>
    public bool UnlockSpecies(string speciesName)
    {
        foreach (var species in dataTable.boundSpecies)
        {
            if (species.speciesName == speciesName && !species.unlocked)
            {
                species.unlocked = true;
                Debug.Log($"物种解锁成功: {speciesName}");
                OnSpeciesUnlocked(species);
                return true;
            }
        }
        
        Debug.LogWarning($"未找到物种或已解锁: {speciesName}");
        return false;
    }

    /// <summary>
    /// 解锁线索 - 供外部系统调用
    /// </summary>
    public bool UnlockClue(string clueName)
    {
        foreach (var clue in dataTable.boundClues)
        {
            if (clue.clueName == clueName && !clue.isUnlocked)
            {
                clue.isUnlocked = true;
                Debug.Log($"线索解锁成功: {clueName}");
                OnClueUnlocked(clue);
                return true;
            }
        }
        
        Debug.LogWarning($"未找到线索或已解锁: {clueName}");
        return false;
    }

    /// <summary>
    /// 解锁秘密 - 供外部系统调用
    /// </summary>
    public bool UnlockSecret()
    {
        if (dataTable.boundSecret != null && !dataTable.boundSecret.isUnlocked)
        {
            dataTable.boundSecret.isUnlocked = true;
            Debug.Log($"秘密解锁成功: {dataTable.boundSecret.secretName}");
            OnSecretUnlocked(dataTable.boundSecret);
            return true;
        }
        
        Debug.LogWarning("未找到秘密或已解锁");
        return false;
    }

    /// <summary>
    /// 解锁信物 - 供外部系统调用
    /// </summary>
    public bool UnlockToken()
    {
        if (dataTable.boundToken != null && !dataTable.boundToken.isUnlocked)
        {
            dataTable.boundToken.isUnlocked = true;
            Debug.Log($"信物解锁成功: {dataTable.boundToken.tokenName}");
            OnTokenUnlocked(dataTable.boundToken);
            return true;
        }
        
        Debug.LogWarning("未找到信物或已解锁");
        return false;
    }

    /// <summary>
    /// 批量解锁物种
    /// </summary>
    public void UnlockSpeciesBatch(List<string> speciesNames)
    {
        int unlockedCount = 0;
        foreach (var speciesName in speciesNames)
        {
            if (UnlockSpecies(speciesName))
            {
                unlockedCount++;
            }
        }
        Debug.Log($"批量解锁物种完成: {unlockedCount}/{speciesNames.Count}");
    }

    /// <summary>
    /// 批量解锁线索
    /// </summary>
    public void UnlockCluesBatch(List<string> clueNames)
    {
        int unlockedCount = 0;
        foreach (var clueName in clueNames)
        {
            if (UnlockClue(clueName))
            {
                unlockedCount++;
            }
        }
        Debug.Log($"批量解锁线索完成: {unlockedCount}/{clueNames.Count}");
    }

    #endregion

    #region 事件回调（供外部监听）

    /// <summary>
    /// 物种解锁时的回调
    /// </summary>
    private void OnSpeciesUnlocked(SpeciesScriptableObject species)
    {
        // 可以在这里触发UI更新或其他逻辑
        Debug.Log($"物种解锁回调: {species.speciesName}");
        // 示例：通知UI系统更新显示
        // UIManager.Instance.UpdateSpeciesDisplay(species);
    }

    /// <summary>
    /// 线索解锁时的回调
    /// </summary>
    private void OnClueUnlocked(ClueScriptableObject clue)
    {
        // 可以在这里触发UI更新或其他逻辑
        Debug.Log($"线索解锁回调: {clue.clueName}");
        // 示例：通知UI系统更新显示
        // UIManager.Instance.UpdateClueDisplay(clue);
    }

    /// <summary>
    /// 秘密解锁时的回调
    /// </summary>
    private void OnSecretUnlocked(SecretScriptableObject secret)
    {
        // 可以在这里触发UI更新或其他逻辑
        Debug.Log($"秘密解锁回调: {secret.secretName}");
        // 示例：通知UI系统更新显示
        // UIManager.Instance.UpdateSecretDisplay(secret);
    }

    /// <summary>
    /// 信物解锁时的回调
    /// </summary>
    private void OnTokenUnlocked(TokenScriptableObject token)
    {
        // 可以在这里触发UI更新或其他逻辑
        Debug.Log($"信物解锁回调: {token.tokenName}");
        // 示例：通知UI系统更新显示
        // UIManager.Instance.UpdateTokenDisplay(token);
    }

    #endregion

    #region 数据更新接口

    /// <summary>
    /// 添加新物种到绑定列表
    /// </summary>
    public void AddSpeciesToBoundList(SpeciesScriptableObject species)
    {
        if (species == null) return;
        
        if (!dataTable.boundSpecies.Contains(species))
        {
            dataTable.boundSpecies.Add(species);
            Debug.Log($"添加物种到绑定列表: {species.speciesName}");
        }
    }

    /// <summary>
    /// 添加新线索到绑定列表
    /// </summary>
    public void AddClueToBoundList(ClueScriptableObject clue)
    {
        if (clue == null) return;
        
        if (!dataTable.boundClues.Contains(clue))
        {
            dataTable.boundClues.Add(clue);
            Debug.Log($"添加线索到绑定列表: {clue.clueName}");
        }
    }

    /// <summary>
    /// 更新绑定的秘密
    /// </summary>
    public void UpdateBoundSecret(SecretScriptableObject newSecret)
    {
        dataTable.boundSecret = newSecret;
        Debug.Log($"更新绑定秘密: {newSecret?.secretName ?? "null"}");
    }

    /// <summary>
    /// 更新绑定的信物
    /// </summary>
    public void UpdateBoundToken(TokenScriptableObject newToken)
    {
        dataTable.boundToken = newToken;
        Debug.Log($"更新绑定信物: {newToken?.tokenName ?? "null"}");
    }

    #endregion
}