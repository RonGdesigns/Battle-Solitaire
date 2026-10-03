using System;
using BattleSolitaire.Battle;
using BattleSolitaire.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleGameController : MonoBehaviour
    {
        private static BattleGameController _instance;

        private BattleAIController _aiController;
        private SolitaireMoveSolver _aiSolver;
        private BattleBoardView _board;
        private BattleHudView _hud;

        private int _matchCounter;
        private float _statusRefreshTimer;
        private string _message = "";
        private float _messageUntil;

        public BattleMatch Match { get; private set; }

        public string VisibleMessage =>
            Time.unscaledTime <= _messageUntil ? _message : "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (_instance != null)
                return;

            BattleGameController existing =
                FindFirstObjectByType<BattleGameController>();

            if (existing != null)
                return;

            var go = new GameObject("Battle Solitaire Runtime");
            go.AddComponent<BattleGameController>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            StartRematch();
            CreateRuntimeUI();

            _board.Refresh();
            _hud.Refresh();
        }

        private void Update()
        {
            if (Match == null)
                return;

            float delta = Time.deltaTime;
            Match.Tick(delta);

            if (Match.State == BattleMatchState.Running)
            {
                BattleAIIntent intent = _aiController.Tick(delta, Match);

                if (intent.WantsMove)
                {
                    SolitaireAIAction action =
                        _aiSolver.TryAct(Match, BattleSide.Opponent);

                    if (action.Move.Success)
                    {
                        Match.RegisterMove(
                            BattleSide.Opponent,
                            action.Move);
                    }
                }

                if (Match.State == BattleMatchState.Running &&
                    intent.WantsAttack)
                {
                    bool attacked = Match.TryAttack(
                        BattleSide.Opponent,
                        intent.AttackType,
                        intent.TargetColumn);

                    if (attacked)
                    {
                        ShowMessage(
                            "Rival used " +
                            intent.AttackType.ToString().ToUpperInvariant() +
                            "!");
                        _board.RefreshStatus();
                    }
                }
            }

            _statusRefreshTimer -= delta;

            if (_statusRefreshTimer <= 0f)
            {
                _statusRefreshTimer = 0.1f;
                _board.RefreshStatus();
            }

            _hud.Refresh();
        }

        public void StartRematch()
        {
            _matchCounter++;

            int baseSeed = unchecked(
                Environment.TickCount +
                (_matchCounter * 104729));

            int opponentSeed = baseSeed ^ 1597463007;

            Match = new BattleMatch(baseSeed, opponentSeed);
            _aiController = new BattleAIController(opponentSeed ^ 173);
            _aiSolver = new SolitaireMoveSolver();

            ShowMessage("Battle started.");

            if (_board != null)
                _board.Refresh();

            if (_hud != null)
                _hud.Refresh();
        }

        public void PlayerDraw()
        {
            if (!CanPlayerAct())
                return;

            bool changed = Match.Player.Game.DrawCard();

            if (!changed)
            {
                ShowMessage("No cards left to draw.");
                return;
            }

            _board.Refresh();
        }

        public bool PlayerMoveWasteToTableau(int destinationColumn)
        {
            if (!CanUsePlayerColumn(destinationColumn))
                return false;

            MoveResult move =
                Match.Player.Game.MoveWasteToTableau(destinationColumn);

            return FinishPlayerMove(move);
        }

        public bool PlayerMoveWasteToFoundation()
        {
            if (!CanPlayerAct())
                return false;

            MoveResult move =
                Match.Player.Game.MoveWasteToFoundation();

            return FinishPlayerMove(move);
        }

        public bool PlayerMoveTableauToFoundation(int sourceColumn)
        {
            if (!CanUsePlayerColumn(sourceColumn))
                return false;

            MoveResult move =
                Match.Player.Game.MoveTableauToFoundation(sourceColumn);

            return FinishPlayerMove(move);
        }

        public bool PlayerMoveTableauToTableau(
            int sourceColumn,
            int startIndex,
            int destinationColumn)
        {
            if (!CanUsePlayerColumn(sourceColumn) ||
                !CanUsePlayerColumn(destinationColumn))
            {
                return false;
            }

            MoveResult move =
                Match.Player.Game.MoveTableauToTableau(
                    sourceColumn,
                    startIndex,
                    destinationColumn);

            return FinishPlayerMove(move);
        }

        public bool PlayerMoveFoundationToTableau(
            Suit sourceSuit,
            int destinationColumn)
        {
            if (!CanUsePlayerColumn(destinationColumn))
                return false;

            MoveResult move =
                Match.Player.Game.MoveFoundationToTableau(
                    sourceSuit,
                    destinationColumn);

            return FinishPlayerMove(move);
        }

        public void UsePlayerAttack(BattleAttackType type)
        {
            if (!CanPlayerAct())
                return;

            int targetColumn = -1;

            if (type == BattleAttackType.Lock ||
                type == BattleAttackType.Blocker)
            {
                targetColumn = FindBestOpponentTarget();

                if (targetColumn < 0)
                {
                    ShowMessage("No rival column can be targeted right now.");
                    return;
                }
            }

            bool success = Match.TryAttack(
                BattleSide.Player,
                type,
                targetColumn);

            if (!success)
            {
                ShowMessage("Not enough energy.");
                return;
            }

            ShowMessage(
                type.ToString().ToUpperInvariant() +
                " launched!");

            _hud.Refresh();
        }

        public void ShowMessage(string message)
        {
            _message = message ?? "";
            _messageUntil = Time.unscaledTime + 1.7f;
        }

        private bool FinishPlayerMove(MoveResult move)
        {
            if (!move.Success)
            {
                ShowMessage("That move is not legal.");
                return false;
            }

            BattleMoveOutcome outcome =
                Match.RegisterMove(BattleSide.Player, move);

            if (outcome.Counted)
            {
                string text = "+" + outcome.EnergyGained + " energy";

                if (outcome.DamageDealt > 0)
                    text += "   " + outcome.DamageDealt + " damage";

                if (outcome.ShieldGained > 0)
                    text += "   +" + outcome.ShieldGained + " shield";

                ShowMessage(text);
            }

            _hud.Refresh();
            return true;
        }

        private bool CanPlayerAct()
        {
            return Match != null &&
                   Match.State == BattleMatchState.Running;
        }

        private bool CanUsePlayerColumn(int column)
        {
            if (!CanPlayerAct())
                return false;

            if (!Match.CanUseColumn(BattleSide.Player, column))
            {
                ShowMessage("That column is disrupted.");
                return false;
            }

            return true;
        }

        private int FindBestOpponentTarget()
        {
            int bestColumn = -1;
            int bestCardCount = -1;

            for (int column = 0; column < 7; column++)
            {
                if (!Match.CanUseColumn(BattleSide.Opponent, column))
                    continue;

                int cardCount =
                    Match.Opponent.Game.Tableau[column].Count;

                if (cardCount > bestCardCount)
                {
                    bestCardCount = cardCount;
                    bestColumn = column;
                }
            }

            return bestColumn;
        }

        private void CreateRuntimeUI()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(StandaloneInputModule));

                DontDestroyOnLoad(eventSystem);
            }

            var canvasObject = new GameObject(
                "Battle Solitaire Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            DontDestroyOnLoad(canvasObject);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler =
                canvasObject.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Image background = PrototypeUI.CreatePanel(
                "Background",
                canvasObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                PrototypeUI.Background);

            background.transform.SetAsFirstSibling();

            _board = BattleBoardView.Create(
                canvasObject.transform,
                this);

            _hud = BattleHudView.Create(
                canvasObject.transform,
                this);
        }
    }
}
