# 从零开始创建可对话NPC - 完整教程

## 📋 教程概述

我们将创建一个可以对话的NPC，包含以下功能：
- ✅ 玩家走近时自动触发对话
- ✅ 打字机效果显示台词
- ✅ 多角色头像自动切换
- ✅ 剧情进度自动保存

**预计耗时：** 15-20分钟

---

## 第一步：准备角色头像图片（5分钟）

### 1.1 确认头像图片位置

你的头像图片应该在这里：
```
Assets/UIs/personIcons/
```

### 1.2 检查图片导入设置

对于每个头像图片：

1. 在 **Project** 窗口选中图片
2. 在 **Inspector** 窗口检查：
   ```
   Texture Type: Sprite (2D and UI)  ← 必须是这个
   ```
3. 如果不是，点击 **Apply** 按钮

**需要的角色头像：**
- [ ] 基诺.png
- [ ] 拉拉.png
- [ ] 库拉奶奶.png
- [ ] 玛莱.png
- [ ] 阿斯特拉.png
- [ ] 莉法.png
- [ ] 艾拉.png
- [ ] 玛拉.png
- [ ] 孩子们.png

---

## 第二步：创建角色头像映射配置（3分钟）

### 2.1 创建配置文件

1. 在 **Project** 窗口中，进入 `Assets/DialogueandStory/` 文件夹
2. 右键 → **Create** → **Dialogue** → **Character Sprite Mapping**
3. 命名为：`GlobalCharacterMapping`

### 2.2 配置角色头像

选中 `GlobalCharacterMapping`，在 Inspector 中：

| Size | Character Name | Character Sprite |
|------|----------------|------------------|
| 9 | 基诺 | 🖼️ 拖入 `Assets/UIs/personIcons/基诺.png` |
|   | 拉拉 | 🖼️ 拖入 `拉拉.png` |
|   | 库拉奶奶 | 🖼️ 拖入 `库拉奶奶.png` |
|   | 玛莱 | 🖼️ 拖入 `玛莱.png` |
|   | 阿斯特拉 | 🖼️ 拖入 `阿斯特拉.png` |
|   | 莉法 | 🖼️ 拖入 `莉法.png` |
|   | 艾拉 | 🖼️ 拖入 `艾拉.png` |
|   | 玛拉 | 🖼️ 拖入 `玛拉.png` |
|   | 孩子们 | 🖼️ 拖入 `孩子们.png` |

**操作步骤：**
1. 将 **Size** 设置为 `9`
2. 逐行填写角色名和拖入对应图片
3. **保存项目**（Ctrl+S）

---

## 第三步：在场景中创建NPC对象（2分钟）

### 3.1 创建NPC对象

1. 在 **Hierarchy** 窗口中右键
2. **Create Empty**
3. 命名为：`NPC_TestChildren`

### 3.2 添加Box Collider

1. 选中 `NPC_TestChildren`
2. **Add Component** → 搜索 `Box Collider`
3. 在 Inspector 中设置：
   ```
   Is Trigger: ✓ 勾选
   Center: (0, 1, 0)  ← 调整到NPC高度
   Size: (2, 2, 2)     ← 调整触发范围
   ```

### 3.3 添加Dialogue Trigger脚本

1. 选中 `NPC_TestChildren`
2. **Add Component** → 搜索 `Dialogue Trigger`
3. 保持选中，我们下一步配置它

---

## 第四步：编写台词文件（3分钟）

### 4.1 创建台词文件

1. 在 `Assets/DialogueandStory/Dialogues/` 文件夹
2. 右键 → **Create** → **Text Asset**
3. 命名为：`test_children.txt`

### 4.2 编写台词内容

双击打开 `test_children.txt`，粘贴以下内容：

```
[基诺]：（走近，带着笑意）
[基诺]：在争论什么呢？
[拉拉]：（跳下石头，眼睛亮晶晶地）
[拉拉]：基诺姐姐！我们在比赛唱"岛谣"，但他们总唱错！
[基诺]：那让我也听听看，是不是这样——"弯弯的月儿，躺在海中央……"
[孩子们]：（齐声接上，手拉手轻轻摇晃）
[孩子们]：一头枕着珊瑚床，一头翘着看远方！
```

**保存文件**（Ctrl+S）

---

## 第五步：配置Dialogue Trigger并导入台词（3分钟）

### 5.1 配置基本设置

选中 `NPC_TestChildren`，在 Inspector 的 **Dialogue Trigger** 组件中：

1. **Character Mapping**:
   - 从 Project 窗口拖入 `GlobalCharacterMapping`

2. **NPC Transform**:
   - 从 Hierarchy 拖入 `NPC_TestChildren` 自己

### 5.2 使用编辑器工具导入台词

在 Inspector 底部找到 **"台词自动导入工具"** 面板：

1. **角色头像映射**: 拖入 `GlobalCharacterMapping`（如果还没填）

2. 点击 **"浏览文件"** 按钮
   - 选择 `Assets/DialogueandStory/Dialogues/test_children.txt`
   - 点击 **"打开"**

3. 点击 **"导入台词"** 按钮

4. 成功提示：
   ```
   ✅ "成功导入 X 行对话"
   ```

**检查结果：**
在 Inspector 的 **Dialogue Strings** 中应该看到自动填充的对话内容！

---

## 第六步：测试NPC（2分钟）

