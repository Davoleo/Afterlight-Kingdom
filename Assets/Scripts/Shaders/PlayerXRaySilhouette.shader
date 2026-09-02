Shader "Custom/PlayerXRaySilhouette"
{
    // Redraws the player mesh a second time. Only survives where the depth
    // buffer already has something CLOSER than the player (ZTest Greater) —
    // i.e. exactly where a wall/prop is blocking the view — and only where
    // that closer pixel is NOT the player's own geometry (Stencil NotEqual 1,
    // written by PlayerStencilWrite.shader via a Render Objects feature).
    Properties
    {
        _SilhouetteTint ("Silhouette Tint", Color) = (1,1,1,1)
        _DotCellSize ("Dot Cell Size (px)", Float) = 6
        _DotRadius ("Dot Radius (0-0.5)", Range(0, 0.5)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "XRaySilhouette"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest Greater
            Blend SrcAlpha OneMinusSrcAlpha
            // Cull Back is the default — matches the body mesh, leave as-is.

            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _SilhouetteTint;
                float _DotCellSize;
                float _DotRadius;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positionInputs.positionCS;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Screen-space (not mesh UV) so dot size stays constant in
                // pixels and the pattern lines up seamlessly across separate
                // renderers (body / bow / quiver).
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 pixelCoord = screenUV * _ScreenParams.xy;

                float2 cell = pixelCoord / max(_DotCellSize, 0.0001);
                float2 cellLocal = frac(cell) - 0.5;
                float distFromCellCenter = length(cellLocal);

                float dotMask = step(distFromCellCenter, _DotRadius);
                clip(dotMask - 0.5); // discard everything outside the dots

                return half4(_SilhouetteTint.rgb, _SilhouetteTint.a);
            }
            ENDHLSL
        }
    }
}
