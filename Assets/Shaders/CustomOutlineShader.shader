Shader "Custom/CustomOutlineShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Exposure ("Exposure", float) = 0
        [Toggle] _IsHighlighted ("Is Highlighted", float) = 0
        _BorderThickness ("Border Thickness", float) = 5
        _BorderMaxPercentThickness ("Border Max Percent Thickness", float) = 0.02
        _BorderDarkness ("Border Darkness", float) = 0.8
        _BorderShininessOffset ("Border Shininess Offset", float) = -1.125
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off 
        ZWrite On
        ZTest LEqual
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
                UNITY_DEFINE_INSTANCED_PROP(float, _IsHighlighted)
                UNITY_DEFINE_INSTANCED_PROP(float, _Exposure)
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

            //float _IsHighlighted;
            float _BorderThickness;
            float _BorderMaxPercentThickness;
            fixed4 _BorderColor;
            float _BorderDarkness;
            float _BorderShininessOffset;

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                // Calculate border thickness
                float borderThickness = fwidth(i.uv) * _BorderThickness;
                if (borderThickness > _BorderMaxPercentThickness) borderThickness = _BorderMaxPercentThickness;

                // Calculate border and border darkness
                float border = 0;
                float borderDarkness = _BorderDarkness;
                // The border is brighter if it's either the top or left edge -> Create a 3D illusion

                border =    step(1.0 - borderThickness, i.uv.x) + 
                            step(i.uv.y, borderThickness);
                
                if (!(border > 0))
                {
                    border =    step(1.0 - borderThickness, i.uv.y) + 
                                step(i.uv.x, borderThickness);
                    borderDarkness = borderDarkness + _BorderShininessOffset;
                }

                float mask = saturate(border); // Clamp the border value between 0 and 1

                fixed4 borderColor = fixed4(_Color.rgb, 1.0); // Calculate border's color
                // If highlighted -> dont calculate darkness and opacity
                if (UNITY_ACCESS_INSTANCED_PROP(Props, _IsHighlighted) < 1) 
                {
                    if (col.a == 0) 
                    {
                        //borderColor.a = 0.5; // Calculate border's opacity
                        borderDarkness *= 0.75;
                    }
                    if (borderDarkness < 0)
                    {
                        borderDarkness = -borderDarkness;
                        borderColor.rgb = lerp(borderColor.rgb, float3(1, 1, 1), borderDarkness);
                    }
                    else
                        borderColor.rgb = lerp(borderColor.rgb, float3(0, 0, 0), borderDarkness); // Calculate border's darkness

                }

                fixed4 finalColor = lerp(col, borderColor, mask);

                float exposure = UNITY_ACCESS_INSTANCED_PROP(Props, _Exposure);
                if (exposure > 0)
                {
                    if (exposure > 1) exposure = 1;
                    finalColor = lerp(finalColor, fixed4(1, 1, 1, finalColor.a), exposure);
                }
                else
                {
                    exposure = -exposure;
                    if (exposure > 1) exposure = 1;
                    finalColor = lerp(finalColor, fixed4(0, 0, 0, finalColor.a), exposure);
                }

                return finalColor;
            }
            ENDCG
        }
    }
}
