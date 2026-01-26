# 十字星岛酋长NPC配置指南

## 🎯 对话概述

**NPC名称：** 阿斯特拉（十字星岛酋长）
**解锁奖励：** 十字星岛的**信物（Token）**
**涉及角色：** 阿斯特拉、基诺
**分支类型：** 2个选项（选项1正确→解锁信物，选项2错误）

---

## 📋 对话流程图

```
玩家走近阿斯特拉
    ↓
[0-5] 开场对话
    ↓
[5] 显示2个选项：
    ├─ 选项1（正确）：天星坠陨而来...
    │    ↓
    │  跳转到索引 7
    │    ↓
    │  [7-11] 正确答案分支
    │    ↓
    │  [16-17] 解锁信物，对话结束
    │
    └─ 选项2（错误）：我们从玉之城出发...
         ↓
       跳转到索引 12
         ↓
       [12] 错误反馈，对话结束
```

---

## 🎮 NPC配置步骤

### 第1步：创建酋长NPC对象

在Hierarchy中创建：

```
NPC_Chief_CrossStarIsland
├─ Box Collider (Is Trigger)
├─ NPC Animation Controller
├─ Dialogue Trigger
└─ Explore Reward Unlocker NPC ← 添加这个组件
```

### 第2步：配置ExploreRewardUnlockerNPC

选中NPC，在Inspector中配置：

```
Explore Reward Unlocker NPC:
├─ Target Island: CrossStarIsland  ← 十字星岛
├─ Custom Progress Table Manager: [留空]
│
├─ Unlock Type:
│  ├─ Unlock Secret: ☐  ← 不勾选
│  └─ Unlock Token: ☑   ← 勾选（解锁信物）
│
└─ Show Debug Log: ✓
```

### 第3步：配置DialogueTrigger基本信息

```
Dialogue Trigger:
├─ Character Mapping: GlobalCharacterMapping
├─ NPC Transform: NPC_Chief_CrossStarIsland
├─ Plot Point ID: met_chief_cross_star
└─ dialogueStrings: Size = 18
```

---

## 📝 DialogueStrings详细配置

### 第1部分：开场对话（Element 0-5）

```
Element 0:
├─ text: 你来了......
├─ speakerName: 阿斯特拉
├─ isEnd: ☐
└─ isQuestion: ☐

Element 1:
├─ text: 你好。
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 2:
├─ text: 我们曾是一家。
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 3:
├─ text: 也许吧，回忆有如礁石上的青苔，在潮汐日复一日的冲刷下消失的无影无踪。
├─ speakerName: 阿斯特拉
├─ isEnd: ☐
└─ isQuestion: ☐

Element 4:
├─ text: 不，不是的，我知道——
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 5:
├─ text: 我们的家园如何而来
├─ speakerName: 阿斯特拉
├─ isQuestion: ☑  ← 勾选！显示选项
├─ answerOption1: 天星坠陨而来，先祖追随天星到达这里，这是十字星岛的开始。
├─ answerOption2: 我们从玉之城出发，向着南十字星的尽头航行，这是十字星岛的开始。
├─ option1IndexJump: 7  ← 正确答案，跳转到Element 7
└─ option2IndexJump: 12  ← 错误答案，跳转到Element 12
```

**注意：** Element 5的`speakerName`是"阿斯特拉"，但问题本身"我们的家园如何而来"不需要显示speakerName，因为这是选择题的提示。

### 第2部分：正确答案分支（Element 7-11, 16-17）

