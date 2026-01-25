# 酋长NPC秘密解锁系统 - 配置指南

## 🎯 功能概述

创建可以解锁岛屿秘密的特殊NPC（如酋长），玩家通过对话选择正确答案来解锁秘密。

**特性：**
- ✅ 支持多个岛屿的酋长（主岛、长尾鸟岛等）
- ✅ 自动查找对应岛屿的ProgressTableManager
- ✅ 防止重复解锁
- ✅ 静默解锁（不显示额外提示）
- ✅ 与对话系统的UnityEvent无缝集成

---

## 📦 文件说明

**[SecretUnlockerNPC.cs](Assets/DialogueandStory/SecretUnlockerNPC.cs)** - 秘密解锁NPC脚本

---

## 🎮 使用步骤

### 第1步：创建酋长NPC对象

在Hierarchy中创建酋长NPC：

```
NPC_Chief_MainIsland (主岛酋长)
├─ Box Collider (Is Trigger)
├─ NPC Animation Controller
├─ Dialogue Trigger
└─ Secret Unlocker NPC ← 添加这个组件

NPC_Chief_CrossIsland (长尾鸟岛酋长)
├─ Box Collider (Is Trigger)
├─ NPC Animation Controller
├─ Dialogue Trigger
└─ Secret Unlocker NPC ← 添加这个组件
```

### 第2步：配置SecretUnlockerNPC

选中酋长NPC，在Inspector中配置：

```
Secret Unlocker NPC:
├─ Target Island: MainIsland ← 选择对应的岛屿
├─ Custom Progress Table Manager: [留空，自动查找]
└─ Show Debug Log: ✓
```

**两个酋长的配置：**

**主岛酋长：**
```
Target Island: MainIsland
```

**长尾鸟岛酋长：**
```
Target Island: LongTailedBirdIsland
```

### 第3步：编写对话台词

创建txt文件，例如 `chief_main_island.txt`：

```
[酋长]：欢迎来到主岛，旅行者。
[酋长]：我有个问题想考考你。
[酋长]：我们的岛屿从远处看像什么？
```

### 第4步：配置DialogueTrigger

在Inspector中配置对话分支：

```
Dialogue Strings:
Element 0:
├─ text: "欢迎来到主岛，旅行者。"
├─ isEnd: ☐

Element 1:
├─ text: "我有个问题想考考你。"
├─ isEnd: ☐

Element 2:
├─ text: "我们的岛屿从远处看像什么？"
├─ isQuestion: ☑  ← 勾选，显示选项
├─ answerOption1: "像一只展开双翼的鸟"
├─ answerOption2: "像一个圆形的盘子"
├─ option1IndexJump: 3  ← 正确答案，跳转到Element 3
└─ option2IndexJump: 5  ← 错误答案，跳转到Element 5

Element 3 (正确答案分支):
├─ text: "回答正确！你很有观察力。"
├─ isEnd: ☐
├─ start Dialogue Event:
│  └─ Secret Unlocker NPC.UnlockSecret  ← 调用解锁方法！
└─ end Dialogue Event: (可选，播放音效等)

Element 4:
├─ text: "这是我们岛屿的秘密，希望你能珍惜。"
├─ isEnd: ☑

Element 5 (错误答案分支):
├─ text: "很遗憾，答案不对。再去探索一下岛屿吧。"
├─ isEnd: ☑
```

### 第5步：连接UnityEvent

在Element 3的 `start Dialogue Event` 中：

1. 点击 **+** 添加事件
2. 将 `NPC_Chief_MainIsland` 对象拖入对象槽
3. 下拉菜单选择：`SecretUnlockerNPC → UnlockSecret()`

---

## 📊 完整配置示例

### 主岛酋长

