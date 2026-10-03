using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleSolitaire.Battle;
using BattleSolitaire.Core;
using UnityEngine;

namespace BattleSolitaire.Presentation
{
    [Serializable] public sealed class SavedPile { public int[] cards; }
    [Serializable] public sealed class SavedBlocker { public int column, moves; public float seconds; }
    [Serializable] public sealed class SavedParticipant
    {
        public int seed, health, energy, shield, combo;
        public float comboSeconds, fog, fogProtection;
        public float[] locks;
        public SavedBlocker[] blockers;
        // Stock, waste, four foundations, then seven tableau columns.
        public SavedPile[] piles;
        public static SavedParticipant Capture(BattleParticipant actor)
        {
            return new SavedParticipant {
                seed=actor.Game.Seed, health=actor.Health, energy=actor.Energy, shield=actor.Shield,
                combo=actor.Combo.Count, comboSeconds=actor.Combo.SecondsSinceLastMove,
                fog=actor.Disruptions.FogTimeRemaining, fogProtection=actor.Disruptions.FogProtectionRemaining,
                locks=Enumerable.Range(0,7).Select(actor.Disruptions.GetLockTimeRemaining).ToArray(),
                blockers=actor.Disruptions.Blockers.Select(b=>new SavedBlocker { column=b.Column,moves=b.MovesRemaining,seconds=b.TimeRemaining }).ToArray(),
                piles=Piles(actor.Game).Select(p=>new SavedPile { cards=p.Select(c=>(int)c.Suit*13+(int)c.Rank-1+(c.IsFaceUp?52:0)).ToArray() }).ToArray()
            };
        }
        private static IEnumerable<List<CardState>> Piles(SolitaireGame game)
        {
            yield return game.Stock; yield return game.Waste;
            foreach(Suit suit in Enum.GetValues(typeof(Suit))) yield return game.Foundations[suit];
            foreach(var column in game.Tableau) yield return column;
        }
        public void Validate()
        {
            if(piles==null || piles.Length!=13 || locks==null || locks.Length!=7 || blockers==null || blockers.Length>7)
                throw new InvalidDataException("Incomplete saved board.");
            var seen=new HashSet<int>();
            for(int p=0;p<piles.Length;p++)
            {
                if(piles[p]?.cards==null || piles[p].cards.Length>52) throw new InvalidDataException("Invalid saved pile.");
                for(int i=0;i<piles[p].cards.Length;i++)
                {
                    int value=piles[p].cards[i];
                    if(value<0 || value>=104 || !seen.Add(value%52)) throw new InvalidDataException("Invalid or duplicate saved card.");
                    if(p==0 && value>=52 || p>=1 && p<=5 && value<52) throw new InvalidDataException("Invalid card visibility.");
                    if(p>=2 && p<=5 && (value%52/13!=p-2 || value%13!=i)) throw new InvalidDataException("Invalid foundation.");
                }
            }
            if(seen.Count!=52 || health<1 || health>100 || energy<0 || energy>100 || shield<0 || shield>40 || combo<0 || combo>100000)
                throw new InvalidDataException("Invalid saved battle values.");
            if(!Finite(comboSeconds,100000) || !Finite(fog,4f) || !Finite(fogProtection,BattleTuning.FogCooldownSeconds) || locks.Any(t=>!Finite(t,BattleTuning.LockDurationSeconds)))
                throw new InvalidDataException("Invalid saved timer.");
            var columns=new HashSet<int>();
            foreach(var b in blockers)
                if(b==null || b.column<0 || b.column>6 || !columns.Add(b.column) || b.moves<1 || b.moves>5 || !Finite(b.seconds,BattleTuning.BlockerMaxDurationSeconds))
                    throw new InvalidDataException("Invalid saved disruption.");
        }
        internal static bool Finite(float value,float max) => !float.IsNaN(value) && !float.IsInfinity(value) && value>=0 && value<=max;
        public void Restore(BattleParticipant actor)
        {
            int index=0;
            foreach(var pile in Piles(actor.Game))
            {
                pile.Clear();
                foreach(int value in piles[index++].cards)
                    pile.Add(new CardState((Suit)(value%52/13),(Rank)(value%13+1),value>=52));
            }
            actor.RestoreResources(health,energy,shield);
            actor.Combo.Restore(combo,comboSeconds);
            actor.Disruptions.RestoreFog(fog,fogProtection);
            for(int i=0;i<7;i++) actor.Disruptions.AddLock(i,locks[i]);
            foreach(var b in blockers) actor.Disruptions.AddBlocker(b.column,b.moves,b.seconds);
        }
    }
    [Serializable] public sealed class SavedMatch
    {
        public int version=1;
        public int playerBattler, opponentBattler, difficulty, maxCombo;
        public SavedParticipant player, opponent;
        public SavedAI ai;
        public int[] foundationSlots;
        public void Validate()
        {
            if(version!=1 || playerBattler<0 || playerBattler>2 || opponentBattler<0 || opponentBattler>2 || difficulty<0 || difficulty>2 || maxCombo<0 || maxCombo>100000 || player==null || opponent==null || ai==null)
                throw new InvalidDataException("Unsupported or incomplete saved match.");
            player.Validate(); opponent.Validate(); ai.Validate();
            if(foundationSlots!=null)
            {
                if(foundationSlots.Length!=4) throw new InvalidDataException("Invalid foundation layout.");
                var assigned=new HashSet<int>();
                foreach(int suit in foundationSlots)
                    if(suit < -1 || suit > 3 || (suit>=0 && !assigned.Add(suit))) throw new InvalidDataException("Invalid foundation layout.");
            }
        }
    }

