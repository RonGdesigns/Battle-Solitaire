using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class TargetingOverlay : MonoBehaviour
    {
        private BattleGameController _controller;
        private BattleAttackType _attackType;
        private Text _title;
        private readonly Button[] _columnButtons =
            new Button[7];

        public bool IsOpen =>
            gameObject.activeSelf;

        public static TargetingOverlay Create(
            Transform parent,
            BattleGameController controller)
        {
            RectTransform root =
                PrototypeUI.CreateRect(
                    "TargetingOverlay",
                    parent,
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    Vector2.zero);

            Image dim =
                root.gameObject
                    .AddComponent<Image>();

            dim.color =
                new Color32(
                    4, 9, 17, 232);

            var overlay =
                root.gameObject
                    .AddComponent<TargetingOverlay>();

            overlay._controller =
                controller;

            overlay.Build();

            root.gameObject
                .SetActive(false);

            return overlay;
        }

        public void Open(
            BattleAttackType attackType)
        {
            _attackType = attackType;

            _title.text =
                "CHOOSE RIVAL COLUMN  •  " +
                attackType
                    .ToString()
                    .ToUpperInvariant();

            RefreshButtons();

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void RefreshButtons()
        {
            BattleMatch match =
                _controller.Match;

            for (int column = 0;
                 column < 7;
                 column++)
            {
                Button button =
                    _columnButtons[column];

                bool targetable =
                    match != null &&
                    match.State ==
                        BattleMatchState.Running &&
                    match.CanUseColumn(
                        BattleSide.Opponent,
                        column);

                int cardCount =
                    match == null
                        ? 0
                        : match.Opponent.Game
                            .Tableau[column]
                            .Count;

                int hiddenCount = 0;

                if (match != null)
                {
                    var cards =
                        match.Opponent.Game
                            .Tableau[column];

                    for (int i = 0;
                         i < cards.Count;
                         i++)
                    {
                        if (!cards[i].IsFaceUp)
                            hiddenCount++;
                    }
                }

                button.interactable =
                    targetable;

                Text label =
                    button.GetComponentInChildren<Text>();

                label.text =
                    (column + 1) +
                    "\n" +
                    cardCount +
                    " CARDS" +
                    (hiddenCount > 0
                        ? "\n" +
                          hiddenCount +
                          " HIDDEN"
                        : "");

                Image image =
                    button.GetComponent<Image>();

                image.color =
                    targetable
                        ? new Color32(
                            22, 63, 93, 255)
                        : new Color32(
                            34, 38, 47, 210);
            }
        }

        private void SelectColumn(
            int column)
        {
            _controller.ExecuteTargetedAttack(
                _attackType,
                column);
        }

        private void Build()
        {
            Image panel =
                PrototypeUI.CreatePanel(
                    "TargetPanel",
                    transform,
                    new Vector2(
                        0.06f,
                        0.33f),
                    new Vector2(
                        0.94f,
                        0.67f),
                    Vector2.zero,
                    Vector2.zero,
                    PrototypeUI.Panel);

            PrototypeUI.AddOutline(
                panel,
                PrototypeUI.GoldDim,
                2f);

            _title =
                PrototypeUI.CreateText(
                    "Title",
                    panel.transform,
                    "",
                    30,
                    TextAnchor.MiddleCenter,
                    PrototypeUI.Gold,
                    FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _title.rectTransform,
                new Vector2(
                    0.04f,
                    0.72f),
                new Vector2(
                    0.96f,
                    0.94f),
                Vector2.zero,
                Vector2.zero);

            Text help =
                PrototypeUI.CreateText(
                    "Help",
                    panel.transform,
                    "Target a column based on its card count and hidden-card depth.",
                    19,
                    TextAnchor.MiddleCenter,
                    PrototypeUI.TextMuted);

            PrototypeUI.SetAnchoredBox(
                help.rectTransform,
                new Vector2(
                    0.05f,
                    0.61f),
                new Vector2(
                    0.95f,
                    0.75f),
                Vector2.zero,
                Vector2.zero);

            for (int column = 0;
                 column < 7;
                 column++)
            {
                float left =
                    0.035f +
                    (column * 0.136f);

                float right =
                    left + 0.116f;

                Button button =
                    PrototypeUI.CreateButton(
                        "Target_" + column,
                        panel.transform,
                        (column + 1).ToString(),
                        new Color32(
                            22, 63, 93, 255),
                        PrototypeUI.TextLight);

                PrototypeUI.SetAnchoredBox(
                    button.GetComponent<RectTransform>(),
                    new Vector2(
                        left,
                        0.24f),
                    new Vector2(
                        right,
                        0.58f),
                    Vector2.zero,
                    Vector2.zero);

                Text label =
                    button.GetComponentInChildren<Text>();

                label.fontSize = 17;

                int captured =
                    column;

                button.onClick.AddListener(
                    () => SelectColumn(
                        captured));

                _columnButtons[column] =
                    button;
            }

            Button cancel =
                PrototypeUI.CreateButton(
                    "Cancel",
                    panel.transform,
                    "CANCEL",
                    PrototypeUI.PanelAlt,
                    PrototypeUI.TextLight);

            PrototypeUI.SetAnchoredBox(
                cancel.GetComponent<RectTransform>(),
                new Vector2(
                    0.32f,
                    0.05f),
                new Vector2(
                    0.68f,
                    0.18f),
                Vector2.zero,
                Vector2.zero);

            cancel.onClick.AddListener(
                Close);
        }
    }
}
