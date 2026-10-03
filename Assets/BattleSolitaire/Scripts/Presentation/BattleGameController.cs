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
        private BattleFeedback _feedback;
        private BattleFxView _fx;
        private TutorialOverlay _tutorial;
        private BattleFrontEndView _frontEnd;
        private TargetingOverlay _targeting;

        private PauseOverlay _pause;
        private SavedMatch _savedMatch;
        private bool _paused, _backgrounded, _sessionStarted;
        private float _checkpointTimer;
        public bool Paused => _paused;
        public bool HasSavedMatch => _savedMatch != null;
        public string RecoveryNotice { get; private set; } = "";
        public bool SoundEnabled => _feedback.SoundEnabled;
        public bool HapticsEnabled => _feedback.HapticsEnabled;
        public void ToggleSound() => _feedback.ToggleSound();
        public void ToggleHaptics() => _feedback.ToggleHaptics();

        private int _matchCounter;
        private float _statusRefreshTimer;
        private string _message = "";
        private float _messageUntil;
        private BattleMatchState _lastObservedState = BattleMatchState.Running;
        private bool _menuOpen;
        private bool _matchRecorded;
        private int _playerMatchMaxCombo;

        private BattlerDefinition _playerBattler;
        private BattlerDefinition _opponentBattler;

        public BattleMatch Match { get; private set; }
        public BattleProfile Profile { get; private set; }
        public BattlerDefinition PlayerBattler => _playerBattler;
        public BattlerDefinition OpponentBattler => _opponentBattler;

        public string VisibleMessage =>
            Time.unscaledTime <= _messageUntil ? _message : "";

        public bool TutorialOpen =>
            _tutorial != null && _tutorial.IsOpen;

        public bool MenuOpen => _menuOpen;

        public bool TargetingOpen =>
            _targeting != null &&
            _targeting.IsOpen;

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
            Input.multiTouchEnabled = false;
            Screen.orientation = ScreenOrientation.Portrait;

            Profile = new BattleProfile();
            _playerBattler = BattlerCatalog.Get(Profile.SelectedBattler);

            _feedback = gameObject.AddComponent<BattleFeedback>();
            _feedback.Initialize();

            StartRematch();
            _sessionStarted = false;
            _savedMatch = MatchSaveStore.Load(out string recovery);
            RecoveryNotice = recovery;
            CreateRuntimeUI();

            _board.Refresh();
            _hud.Refresh();
            ShowFrontEnd();
        }

        private void Update()
        {
            if (Match == null)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_pause != null && _pause.IsOpen) ResumeBattle();
                else if (TargetingOpen) _targeting.Close();
                else OpenPause();
            }
            if (_paused || _backgrounded || _menuOpen ||
                TutorialOpen ||
                TargetingOpen)
            {
                _hud.Refresh();
                return;
            }

            float delta = Time.deltaTime;
            _checkpointTimer += delta;
            if (_checkpointTimer >= 2f) SaveCheckpoint();
            Match.Tick(delta);

            _playerMatchMaxCombo = Mathf.Max(
                _playerMatchMaxCombo,
                Match.Player.Combo.Count);

            if (Match.State == BattleMatchState.Running)
            {
                BattleAIIntent intent = _aiController.Tick(delta, Match);

                if (intent.WantsMove)
                {
                    SolitaireAIAction action =
                        _aiSolver.TryAct(Match, BattleSide.Opponent);

                    if (action.Move.Success)
                    {
                        BattleMoveOutcome outcome = Match.RegisterMove(
                            BattleSide.Opponent,
                            action.Move);

                        if (outcome.DamageDealt > 0)
                        {
                            _feedback.PlayHit();
                            _fx.ShowIncomingDamage(outcome.DamageDealt);
                        }
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
                            _opponentBattler.Name +
                            " used " +
                            intent.AttackType.ToString().ToUpperInvariant() +
                            "!");

                        _feedback.PlayAttack();
                        _fx.ShowAttack(intent.AttackType, true);
                        _board.RefreshStatus();
                    }
                }
            }

            ObserveMatchResult();

            _statusRefreshTimer -= delta;

            if (_statusRefreshTimer <= 0f)
            {
                _statusRefreshTimer = 0.1f;
                _board.RefreshStatus();
            }

            _hud.Refresh();
        }

        public void SelectBattler(BattlerId id)
        {
            Profile.SelectBattler(id);
            _playerBattler = BattlerCatalog.Get(id);

            if (_frontEnd != null)
                _frontEnd.Refresh();
        }

        public void SelectDifficulty(
            BattleDifficulty difficulty)
        {
            Profile.SetDifficulty(difficulty);

            if (_frontEnd != null)
                _frontEnd.Refresh();
        }

        public void StartBattleFromMenu()
        {
            if (_frontEnd != null)
                _frontEnd.Hide();

            _menuOpen = false;
            _savedMatch = null;
            MatchSaveStore.Clear();
            StartRematch();
            SaveCheckpoint();

            if (_tutorial != null)
                _tutorial.OpenIfNeeded();
        }

        public void ShowFrontEnd()
        {
            if (_sessionStarted) SaveCheckpoint();
            _menuOpen = true;
            _paused = false;
            if (_pause != null) _pause.Close();

            if (_frontEnd != null)
                _frontEnd.Show();
        }

        public void StartRematch()
        {
            _matchCounter++;
            _paused = false;
            _sessionStarted = true;
            _savedMatch = null;
            _targeting?.Close();
            if (_pause != null) _pause.Close();

            int baseSeed = unchecked(
                Environment.TickCount +
                (_matchCounter * 104729));

            int opponentSeed = baseSeed ^ 1597463007;

            _playerBattler = BattlerCatalog.Get(
                Profile != null
                    ? Profile.SelectedBattler
                    : BattlerId.Kael);

            _opponentBattler = ChooseOpponent(_playerBattler.Id);

            Match = new BattleMatch(
                baseSeed,
                opponentSeed,
                BuildModifiers(_playerBattler.Id),
                BuildModifiers(_opponentBattler.Id));
            _aiController = new BattleAIController(
                opponentSeed ^ 173,
                Profile != null
                    ? Profile.Difficulty
                    : BattleDifficulty.Standard);
            _aiSolver = new SolitaireMoveSolver();
            _lastObservedState = BattleMatchState.Running;
            _matchRecorded = false;
            _playerMatchMaxCombo = 0;

            ShowMessage(
                _playerBattler.Name +
                " vs " +
                _opponentBattler.Name);

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
                InvalidAction("No cards left to draw.");
                return;
            }

            _feedback.PlayMove(false);
            _board.Refresh();
            SaveCheckpoint();
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

            int cost =
                BattleAttack.GetCost(type);

            if (Match.Player.Energy < cost)
            {
                InvalidAction(
                    "Not enough energy.");
                return;
            }

            if (type == BattleAttackType.Lock ||
                type == BattleAttackType.Blocker)
            {
                if (_targeting != null)
                {
                    _targeting.Open(type);
                    return;
                }
            }

            ExecuteTargetedAttack(type, -1);
        }

        public void ExecuteTargetedAttack(
            BattleAttackType type,
            int targetColumn)
        {
            if (Match == null ||
                Match.State != BattleMatchState.Running)
            {
                if (_targeting != null)
                    _targeting.Close();

                return;
            }

            if (_paused || _backgrounded || _menuOpen || TutorialOpen) return;
            bool needsColumn =
                type == BattleAttackType.Lock ||
                type == BattleAttackType.Blocker;

            if (needsColumn &&
                (targetColumn < 0 ||
                 targetColumn > 6 ||
                 !Match.CanUseColumn(
                     BattleSide.Opponent,
                     targetColumn)))
            {
                InvalidAction(
                    "That rival column cannot be targeted.");

                if (_targeting != null)
                    _targeting.Close();

                return;
            }

            bool success =
                Match.TryAttack(
                    BattleSide.Player,
                    type,
                    targetColumn);

            if (_targeting != null)
                _targeting.Close();

            if (!success)
            {
                InvalidAction(
                    "Attack could not be launched.");
                return;
            }

            ShowMessage(
                type.ToString()
                    .ToUpperInvariant() +
                (needsColumn
                    ? " hit column " +
                      (targetColumn + 1) +
                      "!"
                    : " launched!"));

            _feedback.PlayAttack();
            _fx.ShowAttack(type, false);
            _hud.Refresh();
            SaveCheckpoint();
        }

        public void ShowTutorial()
        {
            if (_tutorial != null)
                _tutorial.Open();
        }

        public void ShowMessage(string message)
        {
            _message = message ?? "";
            _messageUntil = Time.unscaledTime + 1.7f;
        }

        public void InvalidAction(string message)
        {
            ShowMessage(message);
            _feedback.PlayInvalid();

            if (_fx != null)
                _fx.ShowInvalid();
        }

        private bool FinishPlayerMove(MoveResult move)
        {
            if (!move.Success)
            {
                InvalidAction("That move is not legal.");
                return false;
            }

            BattleMoveOutcome outcome =
                Match.RegisterMove(BattleSide.Player, move);

            _playerMatchMaxCombo = Mathf.Max(
                _playerMatchMaxCombo,
                Match.Player.Combo.Count);

            _feedback.PlayMove(move.FoundationMove);
            _fx.ShowMove(outcome);

            if (outcome.Counted)
            {
                string text = "+" + outcome.EnergyGained + " energy";

                if (outcome.DamageDealt > 0)
                    text += "   " + outcome.DamageDealt + " damage";

                if (outcome.ShieldGained > 0)
                    text += "   +" + outcome.ShieldGained + " shield";

                ShowMessage(text);
            }

            ObserveMatchResult();
            SaveCheckpoint();
            _hud.Refresh();
            return true;
        }

        private void ObserveMatchResult()
        {
            if (Match == null ||
                Match.State == BattleMatchState.Running)
            {
                return;
            }

            if (!_matchRecorded)
            {
                _savedMatch = null;
                MatchSaveStore.Clear();
                Profile.RecordMatch(
                    Match,
                    _playerMatchMaxCombo,
                    _playerBattler.Id);

                _matchRecorded = true;

                if (_frontEnd != null)
                    _frontEnd.Refresh();
            }

            if (Match.State == _lastObservedState)
                return;

            _lastObservedState = Match.State;

            if (Match.State == BattleMatchState.PlayerWon)
            {
                _feedback.PlayResult(true);
                _fx.ShowResult(Match.State);
            }
            else if (Match.State == BattleMatchState.OpponentWon)
            {
                _feedback.PlayResult(false);
                _fx.ShowResult(Match.State);
            }
        }

        private BattlerDefinition ChooseOpponent(BattlerId playerId)
        {
            int start =
                (_matchCounter + 1) % BattlerCatalog.Count;

            for (int offset = 0;
                 offset < BattlerCatalog.Count;
                 offset++)
            {
                BattlerDefinition candidate =
                    BattlerCatalog.GetByIndex(
                        (start + offset) %
                        BattlerCatalog.Count);

                if (candidate.Id != playerId)
                    return candidate;
            }

            return BattlerCatalog.Get(BattlerId.Vesper);
        }

        private static BattleModifiers BuildModifiers(
            BattlerId id)
        {
            switch (id)
            {
                case BattlerId.Vesper:
                    return new BattleModifiers(
                        foundationDamageBonus: 1);

                case BattlerId.Kael:
                    return new BattleModifiers(
                        comboEnergyBonusAtThree: 1);

                case BattlerId.Aldric:
                    return new BattleModifiers(
                        foundationShieldBonus: 1,
                        maxShieldBonus: 10);

                default:
                    return BattleModifiers.None;
            }
        }

        private bool CanPlayerAct()
        {
            return Match != null &&
                   Match.State == BattleMatchState.Running &&
                   !_paused && !_backgrounded &&
                   !TutorialOpen &&
                   !_menuOpen &&
                   !TargetingOpen;
        }

        private bool CanUsePlayerColumn(int column)
        {
            if (!CanPlayerAct())
                return false;

            if (!Match.CanUseColumn(BattleSide.Player, column))
            {
                InvalidAction("That column is disrupted.");
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
                if (!Match.CanUseColumn(
                        BattleSide.Opponent,
                        column))
                {
                    continue;
                }

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

        public SavedMatch CaptureCheckpoint()
        {
            return new SavedMatch {
                playerBattler=(int)_playerBattler.Id, opponentBattler=(int)_opponentBattler.Id,
                difficulty=(int)_aiController.Difficulty, maxCombo=_playerMatchMaxCombo,
                player=SavedParticipant.Capture(Match.Player), opponent=SavedParticipant.Capture(Match.Opponent), ai=_aiController.Capture()
            };
        }
        public void SaveCheckpoint()
        {
            _checkpointTimer=0;
            if (!_sessionStarted || _menuOpen || Match==null || Match.State!=BattleMatchState.Running) return;
            _savedMatch=CaptureCheckpoint();
            RecoveryNotice=MatchSaveStore.Save(_savedMatch)?"Battle saved on this device.":"Could not save to this device. Keep the app open and try again.";
        }
        public void ContinueSavedBattle()
        {
            if (_savedMatch==null) return;
            try
            {
                _savedMatch.Validate();
                _playerBattler=BattlerCatalog.Get((BattlerId)_savedMatch.playerBattler);
                _opponentBattler=BattlerCatalog.Get((BattlerId)_savedMatch.opponentBattler);
                var restored=new BattleMatch(_savedMatch.player.seed,_savedMatch.opponent.seed,BuildModifiers(_playerBattler.Id),BuildModifiers(_opponentBattler.Id));
                _savedMatch.player.Restore(restored.Player); _savedMatch.opponent.Restore(restored.Opponent);
                var ai=new BattleAIController(_savedMatch.ai.seed,(BattleDifficulty)_savedMatch.difficulty); ai.Restore(_savedMatch.ai);
                Match=restored; _aiController=ai; _aiSolver=new SolitaireMoveSolver();
                _playerMatchMaxCombo=_savedMatch.maxCombo; _matchRecorded=false; _lastObservedState=BattleMatchState.Running;
                _sessionStarted=true; _menuOpen=false; _frontEnd.Hide();
                ShowMessage("Battle restored.");
                _board.Refresh(); _hud.Refresh(); OpenPause();
            }
            catch (Exception e) when(e is System.IO.InvalidDataException || e is ArgumentException)
            { RecoveryNotice="The saved battle could not be restored. Your career is safe."; _savedMatch=null; _frontEnd.Refresh(); }
        }
        public void OpenPause()
        {
            if (_pause==null) return;
            if (TargetingOpen) _targeting.Close();
            CardView.CancelCurrentDrag();
            _paused=true; SaveCheckpoint(); _pause.Open();
        }
        public void ResumeBattle()
        {
            if (_backgrounded) return;
            _pause?.Close(); _paused=false;

        }
        public void RequestNewBattle()
        {
            if (HasSavedMatch) { OpenPause(); _pause.ConfirmNewBattle(); }
            else StartBattleFromMenu();
        }
        private void OnApplicationPause(bool paused)
        {
            _backgrounded=paused;
            if(paused && _sessionStarted && !_menuOpen) OpenPause();
        }
        private void OnApplicationFocus(bool focused)
        {
            if(!focused && _sessionStarted && !_menuOpen) OpenPause();
        }
        private void OnApplicationQuit() { SaveCheckpoint(); }

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

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
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

            FantasyUI.Backdrop(background.transform);
            background.transform.SetAsFirstSibling();

            RectTransform safeRoot = PrototypeUI.CreateRect(
                "SafeArea",
                canvasObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            safeRoot.gameObject.AddComponent<SafeAreaFitter>();

            _board = BattleBoardView.Create(safeRoot, this);
            _hud = BattleHudView.Create(safeRoot, this);
            _fx = BattleFxView.Create(safeRoot);
            _targeting = TargetingOverlay.Create(
                safeRoot,
                this);

            _tutorial = TutorialOverlay.Create(
                safeRoot,
                this);

            _frontEnd = BattleFrontEndView.Create(
                safeRoot,
                this);
            _pause = PauseOverlay.Create(safeRoot, this);
            if(EventSystem.current!=null) EventSystem.current.pixelDragThreshold=Mathf.Clamp(Mathf.RoundToInt(Screen.dpi>0?Screen.dpi*.06f:12),10,35);
        }
    }
}
