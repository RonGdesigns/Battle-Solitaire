using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleFrontEndView : MonoBehaviour
    {
        private BattleGameController _controller;
        private readonly Button[] _battlerButtons = new Button[BattlerCatalog.Count];
        private readonly Text[] _battlerNames = new Text[BattlerCatalog.Count];
        private readonly Text[] _battlerGlyphs = new Text[BattlerCatalog.Count];

        private Text _activeName;
        private Text _activeTitle;
        private Text _activeQuote;
        private Text _activeTraits;
        private Text _statsText;
        private Text _recordText;
        private Image _activeAccent;

        public bool IsOpen => gameObject.activeSelf;

        public static BattleFrontEndView Create(
            Transform parent,
            BattleGameController controller)
        {
            RectTransform root = PrototypeUI.CreateRect(
                "BattleFrontEnd",
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            Image background = root.gameObject.AddComponent<Image>();
            background.color = PrototypeUI.Background;

            var view = root.gameObject.AddComponent<BattleFrontEndView>();
            view._controller = controller;
            view.Build();
            view.Refresh();
            root.gameObject.SetActive(false);
            return view;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            BattlerDefinition active = _controller.PlayerBattler;
            if (active == null)
                return;

            _activeName.text = active.Name.ToUpperInvariant();
            _activeTitle.text = active.Title;
            _activeQuote.text = "“" + active.Quote + "”";
            _activeTraits.text = active.TraitLine;
            _activeAccent.color = active.Accent;

            for (int i = 0; i < BattlerCatalog.Count; i++)
            {
                BattlerDefinition battler = BattlerCatalog.GetByIndex(i);
                bool selected = battler.Id == active.Id;
                Image image = _battlerButtons[i].GetComponent<Image>();

                image.color = selected
                    ? Color.Lerp(PrototypeUI.PanelAlt, battler.Accent, 0.28f)
                    : PrototypeUI.PanelAlt;

                _battlerNames[i].color = selected
                    ? PrototypeUI.Gold
                    : PrototypeUI.TextLight;

                _battlerGlyphs[i].color = selected
                    ? battler.Accent
                    : PrototypeUI.TextMuted;
            }

            BattleProfile profile = _controller.Profile;

            _statsText.text =
                "WINS  " + profile.Wins +
                "\nLONGEST COMBO  x" + profile.LongestCombo +
                "\nPERFECT CLEARS  " + profile.PerfectClears;

            int losses = Mathf.Max(0, profile.Matches - profile.Wins);

            _recordText.text = profile.Matches == 0
                ? "LOCAL RECORD  •  NEW CHALLENGER"
                : "LOCAL RECORD  •  " + profile.Wins + "W  " + losses + "L";
        }

        private void Build()
        {
            Text logo = PrototypeUI.CreateText(
                "Logo",
                transform,
                "BATTLE\nSOLITAIRE",
                54,
                TextAnchor.UpperLeft,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                logo.rectTransform,
                new Vector2(0.055f, 0.82f),
                new Vector2(0.55f, 0.97f),
                Vector2.zero,
                Vector2.zero);

            Text motto = PrototypeUI.CreateText(
                "Motto",
                transform,
                "SKILL PLAYS.  HIGHER STAKES.",
                20,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextMuted,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                motto.rectTransform,
                new Vector2(0.055f, 0.785f),
                new Vector2(0.72f, 0.825f),
                Vector2.zero,
                Vector2.zero);

            Image hero = PrototypeUI.CreatePanel(
                "ActiveBattler",
                transform,
                new Vector2(0.055f, 0.52f),
                new Vector2(0.945f, 0.775f),
                Vector2.zero,
                Vector2.zero,
                PrototypeUI.Panel);

            PrototypeUI.AddOutline(hero, PrototypeUI.GoldDim, 2f);

            _activeAccent = PrototypeUI.CreatePanel(
                "ActiveAccent",
                hero.transform,
                new Vector2(0f, 0f),
                new Vector2(0.022f, 1f),
                Vector2.zero,
                Vector2.zero,
                PrototypeUI.Danger);

            Text activeLabel = PrototypeUI.CreateText(
                "ActiveLabel",
                hero.transform,
                "ACTIVE BATTLER",
                19,
                TextAnchor.MiddleLeft,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                activeLabel.rectTransform,
                new Vector2(0.07f, 0.78f),
                new Vector2(0.45f, 0.94f),
                Vector2.zero,
                Vector2.zero);

            _activeName = PrototypeUI.CreateText(
                "ActiveName",
                hero.transform,
                "",
                44,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _activeName.rectTransform,
                new Vector2(0.07f, 0.52f),
                new Vector2(0.64f, 0.80f),
                Vector2.zero,
                Vector2.zero);

            _activeTitle = PrototypeUI.CreateText(
                "ActiveTitle",
                hero.transform,
                "",
                25,
                TextAnchor.MiddleLeft,
                PrototypeUI.Accent);

            PrototypeUI.SetAnchoredBox(
                _activeTitle.rectTransform,
                new Vector2(0.07f, 0.39f),
                new Vector2(0.70f, 0.57f),
                Vector2.zero,
                Vector2.zero);

            _activeQuote = PrototypeUI.CreateText(
                "ActiveQuote",
                hero.transform,
                "",
                23,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextMuted);

            PrototypeUI.SetAnchoredBox(
                _activeQuote.rectTransform,
                new Vector2(0.07f, 0.19f),
                new Vector2(0.86f, 0.40f),
                Vector2.zero,
                Vector2.zero);

            _activeTraits = PrototypeUI.CreateText(
                "ActiveTraits",
                hero.transform,
                "",
                18,
                TextAnchor.MiddleLeft,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _activeTraits.rectTransform,
                new Vector2(0.07f, 0.04f),
                new Vector2(0.90f, 0.20f),
                Vector2.zero,
                Vector2.zero);

            Text choose = PrototypeUI.CreateText(
                "ChooseLabel",
                transform,
                "CHOOSE YOUR BATTLER",
                25,
                TextAnchor.MiddleLeft,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                choose.rectTransform,
                new Vector2(0.055f, 0.475f),
                new Vector2(0.80f, 0.515f),
                Vector2.zero,
                Vector2.zero);

            for (int i = 0; i < BattlerCatalog.Count; i++)
            {
                BattlerDefinition battler = BattlerCatalog.GetByIndex(i);
                float left = 0.055f + i * 0.302f;
                float right = left + 0.282f;

                Button button = PrototypeUI.CreateButton(
                    "Battler_" + battler.Name,
                    transform,
                    "",
                    PrototypeUI.PanelAlt,
                    PrototypeUI.TextLight);

                PrototypeUI.SetAnchoredBox(
                    button.GetComponent<RectTransform>(),
                    new Vector2(left, 0.345f),
                    new Vector2(right, 0.47f),
                    Vector2.zero,
                    Vector2.zero);

                int captured = i;
                button.onClick.AddListener(
                    () => _controller.SelectBattler(
                        BattlerCatalog.GetByIndex(captured).Id));

                Text glyph = PrototypeUI.CreateText(
                    "Glyph",
                    button.transform,
                    battler.SuitGlyph,
                    48,
                    TextAnchor.UpperCenter,
                    battler.Accent,
                    FontStyle.Bold);

                PrototypeUI.SetAnchoredBox(
                    glyph.rectTransform,
                    new Vector2(0.04f, 0.35f),
                    new Vector2(0.96f, 0.96f),
                    Vector2.zero,
                    Vector2.zero);

                Text name = PrototypeUI.CreateText(
                    "Name",
                    button.transform,
                    battler.Name.ToUpperInvariant(),
                    22,
                    TextAnchor.LowerCenter,
                    PrototypeUI.TextLight,
                    FontStyle.Bold);

                PrototypeUI.SetAnchoredBox(
                    name.rectTransform,
                    new Vector2(0.04f, 0.08f),
                    new Vector2(0.96f, 0.40f),
                    Vector2.zero,
                    Vector2.zero);

                _battlerButtons[i] = button;
                _battlerNames[i] = name;
                _battlerGlyphs[i] = glyph;
            }

            Image loadout = PrototypeUI.CreatePanel(
                "Loadout",
                transform,
                new Vector2(0.055f, 0.205f),
                new Vector2(0.945f, 0.33f),
                Vector2.zero,
                Vector2.zero,
                PrototypeUI.Panel);

            PrototypeUI.AddOutline(loadout, new Color32(66, 87, 112, 210), 1f);

            Text loadoutLabel = PrototypeUI.CreateText(
                "LoadoutLabel",
                loadout.transform,
                "BATTLE LOADOUT",
                21,
                TextAnchor.UpperLeft,
                PrototypeUI.Gold,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                loadoutLabel.rectTransform,
                new Vector2(0.04f, 0.68f),
                new Vector2(0.52f, 0.96f),
                Vector2.zero,
                Vector2.zero);

            string[] abilityNames = { "LOCK 25", "FOG 25", "BLOCK 50" };
            string[] abilityGlyphs = { "[ ]", "~~~", "<>" };

            for (int i = 0; i < 3; i++)
            {
                float left = 0.04f + i * 0.322f;
                float right = left + 0.29f;

                Image ability = PrototypeUI.CreatePanel(
                    "Loadout_" + i,
                    loadout.transform,
                    new Vector2(left, 0.10f),
                    new Vector2(right, 0.64f),
                    Vector2.zero,
                    Vector2.zero,
                    new Color32(20, 49, 77, 255));

                PrototypeUI.AddOutline(ability, new Color32(45, 149, 207, 220), 1f);

                PrototypeUI.CreateText(
                    "Ability",
                    ability.transform,
                    abilityGlyphs[i] + "  " + abilityNames[i],
                    20,
                    TextAnchor.MiddleCenter,
                    PrototypeUI.TextLight,
                    FontStyle.Bold);
            }

            Image stats = PrototypeUI.CreatePanel(
                "Stats",
                transform,
                new Vector2(0.055f, 0.065f),
                new Vector2(0.49f, 0.19f),
                Vector2.zero,
                Vector2.zero,
                PrototypeUI.Panel);

            _statsText = PrototypeUI.CreateText(
                "StatsText",
                stats.transform,
                "",
                20,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _statsText.rectTransform,
                new Vector2(0.07f, 0.08f),
                new Vector2(0.93f, 0.92f),
                Vector2.zero,
                Vector2.zero);

            Button battleButton = PrototypeUI.CreateButton(
                "BattleButton",
                transform,
                "BATTLE",
                new Color32(119, 25, 35, 255),
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                battleButton.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0.095f),
                new Vector2(0.945f, 0.19f),
                Vector2.zero,
                Vector2.zero);

            PrototypeUI.AddOutline(
                battleButton.GetComponent<Image>(),
                new Color32(238, 70, 78, 245),
                2f);

            battleButton.onClick.AddListener(_controller.StartBattleFromMenu);

            _recordText = PrototypeUI.CreateText(
                "Record",
                transform,
                "",
                18,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextMuted,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _recordText.rectTransform,
                new Vector2(0.52f, 0.055f),
                new Vector2(0.945f, 0.09f),
                Vector2.zero,
                Vector2.zero);
        }
    }
}
