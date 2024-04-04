Shader "Unlit/FindMinDepth"
{
    //This shader takes a depth texture and finds the minimal depth value on that texture
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float min(float x, float y)
            {
                return x < y ? x : y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float minDepth = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y)/2);
                float depth = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y)/2);
                minDepth = min(minDepth, depth);
                depth = tex2D(_MainTex, i.uv + float2( -_MainTex_TexelSize.x, -_MainTex_TexelSize.y)/2);
                minDepth = min(minDepth, depth);
                depth = tex2D(_MainTex, i.uv + float2( -_MainTex_TexelSize.x, _MainTex_TexelSize.y)/2);
                minDepth = min(minDepth, depth);
                return minDepth;
            }
            ENDCG
        }
    }
}
