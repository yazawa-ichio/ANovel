Shader "ANovel/ChromaticAberration"
{
    Properties
    {
        _MainTex("Main", 2D) = "black" {}
        _ChromaPosition("ChromaPosition", Vector) = (0,0,0,0)
        _ChromaAmountCenter("ChromaAmountCenter", float) = 0
        _ChromaAmountAround("ChromaAmountAround", float) = 0
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float2 _ChromaPosition;
            float _ChromaAmountCenter;
            float _ChromaAmountAround;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.texcoord0);

                float2 coords = 2.0 * i.texcoord0 - 1.0;
                float dist = (length(coords - _ChromaPosition));
                float2 end = i.texcoord0 - coords * lerp(_ChromaAmountCenter, _ChromaAmountAround, dist);
                float2 delta = (end - i.texcoord0) / 3.0;

                col.g = tex2D(_MainTex, saturate(i.texcoord0 + delta)).g;
                col.b = tex2D(_MainTex, saturate(i.texcoord0 + delta * 2.0)).b;

                return col;
            }
        ENDHLSL
        }
    }
}
