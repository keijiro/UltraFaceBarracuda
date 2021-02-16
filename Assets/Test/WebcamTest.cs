using UnityEngine;
using UI = UnityEngine.UI;

namespace UltraFace {

sealed class WebcamTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _visualizer = null;
    [SerializeField] Texture2D _texture = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Internal objects

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;

    FaceDetector _detector;

    Material _material;
    ComputeBuffer _drawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Texture allocation
        _webcamRaw = new WebCamTexture();
        _webcamBuffer = new RenderTexture(1920, 1080, 0);

        _webcamRaw.Play();
        _previewUI.texture = _webcamBuffer;

        // Face detector initialization
        _detector = new FaceDetector(_resources);

        // Visualizer initialization
        _material = new Material(_visualizer);
        _drawArgs = new ComputeBuffer
          (4, sizeof(uint), ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});
    }

    void OnDisable()
    {
        _detector?.Dispose();
        _detector = null;

        _drawArgs?.Dispose();
        _drawArgs = null;
    }

    void OnDestroy()
    {
        if (_webcamRaw != null) Destroy(_webcamRaw);
        if (_webcamBuffer != null) Destroy(_webcamBuffer);
        if (_material != null) Destroy(_material);
    }

    void Update()
    {
        // Check if the webcam is ready (needed for macOS support)
        if (_webcamRaw.width <= 16) return;

        // Input buffer update with aspect ratio correction
        var vflip = _webcamRaw.videoVerticallyMirrored;
        var scale = new Vector2(1, vflip ? -1 : 1);
        var offset = new Vector2(0, vflip ? 1 : 0);
        Graphics.Blit(_webcamRaw, _webcamBuffer, scale, offset);

        // Run the object detector with the webcam input.
        _detector.ProcessImage
          (_webcamBuffer, _scoreThreshold, _overlapThreshold);
    }

    void OnPostRender()
    {
        // Bounding box visualization
        _detector.SetIndirectDrawCount(_drawArgs);
        _material.SetFloat("_Threshold", _scoreThreshold);
        _material.SetTexture("_Texture", _texture);
        _material.SetBuffer("_Boxes", _detector.BoundingBoxBuffer);
        _material.SetPass(_texture == null ? 0 : 1);
        Graphics.DrawProceduralIndirectNow
          (MeshTopology.Triangles, _drawArgs, 0);
    }

    #endregion
}

} // namespace UltraFace
