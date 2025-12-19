Shader "Fristy/Nature/Rocks"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0)
        [NoScaleOffset]_MainTex ("Albedo (RBG) and Occlusion (A)", 2D) = "white" {}
         [NoScaleOffset][Normal]_MainNorm ("Normal ", 2D) = "bump" {}
         _MainNormPow("Normal power", Range(-2,2)) = 1
         _uv("uv", float) = 1
        _occlusionPow ("Occlusion Power", Range(0,1)) = 0.5
        _Glossiness ("Smoothness highs", Range(0,1)) = 0.5
        _Glossiness1 ("smoothness lows", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _MainNorm;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness, _Glossiness1, _MainNormPow, _uv, _occlusionPow;
      
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {


            IN.uv_MainTex *= _uv, _uv;
            // Albedo comes from a texture tinted by color
            fixed4 c =tex2D (_MainTex, IN.uv_MainTex);
            fixed3 cnorm = UnpackNormal(tex2D(_MainNorm, IN.uv_MainTex));
            cnorm.x *= _MainNormPow;
            cnorm.y *= _MainNormPow;
            fixed sm = c;

            o.Albedo =  lerp(c*_Color, _Color,_Color.a);
            o.Normal = normalize(cnorm);
            o.Occlusion = lerp(1,c.a, _occlusionPow );
            // Metallic and smoothness come from slider variables
           
            o.Smoothness = lerp(_Glossiness, _Glossiness1, sm);
            o.Alpha = c.a;
        } 
        ENDCG
    }
    FallBack "Diffuse"
}
