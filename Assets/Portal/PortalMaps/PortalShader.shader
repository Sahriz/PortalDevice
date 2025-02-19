Shader "Custom/PortalShader"
{
    Properties
    {
        
        _PortalTex ("Portal Texture", 2D) = "white" { }
        _PortalDepthOffset ("Portal Depth Offset", float) = 0.001
       
    }
    SubShader
    {
        
        Tags { "RenderType"="Opaque" }
        Cull Back

        Pass
        {
            
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            

            // Texture properties

            sampler2D _PortalTex;
            float4 _PortalCorners; // Four corner coordinates of the portal in texture space



            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 screenPos : TEXCOORD0;
                float4 vertex : SV_POSITION;

            };
          

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            

            half4 frag(v2f i) : SV_Target
            {
                
                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;

                return tex2D(_PortalTex, screenSpaceUV);

            }
           
            ENDCG
        }
    }
    FallBack "Diffuse"
}