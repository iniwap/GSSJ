/*
 *用于控制处理和装饰有关的逻辑
 *
 */
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;
using Reign;

public class TYAdjust : MonoBehaviour
{
    public void Start()
    {

    }

    public Text _RText;
    public Text _GText;
    public Text _BText;
    public Text _ExponentText;
    public Text _ContributionText;

    public Slider _RSlider;
    public Slider _GSlider;
    public Slider _BSlider;
    public Slider _ExponentSlider;
    public Slider _ContributionSlider;

    public void InitAdjust()
    {
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SunDiscColor");
        float SunHaloExponent = theme.GetFloat("_SunDiscExponent");
        float SunHaloContribution = theme.GetFloat("_SunDiscMultiplier");

        _RText.text = "" + (int)(c.r * 255);
        _GText.text = "" + (int)(c.g * 255);
        _BText.text = "" + (int)(c.b * 255);
        _ExponentText.text = Math.Round(SunHaloExponent / 500, 1) + "%";
        _ContributionText.text = (int)(SunHaloContribution) + "%";

        _RSlider.value = (int)(c.r * 255);
        _GSlider.value = (int)(c.g * 255);
        _BSlider.value = (int)(c.b * 255);
        _ExponentSlider.value = SunHaloExponent;
        _ContributionSlider.value = SunHaloContribution;
    }

    public void OnRValueChange(float v)
    {
        _RText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SunDiscColor");
        theme.SetColor("_SunDiscColor", new Color(v/255, c.g, c.b));
    }
    public void OnGValueChange(float v)
    {
        _GText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;

        Color c = theme.GetColor("_SunDiscColor");
        theme.SetColor("_SunDiscColor", new Color(c.r, v/255, c.b));
    }
    public void OnBValueChange(float v)
    {
        _BText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SunDiscColor");
        theme.SetColor("_SunDiscColor", new Color(c.r, c.g, v/255));
    }
    public void OnExponentValueChange(float v)
    {
        _ExponentText.text = Math.Round(v / 500,1) + "%";
        Material theme = RenderSettings.skybox;
        theme.SetFloat("_SunDiscExponent", v);
    }
    public void OnContributionValueChange(float v)
    {
        _ContributionText.text = (int)(v) + "%";
        Material theme = RenderSettings.skybox;
        theme.SetFloat("_SunDiscMultiplier", v);
    }

    public void OnSaveSetting()
    {
        Material theme = RenderSettings.skybox;
        string data = "";
        Color c = theme.GetColor("_SunDiscColor");
        float exponent = theme.GetFloat("_SunDiscExponent");
        float contribution = theme.GetFloat("_SunDiscMultiplier");

        data += (int)(c.r * 255) + "#";
        data += (int)(c.g * 255) + "#";
        data += (int)(c.b * 255) + "#";
        data += exponent + "#";
        data += contribution;

        Setting.setPlayerPrefs(theme.name + Setting.SETTING_KEY.TY_ADJUST_SETTING, data);
    }
}
