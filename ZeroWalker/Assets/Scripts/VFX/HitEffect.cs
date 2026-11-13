using UnityEngine;

namespace ZeroWalker.VFX
{
    /// <summary>
    /// 击中特效：播放一次精灵闪光，视觉效果结束后销毁自身实例。
    /// 由 Bullet 在命中 ShootableTarget 时实例化到命中点。
    /// </summary>
    [RequireComponent(typeof(SpriteFlash))]
    public class HitEffect : MonoBehaviour
    {
        [SerializeField] private SpriteFlash flash;

        [Tooltip("特效着色。Bullet 生成时会用自身子弹颜色覆盖，保持与子弹一致")]
        [SerializeField] private Color tint = new Color(0.35f, 0.95f, 1f, 1f);

        [Tooltip("闪光结束后的额外销毁延迟，确保视觉完全结束")]
        [SerializeField] private float destroyDelay = 0.02f;

        private void Awake()
        {
            if (flash == null)
                flash = GetComponent<SpriteFlash>();
        }

        /// <summary>由子弹在生成时调用，让特效颜色与射出它的子弹保持一致。</summary>
        public void SetTint(Color color)
        {
            tint = color;
        }

        private void Start()
        {
            if (flash == null)
            {
                Destroy(gameObject);
                return;
            }

            flash.Play(tint);
            Destroy(gameObject, flash.Duration + destroyDelay);
        }
    }
}
