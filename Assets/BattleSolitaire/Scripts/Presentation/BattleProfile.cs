using BattleSolitaire.Battle;
using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleProfile
    {
        private const string SelectedBattlerKey =
            "BattleSolitaire.Profile.SelectedBattler";

        private const string WinsKey =
            "BattleSolitaire.Profile.Wins";

        private const string MatchesKey =
            "BattleSolitaire.Profile.Matches";

        private const string LongestComboKey =
            "BattleSolitaire.Profile.LongestCombo";

        private const string PerfectClearsKey =
            "BattleSolitaire.Profile.PerfectClears";

        private const string RankPointsKey =
            "BattleSolitaire.Profile.RankPoints";

        private const string DifficultyKey =
            "BattleSolitaire.Profile.Difficulty";

        public BattlerId SelectedBattler
        {
            get
            {
                int raw =
                    PlayerPrefs.GetInt(
                        SelectedBattlerKey,
                        (int)BattlerId.Kael);

                raw = Mathf.Clamp(
                    raw,
                    0,
                    BattlerCatalog.Count - 1);

                return (BattlerId)raw;
            }
        }

        public int Wins =>
            PlayerPrefs.GetInt(WinsKey, 0);

        public int Matches =>
            PlayerPrefs.GetInt(MatchesKey, 0);

        public int LongestCombo =>
            PlayerPrefs.GetInt(
                LongestComboKey,
                0);

        public int PerfectClears =>
            PlayerPrefs.GetInt(
                PerfectClearsKey,
                0);

        public int RankPoints =>
            PlayerPrefs.GetInt(
                RankPointsKey,
                0);

        public BattleSolitaire.Battle.BattleDifficulty Difficulty
        {
            get
            {
                int raw = PlayerPrefs.GetInt(
                    DifficultyKey,
                    (int)BattleSolitaire.Battle.BattleDifficulty.Standard);

                raw = Mathf.Clamp(raw, 0, 2);

                return
                    (BattleSolitaire.Battle.BattleDifficulty)raw;
            }
        }

        public void SelectBattler(
            BattlerId id)
        {
            PlayerPrefs.SetInt(
                SelectedBattlerKey,
                (int)id);

            PlayerPrefs.Save();
        }

        public void SetDifficulty(
            BattleSolitaire.Battle.BattleDifficulty difficulty)
        {
            PlayerPrefs.SetInt(
                DifficultyKey,
                (int)difficulty);

            PlayerPrefs.Save();
        }

        public int GetMasteryXp(
            BattlerId id)
        {
            return PlayerPrefs.GetInt(
                MasteryKey(id),
                0);
        }

        public int GetMasteryLevel(
            BattlerId id)
        {
            return 1 +
                (GetMasteryXp(id) / 250);
        }

        public string GetRankName()
        {
            int rp = RankPoints;

            if (rp < 200)
                return "BRONZE";

            if (rp < 450)
                return "SILVER";

            if (rp < 800)
                return "GOLD";

            if (rp < 1200)
                return "PLATINUM";

            return "DIAMOND";
        }

        public void RecordMatch(
            BattleMatch match,
            int longestCombo,
            BattlerId battlerId)
        {
            if (match == null ||
                match.State ==
                    BattleMatchState.Running)
            {
                return;
            }

            bool perfectClear =
                match.Player.Game.IsVictory();

            PlayerPrefs.SetInt(
                MatchesKey,
                Matches + 1);

            int rpDelta;

            switch (match.State)
            {
                case BattleMatchState.PlayerWon:
                    PlayerPrefs.SetInt(
                        WinsKey,
                        Wins + 1);

                    rpDelta = 30;
                    break;

                case BattleMatchState.Draw:
                    rpDelta = 8;
                    break;

                default:
                    rpDelta = -12;
                    break;
            }

            PlayerPrefs.SetInt(
                RankPointsKey,
                Mathf.Max(
                    0,
                    RankPoints + rpDelta));

            if (longestCombo >
                LongestCombo)
            {
                PlayerPrefs.SetInt(
                    LongestComboKey,
                    longestCombo);
            }

            if (perfectClear)
            {
                PlayerPrefs.SetInt(
                    PerfectClearsKey,
                    PerfectClears + 1);
            }

            int masteryGain =
                60 +
                (match.State ==
                    BattleMatchState.PlayerWon
                    ? 40
                    : 0) +
                Mathf.Min(
                    40,
                    longestCombo * 2) +
                (perfectClear ? 25 : 0);

            PlayerPrefs.SetInt(
                MasteryKey(battlerId),
                GetMasteryXp(battlerId) +
                masteryGain);

            PlayerPrefs.Save();
        }

        private static string MasteryKey(
            BattlerId id)
        {
            return
                "BattleSolitaire.Profile.Mastery." +
                id;
        }
    }
}