```
NPC_Chief_MainIsland
├─ Box Collider
│  ├─ Is Trigger: ✓
│  └─ Size: (2, 2, 2)
│
├─ NPC Animation Controller
│  └─ [配置动画...]
│
├─ Dialogue Trigger
│  ├─ Character Mapping: GlobalCharacterMapping
│  ├─ NPC Transform: NPC_Chief_MainIsland
│  ├─ Plot Point ID: met_chief_main_island
│  └─ dialogueStrings: [配置对话分支...]
│
└─ Secret Unlocker NPC
   ├─ Target Island: MainIsland  ← 主岛
   ├─ Custom Progress Table Manager: [留空]
   └─ Show Debug Log: ✓
```

### 长尾鸟岛酋长

```
NPC_Chief_CrossIsland
├─ Box Collider
│  ├─ Is Trigger: ✓
│  └─ Size: (2, 2, 2)
│
├─ NPC Animation Controller
│  └─ [配置动画...]
│
├─ Dialogue Trigger
│  ├─ Character Mapping: GlobalCharacterMapping
│  ├─ NPC Transform: NPC_Chief_CrossIsland
│  ├─ Plot Point ID: met_chief_cross_island
│  └─ dialogueStrings: [配置对话分支...]
│
└─ Secret Unlocker NPC
   ├─ Target Island: LongTailedBirdIsland  ← 长尾鸟岛
   ├─ Custom Progress Table Manager: [留空]
   └─ Show Debug Log: ✓
```

---

## 🎯 工作流程

```
玩家走近酋长
    ↓
触发对话
    ↓
酋长提出问题
    ↓
显示2个选项：
├─ 选项1（正确答案）
└─ 选项2（错误答案）
    ↓
玩家选择选项1
    ↓
跳转到正确答案分支 (Element 3)
    ↓
触发 startDialogueEvent
    ↓
调用 SecretUnlockerNPC.UnlockSecret()
    ↓
ProgressTableManager.UnlockSecret()
    ├─ 检查秘密是否存在
    ├─ 设置 isUnlocked = true
    ├─ 触发 OnSecretUnlocked 事件
    └─ 通知海图志UI更新
    ↓
显示确认台词 (Element 4)
    ↓
对话结束
    ↓
玩家打开海图志 → 可以看到已解锁的秘密内容
```

---

## 🔧 进阶功能

### 1. 手动指定ProgressTableManager

如果自动查找失败，可以手动指定：

```
Secret Unlocker NPC:
├─ Target Island: [任意]
└─ Custom Progress Table Manager: 🖼️ 拖入 ExploreCanva/MainIslandPage
```

**优先级：** `Custom Progress Table Manager` > `Target Island`

### 2. 检查秘密是否已解锁

如果想让酋长根据秘密是否解锁而改变对话：

在对话的 `requiredPlotPoints` 中配置：

```
Dialogue Trigger:
└─ Required Plot Points: ["exploration_50_percent"]
```

或使用代码检查：

```csharp
SecretUnlockerNPC unlocker = GetComponent<SecretUnlockerNPC>();
if (unlocker.IsSecretUnlocked())
{
    // 已解锁，显示不同的对话
}
```

### 3. 解锁后触发其他事件

在Element 3的 `endDialogueEvent` 中添加多个事件：

```
endDialogueEvent:
├─ SecretUnlockerNPC.UnlockSecret()  ← 解锁秘密
├─ AudioManager.PlaySecretUnlockSound()  ← 播放音效
└─ AchievementManager.UnlockAchievement()  ← 解锁成就
```

---

## 🐛 调试

### Console日志

运行游戏后，Console会显示：

```
✅ [NPC_Chief_MainIsland] 自动找到ProgressTableManager: MainIsland
✅ [NPC_Chief_MainIsland] 成功解锁 'MainIsland' 的秘密！
```

### 场景视图调试

- ✅ **绿色线框** = 秘密已解锁
- ❌ **红色线框** = 秘密未解锁

### Inspector调试

运行时选中SecretUnlockerNPC，在Inspector底部可以看到：

```
Target Island: MainIsland
Custom Progress Table Manager: None (Assigned)
Show Debug Log: True
```

