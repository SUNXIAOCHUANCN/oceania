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
        
        // 订阅解锁事件
        SubscribeToUnlockEvents();
        
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

    private void SubscribeToUnlockEvents()
    {
        if (mainIslandManager != null)
            mainIslandManager.OnAnyItemUnlocked += OnAnyItemUnlocked;

        if (crossIslandManager != null)
            crossIslandManager.OnAnyItemUnlocked += OnAnyItemUnlocked;
    }

    private void OnAnyItemUnlocked()
    {
        // 当任何项目被解锁时，刷新当前岛屿页面
        RefreshCurrentIslandPage();
    }

    private void ShowStartPage()
    {
        // 隐藏当前页面
        if (currentActivePage != null)
            currentActivePage.SetActive(false);

        // 显示startPage
        startPage.SetActive(true);
        currentActivePage = startPage;
        
        Debug.Log("已切换到开始页面");
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
        
        Debug.Log($"已切换到岛屿页面: {islandPage.name}");
    }

    public void UpdateIslandPage(GameObject islandPage, ProgressTableManager manager)
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
            Debug.Log($"岛屿页面 {islandPage.name} 更新完成");
        }
        else
        {
            Debug.LogWarning($"岛屿页面 {islandPage.name} 上未找到IslandPageController组件");
        }
    }

    // 公共方法，允许外部系统更新岛屿页面
    public void RefreshCurrentIslandPage()
    {
        if (currentActivePage != null && currentActivePage != startPage)
        {
            ProgressTableManager manager = null;
            
            if (currentActivePage == mainIslandPage)
                manager = mainIslandManager;
            else if (currentActivePage == crossIslandPage)
                manager = crossIslandManager;
                
            if (manager != null)
            {
                UpdateIslandPage(currentActivePage, manager);
            }
        }
    }

    private void OnExitButtonClicked()
    {
        // 关闭整个ExploreCanvas
        HideCanvas();
    }

    public override void ShowCanvas()
    {
        // 确保在显示画布之前先设置好页面状态
        if (startPage != null)
        {
            ShowStartPage();
        }
        base.ShowCanvas();
    }

    public override void HideCanvas()
    {
        // 隐藏所有页面
        if (startPage != null) startPage.SetActive(false);
        if (mainIslandPage != null) mainIslandPage.SetActive(false);
        if (crossIslandPage != null) crossIslandPage.SetActive(false);
        
        currentActivePage = null;
        base.HideCanvas();
    }
}