```
Element 7:
├─ text: 你是如何知道的？
├─ speakerName: 阿斯特拉
├─ isEnd: ☐
└─ isQuestion: ☐

Element 8:
├─ text: 我知道，我还知道，我们都是玉之城的后裔，我们从玉之城而来，只是海水将我们分离
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 9:
├─ text: 所以......
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 10:
├─ text: 我们本是一家......
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 11:
├─ text: 我们本是一家......
├─ speakerName: 阿斯特拉
├─ isEnd: ☐
└─ isQuestion: ☐

Element 16:
├─ text: （现在我可以在海图志里解锁信物了）
├─ speakerName: 基诺
├─ isEnd: ☐
└─ isQuestion: ☐

Element 17:
├─ text: 这是我族的信物，请您收下......
├─ speakerName: 阿斯特拉
├─ isEnd: ☑  ← 对话结束
├─ startDialogueEvent:
│  ├─ Size: 1
│  └─ Element 0:
│     ├─ Object: NPC_Chief_CrossStarIsland
│     └─ Function: ExploreRewardUnlockerNPC.UnlockToken()  ← 调用这个！
└─ isQuestion: ☐
```

**连接UnityEvent：**
1. 在Element 17的`startDialogueEvent`中点击`+`
2. 将`NPC_Chief_CrossStarIsland`对象拖入Object槽
3. 下拉菜单选择：`ExploreRewardUnlockerNPC → UnlockToken()`

### 第3部分：错误答案分支（Element 12）

```
Element 12:
├─ text: 旅人，我钦佩你的勇气，但请您再想一想吧......
├─ speakerName: 阿斯特拉
├─ isEnd: ☑  ← 对话结束
└─ isQuestion: ☐
```

---

## 🔑 关键配置点

### 1. 角色名称

确保`speakerName`与GlobalCharacterMapping中的名称一致：

| 台词中的角色 | speakerName字段 | 确认 |
|-------------|-----------------|------|
| 阿斯特拉 | 阿斯特拉 | ✓ 确认配置中存在 |
| 基诺 | 基诺 | ✓ 确认配置中存在 |

### 2. 分支跳转

| 起始Element | 目标Element | 说明 |
|------------|------------|------|
| Element 5 → 选项1 | Element 7 | 正确答案分支 |
| Element 5 → 选项2 | Element 12 | 错误答案分支 |

### 3. 解锁调用

```
Element 17:
└─ startDialogueEvent:
   └─ ExploreRewardUnlockerNPC.UnlockToken()
```

**只有正确答案才会执行到Element 17，从而解锁信物！**

---

## 🎯 完整对话索引表

| 索引 | speakerName | text | isEnd | isQuestion | 跳转 | 事件 |
|-----|-------------|------|-------|-----------|------|------|
| 0 | 阿斯特拉 | 你来了...... | ☐ | ☐ | - | - |
| 1 | 基诺 | 你好。 | ☐ | ☐ | - | - |
| 2 | 基诺 | 我们曾是一家。 | ☐ | ☐ | - | - |
| 3 | 阿斯特拉 | 也许吧... | ☐ | ☐ | - | - |
| 4 | 基诺 | 不，不是的，我知道—— | ☐ | ☐ | - | - |
| 5 | 阿斯特拉 | 我们的家园如何而来 | ☐ | ☑ | →7, →12 | - |
| 7 | 阿斯特拉 | 你是如何知道的？ | ☐ | ☐ | - | - |
| 8 | 基诺 | 我知道，我还知道... | ☐ | ☐ | - | - |
| 9 | 基诺 | 所以...... | ☐ | ☐ | - | - |
| 10 | 基诺 | 我们本是一家...... | ☐ | ☐ | - | - |
| 11 | 阿斯特拉 | 我们本是一家...... | ☐ | ☐ | - | - |
| 12 | 阿斯特拉 | 旅人，我钦佩你的勇气... | ☑ | ☐ | - | - |
| 16 | 基诺 | （现在我可以在海图志里解锁信物了） | ☐ | ☐ | - | - |
| 17 | 阿斯特拉 | 这是我族的信物，请您收下...... | ☑ | ☐ | - | UnlockToken |

---

## 🧪 测试流程

### 测试1：错误答案

