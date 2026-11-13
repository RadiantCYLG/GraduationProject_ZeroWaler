using UnityEngine;

namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 移动：按住 A / D 时播放 Move 动画并水平移动。
    /// </summary>
    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerController player, StateMachine machine) : base(player, machine) { }

        public override void Enter()
        {
            Player.Animation.Play(PlayerAnimations.Move, true);
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

            if (Mathf.Abs(inputs.MoveAxis) < 0.01f)
            {
                Machine.ChangeState(Player.IdleState);
                return;
            }

            Player.FaceTo(inputs.MoveAxis);
        }

        public override void FixedTick()
        {
            Player.MoveHorizontal(Player.Inputs.MoveAxis, 1f);
        }
    }
}
