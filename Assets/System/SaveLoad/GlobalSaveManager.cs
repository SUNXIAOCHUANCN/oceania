using System.Collections;
using UnityEngine;

/// <summary>
/// 全局存档管理器 - 统一管理所有系统的存档和读档
/// </summary>
public class GlobalSaveManager : MonoBehaviour
{
    public static GlobalSaveManager Instance { get; private set; }

    [Header("系统引用")]
    [SerializeField] private GlobalTimeSystem globalTimeSystem;
    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private ForestSaveSystem forestSaveSystem;
    [SerializeField] private FarmSaveSystem farmSaveSystem;
    [SerializeField] private RanchSaveSystem ranchSaveSystem;
    [SerializeField] private ExploreSaveSystem exploreSaveSystem;
    [SerializeField] private PersonSaveSystem personSaveSystem;

    [Header("生产系统引用")]
    [SerializeField] private ForestSystem forestSystem;
    [SerializeField] private FarmSystem farmSystem;
    [SerializeField] private RanchSystem ranchSystem;

    [Header("探索系统引用")]
    [SerializeField] private ExploreSystem exploreSystem;

    [Header("人口系统引用")]
    [SerializeField] private PersonManager personManager;

    // 加载完成事件
    public System.Action OnAllDataLoaded;

    private bool isLoading = false;
    private bool wasTimePausedBeforeLoad = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 自动查找系统引用（如果未手动赋值）
        FindSystemReferences();

