#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BattleSolitaire.Battle;
using BattleSolitaire.Core;
using BattleSolitaire.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.EditorTools
{
    // Run in an isolated project copy: -batchmode -executeMethod BattleSolitaire.EditorTools.FrontEndReview.Run
    [InitializeOnLoad]
    public static class FrontEndReview
    {
        private const string Pending = "BattleSolitaire.FrontEndReview";
        private static int _frames;
        private static readonly Dictionary<string, int?> SavedPrefs = new Dictionary<string, int?>();
        static FrontEndReview() { EditorApplication.update += Update; }

        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this review in an isolated batch-mode project copy.");
            SessionState.SetBool(Pending, true);
            EditorApplication.isPlaying = true;
        }

        private static void Update()
        {
            if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            BattleGameController controller = UnityEngine.Object.FindAnyObjectByType<BattleGameController>();
            if (controller == null || controller.Profile == null || ++_frames < 20) return;
            SessionState.SetBool(Pending, false);
            try
            {
                SavePreferences();
                Verify(controller);
                Debug.Log("FRONT_END_REVIEW_PASS");
                RestorePreferences();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                RestorePreferences();
                EditorApplication.Exit(1);
            }
        }

        private static void Verify(BattleGameController controller)
        {
            string[] args = Environment.GetCommandLineArgs();
            int outputIndex = Array.IndexOf(args, "-reviewOutput");
            string output = Path.GetFullPath(outputIndex >= 0 && outputIndex + 1 < args.Length
                ? args[outputIndex + 1] : "Temp/FrontEndReview");
            Directory.CreateDirectory(output);
            var report = new List<string>();
            Check(controller.TitleOpen,"App opens at the title screen");
            var title=UnityEngine.Object.FindAnyObjectByType<TitleScreenView>();
            var bootCanvas=title.GetComponentInParent<Canvas>();
            Capture(bootCanvas,null,output,"title-phone",390,844,report);
            Capture(bootCanvas,null,output,"title-desktop",1280,1920,report);
            title.transform.Find("TitleHelp").GetComponent<Button>().onClick.Invoke();
            var help=UnityEngine.Object.FindAnyObjectByType<TutorialOverlay>();
            for(int page=0;page<4;page++)
            {
                Capture(bootCanvas,null,output,"tutorial-page-"+(page+1),390,844,report);
                var body=help.GetComponentsInChildren<Text>().First(t=>t.name=="TutorialBody");
                Check(body.preferredHeight<=body.rectTransform.rect.height+1,"Tutorial body fits on phone");
                help.GetComponentsInChildren<Button>().First(b=>b.name=="Next").onClick.Invoke();
            }
            Check(!controller.TutorialOpen && controller.TitleOpen,"Tutorial returns to title");
            controller.ShowTutorial();Capture(bootCanvas,null,output,"tutorial-desktop",1280,1920,report);help.CloseAndRemember();
            title.transform.Find("PlayButton").GetComponent<Button>().onClick.Invoke();
            var menu = UnityEngine.Object.FindAnyObjectByType<BattleFrontEndView>();
            var canvas = menu.GetComponentInParent<Canvas>();
            Check(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait, "Portrait project setting");
            Check(canvas.GetComponent<CanvasScaler>().referenceResolution == new Vector2(1080, 1920), "Reference resolution");
            Check(menu.GetComponentInParent<SafeAreaFitter>() != null, "Safe-area root preserved");
            foreach (Vector2 source in new[] { new Vector2(96,117), new Vector2(1920,1080), new Vector2(100,300) })
            foreach (Vector2 destination in new[] { new Vector2(300,400), new Vector2(80,80), new Vector2(400,100) })
            {
                Rect uv = AspectFillRawImage.CalculateUvRect(source, destination);
                Check(Mathf.Abs(source.x * uv.width / (source.y * uv.height) - destination.x / destination.y) < 0.001f, "Undistorted crop");
                Check(uv.x >= 0 && uv.y >= 0 && uv.xMax <= 1.001f && uv.yMax <= 1.001f, "Crop bounds");
            }
            Check(AspectFillRawImage.CalculateUvRect(Vector2.zero, Vector2.zero) == new Rect(0,0,1,1), "Empty crop guard");
            foreach (BattlerId id in Enum.GetValues(typeof(BattlerId)))
            {
                string name = BattlerCatalog.Get(id).Name;
                ExecuteEvents.Execute(menu.transform.Find("Battler_" + name).gameObject,
                    new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Check(new BattleProfile().SelectedBattler == id, "Battler persists " + name);
                Check(menu.GetComponentsInChildren<Text>().Any(t => t.name == "ActiveName" && t.text.Equals(name, StringComparison.OrdinalIgnoreCase)), "Hero updates");
                string passive = BattlerCatalog.Get(id).TraitLine.Split('•')[0].Trim();
                Check(menu.GetComponentsInChildren<Text>().Any(t => t.name == "PassiveName" && t.text == passive), "Passive updates");
                Capture(canvas, menu, output, name.ToLowerInvariant() + "-1080x1920", 1080, 1920, report);
            }
            foreach (BattleDifficulty difficulty in Enum.GetValues(typeof(BattleDifficulty)))
            {
                menu.transform.Find("Difficulty_" + difficulty.ToString().ToUpperInvariant()).GetComponent<Button>().onClick.Invoke();
                Check(new BattleProfile().Difficulty == difficulty, "Difficulty persists");
            }
            controller.SelectBattler(BattlerId.Vesper);
            Capture(canvas, menu, output, "phone-390x844", 390, 844, report);
            Capture(canvas, menu, output, "tablet-1200x1600", 1200, 1600, report);
            var safe = menu.GetComponentInParent<SafeAreaFitter>();
            safe.enabled = false;
            ((RectTransform)safe.transform).anchorMin = new Vector2(0, 0.035f);
            ((RectTransform)safe.transform).anchorMax = new Vector2(1, 0.94f);
            Capture(canvas, menu, output, "phone-safe-area", 1080, 2340, report);
            ((RectTransform)safe.transform).anchorMin = Vector2.zero;
            ((RectTransform)safe.transform).anchorMax = Vector2.one;
            Texture original = menu.GetComponentsInChildren<RawImage>().First(i => i.name == "Portrait" && i.transform.parent.name == "Battler_Vesper").texture;
            var hero = menu.GetComponentsInChildren<AspectFillRawImage>().First(i => i.name == "Portrait" && i.transform.parent.name == "Battler_Vesper");
            hero.SetTexture(null);
            Capture(canvas, menu, output, "missing-portrait", 1080, 1920, report);
            hero.SetTexture(original);
            var battle = menu.transform.Find("BattleButton").GetComponent<Button>();
            EventSystem.current.SetSelectedGameObject(battle.gameObject);
            ExecuteEvents.Execute(battle.gameObject, new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Up }, ExecuteEvents.moveHandler);
            Check(EventSystem.current.currentSelectedGameObject.name == "Battler_Kael", "Keyboard navigation");
            PlayerPrefs.SetInt("BattleSolitaire.TutorialSeen.v1", 1);
            ExecuteEvents.Execute(battle.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            Check(!controller.MenuOpen && controller.Match.State == BattleMatchState.Running, "Battle launches");
            var hud = UnityEngine.Object.FindAnyObjectByType<BattleHudView>();
            foreach (RawImage portrait in hud.GetComponentsInChildren<RawImage>().Where(i => i.name.EndsWith("Portrait")))
                Check(portrait.GetComponent<AspectFillRawImage>() != null, "HUD uses aspect crop");
            Capture(canvas, null, output, "battle-hud", 1080, 1920, report);
            Capture(canvas, null, output, "battle-phone", 390, 844, report);
            MobileReview.Run(controller,(name,width,height)=>Capture(canvas,null,output,name,width,height,report));
            PhoneFeedbackReview.Run(controller,canvas,(name,width,height)=>Capture(canvas,null,output,name,width,height,report));
            controller.ShowTutorial();
            Check(controller.TutorialOpen, "Tutorial opens");
            UnityEngine.Object.FindAnyObjectByType<TutorialOverlay>().CloseAndRemember();
            foreach (BattleAttackType type in new[] { BattleAttackType.Lock, BattleAttackType.Blocker })
            {
                typeof(BattleParticipant).GetProperty("Energy").SetValue(controller.Match.Player, 100);
                controller.UsePlayerAttack(type);
                Check(controller.TargetingOpen && controller.Match.Player.Energy == 100, "Manual targeting opens without spending energy");
                controller.ExecuteTargetedAttack(type, 7);
                Check(!controller.TargetingOpen && controller.Match.Player.Energy == 100, "Invalid target preserves energy");
                controller.UsePlayerAttack(type);
                int column = type == BattleAttackType.Lock ? 0 : 1;
                controller.ExecuteTargetedAttack(type, column);
                Check(!controller.TargetingOpen && controller.Match.Player.Energy == 100 - BattleAttack.GetCost(type), "Targeted attack spends correct energy");
            }
            VerifyDragAndDrop(controller,canvas,output,report);
            int wins = controller.Profile.Wins;
            controller.Match.Opponent.ApplyDamage(10000);
            controller.Match.Tick(0);
            typeof(BattleGameController).GetMethod("ObserveMatchResult", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller, null);
            hud.Refresh();
            Check(controller.Profile.Wins == wins + 1, "Stats recorded after match");
            hud.GetComponentsInChildren<Button>().First(b => b.name == "LoadoutButton").onClick.Invoke();
            Check(controller.MenuOpen, "Result returns to Loadout");
            Check(menu.GetComponentsInChildren<Text>().Any(t => t.name == "StatValue" && t.text == (wins+1).ToString()), "Menu stats refresh");
            Capture(canvas, menu, output, "career-after-win", 1080, 1920, report);
            report.Add("PASS: crop math, all battler/difficulty persistence, passive text, keyboard navigation/submit, pointer selection, Battle, tutorial, Lock/Block targeting and invalid-target recovery, legal/illegal drag dispatch, result return, career refresh, portrait settings, nonempty frame/icon meshes.");
            report.Add("PASS: mobile lifecycle, settings, complete checkpoint round trip, AI scheduling, corrupt-save fallback, replacement confirmation, long-stack bounds.");
            report.Add("PASS: title and tutorial flow, portable suit meshes, full-column pointer drops, single/run/invalid drops, all 16 ace/slot combinations, foundation persistence, bounded Fog, quit/cancel.");
            report.Add("Android build utility compiled with the editor assembly. No APK/device test in this review.");
            File.WriteAllLines(Path.Combine(output, "verification.txt"), report);
        }

        private static void VerifyDragAndDrop(BattleGameController controller,Canvas canvas,string output,List<string> report)
        {
            controller.StartRematch();
            var board=UnityEngine.Object.FindAnyObjectByType<BattleBoardView>();
            SolitaireGame game=controller.Match.Player.Game;
            for(int i=0;i<3;i++)game.Tableau[i].Clear();
            game.Tableau[0].Add(new CardState(Suit.Spades,Rank.King,true));
            game.Tableau[1].Add(new CardState(Suit.Hearts,Rank.Queen,true));
            game.Tableau[2].Add(new CardState(Suit.Diamonds,Rank.Queen,true));
            board.Refresh();
            Capture(canvas,null,output,"battle-court-cards",1080,1920,report);
            var field=typeof(BattleBoardView).GetField("_renderedCards",BindingFlags.Instance|BindingFlags.NonPublic);
            var pointer=new PointerEventData(EventSystem.current){position=new Vector2(300,600),button=PointerEventData.InputButton.Left};
            CardView moving=((List<CardView>)field.GetValue(board)).First(c=>c.SourceKind==CardSourceKind.Tableau && c.Column==1);
            moving.OnBeginDrag(pointer);moving.OnDrag(pointer);
            board.GetComponentsInChildren<PileDropTarget>().First(t=>t.name=="Column_2").OnDrop(pointer);
            moving.OnEndDrag(pointer);
            Check(game.Tableau[1].Count==1 && game.Tableau[2].Count==1 && CardView.CurrentDrag==null,"Illegal drag returns without changing cards");
            moving=((List<CardView>)field.GetValue(board)).First(c=>c.SourceKind==CardSourceKind.Tableau && c.Column==1);
            moving.OnBeginDrag(pointer);moving.OnDrag(pointer);
            board.GetComponentsInChildren<PileDropTarget>().First(t=>t.name=="Column_0").OnDrop(pointer);
            moving.OnEndDrag(pointer);
            Check(game.Tableau[1].Count==0 && game.Tableau[0].Count==2 && game.Tableau[0][1].Rank==Rank.Queen && CardView.CurrentDrag==null,"Legal drag updates tableau");
            controller.StartRematch();
        }

        private static void Capture(Canvas canvas, BattleFrontEndView menu, string output, string name, int width, int height, List<string> report)
        {
            var cameraObject = new GameObject("ReviewCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PrototypeUI.Background;
            camera.orthographic = true;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100;
            var target = new RenderTexture(width, height, 24);
            target.Create();
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            canvas.GetComponent<CanvasScaler>().enabled = false;
            canvas.scaleFactor = Mathf.Sqrt((float)width * height / (1080f * 1920f));
            Canvas.ForceUpdateCanvases();
            foreach (AspectRatioFitter fitter in canvas.GetComponentsInChildren<AspectRatioFitter>()) fitter.SetLayoutHorizontal();
            var board = UnityEngine.Object.FindAnyObjectByType<BattleBoardView>();
            typeof(BattleBoardView).GetMethod("FitLayout",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(board,null);
            board.Refresh();
            Canvas.ForceUpdateCanvases();
            foreach (AspectFillRawImage image in canvas.GetComponentsInChildren<AspectFillRawImage>())
            {
                image.Refresh();
                RawImage raw=image.GetComponent<RawImage>();
                if(raw.texture!=null && raw.rectTransform.rect.width>0 && raw.rectTransform.rect.height>0)
                    {
                    Check(Mathf.Abs(raw.texture.width*raw.uvRect.width/(raw.texture.height*raw.uvRect.height)-raw.rectTransform.rect.width/raw.rectTransform.rect.height)<.01f,"Rendered portrait/texture aspect");
                    Check(raw.uvRect.xMin >= image.SourceRegion.xMin-.001f && raw.uvRect.xMax <= image.SourceRegion.xMax+.001f && raw.uvRect.yMin >= image.SourceRegion.yMin-.001f && raw.uvRect.yMax <= image.SourceRegion.yMax+.001f,"Atlas crop remains inside the character region");
                    }
            }
            Canvas.ForceUpdateCanvases();
            foreach (Graphic g in canvas.GetComponentsInChildren<Graphic>()) g.SetAllDirty();
            Canvas.ForceUpdateCanvases();
            foreach (Graphic g in canvas.GetComponentsInChildren<Graphic>().Where(g=>(g is FantasyFrame || g is FantasyIcon || g is SuitIcon) && g.isActiveAndEnabled && !g.canvasRenderer.cull && g.rectTransform.rect.width>0 && g.rectTransform.rect.height>0))
                Check(g.canvasRenderer.GetMesh()!=null && g.canvasRenderer.GetMesh().vertexCount>0,"Metal frame/icon renders: "+g.name);
            foreach (Text corner in canvas.GetComponentsInChildren<Text>().Where(t=>t.name=="TopCorner" && !string.IsNullOrEmpty(t.text)))
                Check(corner.preferredHeight<=corner.rectTransform.rect.height+1,"Card rank and suit fit");
            camera.Render();
            RenderTexture.active = target;
            var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0,0,width,height), 0,0);
            pixels.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), pixels.EncodeToPNG());
            if (menu != null)
            {
                foreach (Text text in menu.GetComponentsInChildren<Text>())
                    if (!string.IsNullOrEmpty(text.text) && text.preferredHeight > text.rectTransform.rect.height + 2)
                        report.Add("TEXT OVERFLOW " + name + " " + text.name + " preferred=" + text.preferredHeight + " actual=" + text.rectTransform.rect.height);
                foreach (Text label in menu.GetComponentsInChildren<Text>().Where(t => t.transform.parent.name.StartsWith("Difficulty_")))
                    Check(label.preferredWidth <= label.rectTransform.rect.width + 1, "Difficulty label fits on one line: " + name);
                foreach (Button button in menu.GetComponentsInChildren<Button>())
                    if (((RectTransform)button.transform).rect.height < 70)
                        report.Add("SMALL TARGET " + name + " " + button.name);
            }
            report.Add("Captured " + name + " " + width + "x" + height);
            RenderTexture.active = null;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(pixels);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void SavePreferences()
        {
            string[] fields = { "SelectedBattler", "Wins", "Matches", "LongestCombo", "PerfectClears", "RankPoints", "Difficulty", "Mastery.Vesper", "Mastery.Kael", "Mastery.Aldric" };
            foreach (string field in fields) SaveKey("BattleSolitaire.Profile." + field);
            SaveKey("BattleSolitaire.TutorialSeen.v1");
            SaveKey("BattleSolitaire.SoundEnabled"); SaveKey("BattleSolitaire.HapticsEnabled"); SaveKey("BattleSolitaire.ReducedMotion");
        }
        private static void SaveKey(string key) => SavedPrefs[key] = PlayerPrefs.HasKey(key) ? (int?)PlayerPrefs.GetInt(key) : null;
        private static void RestorePreferences()
        {
            foreach (var pair in SavedPrefs)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            PlayerPrefs.Save();
        }
    }
}
#endif
