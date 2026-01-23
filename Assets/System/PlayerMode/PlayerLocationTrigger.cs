using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerLocationTrigger : MonoBehaviour
{
    public PlayerLocationState location;
    [SerializeField] private string playerTag = "Player";

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
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
