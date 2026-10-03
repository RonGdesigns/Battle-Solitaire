#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BattleSolitaire.Battle;
using BattleSolitaire.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.EditorTools
{
    public static class MobileReview
    {
        private static void Check(bool ok,string message) { if(!ok) throw new InvalidOperationException(message); }
        private static void Call(object target,string method,params object[] args) => target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        public static void Run(BattleGameController c,Action<string,int,int> capture)
        {
            c.PlayerDraw(); c.PlayerDraw();
            c.Match.Player.Disruptions.AddLock(3,2.3f);
            c.Match.Opponent.Disruptions.AddFog(2.8f);
            c.Match.Player.Disruptions.AddBlocker(4,2,6.2f);
            c.Match.Player.Combo.RegisterProgressMove(); c.Match.Player.Combo.Tick(.4f);
            c.Match.Player.RestoreResources(72,83,10);
            c.SaveCheckpoint();
            string expected=JsonUtility.ToJson(c.CaptureCheckpoint());
            Check(File.Exists(MatchSaveStore.SavePath),"Checkpoint file written");
            c.OpenPause();
            Check(c.Paused,"Pause opens");
            int stock=c.Match.Player.Game.Stock.Count;
            c.PlayerDraw(); c.UsePlayerAttack(BattleAttackType.Fog);
            for(int i=0;i<10;i++) Call(c,"Update");
            Check(stock==c.Match.Player.Game.Stock.Count && expected==JsonUtility.ToJson(c.CaptureCheckpoint()),"Pause freezes input, both boards, timers, and AI");
            capture("pause-phone",390,844); capture("pause-tablet",1200,1600);
            var pause=UnityEngine.Object.FindAnyObjectByType<PauseOverlay>();
            foreach(var button in pause.GetComponentsInChildren<Button>())
                Check(button.navigation.mode==Navigation.Mode.Explicit &&
                    button.navigation.selectOnDown.transform.IsChildOf(pause.transform) &&
                    button.navigation.selectOnUp.transform.IsChildOf(pause.transform),"Pause keyboard focus stays inside the dialog");
            bool sound=c.SoundEnabled, haptics=c.HapticsEnabled;
            pause.GetComponentsInChildren<Button>().First(b=>b.name=="SoundButton").onClick.Invoke();
            pause.GetComponentsInChildren<Button>().First(b=>b.name=="HapticsButton").onClick.Invoke();
            pause.GetComponentsInChildren<Button>().First(b=>b.name=="MotionButton").onClick.Invoke();
            Check(c.SoundEnabled!=sound && c.HapticsEnabled!=haptics && AccessibilitySettings.ReducedMotion,"Settings controls change state");
            Check(PlayerPrefs.GetInt("BattleSolitaire.ReducedMotion")==1,"Reduced motion persists");
            capture("settings-reduced-motion",390,844);
            c.ToggleSound(); c.ToggleHaptics();
            pause.ConfirmNewBattle(); capture("replace-confirmation",390,844);
            Check(EventSystem.current.currentSelectedGameObject.name=="ResumeButton","Replacement confirmation focuses Keep Current Battle");
            pause.GetComponentsInChildren<Button>().First(b=>b.name=="ResumeButton").onClick.Invoke();
            Check(expected==JsonUtility.ToJson(c.CaptureCheckpoint()),"Cancel replacement preserves the battle");
            c.ShowFrontEnd(); capture("menu-with-continue",390,844);
            c.SelectBattler(BattlerId.Aldric); c.OpenPause(); c.ResumeBattle();
            var disk=MatchSaveStore.Load(out string notice);
            Check(disk!=null,"Checkpoint loads from disk");
            Check(JsonUtility.ToJson(disk)==expected,"Menu settings cannot change the saved battler or match");
            typeof(BattleGameController).GetField("_savedMatch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(c,disk);
            c.ContinueSavedBattle();
            Check(c.Paused && !c.MenuOpen && expected==JsonUtility.ToJson(c.CaptureCheckpoint()),"Disk restore preserves the complete match and starts paused");
            var a=new BattleAIController(disk.ai.seed,(BattleDifficulty)disk.difficulty); a.Restore(disk.ai);
            var b=new BattleAIController(8,(BattleDifficulty)disk.difficulty); b.Restore(disk.ai);
            for(int i=0;i<200;i++)
            {
                var x=a.Tick(.1f,c.Match); var y=b.Tick(.1f,c.Match);
                Check(x.WantsMove==y.WantsMove && x.WantsAttack==y.WantsAttack && x.AttackType==y.AttackType && x.TargetColumn==y.TargetColumn,"AI scheduling survives restore");
            }
            c.ResumeBattle();
            var card=UnityEngine.Object.FindObjectsByType<CardView>(FindObjectsSortMode.None).First(v=>v.Card.IsFaceUp);
            card.OnBeginDrag(new PointerEventData(EventSystem.current));
            Call(c,"OnApplicationPause",true);
            Check(c.Paused && CardView.CurrentDrag==null,"Backgrounding pauses and cancels drag");
            Call(c,"OnApplicationPause",false);
            Check(c.Paused,"Foregrounding does not resume unexpectedly");
            c.ResumeBattle(); Check(!c.Paused,"Explicit resume works");
            c.SaveCheckpoint();
            File.WriteAllText(MatchSaveStore.SavePath,"interrupted write");
            Check(MatchSaveStore.Load(out notice)!=null && notice.Contains("previous"),"Corrupt primary recovers backup");
            File.WriteAllText(MatchSaveStore.SavePath+".bak","also invalid");
            Check(MatchSaveStore.Load(out notice)==null && notice.Contains("career"),"Corrupt saves fail safely");
            var invalid=JsonUtility.FromJson<SavedMatch>(expected); invalid.version=999;
            Check(!MatchSaveStore.Save(invalid),"Future save version is rejected");
            invalid=JsonUtility.FromJson<SavedMatch>(expected); invalid.player.piles[0].cards[0]=invalid.player.piles[0].cards[1];
            Check(!MatchSaveStore.Save(invalid),"Duplicate cards are rejected");
            MatchSaveStore.Clear(); c.StartRematch();
            var board=UnityEngine.Object.FindAnyObjectByType<BattleBoardView>();
            var game=c.Match.Player.Game;
            for(int i=0;i<15;i++)
            {
                var extra=game.Stock[game.Stock.Count-1];game.Stock.RemoveAt(game.Stock.Count-1);
                extra.IsFaceUp=true;game.Tableau[0].Add(extra);
            }
            board.Refresh();capture("long-stack-phone",390,844);
            var bounds=new Vector3[4];((RectTransform)board.transform).GetWorldCorners(bounds);
            var views=(System.Collections.Generic.List<CardView>)typeof(BattleBoardView).GetField("_renderedCards",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(board);
            foreach(var view in views)
            {
                var corners=new Vector3[4];((RectTransform)view.transform).GetWorldCorners(corners);
                Check(corners[0].y>=bounds[0].y && corners[2].y<=bounds[2].y,"Long stacks remain inside the table");
            }
            c.StartRematch();
            AccessibilitySettings.SetReducedMotion(false);
            Debug.Log("MOBILE_REVIEW_PASS: pause, input lock, settings, complete disk restore, AI continuity, background drag cancellation, backup recovery, invalid save validation.");
        }
    }
}
#endif
