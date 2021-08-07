using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace UltraFace {

public sealed class FaceDetector : System.IDisposable
{
    #region Internal objects

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    ComputeBuffer _post1Buffer;
    ComputeBuffer _post2Buffer;
    ComputeBuffer _countBuffer;
    IWorker _worker;

    #endregion

    #region Public constructor

    public FaceDetector(ResourceSet resources)
    {
        _resources = resources;

        _preBuffer = new ComputeBuffer(Config.InputSize, sizeof(float));

        _post1Buffer = new ComputeBuffer
          (Config.MaxDetection, BoundingBox.Size, ComputeBufferType.Append);

        _post2Buffer = new ComputeBuffer
          (Config.MaxDetection, BoundingBox.Size, ComputeBufferType.Append);

        _countBuffer = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        _worker = ModelLoader.Load(_resources.model).CreateWorker();
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _post1Buffer?.Dispose();
        _post1Buffer = null;

        _post2Buffer?.Dispose();
        _post2Buffer = null;

        _countBuffer?.Dispose();
        _countBuffer = null;

        _worker?.Dispose();
        _worker = null;
    }

    #endregion

    #region Public accessors

    public ComputeBuffer BoundingBoxBuffer
      => _post2Buffer;

    public void SetIndirectDrawCount(ComputeBuffer drawArgs)
      => ComputeBuffer.CopyCount(_post2Buffer, drawArgs, sizeof(uint));

    public IEnumerable<BoundingBox> DetectedFaces
      => _post2ReadCache ?? UpdatePost2ReadCache();

    #endregion

    #region Main image processing function

    public void ProcessImage
      (Texture sourceTexture, float scoreThreshold, float overlapThreshold)
    {
        // Constant aliases
        const int width = Config.ImageWidth;
        const int height = Config.ImageHeight;
        const int maxHits = Config.MaxDetection;

        // Reset the compute buffer counters.
        _post1Buffer.SetCounterValue(0);
        _post2Buffer.SetCounterValue(0);

        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetTexture(0, "_Texture", sourceTexture);
        pre.SetBuffer(0, "_Tensor", _preBuffer);
        pre.Dispatch(0, width / 8, height / 8, 1);

        // Run the UltraFace model.
        using (var tensor = new Tensor(1, height, width, 3, _preBuffer))
            _worker.Execute(tensor);

        // Output tensors -> Temporary render textures
        var fmt = RenderTextureFormat.RFloat;
        var scoresRT = RenderTexture.GetTemporary(2, maxHits, 0, fmt);
        var  boxesRT = RenderTexture.GetTemporary(4, maxHits, 0, fmt);

        using (var tensor = _worker.PeekOutput("scores")
                                   .Reshape(new TensorShape(1, maxHits, 2, 1)))
            tensor.ToRenderTexture(scoresRT);

        using (var tensor = _worker.PeekOutput("boxes")
                                   .Reshape(new TensorShape(1, maxHits, 4, 1)))
            tensor.ToRenderTexture(boxesRT);

        // 1st postprocess (bounding box aggregation)
        var post1 = _resources.postprocess1;
        post1.SetFloat("_Threshold", scoreThreshold);
        post1.SetTexture(0, "_Scores", scoresRT);
        post1.SetTexture(0, "_Boxes", boxesRT);
        post1.SetBuffer(0, "_Output", _post1Buffer);
        post1.Dispatch(0, maxHits / 20, 1, 1);

        RenderTexture.ReleaseTemporary(scoresRT);
        RenderTexture.ReleaseTemporary(boxesRT);

        // Bounding box count
        ComputeBuffer.CopyCount(_post1Buffer, _countBuffer, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetFloat("_Threshold", overlapThreshold);
        post2.SetBuffer(0, "_Input", _post1Buffer);
        post2.SetBuffer(0, "_Count", _countBuffer);
        post2.SetBuffer(0, "_Output", _post2Buffer);
        post2.Dispatch(0, 1, 1, 1);

        // Bounding box count after removal
        ComputeBuffer.CopyCount(_post2Buffer, _countBuffer, 0);

        // Read cache invalidation
        _post2ReadCache = null;
    }

    #endregion

    #region GPU to CPU readback function

    BoundingBox[] _post2ReadCache;
    int[] _countReadCache = new int[1];

    BoundingBox[] UpdatePost2ReadCache()
    {
        _countBuffer.GetData(_countReadCache, 0, 0, 1);
        var buffer = new BoundingBox[_countReadCache[0]];
        _post2Buffer.GetData(buffer, 0, 0, buffer.Length);
        return buffer;
    }

    #endregion
}

} // namespace UltraFace
