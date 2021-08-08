using Unity.Barracuda;
using UnityEngine;

namespace UltraFace {

struct Config
{
    #region Compile-time constants

    // These values must be matched with the ones defined in Common.hlsl.
    public const int MaxDetection = 512;

    #endregion

    #region Variables from tensor shapes

    public int InputWidth { get; private set; }
    public int InputHeight { get; private set; }

    #endregion

    #region Data size calculation properties

    public int InputFootprint => InputWidth * InputWidth * 3;

    #endregion

    #region Tensor shape utilities

    public TensorShape InputShape
      => new TensorShape(1, InputHeight, InputWidth, 3);

    #endregion

    #region Constructor

    public Config(ResourceSet resources)
    {
        InputWidth = 320;
        InputHeight = 240;
    }

    #endregion
}

} // namespace UltraFace
