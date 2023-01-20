Shader "ARMuseum/BoxClipShader" {
    Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        [Toggle(_False)]_Emission("Emission", float) = 0
        [HDR] _EmissionColor("Emission_Color", Color) = (0,0,0)
    }


        SubShader{
            Tags { "RenderType" = "Transparent" }
            LOD 200
            Cull Off

            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows addshadow
            #pragma target 3.0

            sampler2D _MainTex;
            half _Glossiness;
            half _Metallic;
            float4x4 _WorldToBox;
            float _Emission;
            fixed4 _EmissionColor;

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
            };

            void surf(Input IN, inout SurfaceOutputStandard o) {
                float3 boxPosition = mul(_WorldToBox, float4(IN.worldPos, 1));
                clip(boxPosition + 0.5);
                clip(0.5 - boxPosition);

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Emission = c.rgb * tex2D(_MainTex, IN.uv_MainTex).a * _EmissionColor * _Emission;
                o.Alpha = c.a;
            }
            ENDCG
        }


            FallBack "Diffuse"
}