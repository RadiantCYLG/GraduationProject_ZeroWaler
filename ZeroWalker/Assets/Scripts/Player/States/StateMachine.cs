namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 轻量级有限状态机。
    /// 只负责持有当前状态、转发生命周期调用，以及安全地切换状态，
    /// 具体的切换条件由各个状态自己判断（状态自治，便于扩展）。
    /// </summary>
    public class StateMachine
    {
        /// <summary>当前状态。</summary>
        public IState Current { get; private set; }

        /// <summary>上一个状态（可用于“返回上一状态”之类逻辑）。</summary>
        public IState Previous { get; private set; }

        /// <summary>
        /// 切换到新状态。重复切换到同一个状态会被忽略，
        /// 需要强制重播时显式指定 <paramref name="force"/>。
        /// </summary>
        public void ChangeState(IState next, bool force = false)
        {
            if (next == null) return;
            if (!force && ReferenceEquals(next, Current)) return;

            Current?.Exit();
            Previous = Current;
            Current = next;
            Current.Enter();
        }

        public void Tick()
        {
            Current?.Tick();
        }

        public void FixedTick()
        {
            Current?.FixedTick();
        }
    }
}
