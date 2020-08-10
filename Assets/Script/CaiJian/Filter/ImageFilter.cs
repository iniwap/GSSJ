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

public class ImageFilter : MonoBehaviour
{
    public enum ImageEffectType {
        None,
        Ascii,
        Bloom,
        Blur,
        ColorGrading,
        CRT,
        Distortion,
        Division,
        Drunk,
        Frost,
        FrostedGlass,
        GameBoy,
        Halftoning,
        Mosaic,
        BlackWhite,
        OldTV,
        RainGlass,
        Sandstorm,
        Tonemapping,
        Water,
        WaterColor,
        Film,
        Fog,
        DeferredNV,
        Ice,
        LightRays,
        Vortex,
        ColorASCII,
        Comic,
        Notebook,
       

        //以下是post滤镜
        OilPaint,
        Pencil,
        Cartoon,
        Technicolor,
        FakeHDR,
        Ramp,
        Streak,
        Grayscale,
        MotionBlur,
        Vignette,
        PPBloom,
        AnalogGlitch,
        DigitalGlitch,
        SpriteGlow,
        ChromaticAberration,
        ThermalVision,

        End,
    }

    public FilterAdjust _filterAdjust;
    //所有的滤镜
    public GameObject _ImageEffectList;
    public GameObject _ImageEffectBtnContent;
    public GameObject _ImageEffect;
    public ClickedWaveAnimation _wa;
    private bool _HasInit = false;
    public void OnInit()
    {
        if (_HasInit) return;
        _HasInit = true;

        for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
        {
            Transform obj = _ImageEffectList.transform.Find("" + (ImageEffectType)i);
            RenderTexture rt = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32);

            Camera c = obj.GetComponentInChildren<Camera>();
            c.targetTexture = rt;


            //由于四维的初始化值无法在编辑器设置，这里代码初始化下
            if ((ImageEffectType)i == ImageEffectType.ChromaticAberration)
            {
                PostProcessVolume pp = obj.GetComponentInChildren<PostProcessVolume>(true);
                ChromaticAberration ca;
                pp.profile.TryGetSettings(out ca);

                ca.intensity.Override(30f);

                //大图的也需要设置
                ChromaticAberration ca2;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ca2);
                ca2.intensity.Override(30f);
            }
        }


        //所有的禁用
        MonoBehaviour[] sc = _ImageEffectCamera.GetComponents<MonoBehaviour>();
        foreach (var b in sc)
        {
            b.enabled = false;
        }

        foreach (var st in _ImageEffectPostProcessVolume.profile.settings)
        {
            st.enabled.Override(false);
        }
    }

    public void Start()
    {
        //将所有按钮的rt设置为camera rt

        for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
        {
            Transform obj = _ImageEffectList.transform.Find("" + (ImageEffectType)i);
            Camera c = obj.GetComponentInChildren<Camera>();
            //c.targetTexture = rt;

            Transform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i);
            RawImage ri = btn.GetComponentInChildren<RawImage>(true);
            ri.texture = c.targetTexture;
        }
    }

    public void OnDisable()
    {
        if (_ImageEffect != null)
        {
            _ImageEffect.SetActive(false);
        }
        _wa.enabled = true;
    }

    private bool GetIsFreeFilter(ImageEffectType type)
    {
        bool ret = true;

        if (type != ImageEffectType.None
              && type != ImageEffectType.Technicolor
              && type != ImageEffectType.Tonemapping
              && type != ImageEffectType.ColorGrading
              && type != ImageEffectType.Ramp
              && type != ImageEffectType.SpriteGlow
              && type != ImageEffectType.Bloom
              && type != ImageEffectType.Water
              && type != ImageEffectType.Blur
              && type != ImageEffectType.Mosaic)
        {
            ret = false;
        }

        return ret;
    }

    public void OnEnable()
    {
        _ImageEffect.SetActive(true);
        _wa.enabled = false;

        bool hasBuy = IAP.getHasBuy(IAP.IAP_LVJING);
        for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
        {
            Transform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i);

            if (!GetIsFreeFilter((ImageEffectType)i))
            {
                btn.Find("BuyBtn").gameObject.SetActive(!hasBuy);
            }
            else
            {
                btn.Find("BuyBtn").gameObject.SetActive(false);
            }
        }
    }

    //菜单滑动时候，阻止切换操作，否则有误
    private bool _canHide = false;
    private void DelayCanHide()
    {
        _canHide = true;
        OnScrollViewValueChanged(Vector2.zero);
    }

    public void OnScrollViewValueChanged(Vector2 pos)
    {
        if (_LeftNeedRefreshBtn.Count != 0) return;

        for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
        {
            RectTransform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i) as RectTransform;
            Transform obj = _ImageEffectList.transform.Find("" + (ImageEffectType)i);

            bool show = true;
            if (btn.position.x > -btn.sizeDelta.x / 2 && btn.position.x < Screen.width + btn.sizeDelta.x / 2)
            {
                //当前滤镜可见
                btn.Find("Icon").gameObject.SetActive(true);
                show = true;
            }
            else
            {
                //当前滤镜不可见
                btn.Find("Icon").gameObject.SetActive(false);
                show = false;
            }

            if (_canHide)
            {
                Camera c = obj.GetComponentInChildren<Camera>(true);
                ImageEffectBase ieb = c.GetComponent<ImageEffectBase>();
                if (show && ieb != null)
                {
                    if (ieb.GetIsDynamic())
                    {
                        obj.gameObject.SetActive(true);
                    }
                    else
                    {
                        obj.gameObject.SetActive(false);
                    }
                }
                else
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }
    }

    private ImageEffectType _CurrentFilter = ImageEffectType.None;
    public Camera _ImageEffectCamera;
    public PostProcessVolume _ImageEffectPostProcessVolume;

    public GameObject _Water;
    public GameObject _RainGlass;
    public GameObject _Frost;
    public GameObject _LightRays;

    private List<ImageEffectType> _LeftNeedRefreshBtn = new List<ImageEffectType>();
    public void OnSetPic(Sprite sp, float picScale)
    {
        _LeftNeedRefreshBtn.Clear();
        _refreshingFilterBtn = false;
        _needRefreshLeftBtn = true;
        _canHide = false;

        _ImageEffectCamera.orthographicSize = 5.0f / picScale;

        _Water.transform.localScale = new Vector3(10.0f / picScale, 10.0f / picScale, 1.0f);
        _RainGlass.transform.localScale = new Vector3(10.0f / picScale, 10.0f / picScale, 1.0f);
        _Frost.transform.localScale = new Vector3(10.0f / picScale, 10.0f / picScale, 1.0f);
        _LightRays.transform.localScale = new Vector3(10.0f / picScale, 10.0f / picScale, 1.0f);


        float x = 128.0f / sp.texture.width;
        float y = 128.0f / sp.texture.height;

        //设置相机大小
        float s = 1.0f;
        s = Math.Max(x, y);
        for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
        {
            Transform obj = _ImageEffectList.transform.Find("" + (ImageEffectType)i);

            SpriteRenderer spr = obj.GetComponentInChildren<SpriteRenderer>();
            spr.sprite = sp;

            Camera c = obj.GetComponentInChildren<Camera>();
            c.orthographicSize = 0.6153846f / s;

            if (i == (int)ImageEffectType.Frost)
            {
                Transform mr = obj.Find("" + (ImageEffectType)i);
                mr.localScale = new Vector3(1.230769f / s, 1.230769f / s, 1.0f);
            }
            else if (i == (int)ImageEffectType.Water)
            {
                Transform mr = obj.Find("" + (ImageEffectType)i);
                mr.localScale = new Vector3(1.230769f / s, 1.230769f / s, 1.0f);
            }
            else if (i == (int)ImageEffectType.RainGlass)
            {
                Transform mr = obj.Find("" + (ImageEffectType)i);
                mr.localScale = new Vector3(1.230769f / s, 1.230769f / s, 1.0f);
            }
            else if (i == (int)ImageEffectType.LightRays)
            {
                Transform mr = obj.Find("" + (ImageEffectType)i);
                mr.localScale = new Vector3(1.230769f / s, 1.230769f / s, 1.0f);
            }

            //此时图标发生变化，需要分开处理
            //优先处理当前可见的图标，剩下的可以是队列刷新，也可以是整体刷新--较慢
            //刷新图标逻辑
            RectTransform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i) as RectTransform;
            if (btn.position.x > -btn.sizeDelta.x / 2 && btn.position.x < Screen.width + btn.sizeDelta.x / 2)
            {
                //可见的图标
                obj.gameObject.SetActive(true);
                btn.Find("Icon").gameObject.SetActive(true);
            }
            else
            {
                //不可见的图标
                obj.gameObject.SetActive(false);
                btn.Find("Icon").gameObject.SetActive(false);
                _LeftNeedRefreshBtn.Add((ImageEffectType)i);
            }
        }

        // 这个仅是处理大图渲染
        //根据图的大小来做一定的等待刷新渲染
        OnEnableCameraRender(Mathf.Sqrt(sp.texture.width * sp.texture.height)/1024f * 0.2f);
    }


    public void OnChageBGColor(Color c,bool isChangeTheme)
    {
        _ImageEffectCamera.backgroundColor = c;
        _filterAdjust.OnChageBGColor(c);

        //更新两个SPRITE GLOW框的颜色
        _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>().UpdateDefaultGlowColor(c);
        _ImageEffectList.transform.Find("" + ImageEffectType.SpriteGlow).GetComponentInChildren<SpriteGlowEffect>().UpdateDefaultGlowColor(c);
        _ImageEffectList.transform.Find("" + ImageEffectType.SpriteGlow).GetComponentInChildren<Camera>().backgroundColor = c;
    }

    public GameObject _AdjustFilterRect;
    public void OnImageEffectBtnClick(string filter)
    {
        ImageEffectType t = GetImageEffectTypeByName(filter);
        if (_CurrentFilter == t)
        {
            switch (_CurrentFilter)
            {
                case ImageEffectType.Bloom:
                    OnOpenFilterAjustPanel(true);
                    break;
                case ImageEffectType.ColorGrading:
                    OnOpenFilterAjustPanel(true);
                    break;
                case ImageEffectType.BlackWhite:
                    OnOpenFilterAjustPanel(true);
                    break;
                case ImageEffectType.Film:
                    OnOpenFilterAjustPanel(true);
                    break;
                case ImageEffectType.LightRays:
                    OnOpenFilterAjustPanel(true);
                    break;
                case ImageEffectType.Notebook:
                    //OnOpenFilterAjustPanel(true);//效率太低，不支持调整了
                    break;
            }

            return;
        }

        _AdjustFilterRect.transform.localPosition = Vector3.zero;

        _CurrentFilter = t;
        _filterAdjust.SetCurrentAdjust(_CurrentFilter);
        //禁用所有脚本以及材质
        _Water.SetActive(false);
        _Frost.SetActive(false);
        _RainGlass.SetActive(false);
        _LightRays.SetActive(false);
        MonoBehaviour[] sc = _ImageEffectCamera.GetComponents<MonoBehaviour>();
        foreach (var b in sc)
        {
            b.enabled = false;
        }

        foreach (var st in _ImageEffectPostProcessVolume.profile.settings)
        {
            st.enabled.Override(false);
        }
        //需要关闭spritglow script
        _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>().enabled = false;
        _ImageEffectPostProcessVolume.transform.localScale = Vector3.one;

        //POST PROCESSING 滤镜
        float delay = 0.0f;
        if ((int)_CurrentFilter >= (int)ImageEffectType.OilPaint)
        {
            _ImageEffectCamera.GetComponent<PostProcessLayer>().enabled = true;
            _ImageEffectPostProcessVolume.enabled = true;
        }
        else
        {
            _ImageEffectCamera.GetComponent<PostProcessLayer>().enabled = false;
            _ImageEffectPostProcessVolume.enabled = false;
        }

        switch (_CurrentFilter)
        {
            case ImageEffectType.None:
                break;
            case ImageEffectType.Ascii:
                _ImageEffectCamera.GetComponent<Ascii>().enabled = true;
                break;
            case ImageEffectType.Bloom:
                _ImageEffectCamera.GetComponent<BloomEffect>().enabled = true;
                break;
            case ImageEffectType.Blur:
                _ImageEffectCamera.GetComponent<BlurEffect>().enabled = true;
                break;
            case ImageEffectType.ColorGrading:
                _ImageEffectCamera.GetComponent<ImageEffects.ColorGrading>().enabled = true;
                break;
            case ImageEffectType.CRT:
                _ImageEffectCamera.GetComponent<CRT>().enabled = true;
                break;
            case ImageEffectType.Distortion:
                _ImageEffectCamera.GetComponent<Distortion>().enabled = true;
                break;
            case ImageEffectType.Division:
                _ImageEffectCamera.GetComponent<Division>().enabled = true;
                break;
            case ImageEffectType.Drunk:
                _ImageEffectCamera.GetComponent<Drunk>().enabled = true;
                break;
            case ImageEffectType.Frost:
                _Frost.SetActive(true);
                break;
            case ImageEffectType.FrostedGlass:
                _ImageEffectCamera.GetComponent<FrostedGlass>().enabled = true;
                break;
            case ImageEffectType.GameBoy:
                _ImageEffectCamera.GetComponent<GameBoy>().enabled = true;
                break;
            case ImageEffectType.Halftoning:
                _ImageEffectCamera.GetComponent<HalftoneEffect>().enabled = true;
                break;
            case ImageEffectType.Mosaic:
                _ImageEffectCamera.GetComponent<Mosaic>().enabled = true;
                break;
            case ImageEffectType.BlackWhite:
                _ImageEffectCamera.GetComponent<BlackWhite>().enabled = true;
                break;
            case ImageEffectType.Film:
                _ImageEffectCamera.GetComponent<Film>().enabled = true;
                break;
            case ImageEffectType.OldTV:
                _ImageEffectCamera.GetComponent<OldTV>().enabled = true;
                break;
            case ImageEffectType.RainGlass:
                _RainGlass.SetActive(true);
                break;
            case ImageEffectType.Sandstorm:
                _ImageEffectCamera.GetComponent<Sandstorm>().enabled = true;
                break;
            case ImageEffectType.Tonemapping:
                _ImageEffectCamera.GetComponent<ImageEffects.Tonemapper>().enabled = true;
                break;
            case ImageEffectType.Water:
                _Water.SetActive(true);
                break;
            case ImageEffectType.WaterColor:
                _ImageEffectCamera.GetComponent<Watercolor>().enabled = true;
                break;
            case ImageEffectType.Fog:
                _ImageEffectCamera.GetComponent<D2FogsNoiseTexPE>().enabled = true;
                break;
            case ImageEffectType.DeferredNV:
                _ImageEffectCamera.GetComponent<DeferredNightVision>().enabled = true;
                break;
            case ImageEffectType.Ice:
                _ImageEffectCamera.GetComponent<Ice>().enabled = true;
                break;
            case ImageEffectType.LightRays:
                _LightRays.SetActive(true);
                break;
            case ImageEffectType.ColorASCII:
                _ImageEffectCamera.GetComponent<ColorASCII>().enabled = true;
                break;
            case ImageEffectType.Comic:
                _ImageEffectCamera.GetComponent<Comic>().enabled = true;
                break;
            case ImageEffectType.Notebook:
                _ImageEffectCamera.GetComponent<Notebook>().enabled = true;
                break;
            case ImageEffectType.Vortex:
                _ImageEffectCamera.GetComponent<Vortex>().enabled = true;
                break;
            //以下是postprocess类型滤镜
            case ImageEffectType.OilPaint:
                OilPaint op;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out op);
                op.enabled.Override(true);
                break;
            case ImageEffectType.Pencil:
                Pencil pc;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out pc);
                pc.enabled.Override(true);
                break;
            case ImageEffectType.Cartoon:
                Cartoon ct;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ct);
                ct.enabled.Override(true);
                break;
            case ImageEffectType.Technicolor:
                Technicolor th;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out th);
                th.enabled.Override(true);
                break;
            case ImageEffectType.FakeHDR:
                FakeHDR fh;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out fh);
                fh.enabled.Override(true);
                break;
            case ImageEffectType.Ramp:
                Ramp rmp;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out rmp);
                rmp.enabled.Override(true);
                break;
            case ImageEffectType.Streak:
                Streak sk;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out sk);
                sk.enabled.Override(true);
                break;
            case ImageEffectType.Grayscale:
                Grayscale gry;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out gry);
                gry.enabled.Override(true);
                break;
            case ImageEffectType.MotionBlur:
                MotionBlur mb;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out mb);
                mb.enabled.Override(true);
                delay = 0.1f;
                break;
            case ImageEffectType.Vignette:
                Vignette vt;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out vt);
                vt.enabled.Override(true);
                break;
            case ImageEffectType.PPBloom:
                Bloom bl;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out bl);
                bl.enabled.Override(true);

                //需要重新设置默认参数，因为二者的并不同
                bl.intensity.Override(30.0f);
                bl.threshold.Override(1.0f);
                bl.dirtIntensity.Override(0f);
                break;

            case ImageEffectType.AnalogGlitch:
                AnalogGlitch ag;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ag);
                ag.enabled.Override(true);
                break;
            case ImageEffectType.DigitalGlitch:
                DigitalGlitch dg;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out dg);
                dg.enabled.Override(true);
                delay = 0.2f;

                break;
            case ImageEffectType.SpriteGlow:
                Bloom blSg;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out blSg);
                blSg.enabled.Override(true);
                //需要重新设置默认参数，因为二者的并不同
                blSg.intensity.Override(1.0f);
                blSg.threshold.Override(1.5f);
                blSg.diffusion.Override(7f);
                blSg.softKnee.Override(0.5f);
                blSg.dirtIntensity.Override(5f);

                //同时需要开启spritglow script
                _ImageEffectPostProcessVolume.GetComponent<SpriteGlowEffect>().enabled = true;

                //需要缩小图片以显示完整
                _ImageEffectPostProcessVolume.transform.localScale = new Vector3(0.9f,0.9f);
                break;
            case ImageEffectType.ChromaticAberration:
                ChromaticAberration ca;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out ca);
                ca.enabled.Override(true);

                //需要缩小图片以显示完整
                _ImageEffectPostProcessVolume.transform.localScale = new Vector3(0.9f, 0.9f);

                break;
            case ImageEffectType.ThermalVision:
                ThermalVision tv;
                _ImageEffectPostProcessVolume.profile.TryGetSettings(out tv);
                tv.enabled.Override(true);
                break;

        }

        OnEnableCameraRender(delay);

        //处理材质
        for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
        {
            Transform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i);

            RawImage ri = btn.Find("Icon").GetComponent<RawImage>();

            if ("" + (ImageEffectType)i == filter)
            {
                ri.material = Resources.Load("icon/CircleBtnMask1", typeof(Material)) as Material;
            }
            else
            {
                if (ri.material.name == "CircleBtnMask1")
                {
                    ri.material = Resources.Load("icon/CircleBtnMask2", typeof(Material)) as Material;
                }
                else
                {
                    //不需要重置
                }
            }
        }

        // 提示操作
        ShowOpenAdjustToast();
    }

    private bool _needDisableCameraRender = false;
    private float _DelayDisableCameraRender = 0.0f;
    private bool _needRefreshLeftBtn = false;
    private bool _refreshingFilterBtn = false;
    private ImageEffectType _CurrentRefresh = ImageEffectType.End;
    public void OnGUI()
    {
        if (_needDisableCameraRender)
        {
            _needDisableCameraRender = false;

            if (_DelayDisableCameraRender > 0.0f)
            {
                Invoke("DelayCheck", _DelayDisableCameraRender);
                _DelayDisableCameraRender = 0.0f;
            }
            else
            {
                DelayCheck();
            }
        }

        if (!_needRefreshLeftBtn && _refreshingFilterBtn)
        {
            if (_CurrentRefresh != ImageEffectType.End)
            {
                Transform obj2 = _ImageEffectList.transform.Find("" + _CurrentRefresh);
                obj2.gameObject.SetActive(false);

                Camera c = obj2.GetComponentInChildren<Camera>(true);
                ImageEffectBase ieb = c.GetComponent<ImageEffectBase>();
                if (ieb != null)
                {
                    if (ieb.GetIsDynamic())
                    {
                        obj2.gameObject.SetActive(true);
                    }
                    else
                    {
                        obj2.gameObject.SetActive(false);
                    }
                }
                else
                {
                    obj2.gameObject.SetActive(false);
                }
            }

            if (_LeftNeedRefreshBtn.Count == 0)
            {
                _CurrentRefresh = ImageEffectType.End;
                _refreshingFilterBtn = false;
                //mask
                Invoke("DelayCanHide",2.0f);

                //台风图标无法正常显示，这里重新进行设置
                Transform vortex = _ImageEffectList.transform.Find("" + ImageEffectType.Vortex);
                vortex.GetComponentInChildren<Vortex>().enabled = true;

                return;
            }

            _refreshingFilterBtn = false;
            _CurrentRefresh = _LeftNeedRefreshBtn[0];
            Transform obj = _ImageEffectList.transform.Find("" + _CurrentRefresh);

            //刷新当前
            _LeftNeedRefreshBtn.RemoveAt(0);
            obj.gameObject.SetActive(true);

            Invoke("DelayRefreshLeftBtn", 0.05f);

        }
    }
    private void DelayRefreshLeftBtn()
    {
        _refreshingFilterBtn = true;
    }

    private void DelayCheck()
    {
        if (_needRefreshLeftBtn)
        {
            _needRefreshLeftBtn = false;//此时可见的图标已经刷新完毕
                                        //接下来需要依次刷新剩余的图标
            _refreshingFilterBtn = true;
            for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
            {
                if ((!_LeftNeedRefreshBtn.Contains((ImageEffectType)i)))
                {
                    Transform obj = _ImageEffectList.transform.Find("" + (ImageEffectType)i);
                    Camera c = obj.GetComponentInChildren<Camera>(true);
                    ImageEffectBase ieb = c.GetComponent<ImageEffectBase>();
                    if (ieb != null)
                    {
                        if (ieb.GetIsDynamic())
                        {
                            obj.gameObject.SetActive(true);
                        }
                        else
                        {
                            obj.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        obj.gameObject.SetActive(false);
                    }
                }
            }
        }

        DisableCameraRender();
    }

    //调节参数开启关闭渲染
    public void OnEnableCameraRender(float delay)
    {
        _needDisableCameraRender = true;
        _DelayDisableCameraRender = delay;

        _ImageEffectPostProcessVolume.gameObject.SetActive(true);
        _ImageEffectCamera.gameObject.SetActive(true);
    }

    private void DisableCameraRender()
    {
        Transform obj = _ImageEffectList.transform.Find("" + _CurrentFilter);
        Camera c = obj.GetComponentInChildren<Camera>(true);
        ImageEffectBase ieb = c.GetComponent<ImageEffectBase>();
        if ((ieb != null && ieb.GetIsDynamic())
            ||_CurrentFilter == ImageEffectType.MotionBlur)
        {
            // 动态滤镜不能停止渲染
            return;
        }

        _ImageEffectPostProcessVolume.gameObject.SetActive(false);
        _ImageEffectCamera.gameObject.SetActive(false);

    }

    private ImageEffectType GetImageEffectTypeByName(string type)
    {
        ImageEffectType ret = ImageEffectType.None;

        switch (type)
        {
            case "None":
                ret = ImageEffectType.None;
                break;
            case "Ascii":
                ret = ImageEffectType.Ascii;
                break;
            case "Bloom":
                ret = ImageEffectType.Bloom;
                break;
            case "Blur":
                ret = ImageEffectType.Blur;
                break;
            case "ColorGrading":
                ret = ImageEffectType.ColorGrading;
                break;
            case "CRT":
                ret = ImageEffectType.CRT;
                break;
            case "Distortion":
                ret = ImageEffectType.Distortion;
                break;
            case "Division":
                ret = ImageEffectType.Division;
                break;
            case "Drunk":
                ret = ImageEffectType.Drunk;
                break;
            case "Frost":
                ret = ImageEffectType.Frost;
                break;
            case "FrostedGlass":
                ret = ImageEffectType.FrostedGlass;
                break;
            case "GameBoy":
                ret = ImageEffectType.GameBoy;
                break;
            case "Halftoning":
                ret = ImageEffectType.Halftoning;
                break;
            case "Mosaic":
                ret = ImageEffectType.Mosaic;
                break;
            case "BlackWhite":
                ret = ImageEffectType.BlackWhite;
                break;
            case "Film":
                ret = ImageEffectType.Film;
                break;
            case "OldTV":
                ret = ImageEffectType.OldTV;
                break;
            case "RainGlass":
                ret = ImageEffectType.RainGlass;
                break;
            case "Sandstorm":
                ret = ImageEffectType.Sandstorm;
                break;
            case "Tonemapping":
                ret = ImageEffectType.Tonemapping;
                break;
            case "Water":
                ret = ImageEffectType.Water;
                break;
            case "WaterColor":
                ret = ImageEffectType.WaterColor;
                break;
            case "OilPaint":
                ret = ImageEffectType.OilPaint;
                break;
            case "Pencil":
                ret = ImageEffectType.Pencil;
                break;
            case "Cartoon":
                ret = ImageEffectType.Cartoon;
                break;
            case "Technicolor":
                ret = ImageEffectType.Technicolor;
                break;
            case "FakeHDR":
                ret = ImageEffectType.FakeHDR;
                break;
            case "Ramp":
                ret = ImageEffectType.Ramp;
                break;
            case "Streak":
                ret = ImageEffectType.Streak;
                break;
            case "Grayscale":
                ret = ImageEffectType.Grayscale;
                break;
            case "MotionBlur":
                ret = ImageEffectType.MotionBlur;
                break;
            case "Vignette":
                ret = ImageEffectType.Vignette;
                break;
            case "PPBloom":
                ret = ImageEffectType.PPBloom;
                break;

            case "AnalogGlitch":
                ret = ImageEffectType.AnalogGlitch;
                break;
            case "DigitalGlitch":
                ret = ImageEffectType.DigitalGlitch;
                break;
            case "SpriteGlow":
                ret = ImageEffectType.SpriteGlow;
                break;
            case "Fog":
                ret = ImageEffectType.Fog;
                break;
            case "Ice":
                ret = ImageEffectType.Ice;
                break;
            case "LightRays":
                ret = ImageEffectType.LightRays;
                break;
            case "DeferredNV":
                ret = ImageEffectType.DeferredNV;
                break;
            case "ChromaticAberration":
                ret = ImageEffectType.ChromaticAberration;
                break;
            case "ColorASCII":
                ret = ImageEffectType.ColorASCII;
                break;
            case "Comic":
                ret = ImageEffectType.Comic;
                break;
            case "Notebook":
                ret = ImageEffectType.Notebook;
                break;
            case "Vortex":
                ret = ImageEffectType.Vortex;
                break;
            case "ThermalVision":
                ret = ImageEffectType.ThermalVision;
                break;
        }

        return ret;
    }

    //-------------------------------购买装饰-----------------------------------
    public IAP _iap;
    private bool _ProcessingPurchase = false;

    //此处需要判断购买的是哪个内购
    public void OnBuyBtnClick()
    {

        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            _iap.onBuyClick(IAP.GetAppIDByName("IAP_LVJING"));
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍后...");
        }
    }



    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        if (inAppID != IAP.IAP_LVJING)
        {
            return;
        }

        _ProcessingPurchase = false;
        if (ret)
        {
            // 购买成功，根据inAppID，刷新界面
            if (!string.IsNullOrEmpty(receipt))
            {
                //说明是真正的购买，其他可能是正在购买或者已经购买过了
            }

            //如果lock不可见，不必执行动画
            if (gameObject.activeSelf)
            {
                for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
                {
                    Transform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i);

                    if (!GetIsFreeFilter((ImageEffectType)i))
                    {
                        GameObject buyBtn = btn.Find("BuyBtn").gameObject;
                        DoUnLockAni(buyBtn, () => {
                            buyBtn.SetActive(false);
                        });
                    }
                }
            }

            OnReportEvent(ret, EventReport.BuyType.BuySuccess);
        }
        else
        {
            //简单提示，购买失败
            ShowToast("购买失败，请稍后再试：(");
            OnReportEvent(ret, EventReport.BuyType.BuyFail);
        }
    }

    public void OnRestoreCallback(bool ret, string inAppID)
    {
        if (inAppID != IAP.IAP_LVJING)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            //如果lock不可见，不必执行动画
            if (gameObject.activeSelf)
            {
                for (int i = (int)ImageEffectType.None; i < (int)ImageEffectType.End; i++)
                {
                    Transform btn = _ImageEffectBtnContent.transform.Find("" + (ImageEffectType)i);

                    if (!GetIsFreeFilter((ImageEffectType)i))
                    {
                        GameObject buyBtn = btn.Find("BuyBtn").gameObject;
                        DoUnLockAni(buyBtn, () => {
                            buyBtn.SetActive(false);
                        });
                    }
                }
            }
        }
        else
        {
            //MessageBoxManager.Show("", "恢复购买失败，请确认是否购买过？");
        }

        OnReportEvent(ret, EventReport.BuyType.BuyRestore);
    }

    private void DoUnLockAni(GameObject lockObj, Action cb = null)
    {
        Image[] ll = lockObj.GetComponentsInChildren<Image>(true);

        Image bg = ll[1];
        Image lockImg = ll[2];
        Image unlockImg = ll[3];
        foreach (var img in ll)
        {
            if (img.name == "BuyLockBtnImg")
            {
                lockImg = img;
            }
            if (img.name == "BuyUnLockBtnImg")
            {
                unlockImg = img;
            }
            if (img.name == "Bg")
            {
                bg = img;
            }
        }

        Sequence mySequence = DOTween.Sequence();
        unlockImg.gameObject.SetActive(true);
        mySequence
            .Append(unlockImg.DOFade(0.0f, 0.0f))
            .Append(unlockImg.transform.DOScale(1.5f, 0.0f))
            .Append(lockImg.DOFade(0.0f, 0.5f))
            .Join(lockImg.transform.DOScale(1.5f, 0.5f))
            .SetEase(Ease.InSine)
            .Append(unlockImg.DOFade(1.0f, 0.8f))
            .Join(unlockImg.transform.DOScale(1.0f, 0.8f))
            .Append(unlockImg.transform.DOShakeRotation(1.0f, 45.0f))
            .Append(unlockImg.DOFade(0.0f, 0.5f))
            .Join(bg.DOFade(0.0f, 1.0f))
            .SetEase(Ease.InSine)
            .OnComplete(() => {

                if (cb != null)
                {
                    cb();
                }
            });
    }

    [System.Serializable] public class OnEventReport : UnityEvent<string> { }
    public OnEventReport ReportEvent;
    public void OnReportEvent(bool success,
                            EventReport.BuyType buyType)
    {
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.LJBtnBuyClick + "_" + success);
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = _ImageEffectCamera.backgroundColor;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }

    private void ShowOpenAdjustToast()
    {

        if (_CurrentFilter == ImageEffectType.Bloom
            || _CurrentFilter == ImageEffectType.ColorGrading
            || _CurrentFilter == ImageEffectType.BlackWhite
            || _CurrentFilter == ImageEffectType.Film)
        {
            int tipCnt = Setting.getPlayerPrefs("MORE" + Setting.SETTING_KEY.SHOW_ADJUST_FILTER_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                ShowToast("再点一次可以调节更详细的参数");
                Setting.setPlayerPrefs("MORE" + Setting.SETTING_KEY.SHOW_ADJUST_FILTER_TIP, tipCnt + 1);
            }
        }
        else
        {
            //根据当前选择的滤镜进行提示
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_FILTER_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT * 2)
            {
                ShowToast("<b>单指触摸</b>图片，<b>左右/上下/斜方向</b>滑动，调整滤镜效果", 5f);
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_FILTER_TIP, tipCnt + 1);
            }
            else if (tipCnt < Define.BASIC_TIP_CNT * 3)
            {
                ShowToast("<b>单指双击</b>图片，可以恢复到默认滤镜效果。");
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_FILTER_TIP, tipCnt + 1);
            }
            else if(tipCnt < Define.BASIC_TIP_CNT * 4)
            {
                ShowToast("并不是所有滤镜都支持8个方向的滑动调节哟～");
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_FILTER_TIP, tipCnt + 1);
            }
        }
    }

    public void OnOpenFilterAjustPanel(bool open)
    {
        Transform btn = _ImageEffectBtnContent.transform.Find("" + _CurrentFilter);
        string title = btn.Find("BtnText").GetComponent<Text>().text;

        _filterAdjust.ShowAdjust(title,open);
    }

    public Image _ShowFilterDirection;
    public void ShowFilterDirection(Color BgColor, int num/*共有几个参数*/)
    {
        Color fc = Define.GetFixColor(BgColor);

        _ShowFilterDirection.gameObject.SetActive(true);
        this.gameObject.SetActive(true);

        Image[] dirs = _ShowFilterDirection.GetComponentsInChildren<Image>(true);
        for (int i = 1; i < dirs.Length; i++)
        {
            dirs[i].color = new Color(fc.r, fc.g, fc.b, 0.0f);
        }
    }
}
