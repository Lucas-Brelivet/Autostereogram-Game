Shader "Unlit/Autostereogram"
{
    Properties
    {
        _MainTex ("Stereo Image", 2D) = "white" {}
        _RightDepthTex ("Right Eye Depth Texture", 2D) = "white" {}
        _LeftDepthTex ("Left Eye Depth Texture", 2D) = "white" {}
        _EyesToScreenDistance("Distance between eyes and screen", float) = 0.3
        _PupilDistance("Distance between pupils", float) = 0.066
        _PixelsPerMeter("Pixels per meter", float) = 5760
        _RandomSeed("Random Seed", int) = 0
    }

    //This shader allows to render an autostereogram based on a depth image.
    //To do so, the image must be divided in several vertical bands. The bands must be rendered in order from left to right.
    //The leftmost band is colored randomly, then for each successive band, the shader takes into consideration the left side
    //of the image, that has already been rendered and corresponds to what the left eye sees,
    //and renders on the band what the right eye must see here so that the viewer will perceive the depth indicated in the depth image.
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
            sampler2D _RightDepthTex;
            sampler2D _LeftDepthTex;

            float _EyesToScreenDistance;
            float _PupilDistance;
            float _PixelsPerMeter;
            uint _RandomSeed;
            float _MaxDepthValue;

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

            float4 colorHash(float2 uv)
            {
                float4 col;
                uint2 uvInt = uint2(uv * _MainTex_TexelSize.zw);
                col.x = hash(uvInt.x + hashi(uvInt.y) + _RandomSeed);
                col.y = hash(uvInt.x + hashi(uvInt.y) - _RandomSeed);
                col.z = hash(uvInt.x - hashi(uvInt.y) - _RandomSeed);
                return col;
            }

            //reads the depth from the given depth texture at the given uv
            float readDepth(sampler2D depthTex, float2 uv)
            {
                return max(DecodeFloatRGBA(tex2D(depthTex, uv))*_MaxDepthValue, _EyesToScreenDistance * 2);
            }


            //Input structs
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


            //Shader functions
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float normalizedPanelWidth = _PupilDistance/2 * _PixelsPerMeter * _MainTex_TexelSize.x;
                //Determine which uv on the right eye depth image this point corresponds to
                float2 rightEyeDepthUV = i.uv - float2(normalizedPanelWidth, 0);
                //Get the depth that the right eye sees
                float rightDepth = readDepth(_RightDepthTex, rightEyeDepthUV);
                //Determine where the left eye should be looking on the screen to see the correct depth, using Thales' theorem
                float2 leftEyeScreenUV = float2(i.uv.x - (_PupilDistance * (rightDepth - _EyesToScreenDistance) / rightDepth) * _PixelsPerMeter * _MainTex_TexelSize.x, i.uv.y);
                //return 1-((i.uv-leftEyeScreenUV).x-normalizedPanelWidth)/normalizedPanelWidth;
                if(leftEyeScreenUV.x >= 0)
                {
                    //Determine which uv on the left eye depth image corresponds to where the left eye looks
                    float2 leftEyeDepthUV = leftEyeScreenUV + float2(normalizedPanelWidth, 0);
                    //Check if the left eye sees the same depth, or if something else hides its view
                    float leftDepth = readDepth(_LeftDepthTex, leftEyeDepthUV);

                    if(equal(leftDepth, rightDepth, 0.15))
                    {
                        //if both eyes see the same depth, copy the left eye pixel on the main texture to this pixel
                        return tex2D(_MainTex, leftEyeScreenUV);
                    }
                }

                // If the two eyes see different depths, or if the left eye looks outside the screen, color the pixel randomly
                fixed4 col = colorHash(i.uv);
                return col;
            }
            ENDCG
        }
    }
}
