using System.Runtime.InteropServices;

namespace UltraFace {

// Detection data structure - The layout of this structure must be matched
// with the one defined in Common.hlsl.
[StructLayout(LayoutKind.Sequential)]
public readonly struct Detection
{
    public readonly float x1, y1, x2, y2;
    public readonly float score;
    public readonly float pad1, pad2, pad3;

    // sizeof(Detection)
    public static int Size = 8 * sizeof(float);

    // String formatting
    public override string ToString()
      => $"({x1},{y1})-({x2},{y2}):({score})";
};

} // namespace UltraFace
