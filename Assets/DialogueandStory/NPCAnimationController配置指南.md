# NPC动画系统 - 快速配置指南

## 🎯 脚本挂载位置

### ✅ 正确的配置方式

```
NPC对象（GameObject）  ← NPCAnimationController 挂载在这里
├─ Box Collider
├─ NPC Animation Controller (脚本)  ← 挂载位置
├─ NPC模型
│  └─ Animator  ← 脚本会自动找到这里的Animator
└─ Dialogue Trigger
```

**重要：**
- ✅ **NPCAnimationController** 挂载在 **NPC对象** 上（和DialogueTrigger同级）
- ✅ **Animator** 在 **NPC模型** 子对象上
- ✅ 脚本会**自动查找子对象中的Animator**

---

## 📋 配置步骤（2分钟）

### 第1步：在NPC对象上添加组件

选中 **NPC对象**（不是NPC模型）：

```
NPC_Children (GameObject)
├─ 添加组件: NPC Animation Controller
```

### 第2步：配置Inspector参数

```
NPC Animation Controller
├─ Animator: [留空，自动查找]
│
├─ ✅ Auto Find Animator In Children: 勾选
├─ Animator Object Name: NPC模型  ← 填写模型子对象的名称（可选）
│
├─ Gesture A Trigger: GestureA
├─ Gesture B Trigger: GestureB
├─ Gesture C Trigger: GestureC
│
├─ ✅ Play Gesture On Dialogue: 勾选
├─ Min Delay Between Gestures: 0.5
├─ ✅ Random Gesture: 勾选
└─ Show Debug Log: 勾选（测试时）
```

---

## 🔍 自动查找Animator的行为

脚本会按以下顺序查找Animator：

### 1. 如果指定了 `Animator Object Name`
```
查找: transform.Find("NPC模型")
→ 获取该子对象的Animator组件
```

**示例：**
```
NPC_Children
└─ NPC模型  ← 精确查找这个名称的子对象
   └─ Animator
```

### 2. 如果没找到或未指定名称
```
查找: GetComponentInChildren<Animator>()
→ 获取任意子对象中的Animator组件
```

**示例：**
```
NPC_Children
├─ NPC模型
│  └─ Animator  ← 找到这个
└─ 其他子对象
   └─ Animator  ← 或找到这个（取第一个）
```

### 3. 如果都没找到
```
Console警告: "没有找到Animator组件！"
```

---

## 📁 NPC对象结构示例

### 示例1：单个模型子对象

```
NPC_VillageChief
├─ Box Collider
├─ Dialogue Trigger
├─ NPC Animation Controller  ← 脚本挂在这里
└─ Chief_Model  ← 子对象
   └─ Animator
      └─ Controller: ChiefAnimatorController
```

**配置：**
```
NPC Animation Controller:
├─ Auto Find Animator In Children: ✓
├─ Animator Object Name: Chief_Model  ← 可选
```

### 示例2：复杂的模型结构

```
NPC_Children
├─ Box Collider
├─ Dialogue Trigger
├─ NPC Animation Controller  ← 脚本挂在这里
└─ Character_Group
   └─ Children_Model
      └─ SkinnedMeshRenderer
      └─ Animator
         └─ Controller: ChildrenAnimatorController
```

**配置：**
```
NPC Animation Controller:
├─ Auto Find Animator In Children: ✓
├─ Animator Object Name: (留空)  ← 自动查找任意子对象
```

---

## 🎮 不同NPC使用不同的Animator Controller

### NPC 1: 孩子们

```
NPC_Children
└─ NPC模型
   └─ Animator
      └─ Controller: ChildrenAnimatorController  ← 每个NPC不同
```

**Animator Controller设置：**
```
ChildrenAnimatorController:
├─ Parameters:
│  ├─ GestureA (Trigger)
│  ├─ GestureB (Trigger)
│  └─ GestureC (Trigger)
│
└─ States: Idle + GestureA/B/C
```

### NPC 2: 库拉奶奶

```
NPC_KulaGrandma
└─ NPC模型
   └─ Animator
      └─ Controller: KulaAnimatorController  ← 不同的Controller
```

