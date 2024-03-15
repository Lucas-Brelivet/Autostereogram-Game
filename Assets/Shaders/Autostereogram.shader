Shader "Unlit/Autostereogram"
{
    Properties
    {
        _MainTex ("Pattern", 2D) = "white" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}
        _EyesToScreenDistance("Distance between eyes and screen", float) = 0.3
        _PupilDistance("Distance between pupils", float) = 0.066
        _PixelsPerMeter("Pixels per meter", float) = 5760
        _RandomSeed("Random Seed", int) = 0
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
            #include "ShaderUtils.cginc"



            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _DepthTex;
            float4 _DepthTex_TexelSize;

            float _EyesToScreenDistance;
            float _PupilDistance;
            float _PixelsPerMeter;
            uint _RandomSeed;


            float max(float x, float y)
            {
                return x > y ? x : y;
            }

            //Hashes for pseudo random pixels
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Find a pixel in the left eye image that would have its right eye version on the current pixel
                float xMin = max(0, _MainTex_TexelSize.z - _PupilDistance * _PixelsPerMeter);

                return fixed4(xMin / _MainTex_TexelSize.z, _MainTex_TexelSize.z/1920, 0, 1);

                for(float x = _MainTex_TexelSize.z; x > xMin; x--)
                {
                    float2 depthUV = float2(x/_DepthTex_TexelSize.z, i.uv.y);
                    float depth = tex2D(_DepthTex, depthUV) * _ProjectionParams.w;

                    int currentPixelInterval = _MainTex_TexelSize.z - x + 1;
                    int neededPixelInterval = _PupilDistance * (depth - _EyesToScreenDistance) / depth * _PixelsPerMeter;

                    if(neededPixelInterval == currentPixelInterval)
                    {
                        float2 mainTexUV = float2(x/_MainTex_TexelSize.z, i.uv.y);
                        return tex2D(_MainTex, mainTexUV);
                    }
                }

                // If no corresponding pixel was found, color this frangment pseudo randomly
                uint2 uvInt = uint2(i.uv * _MainTex_TexelSize.zw);
                fixed4 col = hash(uvInt.x + hashi(uvInt.y) + _RandomSeed);
                return col;
            }
            ENDCG
        }
    }
}
