#include "ANovelBlend.hlsl"

struct ANovelVertexInput
{
    float4 vertex : POSITION;
    float2 texcoord0 : TEXCOORD0;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ANovelVertexOutput
{
    float4 vertex : SV_POSITION;
    half2 texcoord0 : TEXCOORD0;
    half2 texcoord1 : TEXCOORD1;
#ifdef _ANOVEL_TRANS_RULE
    half2 texcoord2 : TEXCOORD2;
#endif
#if _ANOVEL_ENV_COLOR_TEX
    half2 texcoord3 : TEXCOORD3;
#endif
    float4 color0 : COLOR;
    float4 color1 : COLOR1;
    UNITY_VERTEX_OUTPUT_STEREO
};

ANovelVertexOutput DefaultVert(ANovelVertexInput v)
{
    ANovelVertexOutput OUT;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
    OUT.vertex = UnityObjectToClipPos(v.vertex);
    OUT.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
    OUT.color0 = v.color * _Color;
    OUT.texcoord1 = TRANSFORM_TEX(v.texcoord0, _BackTex);
    OUT.color1 = v.color * _BackColor;
#ifdef _ANOVEL_TRANS_RULE
    OUT.texcoord2 = TRANSFORM_TEX(v.texcoord0, _RuleTex);
#endif
#if _ANOVEL_ENV_COLOR_TEX
    OUT.texcoord3 = TRANSFORM_TEX(v.texcoord0, _EnvColorTex);
#endif
    return OUT;
}

inline float4 getEnvColor(ANovelVertexOutput i)
{
#if defined(_ANOVEL_ENV_COLOR)
    return _EnvColor;
#elif defined(_ANOVEL_ENV_COLOR_TEX)
    return tex2D(_EnvColorTex, i.texcoord3);
#else
    return 0;
#endif
}

fixed4 DefaultFrag(ANovelVertexOutput i) : COLOR
{
    fixed4 color = tex2D(_MainTex, i.texcoord0) * i.color0;
    color.rgb *= color.a;

#ifdef _ANOVEL_TRANS_RULE
    fixed4 back = tex2D(_BackTex, i.texcoord1) * i.color1;
    back.rgb *= back.a;
    fixed4 rule = tex2D(_RuleTex, i.texcoord2);
    rule = smoothstep(_TransValue - _RuleVague, _TransValue + _RuleVague, (rule.x + rule.a * _RuleAlpha) * (1 - _RuleVague * 2) + _RuleVague);
    color = lerp(back, color, rule);
#else
    fixed4 back = tex2D(_BackTex, i.texcoord1) * i.color1;
    back.rgb *= back.a;
    color = lerp(color, back, _TransValue);
#endif

#if defined(_ANOVEL_ENV_COLOR) || defined(_ANOVEL_ENV_COLOR_TEX)
    float4 envColor = getEnvColor(i);
    envColor.rgb *= color.a;
    color.rgb = BlendEnvColor(color.rgb, envColor.rgb, _EnvColorBlendMode);
#endif

    return color;
}
