# 资源计算系统文档

## 概述
本文档详细说明了游戏中的资源变化计算机制,以及新创建的集中化资源计算系统。

---

## 1. 各系统资源计算机制

### 1.1 FarmSystem (农场系统)
**文件:** [FarmSystem.cs](../SystemProduction/Field/FarmSystem.cs)

**触发时机:** 每个月相变化 (OnPhaseChangedWithTotalPhases)

**计算公式:**
```
本月生产 = Σ(各田地产量) × 管理者加成
管理者加成 = 1.2 (如果管理者是farmer)
产出: +Crop
```

**特点:**
- 无退化机制
- 无消耗
- 只生产Crop资源

---

### 1.2 ForestSystem (森林系统)
**文件:** [ForestSystem.cs](../SystemProduction/Forest/ForestSystem.cs)

**触发时机:** 每个月相变化 (OnPhaseChangedWithTotalPhases)

**计算公式:**
```
本月生产 = Σ(nextPhaseYield × amount) × 管理者加成
本月消耗 = Σ(monthlyCropConsumption × amount)
管理者加成 = 1.2 (farmer) 或 0 (无管理者)

产出: +Crop (生产)
消耗: -Crop (维持材料物种)
```

**退化机制:**
```
退化后产量 = nextPhaseYield - decayPerPhase
退化后产量 = Max(退化后产量, leastYield) // 不低于最低产量
```

**特点:**
- 有退化机制(每月产量降低)
- 有消耗(需要Crop来维持)
- 只影响Crop资源

---

### 1.3 RanchSystem (牧场系统)
**文件:** [RanchSystem.cs](../SystemProduction/Ranch/RanchSystem.cs)

**触发时机:** 每个月相变化 (OnPhaseChangedWithTotalPhases)

**计算公式:**
```
本月生产 = Σ(nextPhaseYield × amount) × 管理者加成
本月消耗 = Σ(monthlyCropConsumption × amount)
管理者加成 = 1.2 (farmer) 或 0 (无管理者)

产出: +Crop (生产)
消耗: -Crop (饲养动物)
```

**退化机制:**
```
退化后产量 = nextPhaseYield - decayPerPhase
退化后产量 = Max(退化后产量, leastYield) // 不低于最低产量
```

**特点:**
- 有退化机制(每月产量降低)
- 有消耗(需要Crop来饲养动物)
- 只影响Crop资源

---

### 1.4 PersonManager (人口系统)
**文件:** [PersonManager.cs](../SystemPopulation/PersonManager.cs)

**触发时机:** 每个新周期开始 (OnNewCycle - 即4个月相,一次完整循环)

**计算公式:**
```
总消耗 = Σ(所有已招募人员)
Crop消耗 = Σ(monthlyCropConsumption)
Ani消耗 = Σ(monthlyAniConsumption)
Mat消耗 = Σ(monthlyMatConsumption)

消耗: -Crop, -Ani, -Mat
```

**特点:**
- 无退化机制
- 消耗三种资源
- 按周期(月)扣除,而非月相

---

## 2. 资源计算时序图

```
时间轴 (单位: 月相)
├─ Phase 0 (Crescent开始)
├─ Phase 1 (UpQuarterMoon)
├─ Phase 2 (FullMoon)
├─ Phase 3 (DownQuarterMoon)
└─ Phase 0 → 新周期开始

月相变化时 (每个Phase):
  1. FarmSystem: 收获作物 → +Crop
  2. ForestSystem: 生产材料 → +Crop, 消耗维持 → -Crop
  3. RanchSystem: 生产动物产品 → +Crop, 消耗饲料 → -Crop

新周期开始时 (Phase 0, Crescent):
  4. PersonManager: 扣除人口消耗 → -Crop, -Ani, -Mat
```

---

## 3. 新增资源计算系统

### 3.1 ResourceManagerCalculator
**文件:** [ResourceManagerCalculator.cs](ResourceManagerCalculator.cs)

**功能:** 集中计算每月的资源变化,提供完整的资源摘要

**核心数据结构:**
```csharp
public class ResourceChange
{
    public float crop;      // Crop变化量
    public float animal;    // Animal变化量
    public float material;  // Material变化量
}

public class MonthlyResourceSummary
{
    public ResourceChange currentMonthNetGrowth;  // 本月净增长
    public ResourceChange nextMonthNetGrowth;    // 下月净增长
    public ResourceChange monthOverMonthChange;   // 环比变化 (下月-本月)
    public ResourceChange currentResources;       // 当前资源量
}
```

**主要方法:**

1. **CalculateCurrentMonthNetGrowth()**
   - 计算本月资源净增长
   - 包括: Farm生产 + Forest生产/消耗 + Ranch生产/消耗 - 人口消耗

2. **CalculateNextMonthNetGrowth()**
   - 计算下月资源净增长
   - 考虑退化机制 (decayPerPhase)
   - 假设人口不变

