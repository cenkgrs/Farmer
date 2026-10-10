Shader "Farmer/Soft Road"
{
    Properties
    {
        _BaseMap ("Paving", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.035
        _RoadWidth ("Width in metres", Float) = 3
        _RoadLength ("Square length (0 for continuous road)", Float) = 0
        _EdgeWidth ("Soft shoulder in metres", Float) = 0.55
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-10" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #define _SURFACE_TYPE_TRANSPARENT 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Smoothness;
                float _RoadWidth, _RoadLength, _EdgeWidth;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; half fog : TEXCOORD3; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv; o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 cell = floor(p), f = frac(p); f = f*f*(3-2*f);
                return lerp(lerp(Hash(cell),Hash(cell+float2(1,0)),f.x),lerp(Hash(cell+float2(0,1)),Hash(cell+1),f.x),f.y);
            }
            half4 Frag(Varyings i) : SV_Target
            {
                // Fade the paving itself so the actual ground shows through, with no duplicate grass UV seams.
                float edge = min(i.uv.x, 1-i.uv.x) * _RoadWidth;
                if (_RoadLength > 0) edge = min(edge, min(i.uv.y,1-i.uv.y) * _RoadLength);
                else edge = min(edge, i.uv.y * 3); // Start fades; the far end continues into the square.
                float noise = Noise(i.positionWS.xz*3.7)*.7 + Noise(i.positionWS.xz*13)*.3;
                half alpha = smoothstep(.035, max(.06,_EdgeWidth), edge - noise*.18);
                half4 paving = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv*_BaseMap_ST.xy+_BaseMap_ST.zw)*_BaseColor;
                InputData input = (InputData)0;
                input.positionWS=i.positionWS; input.normalWS=NormalizeNormalPerPixel(i.normalWS);
                input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                input.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
                input.bakedGI=SampleSH(input.normalWS);
                input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                input.shadowMask=half4(1,1,1,1);
                input.vertexLighting=VertexLighting(i.positionWS,input.normalWS);
                SurfaceData surface=(SurfaceData)0;
                surface.albedo=paving.rgb; surface.alpha=alpha*paving.a;
                surface.normalTS=half3(0,0,1); surface.occlusion=1; surface.smoothness=_Smoothness;
                half4 color=UniversalFragmentPBR(input,surface);
                color.rgb=MixFog(color.rgb,i.fog); color.a=surface.alpha;
                return color;
            }
            ENDHLSL
        }
    }
}
