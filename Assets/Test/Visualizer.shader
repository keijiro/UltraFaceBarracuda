Shader "Hidden/UltraFace/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "../Shader/Common.hlsl"

    float _Threshold;
    StructuredBuffer<BoundingBox> _Boxes;

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        BoundingBox box = _Boxes[iid];

        // Bounding box vertex
        float x = lerp(box.x1, box.x2, vid & 1);
        float y = lerp(box.y1, box.y2, vid < 2 || vid == 5);

        // Clip space to screen space
        x =  2 * x - 1;
        y = -2 * y + 1;

        // Opacity from score value
        float alpha = (box.score - _Threshold) / (1 - _Threshold);

        // Vertex attributes
        position = float4(x, y, 1, 1);
        color = float4(1, 0, 0, alpha);
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        return color * color.a;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend One One
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
