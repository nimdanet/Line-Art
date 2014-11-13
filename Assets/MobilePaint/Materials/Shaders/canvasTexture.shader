Shader "UnityCoder/CanvasTexture" 
{
	Properties {
		_MainTex ("Canvas (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		Pass {
			Tags { "LightMode" = "Vertex" }
			Lighting Off
			
			SetTexture [_MainTex]
			{
				combine texture
			}

			SetTexture [_MaskTex] 
			{ 
				combine previous*texture 
			}
			

		} // pass
		
	} // subshader
}

