using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerLocationTrigger : MonoBehaviour
{
    public PlayerLocationState location;
    [SerializeField] private string playerTag = "Player";

    private Collider triggerCollider;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
    }

    void Start()
    {
        // 检查玩家是否已经在触发器内（处理出生在触发器内的情况）
        if (PlayerModeManager.Instance != null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                Collider playerCollider = playerController.GetComponent<Collider>();
                if (playerCollider != null && triggerCollider.bounds.Intersects(playerCollider.bounds))
                {
                    PlayerModeManager.Instance.OnEnterLocationTrigger(location);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (PlayerModeManager.Instance == null) return;
        PlayerModeManager.Instance.OnEnterLocationTrigger(location);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (PlayerModeManager.Instance == null) return;
        PlayerModeManager.Instance.OnExitLocationTrigger(location);
    }
}