---

## ⚠️ 常见问题

### 问题1：找不到ProgressTableManager

**错误信息：**
```
未找到岛屿 'MainIsland' 的ProgressTableManager
```

**解决方法：**
1. 确保ExploreCanva在场景中
2. 确保MainIslandPage子对象存在
3. 确保MainIslandPage挂载了ProgressTableManager
4. 或手动拖入Custom Progress Table Manager字段

### 问题2：解锁失败

**可能原因：**
- 秘密已经解锁过了（正常情况）
- ProgressTableManager的boundSecret为null

**检查方法：**
```csharp
SecretUnlockerNPC unlocker = GetComponent<SecretUnlockerNPC>();
SecretScriptableObject secret = unlocker.GetSecret();
if (secret == null)
{
    Debug.Log("该岛屿没有配置秘密");
}
```

### 问题3：UnityEvent不触发

**检查：**
- Event是否正确连接到SecretUnlockerNPC对象？
- 选择的方法是否是 `UnlockSecret()`（无参数）？

---

## 📝 SpeciesSource枚举值

根据你的探索系统，可用的岛屿枚举：

```csharp
public enum SpeciesSource
{
    MainIsland,              // 主岛
    LongTailedBirdIsland,    // 长尾鸟岛
    // ... 其他岛屿
}
```

**使用示例：**
```
主岛酋长: Target Island = MainIsland
长尾鸟岛酋长: Target Island = LongTailedBirdIsland
```

---

## ✅ 配置检查清单

为每个酋长NPC配置时，检查：

- [ ] NPC对象创建完成
- [ ] 添加了Secret Unlocker NPC组件
- [ ] Target Island设置正确（主岛/长尾鸟岛）
- [ ] Show Debug Log已勾选
- [ ] DialogueTrigger配置了对话分支
- [ ] 正确答案的startDialogueEvent连接了UnlockSecret()
- [ ] 运行测试，Console显示成功信息

---

## 🎉 测试流程

1. **配置酋长NPC**
   - 设置Target Island
   - 配置对话分支

2. **运行游戏**
   - 走近酋长触发对话

3. **选择错误答案**
   - 验证不解锁秘密

4. **重新运行，选择正确答案**
   - 观察Console: "成功解锁 'MainIsland' 的秘密"
   - 打开海图志UI
   - 查看对应岛屿页面
   - 确认秘密已显示（不再是锁定状态）

---

## 💡 设计建议

### 谜题设计

**主岛酋长的谜题：**
- 关于主岛形状的问题
- 答案："像一只展开双翼的鸟"

**长尾鸟岛酋长的谜题：**
- 关于长尾鸟岛形状的问题
- 答案："像一只尾巴弯弯的长尾鸟"

### 对话设计建议

1. **铺垫** - 先介绍背景
2. **提问** - 提出谜题
3. **选项** - 给出2个选项
4. **反馈** - 根据选择给出不同回应
5. **奖励** - 正确答案解锁秘密

### 重复对话处理

如果玩家再次与酋长对话：

```
[酋长]：你已经知道了我们岛屿的秘密。
[酋长]：希望你能好好珍惜这份知识。
```

可以在DialogueTrigger中设置Plot Point ID来控制：
- 第一次对话：有选项的对话
- 解密后：简单的确认对话

---

## 📚 相关文件

- [SecretUnlockerNPC.cs](Assets/DialogueandStory/SecretUnlockerNPC.cs) - 本脚本
- [ExploreSystem.cs](Assets/SystemExplore/ExploreSystem.cs) - 探索系统管理器
- [ProgressTableManager.cs](Assets/SystemExplore/ProgressTableManager.cs) - 岛屿进度管理器
- [SecretScriptableObject.cs](Assets/SystemExplore/SecretScriptableObject.cs) - 秘密数据结构
- [IslandPageController.cs](Assets/SystemExplore/IslandPageController.cs) - 海图志UI控制器
