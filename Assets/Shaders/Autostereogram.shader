Shader "Unlit/Autostereogram"
{
    Properties
    {
        _MainTex ("Stereo Image", 2D) = "white" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}
        _EyesToScreenDistance("Distance between eyes and screen", float) = 0.3
        _PupilDistance("Distance between pupils", float) = 0.066
        _PixelsPerMeter("Pixels per meter", float) = 5760
        _PanelWidth("Panel width", int) = 100
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
            int _PanelWidth;
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
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.screenPos /= o.screenPos.w;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.screenPos.xy,1,1);
                if(i.screenPos.x > _PanelWidth)
                {
                    // Find a pixel in the left eye image that would have its right eye version on the current pixel
                    float xMin = max(0, i.screenPos.x - _PupilDistance * _PixelsPerMeter);

                    for(float x = i.screenPos.x - _PanelWidth; x >= xMin; x--)
                    {
                        float2 depthUV = float2(x/_DepthTex_TexelSize.z, i.uv.y);
                        float depth = map(tex2D(_DepthTex, depthUV).x, 0, 1, _ProjectionParams.y, _ProjectionParams.z);

                        int currentPixelInterval = i.screenPos.x - x;
                        int neededPixelInterval = _PupilDistance * (depth - _EyesToScreenDistance) / depth * _PixelsPerMeter;

                        if(neededPixelInterval == currentPixelInterval)
                        {
                            float2 mainTexUV = float2(x/_MainTex_TexelSize.z, i.uv.y);
                            return tex2D(_MainTex, mainTexUV);
                        }
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
