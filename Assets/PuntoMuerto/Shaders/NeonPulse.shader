Shader "PuntoMuerto/NeonPulse"
{
    Properties
    {
        _Color ("Neon Color", Color) = (1, 0.4, 0.1, 1)
        _Intensity ("Intensity", Float) = 2.5
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseAmount ("Pulse Amount", Range(0,1)) = 0.15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _PulseSpeed;
                half _PulseAmount;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // pulso suave + parpadeo ocasional estilo letrero viejo
                half pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                half flicker = step(0.97, frac(sin(floor(_Time.y * 7.0) * 12.9898) * 43758.5453)) * 0.5;
                half4 c = _Color * _Intensity * pulse * (1.0 - flicker);
                c.a = 1.0;
                return c;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
