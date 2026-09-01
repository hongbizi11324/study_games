

## Codely Structured Memories

### User
- [2026-08-31 09:58:33] 大四计算机专业，目标岗位游戏客户端开发，正在准备秋招。有 C++ 背景，但 Unity/团结引擎/C# 语言机制几乎零基础（会写游戏逻辑架构，如泛型FSM、EventBus、对象池、UGUI优化）。正在用造梦西游素材在团结引擎1.9.3里复刻2D demo，边做边学Unity/C#，目标是做出能写进简历、能在面试讲深的作品。
### Feedback
- [2026-08-31 09:58:33] 用户要求：在没明确要求之前，不要对编辑器进行操作，只需告诉详细的每一步点击和对应原理，用户自己学习操作。

### Project
- [2026-08-31 09:58:33] 造梦西游复刻项目，采用四层数据驱动架构（表现层View/逻辑层Gameplay/数据层ScriptableObject/服务层对象池事件）。禁止GameObject.Find、全局单例通信、Update内GetComponent、魔法字符串、Update内GC分配。里程碑M0→M5，当前在M0阶段。素材在 E:\造梦资源\ 旗下各子目录。**Why:** 面试加分点和反模式红线。**How to apply:** 所有代码建议都需遵守这些架构规范。

### Reference

