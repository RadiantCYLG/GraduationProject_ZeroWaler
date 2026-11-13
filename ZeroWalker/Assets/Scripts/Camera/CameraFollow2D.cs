using UnityEngine;

namespace ZeroWalker.CameraSystem
{
    /// <summary>
    /// 简易 2D 横板摄像机跟随，使用 SmoothDamp 平滑移动，可选锁定 Y 轴。
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.4f, -10f);
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private bool lockY;

        private Vector3 _velocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            if (target == null)
            {
                var player = FindObjectOfType<Player.PlayerController>();
                if (player != null) target = player.transform;
            }

            // 开局直接对齐，避免从原点滑过去
            if (target != null)
                transform.position = GetGoalPosition();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                GetGoalPosition(),
                ref _velocity,
                smoothTime);
        }

        private Vector3 GetGoalPosition()
        {
            var goal = target.position + offset;
            if (lockY) goal.y = transform.position.y;
            return goal;
        }
    }
}
