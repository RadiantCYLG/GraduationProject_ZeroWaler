using UnityEngine;

namespace ZeroWalker.Player.States
{
    /// <summary>
    /// 攻击/射击状态（按住连射，射速由动画事件决定）。
    /// 动画流程：Attack_Begin（抬枪）→ Attack_Loop（循环）→ 松开扳机后 Attack_End（收枪）。
    /// 按住扳机时只在 Attack_Loop 的 OnAttack 事件处出弹：每个循环射出一次，间隔等于动画循环时长（约 1.2 秒），
    /// 不会按帧或无间隔开火。松开扳机后收枪；射击期间角色保持原地不动，只允许原地转身。
    /// </summary>
    public class PlayerAttackState : PlayerState
    {
        private float _finishTimer;
        private bool _hasFired;
        private bool _finishing;
        private bool _subscribed;

        public PlayerAttackState(PlayerController player, StateMachine machine) : base(player, machine) { }

        public override void Enter()
        {
            if (!_subscribed)
            {
                Player.Animation.SpineEvent += OnSpineEvent;
                _subscribed = true;
            }

            Player.StopHorizontalMovement();

            Player.Animation.Play(PlayerAnimations.AttackBegin, false);
            Player.Animation.Queue(PlayerAnimations.AttackLoop, true);

            _finishTimer = 0f;
            _hasFired = false;
            _finishing = false;
        }

        public override void Exit()
        {
            if (_subscribed)
            {
                Player.Animation.SpineEvent -= OnSpineEvent;
                _subscribed = false;
            }
        }

        public override void Tick()
        {
            var inputs = Player.Inputs;
            var deltaTime = Time.deltaTime;

            // 射击过程中也允许转身，子弹跟着枪口方向走（但不产生位移）
            Player.FaceTo(inputs.MoveAxis);

            if (_finishing)
            {
                // 收枪期间再次按下扳机：重新进入攻击，保证连点手感
                if (inputs.AttackPressed)
                {
                    Machine.ChangeState(Player.AttackState, true);
                    return;
                }

                _finishTimer -= deltaTime;
                if (_finishTimer <= 0f)
                    ExitToLocomotion();
                return;
            }

            // 松开扳机：已经出过弹就收枪；若还没出过弹（极短点击），等第一次 OnAttack 出弹后再收枪
            if (_hasFired && !inputs.AttackHeld)
                StartFinishing();
        }

        public override void FixedTick()
        {
            // 射击时静止：每帧把水平速度清零，避免惯性滑动
            Player.StopHorizontalMovement();
        }

        /// <summary>
        /// Spine 动画事件：Attack_Loop 每循环一次都会触发 OnAttack，每次触发都射出一发。
        /// 射速因此由动画事件间隔决定，而不是固定时间间隔或每帧开火。
        /// </summary>
        private void OnSpineEvent(string animationName, string eventName)
        {
            if (_finishing) return;
            if (animationName != PlayerAnimations.AttackLoop) return;
            if (eventName != PlayerAnimations.AttackEvent) return;

            _hasFired = true;
            Player.FireSpecial();
        }

        private void StartFinishing()
        {
            _finishing = true;
            Player.Animation.Play(PlayerAnimations.AttackEnd, false);
            _finishTimer = Mathf.Max(Player.Animation.GetDuration(PlayerAnimations.AttackEnd), 0.05f);
        }

        private void ExitToLocomotion()
        {
            var hasMoveInput = Mathf.Abs(Player.Inputs.MoveAxis) > 0.01f;
            Machine.ChangeState(hasMoveInput ? Player.MoveState : Player.IdleState);
        }
    }
}
