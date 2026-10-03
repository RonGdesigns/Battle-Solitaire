using UnityEngine;

namespace BattleSolitaire.Presentation
{
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            Vector2Int size = new Vector2Int(Screen.width, Screen.height);

            if (Screen.safeArea != _lastSafeArea || size != _lastScreenSize)
                Apply();
        }

        private void Apply()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
                return;

            Rect safe = Screen.safeArea;

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            _rectTransform.anchorMin = min;
            _rectTransform.anchorMax = max;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _lastSafeArea = safe;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
