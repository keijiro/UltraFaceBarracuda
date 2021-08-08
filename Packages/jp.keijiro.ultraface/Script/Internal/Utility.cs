using Unity.Barracuda;
using UnityEngine;

namespace UltraFace {

#region Object construction/destruction helpers

static class ObjectUtil
{
    public static void Destroy(Object o)
    {
        if (o == null) return;
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}

static class RTUtil
{
    public static RenderTexture NewFloat(int w, int h)
      => new RenderTexture(w, h, 0, RenderTextureFormat.RFloat);
}

#endregion

#region Extension methods

static class ComputeShaderExtensions
{
    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);

        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;

        compute.Dispatch(kernel, x, y, z);
    }
}

static class IWorkerExtensions
{
    public static void CopyOutput
      (this IWorker worker, string tensorName, RenderTexture rt)
    {
        var output = worker.PeekOutput(tensorName);
        var shape = new TensorShape(1, rt.height, rt.width, 1);
        using var tensor = output.Reshape(shape);
        tensor.ToRenderTexture(rt);
    }
}

#endregion

#region GPU to CPU readback helpers

sealed class DetectionCache
{
    ComputeBuffer _dataBuffer;
    ComputeBuffer _countBuffer;

    Detection[] _cached;
    int[] _countRead = new int[1];

    public DetectionCache(ComputeBuffer data, ComputeBuffer count)
      => (_dataBuffer, _countBuffer) = (data, count);

    public Detection[] Cached => _cached ?? UpdateCache();

    public void Invalidate() => _cached = null;

    public Detection[] UpdateCache()
    {
        _countBuffer.GetData(_countRead, 0, 0, 1);
        var count = _countRead[0];

        _cached = new Detection[count];
        _dataBuffer.GetData(_cached, 0, 0, count);

        return _cached;
    }
}

#endregion

} // namespace UltraFace
