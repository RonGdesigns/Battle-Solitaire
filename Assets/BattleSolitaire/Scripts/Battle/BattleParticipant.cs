using System;
using BattleSolitaire.Core;

namespace BattleSolitaire.Battle
{
    public sealed class BattleParticipant
    {
        public string Name { get; }
        public SolitaireGame Game { get; }
        public ComboTracker Combo { get; } = new ComboTracker();
        public BattleDisruptionState Disruptions { get; }
            = new BattleDisruptionState();

        public BattleModifiers Modifiers { get; }

        public int Health { get; private set; } = BattleTuning.MaxHealth;
        public int Energy { get; private set; }
        public int Shield { get; private set; }

        public int MaxShield =>
            BattleTuning.MaxShield +
            Modifiers.MaxShieldBonus;

        public bool IsDefeated => Health <= 0;
        public float ClearPercentage => Game.GetClearPercentage();

        public BattleParticipant(string name, int seed)
            : this(name, seed, BattleModifiers.None)
        {
        }

        public BattleParticipant(
            string name,
            int seed,
            BattleModifiers modifiers)
        {
            Name = name;
            Modifiers = modifiers ?? BattleModifiers.None;
            Game = new SolitaireGame(seed);
        }

        public void Tick(float deltaTime)
        {
            Combo.Tick(deltaTime);
            Disruptions.Tick(deltaTime);
        }

        public bool CanUseColumn(int column)
        {
            return Disruptions.CanUseColumn(column);
        }

        public BattleMoveOutcome ApplyMove(MoveResult move)
        {
            if (!move.Success || !move.CountsForBattleProgress)
                return BattleMoveOutcome.None;

            int comboBonus = Combo.RegisterProgressMove();
            int energyGained =
                BattleTuning.BaseMoveEnergy +
                comboBonus;

            int shieldGained = 0;
            int damageDealt = 0;

            if (Combo.Count >= 3)
            {
                energyGained +=
                    Modifiers.ComboEnergyBonusAtThree;
            }

            if (move.RevealedHiddenCard)
            {
                energyGained +=
                    BattleTuning.RevealHiddenCardEnergy;
            }

            if (move.FoundationMove)
            {
                energyGained +=
                    BattleTuning.FoundationMoveEnergy;

                shieldGained +=
                    BattleTuning.FoundationShield +
                    Modifiers.FoundationShieldBonus;

                damageDealt +=
                    BattleTuning.FoundationDamage +
                    Modifiers.FoundationDamageBonus;
            }

            if (move.ClearedColumn)
            {
                energyGained +=
                    BattleTuning.ClearedColumnEnergy;

                damageDealt +=
                    BattleTuning.ClearedColumnDamage;
            }

            int previousEnergy = Energy;
            int previousShield = Shield;

            Energy = Math.Min(
                BattleTuning.MaxEnergy,
                Energy + energyGained);

            Shield = Math.Min(
                MaxShield,
                Shield + shieldGained);

            Disruptions.RegisterSuccessfulProgressMove();

            return new BattleMoveOutcome(
                counted: true,
                energyGained: Energy - previousEnergy,
                shieldGained: Shield - previousShield,
                damageDealt: damageDealt,
                comboCount: Combo.Count);
        }

        public void RestoreResources(int health, int energy, int shield)
        {
            if (health < 1 || health > BattleTuning.MaxHealth || energy < 0 || energy > BattleTuning.MaxEnergy || shield < 0 || shield > MaxShield)
                throw new ArgumentOutOfRangeException("Saved resources are outside battle limits.");
            Health = health; Energy = energy; Shield = shield;
        }

        public bool SpendEnergy(int amount)
        {
            if (amount < 0 || Energy < amount)
                return false;

            Energy -= amount;
            return true;
        }

        public int ApplyDamage(int amount)
        {
            if (amount <= 0)
                return 0;

            int remainingDamage = amount;

            if (Shield > 0)
            {
                int absorbed =
                    Math.Min(Shield, remainingDamage);

                Shield -= absorbed;
                remainingDamage -= absorbed;
            }

            if (remainingDamage <= 0)
                return 0;

            int previousHealth = Health;

            Health = Math.Max(
                0,
                Health - remainingDamage);

            return previousHealth - Health;
        }
    }
}
