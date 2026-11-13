using UnityEngine;
using ZeroWalker.Combat;
using ZeroWalker.Player.States;

namespace ZeroWalker.Player
{
    /// <summary>
    /// 玩家角色控制器（Blitz）。
    /// 负责：读取输入 -> 驱动有限状态机 -> 播放 Spine 动画 -> 移动/转向/跳跃 -> 开枪。
    /// 具体的状态行为在 States 目录下，每个状态自治地决定何时切换。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("射击")]
        [Tooltip("子弹预制体（按住左键时，Attack_Loop 每次 OnAttack 事件出一发）")]
        [SerializeField] private Bullet bulletPrefab;

        [Header("跳跃")]
        [Tooltip("起跳初速度（米/秒）")]
        [SerializeField] private float jumpForce = 9f;
        [Tooltip("脚下落地检测半径")]
        [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.12f;

        [Header("冲刺")]
        [Tooltip("一次冲刺前进的距离（米）")]
        [SerializeField] private float dashDistance = 4f;
        [Tooltip("一次冲刺的持续时间（秒）")]
        [SerializeField] private float dashDuration = 0.2f;
        [Tooltip("两次冲刺之间的冷却（秒，从冲刺开始时计算）")]
        [SerializeField] private float dashCooldown = 1f;

        [Header("引用（留空会自动查找）")]
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpineAnimationPlayer animationPlayer;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private MuzzlePoint muzzle;

        // ---- 状态机 ----
        private readonly StateMachine _machine = new StateMachine();

        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerDashState DashState { get; private set; }
        public PlayerMeleeState MeleeState { get; private set; }

        private CapsuleCollider2D _capsule;
        private readonly Collider2D[] _groundHits = new Collider2D[8];
        private float _dashCooldownRemaining;

        // ---- 对外只读数据 ----
        public PlayerInputReader Inputs => inputReader;
        public SpineAnimationPlayer Animation => animationPlayer;
        public Rigidbody2D Body => body;
        public Transform VisualRoot => visualRoot;
        public MuzzlePoint Muzzle => muzzle;
        public StateMachine Machine => _machine;

        /// <summary>朝向：1 右，-1 左。</summary>
        public int Facing { get; private set; } = 1;

        /// <summary>本次运行累计射出的子弹数（便于调试、UI 或音效统计）。</summary>
        public int ShotCount { get; private set; }

        /// <summary>脚下是否踩在地面/平台上（不包含触发器）。</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>冲刺冷却是否已结束（1 秒内不能再次冲刺）。</summary>
        public bool CanDash => _dashCooldownRemaining <= 0f;

        /// <summary>单次冲刺的持续时间（秒）。</summary>
        public float DashDuration => dashDuration;

        /// <summary>冲刺时的水平速度（米/秒），由距离/时长换算。</summary>
        public float DashSpeed => dashDistance / Mathf.Max(dashDuration, 0.01f);

        /// <summary>剩余冲刺冷却时间（秒）。</summary>
        public float DashCooldownRemaining => _dashCooldownRemaining;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
            if (animationPlayer == null) animationPlayer = GetComponentInChildren<SpineAnimationPlayer>();
            if (muzzle == null) muzzle = GetComponent<MuzzlePoint>();
            if (_capsule == null) _capsule = GetComponent<CapsuleCollider2D>();

            if (visualRoot == null && animationPlayer != null)
                visualRoot = animationPlayer.transform;

            IdleState = new PlayerIdleState(this, _machine);
            MoveState = new PlayerMoveState(this, _machine);
            AttackState = new PlayerAttackState(this, _machine);
            JumpState = new PlayerJumpState(this, _machine);
            DashState = new PlayerDashState(this, _machine);
            MeleeState = new PlayerMeleeState(this, _machine);
        }

        private void Start()
        {
            UpdateGrounded();
            _machine.ChangeState(IdleState, true);
        }

