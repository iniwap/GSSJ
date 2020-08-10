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

public class ZSParamAdjust : MonoBehaviour
{
    public void Start()
    {

    }

    public void OnInit()
    {
        List<string> cinfo = PickColor.GetColorByID(Setting.GetStartColorID());//上次保存的主题色
        Color bgColor = new Color(int.Parse(cinfo[3]) / 255.0f,int.Parse(cinfo[4]) / 255.0f, int.Parse(cinfo[5]) / 255.0f);
        Color hzColor = Define.GetUIFontColorByBgColor(bgColor, Define.eFontAlphaType.FONT_ALPHA_255);
        Color spColor = Define.DARKBG_SP_COLOR;
        if (Define.GetIfUIFontBlack(bgColor))
        {
            spColor = Define.LIGHTBG_SP_COLOR;
        }

        //初始化调整界面的文字颜色
        SetHSImgColor(spColor,false);
        SetHSImgSize(Define.DEFAULT_HS_SIZE,false);
        //SetHSImg("");//暂不支持行线图

        SetZSHZColor(hzColor,false);
        SetZSImgColor(Define.BG_COLOR_50,false);
        SetZSImgSize(1.0f,false);
        SetZSImg("");

        //点缀

    }

    private ZhuangShiBtnItem.eZSBtnType _CurrentAdjustType;//当前调节的参数类型
    //用于如果没有点击确定的调整复原
    private Color _PrevSPColor;
    private Color _PrevHZColor;
    private Color _PrevHZImgColor;
    private float _PrevSPSize;
    private float _PrevHZImgSize;

    public Text _AdjustTitle;
    public Text _AdjustTitle2;
    public Image _AdjustTitleImg;
    public Text[] HZTextZS;
    public Image[] HZZS;
    public Image[] HZZSSP;
    public Image[] _neenChangeWhithBGColor;
    public Image[] _darkBgColor;
    public Image[] _lightBgColor;

    public void ChangeBGColor(Color c)
    {
        _Bg.color = c;

        Color fc = Define.GetFixColor(c);

        foreach (var img in _neenChangeWhithBGColor)
        {
            img.color = fc;
        }
        foreach (var img in _darkBgColor)
        {
            img.color = Define.GetDarkColor(fc);
        }

        foreach (var img in _lightBgColor)
        {
            img.color = Define.GetLightColor(fc);
        }

        Color ftc = Define.GetUIFontColorByBgColor(c,Define.eFontAlphaType.FONT_ALPHA_128);
        Text[] allText = gameObject.GetComponentsInChildren<Text>(true);
        foreach(var t in allText){
            if(!t.name.Contains("HZ")){
                t.color = ftc;
            }
        }
    }

    public void LineRenderChangeHSAlpha(float a)
    {
        Color c = new Color(_PrevSPColor.r, _PrevSPColor.g, _PrevSPColor.b,a);

        SetHSImgColor(c,false);//强制设置

        _ASlider.value = (int)(c.a * 255);
        _AValue.text = "" + (int)(_ASlider.value);
    }

