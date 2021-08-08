using UnityEngine;
using UI = UnityEngine.UI;
using Klak.TestTools;

namespace UltraFace {

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [SerializeField, Range(0, 1)] float _threshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _visualizer = null;
    [SerializeField] Texture2D _texture = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Private objects

    FaceDetector _detector;
    Material _material;
    ComputeBuffer _drawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _detector = new FaceDetector(_resources);
        _material = new Material(_visualizer);
        _drawArgs = new ComputeBuffer(4, sizeof(uint),
                                      ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});
    }

    void OnDestroy()
    {
        _detector?.Dispose();
        Destroy(_material);
        _drawArgs?.Dispose();
    }

    void Update()
    {
        _detector.ProcessImage(_source.Texture, _threshold);
        _previewUI.texture = _source.Texture;

        if (Input.GetMouseButtonDown(0))
            foreach (var d in _detector.Detections) Debug.Log(d);
    }

    void OnRenderObject()
    {
        _detector.SetIndirectDrawCount(_drawArgs);
        _material.SetFloat("_Threshold", _threshold);
        _material.SetTexture("_Texture", _texture);
        _material.SetBuffer("_Detections", _detector.DetectionBuffer);
        _material.SetPass(_texture == null ? 0 : 1);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, _drawArgs, 0);
    }

    #endregion
}

} // namespace UltraFace
