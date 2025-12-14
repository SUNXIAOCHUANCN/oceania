# 本次更新内容：
1、增加直接文本显示的功能，可以用于触发剧情

2、修复了对话过程中人物会移动的bug

3、添加了中文字体（微软雅黑，见Fonts文件夹），但是会有一些字体缺失的问题

4、增加了一些台词和剧情

# 现有功能
1、走向右边的雕像会触发对话

2、走向左边的雕像会触发剧情

# 使用方法
1、player需要挂载`Assets/player/DialogueManager.cs`和`TextManager.cs`

2、如果添加新的NPC或剧情点，需要创建一个Cube作为碰撞体，勾选isTrigger，挂载`Assets/player/DialogueTrigger.cs`或`TextTrigger.cs`

3、把碰撞体包裹住NPC或剧情点，然后隐藏线

4、在碰撞体的相应string下添加台词或者剧情

（特别说明，DialogueString的Index从0开始标号，设置跳转位置时请注意）
