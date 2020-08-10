using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

[Serializable]
[PostProcess(typeof(DigitalGlitchRenderer), PostProcessEvent.BeforeStack, "Custom/DigitalGlitch")]
public class DigitalGlitch : PostProcessEffectSettings
{
    [Range(0, 1), Tooltip("Intensity")]
    public FloatParameter intensity = new FloatParameter { value = 0 };

    public BoolParameter change = new BoolParameter { value = true };
}

public sealed class DigitalGlitchRenderer : PostProcessEffectRenderer<DigitalGlitch>
{
    static class ShaderPropertyID
    {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");
        internal static readonly int TrashTex = Shader.PropertyToID("_TrashTex");
    }

    Texture2D noiseTexture;
    public override void Init()
    {
        noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
        noiseTexture.hideFlags = HideFlags.DontSave;
        noiseTexture.wrapMode = TextureWrapMode.Clamp;
        noiseTexture.filterMode = FilterMode.Point;
        UpdateNoiseTexture();
    }

    Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, Random.value);
    }

    void UpdateNoiseTexture()
    {
        var color = GetRandomColor();
        for (int y = 0; y < noiseTexture.height; y++) {
            for (int x = 0; x < noiseTexture.width; x++) {
                if (Random.value > 0.89f)
                    color = GetRandomColor();
                noiseTexture.SetPixel(x, y, color);
            }
        }

        noiseTexture.Apply();
    }

    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Custom/DigitalGlitch"));
        RenderTexture trashFrame = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

        context.command.Blit(context.source, trashFrame);

        sheet.properties.SetFloat(ShaderPropertyID.Intensity, settings.intensity);
        sheet.properties.SetTexture(ShaderPropertyID.NoiseTex, noiseTexture);
        sheet.properties.SetTexture(ShaderPropertyID.TrashTex, trashFrame);
        sheet.EnableKeyword("APPLY_FORWARD_FOG");
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

        RenderTexture.ReleaseTemporary(trashFrame);
    }

    public override void Release()
    {
        base.Release();
        UnityEngine.Object.DestroyImmediate(noiseTexture);
    }
}
