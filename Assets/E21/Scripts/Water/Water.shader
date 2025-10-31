Shader "Custom/Water" {
    Properties {
        _Color ("Water Color", Color) = (0.2, 0.5, 0.8, 0.7)
        _DeepColor ("Deep Water Color", Color) = (0.1, 0.3, 0.5, 0.9)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveHeight ("Wave Height", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 5.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.95
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _FresnelPower ("Fresnel Power", Range(0,5)) = 2.0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard alpha:fade vertex:vert
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
        };

        fixed4 _Color;
        fixed4 _DeepColor;
        float _WaveSpeed;
        float _WaveHeight;
        float _WaveFrequency;
        half _Glossiness;
        half _Metallic;
        float _FresnelPower;

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            // Vertex wave animation
            float wave1 = sin(v.vertex.x * _WaveFrequency + _Time.y * _WaveSpeed);
            float wave2 = cos(v.vertex.z * _WaveFrequency + _Time.y * _WaveSpeed * 0.8);
            
            v.vertex.y += (wave1 + wave2) * _WaveHeight * 0.5;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Fresnel effect (more transparent when looking straight down)
            float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), _FresnelPower);
            
            // Mix colors based on fresnel
            fixed4 c = lerp(_DeepColor, _Color, fresnel);
            
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}