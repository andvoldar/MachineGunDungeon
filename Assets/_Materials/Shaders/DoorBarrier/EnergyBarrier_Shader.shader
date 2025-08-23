Shader "Custom/EnergyBarrier"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {} // requerido por SpriteRenderer
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _MainColor("Main Color", Color) = (0.0, 1.0, 1.0, 1.0)
        _EdgeColor("Edge Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Speed("Speed", Float) = 1.0
        _EdgeWidth("Edge Width", Float) = 0.1
        _Alpha("Alpha", Float) = 0.8
    }

        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                sampler2D _NoiseTex;
                float4 _MainColor;
                float4 _EdgeColor;
                float _Speed;
                float _EdgeWidth;
                float _Alpha;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    float time = _Time.y * _Speed;
                    float2 uv = i.uv + float2(0, time);
                    float noise = tex2D(_NoiseTex, uv).r;

                    float distToCenter = abs(i.uv.x - 0.5);
                    float edge = smoothstep(0.5 - _EdgeWidth, 0.5, distToCenter);

                    float4 color = lerp(_MainColor, _EdgeColor, edge);
                    color.a *= (1.0 - edge) * noise * _Alpha;

                    return color;
                }
                ENDCG
            }
        }
}
