using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RanchUIController : CanvasController
{
    [Header("UI References")]
    [SerializeField] private GameObject aniChoose;
    [SerializeField] private Transform content;
    [SerializeField] private TextMeshProUGUI currentProductionText;
    [SerializeField] private TextMeshProUGUI nextMonthProductionText;
    [SerializeField] private TextMeshProUGUI cropConsumptionText;
    [SerializeField] private TextMeshProUGUI managerNameText;
    [SerializeField] private Image managerIcon;
    [SerializeField] private GameObject adminManagerPanel;
    [SerializeField] private Transform managerList;
    [SerializeField] private Button resetManagerButton;
    [SerializeField] private Button closeRanchButton; // 新增关闭按钮
    
    [Header("动物总数显示")]
    [SerializeField] private Image animalsDisplayImage;
    [SerializeField] private TextMeshProUGUI aniNumbersText;
    [SerializeField] private TextMeshProUGUI aniDecayText;
    [SerializeField] private Sprite[] animalCountSprites; // 根据总数量显示的图片数组 [0]less-animals, [1]medium_animals, [2]more_animals
    
    [Header("Prefab Path")]
    private const string ANIMAL_PREFAB_PATH = "UIprefabs/Production/Animal";
    private const string ADMIN_PREFAB_PATH = "UIprefabs/Production/Admin";
    
    private RanchSystem ranchSystem;
    private GameObject animalPrefab;
    
    private void Start()
    {
        base.Start();
        // 获取RanchSystem引用
        ranchSystem = FindObjectOfType<RanchSystem>();
        if (ranchSystem == null)
        {
            Debug.LogError("找不到RanchSystem实例！");
        }
        
        // 加载Animal.prefab
        animalPrefab = Resources.Load<GameObject>(ANIMAL_PREFAB_PATH);
        if (animalPrefab == null)
        {
            Debug.LogError($"无法加载Animal预制体: {ANIMAL_PREFAB_PATH}");
        }
        
        // 初始化管理员面板
        if (adminManagerPanel != null) adminManagerPanel.SetActive(false);
        if (resetManagerButton != null) resetManagerButton.onClick.AddListener(OnResetManagerButtonClicked);
        
        // 初始化关闭按钮
        if (closeRanchButton != null) closeRanchButton.onClick.AddListener(OnCloseRanchButtonClicked);
        
        // 初始化UI
        UpdateRanchInfo(0f, 0f, 0f, null);
    }
    
    /// <summary>
    /// 更新牧场信息（由RanchSystem调用）
    /// </summary>
    public void UpdateRanchInfo(float currentProduction, float nextMonthExpectedYield, 
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
        
        // 更新动物总数显示
        UpdateAnimalCountDisplay();
    }
    
    /// <summary>
    /// 更新动物总数显示
    /// </summary>
    private void UpdateAnimalCountDisplay()
    {
        if (ranchSystem == null) return;
        
        // 更新动物总数文本：总数量/上限
        if (aniNumbersText != null)
        {
            aniNumbersText.text = $"{ranchSystem.CurrentTotalAmount}/{ranchSystem.MaxTotalAmount}";
        }
        

        
        // 更新退化总量显示：所有动物减少量相加（decayPerPhase*amount）
        if (aniDecayText != null)
        {
            aniDecayText.text = $"{ranchSystem.CurrentMonthDecay:F1}";
        }
        
        // 根据总数量更新显示的图片（分界值：5,10）
        if (animalsDisplayImage != null && animalCountSprites != null && animalCountSprites.Length >= 3)
        {
            int totalAmount = ranchSystem.CurrentTotalAmount;
            int spriteIndex = 0; // 默认显示less-animals
            
            if (totalAmount >= 10)
            {
                spriteIndex = 2; // more_animals
            }
            else if (totalAmount >= 5)
            {
                spriteIndex = 1; // medium_animals
            }
            // 否则使用默认的less-animals (spriteIndex = 0)
            
            animalsDisplayImage.sprite = animalCountSprites[spriteIndex];
        }
        else if (animalsDisplayImage != null && animalCountSprites != null && animalCountSprites.Length > 0)
        {
            // 如果数组长度不够3，使用回退逻辑
            int totalAmount = ranchSystem.CurrentTotalAmount;
            int spriteIndex = Mathf.Clamp(totalAmount, 0, animalCountSprites.Length - 1);
            animalsDisplayImage.sprite = animalCountSprites[spriteIndex];
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
        if (ranchSystem == null || content == null || animalPrefab == null)
            return;
        
        // 清除现有内容
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        
        // 获取所有已解锁的动物物种（根据你的描述，UI应该显示所有解锁的物种）
        List<SpeciesScriptableObject> unlockedAnis = SpeciesLoader.Instance?.GetUnlockedAniSpecies();
        if (unlockedAnis == null || unlockedAnis.Count == 0)
            return;
        
        // 为每个解锁的物种创建UI项
        foreach (SpeciesScriptableObject species in unlockedAnis)
        {
            // 实例化Animal.prefab
            GameObject animalItem = Instantiate(animalPrefab, content);
            if (animalItem == null)
                continue;
            
            // 从牧场数据库中获取该物种的数据
            List<RanchSpeciesData> ranchDatabase = ranchSystem.GetRanchDatabase();
            RanchSpeciesData speciesData = ranchDatabase.Find(data => data.speciesName == species.speciesName);
            
            // 如果数据库中没有该物种的数据，创建一个默认数据（amount=0）
            if (speciesData == null)
            {
                speciesData = new RanchSpeciesData(species.speciesName, 0f, species.initialYield);
            }
            
            // 设置UI元素
            SetupAnimalItemUI(animalItem, species, speciesData);
        }
    }
    
    /// <summary>
    /// 设置Animal.prefab的UI元素
    /// </summary>
    private void SetupAnimalItemUI(GameObject animalItem, SpeciesScriptableObject species, RanchSpeciesData speciesData)
    {
        // 查找UI元素
        Image aniImage = animalItem.transform.Find("aniImage")?.GetComponent<Image>();
        TextMeshProUGUI aniName = animalItem.transform.Find("aniName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI aniFrom = animalItem.transform.Find("aniFrom")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI aniId = animalItem.transform.Find("aniId")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI productNumber = animalItem.transform.Find("productNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI resumptNumber = animalItem.transform.Find("resumptNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI decayNumber = animalItem.transform.Find("decayNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI amountText = animalItem.transform.Find("numbersoftheAni")?.GetComponent<TextMeshProUGUI>();
        
        Button addAniButton = animalItem.transform.Find("addAni")?.GetComponent<Button>();
        Button cutAniButton = animalItem.transform.Find("cutAni")?.GetComponent<Button>();
        
        // 设置数据
        if (aniImage != null && species.icon != null)
            aniImage.sprite = species.icon;
        
        if (aniName != null)
            aniName.text = species.speciesName;
        
        if (aniFrom != null)
            aniFrom.text = species.source.ToString();
        
        if (aniId != null)
            aniId.text = species.speciesDescription;
        
        if (productNumber != null)
            productNumber.text = speciesData.nextPhaseYield.ToString("F1");
        
        if (resumptNumber != null)
            resumptNumber.text = species.monthlyCropConsumption.ToString("F1");
        
        if (decayNumber != null)
            decayNumber.text = species.decayPerPhase.ToString("F1");
        
        if (amountText != null)
            amountText.text = speciesData.amount.ToString("F0");
        
        // 设置按钮事件
        if (addAniButton != null)
        {
            addAniButton.onClick.RemoveAllListeners();
            addAniButton.onClick.AddListener(() => OnAddAniButtonClick(species.speciesName));
        }
        
        if (cutAniButton != null)
        {
            cutAniButton.onClick.RemoveAllListeners();
            cutAniButton.onClick.AddListener(() => OnCutAniButtonClick(species.speciesName));
        }
    }
    
    /// <summary>
    /// 通过名称获取物种ScriptableObject
    /// </summary>
    private SpeciesScriptableObject GetSpeciesByName(string speciesName)
    {
        if (SpeciesLoader.Instance == null)
            return null;
        
        // 从所有已解锁的动物物种中查找
        List<SpeciesScriptableObject> unlockedAnis = SpeciesLoader.Instance.GetUnlockedAniSpecies();
        return unlockedAnis.Find(s => s.speciesName == speciesName);
    }
    
    /// <summary>
    /// 处理增加物种数量按钮点击
    /// </summary>
    private void OnAddAniButtonClick(string speciesName)
    {
        if (ranchSystem != null)
        {
            bool success = ranchSystem.IncrementSpeciesAmount(speciesName);
            if (!success)
            {
                Debug.LogWarning($"增加物种数量失败: {speciesName}");
            }
        }
    }
    
    /// <summary>
    /// 处理减少物种数量按钮点击
    /// </summary>
    private void OnCutAniButtonClick(string speciesName)
    {
        if (ranchSystem != null)
        {
            bool success = ranchSystem.DecrementSpeciesAmount(speciesName);
            if (!success)
            {
                Debug.LogWarning($"减少物种数量失败: {speciesName}");
            }
        }
    }
    
    /// <summary>
    /// 显示牧场UI
    /// </summary>
    public void ShowRanchUI()
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
        
        // 检查RanchSystem实例
        if (ranchSystem == null)
        {
            Debug.LogError("RanchSystem实例未找到");
            return;
        }
        
        // 获取所有已招募的人员，并筛选不在onsea状态的人员
        List<PersonScriptableObject> allPersons = ranchSystem.GetAllRecruitedPersons();
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
        if (ranchSystem == null)
        {
            Debug.LogError("RanchSystem未找到");
            return;
        }
        
        // 设置新的管理者
        bool success = ranchSystem.SetManager(admin);
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
            Debug.LogError($"设置管理者失败: {admin.personName}");
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
    /// 隐藏牧场UI
    /// </summary>
    public void HideRanchUI()
    {
        HideCanvas();
    }

    /// <summary>
    /// 关闭牧场UI按钮点击事件
    /// </summary>
    private void OnCloseRanchButtonClicked()
    {
        HideRanchUI();
        if (CursorManager.Instance != null)
    {
        CursorManager.Instance.RegisterInteractionPanel(false);
    }
    }
}