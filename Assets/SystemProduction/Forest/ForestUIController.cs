using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForestUIController : CanvasController
{
    [Header("UI References")]
    [SerializeField] private GameObject matChoose;
    [SerializeField] private Transform content;
    [SerializeField] private TextMeshProUGUI currentProductionText;
    [SerializeField] private TextMeshProUGUI nextMonthProductionText;
    [SerializeField] private TextMeshProUGUI cropConsumptionText;
    [SerializeField] private TextMeshProUGUI managerNameText;
    [SerializeField] private Image managerIcon;
    [SerializeField] private GameObject adminManagerPanel;
    [SerializeField] private Transform managerList;
    [SerializeField] private Button resetManagerButton;
    [SerializeField] private Button closeForestButton; // 新增关闭按钮
    
    [Header("材料总数显示")]
    [SerializeField] private Image materialsDisplayImage;
    [SerializeField] private TextMeshProUGUI matNumbersText;
    [SerializeField] private TextMeshProUGUI matDecayText;
    [SerializeField] private Sprite[] materialCountSprites; // 根据总数量显示的图片数组 [0]less-materials, [1]medium_materials, [2]more_materials
    
    [Header("Prefab Path")]
    private const string MATERIAL_PREFAB_PATH = "UIprefabs/Production/Material";
    private const string ADMIN_PREFAB_PATH = "UIprefabs/Production/Admin";
    
    private ForestSystem forestSystem;
    private GameObject materialPrefab;
    
    private void Start()
    {
        base.Start();
        DebugTool.LogForest("Start() 开始执行");

        // 获取ForestSystem引用
        forestSystem = FindObjectOfType<ForestSystem>();
        if (forestSystem == null)
        {
            DebugTool.LogError("ForestUIController", "找不到ForestSystem实例！");
        }
        else
        {
            DebugTool.LogForest("ForestSystem 已找到");
        }

        // 加载Material.prefab
        materialPrefab = Resources.Load<GameObject>(MATERIAL_PREFAB_PATH);
        if (materialPrefab == null)
        {
            DebugTool.LogError("ForestUIController", "无法加载Material预制体: {0}", MATERIAL_PREFAB_PATH);
        }

        // 初始化管理员面板
        if (adminManagerPanel != null) adminManagerPanel.SetActive(false);
        if (resetManagerButton != null) resetManagerButton.onClick.AddListener(OnResetManagerButtonClicked);

        // 初始化关闭按钮
        if (closeForestButton != null) closeForestButton.onClick.AddListener(OnCloseForestButtonClicked);

        // 初始化UI
        UpdateForestInfo(0f, 0f, 0f, null);

        DebugTool.LogForest("Start() 完成");
    }
    
    /// <summary>
    /// 更新森林信息（由ForestSystem调用�?
    /// </summary>
    public void UpdateForestInfo(float currentProduction, float nextMonthExpectedYield, 
                               float cropConsumption, PersonScriptableObject manager)
    {
        // 更新产量信息
        if (currentProductionText != null)
            currentProductionText.text = $"{currentProduction:F1}";
        
        if (nextMonthProductionText != null)
            nextMonthProductionText.text = $"{nextMonthExpectedYield:F1}";
        
        if (cropConsumptionText != null)
            cropConsumptionText.text = $"{cropConsumption:F1}";
        
        // 更新管理者信�?
        UpdateManagerInfo(manager);
        
        // 更新物种列表
        UpdateSpeciesList();
        
        // 更新材料总数显示
        UpdateMaterialCountDisplay();
    }
    
    /// <summary>
    /// 更新材料总数显示
    /// </summary>
    private void UpdateMaterialCountDisplay()
    {
        if (forestSystem == null) return;
        
        // 更新材料总数文本：总数�?上限
        if (matNumbersText != null)
        {
            matNumbersText.text = $"{forestSystem.CurrentTotalAmount}/{forestSystem.MaxTotalAmount}";
        }
        
        // 更新退化总量显示：所有材料减少量相加（decayPerPhase*amount�?
        if (matDecayText != null)
        {
            matDecayText.text = $"{forestSystem.CurrentMonthDecay:F1}";
        }
        
        // 根据总数量更新显示的图片（分界值：5,10�?
        if (materialsDisplayImage != null && materialCountSprites != null && materialCountSprites.Length >= 3)
        {
            int totalAmount = forestSystem.CurrentTotalAmount;
            int spriteIndex = 0; // 默认显示less-materials
            
            if (totalAmount >= 10)
            {
                spriteIndex = 2; // more_materials
            }
            else if (totalAmount >= 5)
            {
                spriteIndex = 1; // medium_materials
            }
            // 否则使用默认的less-materials (spriteIndex = 0)
            
            materialsDisplayImage.sprite = materialCountSprites[spriteIndex];
        }
        else if (materialsDisplayImage != null && materialCountSprites != null && materialCountSprites.Length > 0)
        {
            // 如果数组长度不够3，使用回退逻辑
            int totalAmount = forestSystem.CurrentTotalAmount;
            int spriteIndex = Mathf.Clamp(totalAmount, 0, materialCountSprites.Length - 1);
            materialsDisplayImage.sprite = materialCountSprites[spriteIndex];
        }
    }
    
    /// <summary>
    /// 更新管理者信�?
    /// </summary>
    private void UpdateManagerInfo(PersonScriptableObject manager)
    {
        if (manager != null)
        {
            if (managerNameText != null)
                managerNameText.text = manager.personName;
            
            if (managerIcon != null)
            {
                managerIcon.sprite = manager.avatar;
                managerIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            if (managerNameText != null)
                managerNameText.text = "无管理员";
            
            if (managerIcon != null)
                managerIcon.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 更新物种列表
    /// </summary>
    private void UpdateSpeciesList()
    {
        if (forestSystem == null || content == null || materialPrefab == null)
            return;
        
        // 清除现有内容
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        
        // 获取所有已解锁的材料物种（根据你的描述，UI应该显示所有解锁的物种�?
        List<SpeciesScriptableObject> unlockedMats = SpeciesLoader.Instance?.GetUnlockedMatSpecies();
        if (unlockedMats == null || unlockedMats.Count == 0)
            return;
        
        // 为每个解锁的物种创建UI�?
        foreach (SpeciesScriptableObject species in unlockedMats)
        {
            // 实例化Material.prefab
            GameObject materialItem = Instantiate(materialPrefab, content);
            if (materialItem == null)
                continue;
            
            // 从森林数据库中获取该物种的数�?
            List<ForestSpeciesData> forestDatabase = forestSystem.GetForestDatabase();
            ForestSpeciesData speciesData = forestDatabase.Find(data => data.speciesName == species.speciesName);
            
            // 如果数据库中没有该物种的数据，创建一个默认数据（amount=0�?
            if (speciesData == null)
            {
                speciesData = new ForestSpeciesData(species.speciesName, 0f, species.initialYield);
            }
            
            // 设置UI元素
            SetupMaterialItemUI(materialItem, species, speciesData);
        }
    }
    
    /// <summary>
    /// 设置Material.prefab的UI元素
    /// </summary>
    private void SetupMaterialItemUI(GameObject materialItem, SpeciesScriptableObject species, ForestSpeciesData speciesData)
    {
        // 查找UI元素
        Image matImage = materialItem.transform.Find("matImage")?.GetComponent<Image>();
        TextMeshProUGUI matName = materialItem.transform.Find("matName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI matFrom = materialItem.transform.Find("matFrom")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI matId = materialItem.transform.Find("matId")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI productNumber = materialItem.transform.Find("productNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI resumptNumber = materialItem.transform.Find("resumptNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI decayNumber = materialItem.transform.Find("decayNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI amountText = materialItem.transform.Find("numbersoftheMat")?.GetComponent<TextMeshProUGUI>();
        
        Button addMatButton = materialItem.transform.Find("addMat")?.GetComponent<Button>();
        Button cutMatButton = materialItem.transform.Find("cutMat")?.GetComponent<Button>();
        
        // 设置数据
        if (matImage != null && species.icon != null)
            matImage.sprite = species.icon;
        
        if (matName != null)
            matName.text = species.speciesName;
        
        if (matFrom != null)
            matFrom.text = species.source.ToString();
        
        if (matId != null)
            matId.text = species.speciesDescription;
        
        if (productNumber != null)
            productNumber.text = speciesData.nextPhaseYield.ToString("F1");
        
        if (resumptNumber != null)
            resumptNumber.text = species.monthlyCropConsumption.ToString("F1");
        
        if (decayNumber != null)
            decayNumber.text = species.decayPerPhase.ToString("F1");
        
        if (amountText != null)
            amountText.text = speciesData.amount.ToString("F0");
        
        // 设置按钮事件
        if (addMatButton != null)
        {
            addMatButton.onClick.RemoveAllListeners();
            addMatButton.onClick.AddListener(() => OnAddMatButtonClick(species.speciesName));
        }
        
        if (cutMatButton != null)
        {
            cutMatButton.onClick.RemoveAllListeners();
            cutMatButton.onClick.AddListener(() => OnCutMatButtonClick(species.speciesName));
        }
    }
    
    /// <summary>
    /// 通过名称获取物种ScriptableObject
    /// </summary>
    private SpeciesScriptableObject GetSpeciesByName(string speciesName)
    {
        if (SpeciesLoader.Instance == null)
            return null;
        
        // 从所有已解锁的材料物种中查找
        List<SpeciesScriptableObject> unlockedMats = SpeciesLoader.Instance.GetUnlockedMatSpecies();
        return unlockedMats.Find(s => s.speciesName == speciesName);
    }
    
    /// <summary>
    /// 处理增加物种数量按钮点击
    /// </summary>
    private void OnAddMatButtonClick(string speciesName)
    {
        DebugTool.LogForest("OnAddMatButtonClick() 增加材料: {0}", speciesName);
        if (forestSystem != null)
        {
            bool success = forestSystem.IncrementSpeciesAmount(speciesName);
            if (!success)
            {
                DebugTool.LogWarning("ForestUIController", "增加物种数量失败: {0}", speciesName);
            }
            else
            {
                DebugTool.LogForest("成功增加物种数量: {0}", speciesName);
            }
        }
    }

    /// <summary>
    /// 处理减少物种数量按钮点击
    /// </summary>
    private void OnCutMatButtonClick(string speciesName)
    {
        DebugTool.LogForest("OnCutMatButtonClick() 减少材料: {0}", speciesName);
        if (forestSystem != null)
        {
            bool success = forestSystem.DecrementSpeciesAmount(speciesName);
            if (!success)
            {
                DebugTool.LogWarning("ForestUIController", "减少物种数量失败: {0}", speciesName);
            }
            else
            {
                DebugTool.LogForest("成功减少物种数量: {0}", speciesName);
            }
        }
    }
    
    /// <summary>
    /// 显示森林UI
    /// </summary>
    public void ShowForestUI()
    {
        ShowCanvas();
        UpdateSpeciesList();
    }
    
    /// <summary>
    /// 重置管理者按钮点击事�?
    /// </summary>
    private void OnResetManagerButtonClicked()
    {
        if (adminManagerPanel != null)
        {
            // 显示管理者选择面板
            adminManagerPanel.SetActive(true);
            // 初始化管理者名�?
            InitializeAdminManagerPanel();
        }
        else
        {
            DebugTool.LogError("ForestUIController", "adminManagerPanel is not assigned!");
        }
    }
    
    /// <summary>
    /// 初始化管理者选择面板
    /// </summary>
    private void InitializeAdminManagerPanel()
    {
        // 清除现有管理者列�?
        foreach (Transform child in managerList)
        {
            Destroy(child.gameObject);
        }
        
        // 加载Admin预制�?
        GameObject adminPrefab = Resources.Load<GameObject>(ADMIN_PREFAB_PATH);
        if (adminPrefab == null)
        {
            DebugTool.LogError("ForestUIController", $"无法加载管理者预制体: {ADMIN_PREFAB_PATH}");
            return;
        }
        
        // 检查ForestSystem实例
        if (forestSystem == null)
        {
            DebugTool.LogError("ForestUIController", "ForestSystem实例未找到");
            return;
        }
        
        // 获取所有已招募的人员，并筛选不在onsea状态的人员
        List<PersonScriptableObject> allPersons = forestSystem.GetAllRecruitedPersons();
        List<PersonScriptableObject> eligiblePersons = new List<PersonScriptableObject>();

        DebugTool.LogForest("=== 开始调试管理者信息 ===");
        DebugTool.LogForest("总人员数: {0}", allPersons.Count);

        // 输出所有人员的详细信息
        for (int i = 0; i < allPersons.Count; i++)
        {
            var person = allPersons[i];
            DebugTool.LogForest("索引 {0}: 人员名称: {1}, 已招募: {2}, 状态: {3}, 职业: {4}",
                i, person.personName, person.recruited, person.status, person.profession);
        }

        DebugTool.LogForest("=== 筛选符合条件的管理者 ===");
        int addedCount = 0;
        foreach (PersonScriptableObject person in allPersons)
        {
            // 条件：已招募且不在onsea状态
            if (person.recruited && person.status != PersonStatus.onsea)
            {
                DebugTool.LogForest("检查人员: {0}, 状态: {1}, 已招募: {2}",
                    person.personName, person.status, person.recruited);
                // 检查是否已经添加过该人员（避免重复）
                if (!eligiblePersons.Contains(person))
                {
                    eligiblePersons.Add(person);
                    addedCount++;
                    DebugTool.LogForest("添加管理者: {0}, 状态: {1}", person.personName, person.status);
                }
                else
                {
                    DebugTool.LogWarning("ForestUIController", "检测到重复人员: {0}, 已跳过添加", person.personName);
                }
            }
        }

        DebugTool.LogForest("=== 管理者筛选完成 ===");
        DebugTool.LogForest("符合条件的管理者总数: {0}", eligiblePersons.Count);
        DebugTool.LogForest("本次筛选新增人员数: {0}", addedCount);

        if (eligiblePersons.Count == 0)
        {
            DebugTool.LogForest("没有符合条件的管理者");
            return;
        }

        // 创建管理者选择项
        DebugTool.LogForest("开始创建 {0} 个管理者UI元素", eligiblePersons.Count);
        foreach (PersonScriptableObject person in eligiblePersons)
        {
            DebugTool.LogForest("正在创建管理者UI: {0}", person.personName);
            GameObject adminItem = Instantiate(adminPrefab, managerList);
            adminItem.name = person.personName;

            // 填充管理者信息（按照用户指定的映射关系）
            // touxiang → Avatar
            Transform avatarTransform = adminItem.transform.Find("touxiang");
            if (avatarTransform != null && avatarTransform.TryGetComponent<Image>(out Image avatar))
            {
                avatar.sprite = person.avatar;
            }

            // name → Person Name
            Transform nameTransform = adminItem.transform.Find("name");
            if (nameTransform != null && nameTransform.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI nameText))
            {
                nameText.text = person.personName;
            }

            // job → Profession
            Transform jobTransform = adminItem.transform.Find("job");
            if (jobTransform != null && jobTransform.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI jobText))
            {
                jobText.text = person.profession.ToString();
            }

            // effect → Profession Description
            Transform effectTransform = adminItem.transform.Find("effect");
            if (effectTransform != null && effectTransform.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI effectText))
            {
                effectText.text = person.professionDescription;
            }

            // 获取按钮并添加点击事件
            Transform adminButton = adminItem.transform.Find("adminButton");
            if (adminButton != null && adminButton.TryGetComponent<Button>(out Button button))
            {
                button.onClick.AddListener(() => OnAdminSelected(person));
            }
            else
            {
                // 如果找不到特定按钮，使用根对象的按钮
                if (adminItem.TryGetComponent<Button>(out Button rootButton))
                {
                    rootButton.onClick.AddListener(() => OnAdminSelected(person));
                }
            }
            DebugTool.LogForest("成功创建管理者UI: {0}", person.personName);
        }
        DebugTool.LogForest("完成创建管理者UI，共创建 {0} 个元素", eligiblePersons.Count);

        DebugTool.LogForest("已加载 {0} 个管理者到UI", eligiblePersons.Count);
    }
    
    /// <summary>
    /// 当管理者被选择
    /// </summary>
    private void OnAdminSelected(PersonScriptableObject admin)
    {
        DebugTool.LogForest("OnAdminSelected() 开始执行，管理者: {0}", admin?.personName ?? "null");

        if (forestSystem == null)
        {
            DebugTool.LogError("ForestUIController", "ForestSystem未找到");
            return;
        }

        // 设置新的管理者
        bool success = forestSystem.SetManager(admin);
        if (success)
        {
            DebugTool.LogForest("成功设置管理者: {0}", admin.personName);

            // 隐藏管理者选择面板
            if (adminManagerPanel != null)
            {
                adminManagerPanel.SetActive(false);
            }

            // 更新UI显示
            UpdateManagerInfo(admin);

            DebugTool.LogForest("OnAdminSelected() 完成");
        }
        else
        {
            DebugTool.LogForest("设置管理者失败: {0}", admin.personName);
        }
    }
    
    /// <summary>
    /// 关闭管理者选择面板
    /// </summary>
    public void CloseAdminManagerPanel()
    {
        if (adminManagerPanel != null)
        {
            adminManagerPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 隐藏森林UI
    /// </summary>
    public void HideForestUI()
    {
        HideCanvas();
    }

    /// <summary>
    /// 关闭森林UI按钮点击事件
    /// </summary>
    private void OnCloseForestButtonClicked()
    {
        HideForestUI();
        if (CursorManager.Instance != null)
    {
        CursorManager.Instance.RegisterInteractionPanel(false);
    }
    }
}
