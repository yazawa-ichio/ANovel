#include "UnityCG.cginc"
#include "ANovelUtility.hlsl"

#pragma multi_compile_local _ANOVEL_COLOR_CHANGE_GRAYSCALE _ANOVEL_COLOR_CHANGE_SEPIA _ANOVEL_COLOR_CHANGE_NEGATIVE _ANOVEL_COLOR_CHANGE_HSV

struct ANovelColorEffectInput
{
	float4 vertex : POSITION;
	float2 texcoord0 : TEXCOORD0;
};

struct ANovelColorEffectOutput
{
	float4 vertex : SV_POSITION;
	half2 texcoord0 : TEXCOORD0;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _Rate;
float4 _HSV;

ANovelColorEffectOutput ColorEffectVert(ANovelColorEffectInput v)
{
	ANovelColorEffectOutput o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
	return o;
}

fixed4 ColorEffectFrag(ANovelColorEffectOutput i) : SV_Target
{
	half4 color = tex2D(_MainTex, i.texcoord0);
#if _ANOVEL_COLOR_CHANGE_GRAYSCALE
	color.rgb = lerp(color.rgb, GetGrayscale(color.rgb), _Rate);
#elif _ANOVEL_COLOR_CHANGE_SEPIA
	color.rgb = lerp(color.rgb, GetSepia(color.rgb), _Rate);
#elif _ANOVEL_COLOR_CHANGE_NEGATIVE
	color.rgb = lerp(color.rgb, 1 - color.rgb, _Rate);
#elif _ANOVEL_COLOR_CHANGE_HSV
	float3 hsv = RGB2HSV(color.rgb);
	hsv.r = frac(hsv.r + _HSV.r);
	hsv.g = saturate(hsv.g * _HSV.g);
	hsv.b = saturate(hsv.b * _HSV.b);
	color.rgb = lerp(color.rgb, HSV2RGB(hsv), _Rate);
#endif
	return color;
}