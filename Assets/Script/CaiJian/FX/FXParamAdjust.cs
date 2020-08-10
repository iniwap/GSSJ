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

public class FXParamAdjust : MonoBehaviour
{
    public void Start()
    {

    }

    private FXLineRender.LineType _CurrentAdjustLineType;//当前调节的参数类型
    //用于如果没有点击确定的调整复原
    private float _PrevHZAlphaRange = 0.0f;
    private float _PrevHZSizeRange = 0.0f;
    private float _PrevRandomRange = 0.0f;
    private float _PrevLineAlphaRange = 0.0f;
    private float _PrevLineSizeRange = 0.0f;

    public Image _AdjustTitleImg;
    public Text[] HZTextZS;
    public Image[] HZZS;
    public GameObject []HZZSSP;
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

    private CJHZ.HZParam _CurrentLineHZParam;
    private float _CurrentLineHZSize;
    //只有当选中的是非字饰按钮时有效，且仅在调节字饰参数时修改
    public void RefreshParamAdjustUI()
    {
        _AdjustTitleImg.sprite = Resources.Load("FX/Icon/" + _CurrentAdjustLineType,typeof(Sprite)) as Sprite;

        _HZAlphaRangeSlider.value = _PrevHZAlphaRange;
        _HZAlphaRangeValue.text = (int)(100 * _HZAlphaRangeSlider.value) + "%";

        _HZSizeRangeSlider.value = _PrevHZSizeRange;
        _HZSizeRangeValue.text = (int)(100 * _HZSizeRangeSlider.value) + "%";

        _LineAlphaRangeSlider.value = _PrevLineAlphaRange;
        _LineAlphaRangeValue.text = (int)(100 * _LineAlphaRangeSlider.value) + "%";

        _LineSizeRangeSlider.value = _PrevLineSizeRange;
        _LineSizeRangeValue.text = (int)(100 * _LineSizeRangeSlider.value) + "%";

        _RandomRangeSlider.value = _PrevRandomRange;
        _RandomRangeValue.text = (int)(100 * _RandomRangeSlider.value) + "%";


        //汉字 - 透明度、大小 + 随机性
        SetHZAlphaRange(_PrevHZAlphaRange, _CurrentLineHZParam.zsHZColor);
        SetHZSizeRange(_PrevHZSizeRange,_CurrentLineHZSize);
        //线 - 透明度、大小 + 随机性
        SetLineAlphaRange(_PrevLineAlphaRange, _CurrentLineHZParam.hsImgColor);
        SetLineSizeRange(_PrevLineSizeRange, _CurrentLineHZParam.hsImgSize);
        SetRandomRange(_PrevRandomRange);

        bool useZS = _CurrentLineHZParam.zsImgPath != "";
        foreach (var zs in HZZS)
        {
            if (useZS)
            {
                zs.sprite = Resources.Load(_CurrentLineHZParam.zsImgPath, typeof(Sprite)) as Sprite;
                zs.gameObject.SetActive(true);
                zs.color = _CurrentLineHZParam.zsImgColor;
                zs.transform.localScale = new Vector3(_CurrentLineHZParam.zsImgSize, _CurrentLineHZParam.zsImgSize, 1f);

            }
            else
            {
                zs.gameObject.SetActive(false);
                zs.color = _CurrentLineHZParam.zsImgColor;
                zs.transform.localScale = new Vector3(_CurrentLineHZParam.zsImgSize, _CurrentLineHZParam.zsImgSize, 1f);

            }
        }
    }

    private void SetHZAlphaRange(float v,Color zsHZColor)
    {
        bool back = false;
        if (v < 0) back = true;

        int a0_pos = (int)(Mathf.Abs(v) * 4);

        float aInv0 = 0;
        float aInv1 = 0f;
        if (a0_pos == HZTextZS.Length)
        {
            aInv1 = zsHZColor.a / HZTextZS.Length;
        }
        else if (a0_pos == 0)
        {

        }
        else
        {
            aInv0 = zsHZColor.a / a0_pos;
            aInv1 = zsHZColor.a / (HZTextZS.Length - a0_pos);
        }

        for (int i = 0; i < HZTextZS.Length; i++)
        {
            if (a0_pos == 0)
            {
                HZTextZS[i].color = zsHZColor;
            }
            else if (a0_pos == HZTextZS.Length)
            {
                if (back)
                {
                    HZTextZS[i].color = new Color(zsHZColor.r, zsHZColor.g, zsHZColor.b, Define.MIN_LINE_HZ_ALPHA + zsHZColor.a - i * aInv1);
                }
                else
                {
                    HZTextZS[i].color = new Color(zsHZColor.r, zsHZColor.g, zsHZColor.b, Define.MIN_LINE_HZ_ALPHA + i * aInv1);
                }
            }
            else
            {
                if (i < a0_pos)
                {
                    if (back)
                    {
                        HZTextZS[i].color = new Color(zsHZColor.r, zsHZColor.g, zsHZColor.b, Define.MIN_LINE_HZ_ALPHA + i * aInv0);

                    }
                    else
                    {
                        HZTextZS[i].color = new Color(zsHZColor.r, zsHZColor.g, zsHZColor.b, Define.MIN_LINE_HZ_ALPHA + zsHZColor.a - i * aInv0);
                    }
                }
                else
                {
                    if (back)
                    {
                        HZTextZS[i].color = new Color(zsHZColor.r, zsHZColor.g, zsHZColor.b, Define.MIN_LINE_HZ_ALPHA + zsHZColor.a - (i + 1 - a0_pos) * aInv1);
                    }
                    else
                    {
                        HZTextZS[i].color = new Color(zsHZColor.r, zsHZColor.g, zsHZColor.b, Define.MIN_LINE_HZ_ALPHA + (i + 1 - a0_pos) * aInv1);

                    }
                }
            }
        }
    }

