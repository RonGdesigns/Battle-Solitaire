namespace BattleSolitaire.Battle
{
    public sealed class ComboTracker
    {
        public int Count { get; private set; }
        public float SecondsSinceLastMove { get; private set; }
            = BattleTuning.ComboExpireSeconds + 1f;

        public float Strength01
        {
            get
            {
                if (Count <= 0)
                    return 0f;

                if (SecondsSinceLastMove <= BattleTuning.FullComboWindowSeconds)
                    return 1f;

                if (SecondsSinceLastMove >= BattleTuning.ComboExpireSeconds)
                    return 0f;

                float drainWindow =
                    BattleTuning.ComboExpireSeconds -
                    BattleTuning.FullComboWindowSeconds;

                return 1f -
                    ((SecondsSinceLastMove - BattleTuning.FullComboWindowSeconds) /
                    drainWindow);
            }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || Count <= 0)
                return;

            SecondsSinceLastMove += deltaTime;

            if (SecondsSinceLastMove >= BattleTuning.ComboExpireSeconds)
                Reset();
        }

        public int RegisterProgressMove()
        {
            if (Count <= 0 ||
                SecondsSinceLastMove >= BattleTuning.ComboExpireSeconds)
            {
                Count = 1;
            }
            else
            {
                Count++;
            }

            SecondsSinceLastMove = 0f;

            if (Count == 3)
                return BattleTuning.ComboThreeBonusEnergy;

            if (Count == 5)
                return BattleTuning.ComboFiveBonusEnergy;

            return 0;
        }

        public void Reset()
        {
            Count = 0;
            SecondsSinceLastMove =
                BattleTuning.ComboExpireSeconds + 1f;
        }
    }
}
