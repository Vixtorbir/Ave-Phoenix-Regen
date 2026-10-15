using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Manages story transmissions and character dialogues.
    /// Freezes gameplay safely, renders typewriter text with audio blips,
    /// and advances on player screen taps.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private CanvasGroup dialoguePanel;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueMessageText;
        [SerializeField] private GameObject continuePromptObject;

        [Header("Default Character Data")]
        [SerializeField] private Sprite alexPortrait;
        [SerializeField] private Sprite victorPortrait;
        [SerializeField] private Sprite mamaPortrait;

        [Header("Settings")]
        [SerializeField] private bool pauseGameDuringDialogue = true;
        [SerializeField] private float defaultTypeSpeed = 0.025f;

        private readonly Queue<DialogueLine> _dialogueQueue = new Queue<DialogueLine>();
        private Coroutine _typewriterCoroutine;
        private Action _onDialogueFinished;
        private bool _isTyping;
        private string _currentFullMessage = "";
        private bool _isDialogueActive;

        public bool IsDialogueActive => _isDialogueActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (dialoguePanel != null)
            {
                dialoguePanel.alpha = 0f;
                dialoguePanel.blocksRaycasts = false;
                dialoguePanel.interactable = false;
            }

            if (continuePromptObject != null)
            {
                continuePromptObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!_isDialogueActive) return;

            bool advancePressed = false;

            // Touch / Mouse Pointer press
            if (UnityEngine.InputSystem.Pointer.current != null && UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
            {
                advancePressed = true;
            }
            // Keyboard Space / Enter
            else if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame))
            {
                advancePressed = true;
            }
            // Gamepad South / Start
            else if (UnityEngine.InputSystem.Gamepad.current != null && (UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame || UnityEngine.InputSystem.Gamepad.current.startButton.wasPressedThisFrame))
            {
                advancePressed = true;
            }

            if (advancePressed)
            {
                OnUserAdvanceInput();
            }
        }

        public void PlayWave1Story(Action onComplete = null)
        {
            DialogueLine[] lines = new DialogueLine[]
            {
                new DialogueLine
                {
                    speakerName = "ALEX",
                    speakerColor = new Color(0.2f, 0.95f, 1f, 1f),
                    portraitSprite = alexPortrait,
                    messageText = "¡Papá! ¡Han secuestrado a mamá en la nave nodriza alienígena!"
                },
                new DialogueLine
                {
                    speakerName = "ALEX",
                    speakerColor = new Color(0.2f, 0.95f, 1f, 1f),
                    portraitSprite = alexPortrait,
                    messageText = "¡Toma los mandos del caza Phoenix! ¡Tienes que rescatarla!"
                },
                new DialogueLine
                {
                    speakerName = "ALEX",
                    speakerColor = new Color(0.2f, 0.95f, 1f, 1f),
                    portraitSprite = alexPortrait,
                    messageText = "Desliza el dedo para moverte. El láser dispara solo y el orbe de la derecha es tu escudo. ¡A por ellos!"
                }
            };
            StartDialogue(lines, onComplete);
        }

        public void PlayWave2Story(Action onComplete = null)
        {
            DialogueLine[] lines = new DialogueLine[]
            {
                new DialogueLine
                {
                    speakerName = "VÍCTOR",
                    speakerColor = new Color(0.25f, 1f, 0.55f, 1f),
                    portraitSprite = victorPortrait,
                    messageText = "¡Bien hecho, papá! Pero cuidado: los radares detectan una segunda oleada más rápida y peligrosa."
                },
                new DialogueLine
                {
                    speakerName = "VÍCTOR",
                    speakerColor = new Color(0.25f, 1f, 0.55f, 1f),
                    portraitSprite = victorPortrait,
                    messageText = "¡Esquiva sus ráfagas de plasma! ¡Estamos muy cerca de mamá!"
                }
            };
            StartDialogue(lines, onComplete);
        }

        public void PlayVictoryStory(Action onComplete = null)
        {
            DialogueLine[] lines = new DialogueLine[]
            {
                new DialogueLine
                {
                    speakerName = "MAMÁ",
                    speakerColor = new Color(1f, 0.85f, 0.25f, 1f),
                    portraitSprite = mamaPortrait,
                    messageText = "¡Nene, lo lograste! ¡Sabía que vendrías a rescatarme!"
                },
                new DialogueLine
                {
                    speakerName = "MAMÁ",
                    speakerColor = new Color(1f, 0.85f, 0.25f, 1f),
                    portraitSprite = mamaPortrait,
                    messageText = "¡Eres nuestro auténtico héroe! ¡Buen vuelo de vuelta a casa, capitán!"
                }
            };
            StartDialogue(lines, onComplete);
        }

        public void StartDialogue(DialogueLine[] lines, Action onComplete = null)
        {
            if (lines == null || lines.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _dialogueQueue.Clear();
            foreach (var line in lines)
            {
                _dialogueQueue.Enqueue(line);
            }

            _onDialogueFinished = onComplete;
            _isDialogueActive = true;

            if (pauseGameDuringDialogue)
            {
                Time.timeScale = 0f;
            }

            AudioManager.Instance?.PlayRadioOpen();
            StartCoroutine(FadePanel(1f, 0.25f));
            DisplayNextLine();
        }

        private void OnUserAdvanceInput()
        {
            if (_isTyping)
            {
                // Instantly complete current line
                if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                dialogueMessageText.text = _currentFullMessage;
                _isTyping = false;
                if (continuePromptObject != null) continuePromptObject.SetActive(true);
            }
            else
            {
                // Advance to next line
                DisplayNextLine();
            }
        }

        private void DisplayNextLine()
        {
            if (_dialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            DialogueLine currentLine = _dialogueQueue.Dequeue();

            if (speakerNameText != null)
            {
                speakerNameText.text = currentLine.speakerName;
                speakerNameText.color = currentLine.speakerColor;
            }

            if (portraitImage != null && currentLine.portraitSprite != null)
            {
                portraitImage.sprite = currentLine.portraitSprite;
                portraitImage.gameObject.SetActive(true);
            }

            if (continuePromptObject != null) continuePromptObject.SetActive(false);

            _currentFullMessage = currentLine.messageText;
            float speed = currentLine.customTypeSpeed > 0 ? currentLine.customTypeSpeed : defaultTypeSpeed;

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(_currentFullMessage, speed));
        }

        private IEnumerator TypewriterRoutine(string message, float charDelay)
        {
            _isTyping = true;
            dialogueMessageText.text = "";

            int blipCounter = 0;
            for (int i = 0; i < message.Length; i++)
            {
                dialogueMessageText.text += message[i];

                if (!char.IsWhiteSpace(message[i]) && blipCounter++ % 2 == 0)
                {
                    AudioManager.Instance?.PlayTextBlip();
                }

                yield return new WaitForSecondsRealtime(charDelay);
            }

            _isTyping = false;
            if (continuePromptObject != null) continuePromptObject.SetActive(true);
        }

        private void EndDialogue()
        {
            _isDialogueActive = false;
            StartCoroutine(FadePanel(0f, 0.2f, () =>
            {
                if (pauseGameDuringDialogue)
                {
                    Time.timeScale = 1f;
                }
                _onDialogueFinished?.Invoke();
            }));
        }

        private IEnumerator FadePanel(float targetAlpha, float duration, Action onFadeDone = null)
        {
            if (dialoguePanel == null)
            {
                onFadeDone?.Invoke();
                yield break;
            }

            float startAlpha = dialoguePanel.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                dialoguePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            dialoguePanel.alpha = targetAlpha;
            dialoguePanel.blocksRaycasts = targetAlpha > 0.5f;
            dialoguePanel.interactable = targetAlpha > 0.5f;
            onFadeDone?.Invoke();
        }
    }
}
