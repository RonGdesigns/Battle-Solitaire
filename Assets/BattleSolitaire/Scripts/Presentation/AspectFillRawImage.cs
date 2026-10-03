using UnityEngine;
using UnityEngine.UI;

namespace BattleSolitaire.Presentation
{
    /// <summary>Fills a UI rectangle with a centered, undistorted texture crop.</summary>
    [ExecuteAlways, RequireComponent(typeof(RawImage)), DisallowMultipleComponent]
    public sealed class AspectFillRawImage : MonoBehaviour
    {
        private RawImage _image;
        private Texture _lastTexture;
        private Vector2 _lastSize;
        private Vector2Int _lastTextureSize;

        private void OnEnable() => Refresh();
        private void OnRectTransformDimensionsChange() => Refresh();

        private void LateUpdate()
        {
            if (_image == null) _image = GetComponent<RawImage>();
            Texture texture = _image.texture;
            Vector2Int textureSize = texture == null
                ? Vector2Int.zero : new Vector2Int(texture.width, texture.height);
            if (texture != _lastTexture || textureSize != _lastTextureSize ||
                _image.rectTransform.rect.size != _lastSize)
                Refresh();
        }

        public void SetTexture(Texture texture)
        {
            if (_image == null) _image = GetComponent<RawImage>();
            _image.texture = texture;
            _image.enabled = texture != null;
            Refresh();
        }

        public void Refresh()
        {
            if (_image == null) _image = GetComponent<RawImage>();
            if (_image == null) return;
            _lastTexture = _image.texture;
            _lastSize = _image.rectTransform.rect.size;
            _lastTextureSize = _lastTexture == null
                ? Vector2Int.zero : new Vector2Int(_lastTexture.width, _lastTexture.height);
            _image.uvRect = CalculateUvRect(_lastTextureSize, _lastSize);
        }

        public static Rect CalculateUvRect(Vector2 source, Vector2 destination)
        {
            if (source.x <= 0f || source.y <= 0f || destination.x <= 0f || destination.y <= 0f)
                return new Rect(0f, 0f, 1f, 1f);

            float sourceAspect = source.x / source.y;
            float destinationAspect = destination.x / destination.y;
            if (sourceAspect > destinationAspect)
            {
                float width = destinationAspect / sourceAspect;
                return new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            float height = sourceAspect / destinationAspect;
            return new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
    }
}
