using System;

namespace BattleSolitaire.Battle
{
    public readonly struct BattleAIIntent
    {
        public bool WantsMove { get; }
        public bool WantsAttack { get; }
        public BattleAttackType AttackType { get; }
        public int TargetColumn { get; }

        public BattleAIIntent(
            bool wantsMove,
            bool wantsAttack,
            BattleAttackType attackType,
            int targetColumn)
        {
            WantsMove = wantsMove;
            WantsAttack = wantsAttack;
            AttackType = attackType;
            TargetColumn = targetColumn;
        }
    }

    public sealed class BattleAIController
    {
        private readonly Random _random;
        private readonly BattleDifficultySettings _settings;

        private float _moveTimer;
        private float _attackThinkTimer;

        public BattleDifficulty Difficulty { get; }

        public BattleAIController(
            int seed,
            BattleDifficulty difficulty = BattleDifficulty.Standard)
        {
            _random = new Random(seed);
            Difficulty = difficulty;
            _settings = BattleDifficultyTuning.Get(difficulty);

            ScheduleNextMove();
            _attackThinkTimer = _settings.AttackThinkSeconds;
        }

        public BattleAIIntent Tick(
            float deltaTime,
            BattleMatch match)
        {
            bool wantsMove = false;
            bool wantsAttack = false;
            BattleAttackType attackType = BattleAttackType.Lock;
            int targetColumn = -1;

            _moveTimer -= deltaTime;
            _attackThinkTimer -= deltaTime;

            if (_moveTimer <= 0f)
            {
                bool fogged =
                    match.Opponent.Disruptions.HasFog;

                wantsMove =
                    !fogged ||
                    _random.NextDouble() >=
                        _settings.FogMoveSkipChance;

                float fogMultiplier =
                    fogged ? 1.45f : 1f;

                ScheduleNextMove(
                    fogMultiplier *
                    _settings.MoveSpeedMultiplier);
            }

            if (_attackThinkTimer <= 0f)
            {
                _attackThinkTimer =
                    _settings.AttackThinkSeconds;

                if (_random.NextDouble() <=
                    _settings.AttackAttemptChance)
                {
                    BattleParticipant ai =
                        match.Opponent;

                    if (ai.Energy >=
                            BattleTuning.BlockerCost &&
                        match.Player.ClearPercentage >=
                            _settings.BlockerProgressThreshold)
                    {
                        wantsAttack = true;
                        attackType =
                            BattleAttackType.Blocker;

                        targetColumn =
                            PickTargetColumn(match);
                    }
                    else if (ai.Energy >=
                             BattleTuning.LockCost)
                    {
                        wantsAttack = true;

                        bool preferLock =
                            Difficulty ==
                                BattleDifficulty.Expert
                                ? _random.NextDouble() < 0.65
                                : _random.NextDouble() < 0.50;

                        if (preferLock)
                        {
                            attackType =
                                BattleAttackType.Lock;

                            targetColumn =
                                PickTargetColumn(match);
                        }
                        else
                        {
                            attackType =
                                BattleAttackType.Fog;
                        }
                    }
                }
            }

            return new BattleAIIntent(
                wantsMove,
                wantsAttack,
                attackType,
                targetColumn);
        }

        private int PickTargetColumn(
            BattleMatch match)
        {
            if (_random.NextDouble() <=
                _settings.SmartTargetingChance)
            {
                int bestColumn = -1;
                int bestScore = int.MinValue;

                for (int column = 0;
                     column < 7;
                     column++)
                {
                    if (!match.CanUseColumn(
                            BattleSide.Player,
                            column))
                    {
                        continue;
                    }

                    var cards =
                        match.Player.Game
                            .Tableau[column];

                    int faceDown = 0;

                    for (int i = 0;
                         i < cards.Count;
                         i++)
                    {
                        if (!cards[i].IsFaceUp)
                            faceDown++;
                    }

                    int score =
                        (faceDown * 12) +
                        cards.Count;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestColumn = column;
                    }
                }

                if (bestColumn >= 0)
                    return bestColumn;
            }

            int start = _random.Next(0, 7);

            for (int offset = 0;
                 offset < 7;
                 offset++)
            {
                int column =
                    (start + offset) % 7;

                if (match.CanUseColumn(
                        BattleSide.Player,
                        column))
                {
                    return column;
                }
            }

            return start;
        }

        private void ScheduleNextMove(
            float multiplier = 1f)
        {
            _moveTimer =
                (0.7f +
                 (float)_random.NextDouble() *
                 0.8f) *
                multiplier;
        }
    }
}
