using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleFxView : MonoBehaviour
    {
        private Image _flash;
        private Text _banner;

        private float _flashTimer;
        private float _flashDuration;
        private Color _flashColor;

        private float _bannerTimer;
        private float _bannerDuration;
        private Color _bannerColor;

        public static BattleFxView Create(Transform parent)
        {
            RectTransform root = PrototypeUI.CreateRect(
                "BattleFX",
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            var view = root.gameObject.AddComponent<BattleFxView>();
            view.Build();
            return view;
        }

        public void ShowMove(BattleMoveOutcome outcome)
        {
            if (!outcome.Counted)
                return;

            if (outcome.ComboCount >= 5)
            {
                ShowBanner(
                    "COMBO x" + outcome.ComboCount,
                    PrototypeUI.Gold,
                    0.65f);
            }
            else if (outcome.DamageDealt > 0)
            {
                ShowBanner(
                    outcome.DamageDealt + " DAMAGE",
                    PrototypeUI.Accent,
                    0.50f);
            }

            if (outcome.DamageDealt > 0)
                Flash(new Color32(91, 191, 255, 255), 0.16f);
        }

        public void ShowIncomingDamage(int amount)
        {
            if (amount <= 0)
                return;

            ShowBanner("-" + amount + " HP", PrototypeUI.Danger, 0.55f);
            Flash(PrototypeUI.Danger, 0.24f);
        }

        public void ShowAttack(BattleAttackType type, bool incoming)
        {
            string text = incoming
                ? "INCOMING " + type.ToString().ToUpperInvariant()
                : type.ToString().ToUpperInvariant() + "!";

            ShowBanner(
                text,
                incoming ? PrototypeUI.Danger : PrototypeUI.Gold,
                0.75f);

            Flash(
                incoming ? PrototypeUI.Danger : PrototypeUI.Gold,
                0.18f);
        }

        public void ShowInvalid()
        {
            ShowBanner("INVALID MOVE", PrototypeUI.Danger, 0.42f);
        }

        public void ShowResult(BattleMatchState state)
        {
            if (state == BattleMatchState.PlayerWon)
                ShowBanner("VICTORY", PrototypeUI.Gold, 1.15f);
            else if (state == BattleMatchState.OpponentWon)
                ShowBanner("DEFEAT", PrototypeUI.Danger, 1.15f);
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;

            if (_flashTimer > 0f)
            {
                _flashTimer -= delta;
                float normalized = Mathf.Clamp01(_flashTimer / _flashDuration);
                Color color = _flashColor;
                color.a = normalized * 0.18f;
                _flash.color = color;
                _flash.gameObject.SetActive(true);
            }
            else
            {
                _flash.gameObject.SetActive(false);
            }

            if (_bannerTimer > 0f)
            {
                _bannerTimer -= delta;
                float normalized = Mathf.Clamp01(_bannerTimer / _bannerDuration);
                float reveal = 1f - normalized;

                Color color = _bannerColor;
                color.a = Mathf.Clamp01(normalized * 2.2f);
                _banner.color = color;

                float scale = Mathf.Lerp(1.18f, 1f, Mathf.Clamp01(reveal * 4f));
                _banner.rectTransform.localScale =
                    new Vector3(scale, scale, 1f);

                _banner.gameObject.SetActive(true);
            }
            else
            {
                _banner.gameObject.SetActive(false);
            }
        }

        private void Build()
        {
            _flash = PrototypeUI.CreatePanel(
                "Flash",
                transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                Color.clear);

            _flash.raycastTarget = false;
            _flash.gameObject.SetActive(false);

            _banner = PrototypeUI.CreateText(
                "BattleBanner",
                transform,
                "",
                48,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _banner.rectTransform,
                new Vector2(0.08f, 0.43f),
                new Vector2(0.92f, 0.58f),
                Vector2.zero,
                Vector2.zero);

            _banner.gameObject.SetActive(false);
        }

        private void Flash(Color color, float duration)
        {
            _flashColor = color;
            _flashDuration = Mathf.Max(0.01f, duration);
            _flashTimer = _flashDuration;
        }

        private void ShowBanner(string text, Color color, float duration)
        {
            _banner.text = text;
            _bannerColor = color;
            _bannerDuration = Mathf.Max(0.01f, duration);
            _bannerTimer = _bannerDuration;
        }
    }
}
