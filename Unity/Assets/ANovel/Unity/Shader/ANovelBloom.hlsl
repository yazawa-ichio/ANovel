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
sampler2D _BloomTex;
float4 _BloomTex_ST;

half _BrightnessMax;
half _Threshold;
half _SoftKnee;
float _Intensity;
float _SamplingDistance;
half _ThresholdKnee;
float _Distance;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
    o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _BloomTex);
    return o;
}

half4 frag_prefilter(v2f i) : SV_Target
{
    half3 color = tex2D(_MainTex, i.texcoord0).xyz;
    half brightness = GetBrightness(color);
    half softness = clamp(brightness - _Threshold + _ThresholdKnee, 0.0, 2.0 * _ThresholdKnee);
    softness = (softness * softness) / (4.0 * _ThresholdKnee + 1e-4);
    half multiplier = max(brightness - _Threshold, softness) / max(brightness, 1e-4);
    color *= multiplier;

    color = max(color, 0);
    return half4(color, 0);
}

half4 frag_downsample_h(v2f i) : SV_Target
{
    float texelSize = _MainTex_TexelSize.x * _Distance;

    // 9-tap gaussian blur on the downsampled source
    half3 c0 = tex2D(_MainTex, i.texcoord0 - float2(texelSize * 4.0, 0.0));
    half3 c1 = tex2D(_MainTex, i.texcoord0 - float2(texelSize * 3.0, 0.0));
    half3 c2 = tex2D(_MainTex, i.texcoord0 - float2(texelSize * 2.0, 0.0));
    half3 c3 = tex2D(_MainTex, i.texcoord0 - float2(texelSize * 1.0, 0.0));
    half3 c4 = tex2D(_MainTex, i.texcoord0);
    half3 c5 = tex2D(_MainTex, i.texcoord0 + float2(texelSize * 1.0, 0.0));
    half3 c6 = tex2D(_MainTex, i.texcoord0 + float2(texelSize * 2.0, 0.0));
    half3 c7 = tex2D(_MainTex, i.texcoord0 + float2(texelSize * 3.0, 0.0));
    half3 c8 = tex2D(_MainTex, i.texcoord0 + float2(texelSize * 4.0, 0.0));

    half3 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459 + c4 * 0.22702703 + c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

    return half4(color, 0);
}

half4 frag_downsample_v(v2f i) : SV_Target
{
    float texelSize = _MainTex_TexelSize.y * _Distance;
    float2 uv = i.texcoord0;

    // Optimized bilinear 5-tap gaussian on the same-sized source (9-tap equivalent)
    half3 c0 = tex2D(_MainTex, i.texcoord0 - float2(0.0, texelSize * 3.23076923));
    half3 c1 = tex2D(_MainTex, i.texcoord0 - float2(0.0, texelSize * 1.38461538));
    half3 c2 = tex2D(_MainTex, i.texcoord0);
    half3 c3 = tex2D(_MainTex, i.texcoord0 + float2(0.0, texelSize * 1.38461538));
    half3 c4 = tex2D(_MainTex, i.texcoord0 + float2(0.0, texelSize * 3.23076923));

    half3 color = c0 * 0.07027027 + c1 * 0.31621622 + c2 * 0.22702703 + c3 * 0.31621622 + c4 * 0.07027027;

    return half4(color, 0);
}

fixed4 frag_upsample(v2f i) : SV_Target
{
    float4 lowMip = tex2D(_BloomTex, i.texcoord1);
    float4 highMip = tex2D(_MainTex, i.texcoord0);
    return lerp(highMip, lowMip, 0.4);
}

fixed4 frag_finish(v2f i) : SV_Target
{
    float4 base = tex2D(_MainTex, i.texcoord0);
    float4 bloom = tex2D(_BloomTex, i.texcoord1);
    return fixed4(base.rgb + bloom.r * _Intensity, base.a);
}

/*
fixed4 frag_upsample(v2f i) : SV_Target
{
    float4 base = tex2D(_BloomTex, i.texcoord1);
    float color = 0;
    color += tex2D(_MainTex, i.texcoord0).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(-_MainTex_TexelSize.x, 0)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(-_MainTex_TexelSize.x, 0)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(0, _MainTex_TexelSize.y)).x;
    color += tex2D(_MainTex, i.texcoord0 + float2(0, -_MainTex_TexelSize.y)).x;
    return fixed4(base.rgb + color.x * _Intensity / 5, base.a);
}
*/