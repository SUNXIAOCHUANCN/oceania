# Token解锁失败排查指南

## 🎯 问题症状

选择正确的对话选项后，Token（信物）没有解锁。

---

## 🔍 快速检查清单

按顺序检查以下项目：

### ✅ 检查项1：Console日志

运行游戏，选择正确选项后，查看Console：

**期望看到：**
```
✅ [NPC名称] 自动找到ProgressTableManager: CrossStarIsland
✅ [NPC名称] ✓ 成功解锁 'CrossStarIsland' 的信物！
```

**如果看到错误：**
```
❌ 未找到岛屿 'CrossStarIsland' 的ProgressTableManager
```
→ **原因：** ProgressTableManager未找到
→ **解决：** 检查目标岛屿名称，或手动指定Custom Progress Table Manager

```
❌ 信物已经解锁过了，跳过
```
→ **原因：** 信物已经解锁过了（正常情况）

**如果完全没有任何日志：**
→ **原因：** UnlockToken()方法没有被调用
→ **解决：** 检查UnityEvent连接（见检查项2）

---

### ✅ 检查项2：UnityEvent连接

选中NPC对象，在Inspector中找到**Dialogue Trigger**组件：

1. 展开`dialogueStrings`列表
2. 找到**Element 17**（或最后一个Element）
3. 展开`start Dialogue Event`
4. 检查是否有事件连接

**正确配置：**
```
start Dialogue Event:
├─ Size: 1  ← 必须至少有1个
└─ Element 0:
   ├─ Object: NPC_Chief_CrossStarIsland  ← 必须是NPC对象
   └─ Function: ExploreRewardUnlockerNPC.UnlockToken()  ← 必须是这个方法
```

**如果Size是0：**
→ **没有连接事件！**
→ **解决：** 点击`+`添加事件，连接UnlockToken()

**如果Object是None或错误的：**
→ **对象未正确连接！**
→ **解决：** 从Hierarchy拖入正确的NPC对象

**如果Function选择错误：**
→ **方法未正确连接！**
→ **解决：** 重新选择正确的函数

---

### ✅ 检查项3：ExploreRewardUnlockerNPC配置

选中NPC对象，检查`Explore Reward Unlocker NPC`组件：

**关键配置：**
```
Explore Reward Unlocker NPC:
├─ Target Island: CrossStarIsland  ← 必须匹配
├─ Custom Progress Table Manager: [可选]
│
├─ Unlock Type:
│  ├─ Unlock Secret: ☐
│  └─ Unlock Token: ☑  ← 必须勾选！！！
│
└─ Show Debug Log: ✓  ← 必须勾选，才能看到日志
```

**最常见错误：**
- ❌ **Unlock Token未勾选** → 导致不会解锁！

**验证方法：**
1. 查看Inspector中`Unlock Token`是否勾选
2. 如果未勾选，勾选它
3. 保存场景
4. 重新运行测试

---

### ✅ 检查项4：目标岛屿名称

确认`Target Island`与ProgressTableManager的岛屿一致：

**查找方法：**
1. 在Hierarchy中找到`ExploreCanva`对象
2. 展开查看子对象：`MainIslandPage`、`CrossIslandPage`等
3. 选中子对象，查看`ProgressTableManager`组件
4. 记下`Data Table`中的`Island`字段值

**常见岛屿名称：**
- `MainIsland` (主岛)
- `LongTailedBirdIsland` (长尾鸟岛)
- `CrossStarIsland` (十字星岛)

**确保NPC的Target Island与之一致！**

---

### ✅ 检查项5：对话索引跳转

确认Element 5的跳转设置正确：

```
Element 5:
├─ isQuestion: ☑  ← 必须勾选
├─ answerOption1: 天星坠陨而来...
├─ answerOption2: 我们从玉之城...
├─ option1IndexJump: 7  ← 正确答案→跳到7
└─ option2IndexJump: 12  ← 错误答案→跳到12
```

**验证流程：**
```
选择选项1 → Element 7 → 8 → 9 → 10 → 11 → 16 → 17(执行解锁)
选择选项2 → Element 12 → 对话结束(不解锁)
```

**常见错误：**
- ❌ `option1IndexJump`不是7，导致跳转错误
- ❌ Element 7-17之间某个Element的`isEnd`错误设置为true，导致对话提前结束

---

## 🛠️ 使用调试工具

### 方法1：使用调试助手