3. **CalculateResourceSummary()**
   - 生成完整的资源摘要
   - 自动触发UI更新事件

---

### 3.2 ResourceSummaryUI
**文件:** [ResourceSummaryUI.cs](ResourceSummaryUI.cs)

**功能:** UI组件,显示资源摘要信息

**显示内容:**
1. **当前资源量**: Crop, Animal, Material的当前数量
2. **本月净增长**: 本月预计的资源变化 (+/-)
3. **下月净增长**: 下月预计的资源变化 (+/-)
4. **环比变化**: 下月与本月的变化差值 (↑/↓)

**颜色编码:**
- 绿色: 正增长
- 红色: 负增长
- 白色: 无变化

---

## 4. 使用示例

### 4.1 获取资源摘要
```csharp
// 获取当前资源摘要
var summary = ResourceManagerCalculator.Instance.GetResourceSummary();

// 读取数据
float currentCrop = summary.currentMonthNetGrowth.crop;
float nextCrop = summary.nextMonthNetGrowth.crop;
float cropChange = summary.monthOverMonthChange.crop;
```

### 4.2 订阅资源更新
```csharp
// 在Start()中订阅
ResourceManagerCalculator.Instance.OnResourceSummaryUpdated += OnResourceUpdated;

private void OnResourceUpdated(ResourceManagerCalculator.MonthlyResourceSummary summary)
{
    // 处理更新
    Debug.Log($"本月净增长: {summary.currentMonthNetGrowth}");
    Debug.Log($"下月净增长: {summary.nextMonthNetGrowth}");
}
```

### 4.3 手动刷新计算
```csharp
// 强制重新计算
ResourceManagerCalculator.Instance.CalculateResourceSummary();
```

---

## 5. 资源计算流程图

```
ResourceManagerCalculator.CalculateResourceSummary()
│
├─ CalculateCurrentMonthNetGrowth()
│   ├─ FarmSystem.CurrentMonthProduction (Crop +)
│   ├─ ForestSystem (生产 +, 消耗 -)
│   ├─ RanchSystem (生产 +, 消耗 -)
│   └─ PersonManager (人口消耗 -Crop, -Ani, -Mat)
│
├─ CalculateNextMonthNetGrowth()
│   ├─ FarmSystem.NextMonthExpectedYield (Crop +)
│   ├─ ForestSystem (考虑退化的生产 +, 消耗 -)
│   ├─ RanchSystem (考虑退化的生产 +, 消耗 -)
│   └─ PersonManager (假设人口不变, -Crop, -Ani, -Mat)
│
├─ GetCurrentResources()
│   └─ ResourceManager.Instance.GetCrop/Ani/MatAmount()
│
└─ 生成 MonthlyResourceSummary
    └─ 触发 OnResourceSummaryUpdated 事件
```

---

## 6. 重要说明

### 6.1 "每月"的定义
- **本月**: 指距离最近的下一个月相变化时刻
- **下月**: 指本月之后的月相变化时刻
- 实际触发频率由 `GlobalTimeSystem.phaseDuration` 决定

### 6.2 人口消耗时机
- 人口消耗按**周期**(4个月相)扣除,而非月相
- 在计算"本月"和"下月"时,假设人口消耗已经包含在内

### 6.3 退化机制影响
- 森林和牧场的产量会逐月递减
- 计算下月净增长时,已考虑退化因素
- 最低产量由 `leastYield` 限制

### 6.4 管理者加成
- Farm: farmer = 1.2x
- Forest: farmer = 1.2x, 无管理者 = 0x
- Ranch: farmer = 1.2x, 无管理者 = 0x

---

## 7. 扩展建议

### 7.1 添加新的生产系统
如果要添加新的生产系统(如矿场):
1. 在 `ResourceManagerCalculator` 中添加对应字段
2. 在 `CalculateCurrentMonthNetGrowth()` 中添加本月计算
3. 在 `CalculateNextMonthNetGrowth()` 中添加下月计算
4. 如果有退化,参考森林/牧场的实现

### 7.2 添加更多资源类型
如果要添加新资源(如金币):
1. 在 `ResourceChange` 中添加字段
2. 在各计算方法中添加对应资源的计算
3. 在UI中添加对应的显示组件

---

## 8. 故障排查

### 问题1: UI显示不更新
- 检查 `ResourceManagerCalculator` 是否正确初始化
- 确认 `OnPhaseChanged` 事件是否正确订阅
- 验证系统引用 (Farm/Forest/Ranch/Person) 是否为null

### 问题2: 计算结果不正确
- 检查各系统的 `CurrentMonthProduction` 等值是否正确
- 验证退化计算逻辑是否正确
- 确认管理者加成是否正确应用

### 问题3: 事件未触发
- 确认 `GlobalTimeSystem.Instance` 不为null
- 检查事件订阅时机 (应在 `OnEnable` 中订阅)
