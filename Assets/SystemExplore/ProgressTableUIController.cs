using UnityEngine;
using UnityEngine.UI;

public class ProgressTableUIController : CanvasController
{
    [Header("页面引用")]
    [SerializeField] private GameObject startPage;
    [SerializeField] private GameObject mainIslandPage;
    [SerializeField] private GameObject crossIslandPage;

    [Header("岛屿管理器引用")]
    [SerializeField] private ProgressTableManager mainIslandManager;
    [SerializeField] private ProgressTableManager crossIslandManager;

    [Header("Start Page按钮")]
    [SerializeField] private Button startPageExitButton;
    [SerializeField] private Button mainIslandButton;
    [SerializeField] private Button crossIslandButton;

    [Header("岛屿页面退出按钮")]
    [SerializeField] private Button mainIslandExitButton;
    [SerializeField] private Button crossIslandExitButton;

    private GameObject currentActivePage;

    protected override void Start()
    {
        base.Start();
        
        // 初始化按钮监听
        InitializeButtonListeners();
        
        // 初始显示startPage
        ShowStartPage();
    }

    private void InitializeButtonListeners()
    {
        // Start Page按钮
        if (startPageExitButton != null)
            startPageExitButton.onClick.AddListener(OnExitButtonClicked);

        if (mainIslandButton != null)
            mainIslandButton.onClick.AddListener(() => ShowIslandPage(mainIslandPage, mainIslandManager));

        if (crossIslandButton != null)
            crossIslandButton.onClick.AddListener(() => ShowIslandPage(crossIslandPage, crossIslandManager));

        // 岛屿页面退出按钮
        if (mainIslandExitButton != null)
            mainIslandExitButton.onClick.AddListener(ShowStartPage);

        if (crossIslandExitButton != null)
            crossIslandExitButton.onClick.AddListener(ShowStartPage);
    }

    private void ShowStartPage()
    {
        // 隐藏当前页面
        if (currentActivePage != null)
            currentActivePage.SetActive(false);

        // 显示startPage
        startPage.SetActive(true);
        currentActivePage = startPage;
    }

    private void ShowIslandPage(GameObject islandPage, ProgressTableManager manager)
    {
        // 隐藏当前页面
        if (currentActivePage != null)
            currentActivePage.SetActive(false);

        // 显示目标岛屿页面
        islandPage.SetActive(true);
        currentActivePage = islandPage;

        // 更新岛屿页面内容
        UpdateIslandPage(islandPage, manager);
    }

    private void UpdateIslandPage(GameObject islandPage, ProgressTableManager manager)
    {
        if (manager == null)
        {
            Debug.LogError("ProgressTableManager引用为空，无法更新岛屿页面");
            return;
        }

        // 获取岛屿页面控制器
        IslandPageController pageController = islandPage.GetComponent<IslandPageController>();
        if (pageController != null)
        {
            pageController.UpdatePageContent(manager);
        }
        else
        {
            Debug.LogWarning($"岛屿页面 {islandPage.name} 上未找到IslandPageController组件");
        }
    }

    private void OnExitButtonClicked()
    {
        // 关闭整个ExploreCanvas
        HideCanvas();
    }

    public override void ShowCanvas()
    {
        base.ShowCanvas();
        // 显示时重置到startPage
        ShowStartPage();
    }

    public override void HideCanvas()
    {
        base.HideCanvas();
        // 可选：清理资源或重置状态
    }
}