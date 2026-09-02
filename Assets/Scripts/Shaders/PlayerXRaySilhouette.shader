Shader "Custom/PlayerXRaySilhouette"
{
    // Redraws the player mesh a second time. Only survives where the depth
    // buffer already has something CLOSER than the player (ZTest Greater),
    // AND that closer pixel is specifically wall geometry (Stencil Equal 2,
    // written by WallStencilWrite.shader via a Render Objects feature) —
    // so props and self-occlusion (arm over torso, etc.) never trigger it.
    // A further linear-depth threshold (_MinOcclusionDepth) suppresses the
    // effect for shallow mesh interpenetration (e.g. a foot briefly poking
    // into a block edge mid-animation) so only genuine "hidden behind a
    // wall" occlusion shows the dither.
    Properties
    {
        _SilhouetteTint ("Silhouette Tint", Color) = (1,1,1,1)
        _DotCellSize ("Dot Cell Size (px)", Float) = 6
        _DotRadius ("Dot Radius (0-0.5)", Range(0, 0.5)) = 0.35
        _MinOcclusionDepth ("Min Occlusion Depth (m)", Float) = 0.1
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
                Ref 2
                Comp Equal
                Pass Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

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
                float _MinOcclusionDepth;
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

                // Stencil already guarantees the closer pixel here is wall
                // geometry, so the scene depth sample at this UV IS the
                // wall's own depth. Compare it to this fragment's (the
                // player's) depth to measure how far behind the wall the
                // player actually is.
                float sceneDeviceDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(sceneDeviceDepth, _ZBufferParams);
                float playerEyeDepth = LinearEyeDepth(IN.positionHCS.z, _ZBufferParams);
                float occlusionDepth = playerEyeDepth - sceneEyeDepth;

                clip(occlusionDepth - _MinOcclusionDepth); // discard shallow clipping

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