    private void SetHZSizeRange(float v,float hzSize)
    {

        bool back = false;
        if (v < 0) back = true;

        int s0_pos = (int)(Mathf.Abs(v) * HZTextZS.Length);

        float sInv0 = 0;
        float sInv1 = 0f;
        if (s0_pos == HZTextZS.Length)//全部依次降低
        {
            sInv1 = hzSize / HZTextZS.Length;
        }
        else if(s0_pos == 0)//保存原样
        {
        }
        else
        {
            sInv0 = hzSize / s0_pos;
            sInv1 = hzSize / (HZTextZS.Length - s0_pos);
        }

        for (int i = 0; i < HZTextZS.Length; i++)
        {
            //所有的字要设置为当前调节的实际大小
            RectTransform rt = HZTextZS[i].GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(hzSize,hzSize);

            if (s0_pos == 0)
            {
                HZTextZS[i].transform.localScale = new Vector3(1f, 1f, 1.0f);
            }
            else if (s0_pos == HZTextZS.Length)
            {
                if (back)
                {
                    HZTextZS[i].transform.localScale = new Vector3(Define.MIN_LINE_HZ_SIZE + 1 - i * sInv1 / hzSize,
                        Define.MIN_LINE_HZ_SIZE + 1 - i * sInv1 / hzSize, 1.0f);
                }
                else
                {
                    HZTextZS[i].transform.localScale = new Vector3(Define.MIN_LINE_HZ_SIZE + i * sInv1 / hzSize,
                        Define.MIN_LINE_HZ_SIZE + i * sInv1 / hzSize, 1.0f);
                }

            }
            else
            {
                if (i < s0_pos)
                {
                    if (back)
                    {

                        HZTextZS[i].transform.localScale = new Vector3(Define.MIN_LINE_HZ_SIZE + 1 - (hzSize - i * sInv0) / hzSize,
                            Define.MIN_LINE_HZ_SIZE + 1 - (hzSize - i * sInv0) / hzSize, 1.0f);
                    }
                    else
                    {

                        HZTextZS[i].transform.localScale = new Vector3(Define.MIN_LINE_HZ_SIZE + (hzSize - i * sInv0) / hzSize
                            , Define.MIN_LINE_HZ_SIZE + (hzSize - i * sInv0) / hzSize, 1.0f);
                    }
                }
                else
                {
                    if (back)
                    {
                        HZTextZS[i].transform.localScale = new Vector3(Define.MIN_LINE_HZ_SIZE + 1 - (i + 1 - s0_pos) * sInv1 / hzSize,
                            Define.MIN_LINE_HZ_SIZE + 1 - (i + 1 - s0_pos) * sInv1 / hzSize, 1.0f);
                    }
                    else
                    {
                        HZTextZS[i].transform.localScale = new Vector3(Define.MIN_LINE_HZ_SIZE + (i + 1 - s0_pos) * sInv1 / hzSize,
                            Define.MIN_LINE_HZ_SIZE + (i + 1 - s0_pos) * sInv1 / hzSize, 1.0f);
                    }
                }
            }
        }
    }

    private void SetLineAlphaRange(float v,Color hsImgColor)
    {
        List<Image> lineSP = new List<Image>();
        foreach (var hzsp in HZZSSP)
        {
            Image[] sps = hzsp.GetComponentsInChildren<Image>();
            lineSP.AddRange(sps);
        }

        bool back = false;
        if (v < 0) back = true;

        float minAlpha = (1 - Mathf.Abs(v)) * hsImgColor.a;

        float aInv0 = 0;
        aInv0 = (hsImgColor.a - minAlpha) / lineSP.Count;

        for (int i = 0; i < lineSP.Count; i++)
        {
            if (back)
            {
                lineSP[i].color = new Color(hsImgColor.r, hsImgColor.g, hsImgColor.b, i * aInv0);
            }
            else
            {
                lineSP[i].color = new Color(hsImgColor.r, hsImgColor.g, hsImgColor.b, hsImgColor.a - i * aInv0);
            }
        }
    }

