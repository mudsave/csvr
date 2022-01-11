// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CloudEffect" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		Tags {"Queue"="Geometry +500" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off 
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 500
		pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			struct v2f {
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			} ;

			v2f vert (appdata_base v)
			{
    				v2f o;
   				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
    				return o;
			}
			float4 frag (v2f i) : COLOR
			{
				float4 texCol = tex2D(_MainTex,i.uv) * _Color;
    				float4 outp = texCol;
    				return outp;
			}
			ENDCG
			}
		}
	}