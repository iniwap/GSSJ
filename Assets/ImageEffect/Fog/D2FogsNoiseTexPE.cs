//#define DEBUG_RENDER

using UnityEngine;
using ImageEffects;

[ExecuteInEditMode]
public class D2FogsNoiseTexPE : ImageEffectBase
{
    public Texture2D Noise;

    public Color Color = new Color(1f, 1f, 1f, 1f);
    private Color _Color = new Color(1f, 1f, 1f, 1f);
    public float Color1Value { get; set; }//用来控制颜色变化时的条件/间隔

    [Range(0.0f, 1f)]
    public float Size = 0.02f;
    private float _Size = 0.02f;

    [Range(0.0f, 1f)]
    public float HorizontalSpeed = 0.02f;
    private float _HorizontalSpeed = 0.02f;

    //public float VerticalSpeed = 0f;

    [Range(0.0f, 3f)]
    public float Density = 1.18f;
    private float _Density = 1.18f;

    protected override void Awake()
    {
        _shaderName = "Custom/D2FogsNoiseTex";
        _Color = Color;
        _Size = Size;
        _HorizontalSpeed = HorizontalSpeed;
        _Density = Density;
        Color1Value = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _material.SetTexture("_NoiseTex", Noise);
    }

    public void ResetDefault()
    {
        Color = _Color;
        Size = _Size;
        HorizontalSpeed = _HorizontalSpeed;
        Density = _Density;
        Color1Value = 0;
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetColor("_Color", Color);
        _material.SetFloat("_Size", Size);
        _material.SetFloat("_Speed", HorizontalSpeed);
        //_material.SetFloat("_VSpeed", VerticalSpeed);
        _material.SetFloat("_Density", Density);


        Graphics.Blit(source, destination, _material);
    }
}