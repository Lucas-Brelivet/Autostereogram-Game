Shader "Unlit/Noise"
{
    Properties
    {
        _MainTex ("Pattern", 2D) = "white" {}
        _RandomSeed ("Random Seed", int) = 0
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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            uint _RandomSeed;

            uint hashi(uint x)
            {
                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;
                return x;
            }

            float hash(uint x)
            {
                return float( hashi(x) ) / float( 0xffffffffU );
            }

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                uint2 uvInt = uint2(i.uv * _MainTex_TexelSize.zw);
                fixed4 col = hash(uvInt.x + hashi(uvInt.y) + _RandomSeed);
                return col;
            }
            ENDCG
        }
    }
}
