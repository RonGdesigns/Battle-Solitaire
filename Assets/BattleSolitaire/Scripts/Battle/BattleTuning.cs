namespace BattleSolitaire.Battle
{
    public static class BattleTuning
    {
        public const float RivalWarningSeconds = 2f;
        public const float RivalAttackCooldownSeconds = 10f;
        public const float RivalOpeningGraceSeconds = 15f;
        public const int MaxHealth = 100;
        public const int MaxEnergy = 100;
        public const int MaxShield = 30;

        public const int BaseMoveEnergy = 1;
        public const int RevealHiddenCardEnergy = 3;
        public const int FoundationMoveEnergy = 2;
        public const int ClearedColumnEnergy = 10;

        public const int ComboThreeBonusEnergy = 4;
        public const int ComboFiveBonusEnergy = 8;

        public const int FoundationDamage = 1;
        public const int ClearedColumnDamage = 5;
        public const int FoundationShield = 2;

        public const float FullComboWindowSeconds = 2.5f;
        public const float ComboExpireSeconds = 5f;

        public const int LockCost = 25;
        public const float LockDurationSeconds = 3f;

        public const int FogCost = 25;
        public const float FogDurationSeconds = 2f;
        public const float FogCooldownSeconds = 20f;
        public const float FogOpeningGraceSeconds = 15f;

        public const int BlockerCost = 50;
        public const int BlockerMovesToClear = 3;
        public const float BlockerMaxDurationSeconds = 8f;
    }
}
