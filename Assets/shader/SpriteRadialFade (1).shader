Shader "Custom/SpriteRadialFade"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FadeProgress ("Fade Progress", Range(0,1)) = 1
        _FadeSoftness ("Fade Softness", Range(0.01,1.0)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _FadeProgress;
            float _FadeSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Calculate distance from center (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Max distance from center to corner is ~0.707
                // Normalize to 0-1 range
                float normalizedDist = dist / 0.707;
                
                // Calculate fade based on progress
                // When progress = 0, nothing visible
                // When progress = 1, everything visible
                float fadeEdge = _FadeProgress * 1.5;
                float alpha = smoothstep(fadeEdge, fadeEdge - _FadeSoftness, normalizedDist);
                
                // Only modify alpha, don't touch RGB
                col.a *= alpha;
                
                // Premultiply alpha for proper blending
                col.rgb *= col.a;
                
                // Clamp to prevent weird edge colors
                col = saturate(col);
                
                return col;
            }
            ENDCG
        }
    }
}
