using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Global Audio Manager handling Background Music (BGM) and Sound Effects (SFX).
    /// Standard Singleton with serialized audio clip references.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)][SerializeField] private float musicVolume = 0.60f;
        [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.85f;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip playerShootClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip enemyShootClip;
        [SerializeField] private AudioClip smallExplosionClip;
        [SerializeField] private AudioClip mediumExplosionClip;
        [SerializeField] private AudioClip bossExplosionClip;
        [SerializeField] private AudioClip bossWarningClip;
        [SerializeField] private AudioClip bossHitClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip shieldActivateClip;
        [SerializeField] private AudioClip barrierHitClip;
        [SerializeField] private AudioClip radioOpenClip;
        [SerializeField] private AudioClip textBlipClip;
        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip bgmClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            AudioListener.pause = false;

            // AudioSource setup
            AudioSource[] existingSources = GetComponents<AudioSource>();

            if (musicSource == null)
            {
                if (existingSources != null && existingSources.Length > 0)
                {
                    musicSource = existingSources[0];
                }
                else
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (sfxSource == null)
            {
                if (existingSources != null && existingSources.Length > 1)
                {
                    sfxSource = existingSources[1];
                }
                else
                {
                    sfxSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (musicSource != null)
            {
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }
        }

        private void Start()
        {
            if (bgmClip != null)
            {
                PlayMusic(bgmClip, true);
            }
        }

        public void PlayPlayerShoot() => PlaySFX(playerShootClip, 0.75f);
        public void PlayPlayerHit() => PlaySFX(playerHitClip, 1.0f);
        public void PlayEnemyShoot() => PlaySFX(enemyShootClip, 0.65f);
        public void PlaySmallExplosion() => PlaySFX(smallExplosionClip, 0.85f);
        public void PlayMediumExplosion() => PlaySFX(mediumExplosionClip, 0.95f);
        public void PlayBossExplosion() => PlaySFX(bossExplosionClip, 1.0f);
        public void PlayBossWarning() => PlaySFX(bossWarningClip, 1.0f);
        public void PlayBossHit() => PlaySFX(bossHitClip, 0.75f);
        public void PlayWaveStart() => PlaySFX(waveStartClip, 0.85f);
        public void PlayShieldActivate() => PlaySFX(shieldActivateClip, 0.9f);
        public void PlayBarrierHit() => PlaySFX(barrierHitClip, 0.65f);
        public void PlayRadioOpen() => PlaySFX(radioOpenClip, 0.85f);
        public void PlayTextBlip() => PlaySFX(textBlipClip, 0.35f);
        public void PlayVictory() => PlaySFX(victoryClip, 1.0f);
        public void PlayGameOver() => PlaySFX(gameOverClip, 1.0f);
        public void PlayButtonClick() => PlaySFX(buttonClickClip, 0.9f);

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
            }
            else if (musicSource != null)
            {
                musicSource.PlayOneShot(clip, volumeScale * sfxVolume);
            }
        }
    }
}