    /// <summary>Validated, versioned local checkpoint with an atomic replacement and previous-copy recovery.</summary>
    public static class MatchSaveStore
    {
        public static string DirectoryPath => Application.isBatchMode
            ? Path.Combine(Application.temporaryCachePath,"BattleSolitaireBatch-"+System.Diagnostics.Process.GetCurrentProcess().Id)
            : Application.persistentDataPath;
        public static string SavePath => Path.Combine(DirectoryPath,"active-match-v1.json");
        public static bool Save(SavedMatch snapshot)
        {
            try
            {
                snapshot.Validate();
                Directory.CreateDirectory(DirectoryPath);
                string temp=SavePath+".tmp";
                using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))
                using(var writer=new StreamWriter(stream)) { writer.Write(JsonUtility.ToJson(snapshot)); writer.Flush(); stream.Flush(true); }
                if(File.Exists(SavePath)) File.Replace(temp,SavePath,SavePath+".bak");
                else File.Move(temp,SavePath);
                return true;
            }
            catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is InvalidDataException)
            { Debug.LogWarning("Match checkpoint unavailable: "+e.Message); return false; }
        }
        public static SavedMatch Load(out string notice)
        {
            notice="";
            foreach(string path in new[]{SavePath,SavePath+".bak"})
            {
                if(!File.Exists(path)) continue;
                try
                {
                    if(new FileInfo(path).Length>262144) throw new InvalidDataException("Save is too large.");
                    var snapshot=JsonUtility.FromJson<SavedMatch>(File.ReadAllText(path));
                    if(snapshot==null) throw new InvalidDataException("Empty save.");
                    snapshot.Validate();
                    notice=path.EndsWith(".bak")?"Recovered the previous checkpoint.":"Your unfinished battle is ready.";
                    return snapshot;
                }
                catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
                { notice="The saved battle could not be restored. Your career is safe."; }
            }
            return null;
        }
        public static void Clear()
        {
            foreach(string path in new[]{SavePath,SavePath+".bak",SavePath+".tmp"})
                try { if(File.Exists(path)) File.Delete(path); }
                catch(Exception e) when(e is IOException || e is UnauthorizedAccessException) { Debug.LogWarning("Could not clear checkpoint: "+e.Message); }
        }
    }
}
