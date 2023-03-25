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
        _EnvColor("EnvColor", Color) = (0, 0, 0, 0)
        _EnvColorTex("EnvColorTex", 2D) = "black" {}
        [Enum(ANovel.BlendMode)]_EnvColorBlendMode("EnvColorBlendMode", int) = 0
        [Toggle] _RuleAlpha("RuleUseAlpha", int) = 0
        [KeywordEnum(DEF,RULE)]_ANOVEL_TRANS("Trans", int) = 0
        [KeywordEnum(DEF,COLOR,COLOR_TEX)]_ANOVEL_ENV("EnvColorSource", int) = 0
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
        HLSLPROGRAM
            #pragma vertex DefaultVert
            #pragma fragment DefaultFrag
            #pragma target 2.0
            #pragma multi_compile_local _ _ANOVEL_TRANS_RULE
            #pragma multi_compile_local _ _ANOVEL_ENV_COLOR _ANOVEL_ENV_COLOR_TEX

            #include "UnityCG.cginc"
            #include "ANovelInput.hlsl"
            #include "ANovelPasses.hlsl"

        ENDHLSL
        }
    }
}
