using UnityEngine;
using System.Collections;
using ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Deferred night vision effect.
/// </summary>
public class DeferredNightVision : ImageEffectBase {

    [SerializeField]
    [Tooltip("The main color of the NV effect")]
    public Color m_NVColor = new Color(0f, 1f, 0.1724138f, 1f);
    private float _nvAlpha = 1.0f;

    [SerializeField]
    [Tooltip("The color that the NV effect will 'bleach' towards (white = default)")]
    public Color m_TargetBleachColor = new Color(1f, 1f, 1f, 0f);

    [Range(0f, 0.1f)]
    [Tooltip("How much base lighting does the NV effect pick up")]
    public float m_baseLightingContribution = 0.025f;

    [Range(0f, 100f)]
    [Tooltip("The higher this value, the more bright areas will get 'bleached out'")]
    public float m_LightSensitivityMultiplier = 10f;
    private float _m_LightSensitivityMultiplier;

    [Tooltip("Do we want to apply a vignette to the edges of the screen?")]
    public bool useVignetting = true;

    [Range(0.05f, 1f)]
    public float scope = 0.5f;
    private float _scope = 0.5f;

    [ContextMenu("UpdateShaderValues")]
    public void UpdateShaderValues()
    {
        if (_material == null)
            return;
        _material.SetVector("_NVColor", m_NVColor);
        _material.SetVector("_TargetWhiteColor", m_TargetBleachColor);
        _material.SetFloat("_BaseLightingContribution", m_baseLightingContribution);
        _material.SetFloat("_LightSensitivityMultiplier", m_LightSensitivityMultiplier);
        _material.SetFloat("_Scope", scope);

        // State switching		
        _material.shaderKeywords = null;

        if (useVignetting)
        {
            Shader.EnableKeyword("USE_VIGNETTE");
        } else {
            Shader.DisableKeyword("USE_VIGNETTE");
        }

    }

    public void ResetDefault()
    {
        m_NVColor = new Color(m_NVColor.r, m_NVColor.g, m_NVColor.b, _nvAlpha);
        m_LightSensitivityMultiplier = _m_LightSensitivityMultiplier;
        scope = _scope;
    }

    protected override void Awake()
    {
        _shaderName = "Custom/DeferredNightVision";
        _nvAlpha = m_NVColor.a;
        _m_LightSensitivityMultiplier = m_LightSensitivityMultiplier;
        _scope = scope;
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{		
		UpdateShaderValues();
		
		Graphics.Blit(source, destination, _material);
	}
}
