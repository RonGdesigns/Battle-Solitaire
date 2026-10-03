namespace BattleSolitaire.Battle
{
    public enum BattleAttackType
    {
        Lock,
        Fog,
        Blocker
    }

    public static class BattleAttack
    {
        public static int GetCost(BattleAttackType type)
        {
            switch (type)
            {
                case BattleAttackType.Lock:
                    return BattleTuning.LockCost;
                case BattleAttackType.Fog:
                    return BattleTuning.FogCost;
                case BattleAttackType.Blocker:
                    return BattleTuning.BlockerCost;
                default:
                    return int.MaxValue;
            }
        }
    }
}
