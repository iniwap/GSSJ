using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ColorASCII : ImageEffectBase
{
    public Texture tiles;

    [Range(1, 100)]
    public float tilesXY = 32;
    private float _tilesXY = 32;

    [Range(0, 1)]
    public float loResAlpha = 1;
    private float _loResAlpha = 1;

    [Range(0, 1)]
    public float hiResAlpha = 1;
    private float _hiResAlpha = 1;

    [Range(0, 10)]
    public float characterBrightness = 0;
    private float _characterBrightness = 0;

    protected override void Awake()
    {
        _shaderName = "Custom/ColorASCII";
        _loResAlpha = loResAlpha;
        _hiResAlpha = hiResAlpha;
        _characterBrightness = characterBrightness;

        _tilesXY = tilesXY;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _material.SetTexture("_Tiles", tiles);
    }


    public void ResetDefault()
    {
        loResAlpha = _loResAlpha;
        hiResAlpha = _hiResAlpha;
        characterBrightness = _characterBrightness;
        tilesXY = _tilesXY;
    }

    // Start is called before the first frame update
    void Start()
    {
        _material.SetInt("_TileArraySize", 11);
    }

    protected override  void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetInt("_TilesX", (int)tilesXY);
        _material.SetInt("_TilesY", (int)tilesXY);
        _material.SetFloat("_LoResAlpha", loResAlpha);
        _material.SetFloat("_HiResAlpha", hiResAlpha);
        _material.SetFloat("_CharacterBrightness", characterBrightness);

        RenderTexture scaled = RenderTexture.GetTemporary((int)tilesXY, (int)tilesXY);
        scaled.filterMode = FilterMode.Point;
        source.filterMode = FilterMode.Point;

        Graphics.Blit(source, scaled);
        _material.SetTexture("_ScaledTex", scaled);

        Graphics.Blit(source, destination, _material);

        RenderTexture.ReleaseTemporary(scaled);
    }
}
