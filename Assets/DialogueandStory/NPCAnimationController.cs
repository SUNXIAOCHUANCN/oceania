using UnityEngine;

/// <summary>
/// NPC动画控制器 - 负责播放NPC的对话手势动画
/// 与对话系统解耦，可独立控制NPC动画行为
/// </summary>
public class NPCAnimationController : MonoBehaviour
{
    [Header("Animator配置")]
    [SerializeField] private Animator animator;

    [Header("自动查找设置")]
    [SerializeField] private bool autoFindAnimatorInChildren = true;  // 是否自动在子对象中查找Animator
    [SerializeField] private string animatorObjectName = "NPC模型";  // 子对象名称（可选，用于精确查找）

    [Header("手势动画触发器")]
    [SerializeField] private string gestureATrigger = "A";
    [SerializeField] private string gestureBTrigger = "B";
    [SerializeField] private string gestureCTrigger = "C";

    [Header("动画设置")]
    [SerializeField] private bool playGestureOnDialogue = true;  // 对话时播放手势
    [SerializeField] private float minDelayBetweenGestures = 0.5f;  // 手势之间的最小间隔
    [SerializeField] private bool randomGesture = true;  // 是否随机选择手势

    [Header("调试")]
    [SerializeField] private bool showDebugLog = false;

    private float lastGestureTime = -999f;
    private int lastGestureIndex = -1;

    private void Awake()
    {
        // 如果没有手动指定Animator，自动获取
        if (animator == null)
        {
            if (autoFindAnimatorInChildren)
            {
                // 优先在指定名称的子对象中查找
                if (!string.IsNullOrEmpty(animatorObjectName))
                {
                    Transform child = transform.Find(animatorObjectName);
                    if (child != null)
                    {
                        animator = child.GetComponent<Animator>();
                        if (animator != null && showDebugLog)
                        {
                            Debug.Log($"在子对象 '{animatorObjectName}' 中找到Animator");
                        }
                    }
                }

                // 如果还没找到，在所有子对象中查找
                if (animator == null)
                {
                    animator = GetComponentInChildren<Animator>();
                    if (animator != null && showDebugLog)
                    {
                        Debug.Log($"在子对象中找到Animator: {animator.gameObject.name}");
                    }
                }
            }
            else
            {
                // 只在当前对象上查找
                animator = GetComponent<Animator>();
            }
        }

        if (animator == null)
        {
            Debug.LogWarning($"NPCAnimationController: {gameObject.name} 没有找到Animator组件！\n" +
                           $"提示：勾选 'Auto Find Animator In Children' 可自动在子对象中查找");
        }
    }

    /// <summary>
    /// 播放一个随机手势动画
    /// </summary>
    public void PlayRandomGesture()
    {
        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] PlayRandomGesture被调用，playGestureOnDialogue={playGestureOnDialogue}, animator={(animator != null ? "找到" : "未找到")}");
        }

        if (!playGestureOnDialogue)
        {
            if (showDebugLog)
            {
                Debug.LogWarning($"[{gameObject.name}] PlayGestureOnDialogue未勾选，不播放手势");
            }
            return;
        }

        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Animator为null，无法播放手势！请检查Animator是否正确配置");
            return;
        }

        // 检查是否可以播放新手势（防止过于频繁）
        if (Time.time - lastGestureTime < minDelayBetweenGestures)
        {
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 手势播放冷却中，还需等待 {minDelayBetweenGestures - (Time.time - lastGestureTime):F2}秒");
            }
            return;
        }

        // 随机选择一个手势（不重复上一个）
        int gestureIndex = RandomGesture();

        // 触发对应的动画
        TriggerGesture(gestureIndex);
        if (showDebugLog)
        {
            Debug.Log($"{gameObject.name} 播放手势动画: {GetTriggerName(gestureIndex)}");
        }

        lastGestureTime = Time.time;
    }

    /// <summary>
    /// 随机选择一个手势索引（避免连续重复）
    /// </summary>
    private int RandomGesture()
    {
        if (!randomGesture)
        {
            // 顺序循环：A -> B -> C -> A...
            return (lastGestureIndex + 1) % 3;
        }

        // 随机选择，但避免连续重复同一个
        int newIndex;
        do
        {
            newIndex = Random.Range(0, 3);
        } while (newIndex == lastGestureIndex);

        lastGestureIndex = newIndex;
        return newIndex;
    }

    /// <summary>
    /// 触发指定的手势动画
    /// </summary>
    /// <param name="gestureIndex">0=A, 1=B, 2=C</param>
    public void TriggerGesture(int gestureIndex)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator未设置，无法播放手势动画");
            return;
        }

        string triggerName = GetTriggerName(gestureIndex);

        if (showDebugLog)
        {
            Debug.Log($"{gameObject.name} 播放手势动画: {triggerName}");
        }

        animator.SetTrigger(triggerName);
        lastGestureIndex = gestureIndex;
    }

    /// <summary>
    /// 根据索引获取触发器名称
    /// </summary>
    private string GetTriggerName(int index)
    {
        switch (index)
        {
            case 0: return gestureATrigger;
            case 1: return gestureBTrigger;
            case 2: return gestureCTrigger;
            default: return gestureATrigger;
        }
    }

    /// <summary>
    /// 设置是否在对话时播放手势
    /// </summary>
    public void SetPlayGestureOnDialogue(bool enable)
    {
        playGestureOnDialogue = enable;
    }

    /// <summary>
    /// 设置手势播放模式（随机或顺序）
    /// </summary>
    public void SetRandomGesture(bool isRandom)
    {
        randomGesture = isRandom;
    }

    /// <summary>
    /// 重置动画状态
    /// </summary>
    public void ResetAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(gestureATrigger);
            animator.ResetTrigger(gestureBTrigger);
            animator.ResetTrigger(gestureCTrigger);
        }
    }

    /// <summary>
    /// 获取当前Animator
    /// </summary>
    public Animator GetAnimator()
    {
        return animator;
    }
}
