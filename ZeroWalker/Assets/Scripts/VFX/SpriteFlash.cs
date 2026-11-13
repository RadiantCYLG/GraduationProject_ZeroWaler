using UnityEngine;

namespace ZeroWalker.VFX
{
    /// <summary>
    /// 通用的一次性精灵闪光（枪口火光、命中火花等）。
    /// 播放时会放大并淡出，结束后自动隐藏。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private float duration = 0.06f;
        [SerializeField] private float startScale = 0.9f;
        [SerializeField] private float endScale = 1.35f;

        private float _elapsed;
        private Color _baseColor;
        private Color _playColor;

        /// <summary>是否正在播放。</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>一次闪光的总时长（秒）。</summary>
        public float Duration => duration;

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            _baseColor = target.color;
            _playColor = _baseColor;
            Hide();
        }

        /// <summary>从头播放一次闪光（保持原色）。</summary>
        public void Play()
        {
            Play(Color.white);
        }

        /// <summary>从头播放一次闪光，tint 会与自身颜色相乘（白色表示不改变原色）。</summary>
        public void Play(Color tint)
        {
            _playColor = _baseColor * tint;
            _playColor.a = _baseColor.a;

            _elapsed = 0f;
            IsPlaying = true;
            gameObject.SetActive(true);

            target.enabled = true;
            target.color = _playColor;
            transform.localScale = Vector3.one * startScale;
        }

        private void Update()
        {
            if (!IsPlaying) return;

            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / Mathf.Max(duration, 0.0001f));

            var color = _playColor;
            color.a = _playColor.a * (1f - t);
            target.color = color;
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            if (t >= 1f) Hide();
        }

        private void Hide()
        {
            IsPlaying = false;
            if (target != null) target.enabled = false;
        }
    }
}
