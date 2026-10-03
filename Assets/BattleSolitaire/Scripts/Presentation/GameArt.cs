using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public static class GameArt
    {
        private static readonly Dictionary<string, Texture2D> Cache =
            new Dictionary<string, Texture2D>();

        public static Texture2D GetBattlerPortrait(
            BattlerId id)
        {
            Texture2D atlas = Load("Art/Premium/BattlerAtlas");
            if (atlas != null) return atlas;
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
            return Load("Art/Premium/RoyalCardBack") ?? Load("Art/CardBack");
        }

        public static Texture2D GetCrest()
        {
            return Load("Art/AppCrest");
        }

        public static Texture2D GetBackdrop() => Load("Art/Premium/CastleBackdrop");
        public static Texture2D GetFelt() => Load("Art/Premium/EmeraldFelt");
        public static void SetPortrait(RawImage image, BattlerId id)
        {
            AspectFillRawImage fill = image.GetComponent<AspectFillRawImage>();
            if (fill == null) fill = image.gameObject.AddComponent<AspectFillRawImage>();
            Texture2D atlas = Load("Art/Premium/BattlerAtlas");
            fill.FocalPoint = new Vector2(0.5f, 0.90f);
            fill.SetSourceRegion(atlas != null ? new Rect((int)id / 3f, 0f, 1f / 3f, 1f) : new Rect(0,0,1,1));
            fill.SetTexture(GetBattlerPortrait(id));
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
