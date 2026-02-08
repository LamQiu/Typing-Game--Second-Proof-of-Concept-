using System.Collections;
using UnityEngine;

namespace Audio
{
    public class ThemeMusicManager : Singleton<ThemeMusicManager>
    {

        [Header("Audio Source")]
        [SerializeField] private AudioSource MusicSource;

        [Header("Theme Clips")]
        [SerializeField] private AudioClip MainMenuTheme;
        [SerializeField] private AudioClip TypingTheme;
        [SerializeField] private AudioClip ScoringTheme;

        [Header("Fade Settings")]
        [SerializeField] private float FadeDuration = 0.5f;

        private Coroutine m_fadeCoroutine;

        protected override void Awake()
        {
            base.Awake();

            if (!MusicSource)
            {
                MusicSource = gameObject.AddComponent<AudioSource>();
                MusicSource.loop = true;
            }
        }

        // ================================
        // Public API
        // ================================

        public void PlayMainMenuTheme()
        {
            PlayTheme(MainMenuTheme, true);
        }

        public void PlayTypingTheme()
        {
            PlayTheme(TypingTheme, true);
        }

        public void PlayScoringTheme()
        {
            PlayTheme(ScoringTheme, true);
        }

        private void PlayTheme(AudioClip clip, bool loop)
        {
            if (!clip) return;

            if (MusicSource.clip == clip && MusicSource.isPlaying)
                return;

            if (m_fadeCoroutine != null)
                StopCoroutine(m_fadeCoroutine);

            m_fadeCoroutine = StartCoroutine(FadeAndSwitch(clip, loop));
        }

        private IEnumerator FadeAndSwitch(AudioClip newClip, bool loop)
        {
            float startVolume = MusicSource.volume;

            // Fade Out
            for (float t = 0; t < FadeDuration; t += Time.deltaTime)
            {
                MusicSource.volume = Mathf.Lerp(startVolume, 0f, t / FadeDuration);
                yield return null;
            }

            MusicSource.Stop();
            MusicSource.clip = newClip;
            MusicSource.loop = loop;
            MusicSource.Play();

            // Fade In
            for (float t = 0; t < FadeDuration; t += Time.deltaTime)
            {
                MusicSource.volume = Mathf.Lerp(0f, startVolume, t / FadeDuration);
                yield return null;
            }

            MusicSource.volume = startVolume;
        }
    }
}
