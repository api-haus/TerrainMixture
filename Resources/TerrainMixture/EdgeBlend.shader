Shader "Hidden/TerrainMixture/EdgeBlend"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D_X(_EdgeTex);
            SAMPLER(sampler_EdgeTex);

            float2 _TexelSize;
            int _Side;
            int _Blend;

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = float2(i.texcoord.x, 1. - i.texcoord.y);

                float4 sample = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);

                if (_Side == 0)
                {
                    if (uv.y >= 1. - _TexelSize.y)
                    {
                        float2 edgeCoord = float2(
                            uv.x,
                            _TexelSize.y * .5
                        );
                        float4 edgeSample = SAMPLE_TEXTURE2D_X(
                            _EdgeTex, sampler_EdgeTex, edgeCoord);

                        return _Blend > .5 ? lerp(sample, edgeSample, .5) : edgeSample;
                    }
                }
                else
                {
                    if (uv.x >= 1. - _TexelSize.x)
                    {
                        float2 edgeCoord = float2(
                            _TexelSize.x * .5,
                            uv.y
                        );
                        float4 edgeSample = SAMPLE_TEXTURE2D_X(
                            _EdgeTex, sampler_EdgeTex, edgeCoord);

                        return _Blend > .5 ? lerp(sample, edgeSample, .5) : edgeSample;
                    }
                }

                return sample;
            }
            ENDHLSL
        }
    }
}
