using UnityEngine;
using ImageEffects;

[ExecuteInEditMode]
public class Ice : ImageEffectBase
{
    [Range(0, 1)]
    public float FrostAmount = 0.5f; //0-1 (0=minimum Frost, 1=maximum frost)
    private float _FrostAmount = 0.5f;
    [Range(0, 1)]
    public float EdgeSharpness = 1; //>=1
    private float _EdgeSharpness = 1; //>=1
    [Range(0, 1)]
    public float minFrost = 0; //0-1
    [Range(0, 1)]
    public float maxFrost = 1; //0-1
    [Range(0, 1)]
    public float seethroughness = 0.2f; //blends between 2 ways of applying the frost effect: 0=normal blend mode, 1="overlay" blend mode
    private float _seethroughness = 0.2f;
    [Range(0, 1)]
    public float distortion = 0.1f; //how much the original image is distorted through the frost (value depends on normal map)
    private float _distortion = 0.1f;

    public Texture2D Frost; //RGBA
    public Texture2D FrostNormals; //normalmap
	
	protected override void Awake()
	{
        _shaderName = "Custom/Ice";
        _FrostAmount = FrostAmount;
        _seethroughness = seethroughness;
        _distortion = distortion;
        _EdgeSharpness = EdgeSharpness;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _material.SetTexture("_BlendTex", Frost);
        _material.SetTexture("_BumpMap", FrostNormals);
    }

    public void ResetDefault()
    {
        FrostAmount = _FrostAmount;
        seethroughness = _seethroughness;
        distortion = _distortion;
        EdgeSharpness = _EdgeSharpness;
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetFloat("_BlendAmount", Mathf.Clamp01(Mathf.Clamp01(FrostAmount) * (maxFrost - minFrost) + minFrost));
        _material.SetFloat("_EdgeSharpness", EdgeSharpness);
        _material.SetFloat("_SeeThroughness", seethroughness);
        _material.SetFloat("_Distortion", distortion);

		Graphics.Blit(source, destination, _material);
	}
}