    private void SetLineSizeRange(float v,float hsImgSize)
    {
        List<Image> lineSP = new List<Image>();
        foreach (var hzsp in HZZSSP)
        {
            Image[] sps = hzsp.GetComponentsInChildren<Image>();
            lineSP.AddRange(sps);
        }

        bool back = false;
        if (v < 0) back = true;


        float minSize = (1 - Mathf.Abs(v)) * hsImgSize;

        float sInv0 = 0;
        sInv0 = (hsImgSize - minSize) / lineSP.Count;

        for (int i = 0; i < lineSP.Count; i++)
        {
            RectTransform rt = lineSP[i].GetComponent<RectTransform>();
            if (back)
            {
                rt.sizeDelta = new Vector2(i * sInv0, rt.sizeDelta.y);
            }
            else
            {
                rt.sizeDelta = new Vector2(hsImgSize - i * sInv0, rt.sizeDelta.y);
            }

        }
    }

    private void SetRandomRange(float v)
    {
        foreach (var sp in HZZS)
        {
            float s = sp.transform.localScale.x;
            //0 - >1,1 -> 0
            float r = HZManager.GetInstance().GenerateRandomInt((int)((1 - v)*100), 100) / 100f;
            sp.transform.localScale = new Vector3(_CurrentLineHZParam.zsImgSize * r, _CurrentLineHZParam.zsImgSize * r,1f);
        }
    }

    public GameObject _zsParamAdjust;
    public Image _Mask;
    public Button _clickToCloseBtn;
    public Image _Bg;

    //在显示调节界面前，需要设置以上参数，否则呈现效果不正确
    public void HideFXParamAdjust()
    {
        if (!_zsParamAdjust.activeSelf) return;

        Sequence mySequence = DOTween.Sequence();
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

    public void ShowFXParamAdjust(FXLineRender.LineType type, CJHZ.HZParam p,float hzSize)
    {

        if (_zsParamAdjust.activeSelf) return;

        Sequence mySequence = DOTween.Sequence();

        _CurrentAdjustLineType = type;

        _CurrentLineHZParam = p;
        _CurrentLineHZSize = hzSize;
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

    public Text _HZAlphaRangeValue;
    public Text _HZSizeRangeValue;
    public Text _LineAlphaRangeValue;
    public Text _LineSizeRangeValue;
    public Text _RandomRangeValue;

    public Slider _HZAlphaRangeSlider;
    public Slider _HZSizeRangeSlider;
    public Slider _LineAlphaRangeSlider;
    public Slider _LineSizeRangeSlider;
    public Slider _RandomRangeSlider;

    public void OnHZAlphaRangeValueChange(float v)
    {
        int iv = (int)(100 * v); 
        _HZAlphaRangeValue.text = iv + "%";
        //更新预览显示效果
        SetHZAlphaRange(iv / 100f, _CurrentLineHZParam.zsHZColor);
    }
    public void OnHZSizeRangeValueChange(float v)
    {
        int iv = (int)(100 * v);
        _HZSizeRangeValue.text = iv + "%";
        SetHZSizeRange(iv / 100f, _CurrentLineHZSize);
        //更新预览显示效果
    }
    public void OnLineAlphaRangeValueChange(float v)
    {
        int iv = (int)(100 * v);
        _LineAlphaRangeValue.text = iv + "%";
        SetLineAlphaRange(iv / 100f,_CurrentLineHZParam.hsImgColor);
        //更新预览显示效果
    }
    public void OnLineSizeRangeValueChange(float v)
    {
        int iv = (int)(100 * v);
        _LineSizeRangeValue.text = iv + "%";

        SetLineSizeRange(iv / 100f, _CurrentLineHZParam.hsImgSize);
        //更新预览显示效果
    }
    public void OnRandomRangeValueChange(float v)
    {
        int iv = (int)(100 * v);

        _RandomRangeValue.text = iv + "%";
        //更新预览显示效果
        SetRandomRange(iv/100f);

        if (_CurrentLineHZParam.zsImgPath == "")
        {
            ShowToast("仅当使用了文字底图时有效，可在<b>装饰</b>里选择底图");
        }
    }

    [Serializable] public class OnFXParamAdjustFinishEvent : UnityEvent<FXBtnItem.FXParam> { }
    public OnFXParamAdjustFinishEvent OnFXParamAdjustFinish;
    public void OnFinishClick(){

        //将prev值更新为最新的
        _PrevHZAlphaRange = _HZAlphaRangeSlider.value;
        _PrevHZSizeRange = _HZSizeRangeSlider.value;
        _PrevLineAlphaRange = _LineAlphaRangeSlider.value;
        _PrevLineSizeRange = _LineSizeRangeSlider.value;
        _PrevRandomRange = _RandomRangeSlider.value;

        HideFXParamAdjust();
        FXBtnItem.FXParam p;

        p.lineType = _CurrentAdjustLineType;
        p.HZAlphaRange = _PrevHZAlphaRange;
        p.HZSizeRange = _PrevHZSizeRange;
        p.LineAlphaRange = _PrevLineAlphaRange;
        p.LineSizeRange = _PrevLineSizeRange;
        p.RandomRange = _PrevRandomRange;// 

        OnFXParamAdjustFinish.Invoke(p);
    }

    public FXLineRender.LineType GetCurrentAdjustType(){
        return _CurrentAdjustLineType;
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = _Bg.color;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }
}
