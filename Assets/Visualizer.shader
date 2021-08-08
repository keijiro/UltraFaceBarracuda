Shader "Hidden/UltraFace/Visualizer"
{
    Properties
    {
        _Texture("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Packages/jp.keijiro.ultraface/Shader/Common.hlsl"

    float _Threshold;
    sampler2D _Texture;
    StructuredBuffer<Detection> _Detections;

    //
    // Pass 0: Simple fill
    //

    void VertexFill(uint vid : SV_VertexID,
                    uint iid : SV_InstanceID,
                    out float4 position : SV_Position,
                    out float4 color : COLOR)
    {
        // UV coordinates
        float u = vid & 1;
        float v = vid < 2 || vid == 5;

        // Select a bounding box.
        Detection box = _Detections[iid];

        // Vertex position
        float x =  2 * lerp(box.x1, box.x2, u) - 1;
        float y = -2 * lerp(box.y1, box.y2, v) + 1;

        // Opacity from the score value
        float alpha = (box.score - _Threshold) / (1 - _Threshold);

        // Vertex attributes
        position = float4(x, y, 1, 1);
        color = float4(1, 0, 0, alpha * 0.8);
    }

    float4 FragmentFill(float4 position : SV_Position,
                        float4 color : COLOR) : SV_Target
    {
        return color;
    }

    //
    // Pass 2: Textured square (emoji)
    //

    void VertexTextured(uint vid : SV_VertexID,
                        uint iid : SV_InstanceID,
                        out float4 position : SV_Position,
                        out float2 uv : TEXCOORD0,
                        out float4 color : COLOR)
    {
        // UV coordinates
        float u = vid & 1;
        float v = vid < 2 || vid == 5;

        // Select a bounding box.
        Detection box = _Detections[iid];

        // Box center
        float2 center = float2(box.x1 + box.x2, box.y1 + box.y2) / 2;

        // The longest edge of the box
        float size = max(box.x2 - box.x1, box.y2 - box.y1) * 0.53;

        // Clip space position
        float aspect = _ScreenParams.y / _ScreenParams.x;
        float x = center.x + lerp(-1, 1, u) * size * aspect;
        float y = center.y + lerp(-1, 1, v) * size;

        // Clip space to screen space
        x =  2 * x - 1;
        y = -2 * y + 1;

        // Opacity from the score value
        float alpha = (box.score - _Threshold) / (1 - _Threshold);

        // Vertex attributes
        position = float4(x, y, 1, 1);
        uv = float2(u, 1 - v);
        color = float4(1, 0, 0, alpha);
    }

    float4 FragmentTextured(float4 position : SV_Position,
                            float2 uv : TEXCOORD0,
                            float4 color : COLOR) : SV_Target
    {
        return tex2D(_Texture, uv) * color.a;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex VertexFill
            #pragma fragment FragmentFill
            ENDCG
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex VertexTextured
            #pragma fragment FragmentTextured
            ENDCG
        }
    }
}
