using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhoenixReborn.Gameplay;
using PhoenixReborn.Gameplay.Enemies;
using PhoenixReborn.Gameplay.Enemies.Boss;
using PhoenixReborn.Systems;

namespace PhoenixReborn.UI
{
    /// <summary>
    /// In-game HUD.
    /// Manages score, high score, lives, shield gauge, wave banners,
    /// and the pixelated horizontal "BOSS INCOMING" warning banner.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("Score & Stats UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private GameObject[] lifeIcons;

        [Header("Shield Gauge UI")]
        [SerializeField] private Image shieldFillBar;
        [SerializeField] private TextMeshProUGUI shieldStatusText;

        [Header("Wave Announcement Banner")]
        [SerializeField] private CanvasGroup waveBannerGroup;
        [SerializeField] private TextMeshProUGUI waveBannerText;
        [SerializeField] private float waveBannerDuration = 2.0f;

        [Header("Boss Warning Banner")]
        [SerializeField] private CanvasGroup bossWarningGroup;
        [SerializeField] private Image bossWarningBannerImage;
        [SerializeField] private float bossWarningDuration = 2.4f;

        [Header("Victory Screen")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private CanvasGroup victoryCanvasGroup;
        [SerializeField] private TextMeshProUGUI victoryScoreText;
        [SerializeField] private TextMeshProUGUI victoryHighScoreText;
        [SerializeField] private GameObject victoryNewRecordBadge;
        [SerializeField] private Button victoryRetryButton;
        [SerializeField] private Button victoryMenuButton;

        [Header("Game Over Screen")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private CanvasGroup gameOverCanvasGroup;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
        [SerializeField] private GameObject gameOverNewRecordBadge;
        [SerializeField] private Button gameOverRetryButton;
        [SerializeField] private Button gameOverMenuButton;

        private PlayerHealth _playerHealth;
        private PlayerShield _playerShield;
        private Coroutine _waveBannerCoroutine;
        private Coroutine _bossWarningCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Time.timeScale = 1f;

            if (waveBannerGroup != null) waveBannerGroup.alpha = 0f;
            if (bossWarningGroup != null) bossWarningGroup.alpha = 0f;
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (victoryRetryButton != null) victoryRetryButton.onClick.AddListener(OnClickRetry);
            if (victoryMenuButton != null) victoryMenuButton.onClick.AddListener(OnClickMainMenu);
            if (gameOverRetryButton != null) gameOverRetryButton.onClick.AddListener(OnClickRetry);
            if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnClickMainMenu);
        }

        private void Start()
        {
            FindPlayerReferences();

            if (ScoreManager.Instance != null)
            {
                UpdateScore(ScoreManager.Instance.CurrentScore);
                UpdateHighScore(ScoreManager.Instance.HighScore);
            }
        }

        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += UpdateScore;
            ScoreManager.OnHighScoreChanged += UpdateHighScore;

            BossController.OnBossIncoming += TriggerBossWarning;
            BossController.OnBossDefeated += HandleBossDefeated;

            WaveManager waveMgr = FindFirstObjectByType<WaveManager>();
            if (waveMgr != null)
            {
                waveMgr.OnWaveStarted += TriggerWaveAnnouncement;
            }
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= UpdateScore;
            ScoreManager.OnHighScoreChanged -= UpdateHighScore;

            BossController.OnBossIncoming -= TriggerBossWarning;
            BossController.OnBossDefeated -= HandleBossDefeated;

            WaveManager waveMgr = FindFirstObjectByType<WaveManager>();
            if (waveMgr != null)
            {
                waveMgr.OnWaveStarted -= TriggerWaveAnnouncement;
            }

