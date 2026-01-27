using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FarmUIController : CanvasController
{
    [Header("UI References")]
    [SerializeField] private Transform fieldsContainer;
    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private GameObject cropChoosePanel;
    [SerializeField] private GameObject workerChoosePanel;
    [SerializeField] private Transform cropList;
    [SerializeField] private Transform workerList;
    [SerializeField] private GameObject adminManagerPanel;
    [SerializeField] private Button resetManagerButton;
    [SerializeField] private Button closeFarmButton; 
    [SerializeField] private Transform managerList;
    [SerializeField] private TextMeshProUGUI currentProductionText;
    [SerializeField] private TextMeshProUGUI nextMonthProductionText;
    [SerializeField] private TextMeshProUGUI managerNameText;
    [SerializeField] private Image managerIcon;
    [SerializeField] private TextMeshProUGUI managerProfessionText;
    
    private const string CROP_CHOICE_PREFAB_PATH = "UIprefabs/Production/CropChoice";
    private const string PERSON_IN_CROP_PREFAB_PATH = "UIprefabs/Production/PersonInCrop";
    private const string ADMIN_PREFAB_PATH = "UIprefabs/Production/Admin";

    [Header("Locked Field Sprites")]
    [SerializeField] private Sprite lockedFieldSprite;
    [SerializeField] private Sprite selectedLockedFieldSprite;

    [Header("Field Sprites")]
    [SerializeField] private Sprite fieldSprite;
    [SerializeField] private Sprite selectedfieldSprite;

    
    
    [Header("Field Settings")]
    [SerializeField] private int totalFields = 15;
    
    [Header("Field Toggles (Manual Assignment)")]
    [SerializeField] private List<Toggle> fieldToggles = new List<Toggle>();
    
    private FarmSystem farmSystem;
    private FieldUnit selectedField;

    
    
    /// <summary>
    /// 重写ShowCanvas以添加调试信息
    /// </summary>
    public override void ShowCanvas()
    {
        Debug.Log($"FarmUIController.ShowCanvas() called, current active state: {gameObject.activeSelf}");
        base.ShowCanvas();
        Debug.Log($"FarmUIController.ShowCanvas() completed, new active state: {gameObject.activeSelf}");
    }

    private void Start()
    {
        // 初始化UI
        InitializeUI();
        
        // 初始化AdminManager UI
        if (adminManagerPanel != null) adminManagerPanel.SetActive(false);
        if (resetManagerButton != null) resetManagerButton.onClick.AddListener(OnResetManagerButtonClicked);
        if (closeFarmButton != null) closeFarmButton.onClick.AddListener(OnCloseFarmButtonClicked);
        Debug.Log("ResetManagerButton initialized");
        
        // 获取农场系统
        farmSystem = FindObjectOfType<FarmSystem>();
        if (farmSystem == null)
        {
            Debug.LogError("FarmSystem not found in the scene!");
            return;
        }
        
        // 初始更新UI
        UpdateFarmInfo(0, 0, null);
        
        // 订阅农场事件
        farmSystem.OnProductionCalculated.AddListener(OnProductionCalculated);
        farmSystem.OnManagerChanged.AddListener(UpdateManagerInfo);

        // 初始化田地UI（设置作物图标等）
        UpdateFieldUI();
        
        // 订阅农场事件
        farmSystem.OnProductionCalculated.AddListener(OnProductionCalculated);

        // 调试信息
        Debug.Log("FarmUIController initialized");
        Debug.Log("Crop panel active: " + cropChoosePanel.activeSelf);
        Debug.Log("Worker panel active: " + workerChoosePanel.activeSelf);
        Debug.Log("Field toggles count: " + fieldToggles.Count);
        
        // 在开始时不显示UI
        HideCanvas();
    }
    
    private void OnDestroy()
    {
        if (farmSystem != null)
        {
            farmSystem.OnProductionCalculated.RemoveListener(OnProductionCalculated);
            farmSystem.OnManagerChanged.RemoveListener(UpdateManagerInfo);
        }
    }
    
    /// <summary>
/// 初始化UI元素
/// </summary>
private void InitializeUI()
{
    // 初始化已存在的田地UI
    // 注意：现在fieldToggles是通过Inspector手动赋值的
    InitializeExistingFieldUIs();
    
    // 初始化选择面板（现在不传递参数，等选择田地时再更新）
    InitializeCropChoosePanel(); // 不传递田地，使用默认值
    InitializeWorkerChoosePanel();
    
    // 默认隐藏选择面板
    cropChoosePanel.SetActive(false);
    workerChoosePanel.SetActive(false);
}
    
    /// <summary>
/// 初始化已存在的田地UI
/// </summary>
private void InitializeExistingFieldUIs()
{
    // 注意：现在fieldToggles是通过Inspector手动赋值的
    // 这里不需要做任何操作
    
    // 为已有的fieldToggles设置事件监听器
    for (int i = 0; i < fieldToggles.Count; i++)
    {
        if (fieldToggles[i] != null)
        {
            int fieldIndex = i;
            fieldToggles[i].onValueChanged.RemoveAllListeners(); // 清除旧监听器
            fieldToggles[i].onValueChanged.AddListener((isOn) => 
            {
                if (isOn)
                {
                    OnFieldSelected(fieldIndex);
                }
            });
            Debug.Log($"Field toggle {i} 初始化完成");
        }
    }
}
    
    /// <summary>
    /// 初始化作物选择面板
    /// </summary>
    private void InitializeCropChoosePanel(FieldUnit targetField = null)
    {
        // 清除现有作物
        foreach (Transform child in cropList)
        {
            Destroy(child.gameObject);
        }
        
        // 加载CropChoice预制体
        GameObject cropChoicePrefab = Resources.Load<GameObject>(CROP_CHOICE_PREFAB_PATH);
        if (cropChoicePrefab == null)
        {
            Debug.LogError($"无法加载作物选择预制体: {CROP_CHOICE_PREFAB_PATH}");
            return;
        }
        
        // 获取所有已解锁的作物
        List<SpeciesScriptableObject> unlockedCrops = SpeciesLoader.Instance.GetUnlockedCropSpecies();
        
        // 创建作物选择项
        foreach (SpeciesScriptableObject crop in unlockedCrops)
        {
            GameObject cropChoice = Instantiate(cropChoicePrefab, cropList);
            cropChoice.name = crop.speciesName;
            
            // 填充作物信息
            Transform cropImage = cropChoice.transform.Find("cropImage");
            if (cropImage != null && cropImage.TryGetComponent<Image>(out Image image))
            {
                image.sprite = crop.icon;
            }
            
            Transform cropNameText = cropChoice.transform.Find("cropName");
            if (cropNameText != null && cropNameText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI cropName))
            {
                cropName.text = crop.speciesName;
            }
            
            Transform cropFromText = cropChoice.transform.Find("cropFrom");
            if (cropFromText != null && cropFromText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI cropFrom))
            {
                cropFrom.text = crop.source.ToString();
            }
            
            Transform cropIdText = cropChoice.transform.Find("cropId");
            if (cropIdText != null && cropIdText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI cropId))
            {
                cropId.text = crop.speciesDescription;
            }
            
            Transform productNumberText = cropChoice.transform.Find("productNumber");
            if (productNumberText != null && productNumberText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI productNumber))
            {
                // 如果指定了田地，获取该作物在该田地上的预计产量
                if (targetField != null)
                {
                    // 从田地获取该作物的最后记录NPY
                    var cropLastNPY = targetField.GetCropLastNPY();
                    string cropNameStr = crop.speciesName;
                    
                    if (cropLastNPY.TryGetValue(cropNameStr, out float lastNPY))
                    {
                        // 使用上次记录的NPY
                        productNumber.text = lastNPY.ToString("F1");
                        Debug.Log($"作物 {cropNameStr} 在该田地上的预计产量: {lastNPY}");
                    }
                    else
                    {
                        // 使用初始产量
                        productNumber.text = crop.initialYield.ToString("F1");
                        Debug.Log($"作物 {cropNameStr} 在该田地上首次种植，使用初始产量: {crop.initialYield}");
                    }
                }
                else
                {
                    // 如果没有指定田地，显示默认的nextPhaseYield
                    productNumber.text = crop.nextPhaseYield.ToString("F1");
                }
            }
            
            Transform growMoonsText = cropChoice.transform.Find("growMoons");
            if (growMoonsText != null && growMoonsText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI growMoons))
            {
                growMoons.text = crop.growthPhases.ToString();
            }
            
            Transform decayNumberText = cropChoice.transform.Find("decayNumber");
            if (decayNumberText != null && decayNumberText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI decayNumber))
            {
                decayNumber.text = crop.decayPerPhase.ToString("F2");
            }
            
            // 获取按钮并添加点击事件
            Transform cropButton = cropChoice.transform.Find("cropButton");
            if (cropButton != null && cropButton.TryGetComponent<Button>(out Button button))
            {
                button.onClick.AddListener(() => OnCropSelected(crop));
            }
            else
            {
                // 如果找不到特定按钮，使用根对象的按钮
                if (cropChoice.TryGetComponent<Button>(out Button rootButton))
                {
                    rootButton.onClick.AddListener(() => OnCropSelected(crop));
                }
            }
        }
    }
    
    /// <summary>
    /// 初始化工人选择面板
    /// </summary>
    private void InitializeWorkerChoosePanel()
    {
        // 清除现有工人
        foreach (Transform child in workerList)
        {
            Destroy(child.gameObject);
        }
        
        // 加载PersonInCrop预制体
        GameObject personInCropPrefab = Resources.Load<GameObject>(PERSON_IN_CROP_PREFAB_PATH);
        if (personInCropPrefab == null)
        {
            Debug.LogError($"无法加载人员预制体: {PERSON_IN_CROP_PREFAB_PATH}");
            return;
        }
        
        // 检查PersonManager实例
        if (PersonManager.Instance == null)
        {
            Debug.LogError("PersonManager实例未找到");
            return;
        }
        
        // 获取所有已招募的人员，并筛选状态为Rest或Infarm的
        List<PersonScriptableObject> allPersons = PersonManager.Instance.GetAllPersons();
        List<PersonScriptableObject> eligiblePersons = new List<PersonScriptableObject>();
        
        Debug.Log($"=== 开始调试人员信息 ===");
        Debug.Log($"总人员数: {allPersons.Count}");
        
        // 输出所有人员的详细信息
        for (int i = 0; i < allPersons.Count; i++)
        {
            var person = allPersons[i];
            Debug.Log($"索引 {i}: 人员名称: {person.personName}, 已招募: {person.recruited}, 状态: {person.status}, 职业: {person.profession}");
        }
        
        Debug.Log($"=== 筛选符合条件的可开垦人员 ===");
        int addedCount = 0;
        foreach (PersonScriptableObject person in allPersons)
        {
            if (person.recruited && (person.status == PersonStatus.rest || person.status == PersonStatus.infarm))
            {
                Debug.Log($"检查人员: {person.personName}, 状态: {person.status}, 已招募: {person.recruited}");
                // 检查是否已经添加过该人员（避免重复）
                if (!eligiblePersons.Contains(person))
                {
                    eligiblePersons.Add(person);
                    addedCount++;
                    Debug.Log($"添加可开垦人员: {person.personName}, 状态: {person.status}");
                }
                else
                {
                    Debug.LogWarning($"检测到重复人员: {person.personName}, 已跳过添加");
                }
            }
        }
        
        Debug.Log($"=== 可开垦人员筛选完成 ===");
        Debug.Log($"符合条件的可开垦人员总数: {eligiblePersons.Count}");
        Debug.Log($"本次筛选新增人员数: {addedCount}");
        
        if (eligiblePersons.Count == 0)
        {
            Debug.Log("没有符合条件的可开垦人员");
            return;
        }
        
        // 创建人员选择项
        Debug.Log($"开始创建 {eligiblePersons.Count} 个人员UI元素");
        foreach (PersonScriptableObject person in eligiblePersons)
        {
            Debug.Log($"正在创建人员UI: {person.personName}");
            GameObject personInCrop = Instantiate(personInCropPrefab, workerList);
            personInCrop.name = person.personName;
            
            // 填充人员信息
            Transform nameText = personInCrop.transform.Find("name");
            if (nameText != null && nameText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI name))
            {
                name.text = person.personName;
            }
            
            Transform jobText = personInCrop.transform.Find("job");
            if (jobText != null && jobText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI job))
            {
                job.text = person.profession.ToString();
            }
            
            Transform avatarImage = personInCrop.transform.Find("Avatar");
            if (avatarImage != null && avatarImage.TryGetComponent<Image>(out Image avatar))
            {
                avatar.sprite = person.avatar;
            }
            
            // 获取按钮并添加点击事件
            Transform personButton = personInCrop.transform.Find("personButton");
            if (personButton != null && personButton.TryGetComponent<Button>(out Button button))
            {
                button.onClick.AddListener(() => OnWorkerSelected(person));
            }
            else
            {
                // 如果找不到特定按钮，使用根对象的按钮
                if (personInCrop.TryGetComponent<Button>(out Button rootButton))
                {
                    rootButton.onClick.AddListener(() => OnWorkerSelected(person));
                }
            }
            Debug.Log($"成功创建人员UI: {person.personName}");
        }
        Debug.Log($"完成创建人员UI，共创建 {eligiblePersons.Count} 个元素");
        
        Debug.Log($"已加载 {eligiblePersons.Count} 个可开垦人员到UI");
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
        
        // 检查PersonManager实例
        if (PersonManager.Instance == null)
        {
            Debug.LogError("PersonManager实例未找到");
            return;
        }
        
        // 获取所有已招募的人员，并筛选不在onsea状态的人员
        List<PersonScriptableObject> allPersons = PersonManager.Instance.GetAllPersons();
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
        if (farmSystem == null)
        {
            Debug.LogError("FarmSystem未找到");
            return;
        }
        
        // 设置新的管理者
        bool success = farmSystem.SetManager(admin);
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
    /// 当田地被选择
    /// </summary>
    private void OnFieldSelected(int fieldIndex)
    {
        // 安全检查
        if (farmSystem == null || farmSystem.GetFields() == null || fieldIndex < 0 || fieldIndex >= farmSystem.GetFields().Count)
        {
            Debug.LogWarning("Invalid field selection: " + fieldIndex);
            return;
        }
        
        // 获取选中的田地
        selectedField = farmSystem.GetFields()[fieldIndex];
        
        // 取消其他田地的选中状态
        for (int i = 0; i < fieldToggles.Count; i++)
        {
            if (i != fieldIndex && fieldToggles[i] != null)
            {
                fieldToggles[i].isOn = false;
            }
        }

        // 实时反馈当前选择的field信息
        Debug.Log($"=== 当前选择的田地 ===");
        Debug.Log($"田地索引: {fieldIndex}");
        Debug.Log($"田地名称: {selectedField.name}");
        Debug.Log($"是否已解锁: {selectedField.IsUnlocked}");
        Debug.Log($"当前作物: {(selectedField.CurrentCrop != null ? selectedField.CurrentCrop.speciesName : "无作物")}");
        Debug.Log($"当前产量: {selectedField.CurrentNPY:F2}");
        Debug.Log($"=====================");

        
        // 根据田地状态显示相应面板
        if (selectedField.IsUnlocked)
        {
            // 重新初始化作物选择面板，传入当前选中田地
            InitializeCropChoosePanel(selectedField);
            cropChoosePanel.SetActive(true);
            workerChoosePanel.SetActive(false);
            Debug.Log("显示作物选择面板，已更新为田地特定产量");
        }
        else
        {
            InitializeWorkerChoosePanel();
            cropChoosePanel.SetActive(false);
            workerChoosePanel.SetActive(true);
            Debug.Log("显示工人选择面板");
        }
    }
    
    /// <summary>
    /// 当作物被选择
    /// </summary>
    private void OnCropSelected(SpeciesScriptableObject crop)
    {
        if (selectedField != null && selectedField.IsUnlocked)
        {
            selectedField.PlantCrop(crop);
            UpdateFieldUI();
            
            // 隐藏作物选择面板
            cropChoosePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 当工人被选择
    /// </summary>
    private void OnWorkerSelected(PersonScriptableObject worker)
    {
        if (selectedField != null && !selectedField.IsUnlocked)
        {
            // 检查工人是否已经被其他田地占用
            bool isWorkerOccupied = false;
            foreach (FieldUnit field in farmSystem.GetFields())
            {
                // 跳过当前田地
                if (field == selectedField) continue;
                
                // 检查工人是否被其他田地占用（包括开垦中和已解锁但有工人的情况）
                if (field.CurrentWorker == worker)
                {
                    isWorkerOccupied = true;
                    Debug.LogWarning($"工人 {worker.personName} 已经被其他田地占用，无法重复分配");
                    break;
                }
            }
            
            if (isWorkerOccupied)
            {
                // 可以选择显示UI提示，目前先返回
                return;
            }
            
            // 设置开垦工人并更新工人状态
            selectedField.SetWorker(worker);
            if (PersonManager.Instance != null)
            {
                PersonManager.Instance.ChangePersonStatus(worker, PersonStatus.infarm);
                Debug.Log($"工人 {worker.personName} 状态已更新为在农场");
            }
            
            // 开垦田地
            if (selectedField.StartCultivation())
            {
                // 更新UI
                UpdateFieldUI();
                
                // 隐藏工人选择面板
                workerChoosePanel.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 更新田地UI
    /// </summary>
    private void UpdateFieldUI()
{
    for (int i = 0; i < totalFields; i++)
    {
        // 安全检查
        if (i >= farmSystem.GetFields().Count || i >= fieldToggles.Count || fieldToggles[i] == null)
        {
            continue;
        }
        
        FieldUnit field = farmSystem.GetFields()[i];
        Toggle fieldToggle = fieldToggles[i];
        
        // 更新田地锁定状态图标
        Transform background = fieldToggle.transform.Find("Background");
        if (background != null && background.TryGetComponent<Image>(out Image backgroundImage))
        {
            if (!field.IsUnlocked && lockedFieldSprite != null)
            {
                backgroundImage.sprite = lockedFieldSprite;
            }
            else
            {
                // 田地已解锁，恢复默认背景图片（设置为null让Unity使用默认）
                backgroundImage.sprite = fieldSprite;
            }
        }
        
        Transform checkmark = fieldToggle.transform.Find("Background/Checkmark");
        if (checkmark != null && checkmark.TryGetComponent<Image>(out Image checkmarkImage))
        {
            if (!field.IsUnlocked && selectedLockedFieldSprite != null)
            {
                checkmarkImage.sprite = selectedLockedFieldSprite;
            }
            else
            {
                // 田地已解锁，恢复默认选中图片（设置为null让Unity使用默认）
                checkmarkImage.sprite = selectedfieldSprite;
            }
        }
        
        // 更新作物图标（或工人头像）
        Transform cropImage = fieldToggle.transform.Find("crop");
        if (cropImage != null && cropImage.TryGetComponent<Image>(out Image image))
        {
            // 如果田地已解锁且有作物，显示作物图标
            if (field.IsUnlocked && field.CurrentCrop != null)
            {
                image.sprite = field.CurrentCrop.icon;
                image.gameObject.SetActive(true);
            }
            // 如果田地未解锁（正在开垦）且有开垦工人，显示工人头像
            else if (!field.IsUnlocked && field.CurrentWorker != null)
            {
                image.sprite = field.CurrentWorker.avatar;
                image.gameObject.SetActive(true);
            }
            // 其他情况隐藏图标
            else
            {
                image.gameObject.SetActive(false);
            }
        }
    }
}
    
    /// <summary>
    /// 更新农场信息
    /// </summary>
    public void UpdateFarmInfo(float currentProduction, float nextMonthProduction, PersonScriptableObject manager)
    {
        if (currentProductionText != null)
            currentProductionText.text = $"{currentProduction:F1}";
        
        if (nextMonthProductionText != null)
            nextMonthProductionText.text = $"{nextMonthProduction:F1}";
        
        UpdateManagerInfo(manager);
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
            
            if (managerProfessionText != null)
            {
                switch (manager.profession)
                {
                    case PersonProfession.observer:
                        managerProfessionText.text = "观星";
                        break;
                    case PersonProfession.tamer:
                        managerProfessionText.text = "驯化师";
                        break;
                    case PersonProfession.farmer:
                        managerProfessionText.text = "农民";
                        break;
                    case PersonProfession.sailor:
                        managerProfessionText.text = "水手";
                        break;
                    default:
                        managerProfessionText.text = "平民";
                        break;
                }
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
    /// 当生产计算完成
    /// </summary>
    private void OnProductionCalculated(float production)
    {
        farmSystem.CalculateNextMonthExpectedYield();
        UpdateFarmInfo(production, farmSystem.NextMonthExpectedYield, farmSystem.Manager);
        UpdateFieldUI();
    }

    /// <summary>
    /// 隐藏森林UI
    /// </summary>
    public void HideFarmUI()
    {
        HideCanvas();
    }

    /// <summary>
    /// 关闭森林UI按钮点击事件
    /// </summary>
    private void OnCloseFarmButtonClicked()
    {
        HideFarmUI();

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.RegisterInteractionPanel(false);
        }
    }
    
    /// <summary>
    /// 切换显示状态
    /// </summary>
    public void ToggleCanvas()
    {
        // 调用基类方法
        base.GetType().GetMethod("ToggleCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(this, null);
        
        // 或者直接实现相同逻辑
        gameObject.SetActive(!gameObject.activeSelf);
        
        if (IsCanvasVisible())
        {
            // 显示时更新UI
            UpdateFarmInfo(
                farmSystem.CurrentMonthProduction, 
                farmSystem.NextMonthExpectedYield, 
                farmSystem.Manager
            );
            UpdateFieldUI();
        }
    }
}