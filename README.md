# 造梦西游复刻（ZaoMengxiyou）

基于 Unity 开发的横版动作游戏，复刻《造梦西游》的核心玩法。

## 项目状态

开发中，当前已实现：
- 玩家角色（孙悟空）基础移动、奔跑、跳跃
- 角色 + 武器分层精灵渲染（序列帧动画）
- 基础攻击动画状态机

## 技术栈

- **引擎**：Unity
- **语言**：C#
- **渲染**：2D Sprite（序列帧动画）
- **物理**：Unity Physics 2D

## 目录结构

```
Assets/
├── Animations/        # 动画剪辑和 Animator Controller
├── Editor/            # 编辑器扩展工具（精灵切片复制器等）
├── Materials/         # 材质
├── Prefabs/           # 预制体
├── Scenes/            # 场景
├── ScriptableObjects/ # 数据配置
├── Scripts/           # 游戏脚本
│   ├── Gameplay/      #  gameplay 逻辑
│   └── Services/      # 服务层
└── Sprites/           # 精灵资源
```

## 开发说明

- 角色和武器使用分离的精灵图，通过统一 Pivot 对齐
- 使用 `Tools/复制 Sprite 切片数据` 菜单工具同步角色与武器的精灵切片
- 输入：A/D 移动，Shift 奔跑，J 攻击，K 跳跃
