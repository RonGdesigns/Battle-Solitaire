namespace BattleSolitaire.Battle
{
    public readonly struct BattleMoveOutcome
    {
        public bool Counted { get; }
        public int EnergyGained { get; }
        public int ShieldGained { get; }
        public int DamageDealt { get; }
        public int ComboCount { get; }

        public BattleMoveOutcome(
            bool counted,
            int energyGained,
            int shieldGained,
            int damageDealt,
            int comboCount)
        {
            Counted = counted;
            EnergyGained = energyGained;
            ShieldGained = shieldGained;
            DamageDealt = damageDealt;
            ComboCount = comboCount;
        }

        public static BattleMoveOutcome None =>
            new BattleMoveOutcome(false, 0, 0, 0, 0);
    }
}
