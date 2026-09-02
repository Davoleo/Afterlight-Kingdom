Shader "Custom/WallStencilWrite"
{
    // Assigned as the Material override on the "WallStencilWrite" Render
    // Objects feature (Event: After Rendering Opaques, Layer Mask: Wall).
    // Draws nothing (ColorMask 0) — its only job is to stamp stencil = 2
    // wherever a wall's own frontmost (visible) pixels already are.
    // Replaces the old PlayerStencilWrite approach: tagging walls instead
    // of the player means self-occlusion (arm over torso, etc.) is
    // excluded for free, since player geometry is never tagged "wall".
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "StencilWrite"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            ZTest LEqual
            ColorMask 0

            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
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
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