            if (_playerHealth != null)
            {
                _playerHealth.OnLivesChanged -= UpdateLives;
            }
        }

        private void Update()
        {
            UpdateShieldGauge();
        }

        private void FindPlayerReferences()
        {
            if (_playerHealth == null)
            {
                _playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (_playerHealth != null)
                {
                    _playerHealth.OnLivesChanged += UpdateLives;
                    UpdateLives(_playerHealth.CurrentLives);
                }
            }

            if (_playerShield == null)
            {
                _playerShield = FindFirstObjectByType<PlayerShield>();
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString("D6");
            }
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = highScore.ToString("D6");
            }
        }

        public void UpdateLives(int currentLives)
        {
            if (livesText != null)
            {
                livesText.text = $"x{Mathf.Max(0, currentLives)}";
            }

            if (lifeIcons != null && lifeIcons.Length > 0)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                {
                    if (lifeIcons[i] != null)
                    {
                        lifeIcons[i].SetActive(i < currentLives);
                    }
                }
            }

            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        private void UpdateShieldGauge()
        {
            if (_playerShield == null) return;

            float progress = _playerShield.CooldownProgress;

            if (shieldFillBar != null)
            {
                if (_playerShield.IsActive)
                {
                    // Flash / pulse rapidly while active
                    float pulse = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.time * 10f));
                    shieldFillBar.fillAmount = 1f;
                    shieldFillBar.color = new Color(0.2f, 0.95f, 1f, pulse);
                }
                else if (progress >= 1f)
                {
                    // Ready: bright solid emerald-cyan with soft idle glow
                    float softGlow = 0.85f + 0.15f * Mathf.Sin(Time.time * 3f);
                    shieldFillBar.fillAmount = 1f;
                    shieldFillBar.color = new Color(0.2f, 1f, 0.7f, softGlow);
                }
                else
                {
                    // Recharging: circular clockwise radial fill
                    shieldFillBar.fillAmount = progress;
                    shieldFillBar.color = new Color(0.2f, 0.8f, 1f, 0.85f);
                }
            }

            if (shieldStatusText != null)
            {
                if (_playerShield.IsActive)
                {
                    shieldStatusText.text = "ACTIVE";
                    shieldStatusText.color = new Color(0.2f, 0.95f, 1f, 1f);
                }
                else if (progress >= 1f)
                {
                    shieldStatusText.text = "READY";
                    shieldStatusText.color = new Color(0.25f, 1f, 0.5f, 1f);
                }
                else
                {
                    shieldStatusText.text = $"{(progress * 100f):F0}%";
                    shieldStatusText.color = new Color(1f, 0.7f, 0.2f, 1f);
                }
            }
        }

        public void TriggerWaveAnnouncement(string waveTitle)
        {
            if (waveBannerGroup == null) return;

            Systems.AudioManager.Instance?.PlayWaveStart();

            if (_waveBannerCoroutine != null) StopCoroutine(_waveBannerCoroutine);
            _waveBannerCoroutine = StartCoroutine(AnimateWaveBannerRoutine(waveTitle));
        }

        public void ShowWaveBanner(string waveTitle) => TriggerWaveAnnouncement(waveTitle);

        private IEnumerator AnimateWaveBannerRoutine(string waveTitle)
        {
            if (waveBannerText != null)
            {
                waveBannerText.text = waveTitle.ToUpper();
            }

            Transform bannerTransform = waveBannerGroup.transform;
            Vector3 baseScale = Vector3.one;

            float fadeInTime = 0.35f;
            float elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInTime;
                waveBannerGroup.alpha = t;
                bannerTransform.localScale = Vector3.Lerp(baseScale * 1.3f, baseScale, t);
                yield return null;
            }

            waveBannerGroup.alpha = 1f;
            bannerTransform.localScale = baseScale;

            yield return new WaitForSeconds(waveBannerDuration);

            float fadeOutTime = 0.4f;
            elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                waveBannerGroup.alpha = 1f - (elapsed / fadeOutTime);
                yield return null;
            }

            waveBannerGroup.alpha = 0f;
        }

        public void TriggerBossWarning()
        {
            if (bossWarningGroup == null) return;

            if (_bossWarningCoroutine != null) StopCoroutine(_bossWarningCoroutine);
            _bossWarningCoroutine = StartCoroutine(AnimateBossWarningRoutine());
        }

        private IEnumerator AnimateBossWarningRoutine()
        {
            Transform bannerTransform = bossWarningGroup.transform;
            Vector3 basePos = bannerTransform.localPosition;

            float timer = 0f;
            int flashCount = 0;

            while (timer < bossWarningDuration)
            {
                timer += Time.deltaTime;
                flashCount++;

                bool isVisible = (int)(timer * 8f) % 2 == 0;
                bossWarningGroup.alpha = isVisible ? 1f : 0.25f;

                float shakeX = Random.Range(-4f, 4f);
                bannerTransform.localPosition = new Vector3(basePos.x + shakeX, basePos.y, basePos.z);

                yield return null;
            }

            bannerTransform.localPosition = basePos;

            float fadeTime = 0.3f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeTime)
            {
                fadeElapsed += Time.deltaTime;
                bossWarningGroup.alpha = 1f - (fadeElapsed / fadeTime);
                yield return null;
            }

            bossWarningGroup.alpha = 0f;
        }

        private void HandleBossDefeated()
        {
            if (Gameplay.Arcade.ArcadeModeManager.Instance != null)
            {
                // In Arcade mode, boss defeat is managed by ArcadeModeManager for infinite waves
                return;
            }

            StartCoroutine(ShowVictoryRoutine());
        }

        private IEnumerator ShowVictoryRoutine()
        {
            yield return new WaitForSeconds(1.2f);
            if (Systems.DialogueManager.Instance != null)
            {
                Systems.DialogueManager.Instance.PlayVictoryStory(() =>
                {
                    StartCoroutine(DisplayVictoryScreen());
                });
            }
            else
            {
                StartCoroutine(DisplayVictoryScreen());
            }
        }

        private IEnumerator DisplayVictoryScreen()
        {
            if (victoryPanel == null) yield break;

            Time.timeScale = 0f;
            Systems.CameraShake.Instance?.StopShake();

            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;

            if (victoryScoreText != null) victoryScoreText.text = finalScore.ToString("D6");
            if (victoryHighScoreText != null) victoryHighScoreText.text = highScore.ToString("D6");
            if (victoryNewRecordBadge != null) victoryNewRecordBadge.SetActive(finalScore >= highScore && finalScore > 0);

            victoryPanel.SetActive(true);
            Systems.AudioManager.Instance?.PlayVictory();

            if (victoryCanvasGroup != null)
            {
                victoryCanvasGroup.alpha = 0f;
                Transform panelT = victoryCanvasGroup.transform;
                panelT.localScale = Vector3.one * 0.85f;

                float elapsed = 0f;
                float duration = 0.45f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    victoryCanvasGroup.alpha = t;
                    panelT.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }

                victoryCanvasGroup.alpha = 1f;
                panelT.localScale = Vector3.one;
            }
        }

        private void TriggerGameOver()
        {
            StartCoroutine(ShowGameOverRoutine());
        }

        private IEnumerator ShowGameOverRoutine()
        {
            yield return new WaitForSecondsRealtime(0.35f);
            if (gameOverPanel == null) yield break;

            Time.timeScale = 0f;
            Systems.CameraShake.Instance?.StopShake();

            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;

            if (gameOverScoreText != null) gameOverScoreText.text = finalScore.ToString("D6");
            if (gameOverHighScoreText != null) gameOverHighScoreText.text = highScore.ToString("D6");
            if (gameOverNewRecordBadge != null) gameOverNewRecordBadge.SetActive(finalScore >= highScore && finalScore > 0);

            gameOverPanel.SetActive(true);
            Systems.AudioManager.Instance?.PlayGameOver();

            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                Transform panelT = gameOverCanvasGroup.transform;
                panelT.localScale = Vector3.one * 0.85f;

                float elapsed = 0f;
                float duration = 0.3f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    gameOverCanvasGroup.alpha = t;
                    panelT.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }

                gameOverCanvasGroup.alpha = 1f;
                panelT.localScale = Vector3.one;
            }
        }

        public void OnClickRetry()
        {
            Time.timeScale = 1f;
            Systems.AudioManager.Instance?.PlayButtonClick();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void OnClickMainMenu()
        {
            Time.timeScale = 1f;
            Systems.AudioManager.Instance?.PlayButtonClick();
            UnityEngine.SceneManagement.SceneManager.LoadScene("01_MainMenu");
        }

        public void RestartGame() => OnClickRetry();
    }
}
