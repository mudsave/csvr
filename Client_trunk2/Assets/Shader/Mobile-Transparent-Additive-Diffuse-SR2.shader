// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Mobile/Transparent/Additive/Diffuse-SR2" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_ScrollX ("Base layer scroll speed X", Float) = 1.0
	_ScrollY ("Base layer scroll speed Y", Float) = 0.0
	_RotationStart("Rotation Start", Float) = 0	
	_RotationX("Base layer rotation center Y", Float) = 0.5	
	_RotationY("Base layer rotation center X", Float) = 0.5
	_Rotation ("Base layer rotation speed", Float) = 1.0
	
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_ScrollX2 ("2nd layer scroll speed X", Float) = 1.0
	_ScrollY2 ("2nd layer scroll speed Y", Float) = 0.0
	_RotationStart2("Rotation Start", Float) = 0	
	_RotationX2("2nd layer rotation center Y", Float) = 0.5	
	_RotationY2("2nd layer rotation center X", Float) = 0.5
	_Rotation2 ("2nd layer rotation speed", Float) = 1.0	
	
	_Color("Color", Color) = (1,1,1,1)
	_MMultiplier ("Multiplier", Float) = 2.0
	
	_Amount ("Amount", Range (0, 1)) = 0.5
    _StartAmount("StartAmount", float) = 0.1
	_Illuminate ("Illuminate", Range (0, 1)) = 1.0
	_Tile("Tile", float) = 1
	_DissColor ("DissColor", Color) = (1,1,1,1)
	_ColorAnimate ("ColorAnimate", vector) = (1,1,1,1)
	_DissolveSrc ("DissolveSrc", 2D) = "white" {}
}
	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	LOD 100
	
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	float4 _MainTex_ST;
	float _ScrollX;
	float _ScrollY;
	float _RotationStart;
	float _RotationX;
	float _RotationY;
	float _Rotation;
	
	sampler2D _DetailTex;
	float4 _DetailTex_ST;
	float _ScrollX2;
	float _ScrollY2;
	float _RotationStart2;
	float _RotationX2;
	float _RotationY2;
	float _Rotation2;
		
	float4 _Color;
	float _MMultiplier;
	
	sampler2D _DissolveSrc;
	half4 _DissColor;
	half _Amount;
	static half3 Color = float3(1,1,1);
	half4 _ColorAnimate;
	half _Illuminate;
	half _Tile;
	half _StartAmount;
	float Clip;
	
	struct v2f {
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;
		fixed4 color : TEXCOORD1;
	};
	
	float2 CalcUV(float2 uv, float tx, float ty, float rstart, float rx, float ry, float r)
	{
		float s = sin(r*_Time+rstart);
		float c = cos(r*_Time+rstart);
		
		float2x2 m = {c, -s, s, c};
		
		uv -= float2(rx, ry);
		uv = mul(uv, m);
		uv += float2(rx, ry);
		
		uv += frac(float2(tx, ty) * _Time);
		
		return uv;
	}
	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		
		// base layer
		o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
		o.uv.xy = CalcUV(o.uv.xy, _ScrollX, _ScrollY, _RotationStart, _RotationX, _RotationY, _Rotation);
		
		// 2nd layer
		o.uv.zw = TRANSFORM_TEX(v.texcoord.xy,_DetailTex);
		o.uv.zw = CalcUV(o.uv.zw, _ScrollX2, _ScrollY2, _RotationStart2, _RotationX2, _RotationY2, _Rotation2);
		
		o.color = _MMultiplier * _Color * v.color;
		
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
//		#pragma fragmentoption ARB_precision_hint_fastest		
		fixed4 frag (v2f i) : COLOR
		{
			float ClipTex = tex2D(_DissolveSrc, i.uv.xy/_Tile).r;	
			float ClipAmount = ClipTex - _Amount;
			Clip = 0;		
			if (_Amount > 0)
			{
				if (ClipAmount <0)
				{
					Clip = 1; //clip(-0.1);			
				}
				else
				{			
					if (ClipAmount < _StartAmount)
					{
						if (_ColorAnimate.x == 0)
							Color.x = _DissColor.x;
						else
							Color.x = ClipAmount/_StartAmount;
						  
						if (_ColorAnimate.y == 0)
							Color.y = _DissColor.y;
						else
							Color.y = ClipAmount/_StartAmount;
						  
						if (_ColorAnimate.z == 0)
							Color.z = _DissColor.z;
						else
							Color.z = ClipAmount/_StartAmount;

						i.color.rgb = (i.color.rgb *((Color.x+Color.y+Color.z))* Color*((Color.x+Color.y+Color.z)))/(1 - _Illuminate);	
						i.color.a = _DissColor.w;
					}
				}
			}
			
			if (Clip == 1)
			{
				clip(-0.1);
			}
			
			return tex2D (_MainTex, i.uv.xy) * tex2D (_DetailTex, i.uv.zw) * i.color;
		}
		ENDCG 
	}	
}
}