    //只有当选中的是非字饰按钮时有效，且仅在调节字饰参数时修改
    public void RefreshParamAdjustUI(){
    
        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _AdjustTitle.text = "<b>图饰</b>";
            _AdjustTitle2.text = "文字";
            _AdjustTitleImg.sprite = Resources.Load("icon/heart",typeof(Sprite)) as Sprite;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            _AdjustTitle.text = "<b>字饰</b>";
            _AdjustTitle2.text = "文字";
            _AdjustTitleImg.sprite = Resources.Load("icon/heart", typeof(Sprite)) as Sprite;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            _AdjustTitle.text = "<b>线饰</b>";
            _AdjustTitle2.text = "行线";
            _AdjustTitleImg.sprite = Resources.Load("icon/menu", typeof(Sprite)) as Sprite;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
        {
            _AdjustTitle.text = "<b>图饰</b>";
            _AdjustTitle2.text = "行线";
            _AdjustTitleImg.sprite = Resources.Load("icon/menu", typeof(Sprite)) as Sprite;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.DianZhui)
        {
            _AdjustTitle.text = "<b>缀饰</b>";
            _AdjustTitle2.text = "点缀";
            _AdjustTitleImg.sprite = Resources.Load("icon/rongxuejirongjiechi", typeof(Sprite)) as Sprite;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
        {
            _AdjustTitle.text = "<b>图饰</b>";
            _AdjustTitle2.text = "点缀";
            _AdjustTitleImg.sprite = Resources.Load("icon/rongxuejirongjiechi", typeof(Sprite)) as Sprite;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
        {
            _AdjustTitle.text = "<b>配图</b>";
            _AdjustTitle2.text = "形状";
            _AdjustTitleImg.sprite = Resources.Load("icon/border-outer", typeof(Sprite)) as Sprite;
        }

        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            _sizeAdjust.SetActive(false);
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _sizeAdjust.SetActive(true);
            _sizeSliderTitle.text = "大小";
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            _sizeAdjust.SetActive(true);
            _sizeSliderTitle.text = "宽窄";
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
        {
            _sizeAdjust.SetActive(true);
            _sizeSliderTitle.text = "大小";
        }


        Color c = GetPrevColor();
        _RSlider.value = (int)(c.r * 255);
        _RValue.text = "" + (int)(_RSlider.value);
        _GSlider.value = (int)(c.g * 255);
        _GValue.text = "" + (int)(_GSlider.value);
        _BSlider.value = (int)(c.b * 255);
        _BValue.text = "" + (int)(_BSlider.value);
        _ASlider.value = (int)(c.a * 255);
        _AValue.text = "" + (int)(_ASlider.value);

        _SizeSlider.value = GetPrevSize();
        _SizeValue.text = (int)(100 * _SizeSlider.value) + "%";
    }
    public GameObject _zsParamAdjust;
    public Image _Mask;
    public Button _clickToCloseBtn;
    public Image _Bg;
    public GameObject _sizeAdjust;
    public Text _sizeSliderTitle;

    #region  设置参数调整界面的相关参数
    //设置行线的颜色
    public void SetHSImgColor(Color c,bool adjust = true)
    {
        foreach(var sp in HZZSSP){
            sp.color = c;
        }

        //如果是手动调节参数，不要设置初始值
        if(!adjust){
            _PrevSPColor = c;
        }
    }
    //设置行线的宽窄
    public void SetHSImgSize(float s, bool adjust = true)
    {
        foreach (var sp in HZZSSP)
        {
            RectTransform rt = sp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2((int)s,rt.sizeDelta.y);
        }

        if (!adjust)
        {
            _PrevSPSize = s;
        }
    }
    //设置行线的图片
    public void SetHSImg(string path)
    {
        foreach (var sp in HZZSSP)
        {
            sp.sprite = Resources.Load(path,typeof(Sprite)) as Sprite;
        }
    }

    //设置字的颜色
    public void SetZSHZColor(Color c, bool adjust = true)
    {
        foreach (var hz in HZTextZS)
        {
            hz.color = c;
        }

        if (!adjust)
        {
            _PrevHZColor = c;
        }
    }
    //设置字的底图颜色
    public void SetZSImgColor(Color c, bool adjust = true)
    {
        foreach (var sp in HZZS)
        {
            sp.color = c;
        }

        if (!adjust)
        {
            _PrevHZImgColor = c;
        }
    }
    //设置字的底图
    public void SetZSImg(string path)
    {
        foreach (var sp in HZZS)
        {
            if (path == "")
            {
                //初始化设置成透明
                sp.gameObject.SetActive(false);
            }
            else
            {
                sp.sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
                sp.gameObject.SetActive(true);
            }
        }
    }
    public void SetZSImgSize(float s, bool adjust = true)
    {
        foreach (var sp in HZZS)
        {
            sp.transform.localScale = new Vector3(s,s,1.0f);
        }

        if (!adjust)
        {
            _PrevHZImgSize = s;
        }
    }
    //设置点缀的颜色
    public void SetDZColor(Color c, bool adjust = true)
    {
        foreach (var sp in HZZSSP)
        {
            sp.color = c;
        }
    }
    //设置点缀的大小
    public void SetDZSize(float s, bool adjust = true)
    {
        foreach (var sp in HZZSSP)
        {
            RectTransform rt = sp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2((int)s, rt.sizeDelta.y);
        }
    }
    //设置点缀的图片
    public void SetDZImg(string path)
    {
        foreach (var sp in HZZSSP)
        {
            sp.sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
        }
    }
#endregion

    //在显示调节界面前，需要设置以上参数，否则呈现效果不正确
    public void HideZSParamAdjust()
    {
        ShowZSParamAdjust(false,ZhuangShiBtnItem.eZSBtnType.ZSNone);
    }

    public void ShowZSParamAdjust(bool show, ZhuangShiBtnItem.eZSBtnType btnType)
    {

        if (show == _zsParamAdjust.activeSelf) return;

        Sequence mySequence = DOTween.Sequence();
        if (show)
        {
            _CurrentAdjustType = btnType;

            RefreshParamAdjustUI();
            gameObject.SetActive(false);
            _zsParamAdjust.SetActive(true);
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
                    _zsParamAdjust.SetActive(false);
                });
        }
    }
    public Text _RValue;
    public Text _GValue;
    public Text _BValue;
    public Text _AValue;
    public Text _SizeValue;

