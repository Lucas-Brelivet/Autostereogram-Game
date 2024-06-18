Shader "Unlit/Depth"
{
    SubShader
    {
        LOD 100
        Tags{"RenderType"="Opaque"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "ShaderUtils.cginc"

            float _MaxDepthValue;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float depth : DEPTH;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth = mul(UNITY_MATRIX_MV, v.vertex).z/_MaxDepthValue;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return EncodeFloatRGBA(abs(i.depth));
            }
            ENDCG
        }
    }
}
