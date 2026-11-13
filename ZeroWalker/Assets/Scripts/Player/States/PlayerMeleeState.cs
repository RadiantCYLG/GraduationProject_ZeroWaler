using UnityEngine;

namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 近战攻击状态。
    /// 播放 Spine 的 Skill_02 动画（单次），动画结束后回到 Idle / Move。
    /// 近战期间角色原地不动、朝向锁定，不响应移动、跳跃、冲刺和射击输入。
    /// </summary>
    public class PlayerMeleeState : PlayerState
    {
        private float _elapsed;
        private float _duration;

        public PlayerMeleeState(PlayerController player, StateMachine machine) : base(player, machine) { }

        public override void Enter()
        {
            Player.StopHorizontalMovement();
            Player.Animation.Play(PlayerAnimations.Skill02, false);
            _duration = Mathf.Max(Player.Animation.GetDuration(PlayerAnimations.Skill02), 0.1f);
            _elapsed = 0f;
        }

        public override void Tick()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < _duration) return;

            var hasMoveInput = Mathf.Abs(Player.Inputs.MoveAxis) > 0.01f;
            Machine.ChangeState(hasMoveInput ? Player.MoveState : Player.IdleState);
        }

        public override void FixedTick()
        {
            // 近战攻击期间保持原地
            Player.StopHorizontalMovement();
        }
    }
}
