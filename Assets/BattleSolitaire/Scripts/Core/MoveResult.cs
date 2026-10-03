namespace BattleSolitaire.Core
{
    public readonly struct MoveResult
    {
        public bool Success { get; }
        public bool RevealedHiddenCard { get; }
        public bool ClearedColumn { get; }
        public bool FoundationMove { get; }

        public MoveResult(
            bool success,
            bool revealedHiddenCard = false,
            bool clearedColumn = false,
            bool foundationMove = false)
        {
            Success = success;
            RevealedHiddenCard = revealedHiddenCard;
            ClearedColumn = clearedColumn;
            FoundationMove = foundationMove;
        }

        public static MoveResult Failed() => new MoveResult(false);
        public static MoveResult Succeeded(
            bool revealedHiddenCard = false,
            bool clearedColumn = false,
            bool foundationMove = false)
            => new MoveResult(true, revealedHiddenCard, clearedColumn, foundationMove);
    }
}