        private void Update()
        {
            // 冲刺冷却独立于状态机计时，保证任何状态下都按真实时间恢复
            if (_dashCooldownRemaining > 0f)
                _dashCooldownRemaining = Mathf.Max(0f, _dashCooldownRemaining - Time.deltaTime);

            inputReader.Poll();
            _machine.Tick();
        }

        private void FixedUpdate()
        {
            // 每帧先刷新落地状态，状态机内部（跳跃/落地）再使用它
            UpdateGrounded();
            _machine.FixedTick();
        }

        /// <summary>沿着水平方向移动。speedScale 用于不同状态下的速度倍率。</summary>
        public void MoveHorizontal(float axis, float speedScale)
        {
            var velocity = body.velocity;
            velocity.x = axis * moveSpeed * speedScale;
            body.velocity = velocity;
        }

        /// <summary>立刻停止水平移动（射击时角色保持原地不动）。</summary>
        public void StopHorizontalMovement()
        {
            var velocity = body.velocity;
            velocity.x = 0f;
            body.velocity = velocity;
        }

        /// <summary>起跳：给刚体一个向上的初速度（重力由 Rigidbody2D 的 Gravity Scale 提供）。</summary>
        public void Jump()
        {
            var velocity = body.velocity;
            velocity.y = jumpForce;
            body.velocity = velocity;
            IsGrounded = false;
        }

        /// <summary>开始冲刺：启动冷却，并按当前朝向施加冲刺速度。</summary>
        public void BeginDash()
        {
            _dashCooldownRemaining = dashCooldown;
            ApplyDashVelocity();
        }

        /// <summary>每物理帧保持冲刺水平速度（垂直速度不变）。</summary>
        public void ApplyDashVelocity()
        {
            var velocity = body.velocity;
            velocity.x = Facing * DashSpeed;
            body.velocity = velocity;
        }

        /// <summary>用脚底的小圆检测非触发器碰撞体，判断是否踩在地面/平台上。</summary>
        private void UpdateGrounded()
        {
            if (_capsule == null)
            {
                IsGrounded = false;
                return;
            }

            var bounds = _capsule.bounds;
            var checkPoint = new Vector2(transform.position.x, bounds.min.y);
            var count = Physics2D.OverlapCircleNonAlloc(checkPoint, groundCheckRadius, _groundHits);

            for (var i = 0; i < count; i++)
            {
                var hit = _groundHits[i];
                if (hit == null || hit == _capsule) continue;
                if (hit.transform.IsChildOf(transform)) continue;
                if (hit.isTrigger) continue;

                IsGrounded = true;
                return;
            }

            IsGrounded = false;
        }

        /// <summary>根据输入方向转身（翻转折纸的视觉根节点，不影响碰撞体）。</summary>
        public void FaceTo(float axis)
        {
            if (Mathf.Abs(axis) < 0.01f) return;

            var direction = axis > 0f ? 1 : -1;
            if (direction == Facing) return;

            Facing = direction;

            if (visualRoot == null) return;
            var scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            visualRoot.localScale = scale;
        }

        /// <summary>立刻在枪口生成一发普通子弹。</summary>
        public void Fire()
        {
            Fire(false);
        }

        /// <summary>
        /// 立刻在枪口生成一发特殊子弹（颜色/特效区别于普通子弹）。
        /// 由 Attack_Loop 的 OnAttack 动画事件调用；按住扳机时每次事件都会调用一次。
        /// </summary>
        public void FireSpecial()
        {
            Fire(true);
        }

        private void Fire(bool special)
        {
            if (bulletPrefab == null || muzzle == null) return;

            var direction = Facing >= 0 ? Vector3.right : Vector3.left;
            var bullet = Instantiate(bulletPrefab, muzzle.WorldPosition, Quaternion.identity);
            bullet.Launch(direction, special);
            ShotCount++;

            // 枪口火光跟随子弹颜色，让特殊射击在画面上更统一
            muzzle.PlayFlash(Facing, special ? bullet.ShotColor : Color.white);
        }
    }
}
