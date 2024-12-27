//NOTE: This shader does not work properly with the skybox in the game view in edit mode
//      Use "solid color" instead of skybox in your camera(s).

Shader "VisionLib/Transparent with Z Pass"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _LightDir("Light Direction", Vector) = (-1,0.5,0.5,1)
    }

    SubShader
    {
        //Shared Shader Code
        HLSLINCLUDE
        #include "UnityCG.cginc"
        CBUFFER_START(settings)
        half4 _Color;
        float4 _LightDir;
        CBUFFER_END
        

        struct VertInputs
        {
            float4 positionOS : POSITION;
            float3 normal : NORMAL;
        };

        struct VertOutputs
        {
            float4 position : SV_POSITION;
            float3 normal : TEXCOORD1;
        };

        VertOutputs shared_vert(VertInputs inVert)
        {
            VertOutputs outVert;
            outVert.position = UnityObjectToClipPos(inVert.positionOS.xyz);
            outVert.normal = mul(unity_ObjectToWorld, float4(inVert.normal.xyz, 1.0));
            return outVert;
        }

        half4 shared_frag(VertOutputs i, half4 color, half4 lightDir) : SV_Target
        {
            // Simple lambert lighting calculation
            float intensity = dot(lightDir, i.normal);
            // Clamp light intensity from -1:1 to 0.15:0.95 to avoid clipping out any detail on the opposing side of the light
            // These values were chosen arbitrarily to show enough detail in both dark and light areas
            intensity = intensity / 2.5 + 0.6;
            return color * half4(intensity, intensity, intensity, 1);
        }
        ENDHLSL

        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"
        }

        // Render backfaces. Could be disabled if the models are appropriate
        Cull Off
        // "Pre-Z" depth pass
        // This is to avoid rendering faces behind each other. In this pass the depth buffer is set to the face in front,
        // in the next pass only the face with exactly this depth is rendered
        Pass
        {
            ZWrite On
            // Only write depth, no color
            ColorMask 0
        }
        // Color pass for the Built-In RP. 
        // This pass is ignored by the URP because it has no "Tags". 
        // It is therefore assigned the default tag by URP, which is the same tag already assigned to the previous
        // (depth) pass. Since only the first pass with a given tag is actually executed by URP, this pass is skipped.
        Pass
        {
            // Only render fragments with the exact depth determined in the depth prepass
            ZTest Equal
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Vertex shader
            VertOutputs vert(VertInputs inVert)
            {
                return shared_vert(inVert);
            }

            // Fragment shader
            half4 frag(VertOutputs i) : SV_Target
            {
                return shared_frag(i, _Color, _LightDir);
            }
            ENDHLSL
        }
        // Color pass for the Universal RP.
        // This pass is ignored by the Built-In RP because it uses the tag "UniversalForwardOnly".
        // This tag is not known to the Built-In RP & the pass is hence skipped.
        Pass
        {
            Tags
            {
                "LightMode"="UniversalForwardOnly"
            }
            // Only render fragments with the exact depth determined in the depth prepass
            ZTest Equal
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Vertex shader
            VertOutputs vert(VertInputs inVert)
            {
                return shared_vert(inVert);
            }

            // Fragment shader
            half4 frag(VertOutputs i) : SV_Target
            {
                return shared_frag(i, _Color, _LightDir);
            }
            ENDHLSL
        }

    }
}