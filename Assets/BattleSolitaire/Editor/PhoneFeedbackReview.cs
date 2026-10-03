#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
    public static class PhoneFeedbackReview
    {
        private static Camera _pointerCamera;
        private static RenderTexture _pointerTexture;
        private static void Check(bool ok,string label) { if(!ok) throw new InvalidOperationException(label); }
        private static List<CardView> Views(BattleBoardView board)=>(List<CardView>)typeof(BattleBoardView).GetField("_renderedCards",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(board);
        private static CardState Take(SolitaireGame game,Suit suit,Rank rank)
        {
            var card=game.Stock.First(c=>c.Suit==suit && c.Rank==rank);game.Stock.Remove(card);card.IsFaceUp=true;return card;
        }
        private static SolitaireGame Fixture(BattleGameController controller)
        {
            controller.StartRematch();var game=controller.Match.Player.Game;
            var all=new List<CardState>(game.Stock);all.AddRange(game.Waste);
            foreach(var pile in game.Tableau) { all.AddRange(pile);pile.Clear(); }
            foreach(var pile in game.Foundations.Values) { all.AddRange(pile);pile.Clear(); }
            game.Stock.Clear();game.Waste.Clear();foreach(var card in all) {card.IsFaceUp=false;game.Stock.Add(card);}
            return game;
        }
        private static void Drop(BattleBoardView board,CardView source,Vector2 point,bool route=true)
        {
            var data=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};
            data.pointerPressRaycast=new RaycastResult { module=board.GetComponentInParent<GraphicRaycaster>() };
            source.OnBeginDrag(data);Check(CardView.CurrentDrag==source,"Drag starts");source.OnDrag(data);
            if(route)
            {
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
                Check(hits.Count>0,"Destination is raycastable");
                Debug.Log("DROP_REVIEW point="+point+" hits="+string.Join(",",hits.Take(6).Select(h=>h.gameObject.name+" under "+h.gameObject.transform.parent.name)));
                Check(ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.dropHandler)!=null,"Drop reaches a destination handler");
            }
            source.OnEndDrag(data);
        }
        private static Vector2 ScreenPoint(RectTransform rect,Vector2 normalized)
            => RectTransformUtility.WorldToScreenPoint(_pointerCamera,rect.TransformPoint(new Vector2(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,normalized.x),Mathf.Lerp(rect.rect.yMin,rect.rect.yMax,normalized.y))));
        private static void Overlay(Canvas canvas)
        {
            if(_pointerCamera==null)
            {
                _pointerCamera=new GameObject("PointerReviewCamera").AddComponent<Camera>();
                _pointerCamera.orthographic=true;_pointerCamera.nearClipPlane=.01f;
                _pointerTexture=new RenderTexture(390,844,24);_pointerCamera.targetTexture=_pointerTexture;
            }
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=_pointerCamera;canvas.planeDistance=1;
            canvas.GetComponent<CanvasScaler>().enabled=false;canvas.scaleFactor=Mathf.Sqrt(390f*844f/(1080f*1920f));Canvas.ForceUpdateCanvases();
            var board=UnityEngine.Object.FindAnyObjectByType<BattleBoardView>();
            typeof(BattleBoardView).GetMethod("FitLayout",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(board,null);
        }
        public static void Run(BattleGameController c,Canvas canvas,Action<string,int,int> capture)
        {
            var board=UnityEngine.Object.FindAnyObjectByType<BattleBoardView>();
            // Reproduce king through five, then drop a four at the end of the stack.
            foreach(int hit in new[]{0,1,2,3})
            {
                var game=Fixture(c);
                for(int rank=13;rank>=5;rank--) game.Tableau[0].Add(Take(game,rank%2==1?Suit.Spades:Suit.Hearts,(Rank)rank));
                game.Tableau[1].Add(Take(game,Suit.Hearts,Rank.Four));
                Overlay(canvas);board.Refresh();Canvas.ForceUpdateCanvases();
                var target=Views(board).First(v=>v.Column==0 && v.Index==(hit==0?0:8));
                var point=ScreenPoint((RectTransform)target.transform,new Vector2(.5f,hit==2?-.12f:.35f));
                var source=Views(board).First(v=>v.Column==1);
                Drop(board,source,point,hit!=3);
                Check(game.Tableau[0].Count==10 && game.Tableau[0][9].Rank==Rank.Four && game.Tableau[1].Count==0,"Four lands on the five from top, bottom, below-stack, or drag-end fallback");
            }
            capture("fixed-stack-drop-phone",390,844);
            // Rejected moves and multi-card drags use the same physical destination path.
            var movingGame=c.Match.Player.Game;
            movingGame.Tableau[1].Add(Take(movingGame,Suit.Spades,Rank.Three));
            movingGame.Tableau[1].Add(Take(movingGame,Suit.Hearts,Rank.Two));
            Overlay(canvas);board.Refresh();Canvas.ForceUpdateCanvases();
            var tail=Views(board).First(v=>v.Column==0 && v.Card.Rank==Rank.Four);
            Drop(board,Views(board).First(v=>v.Column==1 && v.Index==0),ScreenPoint((RectTransform)tail.transform,new Vector2(.5f,.35f)));
            Check(movingGame.Tableau[0].Count==12 && movingGame.Tableau[1].Count==0,"Multi-card run lands at the tail of a long stack");
            movingGame.Tableau[1].Add(Take(movingGame,Suit.Diamonds,Rank.Ace));board.Refresh();Canvas.ForceUpdateCanvases();
            tail=Views(board).First(v=>v.Column==0 && v.Card.Rank==Rank.Two);
            Drop(board,Views(board).First(v=>v.Column==1),ScreenPoint((RectTransform)tail.transform,new Vector2(.5f,.35f)));
            Check(movingGame.Tableau[0].Count==12 && movingGame.Tableau[1].Count==1 && CardView.CurrentDrag==null,"Same-color invalid drop stays in its original pile");
            UnityEngine.Object.DestroyImmediate(_pointerCamera.gameObject);UnityEngine.Object.DestroyImmediate(_pointerTexture);_pointerCamera=null;
            // Every suit can start every screen slot; later cards must follow its suit.
            foreach(Suit suit in Enum.GetValues(typeof(Suit))) for(int slot=0;slot<4;slot++)
            {
                var game=Fixture(c);game.Waste.Add(Take(game,suit,Rank.Ace));board.Refresh();
                Check(board.TryMoveCardToFoundationSlot(Views(board).First(v=>v.SourceKind==CardSourceKind.Waste),slot),"Any ace fits any empty slot");
                Check(c.FoundationSlots[slot]==(int)suit && game.Foundations[suit].Count==1,"Foundation binds to the chosen ace");
                game.Waste.Add(Take(game,suit,Rank.Two));board.Refresh();
                Check(board.TryMoveCardToFoundationSlot(Views(board).First(v=>v.SourceKind==CardSourceKind.Waste),slot),"Two follows its ace in the chosen slot");
                c.SaveCheckpoint();var saved=MatchSaveStore.Load(out string notice);
                Check(saved!=null && saved.foundationSlots[slot]==(int)suit,"Foundation order persists on disk");
            }
            var shown=Fixture(c);
            foreach(Suit suit in Enum.GetValues(typeof(Suit))) shown.Tableau[(int)suit].Add(Take(shown,suit,Rank.Ace));
            shown.Waste.Add(Take(shown,Suit.Hearts,Rank.King));board.Refresh();
            Check(!board.TryMoveCardToFoundationSlot(Views(board).First(v=>v.SourceKind==CardSourceKind.Waste),2),"Non-ace cannot start an empty foundation");
            capture("all-suits-phone",390,844);capture("all-suits-desktop",1280,1920);
            foreach(var icon in board.GetComponentsInChildren<SuitIcon>()) Check(icon.canvasRenderer.GetMesh()!=null && icon.canvasRenderer.GetMesh().vertexCount>0,"Suit geometry renders without fonts");
            Check(board.TryMoveCardToFoundationSlot(Views(board).First(v=>v.Column==3),0),"Spade ace can occupy the first slot");
            Check(!board.TryMoveCardToFoundationSlot(Views(board).First(v=>v.Column==2),0),"A different suit cannot enter an occupied foundation");
            c.Match.Player.Disruptions.AddFog(2f);board.RefreshStatus();capture("fog-readable-tableau",390,844);
            foreach(var view in Views(board).Where(v=>v.SourceKind!=CardSourceKind.Foundation && v.Card.IsFaceUp))
                Check(view.transform.Find("TopSuit").gameObject.activeSelf,"Fog leaves tableau and waste suits visible");
            c.Match.Player.Disruptions.RestoreFog(0,0);
            c.SaveCheckpoint();string checkpoint=JsonUtility.ToJson(c.CaptureCheckpoint());c.ShowFrontEnd();c.ContinueSavedBattle();
            Check(checkpoint==JsonUtility.ToJson(c.CaptureCheckpoint()),"Chosen foundation positions survive Continue");c.ResumeBattle();
            var match=new BattleMatch(5,6);match.Opponent.RestoreResources(100,100,0);
            Check(!match.TryAttack(BattleSide.Opponent,BattleAttackType.Fog),"Opening grace prevents immediate Fog");
            match.Tick(15f);Check(match.TryAttack(BattleSide.Opponent,BattleAttackType.Fog),"Fog available after opening grace");
            Check(match.Player.Disruptions.FogTimeRemaining==2f,"Fog lasts two seconds");match.Tick(2f);
            int energy=match.Opponent.Energy;
            Check(!match.Player.Disruptions.HasFog && !match.TryAttack(BattleSide.Opponent,BattleAttackType.Fog) && match.Opponent.Energy==energy,"Fog cannot repeat during cooldown or spend energy on rejection");
            match.Tick(18f);Check(match.TryAttack(BattleSide.Opponent,BattleAttackType.Fog),"Fog recovers after twenty seconds");
            c.StartRematch();c.SaveCheckpoint();c.OpenPause();
            var pause=UnityEngine.Object.FindAnyObjectByType<PauseOverlay>();pause.ConfirmQuitBattle();capture("quit-confirmation-phone",390,844);
            pause.Back();Check(c.Paused && c.HasSavedMatch && !c.TitleOpen,"Cancel quit preserves the paused battle");
            int matches=c.Profile.Matches;pause.ConfirmQuitBattle();
            pause.GetComponentsInChildren<Button>().First(b=>b.name=="NewBattleButton").onClick.Invoke();
            Check(c.TitleOpen && !c.HasSavedMatch && MatchSaveStore.Load(out string message)==null && c.Profile.Matches==matches,"Quit ends the battle, removes its checkpoint, and preserves career");
            capture("title-after-quit",390,844);
            c.ShowFrontEnd();c.StartBattleFromMenu();
            Debug.Log("PHONE_FEEDBACK_REVIEW_PASS: full-column raycast drops, all ace/slot combinations, portable suits, foundation persistence, Fog grace/cooldown/readability, quit/cancel, title flow.");
        }
    }
}
#endif
