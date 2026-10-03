using System.Collections.Generic;
using BattleSolitaire.Battle;
using BattleSolitaire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleBoardView : MonoBehaviour
    {
        private const float CardWidth = 128f;
        private const float CardHeight = 178f;

        private BattleGameController _controller;
        private Button _stockButton;
        private Text _stockLabel;
        private RectTransform _wasteRoot;
        private readonly RectTransform[] _foundationRoots = new RectTransform[4];
        private readonly RectTransform[] _columnRoots = new RectTransform[7];
        private readonly Text[] _columnEffects = new Text[7];
        private readonly List<GameObject> _dynamicObjects = new List<GameObject>();
        private readonly List<CardView> _renderedCards = new List<CardView>();

        private CardView _selected;

        public static BattleBoardView Create(
            Transform parent,
            BattleGameController controller)
        {
            RectTransform root = PrototypeUI.CreateRect(
                "BattleBoard",
                parent,
                new Vector2(0f, 0.18f),
                new Vector2(1f, 0.82f),
                new Vector2(18f, 10f),
                new Vector2(-18f, -10f));

            Image background = root.gameObject.AddComponent<Image>();
            background.color = PrototypeUI.Felt;
            PrototypeUI.AddOutline(
                background,
                PrototypeUI.GoldDim,
                1.5f);

            var view = root.gameObject.AddComponent<BattleBoardView>();
            view._controller = controller;
            view.BuildStaticLayout();
            view.Refresh();

            return view;
        }

        public void Refresh()
        {
            for (int i = 0; i < _dynamicObjects.Count; i++)
            {
                if (_dynamicObjects[i] != null)
                    Destroy(_dynamicObjects[i]);
            }

            _dynamicObjects.Clear();
            _renderedCards.Clear();
            _selected = null;

            BattleParticipant player = _controller.Match.Player;
            SolitaireGame game = player.Game;

            _stockLabel.text = game.Stock.Count > 0
                ? "DRAW\n" + game.Stock.Count
                : (game.Waste.Count > 0 ? "RECYCLE" : "EMPTY");

            if (game.Waste.Count > 0)
            {
                CardState waste = game.Waste[game.Waste.Count - 1];
                CreateCard(
                    _wasteRoot,
                    waste,
                    CardSourceKind.Waste,
                    -1,
                    game.Waste.Count - 1,
                    waste.Suit,
                    Vector2.zero);
            }

            for (int i = 0; i < _foundationRoots.Length; i++)
            {
                Suit suit = (Suit)i;
                List<CardState> foundation = game.Foundations[suit];

                if (foundation.Count == 0)
                    continue;

                CreateCard(
                    _foundationRoots[i],
                    foundation[foundation.Count - 1],
                    CardSourceKind.Foundation,
                    -1,
                    foundation.Count - 1,
                    suit,
                    Vector2.zero);
            }

            for (int column = 0; column < game.Tableau.Count; column++)
            {
                List<CardState> cards = game.Tableau[column];
                float y = 0f;

                for (int index = 0; index < cards.Count; index++)
                {
                    CardState card = cards[index];

                    CreateCard(
                        _columnRoots[column],
                        card,
                        CardSourceKind.Tableau,
                        column,
                        index,
                        card.Suit,
                        new Vector2(0f, y));

                    y -= card.IsFaceUp ? 48f : 29f;
                }
            }

            RefreshStatus();
        }

        public void RefreshStatus()
        {
            if (_controller.Match == null)
                return;

            BattleDisruptionState disruption =
                _controller.Match.Player.Disruptions;

            bool fogged = disruption.HasFog;

            for (int i = 0; i < _renderedCards.Count; i++)
                _renderedCards[i].SetFogged(fogged);

            for (int column = 0; column < 7; column++)
            {
                float lockTime = disruption.GetLockTimeRemaining(column);
                int blockerMoves = disruption.GetBlockerMovesRemaining(column);

                if (lockTime > 0f)
                {
                    _columnEffects[column].gameObject.SetActive(true);
                    _columnEffects[column].text =
                        "LOCKED " + lockTime.ToString("0.0") + "s";
                    _columnEffects[column].color = PrototypeUI.Danger;
                }
                else if (blockerMoves > 0)
                {
                    _columnEffects[column].gameObject.SetActive(true);
                    _columnEffects[column].text =
                        "BLOCK " + blockerMoves + " MOVES";
                    _columnEffects[column].color = PrototypeUI.Gold;
                }
                else
                {
                    _columnEffects[column].gameObject.SetActive(false);
                }

                _columnEffects[column].transform.SetAsLastSibling();
            }
        }

        public bool CanBeginDrag(CardView view)
        {
            if (_controller.Match.State != BattleMatchState.Running ||
                view.Card == null ||
                !view.Card.IsFaceUp)
            {
                return false;
            }

            if (view.SourceKind == CardSourceKind.Tableau)
            {
                return _controller.Match.CanUseColumn(
                    BattleSide.Player,
                    view.Column);
            }

            return true;
        }

        public void CardTapped(CardView view, int clickCount)
        {
            if (view == null || !view.Card.IsFaceUp)
                return;

            if (clickCount >= 2)
            {
                if (TryAutoFoundation(view))
                    return;
            }

            if (_selected == null)
            {
                Select(view);
                return;
            }

            if (_selected == view)
            {
                Select(null);
                return;
            }

            if (view.SourceKind == CardSourceKind.Tableau &&
                _selected.SourceKind == CardSourceKind.Tableau &&
                view.Column == _selected.Column)
            {
                Select(view);
                return;
            }

            if (view.SourceKind == CardSourceKind.Tableau)
            {
                if (TryMoveCardToTableau(_selected, view.Column))
                    return;

                Select(view);
                return;
            }

            if (view.SourceKind == CardSourceKind.Foundation &&
                _selected.SourceKind != CardSourceKind.Foundation)
            {
                if (TryMoveCardToFoundation(_selected, view.FoundationSuit))
                    return;
            }

            Select(view);
        }

        public void EmptyTableauTapped(int column)
        {
            if (_selected != null)
                TryMoveCardToTableau(_selected, column);
        }

        public void FoundationSlotTapped(Suit suit)
        {
            if (_selected != null)
                TryMoveCardToFoundation(_selected, suit);
        }

        public bool TryMoveCardToTableau(CardView source, int destinationColumn)
        {
            if (source == null)
                return false;

            bool success = false;

            switch (source.SourceKind)
            {
                case CardSourceKind.Tableau:
                    success = _controller.PlayerMoveTableauToTableau(
                        source.Column,
                        source.Index,
                        destinationColumn);
                    break;

                case CardSourceKind.Waste:
                    success = _controller.PlayerMoveWasteToTableau(
                        destinationColumn);
                    break;

                case CardSourceKind.Foundation:
                    success = _controller.PlayerMoveFoundationToTableau(
                        source.FoundationSuit,
                        destinationColumn);
                    break;
            }

            if (success)
                Refresh();

            return success;
        }

        public bool TryMoveCardToFoundation(CardView source, Suit targetSuit)
        {
            if (source == null ||
                source.Card == null ||
                source.Card.Suit != targetSuit)
            {
                _controller.ShowMessage("That card belongs on a different foundation.");
                return false;
            }

            bool success = false;

            if (source.SourceKind == CardSourceKind.Waste)
            {
                success = _controller.PlayerMoveWasteToFoundation();
            }
            else if (source.SourceKind == CardSourceKind.Tableau)
            {
                List<CardState> column =
                    _controller.Match.Player.Game.Tableau[source.Column];

                if (source.Index != column.Count - 1)
                {
                    _controller.ShowMessage("Only the exposed top card can move to a foundation.");
                    return false;
                }

                success = _controller.PlayerMoveTableauToFoundation(
                    source.Column);
            }

            if (success)
                Refresh();

            return success;
        }

        private bool TryAutoFoundation(CardView source)
        {
            if (source.SourceKind == CardSourceKind.Foundation)
                return false;

            return TryMoveCardToFoundation(source, source.Card.Suit);
        }

        private void Select(CardView view)
        {
            if (_selected != null)
                _selected.SetSelected(false);

            _selected = view;

            if (_selected != null)
                _selected.SetSelected(true);
        }

        private void BuildStaticLayout()
        {
            // Stock
            Image stockPanel = PrototypeUI.CreatePanel(
                "Stock",
                transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -214f),
                new Vector2(152f, -36f),
                PrototypeUI.CardBack);

            _stockButton = stockPanel.gameObject.AddComponent<Button>();
            _stockButton.targetGraphic = stockPanel;
            _stockButton.onClick.AddListener(_controller.PlayerDraw);

            _stockLabel = PrototypeUI.CreateText(
                "StockLabel",
                stockPanel.transform,
                "DRAW",
                24,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            // Waste
            Image wastePanel = PrototypeUI.CreatePanel(
                "Waste",
                transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(174f, -214f),
                new Vector2(302f, -36f),
                PrototypeUI.FeltDark);

            _wasteRoot = wastePanel.rectTransform;

            Text wasteHint = PrototypeUI.CreateText(
                "WasteHint",
                _wasteRoot,
                "WASTE",
                20,
                TextAnchor.MiddleCenter,
                new Color32(160, 190, 186, 255));

            // Foundations
            float foundationStart = 430f;

            for (int i = 0; i < 4; i++)
            {
                Suit suit = (Suit)i;

                Image slot = PrototypeUI.CreatePanel(
                    "Foundation_" + suit,
                    transform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(foundationStart + i * 148f, -214f),
                    new Vector2(foundationStart + i * 148f + CardWidth, -36f),
                    new Color32(12, 53, 45, 255));

                _foundationRoots[i] = slot.rectTransform;

                var drop = slot.gameObject.AddComponent<PileDropTarget>();
                drop.Initialize(this, DropTargetKind.Foundation, -1, suit);

                Text label = PrototypeUI.CreateText(
                    "FoundationHint",
                    slot.transform,
                    CardView.SuitGlyph(suit),
                    28,
                    TextAnchor.MiddleCenter,
                    PrototypeUI.TextMuted,
                    FontStyle.Bold);
            }

            // Tableau
            float startX = 24f;
            float gap = 20f;

            for (int column = 0; column < 7; column++)
            {
                float x = startX + column * (CardWidth + gap);

                Image lane = PrototypeUI.CreatePanel(
                    "Column_" + column,
                    transform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(x, -1020f),
                    new Vector2(x + CardWidth, -250f),
                    new Color32(7, 45, 37, 130));

                _columnRoots[column] = lane.rectTransform;

                var drop = lane.gameObject.AddComponent<PileDropTarget>();
                drop.Initialize(this, DropTargetKind.Tableau, column, Suit.Clubs);

                Text effect = PrototypeUI.CreateText(
                    "Effect",
                    lane.transform,
                    "",
                    17,
                    TextAnchor.UpperCenter,
                    PrototypeUI.Danger,
                    FontStyle.Bold);

                effect.rectTransform.anchorMin = new Vector2(0f, 1f);
                effect.rectTransform.anchorMax = new Vector2(1f, 1f);
                effect.rectTransform.pivot = new Vector2(0.5f, 1f);
                effect.rectTransform.anchoredPosition = new Vector2(0f, 4f);
                effect.rectTransform.sizeDelta = new Vector2(0f, 44f);
                effect.gameObject.SetActive(false);

                _columnEffects[column] = effect;
            }
        }

        private CardView CreateCard(
            RectTransform parent,
            CardState card,
            CardSourceKind source,
            int column,
            int index,
            Suit foundationSuit,
            Vector2 anchoredPosition)
        {
            var go = new GameObject(
                "Card_" + card.Id,
                typeof(RectTransform));

            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(CardWidth, CardHeight);
            rect.anchoredPosition = anchoredPosition;

            var view = go.AddComponent<CardView>();
            view.Initialize(
                this,
                card,
                source,
                column,
                index,
                foundationSuit,
                _controller.Match.Player.Disruptions.HasFog);

            _dynamicObjects.Add(go);
            _renderedCards.Add(view);

            return view;
        }
    }
}
