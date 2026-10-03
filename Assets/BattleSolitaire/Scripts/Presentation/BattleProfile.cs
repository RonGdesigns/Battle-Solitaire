using BattleSolitaire.Battle;
using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleProfile
    {
        private const string SelectedBattlerKey = "BattleSolitaire.Profile.SelectedBattler";
        private const string WinsKey = "BattleSolitaire.Profile.Wins";
        private const string MatchesKey = "BattleSolitaire.Profile.Matches";
        private const string LongestComboKey = "BattleSolitaire.Profile.LongestCombo";
        private const string PerfectClearsKey = "BattleSolitaire.Profile.PerfectClears";

        public BattlerId SelectedBattler
        {
            get
            {
                int raw = PlayerPrefs.GetInt(
                    SelectedBattlerKey,
                    (int)BattlerId.Kael);

                raw = Mathf.Clamp(raw, 0, BattlerCatalog.Count - 1);
                return (BattlerId)raw;
            }
        }

        public int Wins => PlayerPrefs.GetInt(WinsKey, 0);
        public int Matches => PlayerPrefs.GetInt(MatchesKey, 0);
        public int LongestCombo => PlayerPrefs.GetInt(LongestComboKey, 0);
        public int PerfectClears => PlayerPrefs.GetInt(PerfectClearsKey, 0);

        public void SelectBattler(BattlerId id)
        {
            PlayerPrefs.SetInt(SelectedBattlerKey, (int)id);
            PlayerPrefs.Save();
        }

        public void RecordMatch(BattleMatch match, int longestCombo)
        {
            if (match == null || match.State == BattleMatchState.Running)
                return;

            PlayerPrefs.SetInt(MatchesKey, Matches + 1);

            if (match.State == BattleMatchState.PlayerWon)
                PlayerPrefs.SetInt(WinsKey, Wins + 1);

            if (longestCombo > LongestCombo)
                PlayerPrefs.SetInt(LongestComboKey, longestCombo);

            if (match.Player.Game.IsVictory())
                PlayerPrefs.SetInt(PerfectClearsKey, PerfectClears + 1);

            PlayerPrefs.Save();
        }
    }
}
