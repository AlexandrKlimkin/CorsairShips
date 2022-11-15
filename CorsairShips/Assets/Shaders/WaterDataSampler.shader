Shader "Environment/Water/WaterDataSampler"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Intensity ("Intensity", Range(0, 1)) = 1
	}
	SubShader
	{
//		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 100
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 color : COLOR;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 tangentWorld : TEXCOORD0;
				float3 normalWorld : TEXCOORD1;
				float3 binormalWorld : TEXCOORD2;
				float4 color : TEXCOORD3;
				float2 uv : TEXCOORD4;
			};

			sampler2D _MainTex;
			half _Intensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.tangentWorld = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.normalWorld = float4(0, 1, 0, 0);
				o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);
				o.color = v.color;
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 result = tex2D(_MainTex, i.uv);

				float3 normal = float3((result.rg - 0.5) * 2, 1);

				float3x3 local2WorldTranspose = float3x3(
					i.tangentWorld,
					i.binormalWorld,
					i.normalWorld);

				normal = mul(normal, local2WorldTranspose);	
				//
				// result.rg = (normal.rb + 1) / 2;
				result.a *= i.color.a * _Intensity;
				result.rgb = 1;
				return result;
			}
			ENDCG
		}
	}
}
