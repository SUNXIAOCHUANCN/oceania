using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 交互UI控制器 - 显示捡拾到的物种信息
/// </summary>
public class InteractUIController : CanvasController
{
    [Header("UI组件引用")]
    [SerializeField] private TextMeshProUGUI speciesNameText;
    [SerializeField] private Image speciesIconImage;

    [Header("显示配置")]
    [SerializeField] private float displayDuration = 2f; // 显示时长（秒）

    private float displayTimer = 0f;
    private bool isDisplaying = false;

    protected override void Start()
    {
        base.Start();
        // 初始时隐藏当前Panel，但不影响父Canvas
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // 自动关闭倒计时
        if (isDisplaying && displayTimer > 0f)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0f)
            {
                HideCanvas();
                isDisplaying = false;
            }
        }
    }

    /// <summary>
    /// 显示物种信息
    /// </summary>
    /// <param name="speciesName">物种名称</param>
    /// <param name="speciesIcon">物种图标</param>
    public void ShowSpecies(string speciesName, Sprite speciesIcon)
    {
        // 更新UI内容
        if (speciesNameText != null)
        {
            speciesNameText.text = speciesName;
        }

        if (speciesIconImage != null)
        {
            speciesIconImage.sprite = speciesIcon;
            // 如果图标为空，可以隐藏图片组件
            speciesIconImage.enabled = speciesIcon != null;
        }

        // 显示当前Panel
        ShowCanvas();

        // 启动自动关闭计时器
        displayTimer = displayDuration;
        isDisplaying = true;
    }

    /// <summary>
    /// 设置显示时长
    /// </summary>
    /// <param name="duration">显示时长（秒）</param>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
    }

    /// <summary>
    /// 重写隐藏方法，重置状态
    /// </summary>
    public override void HideCanvas()
    {
        base.HideCanvas();
        displayTimer = 0f;
        isDisplaying = false;

        // 不再隐藏父Canvas，避免影响其他子Panel
    }

    #region 属性访问器

    public TextMeshProUGUI SpeciesNameText => speciesNameText;
    public Image SpeciesIconImage => speciesIconImage;
    public float DisplayDuration => displayDuration;

    #endregion
}
