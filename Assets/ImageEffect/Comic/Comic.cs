using UnityEngine;
using ImageEffects;


class Comic : ImageEffectBase
{    
    private static readonly int CenterX = Shader.PropertyToID("_CenterX");
    private static readonly int CenterY = Shader.PropertyToID("_CenterY");
    private static readonly int CentralEdge = Shader.PropertyToID("_CentralEdge");//0.35
    private static readonly int CentralLength = Shader.PropertyToID("_CentralLength");
    private static readonly int Central = Shader.PropertyToID("_Central");
    private static readonly int Line = Shader.PropertyToID("_Line");

    [Range(0,1)]
    public float centerX = 0.5f;
    private float _centerX = 0.5f;
    [Range(0, 1)]
    public float centerY = 0.5f;
    private float _centerY = 0.5f;
    [Range(0.1f, 0.8f)]
    public float centralEdge = 0.4f;
    private float _centralEdge = 0.4f;
    [Range(0.1f, 1)]
    public float centralLength = 0.84f;
    private float _centralLength = 0.84f;
    [Range(0, 10)]
    public float line = 1f;
    private float _line = 1f;
    [Range(0, 0.2f)]
    public float central = 0.1f;
    private float _central = 0.1f;

    protected override void Awake()
    {
        _shaderName = "Custom/Comic";
        _centerX = centerX;
        _centerY = centerY;
        _centralEdge = centralEdge;
        _centralLength = centralLength;
        _central = central;
        _line = line;
    }

    public void ResetDefault()
    {
        centerX = _centerX;
        centerY = _centerY;
        centralEdge = _centralEdge;
        centralLength = _centralLength;
        central = _central;
        line = _line;
    }


    protected override void OnRenderImage(RenderTexture source, RenderTexture destination) 
    {

        _material.SetFloat(CenterX, centerX);
        _material.SetFloat(CenterY, centerY);
        _material.SetFloat(CentralEdge, centralEdge);
        _material.SetFloat(CentralLength, centralLength);
        _material.SetFloat(Central, central);
        _material.SetFloat(Line, line);

        Graphics.Blit(source, destination, _material);
    }
   
}