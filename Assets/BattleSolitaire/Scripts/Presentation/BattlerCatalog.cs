using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public enum BattlerId
    {
        Vesper = 0,
        Kael = 1,
        Aldric = 2
    }

    public sealed class BattlerDefinition
    {
        public BattlerId Id { get; }
        public string Name { get; }
        public string Title { get; }
        public string Quote { get; }
        public string SuitGlyph { get; }
        public string TraitLine { get; }
        public Color32 Accent { get; }

        public BattlerDefinition(
            BattlerId id,
            string name,
            string title,
            string quote,
            string suitGlyph,
            string traitLine,
            Color32 accent)
        {
            Id = id;
            Name = name;
            Title = title;
            Quote = quote;
            SuitGlyph = suitGlyph;
            TraitLine = traitLine;
            Accent = accent;
        }
    }

    public static class BattlerCatalog
    {
        private static readonly BattlerDefinition[] All =
        {
            new BattlerDefinition(
                BattlerId.Vesper,
                "Vesper",
                "The Crimson Deal",
                "Every card moves the battle forward.",
                "♦",
                "+HP   +DAMAGE   +COMBO",
                new Color32(215, 62, 79, 255)),
            new BattlerDefinition(
                BattlerId.Kael,
                "Kael",
                "The Resolute",
                "Discipline wins wars.",
                "♠",
                "BALANCED   FOCUSED   STEADY",
                new Color32(68, 171, 232, 255)),
            new BattlerDefinition(
                BattlerId.Aldric,
                "Aldric",
                "The Iron Mark",
                "Order endures.",
                "♣",
                "DEFENSE   SHIELD   CONTROL",
                new Color32(210, 170, 94, 255))
        };

        public static int Count => All.Length;

        public static BattlerDefinition Get(BattlerId id)
        {
            int index = Mathf.Clamp((int)id, 0, All.Length - 1);
            return All[index];
        }

        public static BattlerDefinition GetByIndex(int index)
        {
            return All[Mathf.Clamp(index, 0, All.Length - 1)];
        }
    }
}
