using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelEffect : MonoBehaviour
{
    public int pixelWidth = 320;
    [Header("Hue")]
    public int hueSteps = 8;
    public float hueOffset = 0;
    [Header("Saturation")]
    public int saturationSteps = 16;
    //[Range(-1, 1)]
    public float minSaturation = 0.5f;
    public float saturationOffset = 0;
    [Header("Lightness")]
    public int lightnessSteps = 8;
    [Range(0,1)]
    public float minLightness = 0.5f;
    public float lightnessOffset = 0;

    private Material material_pixel_effect;
    // Start is called before the first frame update
    void Awake()
    {
        material_pixel_effect = new Material(Shader.Find("MyShaders/PixelEffectShader"));
    }

    private void OnRenderImage (RenderTexture source, RenderTexture destination) {
        source.filterMode = FilterMode.Point;

        // Pixel Effect Shader
        material_pixel_effect.SetInt("_PixelWidth", pixelWidth);
        material_pixel_effect.SetInt("_PixelHeight", (int)(pixelWidth*Screen.height/Screen.width));
        material_pixel_effect.SetInt("_HueSteps", hueSteps);
        material_pixel_effect.SetFloat("_HueOffset", hueOffset);
        material_pixel_effect.SetInt("_SatSteps", saturationSteps);
        material_pixel_effect.SetFloat("_MinSat", minSaturation);
        material_pixel_effect.SetFloat("_SatOffset", saturationOffset);
        material_pixel_effect.SetInt("_LitSteps", lightnessSteps);
        material_pixel_effect.SetFloat("_MinLit", minLightness);
        material_pixel_effect.SetFloat("_LitOffset", lightnessOffset);
        //material.SetInt("_ScreenWidth", Screen.width);
        //material.SetInt("_ScreenHeight", Screen.height);
        Graphics.Blit(source, destination, material_pixel_effect);
    }
}
