Shader "Hidden/PostProcess/ColorCorrection"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma multi_compile ABERRATION_OFF ABERRATION_ON
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			static const float2 RDir = float2(0, -1);
			static const float2 GDir = float2(-0.87, 0.5);
			static const float2 BDir = float2(0.87, 0.5);

			sampler2D _MainTex;
			sampler2D _LUT;
			half3 _LUTParams;
			half _Aberration;
			half _Desaturation;			
			/*half _VignetteAmmount;
			half _VignettePower;*/

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
#ifdef ABERRATION_ON
				float noise : TEXCOORD1;
#endif
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

#ifdef ABERRATION_ON
				float factor = (_Time.x) * 500;
				float noise = 0.4 * sin(0.22 * factor) + 0.3 * sin(0.53 * factor) + 0.3 * sin(1.25 * factor);
				o.noise = noise;
#endif
				return o;
			}

			half3 ApplyLUT(sampler2D tex, half3 uvw, half3 scaleOffset)
			{
				uvw.z *= scaleOffset.z;
				half shift = floor(uvw.z);
				uvw.xy = uvw.xy * scaleOffset.z * scaleOffset.xy + scaleOffset.xy * 0.5;
				uvw.x += shift * scaleOffset.y;
				uvw.xyz = lerp(tex2D(tex, uvw.xy).rgb, tex2D(tex, uvw.xy + half2(scaleOffset.y, 0)).rgb, uvw.z - shift);
				return uvw;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//fixed2 coord = (i.uv - 0.5) * 2 * _VignetteAmmount;
				//fixed rf = ((dot(coord, coord)) + 1.0);
				//fixed vignette = (1.0 / (rf * rf * rf));
				//vignette = (1 - vignette) * _VignettePower;
				//vignette = (1 - vignette);

#ifdef ABERRATION_OFF
				fixed4 col = tex2D(_MainTex, i.uv);			
#endif

#ifdef ABERRATION_ON
				fixed normalizedNoise = (i.noise + 1) * 0.5;
				fixed aberrationAmmount = normalizedNoise * _Aberration * 0.015f;

				fixed3 rCol = tex2D(_MainTex, i.uv + RDir * aberrationAmmount).rgb;
				fixed3 gCol = tex2D(_MainTex, i.uv + GDir * aberrationAmmount).rgb;
				fixed3 bCol = tex2D(_MainTex, i.uv + BDir * aberrationAmmount).rgb;

				fixed4 col = fixed4(rCol.r, gCol.g, bCol.b, 1);
#endif
				col.rgb = ApplyLUT(_LUT, saturate(col.rgb), _LUTParams);
				col.rgb = lerp(col.rgb, Luminance(col.rgb), _Desaturation);
				col.a = 1;
				return col;
			}
			ENDCG
		}
	}
}
