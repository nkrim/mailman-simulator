Shader "MyShaders/PixelEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelWidth ("Pixel Width", Int) = 320 
        _PixelHeight ("Pixel Height", Int) = 180
        _HueSteps ("Hue Steps", Int) = 8
        _HueOffset ("Hue Offset", Float) = 0.0
        _SatSteps ("Saturation Steps", Int) = 16
        _MinLit ("Minimum Saturation", Float) = 0.5
        _SatOffset ("Saturation Offset", Float) = 0.0
        _LitSteps ("Lightness Steps", Int) = 8
        _MinLit ("Minimum Lightness", Float) = 0.5
        _LitOffset ("Lightness Offset", Float) = 0.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            
            float3 HUEtoRGB(in float H)
            {
                float R = abs(H * 6 - 3) - 1;
                float G = 2 - abs(H * 6 - 2);
                float B = 2 - abs(H * 6 - 4);
                return saturate(float3(R,G,B));
            }
            float Epsilon = 1e-10;
            float3 RGBtoHCV(in float3 RGB)
            {
                // Based on work by Sam Hocevar and Emil Persson
                float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
                float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
                float C = Q.x - min(Q.w, Q.y);
                float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
                return float3(H, C, Q.x);
            }
            float3 RGBtoHSL(in float3 RGB)
            {
                float3 HCV = RGBtoHCV(RGB);
                float L = HCV.z - HCV.y * 0.5;
                float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
                return float3(HCV.x, S, L);
            }
            float3 HSLtoRGB(in float3 HSL)
            {
                float3 RGB = HUEtoRGB(HSL.x);
                float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
                return (RGB - 0.5) * C + HSL.z;
            }
            
            uniform sampler2D _MainTex;
            uniform int _PixelWidth;
            uniform int _PixelHeight;
            uniform int _HueSteps;
            uniform float _HueOffset;
            uniform int _SatSteps;
            uniform float _MinSat;
            uniform float _SatOffset;
            uniform int _LitSteps;
            uniform float _MinLit;
            uniform float _LitOffset;

            fixed4 frag (v2f_img i) : SV_Target
            {
                // Sample pixel color
                float p_u = (floor(i.uv.x * _PixelWidth) + 0.5) / _PixelWidth;
                float p_v = (floor(i.uv.y * _PixelHeight) + 0.5) / _PixelHeight;
                float2 p_uv = {p_u, p_v};
                float4 p_col = tex2D(_MainTex, p_uv);
                // Color ramp
                float3 hsl = RGBtoHSL(p_col);
                float3 ramped = float3( ceil(hsl.x*_HueSteps)/_HueSteps+_HueOffset, 
                                        ceil(max(_MinSat,hsl.y)*_SatSteps)/_SatSteps+_SatOffset, 
                                        ceil(max(_MinLit,hsl.z)*_LitSteps)/_LitSteps+_LitOffset);
                float4 col = fixed4(HSLtoRGB(ramped), 1.0);
                // Return
                return col;
            }
            ENDCG
        }
        
        /*GrabPass {
            "_PixelTex"
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            uniform sampler2D _PixelTex;
            uniform int _PixelWidth;
            uniform int _PixelHeight;
           
            bool equals (fixed3 a, fixed3 b) {
                if(abs(a.x - b.x) < 0.01) {
                    if(abs(a.y - b.y) < 0.01) {
                        if(abs(a.z - b.z) < 0.01)
                            return true;
                    }
                }
                return false;
            }
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                float p_u = (floor(i.uv.x * _PixelWidth) + 0.5) / _PixelWidth;
                float p_v = (floor(i.uv.y * _PixelHeight) + 0.5) / _PixelHeight;
                float2 p_uv = {p_u, p_v};
                fixed4 kernel[9];
                //float4 sum = 0;
                float pixel_inc_x = 1.0f/_PixelWidth;
                float pixel_inc_y = 1.0f/_PixelHeight;
                for(int j=0; j<9; j++) {
                    float2 uv = {
                        p_uv.x + ((j%3)-1)*pixel_inc_x, 
                        1 - p_uv.y + ((j/3)-1)*pixel_inc_y
                    };
                    fixed4 sampled = tex2D(_PixelTex, uv);
                    kernel[j] = sampled;
                    //sum += sampled;
                }
                fixed4 colors[9];
                int colors_length = 0;
                int freq[9];
                int max_freq = 1;
                int max_indx = 0;
                bool multi_max = false;
                colors[0] = kernel[0];
                freq[0] = 1;  
                for(int j=1; j<9; j++) {
                    int weight = j==4 ? 3 : 1;
                    fixed4 cur = kernel[j];
                    bool found_equal = false;
                    for(int k=0; k<colors_length; k++) {
                        if(equals(cur,colors[k])) {
                            int cur_freq = freq[k] + weight;
                            freq[k] = cur_freq;
                            found_equal = true;
                            if(cur_freq > max_freq) {
                                max_freq = cur_freq;
                                max_indx = k;
                                multi_max = false;
                            }
                            else if(cur_freq == max_freq) {
                                multi_max = true;
                            }
                            break;
                        }
                    }
                    if(!found_equal) {
                        colors[colors_length] = cur;
                        freq[colors_length] = weight;
                        colors_length++;
                    }
                }
                fixed4 final_col;
                if(max_freq == 1)
                    final_col = kernel[4];
                else
                    final_col = colors[max_indx];
                return final_col;
                //sum += 6*kernel[4];
                //fixed4 avg = sum / 15;
                //kernel[4] = kernel[0]; // For more intuitive iteration, skip center sample
                //float min_diff = length(avg - kernel[1]);
                //int min_index = 1;
                //for(int j=2; j<9; j++) {
                //    float diff = length(avg - kernel[j]);
                //    if(diff < min_diff) {
                //        min_diff = diff;
                //        min_index = j;
                //    }
                //}
                //return kernel[min_index];
            }
            ENDCG
        }*/
    }
}
