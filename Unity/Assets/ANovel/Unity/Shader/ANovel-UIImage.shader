Shader "ANovel/UIImage"
{
    Properties
    {
        _MainTex("Main", 2D) = "black" {}
        _BackTex("Back", 2D) = "black" {}
        _RuleTex("Rule", 2D) = "grey" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _BackColor("BackColor", Color) = (1, 1, 1, 1)
        _TransValue("TransValue", Range(0,1)) = 0
        _RuleVague("RuleVague", Range(0,0.5)) = 0.1
        [Toggle] _RuleAlpha("RuleUseAlpha", int) = 0
        [KeywordEnum(DEF,RULE)]_ANOVEL_TRANS("Trans", int) = 0
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Opaque"
            "PreviewType" = "Plane"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local __ _ANOVEL_TRANS_RULE

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 color    : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord0 : TEXCOORD0;
                half2 texcoord1 : TEXCOORD1;
                half2 texcoord2 : TEXCOORD2;
                float4 color0    : COLOR;
                float4 color1    : COLOR1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BackTex;
            float4 _BackTex_ST;
            sampler2D _RuleTex;
            float4 _RuleTex_ST;
            float4 _Color;
            float4 _BackColor;
            float _TransValue;
            float _RuleVague;
            int _RuleAlpha;

            v2f vert(appdata v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
                OUT.color0 = v.color * _Color;
                OUT.texcoord1 = TRANSFORM_TEX(v.texcoord0, _BackTex);
                OUT.color1 = v.color * _BackColor;
                OUT.texcoord2 = TRANSFORM_TEX(v.texcoord0, _RuleTex);
                return OUT;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 color = tex2D(_MainTex, i.texcoord0) * i.color0;
                color.rgb *= color.a;
                #ifdef _ANOVEL_TRANS_RULE
                fixed4 back = tex2D(_BackTex, i.texcoord1) * i.color1;
                back.rgb *= back.a;
                fixed4 rule = tex2D(_RuleTex, i.texcoord2);
                rule = smoothstep(_TransValue - _RuleVague, _TransValue + _RuleVague, (rule.x + rule.a * _RuleAlpha)  * (1 - _RuleVague * 2) + _RuleVague);
                color = lerp(back, color, rule);
                #else
                fixed4 back = tex2D(_BackTex, i.texcoord1) * i.color1;
                back.rgb *= back.a;
                color = lerp(color, back, _TransValue);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                return color;
            }
        ENDCG
        }
    }
}
