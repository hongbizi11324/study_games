using System;

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// 状态接口：每个具体状态实现生命周期三件套。
    /// TState 限定为枚举——状态名编译期确定，从类型系统层面消灭魔法字符串。
    /// </summary>
    public interface IState<TState> where TState : struct, Enum
    {
        TState Id { get; }
        void Enter(TState previous);   // 进入：播动画、置标志
        void Tick();                   // 每帧：该状态下的行为逻辑
        void Exit(TState next);        // 离开：清理
    }
}