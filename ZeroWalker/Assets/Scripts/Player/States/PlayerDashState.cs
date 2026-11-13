using UnityEngine;

namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 冲刺状态：朝当前朝向前进固定距离。
    /// 没有专用的冲刺动画，使用 Move 动画表现快速前冲。
    /// 冲刺期间锁定方向与输入，由 PlayerController 负责 1 秒冷却。
    /// </summary>
    public class PlayerDashState : PlayerState
    {
        private float _elapsed;

        public PlayerDashState(PlayerController player, StateMachine machine) : base(player, machine) { }

        public override void Enter()
        {
            Player.Animation.Play(PlayerAnimations.Move, true);
            Player.BeginDash();
            _elapsed = 0f;
        }

        public override void Tick()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < Player.DashDuration) return;

            var hasMoveInput = Mathf.Abs(Player.Inputs.MoveAxis) > 0.01f;
            Machine.ChangeState(hasMoveInput ? Player.MoveState : Player.IdleState);
        }

        public override void FixedTick()
        {
            // 每物理帧保持冲刺速度，避免被上一状态的速度/阻力影响
            Player.ApplyDashVelocity();
        }

        public override void Exit()
        {
            Player.StopHorizontalMovement();
        }
    }
}
