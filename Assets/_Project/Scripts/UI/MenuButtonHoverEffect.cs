using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PhoenixReborn.Systems;

namespace PhoenixReborn.UI
{
    /// <summary>
    /// Button hover, press and idle breathing animation.
    /// Works with both Unity EventSystem pointer events and standard Button states.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.05f;
        [SerializeField] private float pressScaleMultiplier = 0.95f;
        [SerializeField] private float animationSpeed = 14f;

        [Header("Breathing Settings")]
        [SerializeField] private bool enableIdleBreathing = true;
        [SerializeField] private float breathingSpeed = 2.5f;
        [SerializeField] private float breathingAmount = 0.018f;
        [SerializeField] private float phaseOffset = 0f;

        private Vector3 baseScale = Vector3.one;
        private Image buttonImage;
        private TextMeshProUGUI titleText;
        private Color baseImageColor = Color.white;
        private Color baseTitleColor = Color.white;

        private bool isHovered = false;
        private bool isPressed = false;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            if (buttonImage != null) baseImageColor = buttonImage.color;

            Transform titleT = transform.Find("Title");
            if (titleT != null) titleText = titleT.GetComponent<TextMeshProUGUI>();
            if (titleText != null) baseTitleColor = titleText.color;

            baseScale = transform.localScale;
            if (baseScale.x <= 0.01f) baseScale = Vector3.one;
        }

        private void Start()
        {
            if (baseScale.x <= 0.01f) baseScale = Vector3.one;
        }

        public void ConfigureBreathing(bool enabled, float speed, float amount, float phase)
        {
            enableIdleBreathing = enabled;
            breathingSpeed = speed;
            breathingAmount = amount;
            phaseOffset = phase;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Vector3 targetScale = baseScale;
            Color targetImgCol = baseImageColor;
            Color targetTxtCol = baseTitleColor;

            if (isPressed)
            {
                targetScale = baseScale * pressScaleMultiplier;
                targetImgCol = new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else if (isHovered)
            {
                targetScale = baseScale * hoverScaleMultiplier;
                targetImgCol = new Color(1.15f, 1.15f, 1.15f, 1f);
                targetTxtCol = Color.white;
            }
            else if (enableIdleBreathing)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * breathingSpeed + phaseOffset) * breathingAmount;
                targetScale = baseScale * pulse;
            }

            // Smooth spring interpolation
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, dt * animationSpeed);

            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(buttonImage.color, targetImgCol, dt * animationSpeed);
            }

            if (titleText != null)
            {
                titleText.color = Color.Lerp(titleText.color, targetTxtCol, dt * animationSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            isHovered = true;
            AudioManager.Instance?.PlayTextBlip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            isPressed = false;
        }
    }
}
