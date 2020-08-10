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

public class DPXAdjust : MonoBehaviour
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
        Color c = theme.GetColor("_HorizonLineColor");
        float SunHaloExponent = theme.GetFloat("_HorizonLineExponent");
        float SunHaloContribution = theme.GetFloat("_HorizonLineContribution");

        _RText.text = "" + (int)(c.r * 255);
        _GText.text = "" + (int)(c.g * 255);
        _BText.text = "" + (int)(c.b * 255);
        _ExponentText.text = (int)(100-100 * SunHaloExponent / 255) + "%";
        _ContributionText.text = (int)(100 * SunHaloContribution) + "%";

        _RSlider.value = (int)(c.r * 255);
        _GSlider.value = (int)(c.g * 255);
        _BSlider.value = (int)(c.b * 255);
        _ExponentSlider.value = SunHaloExponent;
        _ContributionSlider.value = SunHaloContribution;
    }

    public void OnRValueChange(float v)
    {
        _RText.text = "" + (int)v;
        Material theme =  RenderSettings.skybox;
        Color c = theme.GetColor("_HorizonLineColor");
        theme.SetColor("_HorizonLineColor",new Color(v/255,c.g,c.b));
    }
    public void OnGValueChange(float v)
    {
        _GText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;

        Color c = theme.GetColor("_HorizonLineColor");
        theme.SetColor("_HorizonLineColor", new Color(c.r, v/255, c.b));
    }
    public void OnBValueChange(float v)
    {
        _BText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_HorizonLineColor");
        theme.SetColor("_HorizonLineColor", new Color(c.r, c.g, v/255));
    }
    public void OnExponentValueChange(float v)
    {
        _ExponentText.text = (int)(100-100*v/255)+"%";
        Material theme = RenderSettings.skybox;
        theme.SetFloat("_HorizonLineExponent", v);
    }
    public void OnContributionValueChange(float v)
    {
        _ContributionText.text = (int)(100*v)+"%";
        Material theme = RenderSettings.skybox;
        theme.SetFloat("_HorizonLineContribution", v);
    }

    public void OnSaveSetting()
    {
        Material theme = RenderSettings.skybox;
        string data = "";
        Color c = theme.GetColor("_HorizonLineColor");
        float exponent = theme.GetFloat("_HorizonLineExponent");
        float contribution = theme.GetFloat("_HorizonLineContribution");

        data += (int)(c.r * 255) + "#";
        data += (int)(c.g * 255) + "#";
        data += (int)(c.b * 255) + "#";
        data += exponent + "#";
        data += contribution;

        Setting.setPlayerPrefs(theme.name + Setting.SETTING_KEY.DPX_ADJUST_SETTING, data);
    }
}
