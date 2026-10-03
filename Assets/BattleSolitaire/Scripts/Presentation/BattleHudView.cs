using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleHudView : MonoBehaviour
    {
        private BattleGameController _controller;

        private Text _opponentText;
        private Slider _opponentHealth;
        private Slider _opponentProgress;

        private Text _playerText;
        private Slider _playerHealth;
        private Slider _energy;
        private Slider _combo;

        private Text _comboText;
        private Text _message;

        private Button _lockButton;
        private Button _fogButton;
        private Button _blockerButton;

        private GameObject _resultPanel;
        private Text _resultText;

        public static BattleHudView Create(
            Transform parent,
            BattleGameController controller)
        {
            RectTransform root = PrototypeUI.CreateRect(
                "BattleHUD",
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            var view = root.gameObject.AddComponent<BattleHudView>();
            view._controller = controller;
            view.Build();
            view.Refresh();

            return view;
        }

        public void Refresh()
        {
            if (_controller.Match == null)
                return;

            BattleParticipant player = _controller.Match.Player;
            BattleParticipant opponent = _controller.Match.Opponent;

            _opponentText.text =
                "RIVAL   HP " + opponent.Health +
                "   SHIELD " + opponent.Shield +
                "   BOARD " + Mathf.RoundToInt(opponent.ClearPercentage * 100f) + "%";

            _opponentHealth.value =
                opponent.Health / (float)BattleTuning.MaxHealth;

            _opponentProgress.value = opponent.ClearPercentage;

            _playerText.text =
                "YOU   HP " + player.Health +
                "   SHIELD " + player.Shield +
                "   ENERGY " + player.Energy + "/100";

            _playerHealth.value =
                player.Health / (float)BattleTuning.MaxHealth;

            _energy.value =
                player.Energy / (float)BattleTuning.MaxEnergy;

            _combo.value = player.Combo.Strength01;

            _comboText.text = player.Combo.Count > 0
                ? "COMBO x" + player.Combo.Count
                : "COMBO READY";

            _message.text = _controller.VisibleMessage;

            bool running =
                _controller.Match.State == BattleMatchState.Running;

            _lockButton.interactable =
                running && player.Energy >= BattleTuning.LockCost;

            _fogButton.interactable =
                running && player.Energy >= BattleTuning.FogCost;

            _blockerButton.interactable =
                running && player.Energy >= BattleTuning.BlockerCost;

            if (running)
            {
                _resultPanel.SetActive(false);
            }
            else
            {
                _resultPanel.SetActive(true);

                switch (_controller.Match.State)
                {
                    case BattleMatchState.PlayerWon:
                        _resultText.text = "VICTORY";
                        break;
                    case BattleMatchState.OpponentWon:
                        _resultText.text = "DEFEAT";
                        break;
                    default:
                        _resultText.text = "DRAW";
                        break;
                }
            }
        }

        private void Build()
        {
            // Opponent strip.
            Image top = PrototypeUI.CreatePanel(
                "OpponentPanel",
                transform,
                new Vector2(0f, 0.82f),
                new Vector2(1f, 1f),
                new Vector2(18f, 12f),
                new Vector2(-18f, -18f),
                PrototypeUI.Panel);

            Text title = PrototypeUI.CreateText(
                "Title",
                top.transform,
                "BATTLE SOLITAIRE",
                38,
                TextAnchor.UpperCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                title.rectTransform,
                new Vector2(0f, 0.63f),
                new Vector2(1f, 1f),
                new Vector2(12f, 0f),
                new Vector2(-12f, -8f));

            _opponentText = PrototypeUI.CreateText(
                "OpponentStats",
                top.transform,
                "",
                24,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _opponentText.rectTransform,
                new Vector2(0.04f, 0.32f),
                new Vector2(0.96f, 0.62f),
                Vector2.zero,
                Vector2.zero);

            _opponentHealth = PrototypeUI.CreateBar(
                "OpponentHP",
                top.transform,
                PrototypeUI.Danger);

            PrototypeUI.SetAnchoredBox(
                _opponentHealth.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.18f),
                new Vector2(0.54f, 0.28f),
                Vector2.zero,
                Vector2.zero);

            _opponentProgress = PrototypeUI.CreateBar(
                "OpponentProgress",
                top.transform,
                PrototypeUI.Accent);

            PrototypeUI.SetAnchoredBox(
                _opponentProgress.GetComponent<RectTransform>(),
                new Vector2(0.58f, 0.18f),
                new Vector2(0.96f, 0.28f),
                Vector2.zero,
                Vector2.zero);

            // Player strip.
            Image bottom = PrototypeUI.CreatePanel(
                "PlayerPanel",
                transform,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.18f),
                new Vector2(18f, 18f),
                new Vector2(-18f, -10f),
                PrototypeUI.Panel);

            _playerText = PrototypeUI.CreateText(
                "PlayerStats",
                bottom.transform,
                "",
                23,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _playerText.rectTransform,
                new Vector2(0.04f, 0.71f),
                new Vector2(0.96f, 0.96f),
                Vector2.zero,
                Vector2.zero);

            _playerHealth = PrototypeUI.CreateBar(
                "PlayerHP",
                bottom.transform,
                PrototypeUI.Danger);

            PrototypeUI.SetAnchoredBox(
                _playerHealth.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.62f),
                new Vector2(0.34f, 0.69f),
                Vector2.zero,
                Vector2.zero);

            _energy = PrototypeUI.CreateBar(
                "Energy",
                bottom.transform,
                PrototypeUI.Accent);

            PrototypeUI.SetAnchoredBox(
                _energy.GetComponent<RectTransform>(),
                new Vector2(0.38f, 0.62f),
                new Vector2(0.70f, 0.69f),
                Vector2.zero,
                Vector2.zero);

            _combo = PrototypeUI.CreateBar(
                "Combo",
                bottom.transform,
                PrototypeUI.Gold);

            PrototypeUI.SetAnchoredBox(
                _combo.GetComponent<RectTransform>(),
                new Vector2(0.74f, 0.62f),
                new Vector2(0.96f, 0.69f),
                Vector2.zero,
                Vector2.zero);

            _comboText = PrototypeUI.CreateText(
                "ComboText",
                bottom.transform,
                "",
                20,
                TextAnchor.MiddleRight,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _comboText.rectTransform,
                new Vector2(0.70f, 0.70f),
                new Vector2(0.96f, 0.96f),
                Vector2.zero,
                Vector2.zero);

            _lockButton = PrototypeUI.CreateButton(
                "LockButton",
                bottom.transform,
                "LOCK 25",
                new Color32(102, 74, 170, 255),
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                _lockButton.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.12f),
                new Vector2(0.31f, 0.51f),
                Vector2.zero,
                Vector2.zero);

            _lockButton.onClick.AddListener(
                () => _controller.UsePlayerAttack(BattleAttackType.Lock));

            _fogButton = PrototypeUI.CreateButton(
                "FogButton",
                bottom.transform,
                "FOG 25",
                new Color32(74, 93, 126, 255),
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                _fogButton.GetComponent<RectTransform>(),
                new Vector2(0.365f, 0.12f),
                new Vector2(0.635f, 0.51f),
                Vector2.zero,
                Vector2.zero);

            _fogButton.onClick.AddListener(
                () => _controller.UsePlayerAttack(BattleAttackType.Fog));

            _blockerButton = PrototypeUI.CreateButton(
                "BlockerButton",
                bottom.transform,
                "BLOCK 50",
                new Color32(171, 112, 47, 255),
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                _blockerButton.GetComponent<RectTransform>(),
                new Vector2(0.69f, 0.12f),
                new Vector2(0.96f, 0.51f),
                Vector2.zero,
                Vector2.zero);

            _blockerButton.onClick.AddListener(
                () => _controller.UsePlayerAttack(BattleAttackType.Blocker));

            _message = PrototypeUI.CreateText(
                "Message",
                transform,
                "",
                25,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _message.rectTransform,
                new Vector2(0.08f, 0.785f),
                new Vector2(0.92f, 0.825f),
                Vector2.zero,
                Vector2.zero);

            // Match result overlay.
            Image result = PrototypeUI.CreatePanel(
                "ResultPanel",
                transform,
                new Vector2(0.15f, 0.36f),
                new Vector2(0.85f, 0.66f),
                Vector2.zero,
                Vector2.zero,
                new Color32(16, 19, 28, 248));

            _resultPanel = result.gameObject;

            _resultText = PrototypeUI.CreateText(
                "ResultText",
                result.transform,
                "VICTORY",
                58,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _resultText.rectTransform,
                new Vector2(0.05f, 0.45f),
                new Vector2(0.95f, 0.92f),
                Vector2.zero,
                Vector2.zero);

            Button rematch = PrototypeUI.CreateButton(
                "RematchButton",
                result.transform,
                "REMATCH",
                PrototypeUI.Accent,
                PrototypeUI.TextDark);

            PrototypeUI.SetAnchoredBox(
                rematch.GetComponent<RectTransform>(),
                new Vector2(0.20f, 0.12f),
                new Vector2(0.80f, 0.40f),
                Vector2.zero,
                Vector2.zero);

            rematch.onClick.AddListener(_controller.StartRematch);

            _resultPanel.SetActive(false);
        }
    }
}
