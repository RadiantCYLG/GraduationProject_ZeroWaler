using UnityEngine;

namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 跳跃状态。
    /// 目前没有专用的跳跃动画，按需求统一使用 Idle 动画代替。
    /// 负责起跳、空中的水平操控与落地切换；空中不响应射击输入。
    /// </summary>
    public class PlayerJumpState : PlayerState
    {
        /// <summary>起跳后的最小滞空时间，避免刚离地时被误判为落地。</summary>
        private const float MinAirTime = 0.05f;

        private float _airTime;

        public PlayerJumpState(PlayerController player, StateMachine machine) : base(player, machine) { }

        public override void Enter()
        {
            // 用 Idle 动画代替跳跃动画
            Player.Animation.Play(PlayerAnimations.Idle, true);
            Player.Jump();
            _airTime = 0f;
        }

        public override void Tick()
        {
            // 空中允许转身；攻击/移动状态切换留到落地后
            Player.FaceTo(Player.Inputs.MoveAxis);
        }

        public override void FixedTick()
        {
            _airTime += Time.fixedDeltaTime;

            // 空中保留水平操控，垂直速度由起跳初速度和重力决定
            Player.MoveHorizontal(Player.Inputs.MoveAxis, 1f);

            if (_airTime < MinAirTime) return;
            if (!Player.IsGrounded) return;
            if (Player.Body.velocity.y > 0.01f) return;

            var hasMoveInput = Mathf.Abs(Player.Inputs.MoveAxis) > 0.01f;
            Machine.ChangeState(hasMoveInput ? Player.MoveState : Player.IdleState);
        }
    }
}
