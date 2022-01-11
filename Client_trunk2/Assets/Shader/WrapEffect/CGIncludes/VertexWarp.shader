Shader "CSFS/VertexWarp" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_StrengthRadius("StrengthRadius", Vector) = (0.0, 0.0, 0.0, 0.0)
	_Size("Size", Range(0.0, 1.0)) = 1.0
}
SubShader {
	Tags {  "Queue" = "Geometry+501" "RenderType"="Opaque" }
	LOD 200

CGPROGRAM
#pragma surface surf Lambert vertex:vert
#include "VertexWarp.cginc"


sampler2D _MainTex;
fixed4 _Color;
half4 _StrengthRadius;
VertexWarp warp;
uniform half4x4 vTransform;
uniform half4x4 vInvTransform;
float _Size;

struct Input {
	float2 uv_MainTex;
};

void vert(inout appdata_full v)    
{  
	warp.vTransform = vTransform;
	warp.vInvTransform = vInvTransform;
	warp.vStrengthRadius = _StrengthRadius;
	warp.Size = _Size;
	float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
	float4 outPos = ApplyWarp( worldPos, warp);
	v.vertex = mul(unity_WorldToObject, outPos);
} 

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "VertexLit"
}
