using System;
using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Singleton/System managing Score, High Score persistence via PlayerPrefs,
    /// and C# action events for UI score displays.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string HIGH_SCORE_KEY = "AvePhoenix_HighScore";

        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }

        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnHighScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadHighScore();
        }

        public void AddScore(int points)
        {
            if (points <= 0) return;

            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);

            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                SaveHighScore();
                OnHighScoreChanged?.Invoke(HighScore);
            }
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            OnHighScoreChanged?.Invoke(HighScore);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            PlayerPrefs.Save();
        }
    }
}
