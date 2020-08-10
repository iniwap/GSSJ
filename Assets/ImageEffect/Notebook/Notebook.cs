using UnityEngine;
using ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Notebook : ImageEffectBase
{
    public enum NotebookParamType
    {
        NONE,
        SampNum,
        AngleNum,
        LineAlpha,
        LineComplete,
        LineSoft,
        GridSize,
        GridAlphaX,
        GridAlphaY,
        GridWidth,
        R,
        G,
        B,
        END
    }

    public Texture noiseTexture;

    [Range(0.01f, 2)]
    public float lineAlpha = 1; 
    private float _lineAlpha = 1;

    [Range(1, 100)]
    public float gridWidth = 80f;
    private float _gridWidth = 80f;

    [Range(1, 200)]
    public float gridSize = 100f;
    private float _gridSize = 100f;

    [Range(1, 5)]
    public float angleNum = 3;
    private float _angleNum = 3;

    [Range(1, 16)]
    public float sampNum = 16;
    private float _sampNum = 16;

    [Range(0.001f, 1)]
    public float lineComplete = 0.4f;
    private float _lineComplete = 0.4f;

    [Range(0.001f, 1)]
    public float lineSoft = 0.4f;
    private float _lineSoft = 0.4f;

    [Range(0, 1)]
    public float gridAlphaX = 0.5f;
    private float _gridAlphaX = 0.5f;

    [Range(0, 1)]
    public float gridAlphaY = 0.5f;
    private float _gridAlphaY = 0.5f;

    public Color gridColor = new Color(0.75f, 0.9f, 0.9f);
    private Color _gridColor = new Color(0.75f, 0.9f, 0.9f);

    private static readonly int LineAlpha = Shader.PropertyToID("_LineAlpha");
    private static readonly int GridWidth = Shader.PropertyToID("_GridWidth");
    private static readonly int GridSize = Shader.PropertyToID("_GridSize");//0.35
    private static readonly int AngleNum = Shader.PropertyToID("_AngleNum");
    private static readonly int SampNum = Shader.PropertyToID("_SampNum");
    private static readonly int LineComplete = Shader.PropertyToID("_LineComplete");
    private static readonly int LineSoft = Shader.PropertyToID("_LineSoft");
    private static readonly int GridAlpha = Shader.PropertyToID("_GridAlpha");
    private static readonly int GridColor = Shader.PropertyToID("_GridColor");

    protected override void Awake()
    {
        _shaderName = "Custom/Notebook";
        _lineAlpha = lineAlpha;
        _gridWidth = gridWidth;
        _gridSize = gridSize;
        _angleNum = angleNum;
        _sampNum = sampNum;
        _lineComplete = lineComplete;
        _lineSoft = lineSoft;
        _gridAlphaX = gridAlphaX;
        _gridAlphaY = gridAlphaY;
        _gridColor = gridColor;
    }

    public void Start()
    {
        _material.SetTexture("_NoiseTex", noiseTexture);
    }
    public void ResetDefault()
    {
        lineAlpha = _lineAlpha;
        gridWidth = _gridWidth;
        gridSize = _gridSize;
        angleNum = _angleNum;
        sampNum = _sampNum;
        lineComplete = _lineComplete;
        lineSoft = _lineSoft;
        gridAlphaX = _gridAlphaX;
        gridAlphaY = _gridAlphaY;
        gridColor = _gridColor;
    }


    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetFloat(LineAlpha, lineAlpha);
        _material.SetInt(GridWidth, (int)gridWidth);
        _material.SetInt(GridSize, (int)gridSize);
        _material.SetInt(AngleNum, (int)angleNum);
        _material.SetInt(SampNum, (int)sampNum);
        _material.SetFloat(LineComplete, lineComplete);
        _material.SetFloat(LineSoft, lineSoft);
        _material.SetVector(GridAlpha, new Vector4(gridAlphaX, gridAlphaY, 0f,0f));
        _material.SetColor(GridColor, Color.white - gridColor);


        Graphics.Blit(source, destination, _material);
    }
}
