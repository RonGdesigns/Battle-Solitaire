using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class TutorialOverlay : MonoBehaviour
    {
        private const string TutorialKey = "BattleSolitaire.TutorialSeen.v1";

        private readonly string[] _titles =
        {
            "SOLITAIRE IS YOUR WEAPON",
            "BUILD PRESSURE",
            "SPEND ENERGY",
            "TWO WAYS TO WIN"
        };

        private readonly string[] _bodies =
        {
            "Move cards just like Klondike: alternate colors and build downward. Tap a card then its destination, or drag it. Double-tap an exposed card to try the foundation.",
            "Good solitaire play powers the battle. Reveals, foundation moves, cleared columns and combos generate extra energy. Foundation progress also builds shield and damages your rival.",
            "LOCK disables a rival column. FOG slows the rival AI and hides information. BLOCK obstructs a column until enough progress moves clear it. Save energy or spend it early.",
            "Drop your rival to 0 HP, or complete all four foundations for a Perfect Clear. Your rival is solving its own board at the same time, so keep moving."
        };

        private BattleGameController _controller;
        private Text _stepText;
        private Text _title;
        private Text _body;
        private Button _next;
        private Text _nextLabel;
        private int _step;

        public bool IsOpen => gameObject.activeSelf;

        public static TutorialOverlay Create(
            Transform parent,
            BattleGameController controller)
        {
            RectTransform root = PrototypeUI.CreateRect(
                "TutorialOverlay",
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color32(8, 10, 16, 238);

            var overlay = root.gameObject.AddComponent<TutorialOverlay>();
            overlay._controller = controller;
            overlay.Build();
            root.gameObject.SetActive(false);

            return overlay;
        }

        public void OpenIfNeeded()
        {
            if (PlayerPrefs.GetInt(TutorialKey, 0) == 0)
                Open();
        }

        public void Open()
        {
            _step = 0;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            RefreshStep();
        }

        public void CloseAndRemember()
        {
            PlayerPrefs.SetInt(TutorialKey, 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
            _controller.ShowMessage("Tutorial complete. Good luck.");
        }

        private void Next()
        {
            if (_step >= _titles.Length - 1)
            {
                CloseAndRemember();
                return;
            }

            _step++;
            RefreshStep();
        }

        private void RefreshStep()
        {
            _stepText.text =
                "QUICK START  " + (_step + 1) + "/" + _titles.Length;

            _title.text = _titles[_step];
            _body.text = _bodies[_step];
            _nextLabel.text =
                _step == _titles.Length - 1 ? "PLAY" : "NEXT";
        }

        private void Build()
        {
            Image card = PrototypeUI.CreatePanel(
                "TutorialCard",
                transform,
                new Vector2(0.08f, 0.24f),
                new Vector2(0.92f, 0.76f),
                Vector2.zero,
                Vector2.zero,
                PrototypeUI.Panel);

            _stepText = PrototypeUI.CreateText(
                "Step",
                card.transform,
                "",
                21,
                TextAnchor.MiddleLeft,
                PrototypeUI.Accent,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _stepText.rectTransform,
                new Vector2(0.08f, 0.82f),
                new Vector2(0.92f, 0.94f),
                Vector2.zero,
                Vector2.zero);

            _title = PrototypeUI.CreateText(
                "TutorialTitle",
                card.transform,
                "",
                36,
                TextAnchor.MiddleLeft,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _title.rectTransform,
                new Vector2(0.08f, 0.62f),
                new Vector2(0.92f, 0.82f),
                Vector2.zero,
                Vector2.zero);

            _body = PrototypeUI.CreateText(
                "TutorialBody",
                card.transform,
                "",
                27,
                TextAnchor.UpperLeft,
                new Color32(219, 226, 236, 255));

            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;

            PrototypeUI.SetAnchoredBox(
                _body.rectTransform,
                new Vector2(0.08f, 0.30f),
                new Vector2(0.92f, 0.62f),
                Vector2.zero,
                Vector2.zero);

            _next = PrototypeUI.CreateButton(
                "Next",
                card.transform,
                "NEXT",
                PrototypeUI.Accent,
                PrototypeUI.TextDark);

            PrototypeUI.SetAnchoredBox(
                _next.GetComponent<RectTransform>(),
                new Vector2(0.54f, 0.08f),
                new Vector2(0.92f, 0.23f),
                Vector2.zero,
                Vector2.zero);

            _nextLabel = _next.GetComponentInChildren<Text>();
            _next.onClick.AddListener(Next);

            Button skip = PrototypeUI.CreateButton(
                "Skip",
                card.transform,
                "SKIP",
                PrototypeUI.PanelAlt,
                PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                skip.GetComponent<RectTransform>(),
                new Vector2(0.08f, 0.08f),
                new Vector2(0.44f, 0.23f),
                Vector2.zero,
                Vector2.zero);

            skip.onClick.AddListener(CloseAndRemember);
        }
    }
}
