using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class ThemeBtnItem : MonoBehaviour
{
    public void Start()
    {

    }

    public struct ThemeParam{

    }

    public GameObject _buyBtn;
    public Text _btnText;
    public Image _btnImg;
    private Material _ThemeMaterial;
    private ThemeParam _CurrentThemParam;
    private Define.eWeekType _week;
    private Define.eHourType _hour;

    private bool _isTitle = false;
    public void Init(bool needBuy,bool isTitle,Define.eWeekType week,Define.eHourType hour,bool hasSetCustom)
    {
        _isTitle = isTitle;
        _week = week;
        _hour = hour;

        if (_isTitle)
        {
            string weekname = Define.GetWeekName(week);

            for (int i = 0;i < weekname.Length;i++)
            {
                if (i != weekname.Length - 1)
                {
                    _btnText.text += weekname[i] + "\n";
                }
                else
                {
                    _btnText.text += weekname[i];
                }
            }

            return;
        }

        _btnText.text = Define.GetHourName(hour);

        _adjustBtn.SetActive(false);
        Image[] adj = _adjustBtn.GetComponentsInChildren<Image>();
        adj[0].gameObject.SetActive(false);
        adj[1].gameObject.SetActive(false);
        adj[2].gameObject.SetActive(false);

        _buyBtn.SetActive(needBuy);

        //关联材质
        _ThemeMaterial = Resources.Load("Theme/Materials/" + hour + (int)week, typeof(Material)) as Material;

        if (week == Define.eWeekType.Custom)
        {
            OnLoadSetting(_ThemeMaterial);
        }
        Color c = _ThemeMaterial.GetColor("_SkyGradientTop");
        _btnImg.color = new Color(c.r,c.g,c.b);
    }

    public GameObject _adjustBtn;
    [Serializable] public class OnItemClickEvent : UnityEvent<Define.eWeekType,Define.eHourType> { }
    public OnItemClickEvent OnItemClick;
    public void OnClickItem()
    {
        OnItemClick.Invoke(_week,_hour);
    }

    public bool GetCanAdjust(){
        return _adjustBtn.activeSelf;
    }

    public void SetSelected(bool can)
    {
        //设置选中
        _adjustBtn.SetActive(can);
        _adjustBtn.transform.Find("Mask").gameObject.SetActive(can);

        if (_week == Define.eWeekType.Custom && !_isTitle)
        {
            if (can)
            {
                _adjustBtn.transform.Find("Slider").gameObject.SetActive(true);
                _adjustBtn.transform.Find("Point").gameObject.SetActive(true);
            }
            else
            {
                _adjustBtn.transform.Find("Slider").gameObject.SetActive(false);
                _adjustBtn.transform.Find("Point").gameObject.SetActive(false);
            }
        }
        else
        {
            _adjustBtn.transform.Find("Slider").gameObject.SetActive(false);
            _adjustBtn.transform.Find("Point").gameObject.SetActive(false);
        }
    }


    public bool GetIfShowingBuyBtn(){
        return _buyBtn.activeSelf;
    }

    public void ShowBuyBtn(bool show)
    {
        if (show && _buyBtn.activeSelf)
        {
            _buyBtn.SetActive(true);
        }
        else
        {
            _buyBtn.SetActive(false);
        }
    }

    public Define.eWeekType GetWeekType()
    {
        return _week;
    }
    public Define.eHourType GetHourType()
    {
        return _hour;
    }
    public bool GetIsTitle()
    {
        return _isTitle;
    }
    public Material GetMaterial()
    {
        return _ThemeMaterial;
    }
    public Color GetSkyTopColor()
    {
        Color SkyGradientTop = _ThemeMaterial.GetColor("_SkyGradientTop");
        SkyGradientTop = new Color(SkyGradientTop.r, SkyGradientTop.g, SkyGradientTop.b,1f);//不使用a

        return SkyGradientTop;
    }
    public Color GetSkyBottomColor()
    {
        Color SkyGradientBottom = _ThemeMaterial.GetColor("_SkyGradientBottom");
        SkyGradientBottom = new Color(SkyGradientBottom.r, SkyGradientBottom.g, SkyGradientBottom.b,1f);//不使用a

        return SkyGradientBottom;
    }

    //------------------加载保存--------------------
    public void OnLoadSetting(Material theme)
    {
        OnLoadTYSetting(theme);
        OnLoadDPXSetting(theme);
        OnLoadYGSetting(theme);
        OnLoadTDSetting(theme);
        OnLoadPosSetting(theme);
    }
    public void OnLoadTYSetting(Material theme)
    {
        Color c;
        float exponent;
        float contribution;

        string data = Setting.getPlayerPrefs(theme.name + Setting.SETTING_KEY.TY_ADJUST_SETTING, "");
        if (data == "")
        {
            c = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            exponent = UnityEngine.Random.value * 50000;
            contribution = UnityEngine.Random.value * 100;
        }
        else
        {
            string[] dt = data.Split('#');
            if (dt.Length != 5) return;

            c = new Color(float.Parse(dt[0])/255, float.Parse(dt[1])/255, float.Parse(dt[2]) / 255);
            exponent = float.Parse(dt[3]);
            contribution = float.Parse(dt[4]);
        }

        theme.SetColor("_SunDiscColor", c);
        theme.SetFloat("_SunDiscExponent", exponent);
        theme.SetFloat("_SunDiscMultiplier", contribution);
    }

    public void OnLoadDPXSetting(Material theme)
    {
        Color c;
        float exponent;
        float contribution;

        string data = Setting.getPlayerPrefs(theme.name + Setting.SETTING_KEY.DPX_ADJUST_SETTING, "");
        if (data == "")
        {
            c = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            exponent = UnityEngine.Random.value * 255;
            contribution = UnityEngine.Random.value;
        }
        else
        {
            string[] dt = data.Split('#');
            if (dt.Length != 5) return;

            c = new Color(float.Parse(dt[0]) / 255, float.Parse(dt[1]) / 255, float.Parse(dt[2]) / 255);
            exponent = float.Parse(dt[3]);
            contribution = float.Parse(dt[4]);
        }

        theme.SetColor("_HorizonLineColor", c);
        theme.SetFloat("_HorizonLineExponent", exponent);
        theme.SetFloat("_HorizonLineContribution", contribution);
    }

    public void OnLoadYGSetting(Material theme)
    {
        Color c;
        float exponent;
        float contribution;

        string data = Setting.getPlayerPrefs(theme.name + Setting.SETTING_KEY.YG_ADJUST_SETTING, "");
        if (data == "")
        {
            c = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            exponent = UnityEngine.Random.value * 1000;
            contribution = UnityEngine.Random.value;
        }
        else
        {
            string[] dt = data.Split('#');
            if (dt.Length != 5) return;

            c = new Color(float.Parse(dt[0]) / 255, float.Parse(dt[1]) / 255, float.Parse(dt[2]) / 255);
            exponent = float.Parse(dt[3]);
            contribution = float.Parse(dt[4]);
        }

        theme.SetColor("_SunHaloColor", c);
        theme.SetFloat("_SunHaloExponent", exponent);
        theme.SetFloat("_SunHaloContribution", contribution);
    }
    public void OnLoadTDSetting(Material theme)
    {
        Color tc;
        Color dc;
        float exponent;

        string data = Setting.getPlayerPrefs(theme.name + Setting.SETTING_KEY.TD_ADJUST_SETTING, "");
        if (data == "")
        {
            tc = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            dc = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            exponent = UnityEngine.Random.value * 100;
        }
        else
        {
            string[] dt = data.Split('#');
            if (dt.Length != 7) return;

            tc = new Color(float.Parse(dt[0]) / 255, float.Parse(dt[1]) / 255, float.Parse(dt[2]) / 255);
            dc = new Color(float.Parse(dt[3]) / 255, float.Parse(dt[4]) / 255, float.Parse(dt[5]) / 255);
            exponent = float.Parse(dt[6]);
        }

        if (theme.name.Contains("Morning"))
        {
            tc *= 0.5f;
            dc *= 0.4f;
        }
        else if (theme.name.Contains("Day"))
        {
            tc *= 1.8f;
            dc *= 1.5f;
        }
        else if (theme.name.Contains("Sunset"))
        {
            tc *= 0.7f;
            dc *= 0.6f;
        }
        else if (theme.name.Contains("Night"))
        {
            tc *= 0.2f;
            dc *= 0.1f;
        }

        theme.SetColor("_SkyGradientTop", tc);
        theme.SetColor("_SkyGradientBottom", dc);
        theme.SetFloat("_SkyGradientExponent", exponent);
    }

    public void OnLoadPosSetting(Material theme)
    {
        float camreaRX;
        float camreaRZ;
        float lightRX;
        float lightRY;

        string data = Setting.getPlayerPrefs(theme.name + Setting.SETTING_KEY.POS_ADJUST_SETTING, "");
        if (data == "")
        {
            //随机
            camreaRX = -UnityEngine.Random.value * 10 - 10;
            camreaRZ = UnityEngine.Random.value *100 - 50;
            lightRX = UnityEngine.Random.value * 30 - 15;
            lightRY = UnityEngine.Random.value * 36 + 162;
        }
        else
        {
            string[] dt = data.Split('#');
            if (dt.Length != 4) return;

            camreaRX = float.Parse(dt[0]);
            camreaRZ = float.Parse(dt[1]);
            lightRX = float.Parse(dt[2]);
            lightRY = float.Parse(dt[3]);
        }

        theme.SetFloat("_CameraRX", camreaRX);
        theme.SetFloat("_CameraRZ", camreaRZ);
        theme.SetFloat("_LigthRX", lightRX);
        theme.SetFloat("_LightRY", lightRY);
    }
}
