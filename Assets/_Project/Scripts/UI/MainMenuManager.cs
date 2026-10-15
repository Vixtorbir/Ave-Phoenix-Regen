using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PhoenixReborn.Systems;

namespace PhoenixReborn.UI
{
    /// <summary>
    /// Main Menu screen.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }

        [Header("Menu Buttons")]
        [SerializeField] private Button storyModeButton;
        [SerializeField] private Button arcadeModeButton;
        [SerializeField] private Button dedicationButton;
        [SerializeField] private Button quitButton;

        [Header("Dedication Modal")]
        [SerializeField] private GameObject dedicationModal;
        [SerializeField] private Button closeDedicationButton;

        [Header("Title & Visuals")]
        [SerializeField] private RectTransform titleHeaderTransform;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("High Score Readout")]
        [SerializeField] private RectTransform highScoreTransform;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Target Gameplay Scene")]
        [SerializeField] private string gameplaySceneName = "03_ArcadeMode";

        public static bool IsArcadeModeOnly { get; set; } = false;

        private Vector2 initialTitlePos;
        private Vector2 initialHSPos;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;

            if (dedicationModal != null)
            {
                dedicationModal.SetActive(false);
            }

            if (storyModeButton != null) storyModeButton.onClick.AddListener(OnClickStoryMode);
            if (arcadeModeButton != null) arcadeModeButton.onClick.AddListener(OnClickArcadeMode);
            if (dedicationButton != null) dedicationButton.onClick.AddListener(OnClickDedication);
            if (quitButton != null) quitButton.onClick.AddListener(OnClickQuit);
            if (closeDedicationButton != null) closeDedicationButton.onClick.AddListener(OnClickCloseDedication);

            if (titleHeaderTransform != null) initialTitlePos = titleHeaderTransform.anchoredPosition;
            if (highScoreTransform != null) initialHSPos = highScoreTransform.anchoredPosition;
        }

        private void Start()
        {
            UpdateHighScoreDisplay();
        }

        private void OnEnable()
        {
            UpdateHighScoreDisplay();
        }

        private void UpdateHighScoreDisplay()
        {
            // Read from the exact key used in ScoreManager.cs
            int savedHighScore = PlayerPrefs.GetInt("AvePhoenix_HighScore", PlayerPrefs.GetInt("Phoenix_HighScore", 0));
            if (highScoreText != null)
            {
                highScoreText.text = $"RECORD HISTORICO: {savedHighScore:D6}";
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;

            // 1. Gentle Title Breathing Float
            if (titleHeaderTransform != null)
            {
                float floatOffset = Mathf.Sin(time * 2.2f) * 7f;
                titleHeaderTransform.anchoredPosition = initialTitlePos + new Vector2(0f, floatOffset);
            }

            // 2. High Score Trophy Float
            if (highScoreTransform != null)
            {
                float hsFloat = Mathf.Sin(time * 1.8f) * 4f;
                highScoreTransform.anchoredPosition = initialHSPos + new Vector2(0f, hsFloat);
            }

            // 3. Fiery Phoenix Title Flame Shimmer
            if (titleText != null)
            {
                float shimmer = (Mathf.Sin(time * 3.5f) + 1f) * 0.5f;
                Color flameRed = new Color(1f, 0.25f, 0.05f, 1f);
                Color flameGold = new Color(1f, 0.55f, 0.15f, 1f);
                titleText.color = Color.Lerp(flameRed, flameGold, shimmer);
            }

            // 4. High Score Gold Glow Pulse
            if (highScoreText != null)
            {
                float hsPulse = (Mathf.Sin(time * 2.5f) + 1f) * 0.5f;
                Color goldBright = new Color(1f, 0.94f, 0.45f, 1f);
                Color goldWarm = new Color(1f, 0.72f, 0.18f, 1f);
                highScoreText.color = Color.Lerp(goldWarm, goldBright, hsPulse);
            }
        }

        public void OnClickStoryMode()
        {
            AudioManager.Instance?.PlayButtonClick();
            IsArcadeModeOnly = false;
            StartCoroutine(LoadSceneRoutineByName("03_ArcadeMode"));
        }

        public void OnClickArcadeMode()
        {
            AudioManager.Instance?.PlayButtonClick();
            IsArcadeModeOnly = true;
            StartCoroutine(LoadSceneRoutineByName("02_StoryMode"));
        }

        public void OnClickDedication()
        {
            AudioManager.Instance?.PlayButtonClick();
            StartCoroutine(LoadSceneRoutineByName("04_Dedication"));
        }

        private IEnumerator LoadSceneRoutineByName(string sceneName)
        {
            if (storyModeButton != null) storyModeButton.interactable = false;
            if (arcadeModeButton != null) arcadeModeButton.interactable = false;
            if (dedicationButton != null) dedicationButton.interactable = false;
            if (quitButton != null) quitButton.interactable = false;

            yield return new WaitForSecondsRealtime(0.2f);
            SceneManager.LoadScene(sceneName);
        }

        public void OnClickCloseDedication()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (dedicationModal != null)
            {
                dedicationModal.SetActive(false);
            }
        }

        public void OnClickQuit()
        {
            AudioManager.Instance?.PlayButtonClick();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private IEnumerator LoadSceneRoutine()
        {
            if (storyModeButton != null) storyModeButton.interactable = false;
            if (arcadeModeButton != null) arcadeModeButton.interactable = false;
            if (dedicationButton != null) dedicationButton.interactable = false;
            if (quitButton != null) quitButton.interactable = false;

            yield return new WaitForSecondsRealtime(0.2f);

            if (!string.IsNullOrEmpty(gameplaySceneName))
            {
                SceneManager.LoadScene(gameplaySceneName);
            }
        }
    }
}