    public Slider _RSlider;
    public Slider _GSlider;
    public Slider _BSlider;
    public Slider _ASlider;
    public Slider _SizeSlider;

    public void OnRValueChange(float v)
    {
        Color c0 = GetColor();

        float r = ((int)v)/255.0f;
        _RValue.text ="" + (int)v;
        Color c = new Color(r, c0.g, c0.b, c0.a);
        SetZSColor(c);
    }
    public void OnGValueChange(float v)
    {
        Color c0 = GetColor();

        float g = ((int)v) / 255.0f;
        _GValue.text = "" + (int)v;
        Color c = new Color(c0.r, g, c0.b, c0.a);
        SetZSColor(c);
    }
    public void OnBValueChange(float v)
    {
        Color c0 = GetColor();

        float b = ((int)v) / 255.0f;
        _BValue.text = "" + (int)v;
        Color c = new Color(c0.r, c0.g,b, c0.a);
        SetZSColor(c);
    }
    public void OnAValueChange(float v)
    {
        Color c0 = GetColor();

        float a = ((int)v) / 255.0f;
        _AValue.text = "" + (int)v;
        Color c = new Color(c0.r, c0.g, c0.b, a);
        SetZSColor(c);
    }
    public void OnSizeValueChange(float v)
    {
        _SizeValue.text = (int)(100 * v) +"%";

        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            SetZSImgSize(v);
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            SetHSImgSize(Define.MIN_DEFAULT_HS_SIZE + (int)(v * (Define.MAX_DEFAULT_HS_SIZE - Define.MIN_DEFAULT_HS_SIZE) / 2));
        }
    }
    private void SetZSColor(Color c)
    {
        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            SetZSImgColor(c);
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            SetZSHZColor(c);
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            SetHSImgColor(c);
        }
    }
    [Serializable] public class OnZSParamAdjustFinishEvent : UnityEvent<Color,float> { }
    public OnZSParamAdjustFinishEvent OnZSParamAdjustFinish;

    public void OnFinishClick(){

        ShowZSParamAdjust(false,ZhuangShiBtnItem.eZSBtnType.ZSNone);

        Color c = GetColor();
        float s = GetSize();

        //更新当前值
        if(_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            _PrevHZColor = c;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _PrevHZImgColor = c;
            _PrevHZImgSize = s;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            _PrevSPColor = c;
            _PrevSPSize = s;
        }

        OnZSParamAdjustFinish.Invoke(c,s);
    }

    private Color GetPrevColor()
    {
        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            return _PrevHZImgColor;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            return _PrevHZColor;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            return _PrevSPColor;
        }

        return Define.BG_COLOR_50;// 返回汉字的颜色
    }
    private float GetPrevSize()
    {
        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            return _PrevHZImgSize;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            return (_PrevSPSize - Define.MIN_DEFAULT_HS_SIZE) / (Define.MAX_DEFAULT_HS_SIZE - Define.MIN_DEFAULT_HS_SIZE) * 2;
        }

        return 1.0f;// 汉字不可以调节大小，无意义
    }

    public Color GetColor(){
        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            return HZZS[0].color;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            return HZTextZS[0].color;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            return HZZSSP[0].color;
        }

        return Define.BG_COLOR_50;// 返回汉字的颜色
    }

    public float GetSize(){

        if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            return HZZS[0].transform.localScale.x;
        }
        else if (_CurrentAdjustType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            RectTransform rt = HZZSSP[0].GetComponent<RectTransform>();
            return rt.sizeDelta.x;
        }

        return 1.0f;// 汉字不可以调节大小，无意义
    }

    public ZhuangShiBtnItem.eZSBtnType GetCurrentAdjustType(){
        return _CurrentAdjustType;
    }
}
