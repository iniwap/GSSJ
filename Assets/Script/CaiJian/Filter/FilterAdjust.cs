using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Reign;
using System;
using ImageEffects;
using UnityEngine.Rendering.PostProcessing;

public class FilterAdjust : MonoBehaviour
{

    public enum AdjustDirectionType
    {
        NONE,
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        LEFT_UP,
    }

    public void Start()
    {

        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;
        float ipad = FitUI.GetIsPad() ? 0.9f : 1.0f;

        RectTransform rt = gameObject.transform.Find("FilterAdjust").GetComponent<RectTransform>();
        nw = nw * ipad - 56;

        rt.localScale = new Vector3(nw / rt.sizeDelta.y, nw / rt.sizeDelta.y,1.0f);
    }

    public Camera _ImageEffectCamera;
    public PostProcessVolume _ImageEffectPostProcessVolume;
    public GameObject _Water;
    public GameObject _RainGlass;
    public GameObject _Frost;
    public GameObject _LightRays;
    private ImageFilter.ImageEffectType _CurrentFilter = ImageFilter.ImageEffectType.None;

    public GameObject _AdjustContent;
    public Image _ResetBtn;
    public void OnChageBGColor(Color c)
    {
        Text[] allTexts = gameObject.GetComponentsInChildren<Text>(true);
        Color tc = Define.GetUIFontColorByBgColor(c, Define.eFontAlphaType.FONT_ALPHA_128);
        foreach (var t in allTexts)
        {
            t.color = tc;
        }
        _Bg.color = new Color(c.r,c.g,c.b,200/255f);


        Color fc = Define.GetFixColor(c);
        Color darkColor = Define.GetDarkColor(fc);
        Color lightColor = Define.GetLightColor(fc);

        Image[] AllImgs = _AdjustContent.GetComponentsInChildren<Image>(true);
        foreach (var img in AllImgs)
        {
            if (img.name == "Round Button Ripple" || img.name == "Handle")
            {
                img.color = fc;
            }
            else if (img.name.Contains("Dark"))
            {
                img.color = darkColor;
            }
            else if (img.name.Contains("Light"))
            {
                img.color = lightColor;
            }
        }

        _ResetBtn.color = Define.GetUIFontColorByBgColor(c, Define.eFontAlphaType.FONT_ALPHA_128);
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

    public void CloseFilterAdjust()
    {
        ShowAdjust("",false);
    }

    public Image _Bg;
    public GameObject _leanTouch;
    public Text _title;
    // 设置当前滤镜类型
    public void SetCurrentAdjust(ImageFilter.ImageEffectType type)
    {
        _CurrentFilter = type;
    }
    public void ShowAdjust(string title,bool open)
    {
        Transform obj = _AdjustContent.transform.Find("" + _CurrentFilter);
        if (open && obj == null)
        {
            return;
        }

        Sequence mySequence = DOTween.Sequence();

        float toX = 0;

        GameObject likes = gameObject.transform.Find("FilterAdjust").gameObject;
        RectTransform rt = likes.GetComponent<RectTransform>();

        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;

        if (open)
        {
            SetParam(_CurrentFilter);
            //设置参数面板标题
            _title.text = title;
            //打开动画
            _leanTouch.SetActive(false);
            gameObject.SetActive(true);
           
            likes.transform.localPosition = new Vector3(nw/2 + rt.rect.width * rt.localScale.x,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = nw / 2;
            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 1.0f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    //
                });

        }
        else
        {
            //关闭动画
            likes.transform.localPosition = new Vector3(nw / 2,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = nw / 2 + rt.rect.width * rt.localScale.x;

            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _leanTouch.SetActive(true);
                    gameObject.SetActive(false);
                });
        }
    }

    [Serializable] public class OnEnableCameraRenderEvent : UnityEvent<float> { }
    public OnEnableCameraRenderEvent OnEnableCameraRender;
    //参数调节不存在主动开启摄像机渲染的情况
    private void EnableCameraRender(float delay)
    {
        // 首先显示渲染，之后再禁用
        OnEnableCameraRender.Invoke(delay);
    }

    //----------------------------参数调节处理部分----------------------------------
    public void ResetParam()
    {
        switch(_CurrentFilter)
        {
            case ImageFilter.ImageEffectType.Ascii:
                Ascii ac = _ImageEffectCamera.GetComponent<Ascii>();
                ac.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.BlackWhite:
                ResetColorGrading();
                break;
            case ImageFilter.ImageEffectType.Bloom:
                ResetBloom();
                break;
            case ImageFilter.ImageEffectType.Blur:
                BlurEffect be = _ImageEffectCamera.GetComponent<BlurEffect>();
                be.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.ColorGrading:
                ResetColorGrading();
                break;
            case ImageFilter.ImageEffectType.CRT:
                CRT crt = _ImageEffectCamera.GetComponent<CRT>();
                crt.ResetDefault();
                break;
            case ImageFilter.ImageEffectType.Distortion:
                Distortion dt = _ImageEffectCamera.GetComponent<Distortion>();
                dt.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Division:
                Division dv = _ImageEffectCamera.GetComponent<Division>();
                dv.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Drunk:
                Drunk dk = _ImageEffectCamera.GetComponent<Drunk>();
                dk.ResetDefault();
                break;
            case ImageFilter.ImageEffectType.Film:
                ResetColorGrading();
                break;
            case ImageFilter.ImageEffectType.Frost:
                Material fst = _Frost.GetComponent<MeshRenderer>().materials[0];
                fst.SetFloat("_Opacity", 0.5f);//此处默认值固定了，暂时无法同步编辑器
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.FrostedGlass:
                FrostedGlass fg = _ImageEffectCamera.GetComponent<FrostedGlass>();
                fg.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.GameBoy:
                GameBoy gb = _ImageEffectCamera.GetComponent<GameBoy>();
                gb.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Halftoning:
                HalftoneEffect ht = _ImageEffectCamera.GetComponent<HalftoneEffect>();
                ht.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Mosaic:
                Mosaic msc = _ImageEffectCamera.GetComponent<Mosaic>();
                msc.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.OldTV:
                OldTV ot = _ImageEffectCamera.GetComponent<OldTV>();
                ot.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.RainGlass:
                Material rg = _RainGlass.GetComponent<MeshRenderer>().materials[0];
                rg.SetFloat("_Opacity", 1.0f);
                rg.SetFloat("_BumpFactor", 1.0f);
                rg.SetFloat("_Blend", 0.01f);
                break;
            case ImageFilter.ImageEffectType.Sandstorm:
                Sandstorm ss = _ImageEffectCamera.GetComponent<Sandstorm>();
                ss.ResetDefault();
                break;
            case ImageFilter.ImageEffectType.Tonemapping:
                ImageEffects.Tonemapper f = _ImageEffectCamera.GetComponent<ImageEffects.Tonemapper>();
                f.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Water:
                Water w = _Water.GetComponent<Water>();
                w.ResetDefault();
                break;
            case ImageFilter.ImageEffectType.WaterColor:
                Watercolor wc = _ImageEffectCamera.GetComponent<Watercolor>();
                wc.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Technicolor:
                Technicolor th;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out th);
                th.brightness.Override(1.5f);
                th.saturation.Override(1f);
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Ramp:
                Ramp rmp;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out rmp);
                rmp.opacity.Override(1f);
                rmp.angle.Override(45);
                rmp.color1.Override(Color.blue);
                rmp.color2.Override(Color.red);
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.PPBloom:
                Bloom bl;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out bl);
                bl.intensity.Override(30);
                bl.softKnee.Override(0.5f);
                bl.diffusion.Override(7f);
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Vignette:
                Vignette vt;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out vt);
                vt.center.Override(new Vector2(0.5f, 0.5f));
                vt.smoothness.Override(1f);
                vt.roundness.Override(0.5f);
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Streak:
                Streak sk;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out sk);
                sk.threshold.Override(0.5f);
                sk.tint.Override(new Color(1f,192/255f,203/255f));
                sk.stretch.Override(1);
                sk.intensity.Override(0.5f);
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.MotionBlur:
                //无参数可调
                MotionBlur mb;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out mb);
                mb.sampleCount.Override(4);

                MotionBlurEffect mbe = _ImageEffectCamera.GetComponent<MotionBlurEffect>();
                mbe.ResetDefault();

                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Grayscale:
                Grayscale gc;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out gc);
                gc.blend.Override(0.75f);
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.Pencil:
                Pencil pl;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out pl);
                pl.sensivity.Override(20f);
                pl.gradThreshold.Override(0.01f);
                pl.colorThreshold.Override(1f);
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.OilPaint:
                OilPaint op;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out op);
                op.radius.Override(10f);
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.Cartoon:
                Cartoon ct;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ct);
                ct.power.Override(3f);
                ct.edgeSlope.Override(0.5f);
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.FakeHDR:
                FakeHDR fh;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out fh);
                fh.power.Override(5f);
                fh.radius1.Override(3f);
                fh.radius2.Override(0f);
                EnableCameraRender(0.0f);
                break;

            case ImageFilter.ImageEffectType.AnalogGlitch:
                AnalogGlitch ag;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ag);
                ag.scanLineJitter.Override(0f);
                ag.verticalJump.Override(0f);
                ag.horizontalShake.Override(0f);
                ag.colorDrift.Override(0.5f);
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.DigitalGlitch:
                DigitalGlitch dg;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out dg);
                dg.intensity.Override(0.5f);
                EnableCameraRender(0.0f);
                break;
            case ImageFilter.ImageEffectType.ChromaticAberration:
                ChromaticAberration ca;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ca);
                ca.intensity.Override(30f);
                EnableCameraRender(0.2f);
                break;
            case ImageFilter.ImageEffectType.SpriteGlow:
                SpriteGlowEffect sge = _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>();
                sge.ResetDefault();
                EnableCameraRender(0.0f);
                break;

            case ImageFilter.ImageEffectType.Fog:
                D2FogsNoiseTexPE fog = _ImageEffectCamera.GetComponent<D2FogsNoiseTexPE>();
                fog.ResetDefault();
                break;
            case ImageFilter.ImageEffectType.DeferredNV:
                DeferredNightVision dnv = _ImageEffectCamera.GetComponent<DeferredNightVision>();
                dnv.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Ice:
                Ice ice = _ImageEffectCamera.GetComponent<Ice>();
                ice.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.LightRays:
                LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
                lr.ResetDefault();
                SetLightRays(LightRays2D.LightRaysParamType.NONE);
                break;
            case ImageFilter.ImageEffectType.ColorASCII:
                ColorASCII cas = _ImageEffectCamera.GetComponent<ColorASCII>();
                cas.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.Comic:
                Comic cc = _ImageEffectCamera.GetComponent<Comic>();
                cc.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
                /*效率太低，无法支持参数调整
            case ImageFilter.ImageEffectType.Notebook:
                Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
                nb.ResetDefault();
                SetNotebook(Notebook.NotebookParamType.NONE);
                EnableCameraRender(0.0f);
                break;
                */
            case ImageFilter.ImageEffectType.Vortex:
                Vortex vx = _ImageEffectCamera.GetComponent<Vortex>();
                vx.ResetDefault();
                EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                break;
            case ImageFilter.ImageEffectType.ThermalVision:
                ThermalVision tv;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out tv);
                tv.midColor.Override(Color.yellow);
                EnableCameraRender(0.0f);
                break;
        }
    }

    public void SetParam(ImageFilter.ImageEffectType type)
    {

        for (int i = (int)ImageFilter.ImageEffectType.None; i < (int)ImageFilter.ImageEffectType.End; i++)
        {
            Transform obj = _AdjustContent.transform.Find("" + (ImageFilter.ImageEffectType)i);
            if(obj != null)
            {
                //存在该滤镜的【详细参数】设置，则隐藏
                if((int)type == i)
                {
                    //设置内容大小
                    int adjustParamCnt = 0;
                    obj.gameObject.SetActive(true);
                    //初始化该滤镜相关参数
                    switch((ImageFilter.ImageEffectType)i)
                    {
                        case ImageFilter.ImageEffectType.BlackWhite:
                            SetColorGrading();
                            adjustParamCnt = (int)(BlackWhite.BlackWhiteParamType.END) - 1 + 3/*标题*/;
                            break;
                        case ImageFilter.ImageEffectType.Bloom:
                            adjustParamCnt = (int)(BlurEffect.BloomParamType.END) - 1 + 2/*标题*/;
                            SetBloom();
                            break;
                        case ImageFilter.ImageEffectType.ColorGrading:
                            SetColorGrading();
                            adjustParamCnt = (int)(ImageEffects.ColorGrading.ColorGradingParamType.END) - 1 + 3/*标题*/;
                            break;
                        case ImageFilter.ImageEffectType.Film:
                            SetColorGrading();
                            adjustParamCnt = (int)(Film.FilmParamType.END) - 1 + 3/*标题*/;
                            break;
                        case ImageFilter.ImageEffectType.LightRays:
                            SetLightRays(LightRays2D.LightRaysParamType.NONE);
                            adjustParamCnt = (int)(LightRays2D.LightRaysParamType.END) - 1 + 3/*标题*/;
                            break;
                        case ImageFilter.ImageEffectType.Notebook:
                            SetNotebook(Notebook.NotebookParamType.NONE);
                            adjustParamCnt = (int)(Notebook.NotebookParamType.END) - 1 + 3/*标题*/;
                            break;
                    }

                    RectTransform rt = _AdjustContent.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 90 * adjustParamCnt);
                }
                else
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }
    }

    private ColorGradingBase GetCGBase()
    {
        ColorGradingBase cgBase = null;
        if (_CurrentFilter == ImageFilter.ImageEffectType.ColorGrading)
        {
            cgBase = _ImageEffectCamera.GetComponent<ImageEffects.ColorGrading>();
        }
        else if (_CurrentFilter == ImageFilter.ImageEffectType.Film)
        {
            cgBase = _ImageEffectCamera.GetComponent<Film>();
        }
        else if (_CurrentFilter == ImageFilter.ImageEffectType.BlackWhite)
        {
            cgBase = _ImageEffectCamera.GetComponent<BlackWhite>();
        }

        return cgBase;
    }
    public void ResetColorGrading()
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        cgBase.ResetDefault();

        EnableCameraRender(0.0f);

        SetColorGrading();
    }
    public void SetColorGrading()
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        cgt.Find("Temperature/Value").GetComponent<Text>().text = ""+ cgBase.temperature;
        cgt.Find("Tilt/Value").GetComponent<Text>().text = "" + cgBase.tilt;
        cgt.Find("Hue/Value").GetComponent<Text>().text = "" + cgBase.hue;
        cgt.Find("Saturation/Value").GetComponent<Text>().text = "" + cgBase.saturation;
        cgt.Find("Vibrance/Value").GetComponent<Text>().text = "" + cgBase.vibrance;
        cgt.Find("Value/Value").GetComponent<Text>().text = "" + cgBase.value;
        cgt.Find("Contrast/Value").GetComponent<Text>().text = "" + cgBase.contrast;
        cgt.Find("Gain/Value").GetComponent<Text>().text = "" + cgBase.gain;
        cgt.Find("Gamma/Value").GetComponent<Text>().text = "" + cgBase.gamma;

        cgt.Find("Temperature/Slider").GetComponent<Slider>().value = cgBase.temperature;
        cgt.Find("Tilt/Slider").GetComponent<Slider>().value = cgBase.tilt;
        cgt.Find("Hue/Slider").GetComponent<Slider>().value = cgBase.hue;
        cgt.Find("Saturation/Slider").GetComponent<Slider>().value = cgBase.saturation;
        cgt.Find("Vibrance/Slider").GetComponent<Slider>().value = cgBase.vibrance;
        cgt.Find("Value/Slider").GetComponent<Slider>().value = cgBase.value;
        cgt.Find("Contrast/Slider").GetComponent<Slider>().value = cgBase.contrast;
        cgt.Find("Gain/Slider").GetComponent<Slider>().value = cgBase.gain;
        cgt.Find("Gamma/Slider").GetComponent<Slider>().value = cgBase.gamma;

    }
    //电影、黑白、调色 三种滤镜参数一样
    public void OnColorGradingTemperatureValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find(""+_CurrentFilter);

        //变化值小于0.1，认为没有改变
        //if (cg.temperature - v < 0.1f) return;

        cgt.Find("Temperature/Value").GetComponent<Text>().text = "" + Math.Round(v,2);
        cgBase.temperature = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);
    }
    public void OnColorGradingTiltValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);

        //变化值小于0.1，认为没有改变
        //if (cg.tilt - v < 0.1f) return;

        cgt.Find("Tilt/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.tilt = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);
    }
    public void OnColorGradingHueValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);

        //变化值小于0.1，认为没有改变
        //if (cg.hue - v < 0.1f) return;

        cgt.Find("Hue/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.hue = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnColorGradingSaturationValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.saturation - v < 0.1f) return;

        cgt.Find("Saturation/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.saturation = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnColorGradingVibranceValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.vibrance - v < 0.1f) return;

        cgt.Find("Vibrance/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.vibrance = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnColorGradingValueValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.value - v < 0.1f) return;

        cgt.Find("Value/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.value = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnColorGradingContrastValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.contrast - v < 0.1f) return;

        cgt.Find("Contrast/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.contrast = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnColorGradingGainValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gain - v < 0.1f) return;

        cgt.Find("Gain/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.gain = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnColorGradingGammaValueChange(float v)
    {
        ColorGradingBase cgBase = GetCGBase();
        if (cgBase == null) return;

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gamma - v < 0.1f) return;

        cgt.Find("Gamma/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        cgBase.gamma = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);
    }

    public void ResetBloom()
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        bl.ResetDefault();

        SetBloom();

        EnableCameraRender(0.0f);
    }
    public void SetBloom()
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        cgt.Find("Threshold/Value").GetComponent<Text>().text = "" + bl.threshold;
        cgt.Find("Intensity/Value").GetComponent<Text>().text = "" + bl.intensity;
        cgt.Find("Downsampling/Value").GetComponent<Text>().text = "" + bl.downsampling;
        cgt.Find("Iterations/Value").GetComponent<Text>().text = "" + bl.iterations;
        cgt.Find("BlurSpread/Value").GetComponent<Text>().text = "" + bl.blurSpread;

        cgt.Find("Threshold/Slider").GetComponent<Slider>().value = bl.threshold;
        cgt.Find("Intensity/Slider").GetComponent<Slider>().value = bl.intensity;
        cgt.Find("Downsampling/Slider").GetComponent<Slider>().value = bl.downsampling;
        cgt.Find("Iterations/Slider").GetComponent<Slider>().value = bl.iterations;
        cgt.Find("BlurSpread/Slider").GetComponent<Slider>().value = bl.blurSpread;

    }
    public void OnBloomThresholdValueChange(float v)
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gamma - v < 0.1f) return;

        cgt.Find("Threshold/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        bl.threshold = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnBloomIntensityValueChange(float v)
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gamma - v < 0.1f) return;

        cgt.Find("Intensity/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        bl.intensity = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    public void OnBloomDownsamplingValueChange(float v)
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gamma - v < 0.1f) return;

        cgt.Find("Downsampling/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        bl.downsampling = (int)v;

        EnableCameraRender(0.0f);

    }
    public void OnBloomIterationsValueChange(float v)
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gamma - v < 0.1f) return;

        cgt.Find("Iterations/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        bl.iterations = (int)v;

        EnableCameraRender(0.0f);

    }
    public void OnBloomBlurSpreadValueChange(float v)
    {
        BloomEffect bl = _ImageEffectCamera.GetComponent<BloomEffect>();

        Transform cgt = _AdjustContent.transform.Find("" + _CurrentFilter);
        //变化值小于0.1，认为没有改变
        //if (cg.gamma - v < 0.1f) return;

        cgt.Find("BlurSpread/Value").GetComponent<Text>().text = "" + Math.Round(v, 2);
        bl.blurSpread = (float)Math.Round(v, 2);

        EnableCameraRender(0.0f);

    }
    #region 聚光灯·详细参数调整
    public GameObject _LightRaysAjust;
    //--------------------聚光灯·详细参数调整--------------------------------------------
    private void SetLightRaysSliderValue(LightRays2D.LightRaysParamType type,GameObject slider, LightRays2D lr)
    {
        Slider sd = slider.GetComponentInChildren<Slider>();
        Text tv = slider.transform.Find("Value").GetComponent<Text>();

        switch (type)
        {
            case LightRays2D.LightRaysParamType.BeginR:
                sd.value = (int)(lr.color1.r * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.BeginG:
                sd.value = (int)(lr.color1.g * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.BeginB:
                sd.value = (int)(lr.color1.b * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.BeginA:
                sd.value = (int)(lr.color1.a * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.EndR:
                sd.value = (int)(lr.color2.r * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.EndG:
                sd.value = (int)(lr.color2.g * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.EndB:
                sd.value = (int)(lr.color2.b * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.EndA:
                sd.value = (int)(lr.color2.a * 255);
                tv.text = "" + (int)sd.value;
                break;
            case LightRays2D.LightRaysParamType.Contrast:
                sd.value = lr.contrast;
                tv.text = "" + Math.Round(sd.value, 0);
                break;
            case LightRays2D.LightRaysParamType.Speed:
                sd.value = lr.speed;
                tv.text = "" + Math.Round(sd.value, 1);
                break;
            case LightRays2D.LightRaysParamType.Size:
                sd.value = lr.size;
                tv.text = "" + Math.Round(sd.value, 0);
                break;
            case LightRays2D.LightRaysParamType.Skew:
                sd.value = lr.skew;
                tv.text = "" + Math.Round(sd.value, 1);
                break;
            case LightRays2D.LightRaysParamType.Shear:
                sd.value = lr.shear;
                tv.text = "" + Math.Round(sd.value, 1);
                break;
            case LightRays2D.LightRaysParamType.Fade:
                sd.value = lr.fade;
                tv.text = "" + Math.Round(sd.value, 1);
                break;
        }
    }
    private void SetLightRays(LightRays2D.LightRaysParamType slider)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();

        if (slider == LightRays2D.LightRaysParamType.NONE)
        {
            for (int i = (int)LightRays2D.LightRaysParamType.NONE + 1;i < (int)LightRays2D.LightRaysParamType.END;i++)
            {
                LightRays2D.LightRaysParamType type = (LightRays2D.LightRaysParamType)i;
                GameObject sd = _LightRaysAjust.transform.Find(""+ type).gameObject;
                SetLightRaysSliderValue(type, sd,lr);
            }
        }
        else
        {
            GameObject sd = _LightRaysAjust.transform.Find("" + slider).gameObject;
            SetLightRaysSliderValue(slider, sd, lr);

        }
    }

    public void OnLightRaysBeginColorRValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();

        lr.color1.r = v/255f;
        if (lr.color1.r < 0) lr.color1.r = 0f;
        if (lr.color1.r > 1) lr.color1.r = 1f;

        SetLightRays(LightRays2D.LightRaysParamType.BeginR);
    }
    public void OnLightRaysBeginColorGValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color1.g = v / 255f;
        if (lr.color1.g < 0) lr.color1.g = 0f;
        if (lr.color1.g > 1) lr.color1.g = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.BeginG);

    }
    public void OnLightRaysBeginColorBValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color1.b = v / 255f;
        if (lr.color1.b < 0) lr.color1.b = 0f;
        if (lr.color1.b > 1) lr.color1.b = 1f;

        SetLightRays(LightRays2D.LightRaysParamType.BeginB);
    }
    public void OnLightRaysBeginColorAValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color1.a = v / 255f;
        if (lr.color1.a < 0) lr.color1.a = 0f;
        if (lr.color1.a > 1) lr.color1.a = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.BeginA);
    }
    public void OnLightRaysEndColorRValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color2.r = v / 255f;
        if (lr.color2.r < 0) lr.color2.r = 0f;
        if (lr.color2.r > 1) lr.color2.r = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.EndR);
    }
    public void OnLightRaysEndColorGValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color2.g = v / 255f;
        if (lr.color2.g < 0) lr.color2.g = 0f;
        if (lr.color2.g > 1) lr.color2.g = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.EndG);
    }
    public void OnLightRaysEndColorBValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color2.b = v / 255f;
        if (lr.color2.b < 0) lr.color2.b = 0f;
        if (lr.color2.b > 1) lr.color2.b = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.EndB);
    }
    public void OnLightRaysEndColorAValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.color2.a = v / 255f;
        if (lr.color2.a < 0) lr.color2.a = 0f;
        if (lr.color2.a > 1) lr.color2.a = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.EndA);
    }
    public void OnLightRaysContrastValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.contrast = v;
        if (lr.contrast < 0) lr.contrast = 0f;
        if (lr.contrast > 50) lr.contrast = 50f;
        SetLightRays(LightRays2D.LightRaysParamType.Contrast);
    }
    public void OnLightRaysSpeedValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.speed = v;
        if (lr.speed < 0) lr.speed = 0f;
        if (lr.speed > 5) lr.speed = 5f;
        SetLightRays(LightRays2D.LightRaysParamType.Speed);
    }
    public void OnLightRaysSizeValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.size = v;
        if (lr.size < 1) lr.size = 1f;
        if (lr.size > 30) lr.size = 30f;
        SetLightRays(LightRays2D.LightRaysParamType.Size);
    }
    public void OnLightRaysSkewValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.skew = v;
        if (lr.skew < -1) lr.skew = -1f;
        if (lr.skew > 1) lr.skew = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.Skew);
    }
    public void OnLightRaysShearValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.shear = v;
        if (lr.shear < 0) lr.shear = 0f;
        if (lr.shear > 5) lr.shear = 5f;
        SetLightRays(LightRays2D.LightRaysParamType.Shear);
    }
    public void OnLightRaysFadeValueChange(float v)
    {
        LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
        lr.fade = v;
        if (lr.fade < 0) lr.fade = 0f;
        if (lr.fade > 1) lr.fade = 1f;
        SetLightRays(LightRays2D.LightRaysParamType.Fade);
    }

    #endregion

    #region 手绘·详细参数调整
    public GameObject _NotebookAjust;
    //--------------------聚光灯·详细参数调整--------------------------------------------
    private void SetNotebookSliderValue(Notebook.NotebookParamType type, GameObject slider, Notebook nb)
    {
        Slider sd = slider.GetComponentInChildren<Slider>();
        Text tv = slider.transform.Find("Value").GetComponent<Text>();

        switch (type)
        {
            case Notebook.NotebookParamType.SampNum:
                sd.value = (int)nb.sampNum;
                tv.text = "" + (int)sd.value;
                break;
            case Notebook.NotebookParamType.AngleNum:
                sd.value = (int)(nb.angleNum);
                tv.text = "" + (int)sd.value;
                break;
            case Notebook.NotebookParamType.LineAlpha:
                sd.value = (float)Math.Round(nb.lineAlpha,2);
                tv.text = "" + sd.value;
                break;
            case Notebook.NotebookParamType.LineComplete:
                sd.value = (float)Math.Round(nb.lineComplete, 2);
                tv.text = "" + sd.value;
                break;
            case Notebook.NotebookParamType.LineSoft:
                sd.value = (float)Math.Round(nb.lineSoft, 2);
                tv.text = "" + sd.value;
                break;
            case Notebook.NotebookParamType.GridSize:
                sd.value = (int)nb.gridSize;
                tv.text = "" + (int)sd.value;
                break;
            case Notebook.NotebookParamType.GridAlphaX:
                sd.value = (float)Math.Round(nb.gridAlphaX, 2);
                tv.text = "" + sd.value;
                break;
            case Notebook.NotebookParamType.GridAlphaY:
                sd.value = (float)Math.Round(nb.gridAlphaY,2);
                tv.text = "" + sd.value;
                break;
            case Notebook.NotebookParamType.GridWidth:
                sd.value = (int)nb.gridWidth;
                tv.text = "" + (int)sd.value;
                break;
            case Notebook.NotebookParamType.R:
                sd.value = (int)(nb.gridColor.r * 255);
                tv.text = "" + (int)sd.value;
                break;
            case Notebook.NotebookParamType.G:
                sd.value = (int)(nb.gridColor.g * 255);
                tv.text = "" + (int)sd.value;
                break;
            case Notebook.NotebookParamType.B:
                sd.value = (int)(nb.gridColor.b * 255);
                tv.text = "" + (int)sd.value;
                break;
        }
    }
    private void SetNotebook(Notebook.NotebookParamType slider)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();

        if (slider == Notebook.NotebookParamType.NONE)
        {
            for (int i = (int)Notebook.NotebookParamType.NONE + 1; i < (int)Notebook.NotebookParamType.END; i++)
            {
                Notebook.NotebookParamType type = (Notebook.NotebookParamType)i;
                GameObject sd = _NotebookAjust.transform.Find("" + type).gameObject;
                SetNotebookSliderValue(type, sd, nb);
            }
        }
        else
        {
            GameObject sd = _NotebookAjust.transform.Find("" + slider).gameObject;
            SetNotebookSliderValue(slider, sd, nb);

            EnableCameraRender(0.0f);//需要开启一下渲染
        }
    }

    public void OnNotebookSampNumValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.sampNum = v;
        if (nb.sampNum < 1) nb.sampNum = 1f;
        if (nb.sampNum > 16) nb.sampNum = 16f;
        SetNotebook(Notebook.NotebookParamType.SampNum);
    }
    public void OnNotebookAngleNumValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.angleNum = v;
        if (nb.angleNum < 1) nb.angleNum = 1f;
        if (nb.angleNum > 5) nb.angleNum = 5f;
        SetNotebook(Notebook.NotebookParamType.AngleNum);
    }
    public void OnNotebookLineAlphaValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.lineAlpha = v;
        if (nb.lineAlpha < 0.01f) nb.lineAlpha = 0.01f;
        if (nb.lineAlpha > 2) nb.lineAlpha = 2f;
        SetNotebook(Notebook.NotebookParamType.LineAlpha);
    }
    public void OnNotebookLineCompleteValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.lineComplete = v;
        if (nb.lineComplete < 0.001f) nb.lineComplete = 0.001f;
        if (nb.lineComplete > 1) nb.lineComplete = 1f;
        SetNotebook(Notebook.NotebookParamType.LineComplete);
    }
    public void OnNotebookLineSoftValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.lineSoft = v;
        if (nb.lineSoft < 0.001f) nb.lineSoft = 0.001f;
        if (nb.lineSoft > 1) nb.lineSoft = 1f;
        SetNotebook(Notebook.NotebookParamType.LineSoft);
    }
    public void OnNotebookGridSizeValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridSize = v;
        if (nb.gridSize < 1) nb.gridSize = 1f;
        if (nb.gridSize > 200) nb.gridSize = 200f;
        SetNotebook(Notebook.NotebookParamType.GridSize);
    }
    public void OnNotebookGridAlphaXValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridAlphaX = v;
        if (nb.gridAlphaX < 0) nb.gridAlphaX = 0f;
        if (nb.gridAlphaX > 1) nb.gridAlphaX = 1f;
        SetNotebook(Notebook.NotebookParamType.GridAlphaX);
    }
    public void OnNotebookGridAlphaYValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridAlphaY = v;
        if (nb.gridAlphaY < 0) nb.gridAlphaY = 0f;
        if (nb.gridAlphaY > 1) nb.gridAlphaY = 1f;
        SetNotebook(Notebook.NotebookParamType.GridAlphaY);
    }
    public void OnNotebookGridWidthValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridWidth = v;
        if (nb.gridWidth < 1) nb.gridWidth = 1f;
        if (nb.gridWidth > 100) nb.gridWidth = 100f;
        SetNotebook(Notebook.NotebookParamType.GridWidth);
    }

    public void OnNotebookGridColorRValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridColor.r = v / 255f;
        if (nb.gridColor.r < 0) nb.gridColor.r = 0f;
        if (nb.gridColor.r > 1) nb.gridColor.r = 1f;

        SetNotebook(Notebook.NotebookParamType.R);
    }
    public void OnNotebookGridColorGValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridColor.g = v / 255f;
        if (nb.gridColor.g < 0) nb.gridColor.g = 0f;
        if (nb.gridColor.g > 1) nb.gridColor.g = 1f;

        SetNotebook(Notebook.NotebookParamType.G);

    }
    public void OnNotebookGridColorBValueChange(float v)
    {
        Notebook nb = _ImageEffectCamera.GetComponent<Notebook>();
        nb.gridColor.b = v / 255f;
        if (nb.gridColor.b < 0) nb.gridColor.b = 0f;
        if (nb.gridColor.b > 1) nb.gridColor.b = 1f;

        SetNotebook(Notebook.NotebookParamType.B);
    }
    #endregion
    private float GetRealValue(AdjustDirectionType type, float v)
    {
        if (type == AdjustDirectionType.LEFT
            || type == AdjustDirectionType.DOWN
            || type == AdjustDirectionType.DOWN_LEFT
            || type == AdjustDirectionType.LEFT_UP
            )
        {
            return -v;
        }

        return v;
    }
    //代表4个参数，上下、左右、左上右下、右上左下
    //数值范围[0,1]，需要转换
    public void OnAdjustFilter(AdjustDirectionType type,float v)
    {
        float realV = GetRealValue(type,v);//[-1,1]

        if (type == AdjustDirectionType.LEFT || type == AdjustDirectionType.RIGHT)
        {
            //优先使用左右调整
            switch (_CurrentFilter)
            {
                case ImageFilter.ImageEffectType.Technicolor:
                    //0.5 - 1.5
                    //调整亮度
                    Technicolor th;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out th);

                    float brightness = th.brightness.value + realV;
                    if (brightness < 0.5) brightness = 0.5f;
                    if (brightness > 1.5) brightness = 1.5f;

                    th.brightness.Override(brightness);

                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Tonemapping:
                    //exposure
                    ImageEffects.Tonemapper f = _ImageEffectCamera.GetComponent<ImageEffects.Tonemapper>();
                    f.exposure += realV * 16;
                    if (f.exposure < 0) f.exposure = 0;
                    if (f.exposure > 16) f.exposure = 16;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Ramp:
                    //opacity
                    Ramp rmp;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out rmp);

                    float opacity = rmp.opacity.value + realV;
                    if (opacity < 0) opacity = 0f;
                    if (opacity > 1) opacity = 1f;

                    rmp.opacity.Override(opacity);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.PPBloom:
                    //intensity [0,100]
                    Bloom bl;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out bl);

                    float intensity = bl.intensity.value + realV * 100;//
                    if (intensity < 0) intensity = 0f;
                    if (intensity > 100) intensity = 100f;

                    bl.intensity.Override(intensity);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Water:
                    //x方向流动
                    Water w = _Water.GetComponent<Water>();

                    w.speed.x -= realV;
                    if (w.speed.x < -1) w.speed.x = -1f;
                    if (w.speed.x > 1) w.speed.x = 1;

                    //EnableCameraRender(0.0f);//动态滤镜摄像机一直开启，无需该操作
                    break;
                case ImageFilter.ImageEffectType.Blur:
                    //downsampling
                    BlurEffect be = _ImageEffectCamera.GetComponent<BlurEffect>();
                    be.downsamplingValue += realV * 4;
                    if (be.downsamplingValue < 0f) be.downsamplingValue = 0f;
                    if (be.downsamplingValue > 4f) be.downsamplingValue = 4f;

                    if ((int)be.downsamplingValue != be.downsampling)
                    {
                        be.downsampling = (int)be.downsamplingValue;
                        EnableCameraRender(0.0f);
                    }

                    break;
                case ImageFilter.ImageEffectType.Mosaic:
                    //size
                    Mosaic msc = _ImageEffectCamera.GetComponent<Mosaic>();
                    int pszie = (int)msc.Size;
                    msc.Size += realV*100;
                    if (msc.Size < 10f) msc.Size = 10f;
                    if (msc.Size > 100f) msc.Size = 100f;

                    if(pszie != (int)msc.Size)// 起码+1
                        EnableCameraRender(0.0f);
    
                    break;
                case ImageFilter.ImageEffectType.Vignette:
                    //Centerx [0,1]
                    Vignette vt;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out vt);

                    float x = vt.center.value.x + realV;//
                    if (x < 0) x = 0f;
                    if (x > 1) x = 1f;

                    vt.center.Override(new Vector2(x, vt.center.value.y));
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.OldTV:
                    //exposure
                    OldTV ot = _ImageEffectCamera.GetComponent<OldTV>();
                    ot._fringing += realV;
                    if (ot._fringing < 0) ot._fringing = 0;
                    if (ot._fringing > 1) ot._fringing = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Streak:
                    //threshold [0,1]
                    Streak sk;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out sk);

                    float threshold = sk.threshold.value - realV;//越小越亮，这里取反

                    if (threshold < 0) threshold = 0f;
                    if (threshold > 1) threshold = 1f;

                    sk.threshold.Override(threshold);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.MotionBlur:
                    //无参数可调
                    MotionBlur mb;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out mb);

                    MotionBlurEffect mbe = _ImageEffectCamera.GetComponent<MotionBlurEffect>();

                    mbe.sampleCnt += realV * 16;

                    if (mbe.sampleCnt < 1) mbe.sampleCnt = 1;
                    if (mbe.sampleCnt > 16) mbe.sampleCnt = 16;


                    mb.sampleCount.Override((int)mbe.sampleCnt);
                    EnableCameraRender(0.0f);//需要启动一下摄像机渲染
                    break;
                case ImageFilter.ImageEffectType.Drunk:
                    //_Magnitude
                    Drunk dk = _ImageEffectCamera.GetComponent<Drunk>();
                    dk.magnitude += realV;
                    if (dk.magnitude < 0) dk.magnitude = 0;
                    if (dk.magnitude > 1) dk.magnitude = 1;
                   // EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.CRT:
                    //factor
                    CRT crt = _ImageEffectCamera.GetComponent<CRT>();
                    crt.factor += realV * 10f;
                    if (crt.factor < 0.1f) crt.factor = 0.1f;
                    if (crt.factor > 10) crt.factor = 10f;
                    // EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Division:
                    //block
                    Division dv = _ImageEffectCamera.GetComponent<Division>();
                    dv.block += realV * 32;
                    if (dv.block < 1f) dv.block = 1f;
                    if (dv.block > 32) dv.block = 32;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.FrostedGlass:
                    //r
                    FrostedGlass fg = _ImageEffectCamera.GetComponent<FrostedGlass>();
                    fg.radius += realV * 100;
                    if (fg.radius < 1f) fg.radius = 1f;
                    if (fg.radius > 100) fg.radius = 100f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Grayscale:
                    //blend [0,1]
                    Grayscale gc;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out gc);

                    float a = gc.blend.value + realV;

                    if (a < 0) a = 0f;
                    if (a > 1) a = 1f;

                    gc.blend.Override(a);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.GameBoy:
                    //r
                    GameBoy gb = _ImageEffectCamera.GetComponent<GameBoy>();
                    gb.gamma += realV * 10;
                    if (gb.gamma < 0f) gb.gamma = 0f;
                    if (gb.gamma > 10) gb.gamma = 10f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Halftoning:
                    //fr
                    HalftoneEffect ht = _ImageEffectCamera.GetComponent<HalftoneEffect>();
                    ht.frequency += realV * 100f;
                    if (ht.frequency < 0f) ht.frequency = 0f;
                    if (ht.frequency > 100) ht.frequency = 100f;
                    EnableCameraRender(0.0f);
                    break;

                case ImageFilter.ImageEffectType.Sandstorm:
                    //fr
                    Sandstorm ss = _ImageEffectCamera.GetComponent<Sandstorm>();
                    ss.intensity += realV;
                    if (ss.intensity < 0f) ss.intensity = 0f;
                    if (ss.intensity > 1) ss.intensity = 1f;
                    //EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Pencil:
                    Pencil pl;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out pl);

                    float sensivity = pl.sensivity.value + realV *100;

                    if (sensivity < 0) sensivity = 0f;
                    if (sensivity > 100) sensivity = 100f;

                    pl.sensivity.Override(sensivity);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Ascii:
                    //fr
                    Ascii ac = _ImageEffectCamera.GetComponent<Ascii>();
                    ac.size += realV * 100f;
                    if (ac.size < 1f) ac.size = 1f;
                    if (ac.size > 100f) ac.size = 100f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.WaterColor:
                    //fr
                    Watercolor wc = _ImageEffectCamera.GetComponent<Watercolor>();
                    int pi = (int)wc.Iterations;
                    wc.Iterations += realV * 10f;
                    if (wc.Iterations < 1f) wc.Iterations = 1f;
                    if (wc.Iterations > 10f) wc.Iterations = 10f;
                    if(pi != (int)wc.Iterations)
                        EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.OilPaint:
                    OilPaint op;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out op);

                    int pr = (int)op.radius.value;
                    float r = op.radius.value + realV * 16;

                    if (r < 0f) r = 0f;
                    if (r > 16f) r = 16f;

                    op.radius.Override(r);
                    if (pr != (int)r)
                    {
                        EnableCameraRender(0.0f);
                    }

                    break;
                case ImageFilter.ImageEffectType.Cartoon:
                    Cartoon ct;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out ct);

                    float power = ct.power.value + realV * 10;

                    if (power < 0.1f) power = 0.1f;
                    if (power > 10f) power = 10f;

                    ct.power.Override(power);
                    EnableCameraRender(0.0f);
                    

                    break;
                case ImageFilter.ImageEffectType.FakeHDR:
                    FakeHDR fh;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out fh);

                    float fhpower = fh.power.value + realV * 8;

                    if (fhpower < 0.1f) fhpower = 0.1f;
                    if (fhpower > 8f) fhpower = 8f;

                    fh.power.Override(fhpower);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Frost:
                    Material fst = _Frost.GetComponent<MeshRenderer>().materials[0];

                    float opt = fst.GetFloat("_Opacity") + realV;

                    if (opt < 0f) opt = 0f;
                    if (opt > 1f) opt = 1f;

                    fst.SetFloat("_Opacity", opt);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.RainGlass:
                    Material rg = _RainGlass.GetComponent<MeshRenderer>().materials[0];

                    float rgop = rg.GetFloat("_Opacity") + realV;

                    if (rgop < 0f) rgop = 0f;
                    if (rgop > 1f) rgop = 1f;

                    rg.SetFloat("_Opacity", rgop);
                    //EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Distortion:
                    //fr
                    Distortion dt = _ImageEffectCamera.GetComponent<Distortion>();
                    dt.posx += realV;
                    if (dt.posx < 0f) dt.posx = 0f;
                    if (dt.posx > 1f) dt.posx = 1f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.ColorGrading:
                    //fr
                    ImageEffects.ColorGrading cg = _ImageEffectCamera.GetComponent<ImageEffects.ColorGrading>();
                    cg.hue += realV;
                    if (cg.hue < -0.5f) cg.hue = -0.5f;
                    if (cg.hue > 0.5f) cg.hue = 0.5f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Film:
                    //fr
                    Film film = _ImageEffectCamera.GetComponent<Film>();
                    film.temperature += realV * 4;
                    if (film.temperature < -2f) film.temperature = -2f;
                    if (film.temperature > 2f) film.temperature = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.BlackWhite:
                    //fr
                    BlackWhite bw = _ImageEffectCamera.GetComponent<BlackWhite>();
                    bw.temperature += realV * 4;
                    if (bw.temperature < -2f) bw.temperature = -2f;
                    if (bw.temperature > 2f) bw.temperature = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Bloom:
                    //fr
                    BloomEffect ble = _ImageEffectCamera.GetComponent<BloomEffect>();

                    ble.threshold -= realV ;//fan
                    if (ble.threshold < 0f) ble.threshold = 0f;
                    if (ble.threshold > 1f) ble.threshold = 1f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.DigitalGlitch:
                    DigitalGlitch dg;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out dg);

                    float dgi = dg.intensity.value + realV;

                    if (dgi < 0f) dgi = 0f;
                    if (dgi > 1f) dgi = 1f;

                    dg.intensity.Override(dgi);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.ChromaticAberration:

                    ChromaticAberration ca;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out ca);

                    float cai = ca.intensity.value + realV * 100;
                    if (cai < 0f) cai = 0f;
                    if (cai > 100f) cai = 100f;

                    ca.intensity.Override(cai);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.AnalogGlitch:
                    AnalogGlitch ag;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out ag);

                    float slj = ag.scanLineJitter.value + realV;

                    if (slj < 0f) slj = 0f;
                    if (slj > 1f) slj = 1f;

                    ag.scanLineJitter.Override(slj);
                    EnableCameraRender(0.0f);

                    break;

                case ImageFilter.ImageEffectType.SpriteGlow:
                    SpriteGlowEffect sge = _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>();

                    float glowb = sge.GlowBrightness + realV * 20;

                    if (glowb < 1f) glowb = 1f;
                    if (glowb > 20f) glowb = 20f;

                    sge.GlowBrightness = glowb;
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Fog:
                    D2FogsNoiseTexPE fog = _ImageEffectCamera.GetComponent<D2FogsNoiseTexPE>();
                    fog.Density += realV * 3;
                    if (fog.Density < 0f) fog.Density = 0f;
                    if (fog.Density > 3f) fog.Density = 3f;
                    break;
                case ImageFilter.ImageEffectType.DeferredNV:
                    DeferredNightVision dnv = _ImageEffectCamera.GetComponent<DeferredNightVision>();
                    dnv.scope += realV;
                    if (dnv.scope < 0.05f) dnv.scope = 0.05f;
                    if (dnv.scope > 1) dnv.scope = 1;

                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Ice:
                    Ice ice = _ImageEffectCamera.GetComponent<Ice>();
                    ice.FrostAmount += realV;
                    if (ice.FrostAmount < 0) ice.FrostAmount = 0;
                    if (ice.FrostAmount > 1) ice.FrostAmount = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.LightRays:
                    LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
                    lr.contrast += realV * 50;
                    if (lr.contrast < 0) lr.contrast = 0f;
                    if (lr.contrast > 50) lr.contrast = 50f;

                    SetLightRays(LightRays2D.LightRaysParamType.Contrast);

                    break;
                case ImageFilter.ImageEffectType.Vortex:
                    Vortex vortex = _ImageEffectCamera.GetComponent<Vortex>();
                    vortex.angle += realV * 3600;
                    if (vortex.angle < 0) vortex.angle = 0;
                    if (vortex.angle > 3600) vortex.angle = 3600;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.ColorASCII:
                    ColorASCII cas = _ImageEffectCamera.GetComponent<ColorASCII>();
                    cas.tilesXY += realV * 100;
                    if (cas.tilesXY < 1) cas.tilesXY = 1;
                    if (cas.tilesXY > 100) cas.tilesXY = 100;
                    EnableCameraRender(0.0f);
                    break;
                    
                case ImageFilter.ImageEffectType.Comic:
                    Comic comic = _ImageEffectCamera.GetComponent<Comic>();
                    comic.centerX += realV;
                    if (comic.centerX < 0) comic.centerX = 0;
                    if (comic.centerX > 1) comic.centerX = 1;
                    EnableCameraRender(0.0f);
                    break;
                    /*效率太低，无法支持参数调整
                case ImageFilter.ImageEffectType.Notebook:
                    Notebook notebook = _ImageEffectCamera.GetComponent<Notebook>();
                    notebook.angleNum += realV * 5;
                    if (notebook.angleNum < 1) notebook.angleNum = 1;
                    if (notebook.angleNum > 5) notebook.angleNum = 5;
                    EnableCameraRender(0.0f);
                    break;
                    */
                case ImageFilter.ImageEffectType.ThermalVision:
                    ThermalVision tv;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out tv);

                    int cv1 = (int)(tv.ColorValue * 8);
                    tv.ColorValue += realV;
                    int cv2 = (int)(tv.ColorValue * 8);
                    if (tv.ColorValue < 0) tv.ColorValue = 0f;
                    if (tv.ColorValue > 1) tv.ColorValue = 1f;

                    if (cv1 != cv2)
                    {
                        //随机一个颜色
                        Color c = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                        tv.midColor.Override(c);
                        EnableCameraRender(0.0f);
                    }

                    break;
            }
        }
        else if (type == AdjustDirectionType.DOWN || type == AdjustDirectionType.UP)
        {
            //其次上下调整
            switch (_CurrentFilter)
            {
                case ImageFilter.ImageEffectType.Technicolor:
                    //调整S=>0f, 1.5f
                    Technicolor th;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out th);

                    float saturation = th.saturation.value + realV * 1.5f;
                    if (saturation < 0) saturation = 0f;
                    if (saturation > 1.5) saturation = 1.5f;

                    th.saturation.Override(saturation);

                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Tonemapping:
                    //method ->int[0,5] -> int(*5)
                    ImageEffects.Tonemapper f = _ImageEffectCamera.GetComponent<ImageEffects.Tonemapper>();
                    f.MethodValue += realV * 6;
                    if (f.MethodValue < 0) f.MethodValue = 0f;
                    if (f.MethodValue > 5) f.MethodValue = 5f;

                    ImageEffects.Tonemapper.TonemappingMethod nm = (ImageEffects.Tonemapper.TonemappingMethod)((int)f.MethodValue);

                    //只有真正变化的时候才需要开启
                    if (f.method != nm)
                    {
                        f.method = nm;
                        EnableCameraRender(0.0f);
                    }

                    break;
                case ImageFilter.ImageEffectType.Ramp:
                    //angle
                    Ramp rmp;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out rmp);

                    float angle = rmp.angle.value + realV * 180;
                    if (angle < -180) angle = -180f;
                    if (angle > 180) angle = 180f;

                    rmp.angle.Override(angle);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.PPBloom:
                    //soft knee [0,1]
                    Bloom bl;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out bl);

                    float softknee = bl.softKnee.value + realV;//
                    if (softknee < 0) softknee = 0f;
                    if (softknee > 1) softknee = 1;

                    bl.softKnee.Override(softknee);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Water:
                    //y方向流动
                    Water w = _Water.GetComponent<Water>();
                    w.speed.y -= realV;//水流的时候，左右增减是反的
                    if (w.speed.y < -1) w.speed.y = -1f;
                    if (w.speed.y > 1) w.speed.y = 1;

                    //EnableCameraRender(0.0f);//动态滤镜摄像机一直开启，无需该操作
                    break;
                case ImageFilter.ImageEffectType.Blur:
                    //downsampling
                    BlurEffect be = _ImageEffectCamera.GetComponent<BlurEffect>();
                    be.iterationsValue += realV * 20;
                    if (be.iterationsValue < 0f) be.iterationsValue = 0f;
                    if (be.iterationsValue > 20f) be.iterationsValue = 20f;

                    if ((int)be.iterationsValue != be.iterations)
                    {
                        be.iterations = (int)be.iterationsValue;
                        EnableCameraRender(0.0f);
                    }

                    break;
                case ImageFilter.ImageEffectType.Vignette:
                    //Centerx [0,1]
                    Vignette vt;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out vt);

                    float y = vt.center.value.y + realV;//
                    if (y < 0) y = 0f;
                    if (y > 1) y = 1f;

                    vt.center.Override(new Vector2(vt.center.value.x,y));
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.OldTV:
                    //_scanline
                    OldTV ot = _ImageEffectCamera.GetComponent<OldTV>();
                    ot._scanline += realV;
                    if (ot._scanline < 0) ot._scanline = 0;
                    if (ot._scanline > 1) ot._scanline = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Streak:
                    //color [0,1]
                    Streak sk;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out sk);

                    int cv1 = (int)(sk.ColorValue * 8);
                    sk.ColorValue += realV;
                    int cv2 = (int)(sk.ColorValue * 8);
                    if (sk.ColorValue < 0) sk.ColorValue = 0f;
                    if (sk.ColorValue > 1) sk.ColorValue = 1f;

                    if (cv1 != cv2)
                    {
                        //随机一个颜色
                        Color c = new Color(HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f);
                        sk.tint.Override(c);
                        EnableCameraRender(0.0f);
                    }

                    break;
                case ImageFilter.ImageEffectType.Drunk:
                    //_Magnitude
                    Drunk dk = _ImageEffectCamera.GetComponent<Drunk>();
                    dk.speed += realV * 10f;
                    if (dk.speed < 0.1f) dk.speed = 0.1f;
                    if (dk.speed > 10) dk.speed = 10f;
                    // EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Pencil:
                    Pencil pl;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out pl);

                    float gradThreshold = pl.gradThreshold.value + realV / 10;

                    if (gradThreshold < 0.00001f) gradThreshold = 0.00001f;
                    if (gradThreshold > 0.1) gradThreshold = 0.1f;

                    pl.gradThreshold.Override(gradThreshold);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Cartoon:
                    Cartoon ct;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out ct);

                    float edgeSlope = ct.edgeSlope.value + realV;

                    if (edgeSlope < 0.1f) edgeSlope = 0.1f;
                    if (edgeSlope > 1f) edgeSlope = 1f;

                    ct.edgeSlope.Override(edgeSlope);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.FakeHDR:
                    FakeHDR fh;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out fh);

                    float radius1 = fh.radius1.value + realV * 8;

                    if (radius1 < 0.1f) radius1 = 0.1f;
                    if (radius1 > 8f) radius1 = 8f;

                    fh.radius1.Override(radius1);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.RainGlass:
                    Material rg = _RainGlass.GetComponent<MeshRenderer>().materials[0];

                    float rgop = rg.GetFloat("_BumpFactor") + realV;

                    if (rgop < 0f) rgop = 0f;
                    if (rgop > 1f) rgop = 1f;

                    rg.SetFloat("_BumpFactor", rgop);
                    //EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Distortion:
                    //fr
                    Distortion dt = _ImageEffectCamera.GetComponent<Distortion>();
                    dt.posy += realV;
                    if (dt.posy < 0f) dt.posy = 0f;
                    if (dt.posy > 1f) dt.posy = 1f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.ColorGrading:
                    //fr
                    ImageEffects.ColorGrading cg = _ImageEffectCamera.GetComponent<ImageEffects.ColorGrading>();
                    cg.saturation += realV * 2;
                    if (cg.saturation < 0f) cg.saturation = 0f;
                    if (cg.saturation > 2f) cg.saturation = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Film:
                    //fr
                    Film film = _ImageEffectCamera.GetComponent<Film>();
                    film.tilt += realV * 4;
                    if (film.tilt < -2f) film.tilt = -2f;
                    if (film.tilt > 2f) film.tilt = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.BlackWhite:
                    //fr
                    BlackWhite bw = _ImageEffectCamera.GetComponent<BlackWhite>();
                    bw.gamma += realV * 5;
                    if (bw.gamma < 0.01f) bw.gamma = 0.01f;
                    if (bw.gamma > 5f) bw.gamma = 5f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Bloom:
                    //fr
                    BloomEffect ble = _ImageEffectCamera.GetComponent<BloomEffect>();

                    ble.intensity += realV*10;
                    if (ble.intensity < 0f) ble.intensity = 0f;
                    if (ble.intensity > 10f) ble.intensity = 10f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.AnalogGlitch:
                    AnalogGlitch ag;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out ag);

                    float cd = ag.colorDrift.value + realV;

                    if (cd < 0f) cd = 0f;
                    if (cd > 1f) cd = 1f;

                    ag.colorDrift.Override(cd);
                    EnableCameraRender(0.0f);

                    break;

                case ImageFilter.ImageEffectType.SpriteGlow:
                    SpriteGlowEffect sge = _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>();

                    float ow = sge.OutlineWidth + realV * 10;

                    if (ow < 1f) ow = 1f;
                    if (ow > 10f) ow = 10f;

                    sge.OutlineWidth = ow;
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Fog:
                    D2FogsNoiseTexPE fog = _ImageEffectCamera.GetComponent<D2FogsNoiseTexPE>();
                    fog.Size += realV;
                    if (fog.Size < 0f) fog.Size = 0f;
                    if (fog.Size > 1f) fog.Size = 1f;
                    break;

                case ImageFilter.ImageEffectType.DeferredNV:
                    DeferredNightVision dnv = _ImageEffectCamera.GetComponent<DeferredNightVision>();
                    dnv.m_LightSensitivityMultiplier += realV * 100f;
                    if (dnv.m_LightSensitivityMultiplier < 0) dnv.m_LightSensitivityMultiplier = 0;
                    if (dnv.m_LightSensitivityMultiplier > 100) dnv.m_LightSensitivityMultiplier = 100;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Ice:
                    Ice ice = _ImageEffectCamera.GetComponent<Ice>();
                    ice.seethroughness += realV;
                    if (ice.seethroughness < 0) ice.seethroughness = 0;
                    if (ice.seethroughness > 1) ice.seethroughness = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.LightRays:
                    LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
                    lr.speed += realV * 5;
                    if (lr.speed < 0) lr.speed = 0f;
                    if (lr.speed > 50) lr.speed = 5f;
                    SetLightRays(LightRays2D.LightRaysParamType.Speed);
                    break;
                case ImageFilter.ImageEffectType.ColorASCII:
                    ColorASCII cas = _ImageEffectCamera.GetComponent<ColorASCII>();
                    cas.characterBrightness += realV * 10;
                    if (cas.characterBrightness < 0) cas.characterBrightness = 0;
                    if (cas.characterBrightness > 10) cas.characterBrightness = 10;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Comic:
                    Comic comic = _ImageEffectCamera.GetComponent<Comic>();
                    comic.centerY += realV;
                    if (comic.centerY < 0) comic.centerY = 0;
                    if (comic.centerY > 1) comic.centerY = 1;
                    EnableCameraRender(0.0f);
                    break;
                    /*效率太低，无法支持参数调整
                case ImageFilter.ImageEffectType.Notebook:
                    Notebook notebook = _ImageEffectCamera.GetComponent<Notebook>();
                    notebook.sampNum += realV * 10;
                    if (notebook.sampNum < 1) notebook.sampNum = 1;
                    if (notebook.sampNum > 10) notebook.sampNum = 10;
                    EnableCameraRender(0.0f);
                    break;
                    */
            }
        }
        else if (type == AdjustDirectionType.UP_RIGHT || type == AdjustDirectionType.DOWN_LEFT)
        {
            //再次右上左下
            switch (_CurrentFilter)
            {
                case ImageFilter.ImageEffectType.Technicolor:
                    //暂时不支持strength
                    break;
                case ImageFilter.ImageEffectType.Ramp:
                    //color1
                    Ramp rmp;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out rmp);

                    int cv1 = (int)(rmp.Color1Value * 8);
                    rmp.Color1Value += realV;
                    int cv2 = (int)(rmp.Color1Value * 8);
                    if (rmp.Color1Value < 0) rmp.Color1Value = 0f;
                    if (rmp.Color1Value > 1) rmp.Color1Value = 1f;

                    if (cv1 != cv2)
                    {
                        //随机一个颜色
                        Color c = new Color(HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f);
                        rmp.color1.Override(c);
                        EnableCameraRender(0.0f);
                    }
                    break;
                case ImageFilter.ImageEffectType.PPBloom:
                    //diffusion [1,10]
                    Bloom bl;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out bl);

                    float diffusion = bl.diffusion.value + realV * 10f;//

                    if (diffusion < 1) diffusion = 1f;
                    if (diffusion > 10) diffusion = 10;

                    bl.diffusion.Override(diffusion);
                    EnableCameraRender(0.0f);
                    
                    break;
                case ImageFilter.ImageEffectType.Water:
                    //水波幅度
                    Water w = _Water.GetComponent<Water>();
                    float m = w.GetMaterial().GetFloat("_Magnitude");
                    m += realV;
                    if (m < 0) m = 0.01f;
                    if (m > 1) m = 1;
                    w.GetMaterial().SetFloat("_Magnitude",m);
                    break;
                case ImageFilter.ImageEffectType.Blur:
                    //downsampling
                    BlurEffect be = _ImageEffectCamera.GetComponent<BlurEffect>();
                    be.blurSpread += realV;
                    if (be.blurSpread < 0f) be.blurSpread = 0f;
                    if (be.blurSpread > 1f) be.blurSpread = 1;

                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Vignette:
                    //smoothness [0,1]
                    Vignette vt;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out vt);

                    float sm = vt.smoothness.value + realV;//
                    if (sm < 0.01) sm = 0.01f;
                    if (sm > 1) sm = 1f;

                    vt.smoothness.Override(sm);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.OldTV:
                    //_bleeding
                    OldTV ot = _ImageEffectCamera.GetComponent<OldTV>();
                    ot._bleeding += realV;
                    if (ot._bleeding < 0) ot._bleeding = 0;
                    if (ot._bleeding > 1) ot._bleeding = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Streak:
                    //threshold [0,1]
                    Streak sk;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out sk);

                    float stretch = sk.stretch.value + realV;//

                    if (stretch < 0) stretch = 0f;
                    if (stretch > 1) stretch = 1f;

                    sk.stretch.Override(stretch);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Pencil:
                    Pencil pl;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out pl);

                    float colorThreshold = pl.colorThreshold.value + realV;

                    if (colorThreshold < 0) colorThreshold = 0f;
                    if (colorThreshold > 1) colorThreshold = 1;

                    pl.colorThreshold.Override(colorThreshold);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.FakeHDR:
                    FakeHDR fh;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out fh);

                    float radius2 = fh.radius2.value + realV * 8;

                    if (radius2 < 0.1f) radius2 = 0.1f;
                    if (radius2 > 8f) radius2 = 8f;

                    fh.radius2.Override(radius2);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.Distortion:
                    //fr
                    Distortion dt = _ImageEffectCamera.GetComponent<Distortion>();
                    dt.distortion += realV;
                    if (dt.distortion < 0f) dt.distortion = 0f;
                    if (dt.distortion > 1f) dt.distortion = 1f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.ColorGrading:
                    //fr
                    ImageEffects.ColorGrading cg = _ImageEffectCamera.GetComponent<ImageEffects.ColorGrading>();
                    cg.vibrance += realV * 2;
                    if (cg.vibrance < -1f) cg.vibrance = -1f;
                    if (cg.vibrance > 1f) cg.vibrance = 1f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Film:
                    //fr
                    Film film = _ImageEffectCamera.GetComponent<Film>();
                    film.contrast += realV * 2;
                    if (film.contrast < 0f) film.contrast = 0f;
                    if (film.contrast > 2f) film.contrast = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.BlackWhite:
                    //fr
                    BlackWhite bw = _ImageEffectCamera.GetComponent<BlackWhite>();
                    bw.value += realV * 10;
                    if (bw.value < 0f) bw.value = 0f;
                    if (bw.value > 10f) bw.value = 10f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Bloom:
                    //fr
                    BloomEffect ble = _ImageEffectCamera.GetComponent<BloomEffect>();

                    ble.iterations += realV * 10;
                    if (ble.iterations < 0f) ble.iterations = 0f;
                    if (ble.iterations > 10f) ble.iterations = 10f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.RainGlass:
                    Material rg = _RainGlass.GetComponent<MeshRenderer>().materials[0];

                    float rgop = rg.GetFloat("_Blend") + realV / 10f;

                    if (rgop < 0f) rgop = 0f;
                    if (rgop > 0.1f) rgop = 0.1f;

                    rg.SetFloat("_Blend", rgop);
                    //EnableCameraRender(0.0f);
                    break;

                case ImageFilter.ImageEffectType.SpriteGlow:
                    SpriteGlowEffect sge = _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>();

                    cv1 = (int)(sge.Color1Value * 8);
                    sge.Color1Value += realV;
                    cv2 = (int)(sge.Color1Value * 8);
                    if (sge.Color1Value < 0) sge.Color1Value = 0f;
                    if (sge.Color1Value > 1) sge.Color1Value = 1f;

                    if (cv1 != cv2)
                    {
                        //随机一个颜色
                        Color sgec = new Color(HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f);


                        sge.GlowColor = sgec;
                        EnableCameraRender(0.0f);

                    }

                    break;
                case ImageFilter.ImageEffectType.Fog:
                    D2FogsNoiseTexPE fog = _ImageEffectCamera.GetComponent<D2FogsNoiseTexPE>();
                    fog.HorizontalSpeed += realV;
                    if (fog.HorizontalSpeed < 0f) fog.HorizontalSpeed = 0f;
                    if (fog.HorizontalSpeed > 1f) fog.HorizontalSpeed = 1f;
                    break;
                case ImageFilter.ImageEffectType.Ice:
                    Ice ice = _ImageEffectCamera.GetComponent<Ice>();
                    ice.distortion += realV;
                    if (ice.distortion < 0) ice.distortion = 0;
                    if (ice.distortion > 1) ice.distortion = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.LightRays:
                    LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
                    lr.size += realV * 30;
                    if (lr.size < 1) lr.size = 1f;
                    if (lr.size > 30) lr.size = 30f;
                    SetLightRays(LightRays2D.LightRaysParamType.Size);
                    break;
                case ImageFilter.ImageEffectType.DeferredNV:
                    DeferredNightVision dnv = _ImageEffectCamera.GetComponent<DeferredNightVision>();
                    float nvalpha = dnv.m_NVColor.a;
                    nvalpha += realV;
                    if (nvalpha < 0) nvalpha = 0;
                    if (nvalpha > 1) nvalpha = 1;

                    dnv.m_NVColor = new Color(dnv.m_NVColor.r, dnv.m_NVColor.g, dnv.m_NVColor.b, nvalpha);

                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.ColorASCII:
                    ColorASCII cas = _ImageEffectCamera.GetComponent<ColorASCII>();
                    cas.hiResAlpha += realV;
                    if (cas.hiResAlpha < 0) cas.hiResAlpha = 0;
                    if (cas.hiResAlpha > 1) cas.hiResAlpha = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Comic:
                    Comic comic = _ImageEffectCamera.GetComponent<Comic>();
                    comic.centralEdge += realV;
                    if (comic.centralEdge < 0.1) comic.centralEdge = 0.1f;
                    if (comic.centralEdge > 0.8) comic.centralEdge = 0.8f;
                    EnableCameraRender(0.0f);
                    break;
                    /*效率太低，无法支持参数调整
                case ImageFilter.ImageEffectType.Notebook:
                    Notebook notebook = _ImageEffectCamera.GetComponent<Notebook>();
                    notebook.gridSize += realV * 200;
                    if (notebook.gridSize < 1) notebook.gridSize = 1;
                    if (notebook.gridSize > 200) notebook.gridSize = 200;
                    EnableCameraRender(0.0f);
                    break;
                    */

            }
        }
        else if (type == AdjustDirectionType.LEFT_UP || type == AdjustDirectionType.DOWN_RIGHT)
        {
            //最次左上右下
            switch (_CurrentFilter)
            {
                case ImageFilter.ImageEffectType.Ramp:
                    //color2
                    Ramp rmp;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out rmp);

                    int cv1 = (int)(rmp.Color2Value * 8);
                    rmp.Color2Value += realV;
                    int cv2 = (int)(rmp.Color2Value * 8);
                    if (rmp.Color2Value < 0) rmp.Color2Value = 0f;
                    if (rmp.Color2Value > 1) rmp.Color2Value = 1f;

                    if (cv1 != cv2)
                    {
                        //随机一个颜色

                        Color c = new Color(HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f);
                        rmp.color2.Override(c);
                        EnableCameraRender(0.0f);
                    }
                    break;
                case ImageFilter.ImageEffectType.PPBloom:
                    //color [1,10]
                    //无法支持，需要能选择颜色才可以
                    break;
                case ImageFilter.ImageEffectType.Vignette:
                    //Centerx [0,1]
                    Vignette vt;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out vt);

                    float rd = vt.roundness.value + realV;//
                    if (rd < 0.01f) rd = 0.01f;
                    if (rd > 1) rd = 1f;

                    vt.roundness.Override(rd);
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Streak:
                    //threshold [0,1]
                    Streak sk;
                    _ImageEffectPostProcessVolume.profile.TryGetSettings(out sk);

                    float intensity = sk.intensity.value + realV;//

                    if (intensity < 0) intensity = 0f;
                    if (intensity > 1) intensity = 1f;

                    sk.intensity.Override(intensity);
                    EnableCameraRender(0.0f);

                    break;
                case ImageFilter.ImageEffectType.ColorGrading:
                    //fr
                    ImageEffects.ColorGrading cg = _ImageEffectCamera.GetComponent<ImageEffects.ColorGrading>();
                    cg.temperature += realV * 4;
                    if (cg.temperature < -2f) cg.temperature = -2f;
                    if (cg.temperature > 2f) cg.temperature = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Film:
                    //fr
                    Film film = _ImageEffectCamera.GetComponent<Film>();
                    film.gain += realV * 5;
                    if (film.gain < 0.01f) film.gain = 0.01f;
                    if (film.gain > 5f) film.gain = 5f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.BlackWhite:
                    //fr
                    BlackWhite bw = _ImageEffectCamera.GetComponent<BlackWhite>();
                    bw.contrast += realV * 2;
                    if (bw.contrast < 0f) bw.contrast = 0f;
                    if (bw.contrast > 2f) bw.contrast = 2f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Bloom:
                    //fr
                    BloomEffect ble = _ImageEffectCamera.GetComponent<BloomEffect>();

                    ble.blurSpread += realV;
                    if (ble.blurSpread < 0f) ble.blurSpread = 0f;
                    if (ble.blurSpread > 10f) ble.blurSpread = 10f;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Ice:
                    Ice ice = _ImageEffectCamera.GetComponent<Ice>();
                    ice.EdgeSharpness += realV;
                    if (ice.EdgeSharpness < 0) ice.EdgeSharpness = 0;
                    if (ice.EdgeSharpness > 1) ice.EdgeSharpness = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.LightRays:
                    LightRays2D lr = _LightRays.GetComponent<LightRays2D>();
                    lr.shear += realV * 5;
                    if (lr.shear < 0) lr.shear = 0f;
                    if (lr.shear > 5) lr.shear = 5;
                    SetLightRays(LightRays2D.LightRaysParamType.Shear);
                    break;
                case ImageFilter.ImageEffectType.Fog:
                    D2FogsNoiseTexPE fog = _ImageEffectCamera.GetComponent<D2FogsNoiseTexPE>();

                    cv1 = (int)(fog.Color1Value * 8);
                    fog.Color1Value += realV;
                    cv2 = (int)(fog.Color1Value * 8);
                    if (fog.Color1Value < 0) fog.Color1Value = 0f;
                    if (fog.Color1Value > 1) fog.Color1Value = 1f;

                    if (cv1 != cv2)
                    {
                        //随机一个颜色
                        fog.Color = new Color(HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f,
                                            HZManager.GetInstance().GenerateRandomInt(0, 255) / 255f);
                        EnableCameraRender(0.0f);
                    }
                    break;
                case ImageFilter.ImageEffectType.ColorASCII:
                    ColorASCII cas = _ImageEffectCamera.GetComponent<ColorASCII>();
                    cas.loResAlpha += realV;
                    if (cas.loResAlpha < 0) cas.loResAlpha = 0;
                    if (cas.loResAlpha > 1) cas.loResAlpha = 1;
                    EnableCameraRender(0.0f);
                    break;
                case ImageFilter.ImageEffectType.Comic:
                    Comic comic = _ImageEffectCamera.GetComponent<Comic>();
                    comic.line += realV * 10;
                    if (comic.line < 0) comic.line = 0f;
                    if (comic.line > 10) comic.line = 10f;
                    EnableCameraRender(0.0f);
                    break;
                    /*效率太低，无法支持参数调整
                case ImageFilter.ImageEffectType.Notebook:
                    Notebook notebook = _ImageEffectCamera.GetComponent<Notebook>();
                    notebook.lineComplete += realV ;
                    if (notebook.lineComplete < 0.001) notebook.lineComplete = 0.001f;
                    if (notebook.lineComplete > 1) notebook.lineComplete = 1;
                    EnableCameraRender(0.0f);
                    break;
                    */
            }
        }
    }
}
