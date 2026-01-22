using UnityEngine;

/// <summary>
/// 交互物体 - 可被玩家捡拾并解锁物种或线索的物体
/// </summary>
public class InteractableObject : MonoBehaviour
{
    /// <summary>
    /// 物体类别
    /// </summary>
    public enum ObjectCategory
    {
        Crop,     // 作物
        Animal,   // 动物
        Material, // 材料
        Clue      // 线索
    }

    [Header("基础信息")]
    [SerializeField] private string objectName = "未命名物体";
    [SerializeField] private SpeciesSource relatedIsland;

    [Header("解锁配置")]
    [SerializeField] private ObjectCategory objectCategory;

    [Tooltip("直接引用要解锁的物种ScriptableObject")]
    [SerializeField] private SpeciesScriptableObject speciesTarget;

    [Tooltip("直接引用要解锁的线索ScriptableObject")]
    [SerializeField] private ClueScriptableObject clueTarget;

    [Header("状态")]
    [SerializeField] private bool isPickedUp = false;

    [Header("交互设置")]
    [SerializeField] private float interactionRadius = 2.0f;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool requireManualInteraction = true; // 是否需要手动交互

    [Header("UI引用（手动配置）")]
    [SerializeField] private InteractUIController speciesUIController;
    [SerializeField] private ClueUIController clueUIController;

    // 缓存的ProgressTableManager引用
    private ProgressTableManager targetManager;
    private bool isManagerCached = false;

    private void Start()
    {
        CacheProgressTableManager();
        SetupTriggerCollider();
    }

    /// <summary>
    /// 缓存对应岛屿的ProgressTableManager
    /// </summary>
    private void CacheProgressTableManager()
    {
        if (isManagerCached) return;

        // 根据岛屿标签查找管理器
        string islandTag = $"Island_{relatedIsland}";
        GameObject islandObject = GameObject.FindWithTag(islandTag);
        
        if (islandObject != null)
        {
            targetManager = islandObject.GetComponent<ProgressTableManager>();
            if (targetManager != null)
            {
                isManagerCached = true;
                Debug.Log($"InteractableObject '{objectName}' 成功缓存 {relatedIsland} 的 ProgressTableManager");
            }
            else
            {
                Debug.LogError($"InteractableObject '{objectName}': 岛屿对象 {islandTag} 上未找到 ProgressTableManager 组件");
            }
        }
        else
        {
            Debug.LogError($"InteractableObject '{objectName}': 未找到标签为 {islandTag} 的岛屿对象");
        }
    }

    /// <summary>
    /// 设置触发器碰撞体
    /// </summary>
    private void SetupTriggerCollider()
    {
        SphereCollider collider = gameObject.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        
        collider.isTrigger = true;
        collider.radius = interactionRadius;
    }

    /// <summary>
    /// 玩家交互时调用（例如按E键或自动触发）
    /// </summary>
    public void OnInteract()
    {
        if (isPickedUp)
        {
            Debug.LogWarning($"物体 {objectName} 已经被捡起，无法重复交互");
            return;
        }

        if (!isManagerCached || targetManager == null)
        {
            Debug.LogError($"物体 {objectName} 无法交互：未找到ProgressTableManager");
            return;
        }

        // 执行解锁
        bool unlockSuccess = PerformUnlock();

        if (unlockSuccess)
        {
            // 标记为已捡起
            isPickedUp = true;

            // 播放音效
            PlayPickupSound();

            // 显示UI
            ShowPickupUI();

            // 处理物体状态
            HandlePostPickup();

            // 获取解锁目标的名称用于日志
            string targetNameForLog = GetTargetDisplayName();
            Debug.Log($"成功捡起物体 {objectName}，解锁了：{targetNameForLog}");
        }
        else
        {
            string targetNameForLog = GetTargetDisplayName();
            Debug.LogWarning($"捡起物体 {objectName} 但解锁失败，目标：{targetNameForLog}");
        }
    }

    /// <summary>
    /// 获取解锁目标的显示名称
    /// </summary>
    private string GetTargetDisplayName()
    {
        switch (objectCategory)
        {
            case ObjectCategory.Crop:
            case ObjectCategory.Animal:
            case ObjectCategory.Material:
                return speciesTarget != null ? speciesTarget.speciesName : "未设置";
            case ObjectCategory.Clue:
                return clueTarget != null ? clueTarget.clueName : "未设置";
            default:
                return "未知";
        }
    }

