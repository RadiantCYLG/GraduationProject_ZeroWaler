namespace ZeroWalker.Player
{
    /// <summary>
    /// Blitz 骨骼动画名称常量，避免在代码里裸写字符串。
    /// </summary>
    public static class PlayerAnimations
    {
        public const string Idle = "Idle";
        public const string Move = "Move";
        public const string AttackBegin = "Attack_Begin";
        public const string AttackLoop = "Attack_Loop";
        public const string AttackEnd = "Attack_End";

        /// <summary>Attack_Loop 中标记“枪口开火”的 Spine 事件名。</summary>
        public const string AttackEvent = "OnAttack";
        public const string Die = "Die";
        public const string Start = "Start";
        public const string Skill01 = "Skill_01";
        public const string Skill02 = "Skill_02";
    }
}
