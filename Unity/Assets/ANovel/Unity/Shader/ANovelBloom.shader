Shader "ANovel/Bloom"
{
    Properties
    {
        _MainTex("Main", 2D) = "black" {}
        _SamplingDistance("Sampling Distance", float) = 1.0
        _BrightnessMax("BrightnessMax", float) = 1
        _Threshold("Threshold", float) = 0.9
        _SoftKnee("SoftKnee", float) = 0.1
        _Intensity("Intensity", float) = 0.1
        _Distance("Distance", float) = 1
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
            Name "ANovel Bloom Prefilter"
        HLSLPROGRAM
            #pragma target 2.0
            #include "ANovelBloom.hlsl"
            #pragma vertex vert
            #pragma fragment frag_prefilter
        ENDHLSL
        }
        Pass
        {
            Name "ANovel Bloom Downsample H"
        HLSLPROGRAM
            #pragma target 2.0
            #include "ANovelBloom.hlsl"
            #pragma vertex vert
            #pragma fragment frag_downsample_h
        ENDHLSL
        }
        Pass
        {
            Name "ANovel Bloom Downsample V"
        HLSLPROGRAM
            #pragma target 2.0
            #include "ANovelBloom.hlsl"
            #pragma vertex vert
            #pragma fragment frag_downsample_v
        ENDHLSL
        }
        Pass
        {
            Name "ANovel Bloom Upsample"
        HLSLPROGRAM
            #pragma target 2.0
            #include "ANovelBloom.hlsl"
            #pragma vertex vert
            #pragma fragment frag_upsample
        ENDHLSL
        }
        Pass
        {
            Name "ANovel Bloom Finish"
        HLSLPROGRAM
            #pragma target 2.0
            #include "ANovelBloom.hlsl"
            #pragma vertex vert
            #pragma fragment frag_finish
        ENDHLSL
        }
    }
}
