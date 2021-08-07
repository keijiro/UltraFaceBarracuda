using UnityEngine;
using UI = UnityEngine.UI;
using Klak.TestTools;

namespace UltraFace {

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _visualizer = null;
    [SerializeField] Texture2D _texture = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Internal objects

    FaceDetector _detector;
    Material _material;
    ComputeBuffer _drawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
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
        if (_material != null) Destroy(_material);
    }

    void Update()
    {
        _previewUI.texture = _source.Texture;

        // Run the object detector with the webcam input.
        _detector.ProcessImage
          (_source.Texture, _scoreThreshold, _overlapThreshold);
    }

    void OnRenderObject()
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
