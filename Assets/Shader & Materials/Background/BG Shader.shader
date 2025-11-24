Shader "Custom/BG"
{
    Properties
    {
        _Colour1 ("Color 1", Color) = (1,0,0,1)
        _Colour2 ("Color 2", Color) = (0,0,1,1)
        _Colour3 ("Color 3", Color) = (0,0,0,1)
        _Colour4 ("Color 4", Color) = (1,1,1,1)

        _Contrast("Contrast", Int) = 5
        _Gradual("Gradual", Float) = 2
        _Width1("Width1", Float) = 0.04
        _Width2("Width2", Float) = 0.1

        _Scale1("Scale1", Float) = 10
        _Scale2("Scale2", Float) = 1

        _Offset("Offset", Vector) = (0,0,0,0)

        _Intensity("Intensity", Float) = 0.2
        _SpinSpeed("Spin Speed", Float) = 0.2
        _SpinAmount("Spin Amount", Float) = 1.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float3 _Colour1, _Colour2, _Colour3, _Colour4;
            int _Contrast;
            float _Gradual, _Width1, _Width2;
            float _Scale1, _Scale2;
            float2 _Offset;
            float _Intensity, _SpinSpeed, _SpinAmount;

            // Godot smoothstep
            float smooth(float e0, float e1, float x)
            {
                float t = saturate((x - e0) / (e1 - e0));
                return t * t * (3 - 2 * t);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 fragCoord = IN.positionHCS.xy;
                float time = _Time.y;

                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);

                float2 uv = fragCoord * (1.0 / screenSize.y);

                float center = (_ScreenParams.y / _ScreenParams.x);
                uv.y -= 0.5;
                uv.x -= 0.5 * center;
                uv *= 2.0;
                uv += _Offset;

                float uv_len = length(uv);
                float angle = atan2(uv.y, uv.x);

                angle -= _SpinAmount * uv_len;
                angle += time * _SpinSpeed;

                uv = float2(
                    uv_len * cos(angle),
                    uv_len * sin(angle)
                ) * _Scale2;

                uv *= _Scale1;
                float2 uv2 = float2(uv.x + uv.y, 0);

                for (int i = 0; i < _Contrast; i++)
                {
                    uv2 += sin(uv);
                    uv += float2(
                        cos(_Intensity * uv2.y + time * _SpinSpeed),
                        sin(_Intensity * uv2.x - time * _SpinSpeed)
                    );

                    uv -= cos(uv.x + uv.y) - sin(uv.x - uv.y);
                }

                float paint_res = smooth(0, _Gradual, length(uv) / _Scale1);

                float c3p = 1.0 - min(_Width2, abs(paint_res - 0.5)) * (1.0 / _Width2);
                float c_out = max(0.0, (paint_res - (1.0 - _Width1))) * (1.0 / _Width1);
                float c_in = max(0.0, -(paint_res - _Width1)) * (1.0 / _Width1);
                float c4p = c_out + c_in;

                float3 ret_col = lerp(_Colour1, _Colour2, paint_res);
                ret_col = lerp(ret_col, _Colour3, c3p);
                ret_col = lerp(ret_col, _Colour4, c4p);

                return float4(ret_col, 1.0);
            }

            ENDHLSL
        }
    }
}
