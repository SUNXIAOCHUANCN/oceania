using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLocationUI : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private Image locationImage;

    [Header("岛屿图像")]
    [SerializeField] private Sprite 长尾鸟岛Image;
    [SerializeField] private Sprite 十字星岛Image;
    [SerializeField] private Sprite 海上Image;

    private void OnEnable()
    {
        // 订阅位置变更事件（需要在 PlayerModeManager 中添加）
        if (PlayerModeManager.Instance != null)
        {
            PlayerModeManager.Instance.OnLocationChanged += UpdateLocationUI;
        }
    }

    private void OnDisable()
    {
        // 取消订阅
        if (PlayerModeManager.Instance != null)
        {
            PlayerModeManager.Instance.OnLocationChanged -= UpdateLocationUI;
        }
    }

    private void Start()
    {
        // 初始化时立即更新一次UI
        if (PlayerStateManager.Instance != null)
        {
            UpdateLocationUI(PlayerStateManager.Instance.CurrentLocation);
        }
    }

    /// <summary>
    /// 更新位置显示
    /// </summary>
    private void UpdateLocationUI(PlayerLocationState location)
    {
        // 更新文字
        if (locationText != null)
        {
            locationText.text = GetLocationName(location);
        }

        // 更新图像
        if (locationImage != null)
        {
            locationImage.sprite = GetLocationSprite(location);
        }
    }

    /// <summary>
    /// 获取位置显示名称
    /// </summary>
    private string GetLocationName(PlayerLocationState location)
    {
        return location.ToString();
    }

    /// <summary>
    /// 根据位置获取对应图像
    /// </summary>
    private Sprite GetLocationSprite(PlayerLocationState location)
    {
        switch (location)
        {
            case PlayerLocationState.长尾鸟岛:
                return 长尾鸟岛Image;
            case PlayerLocationState.十字星岛:
                return 十字星岛Image;
            case PlayerLocationState.海上:
                return 海上Image;
            case PlayerLocationState.热火山岛:
            case PlayerLocationState.无名花岛:
                return null; // 没有对应的图像
            default:
                return null;
        }
    }
}
