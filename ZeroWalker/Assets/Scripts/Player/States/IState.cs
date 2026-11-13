namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 状态机中一个状态的最小接口。
    /// 生命周期：Enter -> (Tick / FixedTick 循环) -> Exit
    /// </summary>
    public interface IState
    {
        /// <summary>进入状态时调用一次。</summary>
        void Enter();

        /// <summary>每帧调用（对应 Update）。</summary>
        void Tick();

        /// <summary>每个物理帧调用（对应 FixedUpdate）。</summary>
        void FixedTick();

        /// <summary>离开状态时调用一次。</summary>
        void Exit();
    }
}
