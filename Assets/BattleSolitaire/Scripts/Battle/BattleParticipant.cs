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

        public int Health { get; private set; } = BattleTuning.MaxHealth;
        public int Energy { get; private set; }
        public int Shield { get; private set; }

        public bool IsDefeated => Health <= 0;
        public float ClearPercentage => Game.GetClearPercentage();

        public BattleParticipant(string name, int seed)
        {
            Name = name;
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
            int energyGained = BattleTuning.BaseMoveEnergy + comboBonus;
            int shieldGained = 0;
            int damageDealt = 0;

            if (move.RevealedHiddenCard)
                energyGained += BattleTuning.RevealHiddenCardEnergy;

            if (move.FoundationMove)
            {
                energyGained += BattleTuning.FoundationMoveEnergy;
                shieldGained += BattleTuning.FoundationShield;
                damageDealt += BattleTuning.FoundationDamage;
            }

            if (move.ClearedColumn)
            {
                energyGained += BattleTuning.ClearedColumnEnergy;
                damageDealt += BattleTuning.ClearedColumnDamage;
            }

            int previousEnergy = Energy;
            int previousShield = Shield;

            Energy = Math.Min(BattleTuning.MaxEnergy, Energy + energyGained);
            Shield = Math.Min(BattleTuning.MaxShield, Shield + shieldGained);

            Disruptions.RegisterSuccessfulProgressMove();

            return new BattleMoveOutcome(
                counted: true,
                energyGained: Energy - previousEnergy,
                shieldGained: Shield - previousShield,
                damageDealt: damageDealt,
                comboCount: Combo.Count);
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
                int absorbed = Math.Min(Shield, remainingDamage);
                Shield -= absorbed;
                remainingDamage -= absorbed;
            }

            if (remainingDamage <= 0)
                return 0;

            int previousHealth = Health;
            Health = Math.Max(0, Health - remainingDamage);

            return previousHealth - Health;
        }
    }
}
