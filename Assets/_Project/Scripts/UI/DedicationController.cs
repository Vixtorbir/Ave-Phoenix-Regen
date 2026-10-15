using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PhoenixReborn.Systems;

namespace PhoenixReborn.UI
{
    /// <summary>
    /// Controller for the Dedication scene.
    /// Shows the dedication and goes back to the Main Menu.
    /// </summary>
    public class DedicationController : MonoBehaviour
    {
        [Header("UI Controls")]
        [SerializeField] private Button backButton;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnClickBackToMenu);
            }
        }

        public void OnClickBackToMenu()
        {
            AudioManager.Instance?.PlayButtonClick();
            StartCoroutine(LoadMenuRoutine());
        }

        private IEnumerator LoadMenuRoutine()
        {
            if (backButton != null) backButton.interactable = false;
            yield return new WaitForSecondsRealtime(0.2f);
            SceneManager.LoadScene("01_MainMenu");
        }
    }
}
