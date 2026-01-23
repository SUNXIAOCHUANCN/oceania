using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 探索系统管理器 - 管理所有岛屿的探索进度
/// </summary>
public class ExploreSystem : MonoBehaviour
{
    public static ExploreSystem Instance { get; private set; }

    [Header("探索表管理器列表")]
    [SerializeField] private List<ProgressTableManager> progressTableManagers = new List<ProgressTableManager>();

    /// <summary>
    /// 探索数据加载完成事件
    /// </summary>
    public System.Action OnExploreDataLoaded;

    private bool isInitialized = false;

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
            return;
        }
    }

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化探索系统
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;

        // 自动查找所有ProgressTableManager
        if (progressTableManagers.Count == 0)
        {
            ProgressTableManager[] foundManagers = FindObjectsOfType<ProgressTableManager>();
            progressTableManagers = new List<ProgressTableManager>(foundManagers);
            Debug.Log($"[ExploreSystem] 自动找到 {progressTableManagers.Count} 个ProgressTableManager");
        }

        isInitialized = true;
        Debug.Log($"[ExploreSystem] 初始化完成，管理 {progressTableManagers.Count} 个岛屿");
    }

    /// <summary>
    /// 从存档加载所有探索数据
    /// </summary>
    public void LoadFromSaveData(ExploreSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("[ExploreSystem] 存档数据为空，跳过加载");
            return;
        }

        if (saveData.islandsExploreData.Count == 0)
        {
            Debug.Log("[ExploreSystem] 存档为空（首次运行），将从 ScriptableObject 加载初始数据");
            return;
        }

        Debug.Log($"[ExploreSystem] 开始加载探索数据，包含 {saveData.islandsExploreData.Count} 个岛屿");

        int loadedCount = 0;
        foreach (var islandData in saveData.islandsExploreData)
        {
            ProgressTableManager manager = FindProgressTableManagerByIsland(islandData.islandName);
            if (manager != null)
            {
                LoadIslandData(manager, islandData);
                loadedCount++;
            }
            else
            {
                Debug.LogWarning($"[ExploreSystem] 未找到岛屿 '{islandData.islandName}' 的ProgressTableManager");
            }
        }

        Debug.Log($"[ExploreSystem] 探索数据加载完成，成功加载 {loadedCount}/{saveData.islandsExploreData.Count} 个岛屿");

        // 触发加载完成事件
        OnExploreDataLoaded?.Invoke();
    }

    /// <summary>
    /// 导出所有探索数据到存档结构
    /// </summary>
    public ExploreSaveData ExportToSaveData()
    {
        ExploreSaveData saveData = new ExploreSaveData();

        Debug.Log($"[ExploreSystem] 导出探索数据，共 {progressTableManagers.Count} 个岛屿");

        foreach (var manager in progressTableManagers)
        {
            if (manager != null)
            {
                IslandExploreSaveData islandData = ExportIslandData(manager);
                saveData.islandsExploreData.Add(islandData);
            }
        }

        Debug.Log($"[ExploreSystem] 导出完成，共 {saveData.islandsExploreData.Count} 个岛屿的数据");
        return saveData;
    }

    /// <summary>
    /// 加载单个岛屿的数据
    /// </summary>
    private void LoadIslandData(ProgressTableManager manager, IslandExploreSaveData islandData)
    {
        if (manager == null || islandData == null) return;

        string islandName = manager.GetIslandName();
        Debug.Log($"[ExploreSystem] 加载岛屿数据: {islandName}");

        // 加载已解锁的线索
        foreach (var clueData in islandData.unlockedClues)
        {
            if (clueData.isUnlocked)
            {
                manager.UnlockClue(clueData.clueName);
            }
        }

        // 加载已解锁的秘密
        foreach (var secretData in islandData.unlockedSecrets)
        {
            if (secretData.isUnlocked)
            {
                manager.UnlockSecret();
            }
        }

        // 加载已解锁的信物
        foreach (var tokenData in islandData.unlockedTokens)
        {
            if (tokenData.isUnlocked)
            {
                manager.UnlockToken();
            }
        }

        // 加载已解锁的物种
        foreach (var speciesData in islandData.unlockedSpecies)
        {
            if (speciesData.isUnlocked)
            {
                manager.UnlockSpecies(speciesData.speciesName);
            }
        }

        Debug.Log($"[ExploreSystem] 岛屿 {islandName} 数据加载完成");
    }

    /// <summary>
    /// 导出单个岛屿的数据
    /// </summary>
    private IslandExploreSaveData ExportIslandData(ProgressTableManager manager)
    {
        if (manager == null) return null;

        IslandExploreSaveData islandData = new IslandExploreSaveData(manager.GetIslandName());

        // 导出线索
        var boundClues = manager.GetBoundClues();
        foreach (var clue in boundClues)
        {
            islandData.unlockedClues.Add(new ClueSaveData(clue.clueName, clue.isUnlocked));
        }

        // 导出秘密
        var boundSecret = manager.GetBoundSecret();
        if (boundSecret != null)
        {
            islandData.unlockedSecrets.Add(new SecretSaveData(
                boundSecret.secretName,
                boundSecret.isUnlocked
            ));
        }

        // 导出信物
        var boundToken = manager.GetBoundToken();
        if (boundToken != null)
        {
            islandData.unlockedTokens.Add(new TokenSaveData(
                boundToken.tokenName,
                boundToken.isUnlocked
            ));
        }

        // 导出物种
        var boundSpecies = manager.GetBoundSpecies();
        foreach (var species in boundSpecies)
        {
            islandData.unlockedSpecies.Add(new SpeciesUnlockSaveData(
                species.speciesName,
                species.unlocked
            ));
        }

        Debug.Log($"[ExploreSystem] 导出岛屿 {islandData.islandName}: " +
                 $"{islandData.unlockedClues.Count} 线索, " +
                 $"{islandData.unlockedSecrets.Count} 秘密, " +
                 $"{islandData.unlockedTokens.Count} 信物, " +
                 $"{islandData.unlockedSpecies.Count} 物种");

        return islandData;
    }

    /// <summary>
    /// 根据岛屿名称查找ProgressTableManager
    /// </summary>
    private ProgressTableManager FindProgressTableManagerByIsland(string islandName)
    {
        return progressTableManagers.Find(m => m.GetIslandName() == islandName);
    }

    /// <summary>
    /// 添加ProgressTableManager到列表
    /// </summary>
    public void RegisterProgressTableManager(ProgressTableManager manager)
    {
        if (manager != null && !progressTableManagers.Contains(manager))
        {
            progressTableManagers.Add(manager);
            Debug.Log($"[ExploreSystem] 注册ProgressTableManager: {manager.GetIslandName()}");
        }
    }

    /// <summary>
    /// 获取所有ProgressTableManager
    /// </summary>
    public List<ProgressTableManager> GetAllProgressTableManagers()
    {
        return new List<ProgressTableManager>(progressTableManagers);
    }
}
