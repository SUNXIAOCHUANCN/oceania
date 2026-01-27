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
        // 获取ForestSystem引用
        forestSystem = FindObjectOfType<ForestSystem>();
        if (forestSystem == null)
        {
            Debug.LogError("找不到ForestSystem实例！");
        }
        
        // 加载Material.prefab
        materialPrefab = Resources.Load<GameObject>(MATERIAL_PREFAB_PATH);
        if (materialPrefab == null)
        {
            Debug.LogError($"无法加载Material预制体: {MATERIAL_PREFAB_PATH}");
        }
        
        // 初始化管理员面板
        if (adminManagerPanel != null) adminManagerPanel.SetActive(false);
        if (resetManagerButton != null) resetManagerButton.onClick.AddListener(OnResetManagerButtonClicked);
        
        // 初始化关闭按钮
        if (closeForestButton != null) closeForestButton.onClick.AddListener(OnCloseForestButtonClicked);
        
        // 初始化UI
        UpdateForestInfo(0f, 0f, 0f, null);
    }
    
    /// <summary>
    /// 更新森林信息（由ForestSystem调用）
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
        
        // 更新管理者信息
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
        
        // 更新材料总数文本：总数量/上限
        if (matNumbersText != null)
        {
            matNumbersText.text = $"{forestSystem.CurrentTotalAmount}/{forestSystem.MaxTotalAmount}";
        }
        
        // 更新退化总量显示：所有材料减少量相加（decayPerPhase*amount）
        if (matDecayText != null)
        {
            matDecayText.text = $"{forestSystem.CurrentMonthDecay:F1}";
        }
        
        // 根据总数量更新显示的图片（分界值：5,10）
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
    /// 更新管理者信息
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
                managerNameText.text = "无管理者";
            
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
        
        // 获取所有已解锁的材料物种（根据你的描述，UI应该显示所有解锁的物种）
        List<SpeciesScriptableObject> unlockedMats = SpeciesLoader.Instance?.GetUnlockedMatSpecies();
        if (unlockedMats == null || unlockedMats.Count == 0)
            return;
        
        // 为每个解锁的物种创建UI项
        foreach (SpeciesScriptableObject species in unlockedMats)
        {
            // 实例化Material.prefab
            GameObject materialItem = Instantiate(materialPrefab, content);
            if (materialItem == null)
                continue;
            
            // 从森林数据库中获取该物种的数据
            List<ForestSpeciesData> forestDatabase = forestSystem.GetForestDatabase();
            ForestSpeciesData speciesData = forestDatabase.Find(data => data.speciesName == species.speciesName);
            
            // 如果数据库中没有该物种的数据，创建一个默认数据（amount=0）
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
        if (forestSystem != null)
        {
            bool success = forestSystem.IncrementSpeciesAmount(speciesName);
            if (!success)
            {
                Debug.LogWarning($"增加物种数量失败: {speciesName}");
            }
        }
    }
    
    /// <summary>
    /// 处理减少物种数量按钮点击
    /// </summary>
    private void OnCutMatButtonClick(string speciesName)
    {
        if (forestSystem != null)
        {
            bool success = forestSystem.DecrementSpeciesAmount(speciesName);
            if (!success)
            {
                Debug.LogWarning($"减少物种数量失败: {speciesName}");
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
    /// 重置管理者按钮点击事件
    /// </summary>
    private void OnResetManagerButtonClicked()
    {
        if (adminManagerPanel != null)
        {
            // 显示管理者选择面板
            adminManagerPanel.SetActive(true);
            // 初始化管理者名单
            InitializeAdminManagerPanel();
        }
        else
        {
            Debug.LogError("adminManagerPanel is not assigned!");
        }
    }
    
    /// <summary>
    /// 初始化管理者选择面板
    /// </summary>
    private void InitializeAdminManagerPanel()
    {
        // 清除现有管理者列表
        foreach (Transform child in managerList)
        {
            Destroy(child.gameObject);
        }
        
        // 加载Admin预制体
        GameObject adminPrefab = Resources.Load<GameObject>(ADMIN_PREFAB_PATH);
        if (adminPrefab == null)
        {
            Debug.LogError($"无法加载管理者预制体: {ADMIN_PREFAB_PATH}");
            return;
        }
        
        // 检查ForestSystem实例
        if (forestSystem == null)
        {
            Debug.LogError("ForestSystem实例未找到");
            return;
        }
        
        // 获取所有已招募的人员，并筛选不在onsea状态的人员
        List<PersonScriptableObject> allPersons = forestSystem.GetAllRecruitedPersons();
        List<PersonScriptableObject> eligiblePersons = new List<PersonScriptableObject>();
        
        Debug.Log($"=== 开始调试管理者信息 ===");
        Debug.Log($"总人员数: {allPersons.Count}");
        
        // 输出所有人员的详细信息
        for (int i = 0; i < allPersons.Count; i++)
        {
            var person = allPersons[i];
            Debug.Log($"索引 {i}: 人员名称: {person.personName}, 已招募: {person.recruited}, 状态: {person.status}, 职业: {person.profession}");
        }
        
        Debug.Log($"=== 筛选符合条件的管理者 ===");
        int addedCount = 0;
        foreach (PersonScriptableObject person in allPersons)
        {
            // 条件：已招募且不在onsea状态
            if (person.recruited && person.status != PersonStatus.onsea)
            {
                Debug.Log($"检查人员: {person.personName}, 状态: {person.status}, 已招募: {person.recruited}");
                // 检查是否已经添加过该人员（避免重复）
                if (!eligiblePersons.Contains(person))
                {
                    eligiblePersons.Add(person);
                    addedCount++;
                    Debug.Log($"添加管理者: {person.personName}, 状态: {person.status}");
                }
                else
                {
                    Debug.LogWarning($"检测到重复人员: {person.personName}, 已跳过添加");
                }
            }
        }
        
        Debug.Log($"=== 管理者筛选完成 ===");
        Debug.Log($"符合条件的管理者总数: {eligiblePersons.Count}");
        Debug.Log($"本次筛选新增人员数: {addedCount}");
        
        if (eligiblePersons.Count == 0)
        {
            Debug.Log("没有符合条件的管理者");
            return;
        }
        
        // 创建管理者选择项
        Debug.Log($"开始创建 {eligiblePersons.Count} 个管理者UI元素");
        foreach (PersonScriptableObject person in eligiblePersons)
        {
            Debug.Log($"正在创建管理者UI: {person.personName}");
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
            Debug.Log($"成功创建管理者UI: {person.personName}");
        }
        Debug.Log($"完成创建管理者UI，共创建 {eligiblePersons.Count} 个元素");
        
        Debug.Log($"已加载 {eligiblePersons.Count} 个管理者到UI");
    }
    
    /// <summary>
    /// 当管理者被选择
    /// </summary>
    private void OnAdminSelected(PersonScriptableObject admin)
    {
        if (forestSystem == null)
        {
            Debug.LogError("ForestSystem未找到");
            return;
        }
        
        // 设置新的管理者
        bool success = forestSystem.SetManager(admin);
        if (success)
        {
            Debug.Log($"成功设置管理者: {admin.personName}");
            
            // 隐藏管理者选择面板
            if (adminManagerPanel != null)
            {
                adminManagerPanel.SetActive(false);
            }
            
            // 更新UI显示
            UpdateManagerInfo(admin);
        }
        else
        {
            Debug.Log($"设置管理者失败: {admin.personName}");
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