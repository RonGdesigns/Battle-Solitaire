using BattleSolitaire.Core;

namespace BattleSolitaire.Battle
{
    public enum BattleSide
    {
        Player,
        Opponent
    }

    public enum BattleMatchState
    {
        Running,
        PlayerWon,
        OpponentWon,
        Draw
    }

    public sealed class BattleMatch
    {
        public BattleCombatRecord PlayerRecord { get; private set; } = new BattleCombatRecord();
        public BattleCombatRecord OpponentRecord { get; private set; } = new BattleCombatRecord();
        public RivalAttackWarning RivalWarning { get; private set; } = new RivalAttackWarning();
        public int LastHealthDamage { get; private set; }
        public int LastShieldDamage { get; private set; }
        public bool RivalAttackLanded { get; private set; }
        public void RestoreCombat(BattleCombatRecord player, BattleCombatRecord opponent, RivalAttackWarning warning)
        {
            player?.Validate(); opponent?.Validate(); warning?.Validate();
            PlayerRecord=player?.Copy() ?? new BattleCombatRecord();
            OpponentRecord=opponent?.Copy() ?? new BattleCombatRecord();
            RivalWarning=warning?.Copy() ?? new RivalAttackWarning();
        }
        public bool QueueRivalAttack(BattleAttackType type,int column)
        {
            if(State!=BattleMatchState.Running || RivalWarning.pending || RivalWarning.cooldown>0 ||
               !System.Enum.IsDefined(typeof(BattleAttackType),type) || Opponent.Energy<BattleAttack.GetCost(type)) return false;
            if(type==BattleAttackType.Fog ? Player.Disruptions.FogProtectionRemaining>0 : !Player.CanUseColumn(column)) return false;
            RivalWarning.pending=true; RivalWarning.type=type; RivalWarning.column=column;
            RivalWarning.remaining=BattleTuning.RivalWarningSeconds;
            return true;
        }

        public BattleParticipant Player { get; }
        public BattleParticipant Opponent { get; }

        public BattleMatchState State { get; private set; }
            = BattleMatchState.Running;

        public BattleMatch(
            int playerSeed,
            int opponentSeed)
            : this(
                playerSeed,
                opponentSeed,
                BattleModifiers.None,
                BattleModifiers.None)
        {
        }

        public BattleMatch(
            int playerSeed,
            int opponentSeed,
            BattleModifiers playerModifiers,
            BattleModifiers opponentModifiers)
        {
            Player = new BattleParticipant(
                "Player",
                playerSeed,
                playerModifiers);

            Opponent = new BattleParticipant(
                "Opponent",
                opponentSeed,
                opponentModifiers);
        }

        public void Tick(float deltaTime)
        {
            RivalAttackLanded=false;
            if (State != BattleMatchState.Running)
            {
                RivalWarning.pending=false;
                return;
            }

            Player.Tick(System.Math.Max(0,deltaTime));
            Opponent.Tick(System.Math.Max(0,deltaTime));
            EvaluateWinner();
            if(State!=BattleMatchState.Running) { RivalWarning.pending=false; return; }
            if(deltaTime<=0) return;
            RivalWarning.cooldown=System.Math.Max(0,RivalWarning.cooldown-deltaTime);
            if(RivalWarning.pending)
            {
                RivalWarning.remaining=System.Math.Max(0,RivalWarning.remaining-deltaTime);
                if(RivalWarning.remaining<=0)
                {
                    RivalWarning.pending=false;
                    RivalAttackLanded=TryAttack(BattleSide.Opponent,RivalWarning.type,RivalWarning.column);
                    RivalWarning.cooldown=BattleTuning.RivalAttackCooldownSeconds;
                }
            }
        }

