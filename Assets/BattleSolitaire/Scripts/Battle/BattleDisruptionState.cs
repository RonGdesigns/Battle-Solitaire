using System;
using System.Collections.Generic;

namespace BattleSolitaire.Battle
{
    public sealed class BlockerEffect
    {
        public int Column { get; }
        public int MovesRemaining { get; set; }
        public float TimeRemaining { get; set; }

        public BlockerEffect(int column, int movesRemaining, float timeRemaining)
        {
            Column = column;
            MovesRemaining = movesRemaining;
            TimeRemaining = timeRemaining;
        }
    }

    public sealed class BattleDisruptionState
    {
        private readonly float[] _lockTimeRemaining = new float[7];
        private readonly List<BlockerEffect> _blockers = new List<BlockerEffect>();

        public float FogTimeRemaining { get; private set; }

        public float FogProtectionRemaining { get; private set; } = BattleTuning.FogOpeningGraceSeconds;

        public bool HasFog => FogTimeRemaining > 0f;
        public IReadOnlyList<BlockerEffect> Blockers => _blockers;

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            for (int i = 0; i < _lockTimeRemaining.Length; i++)
            {
                _lockTimeRemaining[i] =
                    Math.Max(0f, _lockTimeRemaining[i] - deltaTime);
            }

            FogTimeRemaining = Math.Max(0f, FogTimeRemaining - deltaTime);
            FogProtectionRemaining = Math.Max(0f, FogProtectionRemaining - deltaTime);

            for (int i = _blockers.Count - 1; i >= 0; i--)
            {
                _blockers[i].TimeRemaining -= deltaTime;

                if (_blockers[i].TimeRemaining <= 0f ||
                    _blockers[i].MovesRemaining <= 0)
                {
                    _blockers.RemoveAt(i);
                }
            }
        }

        public bool IsColumnLocked(int column)
        {
            return IsValidColumn(column) && _lockTimeRemaining[column] > 0f;
        }

        public float GetLockTimeRemaining(int column)
        {
            return IsValidColumn(column) ? _lockTimeRemaining[column] : 0f;
        }

        public bool IsColumnBlocked(int column)
        {
            if (!IsValidColumn(column))
                return false;

            for (int i = 0; i < _blockers.Count; i++)
            {
                if (_blockers[i].Column == column)
                    return true;
            }

            return false;
        }

        public bool CanUseColumn(int column)
        {
            return IsValidColumn(column) &&
                   !IsColumnLocked(column) &&
                   !IsColumnBlocked(column);
        }

        public void AddLock(int column, float duration)
        {
            if (!IsValidColumn(column))
                return;

            _lockTimeRemaining[column] =
                Math.Max(_lockTimeRemaining[column], duration);
        }

        public void AddFog(float duration)
        {
            FogTimeRemaining = Math.Max(FogTimeRemaining, duration);
            FogProtectionRemaining = BattleTuning.FogCooldownSeconds;
        }

        public void RestoreFog(float remaining, float protection)
        {
            FogTimeRemaining = remaining;
            FogProtectionRemaining = protection;
        }

        public void AddBlocker(
            int column,
            int movesToClear,
            float maxDuration)
        {
            if (!IsValidColumn(column))
                return;

            for (int i = 0; i < _blockers.Count; i++)
            {
                if (_blockers[i].Column != column)
                    continue;

                _blockers[i].MovesRemaining =
                    Math.Min(5, _blockers[i].MovesRemaining + movesToClear);

                _blockers[i].TimeRemaining =
                    Math.Max(_blockers[i].TimeRemaining, maxDuration);

                return;
            }

            _blockers.Add(
                new BlockerEffect(column, movesToClear, maxDuration));
        }

        public void RegisterSuccessfulProgressMove()
        {
            if (_blockers.Count == 0)
                return;

            _blockers[0].MovesRemaining--;

            if (_blockers[0].MovesRemaining <= 0)
                _blockers.RemoveAt(0);
        }

        public int GetBlockerMovesRemaining(int column)
        {
            for (int i = 0; i < _blockers.Count; i++)
            {
                if (_blockers[i].Column == column)
                    return _blockers[i].MovesRemaining;
            }

            return 0;
        }

        private static bool IsValidColumn(int column)
        {
            return column >= 0 && column < 7;
        }
    }
}
