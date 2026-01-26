using UnityEngine;
using TMPro;

public class PlayerLocationUI : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI locationText;

    void Update()
    {
        // 每帧更新UI
        if (PlayerLocationManager.Instance != null && locationText != null)
        {
            PlayerLocationManager.LocationState currentLocation = PlayerLocationManager.Instance.GetCurrentLocation();
            locationText.text = currentLocation.ToString();
        }
    }
}
