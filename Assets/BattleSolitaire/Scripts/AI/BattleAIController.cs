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

    [Serializable]
    public sealed class SavedAI
    {
        public int seed, samples;
        public float moveTimer, attackTimer;
        public void Validate()
        {
            if (samples < 0 || samples > 1000000 || float.IsNaN(moveTimer) || float.IsNaN(attackTimer) ||
                moveTimer < -10 || moveTimer > 30 || attackTimer < -10 || attackTimer > 30)
                throw new System.IO.InvalidDataException("Invalid AI checkpoint.");
        }
    }

    public sealed class BattleAIController
    {
        private Random _random;
        private int _seed, _samples;
        private readonly BattleDifficultySettings _settings;

        private float _moveTimer;
        private float _attackThinkTimer;

        public BattleDifficulty Difficulty { get; }

        public BattleAIController(
            int seed,
            BattleDifficulty difficulty = BattleDifficulty.Standard)
        {
            _seed = seed; _random = new Random(seed);
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
                    NextSample() >=
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

                if (NextSample() <=
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
                                ? NextSample() < 0.65
                                : NextSample() < 0.50;

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

        private double NextSample() { _samples++; return _random.NextDouble(); }
        public SavedAI Capture() => new SavedAI { seed=_seed, samples=_samples, moveTimer=_moveTimer, attackTimer=_attackThinkTimer };
        public void Restore(SavedAI state)
        {
            state.Validate(); _seed=state.seed; _random=new Random(_seed); _samples=0;
            for(int i=0;i<state.samples;i++) NextSample();
            _moveTimer=state.moveTimer; _attackThinkTimer=state.attackTimer;
        }

        private int PickTargetColumn(
            BattleMatch match)
        {
            if (NextSample() <=
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

            int start = (int)(NextSample() * 7);

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
                 (float)NextSample() *
                 0.8f) *
                multiplier;
        }
    }
}
