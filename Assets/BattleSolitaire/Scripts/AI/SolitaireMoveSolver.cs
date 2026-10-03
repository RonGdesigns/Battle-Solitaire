using BattleSolitaire.Core;

namespace BattleSolitaire.Battle
{
    public readonly struct SolitaireAIAction
    {
        public bool Acted { get; }
        public bool DrewCard { get; }
        public MoveResult Move { get; }

        public SolitaireAIAction(bool acted, bool drewCard, MoveResult move)
        {
            Acted = acted;
            DrewCard = drewCard;
            Move = move;
        }

        public static SolitaireAIAction None =>
            new SolitaireAIAction(false, false, MoveResult.Failed());
    }

    public sealed class SolitaireMoveSolver
    {
        private struct TableauCandidate
        {
            public bool Valid;
            public int Source;
            public int StartIndex;
            public int Destination;
            public int Score;
        }

        public SolitaireAIAction TryAct(BattleMatch match, BattleSide side)
        {
            BattleParticipant participant =
                side == BattleSide.Player ? match.Player : match.Opponent;

            SolitaireGame game = participant.Game;

            TableauCandidate best = FindBestTableauMove(match, side, game);

            // Revealing hidden information is the AI's first priority.
            if (best.Valid && best.Score >= 100)
            {
                MoveResult move = game.MoveTableauToTableau(
                    best.Source,
                    best.StartIndex,
                    best.Destination);

                return new SolitaireAIAction(move.Success, false, move);
            }

            // Safe foundation progress is usually better than rearranging visible cards.
            for (int column = 0; column < game.Tableau.Count; column++)
            {
                if (!match.CanUseColumn(side, column))
                    continue;

                if (game.Tableau[column].Count == 0)
                    continue;

                CardState top = game.Tableau[column][game.Tableau[column].Count - 1];
                if (!SolitaireRules.CanPlaceOnFoundation(
                        top,
                        game.Foundations[top.Suit]))
                {
                    continue;
                }

                MoveResult move = game.MoveTableauToFoundation(column);
                if (move.Success)
                    return new SolitaireAIAction(true, false, move);
            }

            if (game.Waste.Count > 0)
            {
                CardState waste = game.Waste[game.Waste.Count - 1];

                if (SolitaireRules.CanPlaceOnFoundation(
                        waste,
                        game.Foundations[waste.Suit]))
                {
                    MoveResult move = game.MoveWasteToFoundation();
                    if (move.Success)
                        return new SolitaireAIAction(true, false, move);
                }
            }

            if (best.Valid)
            {
                MoveResult move = game.MoveTableauToTableau(
                    best.Source,
                    best.StartIndex,
                    best.Destination);

                if (move.Success)
                    return new SolitaireAIAction(true, false, move);
            }

            if (game.Waste.Count > 0)
            {
                CardState waste = game.Waste[game.Waste.Count - 1];

                for (int destination = 0;
                     destination < game.Tableau.Count;
                     destination++)
                {
                    if (!match.CanUseColumn(side, destination))
                        continue;

                    var target = game.Tableau[destination];

                    bool valid = target.Count == 0
                        ? SolitaireRules.CanPlaceOnEmptyTableau(waste)
                        : SolitaireRules.CanPlaceOnTableau(
                            waste,
                            target[target.Count - 1]);

                    if (!valid)
                        continue;

                    MoveResult move = game.MoveWasteToTableau(destination);
                    if (move.Success)
                        return new SolitaireAIAction(true, false, move);
                }
            }

            bool drew = game.DrawCard();
            return drew
                ? new SolitaireAIAction(true, true, MoveResult.Failed())
                : SolitaireAIAction.None;
        }

        private static TableauCandidate FindBestTableauMove(
            BattleMatch match,
            BattleSide side,
            SolitaireGame game)
        {
            TableauCandidate best = default;
            best.Score = int.MinValue;

            for (int source = 0; source < game.Tableau.Count; source++)
            {
                if (!match.CanUseColumn(side, source))
                    continue;

                var sourceCards = game.Tableau[source];

                for (int start = 0; start < sourceCards.Count; start++)
                {
                    CardState moving = sourceCards[start];

                    if (!moving.IsFaceUp ||
                        !SolitaireRules.IsValidTableauSequence(sourceCards, start))
                    {
                        continue;
                    }

                    for (int destination = 0;
                         destination < game.Tableau.Count;
                         destination++)
                    {
                        if (destination == source ||
                            !match.CanUseColumn(side, destination))
                        {
                            continue;
                        }

                        var destinationCards = game.Tableau[destination];

                        // Moving a complete visible stack from one empty lane to
                        // another is legal but strategically useless and can loop.
                        if (start == 0 &&
                            destinationCards.Count == 0 &&
                            sourceCards[0].IsFaceUp)
                        {
                            continue;
                        }

                        bool valid = destinationCards.Count == 0
                            ? SolitaireRules.CanPlaceOnEmptyTableau(moving)
                            : SolitaireRules.CanPlaceOnTableau(
                                moving,
                                destinationCards[destinationCards.Count - 1]);

                        if (!valid)
                            continue;

                        int score = 10;

                        bool revealsHidden =
                            start > 0 && !sourceCards[start - 1].IsFaceUp;

                        bool clearsColumn = start == 0;

                        if (revealsHidden)
                            score += 120;

                        if (clearsColumn)
                            score += 45;

                        // Favor moves that relocate larger useful sequences.
                        score += sourceCards.Count - start;

                        if (!best.Valid || score > best.Score)
                        {
                            best.Valid = true;
                            best.Source = source;
                            best.StartIndex = start;
                            best.Destination = destination;
                            best.Score = score;
                        }
                    }
                }
            }

            return best;
        }
    }
}
