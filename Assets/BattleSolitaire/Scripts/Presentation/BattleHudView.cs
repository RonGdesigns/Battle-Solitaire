using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleHudView : MonoBehaviour
    {
        private BattleGameController _controller;
        private Text _opponentIdentity;
        private RawImage _opponentPortrait;
        private Text _opponentText;
        private Slider _opponentHealth;
        private Slider _opponentProgress;
        private Text _playerIdentity;
        private RawImage _playerPortrait;
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

        private float _targetOpponentHealth;
        private float _targetOpponentProgress;
        private float _targetPlayerHealth;
        private float _targetEnergy;
        private float _targetCombo;

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

        private void Update()
        {
            const float speed = 4.8f;

            _opponentHealth.value = Mathf.MoveTowards(
                _opponentHealth.value,
                _targetOpponentHealth,
                speed * Time.unscaledDeltaTime);

            _opponentProgress.value = Mathf.MoveTowards(
                _opponentProgress.value,
                _targetOpponentProgress,
                speed * Time.unscaledDeltaTime);

            _playerHealth.value = Mathf.MoveTowards(
                _playerHealth.value,
                _targetPlayerHealth,
                speed * Time.unscaledDeltaTime);

            _energy.value = Mathf.MoveTowards(
                _energy.value,
                _targetEnergy,
                speed * Time.unscaledDeltaTime);

            _combo.value = Mathf.MoveTowards(
                _combo.value,
                _targetCombo,
                speed * Time.unscaledDeltaTime);
        }

        public void Refresh()
        {
            if (_controller.Match == null)
                return;

            BattleParticipant player = _controller.Match.Player;
            BattleParticipant opponent = _controller.Match.Opponent;
            BattlerDefinition playerBattler = _controller.PlayerBattler;
            BattlerDefinition opponentBattler = _controller.OpponentBattler;

            _opponentIdentity.text =
                opponentBattler.Name.ToUpperInvariant() +
                "  •  " +
                opponentBattler.Title;

            _opponentIdentity.color = opponentBattler.Accent;

            if (_opponentPortrait != null)
            {
                _opponentPortrait.texture =
                    GameArt.GetBattlerPortrait(
                        opponentBattler.Id);

                _opponentPortrait.enabled =
                    _opponentPortrait.texture != null;
            }

            _opponentText.text =
                "HP " + opponent.Health +
                "   SHIELD " + opponent.Shield +
                "   BOARD " +
                Mathf.RoundToInt(opponent.ClearPercentage * 100f) +
                "%";

            _targetOpponentHealth =
                opponent.Health / (float)BattleTuning.MaxHealth;

            _targetOpponentProgress = opponent.ClearPercentage;

            _playerIdentity.text =
                playerBattler.Name.ToUpperInvariant() +
                "  •  " +
                playerBattler.Title;

            _playerIdentity.color = playerBattler.Accent;

            if (_playerPortrait != null)
            {
                _playerPortrait.texture =
                    GameArt.GetBattlerPortrait(
                        playerBattler.Id);

                _playerPortrait.enabled =
                    _playerPortrait.texture != null;
            }

            _playerText.text =
                "HP " + player.Health +
                "   SHIELD " + player.Shield +
                "   ENERGY " + player.Energy + "/100";

            _targetPlayerHealth =
                player.Health / (float)BattleTuning.MaxHealth;

            _targetEnergy =
                player.Energy / (float)BattleTuning.MaxEnergy;

            _targetCombo = player.Combo.Strength01;

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
            Image top = PrototypeUI.CreatePanel(
                "OpponentPanel",
                transform,
                new Vector2(0f, 0.82f),
                new Vector2(1f, 1f),
                new Vector2(18f, 12f),
                new Vector2(-18f, -18f),
                PrototypeUI.Panel);

            PrototypeUI.AddOutline(top, PrototypeUI.GoldDim, 1f);

            Text title = PrototypeUI.CreateText(
                "Title",
                top.transform,
                "BATTLE SOLITAIRE",
                31,
                TextAnchor.UpperLeft,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                title.rectTransform,
                new Vector2(0.04f, 0.70f),
                new Vector2(0.62f, 0.96f),
                Vector2.zero,
                Vector2.zero);

            RectTransform opponentPortraitRect =
                PrototypeUI.CreateRect(
                    "OpponentPortrait",
                    top.transform,
                    new Vector2(0.76f, 0.40f),
                    new Vector2(0.88f, 0.94f),
                    Vector2.zero,
                    Vector2.zero);

            _opponentPortrait =
                opponentPortraitRect.gameObject
                    .AddComponent<RawImage>();

            _opponentPortrait.gameObject.AddComponent<AspectFillRawImage>();
            _opponentPortrait.raycastTarget = false;
            _opponentPortrait.color = Color.white;

            PrototypeUI.AddOutline(
                _opponentPortrait,
                PrototypeUI.GoldDim,
                1f);

            _opponentIdentity = PrototypeUI.CreateText(
                "OpponentIdentity",
                top.transform,
                "",
                24,
                TextAnchor.MiddleLeft,
                PrototypeUI.Danger,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _opponentIdentity.rectTransform,
                new Vector2(0.04f, 0.43f),
                new Vector2(0.74f, 0.70f),
                Vector2.zero,
                Vector2.zero);

            Button help = PrototypeUI.CreateButton(
                "Help",
                top.transform,
                "?",
                PrototypeUI.PanelAlt,
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                help.GetComponent<RectTransform>(),
                new Vector2(0.90f, 0.69f),
                new Vector2(0.97f, 0.94f),
                Vector2.zero,
                Vector2.zero);

            help.onClick.AddListener(_controller.ShowTutorial);

            _opponentText = PrototypeUI.CreateText(
                "OpponentStats",
                top.transform,
                "",
                21,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _opponentText.rectTransform,
                new Vector2(0.04f, 0.25f),
                new Vector2(0.74f, 0.45f),
                Vector2.zero,
                Vector2.zero);

            _opponentHealth = PrototypeUI.CreateBar(
                "OpponentHP",
                top.transform,
                PrototypeUI.Danger);

            PrototypeUI.SetAnchoredBox(
                _opponentHealth.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.12f),
                new Vector2(0.54f, 0.22f),
                Vector2.zero,
                Vector2.zero);

            _opponentProgress = PrototypeUI.CreateBar(
                "OpponentProgress",
                top.transform,
                PrototypeUI.Accent);

            PrototypeUI.SetAnchoredBox(
                _opponentProgress.GetComponent<RectTransform>(),
                new Vector2(0.58f, 0.12f),
                new Vector2(0.96f, 0.22f),
                Vector2.zero,
                Vector2.zero);

            Image bottom = PrototypeUI.CreatePanel(
                "PlayerPanel",
                transform,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.18f),
                new Vector2(18f, 18f),
                new Vector2(-18f, -10f),
                PrototypeUI.Panel);

            PrototypeUI.AddOutline(bottom, new Color32(47, 104, 142, 180), 1f);

            RectTransform playerPortraitRect =
                PrototypeUI.CreateRect(
                    "PlayerPortrait",
                    bottom.transform,
                    new Vector2(0.82f, 0.63f),
                    new Vector2(0.94f, 0.96f),
                    Vector2.zero,
                    Vector2.zero);

            _playerPortrait =
                playerPortraitRect.gameObject
                    .AddComponent<RawImage>();

            _playerPortrait.gameObject.AddComponent<AspectFillRawImage>();
            _playerPortrait.raycastTarget = false;
            _playerPortrait.color = Color.white;

            PrototypeUI.AddOutline(
                _playerPortrait,
                new Color32(47, 104, 142, 180),
                1f);

            _playerIdentity = PrototypeUI.CreateText(
                "PlayerIdentity",
                bottom.transform,
                "",
                21,
                TextAnchor.MiddleLeft,
                PrototypeUI.Accent,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _playerIdentity.rectTransform,
                new Vector2(0.04f, 0.79f),
                new Vector2(0.78f, 0.98f),
                Vector2.zero,
                Vector2.zero);

            _playerText = PrototypeUI.CreateText(
                "PlayerStats",
                bottom.transform,
                "",
                20,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _playerText.rectTransform,
                new Vector2(0.04f, 0.64f),
                new Vector2(0.78f, 0.81f),
                Vector2.zero,
                Vector2.zero);

            _playerHealth = PrototypeUI.CreateBar(
                "PlayerHP",
                bottom.transform,
                PrototypeUI.Danger);

            PrototypeUI.SetAnchoredBox(
                _playerHealth.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.54f),
                new Vector2(0.31f, 0.61f),
                Vector2.zero,
                Vector2.zero);

            _energy = PrototypeUI.CreateBar(
                "Energy",
                bottom.transform,
                PrototypeUI.Accent);

            PrototypeUI.SetAnchoredBox(
                _energy.GetComponent<RectTransform>(),
                new Vector2(0.35f, 0.54f),
                new Vector2(0.68f, 0.61f),
                Vector2.zero,
                Vector2.zero);

            _combo = PrototypeUI.CreateBar(
                "Combo",
                bottom.transform,
                PrototypeUI.Gold);

            PrototypeUI.SetAnchoredBox(
                _combo.GetComponent<RectTransform>(),
                new Vector2(0.72f, 0.54f),
                new Vector2(0.96f, 0.61f),
                Vector2.zero,
                Vector2.zero);

            _comboText = PrototypeUI.CreateText(
                "ComboText",
                bottom.transform,
                "",
                19,
                TextAnchor.MiddleRight,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _comboText.rectTransform,
                new Vector2(0.61f, 0.66f),
                new Vector2(0.80f, 0.94f),
                Vector2.zero,
                Vector2.zero);

            _lockButton = PrototypeUI.CreateButton(
                "LockButton",
                bottom.transform,
                "LOCK\n25",
                new Color32(22, 63, 93, 255),
                PrototypeUI.TextLight);

            _fogButton = PrototypeUI.CreateButton(
                "FogButton",
                bottom.transform,
                "FOG\n25",
                new Color32(28, 55, 83, 255),
                PrototypeUI.TextLight);

            _blockerButton = PrototypeUI.CreateButton(
                "BlockerButton",
                bottom.transform,
                "BLOCK\n50",
                new Color32(75, 54, 31, 255),
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                _lockButton.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.08f),
                new Vector2(0.31f, 0.45f),
                Vector2.zero,
                Vector2.zero);

            PrototypeUI.SetAnchoredBox(
                _fogButton.GetComponent<RectTransform>(),
                new Vector2(0.365f, 0.08f),
                new Vector2(0.635f, 0.45f),
                Vector2.zero,
                Vector2.zero);

            PrototypeUI.SetAnchoredBox(
                _blockerButton.GetComponent<RectTransform>(),
                new Vector2(0.69f, 0.08f),
                new Vector2(0.96f, 0.45f),
                Vector2.zero,
                Vector2.zero);

            _lockButton.onClick.AddListener(
                () => _controller.UsePlayerAttack(BattleAttackType.Lock));
            _fogButton.onClick.AddListener(
                () => _controller.UsePlayerAttack(BattleAttackType.Fog));
            _blockerButton.onClick.AddListener(
                () => _controller.UsePlayerAttack(BattleAttackType.Blocker));

            _message = PrototypeUI.CreateText(
                "Message",
                transform,
                "",
                24,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _message.rectTransform,
                new Vector2(0.08f, 0.785f),
                new Vector2(0.92f, 0.825f),
                Vector2.zero,
                Vector2.zero);

            Image result = PrototypeUI.CreatePanel(
                "ResultPanel",
                transform,
                new Vector2(0.15f, 0.36f),
                new Vector2(0.85f, 0.66f),
                Vector2.zero,
                Vector2.zero,
                new Color32(8, 16, 27, 250));

            PrototypeUI.AddOutline(result, PrototypeUI.GoldDim, 2f);
            _resultPanel = result.gameObject;

            _resultText = PrototypeUI.CreateText(
                "ResultText",
                result.transform,
                "VICTORY",
                58,
                TextAnchor.MiddleCenter,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _resultText.rectTransform,
                new Vector2(0.05f, 0.52f),
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
                new Vector2(0.08f, 0.12f),
                new Vector2(0.47f, 0.40f),
                Vector2.zero,
                Vector2.zero);

            rematch.onClick.AddListener(_controller.StartRematch);

            Button loadout = PrototypeUI.CreateButton(
                "LoadoutButton",
                result.transform,
                "LOADOUT",
                PrototypeUI.PanelAlt,
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                loadout.GetComponent<RectTransform>(),
                new Vector2(0.53f, 0.12f),
                new Vector2(0.92f, 0.40f),
                Vector2.zero,
                Vector2.zero);

            loadout.onClick.AddListener(_controller.ShowFrontEnd);
            _resultPanel.SetActive(false);
        }
    }
}
