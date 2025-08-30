Shader "Custom/WorldSizeTexture"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        [Space]
        [Space]
        [MainTexture] _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DetailTex ("Detail Texture", 2D) = "black" {}
        _DetailAlpha ("Detail Alpha", Range(0,1)) = 1
        _PatternSize ("Patterns size (main U, main V, details U, details V)", Vector) = (1,1,1,1)
        [Normal] _NormalMap ("Normalmap", 2D) = "bump" {}
        _NormalScale("Normal Scale", Float) = 1
        [Space]
        [Space]
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _DetailTex;
        sampler2D _NormalMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _NormalScale;
        float4 _PatternSize;
        float _PatternUSize;
        float _PatternVSize;
        float _DetailAlpha;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        float normalizedPositiveModulo(float numerator, float modulo)
        {
            float result = fmod(numerator, modulo) / modulo;
            result = result < 0 ? 1+result : result;
            return result;
        }

        float normalizedPingPong(float value, float halfPeriod)
        {
            return 1 - abs(normalizedPositiveModulo(value, halfPeriod * 2)*2 - 1);
        }

        float3 getObjectScale()
        {
            float3x1 scale;
            for (int i = 0; i < 3; i++)
            {
                float4x1 extractor = {0,0,0,0};
                extractor[i][0] = 1;
                scale[i][0] = length(float4(mul(unity_ObjectToWorld, extractor)).xyz);
            }
            return scale;
        }

        void vert(inout appdata_full v)
        {
            //project the position on the tangent plane
            float3 scale = getObjectScale();
            float3 unscaledPosition = v.vertex.xyz/v.vertex.w * scale;

            float3 tangent = normalize(v.tangent.xyz);
            //v.tangent.w is 1 or -1 and gives the handedness of the tangent coordinate system
            float3 bitangent = cross(normalize(v.normal), tangent) * v.tangent.w;

            float uPosition = dot(unscaledPosition, tangent);
            float vPosition = dot(unscaledPosition, bitangent);

            v.texcoord = float4(uPosition, vPosition,0,1);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv_MainTex = float2(normalizedPositiveModulo(IN.uv_MainTex.x, _PatternSize.x), normalizedPositiveModulo(IN.uv_MainTex.y, _PatternSize.y));
            // determine the derivatives using continuous UVs to avoid mipmapping artifacts
            float2 uv_Derivatives = float2(normalizedPingPong(IN.uv_MainTex.x, _PatternSize.x), normalizedPingPong(IN.uv_MainTex.y, _PatternSize.y));
            float2 _ddx = ddx(uv_Derivatives);
            float2 _ddy = ddy(uv_Derivatives);

            fixed4 c =  tex2Dgrad (_MainTex, uv_MainTex, _ddx, _ddy) * _Color;

            float2 uv_DetailTex = float2(normalizedPositiveModulo(IN.uv_MainTex.x, _PatternSize.z), normalizedPositiveModulo(IN.uv_MainTex.y, _PatternSize.w));
            // determine the derivatives using continuous UVs to avoid mipmapping artifacts
            float2 uv_DetailDerivatives = float2(normalizedPingPong(IN.uv_MainTex.x, _PatternSize.z), normalizedPingPong(IN.uv_MainTex.y, _PatternSize.w));
            float2 _detailDdx = ddx(uv_DetailDerivatives);
            float2 _detailDdy = ddy(uv_DetailDerivatives);

            fixed4 detailColor = tex2Dgrad (_DetailTex, uv_DetailTex, _detailDdx, _detailDdy) * _Color;
            
            float detailAlpha = detailColor.a * _DetailAlpha;

            //alpha blend the details with the main texture
            o.Alpha = detailAlpha + c.a * (1-detailAlpha);
            o.Albedo = (detailColor.rgb * detailAlpha + c.rgb * c.a * (1-detailAlpha))/o.Alpha;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            o.Normal = UnpackNormal(tex2Dgrad (_NormalMap, uv_MainTex, _ddx, _ddy));
            o.Normal.xy *= _NormalScale;
            o.Normal = normalize(o.Normal);
        }

        ENDCG
    }
    FallBack "Diffuse"
}
