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
    
    [Tooltip("要解锁的物种或线索的名称，必须与ScriptableObject中的名称完全一致")]
    [SerializeField] private string unlockTargetName;

    [Header("状态")]
    [SerializeField] private bool isPickedUp = false;

    [Header("交互设置")]
    [SerializeField] private float interactionRadius = 2.0f;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private AudioClip pickupSound;

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
            
            // 播放效果
            PlayPickupEffects();
            
            // 处理物体状态
            HandlePostPickup();

            Debug.Log($"成功捡起物体 {objectName}，解锁了：{unlockTargetName}");
        }
        else
        {
            Debug.LogWarning($"捡起物体 {objectName} 但解锁失败，目标：{unlockTargetName}");
        }
    }

    /// <summary>
    /// 执行解锁逻辑
    /// </summary>
    private bool PerformUnlock()
    {
        if (string.IsNullOrEmpty(unlockTargetName))
        {
            Debug.LogError($"物体 {objectName} 的解锁目标名称未设置");
            return false;
        }

        switch (objectCategory)
        {
            case ObjectCategory.Crop:
            case ObjectCategory.Animal:
            case ObjectCategory.Material:
                // 解锁物种
                return targetManager.UnlockSpecies(unlockTargetName);

            case ObjectCategory.Clue:
                // 解锁线索
                return targetManager.UnlockClue(unlockTargetName);

            default:
                Debug.LogError($"未知的物体类别: {objectCategory}");
                return false;
        }
    }

    /// <summary>
    /// 播放捡拾效果
    /// </summary>
    private void PlayPickupEffects()
    {
        // 播放音效
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 实例化粒子效果
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
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
            OnInteract();
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
    public string UnlockTargetName => unlockTargetName;
    public bool IsPickedUp => isPickedUp;
    public float InteractionRadius => interactionRadius;

    #endregion
}