Shader "Hidden/PostProcess/Bloom"
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
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _BlurTex;
			half _Intensity;
			half _Threshold;

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
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 bloom = tex2D(_BlurTex, i.uv);
				fixed factor = max(bloom.r, max(bloom.g, bloom.b));
				factor = saturate(((factor - _Threshold) / _Threshold) * _Intensity);
				factor = factor * bloom.a;
				col.rgb += bloom.rgb * factor;			
				//return fixed4(bloom.rgb * factor, factor);
				//return factor;
				return col;
			}
			ENDCG
		}
	}
}
