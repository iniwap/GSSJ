using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OilPaintRenderer), PostProcessEvent.AfterStack, "Custom/OilPaint")]
public sealed class OilPaint : PostProcessEffectSettings
{
    [Range(0, 16), Tooltip("Brush Radius")]
    public FloatParameter radius = new FloatParameter { value = 10f };
}

public sealed class OilPaintRenderer : PostProcessEffectRenderer<OilPaint>
{
    static class ShaderPropertyID
    {
        internal static readonly int Radius = Shader.PropertyToID("_Radius");
    }

    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Custom/OilPaint"));

        sheet.properties.SetFloat(ShaderPropertyID.Radius, (int)settings.radius);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
