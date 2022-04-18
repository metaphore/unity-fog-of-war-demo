Shader "Fow/FowFxChain"
{
    Properties
    {
        _MainTex ("Tilemap Texture", 2D) = "white" {}
        _MaskLut ("Mask Lut Texture", 2D) = "white" {}
        
        _MaskThreshold ("Mask Threshold", Range(0, 1)) = 0.5
        
        _FogTimeFactor ("Fog Time Factor", Float) = 100.0
        _FogScaleFactor ("Fog Scale Factor", Float) = 16.0
        _FogContrast ("Fog Contrast", Float) = 3.0
        
        _SmoothPixels ("Smooth Pixels", Float) = 8.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

		// No culling or depth
		Cull Off 
//		ZWrite Off 
		ZTest Always
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.vertex = UnityPixelSnap(o.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
            
            sampler2D _MaskLut;

            fixed _MaskThreshold;
            
            float _FogTimeFactor;
            float _FogScaleFactor;
            fixed _FogContrast;

            fixed _SmoothPixels;

            float4 _CamRect;
            float4 _TextureSize;

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            // Based on Morgan McGuire @morgan3d
            // https://www.shadertoy.com/view/4dS3Wd
            float noise (float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                        (c - a)* u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }

            #define OCTAVES 6
            float fbm (float2 st) {
                // Initial values
                float value = 0.0;
                float amplitude = .5;

                // Loop of octaves
                for (int i = 0; i < OCTAVES; i++) {
                    value += amplitude * noise(st);
                    st *= 2.;
                    amplitude *= .5;
                }
                return value;
            }

            fixed computeMask(float2 uv)
            {
                const half ghostShift = _SmoothPixels;
                const fixed ghostMix = 0.25;
                half shiftX = _TextureSize.x * ghostShift;
                half shiftY = _TextureSize.y * ghostShift;

                fixed maskVal = tex2D(_MainTex, uv).r;
                maskVal += tex2D(_MainTex, uv + fixed2(+shiftX, +shiftY)).r * ghostMix;
                maskVal += tex2D(_MainTex, uv + fixed2(-shiftX, +shiftY)).r * ghostMix;
                maskVal += tex2D(_MainTex, uv + fixed2(+shiftX, -shiftY)).r * ghostMix;
                maskVal += tex2D(_MainTex, uv + fixed2(-shiftX, -shiftY)).r * ghostMix;

                return maskVal;
            }

            fixed4 applyLut(sampler2D lutTexture, fixed2 position)
            {
                return tex2D(lutTexture, position).rgba;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixelWorldPos = _CamRect.xy - _CamRect.zw * 0.5 + _CamRect.zw * i.uv;

                float2 st = pixelWorldPos * _FogScaleFactor;
                // st.x *= _MainTex_TexelSize.y / _MainTex_TexelSize.x;
                
                float time = _Time.x * _FogTimeFactor;
                
                fixed2 r = float2(0.0, 0.0);
                r.x = fbm(st + fixed2(1.7,9.2) + 0.15 * time);
                r.y = fbm(st + fixed2(8.3,2.8) + 0.126 * time);
                
                float noise = fbm(st + r);
                
                fixed alpha = computeMask(i.uv);
                
                noise = saturate(pow(0.5f + noise, _FogContrast) - 0.5f);
                fixed maskAlpha = lerp(noise * alpha, alpha, step(0.99, alpha));
                fixed maskAlphaHard = step(_MaskThreshold, maskAlpha);
                
                fixed4 fogColor = applyLut(_MaskLut, fixed2(maskAlpha, 0.0));
                fogColor.a *= maskAlphaHard;

                // Pre-multiplied alpha.
                fogColor.rgb *= fogColor.a;
                return fogColor;
            }
            ENDCG
        }
    }
}
