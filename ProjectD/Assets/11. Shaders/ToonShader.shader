Shader "Custom/ToonShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _OutlineColor("OutLine Color", Color) = (0, 0, 0, 1)
        _OutlineWeight("Outline Weight", Range(0.1, 20)) = 5
        [Int]_LevelCount("Level Count", Range(2, 10)) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        cull front

        CGPROGRAM
        #pragma surface surf Nolight noambient vertex:vert noshadow
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        fixed4 _OutlineColor;
        float _OutlineWeight;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 diffColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = diffColor.rgb;
            o.Alpha = diffColor.a;
        }
        float4 LightingNolight(SurfaceOutput s, float3 lightDir, float atten)
        {
            return _OutlineColor;
        }
        void vert(inout appdata_full v)
        {
            v.vertex.xyz = v.vertex.xyz + v.normal.xyz * (_OutlineWeight / 1000);
        }
        ENDCG

        cull back


        CGPROGRAM
        #pragma surface surf Toon noambient
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        int _LevelCount;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 mainTex = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = mainTex.rgb;
            o.Alpha = mainTex.a;
        }
        float4 LightingToon(SurfaceOutput s, float3 lightDir, float atten)
        {
            float4 final;
            float3 diffColor = s.Albedo * _LightColor0.rgb * atten;
            float ndotl = dot(s.Normal, lightDir) * 0.5 + 0.5;
            ndotl *= 5;
            ndotl = ceil(ndotl) / _LevelCount;        // 여기에 들어가는 상수가 얼마나 층을 많이 지게 할지를 정한다.

            final.rgb = ndotl * diffColor;
            final.a = s.Alpha;

            return final;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
