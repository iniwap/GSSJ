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

public class TDAdjust : MonoBehaviour
{
    public void Start()
    {

    }

    public Text _TRText;
    public Text _TGText;
    public Text _TBText;
    public Text _DRText;
    public Text _DGText;
    public Text _DBText;
    public Text _ExponentText;
    public Slider _TRSlider;
    public Slider _TGSlider;
    public Slider _TBSlider;
    public Slider _DRSlider;
    public Slider _DGSlider;
    public Slider _DBSlider;
    public Slider _ExponentSlider;

    public void InitAdjust()
    {
        Material theme = RenderSettings.skybox;
        if (!theme.HasProperty("_SkyGradientTop"))
        {
            return;
        }

        Color tc = theme.GetColor("_SkyGradientTop");
        Color dc = theme.GetColor("_SkyGradientBottom");
        float Exponent = theme.GetFloat("_SkyGradientExponent");

        _TRText.text = "" + (int)(tc.r * 255);
        _TGText.text = "" + (int)(tc.g * 255);
        _TBText.text = "" + (int)(tc.b * 255);
        _DRText.text = "" + (int)(dc.r * 255);
        _DGText.text = "" + (int)(dc.g * 255);
        _DBText.text = "" + (int)(dc.b * 255);
        _ExponentText.text = (int)Exponent + "%";

        _TRSlider.value = (int)(tc.r * 255);
        _TGSlider.value = (int)(tc.g * 255);
        _TBSlider.value = (int)(tc.b * 255);
        _DRSlider.value = (int)(dc.r * 255);
        _DGSlider.value = (int)(dc.g * 255);
        _DBSlider.value = (int)(dc.b * 255);
        _ExponentSlider.value = Exponent;
    }

    public void OnTRValueChange(float v)
    {
        _TRText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SkyGradientTop");
        theme.SetColor("_SkyGradientTop", new Color(v/255, c.g, c.b));
    }
    public void OnTGValueChange(float v)
    {
        _TGText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;

        Color c = theme.GetColor("_SkyGradientTop");
        theme.SetColor("_SkyGradientTop", new Color(c.r, v/255, c.b));
    }
    public void OnTBValueChange(float v)
    {
        _TBText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SkyGradientTop");
        theme.SetColor("_SkyGradientTop", new Color(c.r, c.g, v/255));
    }

    public void OnDRValueChange(float v)
    {
        _DRText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SkyGradientBottom");
        theme.SetColor("_SkyGradientBottom", new Color(v/255, c.g, c.b));
    }
    public void OnDGValueChange(float v)
    {
        _DGText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;

        Color c = theme.GetColor("_SkyGradientBottom");
        theme.SetColor("_SkyGradientBottom", new Color(c.r, v/255, c.b));
    }
    public void OnDBValueChange(float v)
    {
        _DBText.text = "" + (int)v;
        Material theme = RenderSettings.skybox;
        Color c = theme.GetColor("_SkyGradientBottom");
        theme.SetColor("_SkyGradientBottom", new Color(c.r, c.g, v/255));
    }

    public void OnExponentValueChange(float v)
    {
        _ExponentText.text = (int)v+"%";
        Material theme = RenderSettings.skybox;
        theme.SetFloat("_SkyGradientExponent", v);
    }

    public void OnSaveSetting()
    {
        Material theme = RenderSettings.skybox;
        string data = "";
        Color tc = theme.GetColor("_SkyGradientTop");
        Color dc = theme.GetColor("_SkyGradientBottom");
        float exponent = theme.GetFloat("_SkyGradientExponent");

        data += (int)(tc.r * 255) + "#";
        data += (int)(tc.g * 255) + "#";
        data += (int)(tc.b * 255) + "#";
        data += (int)(dc.r * 255) + "#";
        data += (int)(dc.g * 255) + "#";
        data += (int)(dc.b * 255) + "#";

        data += exponent;
        Setting.setPlayerPrefs(theme.name + Setting.SETTING_KEY.YG_ADJUST_SETTING,data);
    }
}
