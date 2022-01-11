Shader "Particles/Additive_Dissolve_TexturCoords" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	
	_Amount ("Amount", Range (0, 1)) = 0.5
    _StartAmount("StartAmount", float) = 0.1
	_Illuminate ("Illuminate", Range (0, 1)) = 1.0
	_Tile("Tile", float) = 1
	_DissColor ("DissColor", Color) = (1,1,1,1)
	_ColorAnimate ("ColorAnimate", vector) = (1,1,1,1)
	_DissolveSrc ("DissolveSrc", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend SrcAlpha One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_particles
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			
			sampler2D _DissolveSrc;
			half4 _DissColor;
			half _Amount;
			static half3 Color = float3(1,1,1);
			half4 _ColorAnimate;
			half _Illuminate;
			half _Tile;
			half _StartAmount;
			float Clip;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
				#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : SV_Target
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				#endif
				
				float ClipTex = tex2D(_DissolveSrc, i.texcoord/_Tile).r;	
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
				
				fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
								
				return col;
			}
			ENDCG 
		}
	}	
}
}
