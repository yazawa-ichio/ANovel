

Texture2D<float4> inputTexture; // 入力テクスチャ
SamplerState sampleType;        // サンプリングタイプ

float4 GaussianBlurPS(float2 texCoord
                      : TEXCOORD) : SV_TARGET
{
    float4 color = 0;
    float weight = 0;

    UNITY_UNROLL
    for (int x = -5; x <= 5; x++)
    {
        for (int y = -5; y <= 5; y++)
        {
            float2 offset = float2(x, y) * 0.5; // オフセット
            float4 sampleColor = inputTexture.Sample(sampleType, texCoord + offset);
            float sampleWeight = exp(-(x * x + y * y) / (2 * 2.0)); // ガウシアン関数

            color += sampleColor * sampleWeight;
            weight += sampleWeight;
        }
    }

    return color / weight;
}