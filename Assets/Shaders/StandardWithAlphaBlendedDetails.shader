Shader "Custom/StandardWithAlphaBlendedDetails"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        [Space]
        [Space]
        [MainTexture] _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DetailTex ("Detail Texture", 2D) = "black" {}
        _DetailAlpha ("Detail Alpha", Range(0,1)) = 1
        [Space]
        [Space]
        [Normal] _NormalMap ("Normalmap", 2D) = "bump" {}
        _NormalScale("Normal Scale", Float) = 1
        [Space]
        [Space]
        [Emission] _EmissionMap ("Emission", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _DetailTex;
        sampler2D _NormalMap;
        sampler2D _EmissionMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        float _Smoothness;
        float _Metallic;
        fixed4 _Color;
        float _DetailAlpha;
        float _NormalScale;
        fixed4 _EmissionColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a main texture alpha blended with a detail teaxture and tinted by color
            fixed4 mainColor = tex2D (_MainTex, IN.uv_MainTex);

            fixed4 detailColor = tex2D (_DetailTex, IN.uv_MainTex);

            float detailAlpha = detailColor.a * _DetailAlpha;

            //alpha blend the details with the main texture
            o.Alpha = (detailAlpha + mainColor.a * (1-detailAlpha)) * _Color.a;
            o.Albedo = (detailColor.rgb * detailAlpha + mainColor.rgb * mainColor.a * (1-detailAlpha))/o.Alpha * _Color;
        

            o.Normal = UnpackNormal(tex2D (_NormalMap, IN.uv_MainTex));
            o.Normal.xy *= _NormalScale;
            o.Normal = normalize(o.Normal);

            o.Emission = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor;

            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