1. 打开Unity菜单：`Tools → 对话系统 → Token解锁调试助手`
2. 在Hierarchy中选择酋长NPC对象
3. 拖入到调试助手的"目标NPC"字段
4. 查看配置信息
5. 点击"测试解锁Token"按钮

### 方法2：手动测试

在运行时（Play模式），选中NPC对象，在Inspector中点击`ExploreRewardUnlockerNPC`组件右上角的三个点，选择"Debug"：

1. 展开`Target Island`字段，查看值是否正确
2. 展开`Unlock Token`，确认是否为true
3. 右键组件 → 选择"Unlock Token()"方法手动调用
4. 查看Console输出

---

## 🧪 完整测试流程

### 测试1：验证配置

```
1. 选中NPC对象
2. 检查ExploreRewardUnlockerNPC配置
   ├─ Target Island = CrossStarIsland ✓
   ├─ Unlock Token = 勾选 ✓
   └─ Show Debug Log = 勾选 ✓
3. 检查Dialogue Trigger
   └─ Element 17的startDialogueEvent已连接UnlockToken() ✓
```

### 测试2：运行测试

```
1. 点击Play
2. 走近NPC触发对话
3. 选择选项1（正确答案）
4. 观察对话是否继续到Element 17
5. 查看Console是否有解锁日志
6. 打开海图志，检查Token是否解锁
```

### 测试3：手动调用

```
1. 选中NPC对象
2. 在Inspector中找到ExploreRewardUnlockerNPC组件
3. 右键组件 → "Unlock Token()"
4. 查看Console输出
```

**如果手动调用能解锁：**
- ✅ 配置正确
- ❌ UnityEvent连接有问题

**如果手动调用也不能解锁：**
- ❌ 配置有问题（检查Target Island、Unlock Token等）

---

## 📊 常见错误对照表

| Console日志 | 原因 | 解决方法 |
|-----------|------|----------|
| 完全没有日志 | UnlockToken()未被调用 | 检查UnityEvent连接 |
| 未找到ProgressTableManager | 目标岛屿错误 | 修改Target Island |
| 信物已经解锁过了 | 已解锁（正常） | 无需处理 |
| 解锁信物失败 | ProgressTableManager的boundToken为null | 检查ScriptableObject配置 |
| 成功解锁，但海图志未显示 | UI未更新 | 检查IslandPageController |

---

## 💡 解决方案示例

### 案例1：UnlockToken()没有被调用

**症状：** Console没有任何日志

**检查：**
```
Element 17:
└─ startDialogueEvent:
   ├─ Size: 0  ← 这是问题！
```

**解决：**
```
1. 点击 Size 旁边的 + 按钮
2. 将NPC对象拖入 Object 槽
3. 选择 ExploreRewardUnlockerNPC.UnlockToken()
```

---

### 案例2：目标岛屿不匹配

**症状：** Console显示"未找到岛屿 'CrossStarIsland' 的ProgressTableManager"

**检查：**
```
ExploreRewardUnlockerNPC:
└─ Target Island: MainIsland  ← 错误！应该是CrossStarIsland
```

**解决：**
```
修改 Target Island 为正确的岛屿名称
```

或手动指定：
```
ExploreRewardUnlockerNPC:
└─ Custom Progress Table Manager: 🖼️ 拖入 ExploreCanva/CrossStarIslandPage
```

---

### 案例3：Unlock Token未勾选

**症状：** Console没有日志，但UnityEvent已连接

**检查：**
```
ExploreRewardUnlockerNPC:
└─ Unlock Token: ☐  ← 这是问题！
```

**解决：**
```
勾选 Unlock Token
```

---

## 🎯 最终确认

完成以下所有检查后，重新测试：

1. ✅ Console显示"成功解锁"日志
2. ✅ 海图志中Token已显示为解锁状态
3. ✅ 重复对话显示"已经解锁过了"
4. ✅ ExploreSaveData中Token的isUnlocked = true

如果所有项都通过，说明Token解锁系统工作正常！

---

## 📞 仍无法解决？

请提供以下信息：

1. **Console完整日志**（复制所有相关输出）
2. **ExploreRewardUnlockerNPC的Inspector截图**
3. **Element 17的startDialogueEvent配置截图**
4. **Hierarchy结构**（ExploreCanva及其子对象）
5. **对话是否能正常进行到Element 17？**

这样我可以更精确地帮你定位问题！
