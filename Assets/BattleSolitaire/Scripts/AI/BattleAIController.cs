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
        private float _moveTimer;
        private float _attackThinkTimer;

        public BattleAIController(int seed)
        {
            _random = new Random(seed);
            ScheduleNextMove();
        }

        public BattleAIIntent Tick(float deltaTime, BattleMatch match)
        {
            bool wantsMove = false;
            bool wantsAttack = false;
            BattleAttackType attackType = BattleAttackType.Lock;
            int targetColumn = -1;

            _moveTimer -= deltaTime;
            _attackThinkTimer -= deltaTime;

            if (_moveTimer <= 0f)
            {
                wantsMove = true;
                ScheduleNextMove();
            }

            if (_attackThinkTimer <= 0f)
            {
                _attackThinkTimer = 0.6f;

                BattleParticipant ai = match.Opponent;

                if (ai.Energy >= BattleTuning.BlockerCost &&
                    match.Player.ClearPercentage >= 0.35f)
                {
                    wantsAttack = true;
                    attackType = BattleAttackType.Blocker;
                    targetColumn = PickTargetColumn(match);
                }
                else if (ai.Energy >= BattleTuning.LockCost)
                {
                    wantsAttack = true;

                    if (_random.NextDouble() < 0.5)
                    {
                        attackType = BattleAttackType.Lock;
                        targetColumn = PickTargetColumn(match);
                    }
                    else
                    {
                        attackType = BattleAttackType.Fog;
                    }
                }
            }

            return new BattleAIIntent(
                wantsMove,
                wantsAttack,
                attackType,
                targetColumn);
        }

        private int PickTargetColumn(BattleMatch match)
        {
            int start = _random.Next(0, 7);

            for (int offset = 0; offset < 7; offset++)
            {
                int column = (start + offset) % 7;

                if (match.CanUseColumn(BattleSide.Player, column))
                    return column;
            }

            return start;
        }

        private void ScheduleNextMove()
        {
            _moveTimer =
                0.7f + (float)_random.NextDouble() * 0.8f;
        }
    }
}
