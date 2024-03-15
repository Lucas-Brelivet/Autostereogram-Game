Shader "Unlit/BlitToRegion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Rect ("Rect (Lower Left, Upper Right Coordinates", Vector) = (0,0,1,1)
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
            float4 _MainTex_ST;

            float4 _Rect;

            v2f vert (appdata v)
            {
                v2f o;
                float4 clipPos = UnityObjectToClipPos(v.vertex);

                //Map the clip position this vertex from full screen to the required Rect
                float w = clipPos.w;
                clipPos.x = map(clipPos.x/w, -1, 1, _Rect.x, _Rect.z) * w;
                clipPos.y = map(clipPos.y/w, -1, 1, _Rect.y, _Rect.w) * w;
                o.vertex = clipPos;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
