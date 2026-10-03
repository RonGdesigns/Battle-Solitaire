namespace BattleSolitaire.Core
{
    public enum MoveKind
    {
        None,
        TableauToTableau,
        WasteToTableau,
        WasteToFoundation,
        TableauToFoundation,
        FoundationToTableau
    }

    public readonly struct MoveResult
    {
        public bool Success { get; }
        public MoveKind Kind { get; }
        public bool RevealedHiddenCard { get; }
        public bool ClearedColumn { get; }
        public bool FoundationMove { get; }

        public bool CountsForBattleProgress =>
            Success &&
            Kind != MoveKind.None &&
            Kind != MoveKind.FoundationToTableau;

        public MoveResult(
            bool success,
            MoveKind kind = MoveKind.None,
            bool revealedHiddenCard = false,
            bool clearedColumn = false,
            bool foundationMove = false)
        {
            Success = success;
            Kind = kind;
            RevealedHiddenCard = revealedHiddenCard;
            ClearedColumn = clearedColumn;
            FoundationMove = foundationMove;
        }

        public static MoveResult Failed() => new MoveResult(false);

        public static MoveResult Succeeded(
            MoveKind kind,
            bool revealedHiddenCard = false,
            bool clearedColumn = false,
            bool foundationMove = false)
            => new MoveResult(
                true,
                kind,
                revealedHiddenCard,
                clearedColumn,
                foundationMove);
    }
}
