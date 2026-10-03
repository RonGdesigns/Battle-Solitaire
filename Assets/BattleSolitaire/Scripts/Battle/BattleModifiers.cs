namespace BattleSolitaire.Battle
{
    public sealed class BattleModifiers
    {
        public static BattleModifiers None { get; } =
            new BattleModifiers();

        public int FoundationDamageBonus { get; }
        public int FoundationShieldBonus { get; }
        public int ComboEnergyBonusAtThree { get; }
        public int MaxShieldBonus { get; }

        public BattleModifiers(
            int foundationDamageBonus = 0,
            int foundationShieldBonus = 0,
            int comboEnergyBonusAtThree = 0,
            int maxShieldBonus = 0)
        {
            FoundationDamageBonus = foundationDamageBonus;
            FoundationShieldBonus = foundationShieldBonus;
            ComboEnergyBonusAtThree = comboEnergyBonusAtThree;
            MaxShieldBonus = maxShieldBonus;
        }
    }
}