        // 延迟一帧后自动加载存档，确保所有系统都已初始化
        StartCoroutine(AutoLoadOnStart());
        Debug.Log("[GlobalSaveManager] 自动加载存档已启动");
    }

    /// <summary>
    /// 游戏启动时自动加载存档
    /// </summary>
    private IEnumerator AutoLoadOnStart()
    {
        Debug.Log("[GlobalSaveManager] 自动加载存档...");
        // 等待一帧，确保所有系统的 Awake 都已执行
        yield return null;

        // 检查是否有存档
        if (HasAnySaveData())
        {
            Debug.Log("[GlobalSaveManager] 检测到存档，开始自动加载...");
            LoadAllData();
        }
        else
        {
            Debug.Log("[GlobalSaveManager] 未检测到存档，使用初始状态");
        }
    }

    /// <summary>
    /// 查找所有需要的系统引用
    /// </summary>
    private void FindSystemReferences()
    {
        Debug.Log("[GlobalSaveManager] 查找系统引用...");

        if (globalTimeSystem == null)
            globalTimeSystem = GlobalTimeSystem.Instance;
        Debug.Log($"  GlobalTimeSystem: {(globalTimeSystem != null ? "✓" : "✗")}");

        if (playerStateManager == null)
            playerStateManager = PlayerStateManager.Instance;
        Debug.Log($"  PlayerStateManager: {(playerStateManager != null ? "✓" : "✗")}");

        if (forestSaveSystem == null)
            forestSaveSystem = ForestSaveSystem.Instance;
        Debug.Log($"  ForestSaveSystem: {(forestSaveSystem != null ? "✓" : "✗")}");

        if (farmSaveSystem == null)
            farmSaveSystem = FarmSaveSystem.Instance;
        Debug.Log($"  FarmSaveSystem: {(farmSaveSystem != null ? "✓" : "✗")}");

        if (ranchSaveSystem == null)
            ranchSaveSystem = RanchSaveSystem.Instance;
        Debug.Log($"  RanchSaveSystem: {(ranchSaveSystem != null ? "✓" : "✗")}");

        if (forestSystem == null)
            forestSystem = FindObjectOfType<ForestSystem>();
        Debug.Log($"  ForestSystem: {(forestSystem != null ? "✓" : "✗")}");

        if (farmSystem == null)
            farmSystem = FindObjectOfType<FarmSystem>();
        Debug.Log($"  FarmSystem: {(farmSystem != null ? "✓" : "✗")}");

        if (ranchSystem == null)
            ranchSystem = FindObjectOfType<RanchSystem>();
        Debug.Log($"  RanchSystem: {(ranchSystem != null ? "✓" : "✗")}");

        if (exploreSaveSystem == null)
            exploreSaveSystem = ExploreSaveSystem.Instance;
        Debug.Log($"  ExploreSaveSystem: {(exploreSaveSystem != null ? "✓" : "✗")}");

        if (exploreSystem == null)
            exploreSystem = ExploreSystem.Instance;
        Debug.Log($"  ExploreSystem: {(exploreSystem != null ? "✓" : "✗")}");

        if (personSaveSystem == null)
            personSaveSystem = PersonSaveSystem.Instance;
        Debug.Log($"  PersonSaveSystem: {(personSaveSystem != null ? "✓" : "✗")}");

        if (personManager == null)
            personManager = PersonManager.Instance;
        Debug.Log($"  PersonManager: {(personManager != null ? "✓" : "✗")}");

        if (VoyageSystemManager.Instance != null)
            Debug.Log($"  VoyageSystemManager: ✓");
        else
            Debug.Log($"  VoyageSystemManager: ✗");
    }

    /// <summary>
    /// 保存所有游戏数据
    /// </summary>
    public void SaveAllData()
    {
        Debug.Log("[GlobalSaveManager] 开始保存所有数据...");

        // 1. 保存全局时间
        if (globalTimeSystem != null)
        {
            globalTimeSystem.SaveTime();
            Debug.Log("  ✓ 全局时间已保存");
        }

        // 2. 保存玩家状态
        if (playerStateManager != null)
        {
            playerStateManager.SavePlayerState();
            Debug.Log("  ✓ 玩家状态已保存");
        }

        // 3. 保存森林数据
        if (forestSaveSystem != null && forestSystem != null)
        {
            forestSaveSystem.SaveForestData(forestSystem);
            Debug.Log("  ✓ 森林数据已保存");
        }

        // 4. 保存农场数据
        if (farmSaveSystem != null && farmSystem != null)
        {
            farmSaveSystem.SaveFarmData(farmSystem);
            Debug.Log("  ✓ 农场数据已保存");
        }

        // 5. 保存牧场数据
        if (ranchSaveSystem != null && ranchSystem != null)
        {
            ranchSaveSystem.SaveRanchData(ranchSystem);
            Debug.Log("  ✓ 牧场数据已保存");
        }

        // 6. 保存探索数据
        if (exploreSaveSystem != null && exploreSystem != null)
        {
            ExploreSaveData exploreData = exploreSystem.ExportToSaveData();
            exploreSaveSystem.SaveExploreData(exploreData);
            Debug.Log("  ✓ 探索数据已保存");
        }

        // 7. 保存人口数据
        if (personSaveSystem != null && personManager != null)
        {
            PopulationSaveData populationData = personManager.ExportToSaveData();
            personSaveSystem.SavePopulationData(populationData);
            Debug.Log("  ✓ 人口数据已保存");
        }

        Debug.Log("[GlobalSaveManager] 所有数据保存完成！");
    }

    /// <summary>
    /// 加载所有游戏数据
    /// </summary>
    public void LoadAllData()
    {
        if (isLoading)
        {
            Debug.LogWarning("[GlobalSaveManager] 正在加载中，请勿重复调用");
            return;
        }

        StartCoroutine(LoadAllDataCoroutine());
    }

    private IEnumerator LoadAllDataCoroutine()
    {
        isLoading = true;
        Debug.Log("[GlobalSaveManager] 开始加载所有数据...");

        // 暂停时间系统，防止在加载期间时间继续流逝
        if (globalTimeSystem != null)
        {
            wasTimePausedBeforeLoad = globalTimeSystem.isPaused;
            if (!wasTimePausedBeforeLoad)
            {
                globalTimeSystem.Pause();
                Debug.Log("[GlobalSaveManager] 已暂停时间系统，防止加载期间时间流逝");
            }
        }

        // 重新查找系统引用，确保所有系统都已初始化
        FindSystemReferences();

        // 等待一帧，确保所有系统都已初始化
        yield return null;

        // 1. 全局时间在 Awake 时已自动加载，跳过
        if (globalTimeSystem != null)
        {
            Debug.Log("  ✓ 全局时间已加载 (Awake时自动加载)");
        }

        // 2. 加载玩家状态
        if (playerStateManager != null)
        {
            bool playerLoaded = playerStateManager.LoadPlayerState();
            Debug.Log(playerLoaded ? "  ✓ 玩家状态已加载" : "  ⚠ 玩家存档不存在，使用初始状态");
        }

        // 等待物种和人员加载器完成
        yield return new WaitUntil(() => SpeciesLoader.Instance != null);
        yield return new WaitUntil(() => PersonsLoader.Instance != null);
        yield return null; // 等待加载器完成 LoadPersons/LoadSpecies

        // 3. 加载森林数据
        if (forestSaveSystem != null && forestSystem != null)
        {
            bool forestLoaded = forestSaveSystem.LoadForestData(forestSystem);
            Debug.Log(forestLoaded ? "  ✓ 森林数据已加载" : "  ⚠ 森林存档不存在，使用初始状态");
        }

        // 4. 加载农场数据
        if (farmSaveSystem != null && farmSystem != null)
        {
            bool farmLoaded = farmSaveSystem.LoadFarmData(farmSystem);
            Debug.Log(farmLoaded ? "  ✓ 农场数据已加载" : "  ⚠ 农场存档不存在，使用初始状态");
        }

        // 5. 加载牧场数据
        if (ranchSaveSystem != null && ranchSystem != null)
        {
            bool ranchLoaded = ranchSaveSystem.LoadRanchData(ranchSystem);
            Debug.Log(ranchLoaded ? "  ✓ 牧场数据已加载" : "  ⚠ 牧场存档不存在，使用初始状态");
        }

        // 6. 加载探索数据
        if (exploreSaveSystem != null && exploreSystem != null)
        {
            ExploreSaveData exploreData = exploreSaveSystem.LoadExploreData();
            exploreSystem.LoadFromSaveData(exploreData);
            Debug.Log($"  ✓ 探索数据已加载 ({exploreData.islandsExploreData.Count} 个岛屿)");
        }

        // 7. 加载人口数据
        if (personSaveSystem != null && personManager != null)
        {
            PopulationSaveData populationData = personSaveSystem.LoadPopulationData();
            personManager.LoadFromSaveData(populationData);
            Debug.Log($"  ✓ 人口数据已加载 ({populationData.personsData.Count} 个人员)");
        }

        Debug.Log("[GlobalSaveManager] 所有数据加载完成！");

        isLoading = false;

        // 恢复时间系统的运行状态
        if (globalTimeSystem != null && !wasTimePausedBeforeLoad)
        {
            globalTimeSystem.Resume();
            Debug.Log("[GlobalSaveManager] 已恢复时间系统运行");
        }

        OnAllDataLoaded?.Invoke();
    }

    /// <summary>
    /// 检查是否存在任何存档
    /// </summary>
    public bool HasAnySaveData()
    {
        bool hasSave =
            (playerStateManager != null && playerStateManager.SaveExists()) ||
            (forestSaveSystem != null && forestSaveSystem.SaveExists()) ||
            (farmSaveSystem != null && farmSaveSystem.SaveExists()) ||
            (ranchSaveSystem != null && ranchSaveSystem.SaveExists()) ||
            (exploreSaveSystem != null && exploreSaveSystem.SaveExists()) ||
            (personSaveSystem != null && personSaveSystem.SaveExists()) ||
            PlayerPrefs.HasKey("GlobalTimeSystem_TotalElapsedTime");

        return hasSave;
    }

    /// <summary>
    /// 删除所有存档（用于新游戏）
    /// </summary>
    public void DeleteAllSaveData()
    {
        Debug.Log("[GlobalSaveManager] 删除所有存档...");

        if (playerStateManager != null)
            playerStateManager.DeleteSaveFile();

        // 森林、农场、牧场、探索、人口的存档文件需要手动删除
        string forestSavePath = System.IO.Path.Combine(Application.persistentDataPath, "forest_save.dat");
        string farmSavePath = System.IO.Path.Combine(Application.persistentDataPath, "farm_save.dat");
        string ranchSavePath = System.IO.Path.Combine(Application.persistentDataPath, "ranch_save.dat");
        string exploreSavePath = System.IO.Path.Combine(Application.persistentDataPath, "explore_save.dat");
        string populationSavePath = System.IO.Path.Combine(Application.persistentDataPath, "population_save.dat");

        if (System.IO.File.Exists(forestSavePath))
            System.IO.File.Delete(forestSavePath);

        if (System.IO.File.Exists(farmSavePath))
            System.IO.File.Delete(farmSavePath);

        if (System.IO.File.Exists(ranchSavePath))
            System.IO.File.Delete(ranchSavePath);

        if (System.IO.File.Exists(exploreSavePath))
            System.IO.File.Delete(exploreSavePath);

        if (System.IO.File.Exists(populationSavePath))
            System.IO.File.Delete(populationSavePath);

        // 删除 PlayerPrefs 的时间存档
        PlayerPrefs.DeleteKey("GlobalTimeSystem_TotalElapsedTime");
        PlayerPrefs.DeleteKey("GlobalTimeSystem_FirstStart");

        Debug.Log("[GlobalSaveManager] 所有存档已删除");
    }

    /// <summary>
    /// 游戏退出时自动保存所有数据
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveAllData();
    }
}
