using System.Collections.Generic;
using UnityEngine;

namespace PhoenixReborn.UI
{
    [RequireComponent(typeof(Canvas))]
    public class CutoutAvoider : MonoBehaviour
    {
        private const float OverlapTolerance = 20f;

        private readonly List<RectTransform> _topGroup = new List<RectTransform>();
        private readonly List<RectTransform> _bottomGroup = new List<RectTransform>();
        private readonly Dictionary<RectTransform, Vector2> _basePositions = new Dictionary<RectTransform, Vector2>();

        private Canvas _canvas;
        private Vector2Int _appliedScreenSize;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();

            foreach (Transform child in transform)
            {
                RectTransform rect = child as RectTransform;
                if (rect == null || rect.anchorMin != rect.anchorMax) continue;

                if (rect.anchorMin.y >= 1f) _topGroup.Add(rect);
                else if (rect.anchorMin.y <= 0f) _bottomGroup.Add(rect);
                else continue;

                _basePositions[rect] = rect.anchoredPosition;
            }
        }

        private void Start()
        {
            Apply(Screen.cutouts);
        }

        private void Update()
        {
            if (_appliedScreenSize.x != Screen.width || _appliedScreenSize.y != Screen.height)
            {
                Apply(Screen.cutouts);
            }
        }

        private void Apply(IReadOnlyList<Rect> cutouts)
        {
            _appliedScreenSize = new Vector2Int(Screen.width, Screen.height);

            foreach (KeyValuePair<RectTransform, Vector2> entry in _basePositions)
            {
                entry.Key.anchoredPosition = entry.Value;
            }

            float topPush = 0f;
            float bottomPush = 0f;

            foreach (Rect cutout in cutouts)
            {
                foreach (RectTransform rect in _topGroup)
                {
                    if (TryGetOverlap(rect, cutout, out Rect screenRect))
                    {
                        topPush = Mathf.Max(topPush, screenRect.yMax - cutout.yMin);
                    }
                }

                foreach (RectTransform rect in _bottomGroup)
                {
                    if (TryGetOverlap(rect, cutout, out Rect screenRect))
                    {
                        bottomPush = Mathf.Max(bottomPush, cutout.yMax - screenRect.yMin);
                    }
                }
            }

            Shift(_topGroup, -topPush / _canvas.scaleFactor);
            Shift(_bottomGroup, bottomPush / _canvas.scaleFactor);
        }

        private bool TryGetOverlap(RectTransform rect, Rect cutout, out Rect screenRect)
        {
            screenRect = default;
            if (!rect.gameObject.activeSelf) return false;

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, corners[2]);
            screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            float overlapX = Mathf.Min(screenRect.xMax, cutout.xMax) - Mathf.Max(screenRect.xMin, cutout.xMin);
            float overlapY = Mathf.Min(screenRect.yMax, cutout.yMax) - Mathf.Max(screenRect.yMin, cutout.yMin);
            return overlapX > OverlapTolerance && overlapY > OverlapTolerance;
        }

        private static void Shift(List<RectTransform> group, float offsetY)
        {
            foreach (RectTransform rect in group)
            {
                rect.anchoredPosition += new Vector2(0f, offsetY);
            }
        }
    }
}
