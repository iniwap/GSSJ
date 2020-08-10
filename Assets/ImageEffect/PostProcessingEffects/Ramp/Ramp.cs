using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(RampRenderer), PostProcessEvent.BeforeStack, "Custom/Ramp")]
public class Ramp : PostProcessEffectSettings
{
    [Tooltip("Color1")]
    public ColorParameter color1 = new ColorParameter { value = Color.blue };
    public float Color1Value { get; set; }//用来控制颜色变化时的条件/间隔
    [Tooltip("Color2")]
    public ColorParameter color2 = new ColorParameter { value = Color.red };
    public float Color2Value { get; set; }//用来控制颜色变化时的条件/间隔
    [Range(-180, 180), Tooltip("Angle")]
    public FloatParameter angle = new FloatParameter { value = 45 };
    [Range(0, 1), Tooltip("Opacity")]
    public FloatParameter opacity = new FloatParameter { value = 1 };

    public enum BlendMode
    {
        Multiply = 0,
        Screen,
        Overlay,
        HardLight,
        SoftLight
    }
    [SerializeField]
    public BlendMode blendMode = BlendMode.Overlay;
}

public sealed class RampRenderer : PostProcessEffectRenderer<Ramp>
{
    static class ShaderPropertyID
    {
        internal static readonly int Color1 = Shader.PropertyToID("_Color1");
        internal static readonly int Color2 = Shader.PropertyToID("_Color2");
        internal static readonly int Direction = Shader.PropertyToID("_Direction");
    }

    static string[] _blendModeKeywords = {
        "_MULTIPLY",
        "_SCREEN",
        "_OVERLAY",
        "_HARDLIGHT",
        "_SOFTLIGHT"
    };

    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Custom/Ramp"));
        Color color0;
        if (settings.blendMode == Ramp.BlendMode.Multiply)
            color0 = Color.white;
        else if (settings.blendMode == Ramp.BlendMode.Multiply)
            color0 = Color.black;
        else
            color0 = Color.gray;

        sheet.properties.SetColor(ShaderPropertyID.Color1, Color.Lerp(color0, settings.color1, settings.opacity));
        sheet.properties.SetColor(ShaderPropertyID.Color2, Color.Lerp(color0, settings.color2, settings.opacity));
        var phi = Mathf.Deg2Rad * settings.angle;
        var dir = new Vector2(Mathf.Cos(phi), Mathf.Sin(phi));
        sheet.properties.SetVector(ShaderPropertyID.Direction, dir);

        sheet.ClearKeywords();
        sheet.EnableKeyword(_blendModeKeywords[(int)settings.blendMode]);
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            sheet.EnableKeyword("_LINEAR");

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}