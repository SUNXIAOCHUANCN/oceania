using UnityEngine;
using System.Collections;

public class MoonPhaseMusicController : MonoBehaviour
{
    [Header("音乐配置")]
    [Tooltip("峨眉月音乐（可留空）")]
    public AudioClip crescentMusic;
    [Tooltip("上弦月音乐（可留空）")]
    public AudioClip upQuarterMusic;
    [Tooltip("满月音乐（可留空）")]
    public AudioClip fullMoonMusic;
    [Tooltip("下弦月音乐（可留空）")]
    public AudioClip downQuarterMusic;

    [Header("播放设置")]
    [Range(0f, 1f)]
    [Tooltip("音乐音量")]
    public float musicVolume = 0.7f;

    [Header("过渡设置")]
    [Tooltip("淡入淡出时长（秒）")]
    public float fadeDuration = 10f;
    [Tooltip("月相切换后的等待时长（秒）")]
    public float waitDuration = 10f;

    [Header("音乐播放时长")]
    [Tooltip("每个音乐播放的最大时长（秒）")]
    public float musicPlayDuration = 60f;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;
    private Coroutine _playLimitCoroutine;

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.loop = false;
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0f;
    }

    void OnEnable()
    {
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases += HandlePhaseChanged;
        }
    }

    void OnDisable()
    {
        if (GlobalTimeSystem.Instance != null)
        {
            GlobalTimeSystem.Instance.OnPhaseChangedWithTotalPhases -= HandlePhaseChanged;
        }
    }

    void Start()
    {
        // 游戏开始时，根据当前月相播放音乐
        if (GlobalTimeSystem.Instance != null)
        {
            PlayMusicForPhase(GlobalTimeSystem.Instance.CurrentPhase);
        }
    }

    private void HandlePhaseChanged(GlobalTimeSystem.MoonPhase newPhase, int totalPhases)
    {
        PlayMusicForPhase(newPhase);
    }

    private void PlayMusicForPhase(GlobalTimeSystem.MoonPhase phase)
    {
        AudioClip clipToPlay = GetClipForPhase(phase);

        // 如果当前正在播放，先淡出
        if (_audioSource.isPlaying)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeOutThenPlay(clipToPlay));
        }
        else
        {
            // 如果没有播放，直接开始新的播放流程
            if (clipToPlay != null)
            {
                _fadeCoroutine = StartCoroutine(WaitThenFadeIn(clipToPlay));
            }
        }
    }

    private AudioClip GetClipForPhase(GlobalTimeSystem.MoonPhase phase)
    {
        switch (phase)
        {
            case GlobalTimeSystem.MoonPhase.Crescent:
                return crescentMusic;
            case GlobalTimeSystem.MoonPhase.UpQuarterMoon:
                return upQuarterMusic;
            case GlobalTimeSystem.MoonPhase.FullMoon:
                return fullMoonMusic;
            case GlobalTimeSystem.MoonPhase.DownQuarterMoon:
                return downQuarterMusic;
            default:
                return null;
        }
    }

    private IEnumerator FadeOutThenPlay(AudioClip nextClip)
    {
        // 淡出
        yield return StartCoroutine(FadeOut());

        // 停止当前音乐
        _audioSource.Stop();
        _audioSource.clip = null;

        // 如果有新音乐，等待后淡入
        if (nextClip != null)
        {
            yield return new WaitForSeconds(waitDuration);
            yield return StartCoroutine(FadeIn(nextClip));
        }
    }

    private IEnumerator WaitThenFadeIn(AudioClip clip)
    {
        yield return new WaitForSeconds(waitDuration);
        yield return StartCoroutine(FadeIn(clip));
    }

    private IEnumerator FadeIn(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.time = 0f;
        _audioSource.Play();

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            _audioSource.volume = Mathf.Lerp(0f, musicVolume, progress);
            yield return null;
        }

        _audioSource.volume = musicVolume;

        // 开始限制播放时长的协程
        if (_playLimitCoroutine != null)
        {
            StopCoroutine(_playLimitCoroutine);
        }
        _playLimitCoroutine = StartCoroutine(StopAfterDuration());
    }

    private IEnumerator FadeOut()
    {
        float startVolume = _audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration && _audioSource.volume > 0f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        _audioSource.volume = 0f;
    }

    private IEnumerator StopAfterDuration()
    {
        yield return new WaitForSeconds(musicPlayDuration);

        // 到达播放时长限制，淡出并停止
        if (_audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut());
            _audioSource.Stop();
            _audioSource.clip = null;
        }
    }

    // 测试用方法：手动触发播放某个月相的音乐
    [ContextMenu("测试：播放当前月相音乐")]
    public void TestPlayCurrentPhase()
    {
        if (GlobalTimeSystem.Instance != null)
        {
            PlayMusicForPhase(GlobalTimeSystem.Instance.CurrentPhase);
        }
    }

    // 测试用方法：立即停止所有音乐
    [ContextMenu("测试：立即停止音乐")]
    public void TestStopMusic()
    {
        StopAllCoroutines();
        if (_fadeCoroutine != null) _fadeCoroutine = null;
        if (_playLimitCoroutine != null) _playLimitCoroutine = null;

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.volume = 0f;
            _audioSource.clip = null;
        }
    }
}
