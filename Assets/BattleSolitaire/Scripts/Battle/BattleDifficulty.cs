namespace BattleSolitaire.Battle
{
    public enum BattleDifficulty
    {
        Casual = 0,
        Standard = 1,
        Expert = 2
    }

    public readonly struct BattleDifficultySettings
    {
        public float MoveSpeedMultiplier { get; }
        public float AttackThinkSeconds { get; }
        public float AttackAttemptChance { get; }
        public float BlockerProgressThreshold { get; }
        public float SmartTargetingChance { get; }
        public float FogMoveSkipChance { get; }

        public BattleDifficultySettings(
            float moveSpeedMultiplier,
            float attackThinkSeconds,
            float attackAttemptChance,
            float blockerProgressThreshold,
            float smartTargetingChance,
            float fogMoveSkipChance)
        {
            MoveSpeedMultiplier = moveSpeedMultiplier;
            AttackThinkSeconds = attackThinkSeconds;
            AttackAttemptChance = attackAttemptChance;
            BlockerProgressThreshold = blockerProgressThreshold;
            SmartTargetingChance = smartTargetingChance;
            FogMoveSkipChance = fogMoveSkipChance;
        }
    }

    public static class BattleDifficultyTuning
    {
        public static BattleDifficultySettings Get(
            BattleDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BattleDifficulty.Casual:
                    return new BattleDifficultySettings(
                        moveSpeedMultiplier: 1.35f,
                        attackThinkSeconds: 0.90f,
                        attackAttemptChance: 0.55f,
                        blockerProgressThreshold: 0.60f,
                        smartTargetingChance: 0.15f,
                        fogMoveSkipChance: 0.70f);

                case BattleDifficulty.Expert:
                    return new BattleDifficultySettings(
                        moveSpeedMultiplier: 0.72f,
                        attackThinkSeconds: 0.42f,
                        attackAttemptChance: 1.00f,
                        blockerProgressThreshold: 0.20f,
                        smartTargetingChance: 1.00f,
                        fogMoveSkipChance: 0.35f);

                default:
                    return new BattleDifficultySettings(
                        moveSpeedMultiplier: 1.00f,
                        attackThinkSeconds: 0.60f,
                        attackAttemptChance: 0.82f,
                        blockerProgressThreshold: 0.35f,
                        smartTargetingChance: 0.55f,
                        fogMoveSkipChance: 0.55f);
            }
        }

        public static string GetLabel(
            BattleDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BattleDifficulty.Casual:
                    return "CASUAL";
                case BattleDifficulty.Expert:
                    return "EXPERT";
                default:
                    return "STANDARD";
            }
        }
    }
}
