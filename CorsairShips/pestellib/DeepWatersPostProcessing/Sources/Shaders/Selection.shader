Shader "Hidden/PostProcess/Selection"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SelectionBuffer("Selection", 2D) = "green" {}
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
			sampler2D _SelectionBuffer;
			sampler2D _FillPattern;
			float4 _FillColor;
			float _FillPatternSpeed;
			float _OutlineHardness;

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
				fixed4 source = tex2D(_MainTex, i.uv);
				fixed selection = tex2D(_SelectionBuffer, i.uv).r;
				fixed pattern = tex2D(_FillPattern, (i.uv * _ScreenParams.xy + fixed2(-1, 1) * _Time.z * _FillPatternSpeed) * 0.02 ).a;

				fixed4 fill = fixed4(_FillColor.rgb + pattern * 0.2, _FillColor.a);

				fixed outline = max(0, ((1 - sin(abs(selection - 0.5))) - 0.55) * _OutlineHardness);

				return lerp(source, fill, (selection.r) * fill.a) + outline;
			}
			ENDCG
			}
	}
}
