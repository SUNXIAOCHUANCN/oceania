using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ProductionPanel : MonoBehaviour
{
    public enum ViewCategory { Field, Pasture, Plantation }

    // 内部类：用于跟踪处于建设状态的生产单元及其 UI 文本组件
    private class ConstructionUnitDisplay
    {
        public ProductionUnitScriptableObject unit;
        public Text statusText;
    }

    [Header("Layout")]
    public RectTransform switchContainer;    // 左侧切换栏容器
    public RectTransform listContainer;      // 右侧列表容器
    
    [Header("Prefabs")]
    public GameObject typeButtonPrefab;      // 类型切换按钮预制
    public GameObject unlockedUnitPrefab;    // 对应已解锁状态的生产单元
    public GameObject lockedUnitPrefab;      // 对应未解锁状态的生产单元

    private ProductionUnit productionSystem;
    private ViewCategory currentCategory = ViewCategory.Field;

    private class ProductionUnitDisplay
    {
        public ProductionUnitScriptableObject unit;
        public Slider progressSlider;
    }

    // ... 在 ProductionPanel 类的顶部
    private List<ConstructionUnitDisplay> activeConstructionUnits = new List<ConstructionUnitDisplay>(); // 建设中
    private List<ProductionUnitDisplay> activeProductionUnits = new List<ProductionUnitDisplay>(); // 生产中

    // 暂存用户在下拉菜单选择的资源
    private Dictionary<ProductionUnitScriptableObject, ResourceScriptableObject> pendingSelections = new Dictionary<ProductionUnitScriptableObject, ResourceScriptableObject>();
    // 获取 ConstructionUnit 中定义的月时长（建设所需时长）
    // 假设 GlobalTimeSystem 存在并能提供 TotalElapsedTime
    private float monthSeconds => GlobalTimeSystem.Instance != null ? GlobalTimeSystem.Instance.phaseDuration * 4f : 120f;

    private void Awake()
    {
        // 兼容不同 Unity 版本查找 ProductionUnit
#if UNITY_2023_2_OR_NEWER
        productionSystem = Object.FindFirstObjectByType<ProductionUnit>();
#else
        productionSystem = FindObjectOfType<ProductionUnit>();
#endif
    }

    private void OnEnable()
    {
        BuildSwitchButtonsIfNeeded();
        RefreshSelectedList();
    }
    
    private void Start()
    {
        BuildSwitchButtonsIfNeeded();
        RefreshSelectedList();
    }

    // --- 左侧切换栏逻辑  ---
    private void BuildSwitchButtonsIfNeeded()
    {
        if (switchContainer == null) return;
        
        // 检查是否已经实例化过。如果容器内有子物体，则认为已完成配置。
        if (switchContainer.childCount > 0) return; 

        // --- 按钮配置数据 ---
        string[] names = { "田地", "牧场", "栽培林" };

        if (typeButtonPrefab != null)
        {
            // 方案 A：实例化设计师提供的整个容器预制体 (SwitchUnitPrefab)
            var containerGO = Instantiate(typeButtonPrefab, switchContainer);
            containerGO.name = "TypeSwitchContainer"; 

            // 尝试从实例化后的对象中找到所有按钮
            Button[] buttons = containerGO.GetComponentsInChildren<Button>(true);
            
            if (buttons.Length >= 3) 
            {
                for (int i = 0; i < 3; i++)
                {
                    Button btn = buttons[i];
                    int idx = i;
                    
                    // 绑定点击事件
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnSwitchCategory((ViewCategory)idx));
                    
                    // 更新按钮文本
                    Text txt = btn.GetComponentInChildren<Text>();
                    if (txt != null) txt.text = names[i];
                }
            }
            else
            {
                Debug.LogWarning("ProductionPanel: SwitchUnitPrefab 内部没有找到足够的 Button 组件进行事件绑定！");
            }
        }
        else
        {
            // 方案 B：没有预制体，运行时创建简单按钮
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("TypeBtn" + i);
                go.transform.SetParent(switchContainer, false);
                var img = go.AddComponent<Image>();
                img.color = Color.gray;
                var btn = go.AddComponent<Button>();
                int idx = i;
                btn.onClick.AddListener(() => { OnSwitchCategory((ViewCategory)idx); });
                var txtGO = new GameObject("Text"); txtGO.transform.SetParent(go.transform, false);
                var txt = txtGO.AddComponent<Text>(); txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
                txt.text = names[i];
            }
        }
    }

    public void OnSwitchCategory(ViewCategory cat)
    {
        currentCategory = cat;
        Debug.Log($"切换到 {currentCategory} 类型的生产单元栏。");
        RefreshSelectedList();
    }

    // --- 核心刷新逻辑 ---
    public void RefreshSelectedList()
    {
        if (listContainer == null || productionSystem == null) return;

        // 1. 清空当前列表并清除计时跟踪
        for (int i = listContainer.childCount - 1; i >= 0; i--) 
            Destroy(listContainer.GetChild(i).gameObject);
        activeConstructionUnits.Clear(); 
        activeProductionUnits.Clear();

        // 2. 获取当前分类的数据列表
        List<ProductionUnitScriptableObject> list = GetCurrentUnitList();
        if (list == null) return;
        
        // 3. 确定当前分类对应的资源筛选类型
        ResourceCategory requiredCategory = GetRequiredResourceCategory(currentCategory);

        // 4. 遍历生成 UI
        for (int i = 0; i < list.Count; i++)
        {
            var unit = list[i];
            if (unit == null) continue;

            if (unit.canProduction)
            {
                CreateUnlockedUnitUI(unit, list, i, requiredCategory);
            }
            else
            {
                CreateLockedUnitUI(unit, list, i);
            }
        }
    }
    
    private List<ProductionUnitScriptableObject> GetCurrentUnitList()
    {
        switch (currentCategory)
        {
            case ViewCategory.Field: return productionSystem.field;
            case ViewCategory.Pasture: return productionSystem.pasture;
            case ViewCategory.Plantation: return productionSystem.plantation;
            default: return null;
        }
    }

    private ResourceCategory GetRequiredResourceCategory(ViewCategory cat)
    {
        switch (cat)
        {
            case ViewCategory.Field: return ResourceCategory.Crop;
            case ViewCategory.Pasture: return ResourceCategory.Livestock;
            case ViewCategory.Plantation: return ResourceCategory.Material;
            default: return ResourceCategory.Crop; // 默认值
        }
    }

    // --- 创建：已解锁单元 (UnlockedUnitPrefab) ---
    private void CreateUnlockedUnitUI(ProductionUnitScriptableObject unit, List<ProductionUnitScriptableObject> list, int index, ResourceCategory requiredCategory)
    {
        if (unlockedUnitPrefab == null) return;
        GameObject go = Instantiate(unlockedUnitPrefab, listContainer);

        Text nameTxt = FindComponentInChild<Text>(go, "UnitName"); 
        if (nameTxt != null) nameTxt.text = unit.unitID ?? $"Unit {index + 1}";

        // === 1. 资源下拉菜单 (使用暂存机制) ===
        Dropdown resDropdown = FindComponentInChild<Dropdown>(go, "ResourceName");
        List<ResourceScriptableObject> availableResources = new List<ResourceScriptableObject>();

        if (resDropdown != null && ResourceManager.Instance != null)
        {
            resDropdown.ClearOptions();
            availableResources = ResourceManager.Instance.knownResources
                .Where(r => r.category == requiredCategory).ToList();

            List<string> options = new List<string> { "请选择资源" };
            options.AddRange(availableResources.Select(r => r.resourceName));
            resDropdown.AddOptions(options);

            // 确定当前应该显示哪个资源：优先生产中的，其次暂存的
            ResourceScriptableObject currentDisplayRes = unit.Resource;
            if (currentDisplayRes == null && pendingSelections.ContainsKey(unit))
            {
                currentDisplayRes = pendingSelections[unit];
            }

            int initialValue = 0;
            if (currentDisplayRes != null)
            {
                int idxInList = availableResources.IndexOf(currentDisplayRes);
                if (idxInList != -1) initialValue = idxInList + 1;
            }
            resDropdown.value = initialValue;
            
            // 监听器：只更新 pendingSelections
            resDropdown.onValueChanged.RemoveAllListeners(); 
            resDropdown.onValueChanged.AddListener((val) => {
                if (val == 0)
                {
                    if (pendingSelections.ContainsKey(unit)) pendingSelections.Remove(unit);
                }
                else if (val > 0 && val <= availableResources.Count)
                {
                    pendingSelections[unit] = availableResources[val - 1];
                }
                RefreshSelectedList(); // 刷新以更新按钮状态
            });
        }
        
        // === 2. 生产进度条 (更新 Update 跟踪列表) ===
        Slider progressSlider = FindComponentInChild<Slider>(go, "Slider");
        if (progressSlider != null)
        {
            bool isProducing = unit.Resource != null && unit.ProductionStart > 0;
            
            if (isProducing)
            {
                activeProductionUnits.Add(new ProductionUnitDisplay { unit = unit, progressSlider = progressSlider });
                if (GlobalTimeSystem.Instance != null)
                {
                    float elapsed = GlobalTimeSystem.Instance.TotalElapsedTime - unit.ProductionStart;
                    progressSlider.value = Mathf.Clamp01(elapsed / unit.Resource.growthTime);
                }
            }
            else
            {
                progressSlider.value = 0f;
            }
        }
        
        // === 3. 自动生产 Toggle ===
        Toggle autoToggle = FindComponentInChild<Toggle>(go, "Toggle");
        if (autoToggle != null)
        {
            autoToggle.isOn = unit.autoReplant;
            autoToggle.onValueChanged.AddListener((isOn) => unit.autoReplant = isOn);
        }
        
        // === 4. 管理员下拉菜单 ===
        Dropdown mgrDropdown = FindComponentInChild<Dropdown>(go, "Manager");
        if (mgrDropdown != null && PopulationManager.Instance != null)
        {
            mgrDropdown.ClearOptions();
            var availablePeople = PopulationManager.Instance.allPeople
                .Where(p => p.currentStatus == Person.Status.Free || p == unit.Manager).ToList();

            List<string> options = new List<string> { "无管理员" }; 
            foreach (var p in availablePeople) options.Add(p.personName);
            mgrDropdown.AddOptions(options);

            int initialValue = 0;
            if (unit.Manager != null)
            {
                int pIndex = availablePeople.IndexOf(unit.Manager);
                if (pIndex != -1) 
                {
                    initialValue = pIndex + 1;
                }
                else
                {
                    if (currentCategory == ViewCategory.Pasture && unit.ProductionStart <= 0f)
                    {
                        unit.Manager.currentStatus = Person.Status.Free;
                        unit.Manager = null;
                    }

                    initialValue = 0;
                        
                }
            }

            mgrDropdown.value = initialValue;;

            mgrDropdown.onValueChanged.RemoveAllListeners();
            mgrDropdown.onValueChanged.AddListener((val) => {
                if (val == 0) {
                    if (unit.Manager != null) 
                    { 
                        unit.Manager.currentStatus = Person.Status.Free; 
                        unit.Manager = null; 
                    }
                } else {
                    var person = availablePeople[val - 1];
                    if (unit.Manager != null && unit.Manager != person) 
                        unit.Manager.currentStatus = Person.Status.Free;
                        
                    unit.Manager = person;
                    unit.Manager.currentStatus = Person.Status.Working; 
                }
                RefreshSelectedList(); // 每次管理员变动必须刷新，以便更新按钮状态
            });
        }
        
        // === 5. 开始生产按键 ===
        Button startBtn = FindComponentInChild<Button>(go, "StartButton");
        Text startBtnTxt = FindComponentInChild<Text>(go, "StartButton/Text"); 

        if (startBtn != null)
        {
            // 1. 获取当前状态
            // 检查是否有“待确认”的资源，或者是“正在生产”的资源
            ResourceScriptableObject targetResource = pendingSelections.ContainsKey(unit) ? pendingSelections[unit] : unit.Resource;
            
            bool hasResource = targetResource != null;
            bool hasManager = unit.Manager != null;
            bool isProducing = unit.Resource != null && unit.ProductionStart > 0; // 真正的生产状态

            bool canStart = false; // 默认不可开始
            string buttonText = "开始生产";

            // 2. 检查条件
            if (isProducing)
            {
                buttonText = "生产中...";
            }
            else if (!hasResource)
            {
                buttonText = "请选择资源";
            }
            else if (currentCategory == ViewCategory.Pasture && !hasManager)
            {
                buttonText = "需管理员";
            }
            else
            {
                canStart = true;
            }

            // 3. 应用状态
            startBtn.interactable = canStart; // 只有 canStart 为 true 时按钮才启用
            if (startBtnTxt != null) startBtnTxt.text = buttonText;

            // 4. 设置点击事件
            startBtn.onClick.RemoveAllListeners();
            if (canStart)
            {
                startBtn.onClick.AddListener(() => {
                    // A. 将暂存的资源真正赋值给 Unit
                    if (pendingSelections.ContainsKey(unit))
                    {
                        unit.Resource = pendingSelections[unit];
                        pendingSelections.Remove(unit);
                    }
                    
                    // B. 调用系统开始生产
                    TryStartProduction(unit, list);
                    
                    // C. 解决问题 1: 强制设置开始时间，让进度条立即滑动
                    if (GlobalTimeSystem.Instance != null)
                    {
                        unit.ProductionStart = GlobalTimeSystem.Instance.TotalElapsedTime;
                    }

                    // D. 立即刷新 UI (确保按钮变为“生产中...”)
                    RefreshSelectedList(); 
                });
            }
        }
    }

    // --- 创建：未解锁单元 (LockedUnitPrefab) ---
    // 解决问题 3, 4, 5
    private void CreateLockedUnitUI(ProductionUnitScriptableObject unit, List<ProductionUnitScriptableObject> list, int index)
    {
        if (lockedUnitPrefab == null) return;
        GameObject go = Instantiate(lockedUnitPrefab, listContainer);

        // 1. 单元名
        Text nameTxt = FindComponentInChild<Text>(go, "UnitName"); 
        if (nameTxt != null) nameTxt.text = $"{unit.unitID}";

        // 2. 工人选择 (Dropdown)
        Dropdown workerDropdown = FindComponentInChild<Dropdown>(go, "Worker");
        // 3. 建设按钮 (Button) 
        Button buildBtn = go.GetComponentInChildren<Button>(); 
        // 4. 建设状态文本 (Text)
        Text statusTxt = FindComponentInChild<Text>(go, "ConstructionStatusText");

        // 检查是否处于建设状态 (工人已分配且开始时间已标记)
        bool isConstructing = unit.Worker != null && unit.workerAssignedStart > 0f;
        
        // ------------------------------------
        // A. 处理建设状态
        // ------------------------------------
        if (isConstructing)
        {
            // 隐藏交互 UI
            if (workerDropdown != null) workerDropdown.gameObject.SetActive(false);
            if (buildBtn != null) buildBtn.gameObject.SetActive(false);
            
            // 显示状态文本
            if (statusTxt != null) 
            {
                statusTxt.gameObject.SetActive(true); // 此时显示
                // 添加到计时列表，在 Update 中实时更新
                activeConstructionUnits.Add(new ConstructionUnitDisplay 
                {
                    unit = unit,
                    statusText = statusTxt
                });
                statusTxt.text = "建设中..."; 
            }
        }
        // ------------------------------------
        // B. 处理未建设状态
        // ------------------------------------
        else
        {
            if (buildBtn != null) 
            {
                buildBtn.gameObject.SetActive(true);
            }
            if (statusTxt != null)
            {
                statusTxt.gameObject.SetActive(false);
            }
            if (workerDropdown != null)
            {
                workerDropdown.gameObject.SetActive(true);
            }

            Person currentWorker = unit.Worker;
            int initialDropdownValue = 0;
            
            
            if (workerDropdown != null && PopulationManager.Instance != null)
            {
                
                workerDropdown.ClearOptions();
                
                // 获取当前空闲工人 (确保不能选择已在建设或生产的工人)
                List<Person> freePeople = new List<Person>();
                freePeople = PopulationManager.Instance.allPeople
                    .Where(p => p.currentStatus == Person.Status.Free)
                    .ToList();
                
                List<string> options = new List<string>(){"选择工人"};
                foreach (var p in freePeople) options.Add(p.personName);        
                workerDropdown.AddOptions(options);
                
                if (currentWorker != null)
                {
                    int workerIndex = freePeople.IndexOf(currentWorker);
                    if (workerIndex != -1)
                    {
                        initialDropdownValue = workerIndex + 1; // +1 因为第0项是“选择工人”
                    }                   
                }

                workerDropdown.value = initialDropdownValue;
                
                workerDropdown.onValueChanged.RemoveAllListeners();
                workerDropdown.onValueChanged.AddListener((val) => {
                    if (val > 0)
                    {
                        unit.Worker = freePeople[val - 1];
                    }
                    else
                    {
                        unit.Worker = null;
                    }
                    RefreshSelectedList();
                });    
            }

            // 建设按钮点击逻辑 - 解决问题 5
            if (buildBtn != null)
            {
                bool hasWorker = unit.Worker != null; // 检查下拉菜单是否已选人
                
                if (!hasWorker)
                {
                    buildBtn.interactable = false;
                    buildBtn.GetComponentInChildren<Text>().text = "请选择工人";
                }
                else
                {
                    buildBtn.interactable = true;
                    buildBtn.GetComponentInChildren<Text>().text = "开始建设";
                }
                
                buildBtn.onClick.RemoveAllListeners();
                if (hasWorker)
                {
                    buildBtn.onClick.AddListener(() => {
                        // **调用 ProductionSystem 开始建设** (worker已通过Dropdown暂存到unit.Worker)
                        if (productionSystem.StartConstruction(unit, currentCategory)) 
                        {
                            // 建设成功后，立即刷新以切换到计时状态
                            RefreshSelectedList(); 
                        }
                        else
                        {
                            // 建设失败 (可能是材料不足)，清除Worker并刷新，让用户重新选择
                            if (unit.Worker != null) 
                            { 
                                unit.Worker = null;
                            }
                            RefreshSelectedList();
                            Debug.LogWarning("无法开始建设，可能是材料不足。");
                        }
                    });
                }
            }
        }
    }
    
    // --- Update 循环：实时更新建设计时 ---
    private void Update()
    {
        // 假设 GlobalTimeSystem 存在
        if (GlobalTimeSystem.Instance == null) return;
        float now = GlobalTimeSystem.Instance.TotalElapsedTime;

        // 1. 更新建设进度
        float totalConstructionTime = monthSeconds;
        for(int i = activeConstructionUnits.Count - 1; i >= 0; i--)
        {
            var display = activeConstructionUnits[i];
            var unit = display.unit;

            // 检查 ProductionUnit 系统是否已将 canProduction 设为 true (建设完成)
            if (unit.canProduction)
            {
                // 建设完成，触发刷新，清理 UI
                activeConstructionUnits.RemoveAt(i);
                RefreshSelectedList();
                return; // 刷新后 UI 元素被销毁，必须停止迭代
            }

            float elapsed = now - unit.workerAssignedStart;
            float remaining = totalConstructionTime - elapsed;

            if (remaining > 0)
            {
                // 格式化为 MM:SS
                int seconds = Mathf.CeilToInt(remaining);
                int minutes = seconds / 60;
                seconds %= 60;
                display.statusText.text = $"建设中... 剩余时间: {minutes:00}:{seconds:00}";
            }
            else
            {
                // 建设时间已到，显示等待状态 
                display.statusText.text = "建设完成！等待系统结算...";
            }
        }

        // 2. 更新生产进度
        for (int i = activeProductionUnits.Count - 1; i >= 0; i--)
        {
            var display = activeProductionUnits[i];
            var unit = display.unit;

            // 检查生产是否完成 (假设生产系统会在生产时间结束后重置 ProductionStart 或 Resource)
            if (unit.Resource == null || unit.ProductionStart <= 0f)
            {
                if(display.progressSlider != null) display.progressSlider.value = 0f;
                // 生产完成或被外部停止，移除跟踪并刷新 UI
                activeProductionUnits.RemoveAt(i);
                continue; // 刷新后 UI 元素可能被销毁，停止迭代
            }

            if(display.progressSlider != null)
            {
                float elapsed = now - unit.ProductionStart;
                float totalTime = unit.Resource.growthTime; // 从资源获取所需时间
                float progress = Mathf.Clamp01(elapsed / totalTime);

                display.progressSlider.value = progress;

                if (progress >= 1.0f && !unit.autoReplant)
                {
                    activeProductionUnits.RemoveAt(i);
                }
            }
            
        }
    }
    
    // --- 辅助方法 (TryStartProduction 和 FindComponentInChild 逻辑不变) ---

    // 尝试根据当前资源开始生产
    private void TryStartProduction(ProductionUnitScriptableObject unit, List<ProductionUnitScriptableObject> list)
    {
        if (unit.Resource == null) return;
        int idx = list.IndexOf(unit);
        
        if (currentCategory == ViewCategory.Field) 
            productionSystem.PlantCropInField(idx, unit.Resource, unit.autoReplant);
        else if (currentCategory == ViewCategory.Pasture)
        {
            productionSystem.AssignLivestockToPasture(idx, unit.Resource);
            Debug.Log($"当前生产单元存在管理员：{unit.Manager.personName}");
        }
        else 
            productionSystem.ProduceMaterialInPlantation(idx, unit.Resource);
    }
    
    // 帮助按名称查找子物体上的组件
    private T FindComponentInChild<T>(GameObject parent, string childName) where T : Component
    {
        Transform t = parent.transform.Find(childName);
        if (t != null) return t.GetComponent<T>();
        
        T[] all = parent.GetComponentsInChildren<T>(true);
        foreach (T comp in all)
        {
            if (comp.gameObject.name == childName) return comp;
        }
        return null;
    }
    
}
