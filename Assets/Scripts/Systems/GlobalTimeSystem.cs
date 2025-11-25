using UnityEngine;
using System;

public class GlobalTimeSystem : MonoBehaviour
{
private const string SAVE_KEY_TOTAL_TIME = "GlobalTimeSystem_TotalElapsedTime";
private const string SAVE_KEY_FIRST_START = "GlobalTimeSystem_FirstStart";

    public static GlobalTimeSystem Instance { get; private set; }

    public enum MoonPhase
    {
        Crescent,
        UpQuarterMoon,
        FullMoon,
        DownQuarterMoon
    }

    [Header("配置")]
    public float phaseDuration = 30f; // 每个阶段持续时间（秒）

    [Header("状态 - 只读")]
    public bool isPaused = false;
    public MoonPhase CurrentPhase { get; private set; }
    public float PhaseProgress { get; private set; } // 0~1
    public int CycleCount { get; private set; }
    public float TotalElapsedTime { get; private set; } // 绝对时间（受暂停影响）

    // 缓存上一帧的值，用于检测变化
    private MoonPhase _lastPhase;
    private int _lastCycleCount;

    void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
        GlobalTimeSystem[] existing = FindObjectsOfType<GlobalTimeSystem>();
        foreach (var t in existing)
            if (t != this) DestroyImmediate(t.gameObject);
#endif

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSavedTime();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isPaused)
        {
            TotalElapsedTime += Time.deltaTime;
        }

        // 根据 TotalElapsedTime 同步计算所有状态
        SyncStateFromAbsoluteTime();

        // 检测阶段或周期变化，触发事件
        if (_lastPhase != CurrentPhase)
        {
            OnPhaseChanged?.Invoke(CurrentPhase);
            _lastPhase = CurrentPhase;

            if (CurrentPhase == MoonPhase.Crescent && _lastCycleCount != CycleCount)
            {
                OnNewCycle?.Invoke(CycleCount);
                _lastCycleCount = CycleCount;
            }
        }
    }

    void SyncStateFromAbsoluteTime()
    {
        float cycleDuration = phaseDuration * 4f;
        if (cycleDuration <= 0) return;

        // 计算完整周期数
        CycleCount = (int)(TotalElapsedTime / cycleDuration);

        // 当前周期内的偏移
        float timeInCycle = TotalElapsedTime % cycleDuration;

        // 当前阶段索引（0~3）
        int phaseIndex = Mathf.Clamp((int)(timeInCycle / phaseDuration), 0, 3);
        CurrentPhase = (MoonPhase)phaseIndex;

        // 阶段内进度（0~1）
        float timeInPhase = timeInCycle % phaseDuration;
        PhaseProgress = phaseDuration > 0 ? timeInPhase / phaseDuration : 0f;
    }

    public void ResetTime()
    {
        TotalElapsedTime = 0f;
        SyncStateFromAbsoluteTime(); // 立即同步状态

        // 触发重置事件（此时 CurrentPhase 已是 Crescent）
        OnTimeReset?.Invoke();
        OnPhaseChanged?.Invoke(CurrentPhase);
        OnNewCycle?.Invoke(CycleCount); // CycleCount=0
    }

    //获取格式化后的总时间字符串
    public string GetFormattedTotalTime()
    {
        int totalSeconds = Mathf.FloorToInt(TotalElapsedTime);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        return $"{hours}:{minutes:D2}:{seconds:D2}";
    }

    public MoonPhase GetCurrentMoonPhase()
    {
        return CurrentPhase;
    }

    void LoadSavedTime()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY_TOTAL_TIME))
        {
            TotalElapsedTime = PlayerPrefs.GetFloat(SAVE_KEY_TOTAL_TIME);
        }
        else
        {
            // 首次启动：记录首次启动时间戳（用于方案 A）
            PlayerPrefs.SetFloat(SAVE_KEY_FIRST_START, Time.realtimeSinceStartup);
            TotalElapsedTime = 0f;
        }

        SyncStateFromAbsoluteTime(); // 立即同步月相状态
    }

    public void SaveTime()
    {
        PlayerPrefs.SetFloat(SAVE_KEY_TOTAL_TIME, TotalElapsedTime);
        PlayerPrefs.Save(); // 确保立即写入磁盘
    }
    void OnApplicationQuit()
    {
        SaveTime();
    }

    public void Pause() => isPaused = true;
    public void Resume() => isPaused = false;
    public void TogglePause() => isPaused = !isPaused;

    // ===== 事件 =====
    public event Action<MoonPhase> OnPhaseChanged;
    public event Action<int> OnNewCycle;
    public event Action OnTimeReset;
}