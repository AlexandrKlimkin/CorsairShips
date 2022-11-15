Shader "Environment/Water/FoamDataSampler"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Intensity ("Intensity", Range(0, 2)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 100
		ZWrite Off
		ZTest Always
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR0;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;
				float2 uv : TEXCOORD1;
			};

			sampler2D _MainTex;
			half _Intensity;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 result = tex2D(_MainTex, i.uv);
				return result * i.color * _Intensity;
			}
			ENDCG
		}
	}
}
