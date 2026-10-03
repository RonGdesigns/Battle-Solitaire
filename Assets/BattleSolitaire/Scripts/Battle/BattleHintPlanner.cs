using System;
using System.Collections.Generic;
using System.Linq;
using BattleSolitaire.Core;

namespace BattleSolitaire.Battle
{
    public sealed class BattleHint
    {
        public string Text { get; }
        public bool HasMove { get; }
        public bool Waiting { get; }
        public BattleHint(string text,bool hasMove=false,bool waiting=false)
        { Text=text; HasMove=hasMove; Waiting=waiting; }
    }

    /// <summary>A bounded, nonmutating search. Failure means no route found, never proof of an unwinnable deal.</summary>
    public static class BattleHintPlanner
    {
        private const int SearchLimit=240, MaxDepth=3;
        private sealed class Node
        {
            public SolitaireGame Game;
            public string First;
            public string Next;
            public int Depth;
        }
        private sealed class Step
        {
            public SolitaireGame Game;
            public string Text;
        }
        public static BattleHint Find(BattleParticipant player)
        {
            string route=Search(player.Game,player.CanUseColumn,!player.Disruptions.HasFog);
            if(route!=null) return new BattleHint(route,true);
            bool affected=player.Disruptions.HasFog || Enumerable.Range(0,7).Any(c=>!player.CanUseColumn(c));
            if(affected && Search(player.Game,c=>true,true)!=null)
                return new BattleHint("A route opens when the disruption expires. Resume and watch the effect countdown at the bottom of your board.",false,true);
            return new BattleHint("No useful move found after checking the stock, recycling, and short rearrangements. A longer route may still exist. Keep playing, or choose New Battle for a fresh deal.");
        }
        private static string Search(SolitaireGame original,Func<int,bool> usable,bool foundationsVisible)
        {
            int hidden=Hidden(original), foundations=original.GetFoundationCardCount(), deck=original.Stock.Count+original.Waste.Count;
            var queue=new Queue<Node>();
            var seen=new HashSet<string>{Key(original)};
            queue.Enqueue(new Node{Game=original});
            int expanded=0;
            while(queue.Count>0 && expanded++<SearchLimit)
            {
                Node node=queue.Dequeue();
                string fallback=null;
                foreach(Step step in Steps(node.Game,usable,foundationsVisible))
                {
                    string first=node.First ?? step.Text;
                    string next=node.Next ?? (node.First==null?null:step.Text);
                    string guidance=first+(next==null?"":"\nNext: "+next);
                    // Returning a foundation card and immediately replacing it is not progress.
                    if(Hidden(step.Game)<hidden) return guidance;
                    if(step.Game.GetFoundationCardCount()>foundations || step.Game.Stock.Count+step.Game.Waste.Count<deck)
                        fallback=fallback ?? guidance;
                    if(node.Depth+1<MaxDepth && seen.Count<SearchLimit && seen.Add(Key(step.Game)))
                        queue.Enqueue(new Node{Game=step.Game,First=first,Next=next,Depth=node.Depth+1});
                }
                if(fallback!=null) return fallback;
            }
            return null;
        }
        private static IEnumerable<Step> Steps(SolitaireGame game,Func<int,bool> usable,bool foundationsVisible)
        {
            // Prefer exposing hidden cards, then growing foundations, then planning rearrangements.
            for(int source=0;source<7;source++)
            {
                if(!usable(source)) continue;
                var pile=game.Tableau[source];
                for(int index=0;index<pile.Count;index++)
                {
                    if(!SolitaireRules.IsValidTableauSequence(pile,index)) continue;
                    for(int dest=0;dest<7;dest++)
                    {
                        if(source==dest || !usable(dest) || !Fits(pile[index],game.Tableau[dest])) continue;
                        if(index==0 && game.Tableau[dest].Count==0) continue; // Equivalent king-column swap.
                        var next=Copy(game);next.MoveTableauToTableau(source,index,dest);
                        yield return new Step{Game=next,Text="Move the "+pile[index]+(index<pile.Count-1?" and its run":"")+" from column "+(source+1)+" to column "+(dest+1)+"."};
                    }
                }
                if(foundationsVisible && pile.Count>0 && SolitaireRules.CanPlaceOnFoundation(pile[pile.Count-1],game.Foundations[pile[pile.Count-1].Suit]))
                {
                    var next=Copy(game);next.MoveTableauToFoundation(source);
                    yield return new Step{Game=next,Text="Move the "+pile[pile.Count-1]+" from column "+(source+1)+" to a foundation."};
                }
            }
            foreach(var step in WasteSteps(game,usable,foundationsVisible)) yield return step;

            // Inspect one complete draw-one stock cycle, including cards currently buried in the waste.
            var drawing=Copy(game);
            var drawn=new HashSet<string>();
            if(game.Waste.Count>0) drawn.Add(game.Waste[game.Waste.Count-1].Id);
            int limit=2*(game.Stock.Count+game.Waste.Count)+1;
            for(int draw=0;draw<limit && drawing.DrawCard();draw++)
            {
                if(drawing.Waste.Count==0 || !drawn.Add(drawing.Waste[drawing.Waste.Count-1].Id)) continue;
                foreach(var step in WasteSteps(drawing,usable,foundationsVisible))
                {
                    step.Text=game.Stock.Count==0?"Recycle the waste, then draw until you reach a playable card.":"Draw through the stock for a playable card. Recycle the waste if needed.";
                    yield return step;
                }
            }
            if(!foundationsVisible) yield break;
            foreach(Suit suit in Enum.GetValues(typeof(Suit)))
            {
                var pile=game.Foundations[suit];if(pile.Count==0) continue;
                for(int dest=0;dest<7;dest++)
                {
                    if(!usable(dest) || !Fits(pile[pile.Count-1],game.Tableau[dest])) continue;
                    var next=Copy(game);next.MoveFoundationToTableau(suit,dest);
                    yield return new Step{Game=next,Text="Bring the "+pile[pile.Count-1]+" back from its foundation to column "+(dest+1)+" to open a route."};
                }
            }
        }
        private static IEnumerable<Step> WasteSteps(SolitaireGame game,Func<int,bool> usable,bool foundationsVisible)
        {
            if(game.Waste.Count==0) yield break;
            CardState card=game.Waste[game.Waste.Count-1];
            if(foundationsVisible && SolitaireRules.CanPlaceOnFoundation(card,game.Foundations[card.Suit]))
            {
                var next=Copy(game);next.MoveWasteToFoundation();
                yield return new Step{Game=next,Text="Move the "+card+" from the waste to a foundation."};
            }
            for(int dest=0;dest<7;dest++)
            {
                if(!usable(dest) || !Fits(card,game.Tableau[dest])) continue;
                var next=Copy(game);next.MoveWasteToTableau(dest);
                yield return new Step{Game=next,Text="Move the "+card+" from the waste to column "+(dest+1)+"."};
            }
        }
        private static bool Fits(CardState card,List<CardState> pile) => pile.Count==0?SolitaireRules.CanPlaceOnEmptyTableau(card):SolitaireRules.CanPlaceOnTableau(card,pile[pile.Count-1]);
        private static int Hidden(SolitaireGame game) => game.Tableau.Sum(p=>p.Count(c=>!c.IsFaceUp));
        private static IEnumerable<List<CardState>> Piles(SolitaireGame game)
        {
            yield return game.Stock; yield return game.Waste;
            foreach(Suit suit in Enum.GetValues(typeof(Suit))) yield return game.Foundations[suit];
            foreach(var pile in game.Tableau) yield return pile;
        }
        private static SolitaireGame Copy(SolitaireGame game)
        {
            var copy=new SolitaireGame(game.Seed);
            var target=Piles(copy).GetEnumerator();
            foreach(var pile in Piles(game))
            {
                target.MoveNext();target.Current.Clear();
                foreach(var card in pile) target.Current.Add(new CardState(card.Suit,card.Rank,card.IsFaceUp));
            }
            return copy;
        }
        private static string Key(SolitaireGame game) => string.Join("|",Piles(game).Select(p=>string.Join(",",p.Select(c=>(int)c.Suit*26+(int)c.Rank*2+(c.IsFaceUp?1:0)))));
    }
}
