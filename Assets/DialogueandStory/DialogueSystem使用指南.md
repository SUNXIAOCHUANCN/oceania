# NPC对话系统使用指南

## 📁 文件结构

```
Assets/DialogueandStory/
├── CharacterSpriteMapping.cs    # 角色头像映射配置类
├── DialogueParser.cs             # 台词解析器
├── DialogueManager.cs            # 对话管理器
├── DialogueTrigger.cs            # 对话触发器
├── PlotProgressManager.cs        # 剧情进度管理
└── Editor/
    └── DialogueTriggerEditor.cs  # 编辑器扩展工具

Assets/DialogueandStory/Dialogues/
└── 2长尾鸟岛村落旁与孩子们对话.txt  # 台词文件示例
```

---

## 🎯 两种布置NPC的方式

### 方式1️⃣：手动在Inspector中配置

适用于简单对话，配置流程：

1. **创建NPC对象**
   - Hierarchy → 右键 → Create Empty
   - 命名为 "NPC_XXX"

2. **添加组件**
   - Box Collider (Is Trigger: ✓)
   - DialogueTrigger 脚本

3. **手动配置对话内容**
   - 在 `dialogueStrings` 中逐行添加对话
   - 填写 text, isEnd 等字段

---

### 方式2️⃣：使用txt文件自动导入 ⭐推荐

适用于多角色长对话，配置流程：

#### 第一步：创建角色头像映射配置

1. **创建配置文件**
   ```
   右键 → Create → Dialogue → Character Sprite Mapping
   命名为: VillageChildrenMapping
   ```

2. **配置角色头像**
   在Inspector中填写：

   | Character Name | Character Sprite |
   |----------------|------------------|
   | 基诺           | 拖入基诺的头像图片 |
   | 拉拉           | 拖入拉拉的头像图片 |
   | 孩子们         | 拖入孩子们的群像图片 |

#### 第二步：编写txt台词文件

在 `Assets/DialogueandStory/Dialogues/` 下创建txt文件：

```
[基诺]：（走近，带着笑意）
[基诺]：在争论什么呢？
[拉拉]：（跳下石头，眼睛亮晶晶地）
[拉拉]：基诺姐姐！我们在比赛唱"岛谣"，但他们总唱错！
[基诺]：那让我也听听看，是不是这样——"弯弯的月儿，躺在海中央……"
[孩子们]：（齐声接上，手拉手轻轻摇晃）
[孩子们]：一头枕着珊瑚床，一头翘着看远方！
```

**格式规则：**
- `[角色名]：对话内容`
- 支持中文冒号 `：` 和英文冒号 `:`
- 空行会被自动跳过
- 最后一行会自动标记为对话结束

#### 第三步：导入到DialogueTrigger

1. 选中NPC对象
2. 在Inspector中找到 **Dialogue Trigger** 组件
3. 配置以下字段：
   - **Character Mapping**: 拖入第一步创建的 CharacterSpriteMapping
   - **NPC Transform**: 拖入NPC自己的Transform
   - **Plot Point ID**: （可选）填写剧情点ID

4. 在编辑器面板底部找到 **"台词自动导入工具"**：
   - 点击 **"浏览文件"** 选择txt文件
   - 点击 **"导入台词"** 按钮

完成！对话内容会自动填充到 `dialogueStrings` 列表中。

---

## 🔄 多角色对话图片切换原理

当txt文件中不同角色说话时，系统会：

1. 解析器提取角色名 → 填充到 `dialogueString.speakerName`
2. DialogueManager 显示每行对话时 → 检查 `speakerName`
3. 从 `CharacterSpriteMapping` 查找对应头像 → 切换UI显示的图片

```
[基诺]：你好 → 显示基诺的头像
[拉拉]：基诺姐姐！ → 切换为拉拉的头像
[基诺]：谢谢 → 切换回基诺的头像
```

---

## 📝 完整示例

### 示例：创建"与孩子们对话"的NPC

#### 1. 创建角色头像映射

```
Assets/Resources/CharacterMappings/
└── VillageChildrenMapping.asset
```

配置内容：
- 基诺 → Sprite_Jinuo
- 拉拉 → Sprite_Lala
- 孩子们 → Sprite_Children

#### 2. 创建txt台词文件

```
Assets/DialogueandStory/Dialogues/
└── 2长尾鸟岛村落旁与孩子们对话.txt
```

内容：
```
[基诺]：（走近，带着笑意）
[基诺]：在争论什么呢？
[拉拉]：（跳下石头，眼睛亮晶晶地）
[拉拉]：基诺姐姐！我们在比赛唱"岛谣"，但他们总唱错！
[基诺]：那让我也听听看...
```

#### 3. 创建NPC并导入

1. 创建NPC对象 "NPC_VillageChildren"
2. 添加 Box Collider (Is Trigger)
3. 添加 DialogueTrigger 脚本
4. 拖入 VillageChildrenMapping 到 Character Mapping 字段
5. 点击"浏览文件"选择txt文件
6. 点击"导入台词"

#### 4. 测试

运行游戏，走进NPC触发区域：
- ✅ 相机转向NPC
- ✅ 显示对话UI
- ✅ 每个人说话时显示对应的头像
- ✅ 依次显示对话内容

---

## ⚙️ 高级功能

### 分支对话

手动配置时可以设置：
```
Element 0:
├─ text: "你想做什么？"
├─ isQuestion: ☑
├─ answerOption1: "接受任务"
├─ answerOption2: "拒绝"
├─ option1IndexJump: 1  → 跳转到Element 1
└─ option2IndexJump: 3  → 跳转到Element 3
```

### 剧情进度控制

在DialogueTrigger中设置：
- **Plot Point ID**: `met_village_chief`
- **Required Plot Points**: `["tutorial_completed"]`

这样只有完成tutorial的玩家才能触发此对话。

---

## 🐛 常见问题

| 问题 | 解决方法 |
|------|----------|
| 导入后对话不显示 | 检查txt文件格式，确保使用 `[角色名]：内容` |
| 图片不切换 | 检查Character Mapping是否正确配置，角色名是否完全一致 |
| 触发器不工作 | 检查玩家Tag是否为"Player"，Collider是否设置为Trigger |
| 保存后导入失效 | 重新点击导入按钮 |

---

## 📌 注意事项

1. txt文件必须使用UTF-8编码（避免中文乱码）
2. 角色名必须与Character Mapping中的名称完全一致
3. 图片资源必须是Sprite类型
4. txt文件放在 `Assets/DialogueandStory/Dialogues/` 下（或Resources文件夹中用于Resources.Load）
5. 修改txt文件后需要重新导入

---

## 💡 提示

- 批量导入NPC时，可以先创建一个CharacterMapping模板
- 使用版本控制管理txt台词文件，方便协作
- 可以在txt中添加注释（系统会跳过无法解析的行）
