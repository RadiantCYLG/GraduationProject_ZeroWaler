using UnityEngine;

namespace ZeroWalker.Player
{
    /// <summary>
    /// 玩家输入的统一读取入口。
    /// 当前使用旧版 Input Manager：A/D 左右移动、空格跳跃、左 Shift 冲刺、F 近战、鼠标左键射击。
    /// 同时提供“调试输入”，便于在编辑器里无人值守地验证状态机与射击/跳跃/冲刺/近战流程。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("按键设置")]
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode meleeKey = KeyCode.F;
        [SerializeField] private int attackMouseButton = 0;

        [Header("调试输入（覆盖真实按键）")]
        [SerializeField] private bool useOverride;
        [SerializeField, Range(-1f, 1f)] private float overrideMoveAxis;
        [SerializeField] private bool overrideAttackHeld;
        [SerializeField] private bool overrideJumpHeld;
        [SerializeField] private bool overrideDashHeld;
        [SerializeField] private bool overrideMeleeHeld;

        private bool _attackPressed;
        private bool _jumpPressed;
        private bool _jumpHeld;
        private bool _dashPressed;
        private bool _dashHeld;
        private bool _meleePressed;
        private bool _meleeHeld;

        /// <summary>水平移动轴：-1 左、0 静止、1 右。</summary>
        public float MoveAxis { get; private set; }

        /// <summary>射击键是否处于按住状态。</summary>
        public bool AttackHeld { get; private set; }

        /// <summary>射击键是否在本帧被按下。</summary>
        public bool AttackPressed => _attackPressed;

        /// <summary>跳跃键是否在本帧被按下（边沿触发，长按不会连跳）。</summary>
        public bool JumpPressed => _jumpPressed;

        /// <summary>冲刺键是否在本帧被按下（边沿触发，长按不会连续冲刺）。</summary>
        public bool DashPressed => _dashPressed;

        /// <summary>近战键（F）是否在本帧被按下（边沿触发）。</summary>
        public bool MeleePressed => _meleePressed;

        /// <summary>由 PlayerController 在每帧开始时调用，保证同一帧内读取到一致的值。</summary>
        public void Poll()
        {
            var previousHeld = AttackHeld;
            var previousJumpHeld = _jumpHeld;
            var previousDashHeld = _dashHeld;
            var previousMeleeHeld = _meleeHeld;

            if (useOverride)
            {
                MoveAxis = Mathf.Clamp(overrideMoveAxis, -1f, 1f);
                AttackHeld = overrideAttackHeld;
                _attackPressed = AttackHeld && !previousHeld;

                _jumpHeld = overrideJumpHeld;
                _jumpPressed = _jumpHeld && !previousJumpHeld;

                _dashHeld = overrideDashHeld;
                _dashPressed = _dashHeld && !previousDashHeld;

                _meleeHeld = overrideMeleeHeld;
                _meleePressed = _meleeHeld && !previousMeleeHeld;
                return;
            }

            var left = Input.GetKey(moveLeftKey) ? 1f : 0f;
            var right = Input.GetKey(moveRightKey) ? 1f : 0f;
            MoveAxis = right - left;

            AttackHeld = Input.GetMouseButton(attackMouseButton);
            _attackPressed = Input.GetMouseButtonDown(attackMouseButton);

            _jumpHeld = Input.GetKey(jumpKey);
            _jumpPressed = Input.GetKeyDown(jumpKey);

            _dashHeld = Input.GetKey(dashKey);
            _dashPressed = Input.GetKeyDown(dashKey);

            _meleeHeld = Input.GetKey(meleeKey);
            _meleePressed = Input.GetKeyDown(meleeKey);
        }

        /// <summary>供自动化验证/演示使用的输入覆盖。</summary>
        public void SetOverride(
            bool enable,
            float moveAxis = 0f,
            bool attackHeld = false,
            bool jumpHeld = false,
            bool dashHeld = false,
            bool meleeHeld = false)
        {
            useOverride = enable;
            overrideMoveAxis = Mathf.Clamp(moveAxis, -1f, 1f);
            overrideAttackHeld = attackHeld;
            overrideJumpHeld = jumpHeld;
            overrideDashHeld = dashHeld;
            overrideMeleeHeld = meleeHeld;
        }
    }
}
