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
            if (State != BattleMatchState.Running)
                return;

            Player.Tick(deltaTime);
            Opponent.Tick(deltaTime);

            EvaluateWinner();
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

            if (outcome.Counted &&
                outcome.DamageDealt > 0)
            {
                target.ApplyDamage(
                    outcome.DamageDealt);
            }

            EvaluateWinner();
            return outcome;
        }

        public bool TryAttack(
            BattleSide attackerSide,
            BattleAttackType attackType,
            int targetColumn = -1)
        {
            if (State != BattleMatchState.Running)
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
