using UnityEngine;

namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 待机：没有移动输入、也没有开火时播放 Idle 循环动画。
    /// </summary>
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerController player, StateMachine machine) : base(player, machine) { }

        public override void Enter()
        {
            Player.Animation.Play(PlayerAnimations.Idle, true);
        }

        public override void Tick()
        {
            var inputs = Player.Inputs;

            if (inputs.DashPressed && Player.CanDash)
            {
                Machine.ChangeState(Player.DashState);
                return;
            }

            if (inputs.JumpPressed && Player.IsGrounded)
            {
                Machine.ChangeState(Player.JumpState);
                return;
            }

            if (inputs.MeleePressed)
            {
                Machine.ChangeState(Player.MeleeState);
                return;
            }

            if (inputs.AttackPressed)
            {
                Machine.ChangeState(Player.AttackState);
                return;
            }

            if (Mathf.Abs(inputs.MoveAxis) > 0.01f)
                Machine.ChangeState(Player.MoveState);
        }

        public override void FixedTick()
        {
            Player.MoveHorizontal(0f, 1f);
        }
    }
}
