using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public sealed class BattleFeedback : MonoBehaviour
    {
        private const string SoundKey = "BattleSolitaire.SoundEnabled";
        private const string HapticsKey = "BattleSolitaire.HapticsEnabled";

        private AudioSource _source;
        private AudioClip _move;
        private AudioClip _foundation;
        private AudioClip _attack;
        private AudioClip _hit;
        private AudioClip _invalid;
        private AudioClip _win;
        private AudioClip _lose;

        private float _lastHapticTime = -10f;

        public bool SoundEnabled { get; private set; }
        public bool HapticsEnabled { get; private set; }

        public void Initialize()
        {
            SoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) != 0;
            HapticsEnabled = PlayerPrefs.GetInt(HapticsKey, 1) != 0;

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
            _source.volume = 0.55f;

            if (FindFirstObjectByType<AudioListener>() == null)
                gameObject.AddComponent<AudioListener>();

            _move = CreateSweep("Move", 520f, 650f, 0.035f, 0.16f);
            _foundation = CreateSweep("Foundation", 650f, 980f, 0.075f, 0.22f);
            _attack = CreateSweep("Attack", 230f, 610f, 0.12f, 0.28f);
            _hit = CreateSweep("Hit", 170f, 95f, 0.10f, 0.30f);
            _invalid = CreateSweep("Invalid", 175f, 155f, 0.07f, 0.18f);
            _win = CreateSweep("Win", 440f, 1120f, 0.34f, 0.24f);
            _lose = CreateSweep("Lose", 320f, 120f, 0.32f, 0.22f);
        }

        public void PlayMove(bool foundation)
        {
            Play(foundation ? _foundation : _move);

            if (foundation)
                Vibrate(0.20f);
        }

        public void PlayAttack()
        {
            Play(_attack);
            Vibrate(0.15f);
        }

        public void PlayHit()
        {
            Play(_hit);
            Vibrate(0.12f);
        }

        public void PlayInvalid()
        {
            Play(_invalid);
        }

        public void PlayResult(bool won)
        {
            Play(won ? _win : _lose);
            Vibrate(0.45f);
        }

        public void ToggleSound()
        {
            SoundEnabled = !SoundEnabled;
            PlayerPrefs.SetInt(SoundKey, SoundEnabled ? 1 : 0);
            PlayerPrefs.Save();

            if (SoundEnabled)
                Play(_move);
        }

        public void ToggleHaptics()
        {
            HapticsEnabled = !HapticsEnabled;
            PlayerPrefs.SetInt(HapticsKey, HapticsEnabled ? 1 : 0);
            PlayerPrefs.Save();

            if (HapticsEnabled)
                Vibrate(0f);
        }

        private void Play(AudioClip clip)
        {
            if (!SoundEnabled || clip == null || _source == null)
                return;

            _source.PlayOneShot(clip);
        }

        private void Vibrate(float minimumInterval)
        {
            if (!HapticsEnabled)
                return;

            if (Time.unscaledTime - _lastHapticTime < minimumInterval)
                return;

            _lastHapticTime = Time.unscaledTime;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        private static AudioClip CreateSweep(
            string name,
            float startFrequency,
            float endFrequency,
            float duration,
            float amplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * sampleRate));
            float[] data = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)Mathf.Max(1, sampleCount - 1);
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += (2f * Mathf.PI * frequency) / sampleRate;

                float envelope = Mathf.Sin(Mathf.PI * t);
                data[i] = Mathf.Sin(phase) * amplitude * envelope;
            }

            AudioClip clip = AudioClip.Create(
                name,
                sampleCount,
                1,
                sampleRate,
                false);

            clip.SetData(data, 0);
            return clip;
        }
    }
}
