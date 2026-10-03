using BattleSolitaire.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleFxView : MonoBehaviour
    {
        private Image _flash;
        private Image _impact;
        private Text _banner;

        private float _flashTimer;
        private float _flashDuration;
        private Color _flashColor;

        private float _bannerTimer;
        private float _bannerDuration;
        private Color _bannerColor;

        private float _impactTimer;
        private float _impactDuration;
        private Color _impactColor;

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

        public void ShowMove(BattleMoveOutcome outcome, int healthDamage, int shieldDamage)
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
                    healthDamage>0?healthDamage+" HP DAMAGE":shieldDamage+" SHIELD HIT",
                    PrototypeUI.Accent,
                    1.25f);
            }

            if (outcome.DamageDealt > 0)
                Flash(new Color32(91, 191, 255, 255), 0.16f);
        }

        public void ShowIncomingDamage(int healthDamage, int shieldDamage)
        {
            if (healthDamage<=0 && shieldDamage<=0) return;
            string text=healthDamage>0?"-"+healthDamage+" HP":"SHIELD HELD";
            if(shieldDamage>0) text+="  ·  -"+shieldDamage+" SHIELD";
            ShowBanner(text, healthDamage>0?FantasyUI.Silver:FantasyUI.Blue, 1.5f);
            Flash(PrototypeUI.Danger, 0.24f);
            Impact(PrototypeUI.Danger, 0.32f);
        }

        public void ShowAttack(BattleAttackType type, bool incoming)
        {
            string text = incoming
                ? BattleGameController.AttackName(type) + " LANDED"
                : type.ToString().ToUpperInvariant() + "!";

            ShowBanner(
                text,
                incoming ? PrototypeUI.Danger : PrototypeUI.Gold,
                0.75f);

            Color attackColor =
                incoming
                    ? PrototypeUI.Danger
                    : PrototypeUI.Gold;

            Flash(
                attackColor,
                0.18f);

            Impact(
                attackColor,
                0.34f);
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
            if(AccessibilitySettings.ReducedMotion) { _flashTimer=0; _impactTimer=0; }

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

            if (_impactTimer > 0f)
            {
                _impactTimer -= delta;

                float normalized =
                    Mathf.Clamp01(
                        _impactTimer /
                        _impactDuration);

                float reveal =
                    1f - normalized;

                Color color =
                    _impactColor;

                color.a =
                    Mathf.Sin(
                        Mathf.PI * normalized) *
                    0.55f;

                _impact.color =
                    color;

                float scale =
                    Mathf.Lerp(
                        0.72f,
                        1.32f,
                        reveal);

                _impact.rectTransform
                    .localScale =
                    new Vector3(
                        scale,
                        scale,
                        1f);

                _impact.gameObject
                    .SetActive(true);
            }
            else
            {
                _impact.gameObject
                    .SetActive(false);
            }

            if (_bannerTimer > 0f)
            {
                _bannerTimer -= delta;
                float normalized = Mathf.Clamp01(_bannerTimer / _bannerDuration);
                float reveal = 1f - normalized;

                Color color = _bannerColor;
                color.a = Mathf.Clamp01(normalized * 2.2f);
                _banner.color = color;

                float scale = AccessibilitySettings.ReducedMotion ? 1f : Mathf.Lerp(1.18f, 1f, Mathf.Clamp01(reveal * 4f));
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

            _impact = PrototypeUI.CreatePanel(
                "ImpactFrame",
                transform,
                new Vector2(0.16f, 0.38f),
                new Vector2(0.84f, 0.62f),
                Vector2.zero,
                Vector2.zero,
                Color.clear);

            _impact.raycastTarget = false;

            PrototypeUI.AddOutline(
                _impact,
                PrototypeUI.Gold,
                4f);

            _impact.gameObject
                .SetActive(false);

            _banner = PrototypeUI.CreateText(
                "BattleBanner",
                transform,
                "",
                32,
                TextAnchor.MiddleCenter,
                PrototypeUI.TextLight,
                FontStyle.Bold);

            PrototypeUI.SetAnchoredBox(
                _banner.rectTransform,
                new Vector2(0.08f, 0.343f),
                new Vector2(0.92f, 0.38f),
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

        private void Impact(
            Color color,
            float duration)
        {
            _impactColor = color;
            _impactDuration =
                Mathf.Max(
                    0.01f,
                    duration);

            _impactTimer =
                _impactDuration;
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
