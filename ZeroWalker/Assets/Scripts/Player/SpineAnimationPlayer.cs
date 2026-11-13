using Spine;
using Spine.Unity;
using UnityEngine;

namespace ZeroWalker.Player
{
    /// <summary>
    /// Blitz 的 Spine 动画播放器，对 Spine.AnimationState 做一层薄封装。
    /// 所有动画都放在 0 号轨道上播放（单轨即可满足横板角色需求）。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class SpineAnimationPlayer : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [Header("混接")]
        [Tooltip("动画之间的默认过渡时间（秒）")]
        [SerializeField] private float defaultMix = 0.08f;

        /// <summary>
        /// Spine 动画事件回调：(动画名, 事件名)。
        /// 用于让玩法逻辑精确地跟着动画事件触发（例如 Attack_Loop 的 OnAttack 出弹）。
        /// </summary>
        public event System.Action<string, string> SpineEvent;

        private Spine.AnimationState _subscribedState;

        /// <summary>Spine 的动画状态机。</summary>
        public Spine.AnimationState State => skeletonAnimation != null ? skeletonAnimation.AnimationState : null;

        /// <summary>骨骼数据，用于查询骨骼（例如枪口 F_Muzzle）。</summary>
        public Skeleton Skeleton => skeletonAnimation != null ? skeletonAnimation.Skeleton : null;

        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        private void Awake()
        {
            if (skeletonAnimation == null)
                skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            if (State != null)
                State.Data.DefaultMix = defaultMix;

            TrySubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        /// 订阅 Spine 的动画事件。SkeletonAnimation 在它自己的 Awake 中创建 AnimationState，
        /// 而本组件的执行顺序更早，所以这里允许重复调用，并在 Start 时补齐订阅。
        /// </summary>
        private void TrySubscribe()
        {
            var state = State;
            if (state == null || ReferenceEquals(state, _subscribedState)) return;

            Unsubscribe();
            state.Event += HandleSpineEvent;
            _subscribedState = state;
        }

        private void Unsubscribe()
        {
            if (_subscribedState != null)
                _subscribedState.Event -= HandleSpineEvent;

            _subscribedState = null;
        }

        private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event spineEvent)
        {
            if (trackEntry == null || trackEntry.Animation == null) return;

            var eventName = spineEvent?.Data?.Name;
            if (string.IsNullOrEmpty(eventName)) return;

            SpineEvent?.Invoke(trackEntry.Animation.Name, eventName);
        }

        /// <summary>立即播放动画（替换当前轨道 0 的内容）。</summary>
        public TrackEntry Play(string animationName, bool loop = true, float mixDuration = -1f)
        {
            var state = State;
            if (state == null || string.IsNullOrEmpty(animationName)) return null;

            var entry = state.SetAnimation(0, animationName, loop);
            if (entry == null) return null;

            if (mixDuration >= 0f)
                entry.MixDuration = mixDuration;

            return entry;
        }

        /// <summary>在当前动画之后排队播放下一段动画。</summary>
        public TrackEntry Queue(string animationName, bool loop = true, float delay = 0f)
        {
            var state = State;
            if (state == null || string.IsNullOrEmpty(animationName)) return null;

            return state.AddAnimation(0, animationName, loop, delay);
        }

        /// <summary>当前正在播放的动画名（没有则返回空串）。</summary>
        public string CurrentAnimationName
        {
            get
            {
                var entry = State?.GetCurrent(0);
                return entry?.Animation != null ? entry.Animation.Name : string.Empty;
            }
        }

        /// <summary>查询某段动画的时长（秒），找不到返回 0。</summary>
        public float GetDuration(string animationName)
        {
            var animation = State?.Data?.SkeletonData?.FindAnimation(animationName);
            return animation != null ? animation.Duration : 0f;
        }

        /// <summary>清空 0 号轨道。</summary>
        public void Clear()
        {
            State?.ClearTrack(0);
        }
    }
}
