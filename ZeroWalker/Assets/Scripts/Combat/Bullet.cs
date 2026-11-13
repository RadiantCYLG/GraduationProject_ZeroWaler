using UnityEngine;
using ZeroWalker.VFX;

namespace ZeroWalker.Combat
{
    /// <summary>
    /// 直线飞行的子弹。
    /// 使用 Trigger 触发器 + 连续碰撞检测，命中目标或场景后立即销毁。
    /// 支持两种外观：普通子弹（保持原画颜色）与特殊子弹（OnAttack 事件射出，换用白色弹体并染成高亮色）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [Header("参数")]
        [SerializeField] private float speed = 22f;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private int damage = 1;

        [Header("外观")]
        [Tooltip("普通子弹的着色。白色表示沿用精灵原本的橙色")]
        [SerializeField] private Color normalColor = Color.white;
        [Tooltip("特殊子弹（Attack_Loop 的 OnAttack 事件射出）的着色")]
        [SerializeField] private Color specialColor = new Color(0.35f, 0.95f, 1f, 1f);
        [Tooltip("特殊子弹使用的白色弹体贴图，配合 specialColor 染成高亮颜色")]
        [SerializeField] private Sprite specialSprite;
        [Tooltip("特殊子弹的尺寸倍率，让它在画面中更容易和普通子弹区分")]
        [SerializeField] private float specialScale = 1.2f;

        [Header("命中特效")]
        [Tooltip("命中目标时在命中点生成的特效预制体（播放结束后自行销毁）")]
        [SerializeField] private HitEffect hitEffectPrefab;

        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private Sprite _normalSprite;
        private Vector3 _baseScale;
        private Vector2 _direction;
        private bool _launched;

        /// <summary>本发子弹实际使用的着色。</summary>
        public Color ShotColor { get; private set; } = Color.white;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();

            if (_renderer != null)
                _normalSprite = _renderer.sprite;

            _baseScale = transform.localScale;
        }

        /// <summary>发射子弹。special 为 true 时使用特殊颜色/弹体。</summary>
        public void Launch(Vector2 direction, bool special = false, float speedOverride = -1f)
        {
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;

            _direction = direction.normalized;
            _launched = true;

            if (speedOverride > 0f) speed = speedOverride;

            ApplyVisual(special);

            // 绕 Z 轴旋转朝向（2D 中不要用 transform.right，会绕 Y 轴翻转）
            var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            _body.velocity = _direction * speed;

            Destroy(gameObject, lifeTime);
        }

        private void ApplyVisual(bool special)
        {
            ShotColor = special ? specialColor : normalColor;

            if (_renderer == null) return;

            // 特殊子弹换用白色弹体贴图，染色后颜色才干净（橙色贴图直接染蓝会发绿）
            _renderer.sprite = special && specialSprite != null ? specialSprite : _normalSprite;
            _renderer.color = ShotColor;

            var scale = special ? specialScale : 1f;
            transform.localScale = new Vector3(_baseScale.x * scale, _baseScale.y * scale, _baseScale.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_launched) return;

            // 忽略玩家自身（子弹出生点在角色身上）
            if (other.GetComponentInParent<Player.PlayerController>() != null) return;

            var target = other.GetComponentInParent<ShootableTarget>();
            if (target != null)
            {
                // 先取命中点再结算伤害：目标死亡时碰撞体会被禁用，避免取不到正确位置
                SpawnHitEffect(other);
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }

        /// <summary>在命中点生成击中特效（只对可击中的目标生效）。</summary>
        private void SpawnHitEffect(Collider2D other)
        {
            if (hitEffectPrefab == null) return;

            var point = other.ClosestPoint(transform.position);
            var effect = Instantiate(hitEffectPrefab, new Vector3(point.x, point.y, 0f), Quaternion.identity);

            // 特效颜色跟随子弹，保证命中特效与射出的子弹视觉一致
            effect.SetTint(ShotColor);
        }
    }
}
