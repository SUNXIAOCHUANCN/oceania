# NPC对话手势动画系统 - 使用指南

## 🎯 功能概述

让NPC在对话时自动播放手势动画，每个NPC每说一句话随机播放一个手势动画。

**特性：**
- ✅ 每句台词开始时自动播放手势
- ✅ 随机选择手势A/B/C（避免连续重复）
- ✅ 可配置手势间最小间隔（防止过于频繁）
- ✅ 支持顺序播放模式（A→B→C→A...）
- ✅ 与对话系统解耦，易于维护

---

## 🏗️ 架构设计

```
DialogueTrigger (触发对话)
    ↓
DialogueManager (管理对话流程)
    ├─ 每行台词开始时通知 NPCAnimationController
    ↓
NPCAnimationController (控制NPC动画)
    ├─ 随机选择手势A/B/C
    ├─ 防止连续重复
    └─ 控制播放频率
    ↓
Animator (播放具体动画)
    ├─ Trigger "GestureA"
    ├─ Trigger "GestureB"
    └─ Trigger "GestureC"
```

---

## 📦 文件清单

| 文件 | 作用 |
|------|------|
| **[NPCAnimationController.cs](Assets/DialogueandStory/NPCAnimationController.cs)** | NPC动画控制器 |
| **[DialogueManager.cs](Assets/DialogueandStory/DialogueManager.cs)** | 已修改，添加动画调用 |

---

## 🎮 使用步骤

### 第1步：配置Animator Controller

#### 1.1 创建Animator Controller

```
1. 在Project窗口右键 → Create → Animator Controller
2. 命名为：NPCAnimatorController
```

#### 1.2 设置动画状态

创建以下状态：
- **Idle** (默认状态)
- **GestureA** (手势动画A)
- **GestureB** (手势动画B)
- **GestureC** (手势动画C)

#### 1.3 设置Transitions（过渡）

```
Idle → GestureA (Trigger: GestureA, Has Exit Time: false, Fixed Duration: 0)
GestureA → Idle (Has Exit Time: true, Exit Time: 0.95, Fixed Duration: 0)

Idle → GestureB (Trigger: GestureB, Has Exit Time: false, Fixed Duration: 0)
GestureB → Idle (Has Exit Time: true, Exit Time: 0.95, Fixed Duration: 0)

Idle → GestureC (Trigger: GestureC, Has Exit Time: false, Fixed Duration: 0)
GestureC → Idle (Has Exit Time: true, Exit Time: 0.95, Fixed Duration: 0)
```

**关键参数说明：**
- **Trigger**: 触发器名称（必须与代码中的参数名一致）
- **Has Exit Time (false)**: 手势之间没有过渡，立即触发
- **Has Exit Time (true)**: 动画播放完成后自动回Idle
- **Exit Time: 0.95**: 动画播放到95%时开始过渡
- **Fixed Duration: 0**: 瞬间过渡，无混合时间

#### 1.4 添加Trigger参数

在Animator的Parameters面板中添加：

| 参数名 | 类型 | 说明 |
|--------|------|------|
| GestureA | Trigger | 触发手势A |
| GestureB | Trigger | 触发手势B |
| GestureC | Trigger | 触发手势C |

### 第2步：在NPC上添加组件

选中你的NPC对象：

```
NPC_TestChildren
├─ Animator
│  └─ Controller: NPCAnimatorController ← 拖入上面创建的Controller
│
├─ NPC Animation Controller ← 添加这个脚本
│  ├─ Animator: [自动获取]
│  ├─ Gesture A Trigger: "GestureA"
│  ├─ Gesture B Trigger: "GestureB"
│  ├─ Gesture C Trigger: "GestureC"
│  ├─ Play Gesture On Dialogue: ✓
│  ├─ Min Delay Between Gestures: 0.5
│  └─ Random Gesture: ✓
│
└─ Dialogue Trigger
   └─ [其他配置...]
```

### 第3步：配置NPCAnimationController

在Inspector中配置：

| 字段 | 说明 | 推荐值 |
|------|------|--------|
| **Animator** | NPC的Animator组件 | 自动获取 |
| **Gesture A/B/C Trigger** | 对应的Trigger参数名 | "GestureA/B/C" |
| **Play Gesture On Dialogue** | 对话时是否播放手势 | ✓ 勾选 |
| **Min Delay Between Gestures** | 手势间最小间隔（秒） | 0.3-0.5 |
| **Random Gesture** | 是否随机播放（false则顺序播放） | ✓ 勾选 |
| **Show Debug Log** | 是否显示调试日志 | 测试时勾选 |

### 第4步：测试

1. 运行游戏
2. 走近NPC触发对话
3. 观察NPC动画：
   - ✅ 每句台词开始时播放随机手势
   - ✅ 手势播放完自动回Idle
   - ✅ 不会连续播放相同手势

---

## ⚙️ 高级配置

### 配置1：调整手势播放频率

