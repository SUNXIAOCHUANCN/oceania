using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 线索UI控制器 - 显示捡拾到的线索信息
/// </summary>
public class ClueUIController : CanvasController
{
    [Header("UI组件引用")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("显示配置")]
    [SerializeField] private float autoHideDelay = 0f; // 设置为0表示需要手动关闭

    private float autoHideTimer = 0f;

    protected override void Start()
    {
        base.Start();

        // 初始时隐藏当前Panel，但不影响父Canvas
        gameObject.SetActive(false);

        // 绑定关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        else
        {
            Debug.LogWarning("ClueUIController: 关闭按钮未设置");
        }
    }

    private void Update()
    {
        // 自动关闭倒计时
        if (autoHideDelay > 0f && autoHideTimer > 0f)
        {
            autoHideTimer -= Time.deltaTime;
            if (autoHideTimer <= 0f)
            {
                HideCanvas();
            }
        }
    }

    /// <summary>
    /// 显示线索信息
    /// </summary>
    /// <param name="clueTitle">线索标题</param>
    /// <param name="clueDescription">线索描述</param>
    public void ShowClue(string clueTitle, string clueDescription)
    {
        // 更新UI内容
        if (titleText != null)
        {
            titleText.text = clueTitle;
        }

        if (descriptionText != null)
        {
            descriptionText.text = clueDescription;
        }

        // 显示UI
        ShowCanvas();

        // 启动自动关闭计时器
        if (autoHideDelay > 0f)
        {
            autoHideTimer = autoHideDelay;
        }
    }

    /// <summary>
    /// 设置自动关闭延迟时间
    /// </summary>
    /// <param name="delay">延迟时间（秒），设为0需要手动关闭</param>
    public void SetAutoHideDelay(float delay)
    {
        autoHideDelay = delay;
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void OnCloseButtonClicked()
    {
        HideCanvas();
    }

    /// <summary>
    /// 重写隐藏方法，重置计时器
    /// </summary>
    public override void HideCanvas()
    {
        base.HideCanvas();
        autoHideTimer = 0f;

        // 不再隐藏父Canvas，避免影响其他子Panel
    }

    protected virtual void OnDestroy()
    {
        // 清理事件监听
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }

    #region 属性访问器

    public Button CloseButton => closeButton;
    public TextMeshProUGUI TitleText => titleText;
    public TextMeshProUGUI DescriptionText => descriptionText;

    #endregion
}
