# 全局角色头像映射配置指南

## 📁 推荐的资源文件夹结构

```
Assets/
├── Resources/
│   └── CharacterSprites/
│       ├── Jinuo.png          # 基诺
│       ├── Lala.png           # 拉拉
│       ├── KulaGrandma.png    # 库拉奶奶
│       ├── Malai.png          # 玛莱
│       ├── Astra.png          # 阿斯特拉
│       ├── Lifa.png           # 莉法
│       ├── Aila.png           # 艾拉
│       ├── Mara.png           # 玛拉
│       └── Children.png       # 孩子们
│
└── DialogueandStory/
    ├── Dialogues/
    │   ├── 2长尾鸟岛村落旁与孩子们对话.txt
    │   ├── 3与库拉奶奶对话.txt
    │   └── ...
    │
    └── CharacterMappings/
        └── GlobalCharacterMapping.asset  # 全局角色映射配置
```

---

## 🎨 创建全局角色映射配置

### 第一步：准备角色头像图片

1. **设置图片导入设置**
   - 选中图片 → Inspector → Texture Type: `Sprite (2D and UI)`
   - Multiple: 如果是图集，可以勾选
   - Filter Mode: `Bilinear` (平滑显示)

2. **图片命名规范**
   ```
   角色名_表情.png
   例如：
   - Jinuo_Normal.png    # 基诺-普通
   - Jinuo_Happy.png     # 基诺-开心
   - Lala_Smile.png      # 拉拉-微笑
   ```

### 第二步：创建全局CharacterSpriteMapping

1. **创建配置文件**
   ```
   在 Project 窗口：
   右键 → Create → Dialogue → Character Sprite Mapping
   命名为: GlobalCharacterMapping
   移动到: Assets/DialogueandStory/CharacterMappings/
   ```

2. **配置所有角色**

   在Inspector中填写完整列表：

   | Character Name | Character Sprite |
   |----------------|------------------|
   | 基诺 | 🖼️ 拖入基诺的头像 |
   | 拉拉 | 🖼️ 拖入拉拉的头像 |
   | 库拉奶奶 | 🖼️ 拖入库拉奶奶的头像 |
   | 玛莱 | 🖼️ 拖入玛莱的头像 |
   | 阿斯特拉 | 🖼️ 拖入阿斯特拉的头像 |
   | 莉法 | 🖼️ 拖入莉法的头像 |
   | 艾拉 | 🖼️ 拖入艾拉的头像 |
   | 玛拉 | 🖼️ 拖入玛拉的头像 |
   | 孩子们 | 🖼️ 拖入孩子们的群像 |

### 第三步：在所有NPC中使用

**每个NPC的DialogueTrigger都使用同一个GlobalCharacterMapping：**

```
NPC_孩子们对话:
├─ Character Mapping: GlobalCharacterMapping ← 所有NPC共用
├─ NPC Transform: 自己的Transform
└─ 导入台词: 2长尾鸟岛村落旁与孩子们对话.txt

NPC_库拉奶奶:
├─ Character Mapping: GlobalCharacterMapping ← 同一个配置
├─ NPC Transform: 自己的Transform
└─ 导入台词: 3与库拉奶奶对话.txt
```

---

## 🔧 使用流程示例

### 创建一个新NPC（以"库拉奶奶"为例）

#### 1. 准备台词文件

**Assets/DialogueandStory/Dialogues/3与库拉奶奶对话.txt**

```
[库拉奶奶]：啊，小家伙，你终于来了。
[库拉奶奶]：这把老骨头都快等不动了。
[基诺]：库拉奶奶，我想请教您关于岛上的历史。
[库拉奶奶]：历史？哼，那可不是什么好听的故事。
[库拉奶奶]：你要想知道，就得先帮我找到那些散落的记忆碎片。
```

#### 2. 创建NPC对象

```
Hierarchy:
└─ NPC_KulaGrandma
   ├─ Box Collider (Is Trigger)
   ├─ Dialogue Trigger
   │  ├─ Character Mapping: GlobalCharacterMapping
   │  ├─ NPC Transform: NPC_KulaGrandma
   │  └─ 导入台词: 3与库拉奶奶对话.txt
   └─ 3D Model
```

#### 3. 导入台词

选中NPC_KulaGrandma：
1. Inspector → Dialogue Trigger
2. 确认 **Character Mapping** 已设置为 `GlobalCharacterMapping`
3. 点击"浏览文件" → 选择 `3与库拉奶奶对话.txt`
4. 点击"导入台词"

完成！游戏运行时，当不同角色说话，头像会自动切换：
- 库拉奶奶说话 → 显示库拉奶奶的头像
- 基诺说话 → 自动切换为基诺的头像

---

## 📊 角色列表检查清单

使用此清单确保所有角色都已配置：

- [ ] 基诺 - 准备图片 ✓
- [ ] 拉拉 - 准备图片 ✓
- [ ] 库拉奶奶 - 准备图片 ✓
- [ ] 玛莱 - 准备图片 ✓
- [ ] 阿斯特拉 - 准备图片 ✓
- [ ] 莉法 - 准备图片 ✓
- [ ] 艾拉 - 准备图片 ✓
- [ ] 玛拉 - 准备图片 ✓
- [ ] 孩子们 - 准备图片 ✓

---

## 💡 高级功能：同一角色多个表情

如果需要同一角色的不同表情，有两种方案：

### 方案A：在台词中指定表情

```
[基诺_开心]：太好了！
[基诺_难过]：怎么会这样...
```

需要修改CharacterSpriteMapping，添加更多条目：
- 基诺_开心
- 基诺_难过
- 基诺_愤怒

### 方案B：使用代码控制（更灵活）

在 `dialogueString` 中添加表情字段，通过UnityEvent切换。

---

## 🎯 快速创建步骤

1. **创建文件夹**
   - `Assets/Resources/CharacterSprites/` - 存放所有头像
   - `Assets/DialogueandStory/CharacterMappings/` - 存放配置文件

2. **创建全局映射**
   - 右键 → Create → Dialogue → Character Sprite Mapping
   - 命名为 `GlobalCharacterMapping`

3. **配置角色头像**
   - 将所有角色头像拖入配置表
   - 确保角色名与txt文件中的名字完全一致

4. **应用到所有NPC**
   - 每个NPC的DialogueTrigger都引用这个`GlobalCharacterMapping`

---

## ⚠️ 注意事项

1. **角色名一致性**
   - txt文件中的角色名必须与CharacterSpriteMapping中的名称完全一致
   - `[基诺]` ≠ `[基诺 ]` (注意空格)

2. **图片格式**
   - 建议使用 PNG 格式
   - 尺寸建议：256x256 或 512x512
   - 背景透明

3. **性能优化**
   - 所有角色头像加载后常驻内存（因为是ScriptableObject）
   - 如果角色数量超过50个，考虑分场景创建多个Mapping

4. **版本控制**
   - .asset文件需要提交到Git
   - 建议使用Git LFS管理图片资源
