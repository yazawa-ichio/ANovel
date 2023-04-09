#include "UnityCG.cginc"
#include "ANovelUtility.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float2 texcoord0 : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    half2 texcoord0 : TEXCOORD0;
    half2 texcoord1 : TEXCOORD1;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _MainTex_TexelSize;
sampler2D _SourceTex;
float4 _SourceTex_ST;

half _BrightnessMax;
half _Threshold;
half _SoftKnee;
float _Intensity;
float _SamplingDistance;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
    o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _SourceTex);
    return o;
}

half4 frag_prefilter(v2f i) : SV_Target
{
    half3 color = tex2D(_MainTex, i.texcoord0).xyz;
    half brightness = min(GetBrightness(color), _BrightnessMax);

    float knee = _Threshold * _SoftKnee + 1e-5f;
    half x = clamp(brightness - (_Threshold - knee), 0, knee * 2);
    x = x * x * 0.25f / knee;
    return half4(max(x, brightness - _Threshold) / max(brightness, 1e-5), 0, 0, 1);
}

half4 frag_downsample(v2f i) : SV_Target
{
    float color = 0;
    color += tex2D(_MainTex, i.texcoord0).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(-_MainTex_TexelSize.x, 0)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(-_MainTex_TexelSize.x, 0)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(0, _MainTex_TexelSize.y)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(0, -_MainTex_TexelSize.y)).x;
    return half4(color / 5, 0, 0, 1.0);
}

fixed4 frag_upsample(v2f i) : SV_Target
{
    float4 base = tex2D(_SourceTex, i.texcoord1);
    float color = 0;
    color += tex2D(_MainTex, i.texcoord0).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(-_MainTex_TexelSize.x, 0)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(-_MainTex_TexelSize.x, 0)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(0, _MainTex_TexelSize.y)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(0, -_MainTex_TexelSize.y)).x;
    return fixed4(base.rgb + color.x * _Intensity / 5, base.a);
}