### 6.1 设置玩家标签

确保你的玩家对象：
1. 在 Hierarchy 中选中玩家对象
2. Inspector 顶部 → **Tag** → 选择 `Player`
   - 如果没有 `Player` 标签，点击 **Add Tag...** 创建

### 6.2 运行测试

1. 点击 Unity 顶部的 **Play** 按钮 ▶️
2. 使用 **WASD** 移动玩家
3. 走近 `NPC_TestChildren` 对象

**预期效果：**
- ✅ 玩家进入触发范围
- ✅ 相机自动转向NPC
- ✅ 鼠标解锁，可以点击
- ✅ 显示对话UI
- ✅ 依次显示台词：
  - 第1行显示基诺头像
  - 第2行显示基诺头像
  - 第3行显示拉拉头像 ← 自动切换！
  - 第4行显示拉拉头像
  - 第5行显示基诺头像 ← 自动切回！
  - ...
- ✅ 打字机效果逐字显示
- ✅ 点击鼠标左键继续下一句
- ✅ 对话结束后恢复玩家控制

---

## 🎯 完整检查清单

在测试之前，确认所有项目都已完成：

### 准备工作
- [ ] 所有角色头像图片已放在 `Assets/UIs/personIcons/`
- [ ] 所有头像的 Texture Type 设置为 `Sprite (2D and UI)`
- [ ] 已创建 `GlobalCharacterMapping` 配置文件
- [ ] 配置中包含所有9个角色的映射

### NPC设置
- [ ] 已创建 `NPC_TestChildren` 对象
- [ ] 已添加 `Box Collider`（Is Trigger 勾选）
- [ ] 已添加 `Dialogue Trigger` 脚本
- [ ] `Character Mapping` 已设置为 `GlobalCharacterMapping`
- [ ] `NPC Transform` 已设置为 `NPC_TestChildren` 自己

### 台词文件
- [ ] 已创建 `test_children.txt`
- [ ] 文件格式正确：`[角色名]：对话内容`
- [ ] 已成功导入到 `Dialogue Strings`
- [ ] 导入后可以看到对话列表

### 玩家设置
- [ ] 玩家对象的 Tag 设置为 `Player`
- [ ] 玩家有 `DialogueManager` 组件
- [ ] 玩家有移动控制器

---

## 🐛 常见问题排查

### 问题1：走近NPC没有反应

**可能原因：**
- ❌ 玩家Tag不是"Player"
- ❌ Box Collider的Is Trigger未勾选
- ❌ 玩家身上没有DialogueManager组件

**解决方法：**
```
1. 选中玩家 → Inspector → Tag: Player
2. 选中NPC → Box Collider → Is Trigger: ✓
3. 选中玩家 → Add Component → Dialogue Manager
```

### 问题2：头像不显示或切换

**可能原因：**
- ❌ Character Mapping未配置
- ❌ 角色名不匹配
- ❌ 图片的Texture Type错误

**解决方法：**
```
1. 检查NPC的Character Mapping是否已拖入GlobalCharacterMapping
2. 检查txt文件中的角色名与配置中的名字是否完全一致
3. 选中图片 → Inspector → Texture Type: Sprite (2D and UI) → Apply
```

### 问题3：导入台词失败

**可能原因：**
- ❌ txt文件格式错误
- ❌ 文件编码不是UTF-8
- ❌ Character Mapping未设置

**解决方法：**
```
1. 确保每行格式为：[角色名]：对话内容
2. 用记事本打开txt → 另存为 → 编码选择UTF-8
3. 在导入工具中先设置Character Mapping
```

### 问题4：对话卡住无法继续

**可能原因：**
- ❌ 最后一行未标记为结束
- ❌ 鼠标未解锁

**解决方法：**
```
1. 检查Dialogue Strings最后一行的isEnd是否勾选
2. 按Esc键确保鼠标可见
3. 点击对话区域继续
```

---

## 📚 下一步学习

完成基础NPC后，你可以尝试：

### 1. 创建更多NPC

复制 `NPC_TestChildren`，只需：
- 修改台词文件路径
- 点击"导入台词"

### 2. 添加分支对话

手动配置 `Dialogue Strings`：
- 勾选 `isQuestion`
- 填写 `answerOption1/2`
- 设置 `option1IndexJump/option2IndexJump`

### 3. 添加剧情进度控制

在 `Dialogue Trigger` 中：
- **Plot Point ID**: `met_children`
- **Required Plot Points**: `["tutorial_completed"]`

---

## ✅ 恭喜！

你已经成功创建了一个可以对话的NPC！

**现在你可以：**
1. 复制这个NPC创建更多对话
2. 编写不同的台词文件
3. 让游戏中的9个角色都"活"起来

---

## 💡 小贴士

**快速创建新NPC的流程：**
```
1. 复制 NPC_TestChildren → 命名为 NPC_XXX
2. 编写新的台词txt文件
3. 选中NPC → Inspector → Dialogue Trigger
4. 点击"浏览文件"选择新txt
5. 点击"导入台词"
```

只需要**30秒**就能创建一个新NPC！

---

## 📞 需要帮助？

如果遇到问题：
1. 查看 Console 窗口的错误信息
2. 参考上方的"常见问题排查"
3. 检查本教程的"完整检查清单"

祝你游戏开发顺利！🎮
