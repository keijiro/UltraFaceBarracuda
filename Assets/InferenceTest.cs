using UnityEngine;
using UnityEngine.UI;

namespace UltraFace
{

public sealed class InferenceTest : MonoBehaviour
{
    [SerializeField] Texture2D _image;
    [SerializeField] ResourceSet _resources;
    [SerializeField] RectTransform _markerParent;
    [SerializeField] RectTransform _markerPrefab;

    void Start()
    {
        using var detector = new FaceDetector(_resources);

        detector.ProcessImage(_image, 0.7f, 0.5f);

        var uiw = _markerParent.rect.width;
        var uih = _markerParent.rect.height;

        foreach (var box in detector.DetectedFaces)
        {
            var x1 = box.x1 * uiw;
            var y1 = box.y1 * uih;
            var x2 = box.x2 * uiw;
            var y2 = box.y2 * uih;

            var marker = Instantiate(_markerPrefab, _markerParent);
            marker.anchoredPosition = new Vector2(x1, uih - y2);
            marker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x2 - x1);
            marker.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y2 - y1);
        }
    }
}

} // namespace UltraFace