        public BattleMoveOutcome RegisterMove(
            BattleSide side,
            MoveResult move)
        {
            if (State != BattleMatchState.Running)
                return BattleMoveOutcome.None;

            BattleParticipant actor =
                GetParticipant(side);

            BattleParticipant target =
                GetOpponent(side);

            BattleMoveOutcome outcome =
                actor.ApplyMove(move);

            LastHealthDamage=0; LastShieldDamage=0;
            BattleCombatRecord record=side==BattleSide.Player?PlayerRecord:OpponentRecord;
            BattleCombatRecord defense=side==BattleSide.Player?OpponentRecord:PlayerRecord;
            if(outcome.Counted)
            {
                record.energyEarned+=outcome.EnergyGained;
                record.longestCombo=System.Math.Max(record.longestCombo,outcome.ComboCount);
                int shieldBefore=target.Shield;
                LastHealthDamage=target.ApplyDamage(outcome.DamageDealt);
                LastShieldDamage=shieldBefore-target.Shield;
                record.healthDamage+=LastHealthDamage;
                record.shieldDamage+=LastShieldDamage;
                defense.shieldBlocked+=LastShieldDamage;
            }

            EvaluateWinner();
            return outcome;
        }

        public bool TryAttack(
            BattleSide attackerSide,
            BattleAttackType attackType,
            int targetColumn = -1)
        {
            if (State != BattleMatchState.Running || !System.Enum.IsDefined(typeof(BattleAttackType),attackType))
                return false;

            BattleParticipant attacker =
                GetParticipant(attackerSide);

            BattleParticipant defender =
                GetOpponent(attackerSide);

            int cost =
                BattleAttack.GetCost(attackType);

            if (attacker.Energy < cost)
                return false;

            if ((attackType == BattleAttackType.Lock ||
                 attackType == BattleAttackType.Blocker) &&
                (targetColumn < 0 ||
                 targetColumn > 6))
            {
                return false;
            }

            if (attackType == BattleAttackType.Fog && defender.Disruptions.FogProtectionRemaining > 0f)
                return false;

            if (!attacker.SpendEnergy(cost))
                return false;

            switch (attackType)
            {
                case BattleAttackType.Lock:
                    defender.Disruptions.AddLock(
                        targetColumn,
                        BattleTuning.LockDurationSeconds);
                    break;

                case BattleAttackType.Fog:
                    defender.Disruptions.AddFog(
                        BattleTuning.FogDurationSeconds);
                    break;

                case BattleAttackType.Blocker:
                    defender.Disruptions.AddBlocker(
                        targetColumn,
                        BattleTuning.BlockerMovesToClear,
                        BattleTuning.BlockerMaxDurationSeconds);
                    break;
            }

            return true;
        }

        public bool CanUseColumn(
            BattleSide side,
            int column)
        {
            return GetParticipant(side)
                .CanUseColumn(column);
        }

        public float GetProgress(
            BattleSide side)
        {
            return GetParticipant(side)
                .ClearPercentage;
        }

        private void EvaluateWinner()
        {
            bool playerPerfectClear =
                Player.Game.IsVictory();

            bool opponentPerfectClear =
                Opponent.Game.IsVictory();

            bool playerDefeated =
                Player.IsDefeated;

            bool opponentDefeated =
                Opponent.IsDefeated;

            if ((playerPerfectClear &&
                 opponentPerfectClear) ||
                (playerDefeated &&
                 opponentDefeated))
            {
                State =
                    BattleMatchState.Draw;

                return;
            }

            if (playerPerfectClear ||
                opponentDefeated)
            {
                State =
                    BattleMatchState.PlayerWon;

                return;
            }

            if (opponentPerfectClear ||
                playerDefeated)
            {
                State =
                    BattleMatchState.OpponentWon;
            }
        }

        private BattleParticipant GetParticipant(
            BattleSide side)
        {
            return side == BattleSide.Player
                ? Player
                : Opponent;
        }

        private BattleParticipant GetOpponent(
            BattleSide side)
        {
            return side == BattleSide.Player
                ? Opponent
                : Player;
        }
    }
}
