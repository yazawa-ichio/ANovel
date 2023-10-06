Shader "ANovel/ColorEffect"
{
    Properties
    {
        _MainTex("Main", 2D) = "black" {}
        _Rate("Rate", Range(0, 1)) = 1
        _HSV("HSV", Vector) = (0, 1, 1, 0)
        [KeywordEnum(GRAYSCALE, SEPIA, NEGATIVE, HSV)]_ANOVEL_COLOR_CHANGE("ColorEffect", int) = 0
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
            #pragma vertex ColorEffectVert
            #pragma fragment ColorEffectFrag
            #pragma target 2.0
            #include "ANovelColorEffect.hlsl"
        ENDHLSL
        }
    }
}
