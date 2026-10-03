using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public static class AccessibilitySettings
    {
        private const string MotionKey="BattleSolitaire.ReducedMotion";
        public static bool ReducedMotion { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load() { ReducedMotion=PlayerPrefs.GetInt(MotionKey,0)!=0; }
        public static void SetReducedMotion(bool enabled)
        {
            ReducedMotion=enabled; PlayerPrefs.SetInt(MotionKey,enabled?1:0); PlayerPrefs.Save();
        }
    }
}
