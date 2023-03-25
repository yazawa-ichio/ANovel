

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

// 0 通常
inline float3 BlendNormal(float3 src, float3 dst)
{
    return src;
}
// 1 ディザ合成
inline float3 BlendDissolve(float3 src, float3 dst)
{
    // WIP
    return src;
}
// 2 背景
inline float3 BlendBehine(float3 src, float3 dst)
{
    // WIP
    return src;
}
// 3 消去
inline float3 BlendClear(float3 src, float3 dst)
{
    // WIP
    return src;
}
// 4 比較（暗）
inline float3 BlendDarken(float3 src, float3 dst)
{
    return min(src, dst);
}
// 5 乗算
inline float3 BlendMultiply(float3 src, float3 dst)
{
    return src * dst;
}
// 6 焼き込みカラー
inline float3 dstBurn(float3 src, float3 dst)
{
    return saturate(1 - (1 - src) / (dst));
}
// 7 焼き込み（リニア）
inline float3 BlendLinearBurn(float3 src, float3 dst)
{
    return saturate(src + dst - 1);
}
// 8 カラー比較（暗）
inline float3 BlendDarkerColor(float3 src, float3 dst)
{
    if (src.r + src.g + src.b <= dst.r + dst.g + dst.b)
    {
        return src;
    }
    else
    {
        return dst;
    }
}
// 9 比較（明）
inline float3 BlendLighten(float3 src, float3 dst)
{
    return max(src, dst);
}
// 10 スクリーン
inline float3 BlendScreen(float3 src, float3 dst)
{
    return 1 - (1 - src) * (1 - dst);
}
// 11 覆い焼きカラー
inline float3 dstDodge(float3 src, float3 dst)
{
    return saturate(src / (1 - dst));
}
// 12 覆い焼（リニア）加算
inline float3 BlendLinearDodge(float3 src, float3 dst)
{
    return saturate(src + dst);
}
// 13 カラー比較（明）
inline float3 BlendLighterColor(float3 src, float3 dst)
{
    if (src.r + src.g + src.b > dst.r + dst.g + dst.b)
    {
        return src;
    }
    else
    {
        return dst;
    }
}
// 14 オーバーレイ
inline float3 BlendOverlay(float3 src, float3 dst)
{
    float3 color = (dst < 0.5) ? (2 * src * dst) : (1 - 2 * (1 - src) * (1 - dst));
    return saturate(color);
}
// 15 ソフトライト
inline float3 BlendSoftLight(float3 src, float3 dst)
{
    float3 color = (dst < 0.5) ? (2 * src * dst + src * src * (1 - 2 * dst)) : (sqrt(src) * (2 * dst - 1) + (2 * src) * (1 - dst));
    return saturate(color);
}
// 16 ハードライト
inline float3 BlendHardLight(float3 src, float3 dst)
{
    return BlendOverlay(dst, src);
}
// 17 ビビットライト
inline float3 BlendVividLight(float3 src, float3 dst)
{
    float3 color = (dst < 0.5) ? (1 - ((1 - src) / (2 * dst))) : (src / (2 * (1 - dst)));
    return saturate(color);
}
// 18 リニアライト
inline float3 BlendLinearLight(float3 src, float3 dst)
{
    float3 color = (dst < 0.5) ? (dst + 2 * src - 1) : (dst + 2 * (src - 0.5));
    return saturate(color);
}
// 19 ピンライト
inline float3 BlendPinLight(float3 src, float3 dst)
{
    float3 color = (dst < 2 * src - 1) ? max(src, 2 * dst - 1) : min(src, 2 * dst);
    return saturate(color);
}
// 20 ハードミックス
inline float3 BlendHardMix(float3 src, float3 dst)
{
    return floor(src + dst);
}
// 21 差の絶対値
inline float3 BlendDifference(float3 src, float3 dst)
{
    return abs(dst - src);
}
// 22 減算
inline float3 BlendSubtract(float3 src, float3 dst)
{
    return saturate(src - dst);
}
// 23 除算
inline float3 BlendDivide(float3 src, float3 dst)
{
    return saturate(src / dst);
}
// 24 除外
inline float3 BlendExclusion(float3 src, float3 dst)
{
    return saturate(src + dst - 2 * src * dst);
}
// 25 色相
inline float3 BlendHue(float3 src, float3 dst)
{
    float3 srcHSV = RGB2HSV(src);
    float3 dstHSV = RGB2HSV(dst);
    return HSV2RGB(float3(dstHSV.x, srcHSV.y, srcHSV.z));
}
// 26 彩度
inline float3 BlendSaturation(float3 src, float3 dst)
{
    float3 srcHSV = RGB2HSV(src);
    float3 dstHSV = RGB2HSV(dst);
    return HSV2RGB(float3(srcHSV.x, dstHSV.y, srcHSV.z));
}
// 27 カラー
inline float3 BlendColor(float3 src, float3 dst)
{
    float3 srcHSV = RGB2HSV(src);
    float3 dstHSV = RGB2HSV(dst);
    return HSV2RGB(float3(dstHSV.x, dstHSV.y, srcHSV.z));
}
// 28 輝度
inline float3 BlendLuminosity(float3 src, float3 dst)
{
    float3 srcHSV = RGB2HSV(src);
    float3 dstHSV = RGB2HSV(dst);
    return HSV2RGB(float3(srcHSV.x, srcHSV.y, dstHSV.z));
}

inline float3 BlendEnvColor(float3 src, float3 dst, int mode)
{
    switch (mode)
    {
    // case 0: // 通常
    //     return BlendNormal(src, dst);
    case 1: // ディザ合成
        return BlendDissolve(src, dst);
    case 2: // 背景
        return BlendBehine(src, dst);
    case 3: // 消去
        return BlendClear(src, dst);
    case 4: // 比較（暗）
        return BlendDarken(src, dst);
    case 5: // 乗算
        return BlendMultiply(src, dst);
    case 6: // 焼き込みカラー
        return dstBurn(src, dst);
    case 7: // 焼き込み（リニア）
        return BlendLinearBurn(src, dst);
    case 8: // カラー比較（暗）
        return BlendDarkerColor(src, dst);
    case 9: // 比較（明）
        return BlendLighten(src, dst);
    case 10: // スクリーン
        return BlendScreen(src, dst);
    case 11: // 覆い焼きカラー
        return dstDodge(src, dst);
    case 12: // 覆い焼（リニア）加算
        return BlendLinearDodge(src, dst);
    case 13: // カラー比較（明）
        return BlendLighterColor(src, dst);
    case 14: // オーバーレイ
        return BlendOverlay(src, dst);
    case 15: // ソフトライト
        return BlendSoftLight(src, dst);
    case 16: // ハードライト
        return BlendHardLight(src, dst);
    case 17: // ビビットライト
        return BlendVividLight(src, dst);
    case 18: // リニアライト
        return BlendLinearLight(src, dst);
    case 19: // ピンライト
        return BlendPinLight(src, dst);
    case 20: // ハードミックス
        return BlendHardMix(src, dst);
    case 21: // 差の絶対値
        return BlendDifference(src, dst);
    case 22: // 減算
        return BlendSubtract(src, dst);
    case 23: // 除算
        return BlendDivide(src, dst);
    case 24: // 除外
        return BlendExclusion(src, dst);
    case 25: // 色相
        return BlendHue(src, dst);
    case 26: // 彩度
        return BlendSaturation(src, dst);
    case 27: // カラー
        return BlendColor(src, dst);
    case 28: // 輝度
        return BlendLuminosity(src, dst);
    default:
        return src;
    }
}