如果觉得手势播放太频繁，可以调整：

```
NPCAnimationController
├─ Min Delay Between Gestures: 1.0  ← 增加到1秒
```

### 配置2：顺序播放手势（而非随机）

如果想让手势按固定顺序播放（A→B→C→A...）：

```
NPCAnimationController
├─ Random Gesture: ☐ 不勾选
```

### 配置3：为不同NPC设置不同行为

**NPC 1: 孩子们（活泼，手势多）**
```
NPCAnimationController
├─ Min Delay Between Gestures: 0.3  ← 短间隔，手势频繁
└─ Random Gesture: ✓
```

**NPC 2: 库拉奶奶（稳重，手势少）**
```
NPCAnimationController
├─ Min Delay Between Gestures: 2.0  ← 长间隔，手势少
└─ Random Gesture: ☐  ← 顺序播放，更规律
```

**NPC 3: 村长（不播放手势）**
```
NPCAnimationController
└─ Play Gesture On Dialogue: ☐  ← 禁用手势
```

---

## 🎨 动画时长建议

根据你的对话系统时序，建议手势动画时长：

```
打字机速度: 0.05秒/字符
平均台词长度: 10字符
打字机时长: 0.5秒

建议手势动画时长: 0.8 - 1.5秒
```

**时间线：**
```
0.0s - 台词开始，触发手势A
0.5s - 打字机完成
1.3s - 手势A结束（动画时长1.3秒）
??.?s - 玩家点击鼠标
??.?s - 显示下一句，触发手势B
```

**如果手势动画时长>玩家阅读速度：**
- 手势会被打断（由Animator自动处理）
- 不会影响对话流程

---

## 🔍 调试技巧

### 开启调试日志

```
NPCAnimationController
└─ Show Debug Log: ✓
```

Console输出：
```
NPC_TestChildren 播放手势动画: GestureA
NPC_TestChildren 播放手势动画: GestureC
NPC_TestChildren 播放手势动画: GestureB
```

### 测试动画过渡

在Animator窗口中：
1. 点击参数旁的圆圈图标手动触发Trigger
2. 观察状态机是否正确切换
3. 检查动画是否流畅过渡

---

## 🐛 常见问题

### 问题1：手势不播放

**检查：**
- ✅ NPC是否有Animator组件？
- ✅ Animator Controller是否正确设置？
- ✅ Trigger参数名是否与代码一致（GestureA/B/C）？
- ✅ Play Gesture On Dialogue是否勾选？

### 问题2：手势被立即打断

**原因：** Min Delay Between Gestures太短

**解决：** 增加到0.5或1.0秒

### 问题3：手势播放完成后不回Idle

**检查Animator设置：**
- GestureA → Idle 的Transition是否设置了 **Has Exit Time: true**？
- Exit Time是否在0.9-0.95之间？

### 问题4：连续播放相同手势

**原因：** Random Gesture未勾选且只设置了1个手势

**解决：**
- 确保至少有2个手势
- 勾选 Random Gesture

---

## 📊 性能考虑

### Animator数量
- 每个NPC一个Animator → ✅ 没问题
- 确保Animator Controller不要过于复杂

### Trigger调用频率
- 对话期间每句台词调用一次
- 正常对话5-10句 = 5-10次Trigger调用
- ✅ 性能影响可忽略

---

## 🎯 最佳实践

### 1. 动画准备
- 手势动画简洁（0.8-1.5秒）
- 从Idle开始，到Idle结束
- 避免复杂过渡

### 2. 参数命名
- 代码中使用："GestureA", "GestureB", "GestureC"
- Animator中也使用相同名称
- 保持一致性

### 3. 调试流程
```
1. 先在Animator窗口手动测试动画
2. 开启Show Debug Log查看Trigger调用
3. 进入游戏实际测试
4. 根据效果调整参数
```

---

## 💡 扩展可能性

### 未来可以添加的功能

1. **根据对话内容播放特定手势**
   ```csharp
   // 在dialogueString中添加
   public string requiredGesture = "A";  // 这句话必须播放手势A
   ```

2. **表情动画**
   ```
   除了手势，还可以切换NPC的面部表情
   ```

3. **对话结束动画**
   ```
   NPC_AnimationController.PlayGoodbyeAnimation()
   ```

4. **待机动画循环**
   ```
   NPC在非对话状态播放不同的待机动画
   ```

---

## ✅ 总结

**使用流程：**
1. ✅ 配置Animator Controller（Idle + 3个手势）
2. ✅ 在NPC上添加NPCAnimationController组件
3. ✅ 设置Trigger参数名
4. ✅ 调整播放频率和模式
5. ✅ 测试运行

**优点：**
- ✅ 解耦设计，易于维护
- ✅ 灵活配置，每个NPC独立控制
- ✅ 自动化，无需手动调用
- ✅ 防止重复和过度频繁

**下一步：**
为每个NPC添加此组件，根据角色性格调整手势播放频率！
