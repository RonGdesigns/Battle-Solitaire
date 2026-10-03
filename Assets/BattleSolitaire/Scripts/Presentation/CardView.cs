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
        private Text _label;
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

            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
            shadow.effectDistance = new Vector2(4f, -5f);

            _label = PrototypeUI.CreateText(
                "CardLabel",
                transform,
                "",
                28,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextDark,
                FontStyle.Bold);

            _label.rectTransform.offsetMin = new Vector2(5f, 5f);
            _label.rectTransform.offsetMax = new Vector2(-5f, -5f);

            transform.localScale = new Vector3(0.96f, 0.96f, 1f);
            ApplyVisuals();
        }

        private void Update()
        {
            float desired =
                _dragging ? 1.08f : (_selected ? 1.055f : _targetScale);

            float next = Mathf.Lerp(
                transform.localScale.x,
                desired,
                1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));

            transform.localScale = new Vector3(next, next, 1f);
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
            _canvasGroup.alpha = 0.93f;
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
                _label.color = PrototypeUI.TextLight;
                _label.text = "BS";
                return;
            }

            if (_fogged)
            {
                _background.color = new Color32(65, 68, 79, 255);
                _label.color = PrototypeUI.TextLight;
                _label.text = "??";
                return;
            }

            _background.color = _selected
                ? new Color32(255, 226, 130, 255)
                : PrototypeUI.CardFace;

            _label.color = Card.IsRed
                ? new Color32(180, 45, 55, 255)
                : PrototypeUI.TextDark;

            _label.text = RankLabel(Card.Rank) + "\n" + SuitLabel(Card.Suit);
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

        private static string SuitLabel(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs: return "C";
                case Suit.Diamonds: return "D";
                case Suit.Hearts: return "H";
                case Suit.Spades: return "S";
                default: return "?";
            }
        }
    }
}
