Shader "KST/Kst_DissolveAdd_TexturCoords" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
		_Amount ("Amount", Range (0, 1)) = 0.5
		_StartAmount("StartAmount", float) = 0.1
		_Illuminate ("Illuminate", Range (0, 1)) = 0.5
		_Tile("Tile", float) = 1
		_EdgeFactor ("EdgeFactor", Range (0.1, 0.5)) = 0.2	 
		_DissColor ("DissColor", Color) = (1,1,1,1)
		_ColorAnimate ("ColorAnimate", vector) = (1,1,1,1)
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_DissolveSrc ("DissolveSrc", 2D) = "white" {}
	}
	
	SubShader { 	
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Cull Off
		ZWrite Off
		Lighting off
		Blend SrcAlpha One
		
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf BlinnPhong addshadow


		sampler2D _MainTex;
		sampler2D _DissolveSrc;

		fixed4 _Color;
		half4 _DissColor;
		half _Shininess;
		half _Amount;
		static half3 Color = float3(1,1,1);
		half4 _ColorAnimate;
		half _Illuminate;
		half _Tile;
		half _StartAmount;
		float _EdgeFactor;


		struct Input {
			float2 uv_MainTex;
			float2 uvDissolveSrc;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb * 2;
			
			float ClipTex = tex2D (_DissolveSrc, IN.uv_MainTex/_Tile).r ;
			float ClipAmount = ClipTex - _Amount;
			o.Alpha = tex.a * _Color.a;
			if (_Amount!=0 && _Amount < 1.0)
			{
				o.Alpha = clamp(o.Alpha, 0, 1 - -ClipAmount / _EdgeFactor) * o.Alpha * 2;
				
				if (ClipAmount >=0)
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

						o.Albedo  = (o.Albedo *((Color.x+Color.y+Color.z))* Color*((Color.x+Color.y+Color.z)))/(1 - _Illuminate);								
					}
				}
				
				o.Gloss = tex.a;
			}
			
			if(_Amount == 0)
			{
				o.Gloss = tex.a;
			}
			
			if(_Amount == 1)
			{
				o.Alpha = 0;
				o.Gloss = 0;
			}

			o.Specular = _Shininess;				
		}
		ENDCG
	}

	FallBack "Specular"
}
