using UnityEngine;
using UnityEngine.UI;
using Axis = UnityEngine.RectTransform.Axis;

namespace UltraFace
{

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] Texture2D _image;
    [SerializeField] ResourceSet _resources;
    [SerializeField] RectTransform _markerParent;
    [SerializeField] RectTransform _markerPrefab;

    void Start()
    {
        using var detector = new FaceDetector(_resources);

        detector.ProcessImage(_image, _scoreThreshold, _overlapThreshold);

        var w = _markerParent.rect.width;
        var h = _markerParent.rect.height;

        foreach (var box in detector.DetectedFaces)
        {
            var x1 = box.x1 * w;
            var y1 = box.y1 * h;
            var x2 = box.x2 * w;
            var y2 = box.y2 * h;

            var marker = Instantiate(_markerPrefab, _markerParent);
            marker.anchoredPosition = new Vector2(x1, h - y2);
            marker.SetSizeWithCurrentAnchors(Axis.Horizontal, x2 - x1);
            marker.SetSizeWithCurrentAnchors(Axis.Vertical, y2 - y1);
        }
    }
}

} // namespace UltraFace
