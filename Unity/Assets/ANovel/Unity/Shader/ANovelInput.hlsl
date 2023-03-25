
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _BackTex;
float4 _BackTex_ST;

float4 _Color;
float4 _BackColor;
float _TransValue;

#ifdef _ANOVEL_TRANS_RULE
sampler2D _RuleTex;
float4 _RuleTex_ST;
float _RuleVague;
int _RuleAlpha;
#endif

#ifdef _ANOVEL_ENV_COLOR
float4 _EnvColor;
#endif

#ifdef _ANOVEL_ENV_COLOR_TEX
sampler2D _EnvColorTex;
float4 _EnvColorTex_ST;
#endif

#if defined(_ANOVEL_ENV_COLOR) || defined(_ANOVEL_ENV_COLOR_TEX)
int _EnvColorBlendMode;
#endif
