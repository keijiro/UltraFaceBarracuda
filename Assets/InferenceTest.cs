using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;

namespace UltraFace
{

public sealed class InferenceTest : MonoBehaviour
{
    [SerializeField] WorkerFactory.Type _workerType;
    [SerializeField] NNModel _model;
    [SerializeField] Texture2D _image;
    [SerializeField] RectTransform _markerParent;
    [SerializeField] RectTransform _markerPrefab;

    void Start()
    {
        // Input image -> Tensor (1, 240, 320, 3)
        var source = new float[240 * 320 * 3];

        for (var y = 0; y < 240; y++)
        {
            for (var x = 0; x < 320; x++)
            {
                var i = ((239 - y) * 320 + x) * 3;
                var p = _image.GetPixel(x, y);
                source[i + 0] = p.r * 2 - 1;
                source[i + 1] = p.g * 2 - 1;
                source[i + 2] = p.b * 2 - 1;
            }
        }

        // Inference
        var model = ModelLoader.Load(_model);
        using var worker = WorkerFactory.CreateWorker(_workerType, model);

        using (var tensor = new Tensor(1, 240, 320, 3, source))
            worker.Execute(tensor);

        // Results
        var scores = worker.PeekOutput("scores");
        var boxes = worker.PeekOutput("boxes");

        var uiw = _markerParent.rect.width;
        var uih = _markerParent.rect.height;

        for (var i = 0; i < 4420; i++)
        {
            var score = scores[0, 0, i, 1];
            if (score < 0.7f) continue;

            var x1 = boxes[0, 0, i, 0] * uiw;
            var y1 = boxes[0, 0, i, 1] * uih;
            var x2 = boxes[0, 0, i, 2] * uiw;
            var y2 = boxes[0, 0, i, 3] * uih;

            var marker = Instantiate(_markerPrefab, _markerParent);
            marker.anchoredPosition = new Vector2(x1, uih - y2);
            marker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x2 - x1);
            marker.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y2 - y1);
        }
    }
}

} // namespace UltraFace
