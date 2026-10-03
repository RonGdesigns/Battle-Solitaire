using System;
using System.IO;

namespace BattleSolitaire.Battle
{
    [Serializable] public sealed class BattleCombatRecord
    {
        public int healthDamage, shieldDamage, shieldBlocked, energyEarned, longestCombo;
        public BattleCombatRecord Copy() => (BattleCombatRecord)MemberwiseClone();
        public void Validate()
        {
            foreach(int value in new[]{healthDamage,shieldDamage,shieldBlocked,energyEarned,longestCombo})
                if(value<0 || value>100000000) throw new InvalidDataException("Invalid combat record.");
        }
    }

    // Timers advance only with the match. Saves preserve a warning already in progress.
    [Serializable] public sealed class RivalAttackWarning
    {
        public bool pending;
        public BattleAttackType type;
        public int column=-1;
        public float remaining, cooldown=BattleTuning.RivalOpeningGraceSeconds;
        public RivalAttackWarning Copy() => (RivalAttackWarning)MemberwiseClone();
        public void Validate()
        {
            if(!Enum.IsDefined(typeof(BattleAttackType),type) || !ValidTime(remaining,BattleTuning.RivalWarningSeconds) ||
               !ValidTime(cooldown,BattleTuning.RivalOpeningGraceSeconds) || column < -1 || column > 6 ||
               (pending && type!=BattleAttackType.Fog && column<0)) throw new InvalidDataException("Invalid rival warning.");
        }
        private static bool ValidTime(float n,float max) => !float.IsNaN(n) && !float.IsInfinity(n) && n>=0 && n<=max;
    }
}
