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

public class ThemeParamAdjust : MonoBehaviour
{
    public void Start()
    {

    }
    public enum AdjustType
    {
        PosAdjust,
        DPXAdjust,
        TDAdjust,
        TYAdjust,
        YGAdjust,
    }


    public void OnInit()
    {

    }

    public GameObject _ParamAdjust;
    public Image _Mask;
    public Button _clickToCloseBtn;

    public UnityEvent InitAdjust;
    public void ShowAdjust(bool show)
    {

        if (show == _ParamAdjust.activeSelf) return;

        Sequence mySequence = DOTween.Sequence();
        if (show)
        {
            InitAdjust.Invoke();
            gameObject.SetActive(false);
            _ParamAdjust.SetActive(true);
            _Mask.gameObject.SetActive(false);
            mySequence
                .Append(gameObject.transform.DOScale(0.0f, 0.0f))
                .Join(_Mask.DOFade(0.0f, 0.0f))
                .AppendCallback(() => { gameObject.SetActive(true); _Mask.gameObject.SetActive(true); })
                .Append(_Mask.DOFade(100 / 255.0f, 0.3f))
                .Append(gameObject.transform.DOScale(FitUI.GetFitUIScale(), 0.6f))
                .SetEase(Ease.OutBounce);
        }
        else
        {
            _clickToCloseBtn.interactable = false;
            mySequence
                .Append(_Mask.DOFade(0.0f, 0.3f))
                .Join(gameObject.transform.DOScale(0.0f, 0.3f))
                .SetEase(Ease.InSine).OnComplete(() =>
                {
                    gameObject.SetActive(true);
                    _clickToCloseBtn.interactable = true;
                    _Mask.gameObject.SetActive(true);
                    _ParamAdjust.SetActive(false);
                });
        }
    }
    public void HideParamAdjust()
    {
        ShowAdjust(false);
    }

    public UnityEvent _OnSaveSettingEvent;
    public void OnSaveSetting()
    {
        HideParamAdjust();
        _OnSaveSettingEvent.Invoke();

        //给予提示
        int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_SAVE_THEME_SETTING_TIPS, 0);

        if (tipCnt < Define.BASIC_TIP_CNT)
        {
            ShowToast("已经保存过的设置，下次打开<b>不再随机</b>该设置项的效果",3f);
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_SAVE_THEME_SETTING_TIPS, tipCnt + 1);
        }
    }

    public Image _Bg;
    public void ChangeBGColor(Color c)
    {
        _Bg.color = new Color(c.r,c.g,c.b,0.5f);

        Color fc = Define.GetFixColor(c);
        Color darkc = Define.GetDarkColor(fc);
        Color lightc = Define.GetLightColor(fc);
        darkc = new Color(darkc.r, darkc.g, darkc.b,0.5f);
        lightc = new Color(lightc.r, lightc.g, lightc.b, 0.5f);
        fc = new Color(fc.r, fc.g, fc.b, 0.5f);


        Image[] allImg = gameObject.GetComponentsInChildren<Image>(true);
        foreach (var img in allImg)
        {
            if (img.name == "Round Button Ripple" || img.name == "Handle")
            {
                img.color = fc;
            }
            else if (img.name == "Background"
                || img.name.Contains("Dark"))
            {
                img.color = darkc;

            }
            else if (img.name == "Fill"
                || img.name == "Icon"
                || img.name.Contains("Light"))
            {
                img.color = lightc;
            }
        }

        Color ftc = Define.GetUIFontColorByBgColor(c, Define.eFontAlphaType.FONT_ALPHA_128);
        Text[] allText = gameObject.GetComponentsInChildren<Text>(true);
        foreach (var t in allText)
        {
            t.color = ftc;
        }
    }

    public GameObject[] _AdjustList;
    public Text _themeAdjustTitle;
    public void OnAdjustBtnClick(string type)
    {
        for (int i = 0; i <= (int)AdjustType.YGAdjust; i++)
        {
            if ("" + ((AdjustType)i) == type)
            {
                _AdjustList[i].SetActive(true);
            }
            else
            {
                _AdjustList[i].SetActive(false);
            }
        }

        switch (GetAdjustType(type))
        {
            case AdjustType.DPXAdjust:
                _themeAdjustTitle.text = "地平线";
                break;
            case AdjustType.PosAdjust:
                _themeAdjustTitle.text = "方位";
                break;
            case AdjustType.TDAdjust:
                _themeAdjustTitle.text = "天空·大地";
                break;
            case AdjustType.TYAdjust:
                _themeAdjustTitle.text = "太阳·月亮";
                break;
            case AdjustType.YGAdjust:
                _themeAdjustTitle.text = "阳光·月光";
                break;
        }
    }

    private AdjustType GetAdjustType(string type)
    {
        AdjustType at = AdjustType.PosAdjust;
        if (type == "" + AdjustType.PosAdjust)
        {
            at = AdjustType.PosAdjust;
        }
        else if (type == "" + AdjustType.DPXAdjust)
        {
            at = AdjustType.DPXAdjust;
        }
        else if (type == "" + AdjustType.TDAdjust)
        {
            at = AdjustType.TDAdjust;
        }
        else if (type == "" + AdjustType.TYAdjust)
        {
            at = AdjustType.TYAdjust;
        }
        else if (type == "" + AdjustType.YGAdjust)
        {
            at = AdjustType.YGAdjust;
        }
        return at;
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        Color c = _Bg.color;
        Toast.ToastData data;
        data.c = c;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }
}
