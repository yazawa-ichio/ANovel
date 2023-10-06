
float3 RGB2HSV(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-4;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSV2RGB(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float GaussianWeight(float x, float deviation)
{
	float a = 1.0 / (2.506628 * deviation);
	float b = exp(-(x * x) / (2.0 * deviation * deviation));
	return a * b;
}

half GetBrightness(half3 color)
{
	return max(color.r, max(color.g, color.b));
}

half GetGrayscale(half3 color)
{
	return color.r * 0.2126 + color.g * 0.7152 + color.b * 0.0722;
}

half3 GetSepia(half3 color)
{
	float v = color.r * 0.291 + color.g * 0.571 + color.b * 0.14;
	return half3(v * 1.351, v * 1.200, v * 0.934);
}