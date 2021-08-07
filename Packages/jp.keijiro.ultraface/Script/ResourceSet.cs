using UnityEngine;
using Unity.Barracuda;

namespace UltraFace
{
    [CreateAssetMenu(fileName = "UltraFace",
                     menuName = "ScriptableObjects/UltraFace Resource Set")]
    public sealed class ResourceSet : ScriptableObject
    {
        public NNModel model;
        public ComputeShader preprocess;
        public ComputeShader postprocess1;
        public ComputeShader postprocess2;
    }
}