1. 运行游戏
2. 走近NPC_Chief_CrossStarIsland
3. 触发对话
4. 在选项中选择**选项2**（错误答案）
5. 验证：
   - ✅ 显示"旅人，我钦佩您的勇气..."
   - ✅ 对话结束
   - ❌ 信物**未**解锁（海图志中信物仍锁定）

### 测试2：正确答案

1. 重新运行游戏（或清除存档）
2. 走近NPC
3. 触发对话
4. 在选项中选择**选项1**（正确答案）
5. 验证：
   - ✅ 显示"你是如何知道的？"
   - ✅ 继续对话...
   - ✅ 显示"这是我族的信物，请您收下......"
   - ✅ 对话结束
   - ✅ Console显示："成功解锁 'CrossStarIsland' 的信物！"
   - ✅ 打开海图志，十字星岛页面的信物已解锁

### 测试3：重复对话

1. 再次走近NPC
2. 验证：
   - ✅ 对话正常进行
   - ✅ Console显示："信物已经解锁过了，跳过"
   - ✅ 不会重复解锁

---

## 🔧 调试信息

### Console日志（正确答案）

```
[NPC_Chief_CrossStarIsland] 自动找到ProgressTableManager: CrossStarIsland
[NPC_Chief_CrossStarIsland] ✓ 成功解锁 'CrossStarIsland' 的信物！
```

### 场景视图调试

运行时选中NPC，在Scene视图可以看到：
- 🔴 **红色线框** = 信物未解锁
- 🟢 **绿色线框** = 信物已解锁

---

## ⚠️ 常见问题

### 问题1：选项不显示

**检查：**
- Element 5的`isQuestion`是否勾选？
- `answerOption1`和`answerOption2`是否填写？

### 问题2：选择选项后没反应

**检查：**
- `option1IndexJump`和`option2IndexJump`是否设置正确？
- 目标Element是否存在？

### 问题3：信物不解锁

**检查：**
- ExploreRewardUnlockerNPC的`Unlock Token`是否勾选？
- `Target Island`是否设置为`CrossStarIsland`？
- Element 17的`startDialogueEvent`是否正确连接？

### 问题4：角色名称显示错误

**检查：**
- `speakerName`是否与GlobalCharacterMapping中的名称完全一致？
- 包括中文字符、空格等

---

## ✅ 配置检查清单

- [ ] NPC对象创建完成
- [ ] ExploreRewardUnlockerNPC已添加
- [ ] Target Island设置为CrossStarIsland
- [ ] Unlock Token已勾选
- [ ] DialogueTrigger配置完成
- [ ] Element 5设置为问题（isQuestion勾选）
- [ ] 选项跳转索引正确（7和12）
- [ ] Element 17的startDialogueEvent已连接UnlockToken()
- [ ] 所有speakerName已填写
- [ ] 运行测试通过

---

## 📚 相关文件

- [ExploreRewardUnlockerNPC.cs](Assets/DialogueandStory/ExploreRewardUnlockerNPC.cs) - 解锁脚本
- [十字星岛酋长.txt](Assets/DialogueandStory/Dialogues/十字星岛酋长.txt) - 原始台词
- [ExploreSystem.cs](Assets/SystemExplore/ExploreSystem.cs) - 探索系统
- [ProgressTableManager.cs](Assets/SystemExplore/ProgressTableManager.cs) - 进度管理器

---

## 💡 设计提示

### 故事背景

这个对话揭示了：
- **十字星岛的起源**：天星坠陨，先祖追随天星而来
- **历史联系**：所有岛屿居民都是玉之城的后裔
- **情感纽带**：阿斯特拉与基诺的同根同源

### 奖励设计

- **正确答案**：解锁信物（永久奖励）
- **错误答案**：获得提示，可以再来尝试

### 扩展可能

未来可以添加：
- 根据是否解锁信物，显示不同的对话
- 阿斯特拉赠送信物时的特效/音效
- 解锁信物后的成就系统