**Animator Controller设置：**
```
KulaAnimatorController:
├─ Parameters:
│  ├─ GestureA (Trigger)  ← 相同的参数名
│  ├─ GestureB (Trigger)
│  └─ GestureC (Trigger)
│
└─ States: Idle + GestureA/B/C  ← 但动画不同
```

**重要：**
- ✅ 每个NPC可以使用**不同的Animator Controller**
- ✅ 只要都有 **GestureA/B/C** 这3个Trigger参数即可
- ✅ 动画内容可以完全不同

---

## ⚙️ 手动指定Animator（可选）

如果自动查找失败，可以手动指定：

```
NPC_Children
├─ NPC Animation Controller
│  └─ Animator: 🖼️ 手动拖入 NPC模型/Animator
```

**操作：**
1. 在Hierarchy中展开NPC对象
2. 找到NPC模型下的Animator组件
3. 拖入到NPC Animation Controller的Animator字段

---

## 🔧 参数说明

| 参数 | 说明 | 推荐设置 |
|------|------|----------|
| **Animator** | 手动指定Animator（留空则自动查找） | 留空 |
| **Auto Find Animator In Children** | 是否在子对象中自动查找Animator | ✓ 勾选 |
| **Animator Object Name** | 子对象名称（可选，精确查找） | 填写模型名称 |
| **Gesture A/B/C Trigger** | 对应的Trigger参数名 | "GestureA/B/C" |
| **Play Gesture On Dialogue** | 对话时播放手势 | ✓ 勾选 |
| **Min Delay Between Gestures** | 手势间最小间隔（秒） | 0.3-1.0 |
| **Random Gesture** | 随机播放（false=顺序） | ✓ 勾选 |
| **Show Debug Log** | 显示调试日志 | 测试时勾选 |

---

## 🐛 常见问题

### 问题1：找不到Animator

**错误信息：**
```
NPCAnimationController: NPC_Children 没有找到Animator组件！
```

**解决方法：**
1. ✅ 确认勾选了 `Auto Find Animator In Children`
2. ✅ 确认子对象确实有Animator组件
3. ✅ 如果模型名称特殊，填写 `Animator Object Name`

### 问题2：手势不播放

**检查：**
- ✅ Animator Controller是否设置？
- ✅ 是否有GestureA/B/C这3个Trigger参数？
- ✅ Play Gesture On Dialogue是否勾选？
- ✅ 开启Show Debug Log查看调用情况

### 问题3：多个Animator组件

如果子对象有多个Animator：

```
NPC_Children
├─ NPC模型
│  └─ Animator  ← 想要这个
└─ UI子对象
   └─ Animator  ← 不想要这个
```

**解决方法：**
填写 `Animator Object Name` 精确指定：
```
Animator Object Name: NPC模型
```

---

## ✅ 配置检查清单

为每个NPC配置时，检查：

- [ ] NPCAnimationController挂载在NPC对象上
- [ ] Auto Find Animator In Children 已勾选
- [ ] Animator Object Name 已填写（可选）
- [ ] Animator Controller有GestureA/B/C参数
- [ ] Play Gesture On Dialogue 已勾选
- [ ] Min Delay已设置（建议0.5秒）
- [ ] 运行测试，Console显示"找到Animator"

---

## 💡 快速复制配置

### 方法1：复制组件

1. 配置好第一个NPC
2. 在Inspector中右键 **NPC Animation Controller** 组件
3. 选择 **Copy Component**
4. 选中其他NPC
5. Inspector右键 → **Paste Component**

### 方法2：预制体

1. 配置好一个完整的NPC
2. 拖入Project窗口创建Prefab
3. 后续直接使用Prefab

---

## 🎯 总结

**挂载位置：**
```
✅ NPC对象（GameObject） - 挂载 NPCAnimationController
   └─ NPC模型（子对象）- 包含 Animator
```

**配置要点：**
- ✅ 勾选 `Auto Find Animator In Children`
- ✅ 可选填写 `Animator Object Name` 精确查找
- ✅ 每个NPC可用不同的Animator Controller
- ✅ 参数名必须包含GestureA/B/C

**测试验证：**
```
运行游戏 → 触发对话 → 观察Console:
"在子对象中找到Animator: NPC模型"
"播放手势动画: GestureA"
```
