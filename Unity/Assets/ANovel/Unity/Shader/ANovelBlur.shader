Shader "ANovel/Blur"
{
    Properties
    {
        _MainTex("Main", 2D) = "black" {}
        _Deviation("Deviation", float) = 5
        _SamplingDistance("Sampling Distance", float) = 1.0
    }
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "PreviewType" = "Plane"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "ANovelUtility.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord0 : TEXCOORD0;
            };

            static const int _SampleCount = 5;
            static const int _SampleOffset = _SampleCount/ 2;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Deviation;
            half _SamplingDistance;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 color = 0;

                float2 texelOffset = _MainTex_TexelSize.xy * _SamplingDistance;
                float total = 0;
                UNITY_UNROLL
                for (int x = 0; x < _SampleCount; x++)
                {
                    for (int y = 0; y < _SampleCount; y++)
                    {
                        float w = GaussianWeight(length(float2(x-_SampleOffset, y-_SampleOffset)), _Deviation);
                        total += w;
                        color += tex2D(_MainTex, i.texcoord0 + (texelOffset * float2(x-_SampleOffset, y-_SampleOffset))) * w;
                    }
                }
                return color / total;
            }
        ENDHLSL
        }
    }
}
