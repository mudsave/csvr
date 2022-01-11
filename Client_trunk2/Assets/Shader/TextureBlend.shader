Shader "Custom/TextureBlend"   
 {  
     Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlendTex ("Alpha Blended (RGBA) ", 2D) = "white" {}
    }
    SubShader {
        Pass {
            // Apply base texture
                        // 应用主纹理
            SetTexture [_MainTex] {
                combine texture
            }
            // Blend in the alpha texture using the lerp operator
                        // 使用差值操作混合Alpha纹理
            SetTexture [_BlendTex] {
                combine texture lerp (texture) previous
            }
        }
    }
 } 