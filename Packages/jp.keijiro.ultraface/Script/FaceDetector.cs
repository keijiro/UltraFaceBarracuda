using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace UltraFace {

public sealed class FaceDetector : System.IDisposable
{
    #region Public methods/properties

    public FaceDetector(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture sourceTexture, float threshold)
      => RunModel(sourceTexture, threshold);

    public IEnumerable<Detection> Detections
      => _readCache.Cached;

    public ComputeBuffer DetectionBuffer
      => _buffers.post2;

    public void SetIndirectDrawCount(ComputeBuffer drawArgs)
      => ComputeBuffer.CopyCount(_buffers.post2, drawArgs, sizeof(uint));

    #endregion

    #region Private objects

    ResourceSet _resources;
    Config _config;
    IWorker _worker;

    (ComputeBuffer preprocess,
     RenderTexture scores,
     RenderTexture boxes,
     ComputeBuffer post1,
     ComputeBuffer post2,
     ComputeBuffer counter,
     ComputeBuffer countRead) _buffers;

    DetectionCache _readCache;

    void AllocateObjects(ResourceSet resources)
    {
        // NN model loading
        var model = ModelLoader.Load(resources.model);

        // Private object initialization
        _resources = resources;
        _config = new Config(resources, model);
        _worker = model.CreateWorker();

        // Buffer allocation
        _buffers.preprocess = new ComputeBuffer
          (_config.InputFootprint, sizeof(float));

        _buffers.scores = RTUtil.NewFloat2(_config.OutputCount / 20, 20);
        _buffers.boxes = RTUtil.NewFloat4(_config.OutputCount / 20, 20);

        _buffers.post1 = new ComputeBuffer
          (Config.MaxDetection, Detection.Size);

        _buffers.post2 = new ComputeBuffer
          (Config.MaxDetection, Detection.Size, ComputeBufferType.Append);

        _buffers.counter = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Counter);

        _buffers.countRead = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        // Detection data read cache initialization
        _readCache = new DetectionCache(_buffers.post2, _buffers.countRead);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _buffers.preprocess?.Dispose();
        _buffers.preprocess = null;

        ObjectUtil.Destroy(_buffers.scores);
        _buffers.scores = null;

        ObjectUtil.Destroy(_buffers.boxes);
        _buffers.boxes = null;

        _buffers.post1?.Dispose();
        _buffers.post1 = null;

        _buffers.post2?.Dispose();
        _buffers.post2 = null;

        _buffers.counter?.Dispose();
        _buffers.counter = null;

        _buffers.countRead?.Dispose();
        _buffers.countRead = null;
    }

    #endregion

    #region Main image processing function

    void RunModel(Texture source, float threshold)
    {
        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInts("ImageSize", _config.InputWidth, _config.InputHeight);
        pre.SetTexture(0, "Input", source);
        pre.SetBuffer(0, "Output", _buffers.preprocess);
        pre.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);

        // NNworker invocation
        using (var t = new Tensor(_config.InputShape, _buffers.preprocess))
            _worker.Execute(t);

        // NN output retrieval
        _worker.CopyOutput("scores", _buffers.scores);
        _worker.CopyOutput("boxes", _buffers.boxes);

        // Counter buffer reset
        _buffers.post2.SetCounterValue(0);
        _buffers.counter.SetCounterValue(0);

        // First stage postprocessing: detection data aggregation
        var post1 = _resources.postprocess1;
        post1.SetTexture(0, "Scores", _buffers.scores);
        post1.SetTexture(0, "Boxes", _buffers.boxes);
        post1.SetDimensions("InputSize", _buffers.boxes);
        post1.SetFloat("Threshold", threshold);
        post1.SetBuffer(0, "Output", _buffers.post1);
        post1.SetBuffer(0, "OutputCount", _buffers.counter);
        post1.DispatchThreadPerPixel(0, _buffers.boxes);

        // Second stage postprocessing: overlap removal
        var post2 = _resources.postprocess2;
        post2.SetFloat("Threshold", 0.5f);
        post2.SetBuffer(0, "Input", _buffers.post1);
        post2.SetBuffer(0, "InputCount", _buffers.counter);
        post2.SetBuffer(0, "Output", _buffers.post2);
        post2.Dispatch(0, 1, 1, 1);

        // Detection count after removal
        ComputeBuffer.CopyCount(_buffers.post2, _buffers.countRead, 0);

        // Cache data invalidation
        _readCache.Invalidate();
    }

    #endregion
}

} // namespace UltraFace