    /// <summary>
    /// 执行解锁逻辑
    /// </summary>
    private bool PerformUnlock()
    {
        switch (objectCategory)
        {
            case ObjectCategory.Crop:
            case ObjectCategory.Animal:
            case ObjectCategory.Material:
                if (speciesTarget != null)
                {
                    string targetName = speciesTarget.speciesName;
                    bool success = targetManager.UnlockSpecies(targetName);

                    if (success)
                    {
                        Debug.Log($"成功通过 ScriptableObject 引用解锁物种: {targetName}");
                    }
                    return success;
                }
                else
                {
                    Debug.LogError($"物体 {objectName}: 未设置物种 ScriptableObject 引用");
                    return false;
                }

            case ObjectCategory.Clue:
                if (clueTarget != null)
                {
                    string targetName = clueTarget.clueName;
                    bool success = targetManager.UnlockClue(targetName);

                    if (success)
                    {
                        Debug.Log($"成功通过 ScriptableObject 引用解锁线索: {targetName}");
                    }
                    return success;
                }
                else
                {
                    Debug.LogError($"物体 {objectName}: 未设置线索 ScriptableObject 引用");
                    return false;
                }

            default:
                Debug.LogError($"未知的物体类别: {objectCategory}");
                return false;
        }
    }

    /// <summary>
    /// 显示捡拾UI
    /// </summary>
    private void ShowPickupUI()
    {
        switch (objectCategory)
        {
            case ObjectCategory.Crop:
            case ObjectCategory.Animal:
            case ObjectCategory.Material:
                // 显示物种UI
                if (speciesTarget != null && speciesUIController != null)
                {
                    speciesUIController.ShowSpecies(speciesTarget.speciesName, speciesTarget.icon);
                }
                else if (speciesTarget != null && speciesUIController == null)
                {
                    Debug.LogWarning($"物体 {objectName}: 未配置 SpeciesUIController，请在 Inspector 中手动拖拽配置");
                }
                break;

            case ObjectCategory.Clue:
                // 显示线索UI
                if (clueTarget != null && clueUIController != null)
                {
                    clueUIController.ShowClue(clueTarget.clueName, clueTarget.clueText);
                }
                else if (clueTarget != null && clueUIController == null)
                {
                    Debug.LogWarning($"物体 {objectName}: 未配置 ClueUIController，请在 Inspector 中手动拖拽配置");
                }
                break;
        }
    }

    /// <summary>
    /// 播放捡拾音效
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    /// <summary>
    /// 处理捡起后的物体状态
    /// </summary>
    private void HandlePostPickup()
    {
        if (destroyOnPickup)
        {
            // 销毁物体
            Destroy(gameObject);
        }
        else
        {
            // 禁用交互功能但保留物体
            Collider collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
            
            // 可以改为播放"已收集"的动画或改变材质
        }
    }

    /// <summary>
    /// 自动交互：当玩家进入触发器范围时自动捡起
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            if (requireManualInteraction)
            {
                // 显示交互提示UI
                var promptUI = FindObjectOfType<InteractionPromptUI>();
                if (promptUI != null)
                {
                    promptUI.ShowPrompt(this);
                }
                else
                {
                    Debug.LogWarning("未找到 InteractionPromptUI，请确保场景中有该组件");
                }
            }
            else
            {
                // 自动捡起
                OnInteract();
            }
        }
    }

    /// <summary>
    /// 玩家离开触发器范围
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 隐藏交互提示UI
            var promptUI = FindObjectOfType<InteractionPromptUI>();
            if (promptUI != null)
            {
                promptUI.HidePrompt();
            }
        }
    }

    /// <summary>
    /// 手动交互：由外部控制器调用
    /// </summary>
    public void ManualInteract()
    {
        OnInteract();
    }

    /// <summary>
    /// 在编辑器中可视化交互范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    #region 公共属性访问器

    public string ObjectName => objectName;
    public SpeciesSource RelatedIsland => relatedIsland;
    public ObjectCategory Category => objectCategory;
    public SpeciesScriptableObject SpeciesTarget => speciesTarget;
    public ClueScriptableObject ClueTarget => clueTarget;
    public bool IsPickedUp => isPickedUp;
    public float InteractionRadius => interactionRadius;

    #endregion
}