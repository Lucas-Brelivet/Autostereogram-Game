Shader "Unlit/Distance"
{
    Properties
    {
        _MinDepthValue("Min Depth Value", float) = 1
    }
    SubShader
    {
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "ShaderUtils.cginc"

            float _MinDepthValue;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float distance : DISTANCE;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 viewSpacePosition = UnityObjectToViewPos(v.vertex);
                o.distance = map(length(mul(UNITY_MATRIX_MV, v.vertex)), _MinDepthValue, _ProjectionParams.z, 0, 1);
                if(o.distance < 0)
                {
                    o.distance = 0;
                }
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.distance;
                return col;
            }
            ENDCG
        }
    }
}
