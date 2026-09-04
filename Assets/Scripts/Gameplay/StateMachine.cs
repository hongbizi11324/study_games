using System;
using System.Collections.Generic;

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// 泛型状态机：ChangeState 三重防呆（同态重复/未注册枚举/非法转移），
    /// 任何一步失败都停留在当前态并返回 false。
    /// </summary>
    public class StateMachine<TState> where TState : struct, Enum
    {
        private readonly Dictionary<TState, IState<TState>> states = new Dictionary<TState, IState<TState>>();
        private readonly Dictionary<TState, TState[]> transitionTable = new Dictionary<TState, TState[]>();

        public TState Current { get; private set; }
        public IState<TState> CurrentState { get; private set; }

        /// <summary>注册状态，同时声明它允许去往哪些状态（转移表）。</summary>
        public void Register(IState<TState> state, params TState[] allowedNext)
        {
            states[state.Id] = state;
            transitionTable[state.Id] = allowedNext;
        }

        public void Start(TState initial)
        {
            Current = initial;
            CurrentState = states[initial];
            CurrentState.Enter(default);
        }

        /// <summary>由宿主每帧驱动——状态机不是 MonoBehaviour，生命周期完全受控。</summary>
        public void Tick() => CurrentState.Tick();

        public bool CanTransitionTo(TState next)
        {
            return transitionTable.TryGetValue(Current, out var allowed)
                && Array.IndexOf(allowed, next) >= 0;
        }

        public bool ChangeState(TState next)
        {
            if (EqualityComparer<TState>.Default.Equals(next, Current)) return false;  // 防同态重复
            if (!states.TryGetValue(next, out IState<TState> target)) return false;    // 防未注册枚举
            if (!CanTransitionTo(next)) return false;                                   // 防非法转移

            TState previous = Current;
            CurrentState.Exit(next);
            Current = next;
            CurrentState = target;
            CurrentState.Enter(previous);
            return true;
        }
    }
}