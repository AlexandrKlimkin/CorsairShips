Shader "Mobile/Chest" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Normal ("Nornal (RGB)", 2D) = "grey" {}
		_AO ("AO (RGB)", 2D) = "grey" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
        _Color("_Color", Color) = (1,1,1,1)
        _ColorValue ("ColorValue", Range(0,1)) = 0
        _Transparency ("_Transparency", Range(0,1)) = 0
        _Illum  ("_Illum Color", Color) = (0,0,0,1)
	}
	
	Subshader 
	{
	    Tags {"Queue" = "Transparent+1" "RenderType" = "Transparent"}
            
		Pass 
		{
		    colormask 0 
		}
		Pass 
		{
            zwrite off
            blend SrcAlpha  OneMinusSrcAlpha
			Tags { "LightMode" = "ForwardBase" }
			
			CGPROGRAM
				#pragma exclude_renderers xbox360
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#include "UnityCG.cginc"

				uniform sampler2D _Normal, _MatCap, _MainTex, _AO;
                uniform fixed4 _Color, _Illum;
                
                fixed _ColorValue, _Transparency;
				struct v2f 
				{ 
					float4 pos : SV_POSITION;
				    float2 texcoord : TEXCOORD0;
					float3	TtoV0 : TEXCOORD1;
					float3	TtoV1 : TEXCOORD2;		
				};
								
				v2f vert (appdata_full v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.texcoord = v.texcoord.xy;
					TANGENT_SPACE_ROTATION;
					o.TtoV0 = normalize(mul(rotation, UNITY_MATRIX_IT_MV[0].xyz));
					o.TtoV1 = normalize(mul(rotation, UNITY_MATRIX_IT_MV[1].xyz));
					return o;
				}
				
		
				float4 frag (v2f i) : COLOR
				{
                    float2 ao =  tex2D(_AO, i.texcoord).rg;
					float3 tex = tex2D(_MainTex, i.texcoord) ;
					float3 normTex = tex2D(_Normal, i.texcoord);

					float3 normal = float3(normTex.rg * 2 - 1,1);
					normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
					
					half2 vn;
					vn.x = dot(i.TtoV0, normal);
					vn.y = dot(i.TtoV1, normal);

					float4 matcapLookup = tex2D(_MatCap, vn * 0.5 + 0.5) * ao.r;

					float4 c = float4(tex * matcapLookup.rgb * 2 + matcapLookup.a * normTex.z, _Transparency);
					c.rgb = lerp (c.rgb, c.rgb + _Color.rgb * 2, _ColorValue);
					c.rgb += ao.g * _Illum;
					return  c;

				}
			ENDCG
		}
	}
}