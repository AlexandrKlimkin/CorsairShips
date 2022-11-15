Shader "Hidden/PostProcess/GaussianBlur"
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
			float4 _MainTex_TexelSize;

			int _Iterations;
			float _Weight[32];
            float _Offset[32];

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
				fixed4 col = tex2D(_MainTex, i.uv) * _Weight[0];
				for (int p = 1; p < _Iterations + 1; p++) {
					col += tex2D(_MainTex, i.uv + float2(0, _Offset[p] * _MainTex_TexelSize.y)) * _Weight[p];
					col += tex2D(_MainTex, i.uv - float2(0, _Offset[p] * _MainTex_TexelSize.y)) * _Weight[p];
				}
				return col;
			}
			ENDCG
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_TexelSize;

                int _Iterations;
                float _Weight[32];
                float _Offset[32];

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

				fixed4 frag(v2f i) : SV_Target
				{
                    fixed4 col = tex2D (_MainTex, i.uv) * _Weight[0];
                    for (int p = 1; p < _Iterations + 1; p++) {
                        col += tex2D (_MainTex, i.uv + float2(_Offset[p] * _MainTex_TexelSize.x, 0)) * _Weight[p];
                        col += tex2D (_MainTex, i.uv - float2(_Offset[p] * _MainTex_TexelSize.x, 0)) * _Weight[p];
                    }
                    return col;
				}
				ENDCG
				}
	}
}
