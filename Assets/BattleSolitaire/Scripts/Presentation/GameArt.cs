using System.Collections.Generic;
using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public static class GameArt
    {
        private static readonly Dictionary<string, Texture2D> Cache =
            new Dictionary<string, Texture2D>();

        public static Texture2D GetBattlerPortrait(
            BattlerId id)
        {
            switch (id)
            {
                case BattlerId.Vesper:
                    return Load("Art/Battlers/Vesper");
                case BattlerId.Aldric:
                    return Load("Art/Battlers/Aldric");
                default:
                    return Load("Art/Battlers/Kael");
            }
        }

        public static Texture2D GetCardBack()
        {
            return Load("Art/CardBack");
        }

        public static Texture2D GetCrest()
        {
            return Load("Art/AppCrest");
        }

        private static Texture2D Load(
            string resourcePath)
        {
            if (Cache.TryGetValue(
                    resourcePath,
                    out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture =
                Resources.Load<Texture2D>(
                    resourcePath);

            Cache[resourcePath] = texture;
            return texture;
        }
    }
}
