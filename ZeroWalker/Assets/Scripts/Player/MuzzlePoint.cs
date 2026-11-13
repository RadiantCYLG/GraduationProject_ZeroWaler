using Spine.Unity;
using UnityEngine;
using ZeroWalker.VFX;

namespace ZeroWalker.Player
{
    /// <summary>
    /// 枪口定位点。
    /// 直接读取 Spine 骨骼 "F_Muzzle" 的世界坐标，保证子弹总是从枪口射出，
    /// 无论角色处于哪个动作、朝向哪边。
    /// </summary>
    [DisallowMultipleComponent]
    public class MuzzlePoint : MonoBehaviour
    {
        [Header("骨骼")]
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private string boneName = "F_Muzzle";
        [Tooltip("找不到骨骼时使用的本地偏移（相对骨骼根节点）")]
        [SerializeField] private Vector3 fallbackLocalOffset = new Vector3(0.85f, 1.05f, 0f);

        [Header("枪口火光")]
        [SerializeField] private SpriteFlash flash;

        private Spine.Bone _bone;

        /// <summary>枪口的世界坐标。</summary>
        public Vector3 WorldPosition
        {
            get
            {
                if (skeletonAnimation == null) return transform.position;

                if (_bone == null && skeletonAnimation.Skeleton != null)
                    _bone = skeletonAnimation.Skeleton.FindBone(boneName);

                if (_bone != null)
                    return skeletonAnimation.transform.TransformPoint(new Vector3(_bone.WorldX, _bone.WorldY, 0f));

                return skeletonAnimation.transform.TransformPoint(fallbackLocalOffset);
            }
        }

        private void Awake()
        {
            if (skeletonAnimation == null)
                skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }

        /// <summary>在枪口播放一次火光（使用火光自身的颜色）。</summary>
        public void PlayFlash(int facing)
        {
            PlayFlash(facing, Color.white);
        }

        /// <summary>在枪口播放一次火光，并乘上指定着色（特殊子弹使用自己的颜色）。</summary>
        public void PlayFlash(int facing, Color tint)
        {
            if (flash == null) return;

            flash.transform.position = WorldPosition;
            flash.transform.localScale = Vector3.one * Mathf.Abs(flash.transform.localScale.x);
            flash.Play(tint);
        }

        private void LateUpdate()
        {
            // 火光存活期间跟随枪口，避免角色移动时火光掉队
            if (flash != null && flash.IsPlaying)
                flash.transform.position = WorldPosition;
        }
    }
}
