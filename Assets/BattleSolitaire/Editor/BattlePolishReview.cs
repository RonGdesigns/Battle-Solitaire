#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using BattleSolitaire.Battle;
using BattleSolitaire.Core;
using BattleSolitaire.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleSolitaire.EditorTools
{
    public static class BattlePolishReview
    {
        private static void Check(bool condition,string label)
        { if(!condition) throw new InvalidOperationException("M10: "+label); }
        private static BattleParticipant Empty()
        {
            var player=new BattleParticipant("Hint fixture",1,BattleModifiers.None);
            player.Game.Stock.Clear();player.Game.Waste.Clear();
            foreach(var pile in player.Game.Tableau) pile.Clear();
            foreach(var pile in player.Game.Foundations.Values) pile.Clear();
            return player;
        }
        private static CardState Card(Suit suit,Rank rank,bool up=true) => new CardState(suit,rank,up);
        private static void Refresh(BattleGameController c)
        {
            UnityEngine.Object.FindAnyObjectByType<BattleBoardView>().Refresh();
            UnityEngine.Object.FindAnyObjectByType<BattleHudView>().Refresh();
            Canvas.ForceUpdateCanvases();
        }
        private static void Observe(BattleGameController c) => typeof(BattleGameController).GetMethod("ObserveMatchResult",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
        private static void Touch(Canvas canvas,Button button)
        {
            var camera=new GameObject("M10 Touch Camera").AddComponent<Camera>();
            var target=new RenderTexture(390,844,24);camera.targetTexture=target;camera.orthographic=true;camera.nearClipPlane=.01f;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            canvas.GetComponent<CanvasScaler>().enabled=false;canvas.scaleFactor=Mathf.Sqrt(390f*844f/(1080f*1920f));Canvas.ForceUpdateCanvases();
            var rect=(RectTransform)button.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);
            var lower=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var upper=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            Check(upper.x-lower.x>=44 && upper.y-lower.y>=44,"Touch target is at least 44 pixels: "+button.name);
            var data=new PointerEventData(EventSystem.current){position=(lower+upper)/2,button=PointerEventData.InputButton.Left};
            var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Check(hits.Count>0 && hits[0].gameObject.GetComponentInParent<Button>()==button,"Touch reaches visible control: "+button.name);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler);
            UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(target);
        }
        public static void Run(BattleGameController c,Canvas canvas,Action<string,int,int> capture)
        {
            var match=new BattleMatch(1,2);
            match.Opponent.RestoreResources(100,100,0);
            Check(!match.QueueRivalAttack(BattleAttackType.Lock,0),"Opening grace prevents disruption");
            match.Tick(15);Check(match.QueueRivalAttack(BattleAttackType.Lock,0),"Warning queues after opening grace");
            Check(!match.Player.Disruptions.IsColumnLocked(0) && match.Opponent.Energy==100,"Warning does not land or charge early");
            match.Tick(1);Check(match.RivalWarning.pending && !match.RivalAttackLanded,"Warning remains visible for two seconds");
            Check(!match.QueueRivalAttack(BattleAttackType.Fog,-1),"No overlapping rival abilities");
            var warning=match.RivalWarning.Copy();
            match.Tick(1);Check(match.RivalAttackLanded && match.Player.Disruptions.IsColumnLocked(0) && match.Opponent.Energy==75,"Announced column receives attack after warning");
            Check(!match.QueueRivalAttack(BattleAttackType.Lock,1),"Ten-second cooldown prevents immediate repeat");
            match.Tick(9.9f);Check(!match.QueueRivalAttack(BattleAttackType.Lock,1),"Cooldown stays active until its deadline");
            match.Tick(.11f);Check(match.QueueRivalAttack(BattleAttackType.Lock,1),"Next warning permitted after cooldown");
            match.Player.ApplyDamage(100);match.Tick(3);Check(!match.RivalAttackLanded && !match.RivalWarning.pending,"No late attack after match ends");

            match=new BattleMatch(4,5);match.Opponent.RestoreResources(100,0,1);
            match.RegisterMove(BattleSide.Player,MoveResult.Succeeded(MoveKind.TableauToFoundation,foundationMove:true));
            Check(match.LastHealthDamage==0 && match.LastShieldDamage==1 && match.PlayerRecord.healthDamage==0 && match.PlayerRecord.shieldDamage==1,"Shield-only hits do not count as lost HP");
            match.RegisterMove(BattleSide.Player,MoveResult.Succeeded(MoveKind.TableauToFoundation,foundationMove:true));
            Check(match.PlayerRecord.healthDamage==1 && match.OpponentRecord.shieldBlocked==1 && match.PlayerRecord.longestCombo==2,"Damage, shield defense, and longest combo accumulate");
            match.Tick(10);Check(match.PlayerRecord.longestCombo==2,"Peak combo survives expiration");
            match.Opponent.RestoreResources(1,0,0);
            match.RegisterMove(BattleSide.Player,MoveResult.Succeeded(MoveKind.TableauToTableau,clearedColumn:true));
            Check(match.LastHealthDamage==1,"Overkill is excluded from HP damage totals");

            var actor=Empty();actor.Game.Waste.Add(Card(Suit.Hearts,Rank.Ace));
            string before=JsonUtility.ToJson(SavedParticipant.Capture(actor));
            Check(BattleHintPlanner.Find(actor).Text.Contains("foundation"),"Waste ace hint");
            Check(before==JsonUtility.ToJson(SavedParticipant.Capture(actor)),"Hint leaves all cards and resources untouched");
            actor=Empty();actor.Game.Waste.Add(Card(Suit.Hearts,Rank.Ace));actor.Game.Waste.Add(Card(Suit.Clubs,Rank.Three));
            Check(BattleHintPlanner.Find(actor).Text.StartsWith("Recycle"),"Buried waste ace is reachable through recycling");
            actor=Empty();actor.Game.Stock.Add(Card(Suit.Hearts,Rank.Ace,false));
            Check(BattleHintPlanner.Find(actor).Text.StartsWith("Draw"),"Hidden stock offers a route without exposing its identity");
            actor=Empty();actor.Game.Tableau[0].Add(Card(Suit.Hearts,Rank.King,false));actor.Game.Tableau[0].Add(Card(Suit.Clubs,Rank.Four));
            actor.Game.Tableau[1].Add(Card(Suit.Spades,Rank.Six));
            for(int rank=1;rank<=5;rank++) actor.Game.Foundations[Suit.Hearts].Add(Card(Suit.Hearts,(Rank)rank));
            Check(BattleHintPlanner.Find(actor).Text.StartsWith("Bring"),"Foundation return can open a useful two-move route");
            actor.Disruptions.AddLock(1,3);
            Check(BattleHintPlanner.Find(actor).Waiting,"Temporary lock is not mistaken for a stuck deal");
            actor=Empty();actor.Game.Foundations[Suit.Hearts].Add(Card(Suit.Hearts,Rank.Ace));actor.Game.Tableau[0].Add(Card(Suit.Clubs,Rank.Two));
            Check(!BattleHintPlanner.Find(actor).HasMove,"Foundation return-and-replace cycle is not suggested as progress");
            actor=Empty();actor.Game.Waste.Add(Card(Suit.Hearts,Rank.Ace));actor.Disruptions.AddFog(2);
            Check(BattleHintPlanner.Find(actor).Waiting,"Fog hint waits rather than exposing hidden foundation information");
            var stopwatch=System.Diagnostics.Stopwatch.StartNew();
            for(int seed=0;seed<40;seed++) BattleHintPlanner.Find(new BattleParticipant("Probe",seed,BattleModifiers.None));
            stopwatch.Stop();Debug.Log("M10_HINT_40_DEALS_MS "+stopwatch.ElapsedMilliseconds);

            c.StartRematch();c.Match.Opponent.RestoreResources(100,100,0);c.Match.Tick(15);
            Check(c.Match.QueueRivalAttack(BattleAttackType.Blocker,3),"Live rival block warning queues");
            c.Match.PlayerRecord.healthDamage=17;c.Match.PlayerRecord.shieldDamage=9;c.Match.PlayerRecord.shieldBlocked=11;
            c.Match.PlayerRecord.longestCombo=8;c.Match.PlayerRecord.energyEarned=76;
            Refresh(c);capture("rival-warning-phone",390,844);capture("rival-warning-desktop",1280,1920);
            c.OpenPause();float remaining=c.Match.RivalWarning.remaining;
            typeof(BattleGameController).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
            Check(c.Match.RivalWarning.remaining==remaining,"Pause freezes rival warning");
            c.SaveCheckpoint();var saved=MatchSaveStore.Load(out string notice);
            Check(saved!=null && saved.rivalWarning.pending && saved.playerRecord.healthDamage==17,"Disk checkpoint includes warning and combat stats");
            saved.playerRecord.healthDamage=999;Check(c.Match.PlayerRecord.healthDamage==17,"Saved records do not share live references");
            c.ShowTitleScreen();c.ContinueSavedBattle();
            Check(c.Paused && c.Match.RivalWarning.pending && c.Match.RivalWarning.remaining==remaining && c.Match.PlayerRecord.healthDamage==17,"Continue restores exact warning and stats while paused");
            var legacy=c.CaptureCheckpoint();legacy.playerRecord=null;legacy.opponentRecord=null;legacy.rivalWarning=null;legacy.Validate();
            var restored=new BattleMatch(1,2);restored.RestoreCombat(null,null,null);Check(restored.PlayerRecord.healthDamage==0,"Old checkpoints accept missing additive stats");
            c.ResumeBattle();c.Match.Tick(2);Refresh(c);capture("effect-countdown-phone",390,844);
            c.Match.Tick(8);
            Touch(canvas,UnityEngine.Object.FindAnyObjectByType<BattleHudView>().GetComponentsInChildren<Button>().First(b=>b.name=="HintButton"));
            Check(c.Paused,"Hint pauses both boards");
            capture("hint-phone",390,844);capture("hint-desktop",1280,1920);
            var pause=UnityEngine.Object.FindAnyObjectByType<PauseOverlay>();
            var body=pause.GetComponentsInChildren<Text>().First(t=>t.name=="Description");
            Check(body.preferredHeight<=body.rectTransform.rect.height+1,"Hint copy fits phone panel");
            pause.GetComponentsInChildren<Button>().First(b=>b.name=="NewBattleButton").onClick.Invoke();
            Check(pause.GetComponentsInChildren<Text>().Any(t=>t.text=="START A NEW BATTLE?"),"Hint new battle requires replacement confirmation");
            pause.Back();Check(c.Paused,"Cancel replacement returns to paused hint");pause.Back();Check(!c.Paused,"Resume exits hint");

            // A complete 52-card fixture with no legal forward route; nothing is discarded to test it.
            c.StartRematch();var game=c.Match.Player.Game;
            var cards=game.Stock.Concat(game.Waste).Concat(game.Tableau.SelectMany(p=>p)).ToList();
            game.Stock.Clear();game.Waste.Clear();foreach(var pile in game.Tableau) pile.Clear();
            var tops=cards.Where(card=>card.Suit==Suit.Clubs && (int)card.Rank>=2 && (int)card.Rank<=8).ToList();
            foreach(var card in cards.Except(tops)){card.IsFaceUp=false;game.Tableau[0].Add(card);}
            for(int i=0;i<7;i++){tops[i].IsFaceUp=true;game.Tableau[i].Add(tops[i]);}
            Check(!BattleHintPlanner.Find(c.Match.Player).HasMove,"No-progress board is detected");
            Refresh(c);c.ShowHint();capture("stuck-options-phone",390,844);
            body=pause.GetComponentsInChildren<Text>().First(t=>t.name=="Description");
            Check(body.preferredHeight<=body.rectTransform.rect.height+1,"No-progress explanation fits phone");
            Check(EventSystem.current.currentSelectedGameObject.name=="ResumeButton","Hint keyboard focus stays in dialog");
            c.ResumeBattle();
            foreach(var outcome in new[]{BattleMatchState.PlayerWon,BattleMatchState.OpponentWon,BattleMatchState.Draw})
            {
                c.StartRematch();c.Match.PlayerRecord.healthDamage=32;c.Match.PlayerRecord.shieldDamage=14;c.Match.PlayerRecord.shieldBlocked=9;c.Match.PlayerRecord.longestCombo=12;c.Match.PlayerRecord.energyEarned=87;
                if(outcome!=BattleMatchState.PlayerWon)c.Match.Player.ApplyDamage(100);
                if(outcome!=BattleMatchState.OpponentWon)c.Match.Opponent.ApplyDamage(100);
                c.Match.Tick(.01f);Observe(c);int recorded=c.Profile.Matches;Observe(c);Check(c.Profile.Matches==recorded,"Result records career stats exactly once");Refresh(c);
                Check(c.Match.State==outcome,"Result state "+outcome);
                capture("results-"+outcome+"-phone",390,844);
                if(outcome==BattleMatchState.PlayerWon)capture("results-desktop",1280,1920);
                var hud=UnityEngine.Object.FindAnyObjectByType<BattleHudView>();
                var stats=hud.GetComponentsInChildren<Text>().First(t=>t.name=="ResultStats");
                Check(stats.text.Contains("32") && stats.text.Contains("x12") && stats.preferredHeight<=stats.rectTransform.rect.height+1,"Result statistics are accurate and fit");
                Check(EventSystem.current.currentSelectedGameObject.name=="RematchButton","Results focus Rematch");
                var rematch=hud.GetComponentsInChildren<Button>().First(b=>b.name=="RematchButton");
                Check(rematch.navigation.selectOnRight.name=="ResultTitleButton","Results keyboard navigation stays inside the panel");
                if(outcome==BattleMatchState.PlayerWon)
                {
                    Touch(canvas,rematch);Check(c.Match.State==BattleMatchState.Running && c.Match.PlayerRecord.healthDamage==0,"Rematch button starts a fresh match");
                    c.Match.Opponent.ApplyDamage(100);c.Match.Tick(0);Observe(c);Refresh(c);
                }
                Touch(canvas,hud.GetComponentsInChildren<Button>().First(b=>b.name=="ResultTitleButton"));Check(c.TitleOpen && !c.HasSavedMatch,"Results return to title without saving completed match");
                c.StartBattleFromMenu();Check(c.Match.State==BattleMatchState.Running && c.Match.PlayerRecord.healthDamage==0,"Fresh match resets statistics");
            }
            AccessibilitySettings.SetReducedMotion(true);c.Match.Opponent.RestoreResources(100,100,0);c.Match.Tick(15);c.Match.QueueRivalAttack(BattleAttackType.Lock,2);Refresh(c);capture("warning-reduced-motion-phone",390,844);
            Check(c.CombatStatus.Contains("COLUMN 3"),"Reduced motion preserves warning text");AccessibilitySettings.SetReducedMotion(false);
            c.StartRematch();Refresh(c);
            Debug.Log("BATTLE_POLISH_REVIEW_PASS");
        }
    }
}
#endif
