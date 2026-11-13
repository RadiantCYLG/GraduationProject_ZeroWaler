using UnityEngine;

namespace ZeroWalker.Combat
{
    /// <summary>
    /// 可被击中的标靶，用来验证子弹命中与伤害流程。
    /// 受击时闪白并缩放，血量归零后销毁；开启复活时会在原地重新出现。
    /// </summary>
    public class ShootableTarget : MonoBehaviour
    {
        [Header("生命值")]
        [SerializeField] private int maxHealth = 3;

        [Header("受击表现")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private float hitFlashDuration = 0.08f;

        [Header("死亡")]
        [SerializeField] private bool respawn = true;
        [SerializeField] private float respawnDelay = 2f;

        private Color _baseColor;
        private int _health;
        private float _flashTimer;
        private float _respawnTimer;
        private Collider2D[] _colliders;
        private bool _dead;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _colliders = GetComponentsInChildren<Collider2D>();
            _baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            _health = maxHealth;
        }

        private void Update()
        {
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && spriteRenderer != null)
                    spriteRenderer.color = _baseColor;
            }

            if (_dead && respawn)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f) Revive();
            }
        }

        /// <summary>受到伤害。</summary>
        public void TakeDamage(int amount)
        {
            if (_dead) return;

            _health -= amount;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = hitColor;
                _flashTimer = hitFlashDuration;
            }

            if (_health <= 0) Die();
        }

        private void Die()
        {
            _dead = true;
            _respawnTimer = respawnDelay;

            transform.localScale = Vector3.one * 0.6f;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            foreach (var collider in _colliders) collider.enabled = false;

            if (!respawn) Destroy(gameObject);
        }

        private void Revive()
        {
            _dead = false;
            _health = maxHealth;
            _flashTimer = 0f;

            transform.localScale = Vector3.one;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _baseColor;
                spriteRenderer.enabled = true;
            }

            foreach (var collider in _colliders) collider.enabled = true;
        }
    }
}
