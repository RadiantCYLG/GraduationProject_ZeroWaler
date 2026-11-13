namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 玩家状态的基类，缓存控制器与状态机的引用，并提供空实现方便子类按需重写。
    /// </summary>
    public abstract class PlayerState : IState
    {
        protected readonly PlayerController Player;
        protected readonly StateMachine Machine;

        protected PlayerState(PlayerController player, StateMachine machine)
        {
            Player = player;
            Machine = machine;
        }

        public virtual void Enter() { }

        public virtual void Tick() { }

        public virtual void FixedTick() { }

        public virtual void Exit() { }
    }
}
