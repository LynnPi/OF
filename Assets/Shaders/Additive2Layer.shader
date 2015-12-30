
Shader "OF/Effect/Additive2Layer" 
{
    Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex ("Alpha (RGB)", 2D) = "white" {}
    }

    SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		
		Blend SrcAlpha One
		AlphaTest Greater .01
		ColorMask RGB		
		Cull Off 
		Lighting Off 
		ZWrite Off
		Fog { Color (0,0,0,0) }
		
		BindChannels 
		{
			Bind "Vertex", vertex				
			Bind "TexCoord", texcoord
			Bind "Color", color
		}
	
        Pass 
		{					
            SetTexture [_MainTex] 
			{
				constantColor [_TintColor]
                combine constant * primary DOUBLE              
            }	
            
			SetTexture [_MainTex] 
			{
				combine texture * previous
			}     

			SetTexture [_AlphaTex] 
			{
				constantColor [_TintColor]
				combine texture * previous, constant * texture
			}   
        }
    }
} 