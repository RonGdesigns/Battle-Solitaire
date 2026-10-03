using BattleSolitaire.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public enum CardSourceKind
    {
        Tableau,
        Waste,
        Foundation
    }

    public sealed class CardView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public static CardView CurrentDrag { get; private set; }

        private BattleBoardView _board;
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Image _background;
        private Text _topCorner;
        private Text _center;
        private Text _bottomCorner;
        private Image _innerFrame;
        private Vector3 _startPosition;
        private bool _dragging;
        private bool _dropHandled;
        private bool _selected;
        private bool _fogged;
        private float _targetScale = 1f;

        public CardState Card { get; private set; }
        public CardSourceKind SourceKind { get; private set; }
        public int Column { get; private set; }
        public int Index { get; private set; }
        public Suit FoundationSuit { get; private set; }

        public void Initialize(
            BattleBoardView board,
            CardState card,
            CardSourceKind sourceKind,
            int column,
            int index,
            Suit foundationSuit,
            bool fogged)
        {
            _board = board;
            Card = card;
            SourceKind = sourceKind;
            Column = column;
            Index = index;
            FoundationSuit = foundationSuit;
            _fogged = fogged;

            _rect = GetComponent<RectTransform>();
            _background = gameObject.AddComponent<Image>();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            PrototypeUI.AddOutline(
                _background,
                PrototypeUI.GoldDim,
                1.5f);

            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
            shadow.effectDistance = new Vector2(4f, -6f);

            _innerFrame = PrototypeUI.CreatePanel(
                "InnerFrame",
                transform,
                new Vector2(0.06f, 0.05f),
                new Vector2(0.94f, 0.95f),
                Vector2.zero,
                Vector2.zero,
                Color.clear);

            _innerFrame.raycastTarget = false;
            PrototypeUI.AddOutline(
                _innerFrame,
                new Color32(201, 164, 91, 120),
                1f);

            _topCorner = PrototypeUI.CreateText(
                "TopCorner",
                transform,
                "",
                24,
                TextAnchor.UpperLeft,
                PrototypeUI.TextDark,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _topCorner.rectTransform,
                new Vector2(0.08f, 0.63f),
                new Vector2(0.48f, 0.94f),
                Vector2.zero,
                Vector2.zero);

            _center = PrototypeUI.CreateText(
                "Center",
                transform,
                "",
                48,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextDark,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _center.rectTransform,
                new Vector2(0.08f, 0.18f),
                new Vector2(0.92f, 0.82f),
                Vector2.zero,
                Vector2.zero);

            _bottomCorner = PrototypeUI.CreateText(
                "BottomCorner",
                transform,
                "",
                22,
                TextAnchor.LowerRight,
                PrototypeUI.TextDark,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _bottomCorner.rectTransform,
                new Vector2(0.52f, 0.06f),
                new Vector2(0.92f, 0.36f),
                Vector2.zero,
                Vector2.zero);

            transform.localScale =
                new Vector3(0.96f, 0.96f, 1f);

            ApplyVisuals();
        }

        private void Update()
        {
            float desired =
                _dragging
                    ? 1.08f
                    : (_selected ? 1.055f : _targetScale);

            float next = Mathf.Lerp(
                transform.localScale.x,
                desired,
                1f - Mathf.Exp(
                    -18f * Time.unscaledDeltaTime));

            transform.localScale =
                new Vector3(next, next, 1f);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyVisuals();
        }

        public void SetFogged(bool fogged)
        {
            if (_fogged == fogged)
                return;

            _fogged = fogged;
            ApplyVisuals();
        }

        public void MarkDropHandled()
        {
            _dropHandled = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging)
                return;

            _board.CardTapped(this, eventData.clickCount);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_board.CanBeginDrag(this))
                return;

            _dragging = true;
            _dropHandled = false;
            CurrentDrag = this;
            _startPosition = _rect.position;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.94f;
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _rect.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            if (CurrentDrag == this)
                CurrentDrag = null;

            if (!_dropHandled)
            {
                _rect.position = _startPosition;
                _board.Refresh();
            }

            _dropHandled = false;
        }

        private void ApplyVisuals()
        {
            if (Card == null)
                return;

            if (!Card.IsFaceUp)
            {
                _background.color = PrototypeUI.CardBack;
                _innerFrame.color = new Color32(8, 25, 49, 255);

                _topCorner.text = "";
                _bottomCorner.text = "";
                _center.text = "♠\nBS";
                _center.fontSize = 34;
                _center.color = PrototypeUI.Gold;
                return;
            }

            if (_fogged)
            {
                _background.color = new Color32(49, 55, 68, 255);
                _innerFrame.color = new Color32(37, 43, 55, 255);

                _topCorner.text = "";
                _bottomCorner.text = "";
                _center.text = "??";
                _center.fontSize = 44;
                _center.color = PrototypeUI.TextLight;
                return;
            }

            _background.color = _selected
                ? new Color32(255, 233, 170, 255)
                : PrototypeUI.CardFace;

            _innerFrame.color = new Color32(248, 244, 233, 255);

            Color ink = Card.IsRed
                ? new Color32(178, 43, 55, 255)
                : new Color32(18, 31, 49, 255);

            string rank = RankLabel(Card.Rank);
            string suit = SuitGlyph(Card.Suit);

            _topCorner.text = rank + "\n" + suit;
            _bottomCorner.text = rank + " " + suit;
            _center.text = IsFaceCard(Card.Rank)
                ? rank + "\n" + suit
                : suit;

            _center.fontSize =
                IsFaceCard(Card.Rank) ? 42 : 56;

            _topCorner.color = ink;
            _bottomCorner.color = ink;
            _center.color = ink;
        }

        private static bool IsFaceCard(Rank rank)
        {
            return rank == Rank.Jack ||
                   rank == Rank.Queen ||
                   rank == Rank.King;
        }

        private static string RankLabel(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace: return "A";
                case Rank.Jack: return "J";
                case Rank.Queen: return "Q";
                case Rank.King: return "K";
                default: return ((int)rank).ToString();
            }
        }

        public static string SuitGlyph(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs: return "♣";
                case Suit.Diamonds: return "♦";
                case Suit.Hearts: return "♥";
                case Suit.Spades: return "♠";
                default: return "?";
            }
        }
    }
}
