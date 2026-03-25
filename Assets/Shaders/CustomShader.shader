Shader "Custom/CustomShader"
{
    // Properties
    // {
    //     _Color ("Color", Color) = (1,1,1,1)
    //     _MainTex ("Albedo (RGB)", 2D) = "white" {}
    //     _Glossiness ("Smoothness", Range(0,1)) = 0.5
    //     _Metallic ("Metallic", Range(0,1)) = 0.0
    // }
    // SubShader
    // {
    //     Tags { "RenderType"="Opaque" }
    //     LOD 200

    //     CGPROGRAM
    //     // Physically based Standard lighting model, and enable shadows on all light types
    //     #pragma surface surf Standard fullforwardshadows

    //     // Use shader model 3.0 target, to get nicer looking lighting
    //     #pragma target 3.0

    //     sampler2D _MainTex;

    //     struct Input
    //     {
    //         float2 uv_MainTex;
    //     };

    //     half _Glossiness;
    //     half _Metallic;
    //     fixed4 _Color;

    //     // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    //     // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    //     // #pragma instancing_options assumeuniformscaling
    //     UNITY_INSTANCING_BUFFER_START(Props)
    //         // put more per-instance properties here
    //     UNITY_INSTANCING_BUFFER_END(Props)

    //     void surf (Input IN, inout SurfaceOutputStandard o)
    //     {
    //         // Albedo comes from a texture tinted by color
    //         fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
    //         o.Albedo = c.rgb;
    //         // Metallic and smoothness come from slider variables
    //         o.Metallic = _Metallic;
    //         o.Smoothness = _Glossiness;
    //         o.Alpha = c.a;
    //     }
    //     ENDCG
    // }
    // FallBack "Diffuse"
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // This is the magic line
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Required for ID
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Required to pass ID
            };

            sampler2D _MainTex;

            // Define the property buffer
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // This line uses the Matrix4x4 you passed in C#
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                return col;
            }
            ENDCG
        }
    }
}
