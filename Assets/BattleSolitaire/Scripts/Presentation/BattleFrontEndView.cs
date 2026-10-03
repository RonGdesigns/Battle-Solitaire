using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleFrontEndView : MonoBehaviour
    {
        private BattleGameController _controller;
        private readonly Button[] _battlerButtons = new Button[BattlerCatalog.Count];
        private readonly Text[] _battlerNames = new Text[BattlerCatalog.Count];
        private readonly GameObject[] _activeMarkers = new GameObject[BattlerCatalog.Count];
        private readonly Button[] _difficultyButtons = new Button[3];
        private readonly GameObject[] _difficultyMarkers = new GameObject[3];
        private readonly Text[] _statValues = new Text[4];
        private Text _activeName, _activeTitle, _activeQuote, _passiveName, _passiveDescription, _record;
        private AspectFillRawImage _activePortrait;
        private Outline _heroBorder;
        private Button _battleButton;
        private static readonly Color32 Border = new Color32(104, 126, 148, 255);
        public bool IsOpen => gameObject.activeSelf;

        public static BattleFrontEndView Create(Transform parent, BattleGameController controller)
        {
            RectTransform root = PrototypeUI.CreateRect("BattleFrontEnd", parent,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<Image>().color = PrototypeUI.Background;
            var view = root.gameObject.AddComponent<BattleFrontEndView>();
            view._controller = controller;
            view.BuildHeader();
            view.BuildHero();
            view.BuildBattlerPicker();
            view.BuildLoadout();
            view.BuildCareerAndBattle();
            view.ConfigureNavigation();
            view.Refresh();
            root.gameObject.SetActive(false);
            return view;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_battleButton.gameObject);
        }

        public void Hide() => gameObject.SetActive(false);

        public void Refresh()
        {
            BattlerDefinition active = _controller.PlayerBattler;
            if (active == null) return;
            _activeName.text = active.Name.ToUpperInvariant();
            _activeTitle.text = active.Title;
            _activeQuote.text = "“" + active.Quote + "”";
            string[] trait = active.TraitLine.Split(new[] { '•' }, 2);
            _passiveName.text = trait[0].Trim();
            _passiveDescription.text = trait.Length > 1 ? trait[1].Trim() : "";
            _heroBorder.effectColor = Color.Lerp(active.Accent, PrototypeUI.Gold, 0.35f);
            _activePortrait.SetTexture(GameArt.GetBattlerPortrait(active.Id));

            for (int i = 0; i < _battlerButtons.Length; i++)
            {
                BattlerDefinition battler = BattlerCatalog.GetByIndex(i);
                bool selected = battler.Id == active.Id;
                SetButtonStyle(_battlerButtons[i], selected
                    ? Color.Lerp(PrototypeUI.PanelAlt, battler.Accent, 0.24f) : PrototypeUI.PanelAlt,
                    selected ? battler.Accent : Border);
                _battlerNames[i].color = selected ? PrototypeUI.Gold : PrototypeUI.TextLight;
                _activeMarkers[i].SetActive(selected);
            }
            BattleProfile profile = _controller.Profile;
            for (int i = 0; i < _difficultyButtons.Length; i++)
            {
                bool selected = i == (int)profile.Difficulty;
                SetButtonStyle(_difficultyButtons[i], selected
                    ? new Color32(16, 56, 77, 255) : PrototypeUI.PanelAlt,
                    selected ? PrototypeUI.Accent : Border);
                _difficultyButtons[i].GetComponentInChildren<Text>().color =
                    selected ? PrototypeUI.Accent : PrototypeUI.TextLight;
                _difficultyMarkers[i].SetActive(selected);
            }
            _statValues[0].text = profile.Wins.ToString();
            _statValues[1].text = "x" + profile.LongestCombo;
            _statValues[2].text = profile.PerfectClears.ToString();
            _statValues[3].text = profile.GetMasteryLevel(active.Id).ToString();
            _record.text = profile.GetRankName() + " • " + profile.RankPoints + " RP";
        }

        private void BuildHeader()
        {
            Label("BattleLogo", transform, "BATTLE", 39, PrototypeUI.Gold,
                0.055f, 0.943f, 0.37f, 0.980f, true);
            Label("SolitaireLogo", transform, "SOLITAIRE", 39, PrototypeUI.TextLight,
                0.055f, 0.919f, 0.39f, 0.953f, true);
            Label("Motto", transform, "SKILL PLAYS. HIGHER STAKES.", 20, PrototypeUI.TextMuted,
                0.055f, 0.894f, 0.53f, 0.923f, true);
            BuildCrest();
            Label("DifficultyLabel", transform, "RIVAL AI", 22, PrototypeUI.Gold,
                0.545f, 0.951f, 0.945f, 0.98f, true);
            string[] names = { "CASUAL", "STANDARD", "EXPERT" };
            for (int i = 0; i < names.Length; i++)
            {
                int option = i;
                float[] left = { 0.53f, 0.662f, 0.822f };
                float[] right = { 0.652f, 0.812f, 0.945f };
                float x = left[i];
                Button button = Button("Difficulty_" + names[i], transform, names[i],
                    x, 0.905f, right[i], 0.950f, 20);
                button.onClick.AddListener(() => _controller.SelectDifficulty((BattleDifficulty)option));
                _difficultyButtons[i] = button;
                Image marker = Panel("SelectedDifficulty", button.transform, 0.15f, 0.09f, 0.85f, 0.13f);
                marker.color = PrototypeUI.Accent;
                _difficultyMarkers[i] = marker.gameObject;
            }
        }

        private void BuildCrest()
        {
            Texture texture = GameArt.GetCrest();
            if (texture == null) return;
            RectTransform rect = PrototypeUI.CreateRect("BattleCrest", transform,
                new Vector2(0.365f, 0.946f), new Vector2(0.425f, 0.98f), Vector2.zero, Vector2.zero);
            RawImage crest = rect.gameObject.AddComponent<RawImage>();
            crest.texture = texture;
            crest.raycastTarget = false;
            // The supplied square asset contains clipped logo text below the emblem.
            // Frame the emblem region without resampling or changing the source asset.
            crest.uvRect = new Rect(0.14f, 0.56f, 0.50f, 0.44f);
            AspectRatioFitter ratio = rect.gameObject.AddComponent<AspectRatioFitter>();
            ratio.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            ratio.aspectRatio = texture.width * 0.50f / (texture.height * 0.44f);
        }

        private void BuildHero()
        {
            Image hero = Panel("ActiveBattler", transform, 0.055f, 0.60f, 0.945f, 0.88f);
            _heroBorder = PrototypeUI.AddOutline(hero, PrototypeUI.GoldDim, 2f);
            _activePortrait = Portrait("ActivePortrait", hero.transform, null,
                0.61f, 0.065f, 0.965f, 0.94f, 0.8f);
            Label("ActiveLabel", hero.transform, "ACTIVE BATTLER", 23, PrototypeUI.Gold,
                0.045f, 0.84f, 0.58f, 0.95f, true);
            _activeName = Label("ActiveName", hero.transform, "", 62, PrototypeUI.TextLight,
                0.045f, 0.64f, 0.59f, 0.84f, true);
            _activeTitle = Label("ActiveTitle", hero.transform, "", 28, PrototypeUI.Accent,
                0.045f, 0.55f, 0.59f, 0.66f);
            _activeQuote = Label("ActiveQuote", hero.transform, "", 29, PrototypeUI.TextLight,
                0.045f, 0.30f, 0.56f, 0.53f);
            _passiveName = Label("PassiveName", hero.transform, "", 23, PrototypeUI.Gold,
                0.045f, 0.20f, 0.58f, 0.29f, true);
            _passiveDescription = Label("PassiveDescription", hero.transform, "", 24, PrototypeUI.TextMuted,
                0.045f, 0.045f, 0.57f, 0.20f);
        }

        private void BuildBattlerPicker()
        {
            Label("ChooseLabel", transform, "CHOOSE YOUR BATTLER", 25, PrototypeUI.Gold,
                0.055f, 0.568f, 0.945f, 0.597f, true);
            for (int i = 0; i < BattlerCatalog.Count; i++)
            {
                BattlerDefinition battler = BattlerCatalog.GetByIndex(i);
                float x = 0.055f + i * 0.305f;
                Button button = Button("Battler_" + battler.Name, transform, "",
                    x, 0.39f, x + 0.28f, 0.558f, 28);
                button.onClick.AddListener(() => _controller.SelectBattler(battler.Id));
                Portrait("Portrait", button.transform, GameArt.GetBattlerPortrait(battler.Id),
                    0.055f, 0.25f, 0.945f, 0.97f, 0.8f);
                Text name = Label("Name", button.transform, battler.Name.ToUpperInvariant(), 28,
                    PrototypeUI.TextLight, 0.03f, 0.015f, 0.97f, 0.245f, true, TextAnchor.MiddleCenter);
                Image marker = Panel("ActiveMarker", button.transform, 0.26f, 0.27f, 0.74f, 0.38f);
                marker.color = PrototypeUI.DeepNavy;
                Label("Active", marker.transform, "ACTIVE", 20, PrototypeUI.Gold,
                    0, 0, 1, 1, true, TextAnchor.MiddleCenter);
                _battlerButtons[i] = button;
                _battlerNames[i] = name;
                _activeMarkers[i] = marker.gameObject;
            }
        }

        private void BuildLoadout()
        {
            Label("LoadoutLabel", transform, "BATTLE LOADOUT", 25, PrototypeUI.Gold,
                0.055f, 0.351f, 0.945f, 0.382f, true);
            string[] names = { "LOCK", "FOG", "BLOCK" };
            string[] costs = { "25 ENERGY", "25 ENERGY", "50 ENERGY" };
            for (int i = 0; i < names.Length; i++)
            {
                float x = 0.055f + i * 0.305f;
                Image tile = Panel("Loadout_" + names[i], transform, x, 0.252f, x + 0.28f, 0.345f);
                PrototypeUI.AddOutline(tile, Border, 1f);
                Label("Ability", tile.transform, names[i], 28, PrototypeUI.TextLight,
                    0.07f, 0.57f, 0.93f, 0.93f, true, TextAnchor.MiddleCenter);
                Label("Cost", tile.transform, costs[i], 22, PrototypeUI.Accent,
                    0.07f, 0.32f, 0.93f, 0.58f, true, TextAnchor.MiddleCenter);
                BuildAbilityIcon(tile.transform, i);
            }
        }

        // Simple geometric ability symbols avoid missing platform font glyphs.
        private static void BuildAbilityIcon(Transform parent, int ability)
        {
            RectTransform icon = PrototypeUI.CreateRect("Icon", parent,
                new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f),
                new Vector2(-23, -17), new Vector2(23, 17));
            if (ability == 0)
            {
                Image body = Panel("LockBody", icon, 0.15f, 0, 0.85f, 0.62f);
                body.color = PrototypeUI.Accent;
                Image shackle = Panel("Shackle", icon, 0.3f, 0.5f, 0.7f, 1f);
                shackle.color = PrototypeUI.Accent;
                Image opening = Panel("Opening", shackle.transform, 0.2f, 0, 0.8f, 0.75f);
                opening.color = PrototypeUI.Panel;
            }
            else if (ability == 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    Image line = Panel("FogLine", icon, i == 1 ? 0.15f : 0, i * 0.36f,
                        i == 1 ? 0.85f : 1, i * 0.36f + 0.12f);
                    line.color = PrototypeUI.Accent;
                }
            }
            else
            {
                Image wall = Panel("Block", icon, 0.12f, 0, 0.88f, 1);
                wall.color = PrototypeUI.Accent;
                Image center = Panel("Inset", wall.transform, 0.13f, 0.14f, 0.87f, 0.86f);
                center.color = PrototypeUI.Panel;
            }
        }

        private void BuildCareerAndBattle()
        {
            Image career = Panel("Career", transform, 0.055f, 0.07f, 0.49f, 0.23f);
            Label("CareerLabel", career.transform, "CAREER", 24, PrototypeUI.Gold,
                0.06f, 0.79f, 0.94f, 0.96f, true);
            string[] names = { "WINS", "LONGEST COMBO", "PERFECT CLEARS", "MASTERY LV" };
            for (int i = 0; i < names.Length; i++)
            {
                float y = 0.62f - i * 0.14f;
                Label("StatLabel", career.transform, names[i], 22, PrototypeUI.TextMuted,
                    0.06f, y, 0.79f, y + 0.14f);
                _statValues[i] = Label("StatValue", career.transform, "", 25, PrototypeUI.TextLight,
                    0.8f, y, 0.94f, y + 0.14f, true, TextAnchor.MiddleRight);
            }
            _record = Label("Rank", career.transform, "", 22, PrototypeUI.Gold,
                0.06f, 0.025f, 0.94f, 0.18f, true);
            _battleButton = Button("BattleButton", transform, "BATTLE",
                0.52f, 0.111f, 0.945f, 0.23f, 49);
            SetButtonStyle(_battleButton, new Color32(177, 37, 58, 255), PrototypeUI.Gold);
            _battleButton.gameObject.AddComponent<MenuGradient>();
            Shadow glow = _battleButton.gameObject.AddComponent<Shadow>();
            glow.effectColor = new Color(0.75f, 0.13f, 0.22f, 0.18f);
            glow.effectDistance = new Vector2(0, -9);
            _battleButton.onClick.AddListener(_controller.StartBattleFromMenu);
            Label("BattleTagline", transform, "SOLVE. STRIKE. ASCEND.", 22, PrototypeUI.TextMuted,
                0.52f, 0.073f, 0.945f, 0.107f, true, TextAnchor.MiddleCenter);
        }

        private void ConfigureNavigation()
        {
            // Keep navigation inside the visible menu, away from the covered battle HUD.
            for (int i = 0; i < 3; i++)
            {
                Navigation difficulty = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnLeft = _difficultyButtons[(i + 2) % 3],
                    selectOnRight = _difficultyButtons[(i + 1) % 3], selectOnDown = _battlerButtons[i],
                    selectOnUp = _battleButton };
                _difficultyButtons[i].navigation = difficulty;
                Navigation battler = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnLeft = _battlerButtons[(i + 2) % 3],
                    selectOnRight = _battlerButtons[(i + 1) % 3], selectOnUp = _difficultyButtons[i],
                    selectOnDown = _battleButton };
                _battlerButtons[i].navigation = battler;
            }
            _battleButton.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnUp = _battlerButtons[1], selectOnDown = _difficultyButtons[1] };
        }

        private static void SetButtonStyle(Button button, Color fill, Color border)
        {
            Image image = button.GetComponent<Image>();
            image.color = fill;
            PrototypeUI.AddOutline(image, border, 2f);
            // ColorBlock is a multiplier. Use neutral tints instead of multiplying the fill by itself.
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.18f, 1.18f);
            colors.selectedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.pressedColor = new Color(0.72f, 0.72f, 0.72f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
        }

        private static Image Panel(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            Image image = PrototypeUI.CreatePanel(name, parent, new Vector2(x0, y0),
                new Vector2(x1, y1), Vector2.zero, Vector2.zero, PrototypeUI.Panel);
            image.raycastTarget = false;
            return image;
        }

        private static Button Button(string name, Transform parent, string label,
            float x0, float y0, float x1, float y1, int fontSize)
        {
            Button button = PrototypeUI.CreateButton(name, parent, label, PrototypeUI.PanelAlt, PrototypeUI.TextLight);
            Box((RectTransform)button.transform, x0, y0, x1, y1);
            button.GetComponentInChildren<Text>().fontSize = fontSize;
            SetButtonStyle(button, PrototypeUI.PanelAlt, Border);
            return button;
        }

        private static Text Label(string name, Transform parent, string value, int size, Color color,
            float x0, float y0, float x1, float y1, bool bold = false, TextAnchor align = TextAnchor.MiddleLeft)
        {
            Text text = PrototypeUI.CreateText(name, parent, value, size, align, color,
                bold ? FontStyle.Bold : FontStyle.Normal);
            Box(text.rectTransform, x0, y0, x1, y1);
            return text;
        }

        private static AspectFillRawImage Portrait(string name, Transform parent, Texture texture,
            float x0, float y0, float x1, float y1, float aspect)
        {
            RectTransform frame = PrototypeUI.CreateRect(name + "Frame", parent,
                new Vector2(x0, y0), new Vector2(x1, y1), Vector2.zero, Vector2.zero);
            // The fallback remains behind real art and appears only if a resource fails to load.
            Label("PortraitFallback", frame, "PORTRAIT\nUNAVAILABLE", 20, PrototypeUI.TextMuted,
                0, 0, 1, 1, false, TextAnchor.MiddleCenter);
            RectTransform rect = PrototypeUI.CreateRect(name, frame, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.raycastTarget = false;
            AspectRatioFitter ratio = rect.gameObject.AddComponent<AspectRatioFitter>();
            ratio.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            ratio.aspectRatio = aspect;
            AspectFillRawImage fill = rect.gameObject.AddComponent<AspectFillRawImage>();
            fill.SetTexture(texture);
            frame.Find("PortraitFallback").gameObject.SetActive(texture == null);
            // Hero art changes later in Refresh, so its fallback is kept behind the image.
            if (name == "ActivePortrait") frame.Find("PortraitFallback").gameObject.SetActive(true);
            return fill;
        }

        private static void Box(RectTransform rect, float x0, float y0, float x1, float y1)
            => PrototypeUI.SetAnchoredBox(rect, new Vector2(x0, y0), new Vector2(x1, y1), Vector2.zero, Vector2.zero);
    }
}
