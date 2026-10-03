using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public static class PrototypeUI
    {
        private static Font _font;

        public static readonly Color32 Background = new Color32(17, 20, 29, 255);
        public static readonly Color32 Panel = new Color32(28, 33, 47, 245);
        public static readonly Color32 PanelAlt = new Color32(37, 43, 61, 255);
        public static readonly Color32 Accent = new Color32(91, 191, 255, 255);
        public static readonly Color32 Gold = new Color32(245, 194, 66, 255);
        public static readonly Color32 Danger = new Color32(233, 86, 86, 255);
        public static readonly Color32 CardFace = new Color32(244, 242, 234, 255);
        public static readonly Color32 CardBack = new Color32(44, 91, 145, 255);
        public static readonly Color32 TextLight = new Color32(242, 245, 250, 255);
        public static readonly Color32 TextDark = new Color32(24, 27, 34, 255);

        public static Font Font
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                return _font;
            }
        }

        public static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            return rect;
        }

        public static Image CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                anchorMin,
                anchorMax,
                offsetMin,
                offsetMax);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Color color,
            FontStyle style = FontStyle.Normal)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.fontStyle = style;
            text.text = value;
            text.raycastTarget = false;
            text.resizeTextForBestFit = false;

            return text;
        }

        public static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Color background,
            Color foreground)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = background;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.18f);
            colors.disabledColor = new Color(
                background.r * 0.45f,
                background.g * 0.45f,
                background.b * 0.45f,
                0.8f);
            button.colors = colors;

            Text text = CreateText(
                "Label",
                rect,
                label,
                27,
                TextAnchor.MiddleCenter,
                foreground,
                FontStyle.Bold);

            text.rectTransform.offsetMin = new Vector2(6f, 4f);
            text.rectTransform.offsetMax = new Vector2(-6f, -4f);

            return button;
        }

        public static Slider CreateBar(
            string name,
            Transform parent,
            Color fillColor)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            Slider slider = rect.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;

            Image background = CreatePanel(
                "Background",
                rect,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color32(8, 10, 16, 180));

            Image fill = CreatePanel(
                "Fill",
                rect,
                Vector2.zero,
                Vector2.one,
                new Vector2(3f, 3f),
                new Vector2(-3f, -3f),
                fillColor);

            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = background;

            return slider;
        }

        public static void SetAnchoredBox(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
