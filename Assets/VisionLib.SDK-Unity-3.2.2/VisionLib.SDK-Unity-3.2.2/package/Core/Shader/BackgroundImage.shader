Shader "VisionLib/BackgroundImage" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "black" {}
    }
    SubShader {
        Tags { "Queue" = "Transparent" }
        Pass {
            Cull Off
            ZTest Always
            ZWrite Off
            ColorMask RGBA // Write to all channels

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc" // Defines vert_img and v2f_img

            fixed4 _Color;
            sampler2D _MainTex;

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _Color;
            }

            ENDCG
        }
    }
}