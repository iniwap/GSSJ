using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Reign;
using System;
using System.Text.RegularExpressions;
using System.IO;


public class CaiJian : MonoBehaviour
{
    public void Start()
    {

    }
    public Camera _mainCamera;
    public Camera _themeCamera;
    //OnExitSplash
    [Serializable] public class OnExitSplashEvent : UnityEvent<Color> { }
    public OnExitSplashEvent OnExitSplash;
    public void OnInit()
    {
        //初始化装饰参数
        _CurrentHZParam.zsImgPath = "";
        _CurrentHZParam.zsImgColor = Define.BG_COLOR_50;
        _CurrentHZParam.zsImgSize = 1.0f;

        //启动，指定颜色，不再随机颜色
        List<string> cinfo = PickColor.GetColorByID(Setting.GetStartColorID());//上次保存的主题色

        Color bgc = new Color(int.Parse(cinfo[3]) / 255.0f, int.Parse(cinfo[4]) / 255.0f, int.Parse(cinfo[5]) / 255.0f);
        _CurrentHZParam.zsHZColor = Define.GetUIFontColorByBgColor(bgc, Define.eFontAlphaType.FONT_ALPHA_255);

        Color spColor = Define.DARKBG_SP_COLOR;
        if (Define.GetIfUIFontBlack(bgc))
        {
            spColor = Define.LIGHTBG_SP_COLOR;
        }
        _CurrentHZParam.hsImgColor = spColor;
        _CurrentHZParam.hsImgPath = "";
        _CurrentHZParam.hsImgSize = Define.DEFAULT_HS_SIZE;

        this.gameObject.SetActive(true);

        Invoke("DelaySetPos", 0.1f);

        ChangeColor(false);
        UseAlignment(Setting.GetAlignmentType(), GetShiJuAndSign());

        OnExitSplash.Invoke(_mainCamera.backgroundColor);

        //检测启动次数
        CheckStartCnt();

        //重新设置
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;
        float ipad = FitUI.GetIsPad() ? 0.8f : 1.0f;
        float w = nw * ipad - 56;
        picAreaRt.sizeDelta = new Vector2(w, w);

        RectTransform fxAreaRt = _FXArea.GetComponent<RectTransform>();
        fxAreaRt.sizeDelta = new Vector2(w, w);
    }

    private void DelaySetPos()
    {
        _TextAreaPos = _TextArea.transform.position;
        _PicAreaPos = _picArea.transform.position;
        _ColorInfoPos = _colorInfo.transform.position;
        _TopMenuPos = _TopMenu.transform.localPosition;

    }

    public GameObject _startMask;//for fix mask bug
    public void CheckStartCnt() {
        int cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.START_CNT, 0);
        if (cnt == 0)
        {
            _startMask.SetActive(true);
            //首次启动
            Invoke("ShowDragTips", 3.0f);
        }
        else
        {
            if (UnityEngine.Random.value < 0.2f)
            {
                ShowToast("【更多】子菜单里有非常多的功能、玩法。",2f,2f);
            }
            else if (UnityEngine.Random.value < 0.3f)
            {
                ShowToast("【挑战】与诗词有关的游戏，多种玩法。", 2f, 2f);
            }
            else if (UnityEngine.Random.value < 0.4f)
            {
                ShowToast("使用【写信】创建属于自己的图文。", 2f, 2f);
            }
            else if (UnityEngine.Random.value < 0.5f)
            {
                ShowToast("【配图】类似插图、底图，同时支持超多滤镜以及相框。", 3f, 2f);
            }
            else if (UnityEngine.Random.value < 0.6f)
            {
                ShowToast("【红线】将函数与诗词结合，更加丰富的排版样式。",  3f, 2f);
            }
            else if (UnityEngine.Random.value < 0.7f)
            {
                ShowToast("【装饰】诗文、行线参数样式调整，也有动态背景。",  3f, 2f);
            }
            else if (UnityEngine.Random.value < 0.8f)
            {
                ShowToast("【辨色】独家原创系列诗色玩法，简单有趣。", 2f, 2f);
            }
            else if (UnityEngine.Random.value < 0.9f)
            {
                ShowToast("【设置】满足更多个性化配置。", 2f, 2f);
            }
            else
            {
                ShowToast("诗色有非常多独特的功能，如乱句、网文等。", 3f, 2f);
            }
        }

        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.START_CNT, cnt + 1);
    }
    public void ShowDragTips()
    {
        _mask.GetComponent<MaskTips>().ShowDragTips(_mainCamera.backgroundColor,()=> {
            _startMask.SetActive(false);
            ShowToast("诗色是一个宝藏应用，希望你会喜欢上它:)");
        });
    }

    public void CheckGestureTips() {
        //显示一次手势操作
        int cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HAS_SHOW_GESTURE, 0);
        if (cnt <= 1)
        {
            _leanSelect.SetActive(false);//此时不允许取消选中，否则会很奇怪
            _mask.GetComponent<MaskTips>().ShowGestureTips(_mainCamera.backgroundColor,()=> {
                _leanSelect.SetActive(true);
            });
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.HAS_SHOW_GESTURE, cnt + 1);
        }
    }

    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    //------------------------测试中日传统色-------------------------------------
    public GameObject _colorInfo;
    public GameObject _leanSelect;
    public GameObject _leanTouch;
    public GameObject _leanSwipe;
    //图片区域
    public GameObject _picArea;
    public GameObject _FXArea;

    //诗句区域
    public Transform _TextVerticalArea;
    public Transform _TextHorizontalArea;
    public GameObject _VerticalLinePrefab;
    public GameObject _HorizontalLinePrefab;
    public GameObject _VerticalTextPrefab;
    public GameObject _HorizontalTextPrefab;

    private List<GameObject> _LineList = new List<GameObject>();
    private List<GameObject> _TextList = new List<GameObject>();


    //改变显示行数--仅对下次有效，且自输入不受该项控制
    public void OnChangeShowSJLine(Setting.ShowSJLine line)
    {
        //Setting.GetShowSJLine = line;
    }

    //显隐分割线
    public void OnChangeShowLineSeparator(bool show)
    {
        foreach (var hz in _TextList)
        {
            CJHZ cjhz = hz.GetComponent<CJHZ>();
            cjhz.SP.gameObject.SetActive(show);
        }

        _FXArea.transform.Find("Viewport/Content/FXLine").gameObject.SetActive(show);
    }

    public void OnChangeFontBold(bool bold)
    {
      
        foreach (var hz in _TextList)
        {
            CJHZ cjhz = hz.GetComponent<CJHZ>();
            cjhz.SetHZFonStyle(bold);
        }
    }

    public void OnChangeShowAuthorLine(bool show)
    {

        if (_LineList.Count > 1)
        {
            //存在作者行
            CJHZ[] cjhz = _LineList[_LineList.Count - 1].GetComponentsInChildren<CJHZ>();

            if (cjhz[0].HZ.fontSize != _LineList[0].GetComponentsInChildren<CJHZ>()[0].HZ.fontSize)
            {
                Sequence mySequence2 = DOTween.Sequence();
                if (show)
                {
                    for (int ts = 0; ts < cjhz.Length; ts++)
                    {
                        mySequence2
                            .Join(cjhz[ts].HZ.DOFade(1.0f, 1.5f))
                            .Join(cjhz[ts].SP.DOFade(1.0f, 1.5f));
                        if (_CurrentHZParam.zsImgPath != "") {
                            mySequence2.Join(cjhz[ts].ZhuangShiImg.DOFade(_CurrentHZParam.zsImgColor.a, 1.5f));
                        }
                    }

                    mySequence2
                        .SetEase(Ease.OutBounce);
                }
                else
                {
                    for (int ts = 0; ts < cjhz.Length; ts++)
                    {
                        mySequence2
                            .Join(cjhz[ts].SP.DOFade(0.0f, 1.5f))
                            .Join(cjhz[ts].HZ.DOFade(0.0f, 1.5f));

                        if (_CurrentHZParam.zsImgPath != "")
                        {
                            mySequence2.Join(cjhz[ts].ZhuangShiImg.DOFade(0.0f, 1.5f));
                        }
                    }

                    mySequence2.SetEase(Ease.InBounce);
                }
            }
        }
    }

    //设置触发
    public void OnChangeAlignmentType(Setting.AlignmentType type)
    {
        UseAlignment(type);
    }

    //普通触发
    private void UseAlignment(Setting.AlignmentType type, List<string> shiJu = null,bool changeByFX = false)
    {

        if (!_UsingFX)
        {
            if (type == Setting.AlignmentType.LEFT_VERTICAL || type == Setting.AlignmentType.RIGHT_VERTICAL)
            {
                _TextVerticalArea.gameObject.SetActive(true);
                _TextHorizontalArea.gameObject.SetActive(false);
            }
            else
            {
                _TextVerticalArea.gameObject.SetActive(false);
                _TextHorizontalArea.gameObject.SetActive(true);
            }
        }

        // 重新布局
        if (shiJu == null)
        {
            //设置触发，需要用当前显示的诗句
            List<string> sj = GetCurrentShiJu();

            if (sj.Count >= 2)
            {
                if (sj[sj.Count - 1].Contains("||") || sj[sj.Count - 1].Contains("——"))
                {
                    if (Setting.GetShowAuthorLine())
                    {
                        if (type == Setting.AlignmentType.LEFT_HORIZONTAL || type == Setting.AlignmentType.RIGHT_HORIZONTAL)
                        {
                            sj[sj.Count - 1] = sj[sj.Count - 1].Replace("||", "——");
                        }
                        else
                        {
                            sj[sj.Count - 1] = sj[sj.Count - 1].Replace("——", "||");
                        }
                    }
                }
            }

            ChangeShiJu(sj, changeByFX);
        }
        else
        {
            ChangeShiJu(shiJu, changeByFX);
        }
    }

    private List<string> GetCurrentShiJu()
    {
        List<string> ret = new List<string>();

        foreach (var ln in _LineList)
        {
            Text[] sj = ln.GetComponentsInChildren<Text>();
            string lnTxt = "";
            foreach (var hz in sj)
            {
                lnTxt += hz.text;
            }
            ret.Add(lnTxt);
        }

        return ret;
    }


    [System.Serializable] public class OnChangeBGColorEvent : UnityEvent<Color,bool> { }
    public OnChangeBGColorEvent OnChangeBGColor;
    public Color ChangeColor(bool needAni = true)
    {
        if (needAni)
        {
            CurrentColorInfo = HZManager.GetInstance().GetColor(HZManager.eColorType.BOTH);
        }
        else
        {
            //启动，指定颜色，不再随机颜色
            CurrentColorInfo = PickColor.GetColorByID(Setting.GetStartColorID());//雪白

        }

        Color BgColor = new Color(int.Parse(CurrentColorInfo[3]) / 255.0f,
                                  int.Parse(CurrentColorInfo[4]) / 255.0f,
                                  int.Parse(CurrentColorInfo[5]) / 255.0f);

        ChangeColor(BgColor, needAni);

        return BgColor;
    }
    private void ChangeThemeColor(Color BgColor)
    {
        //设置区域选中标志颜色
        Image[] ts = _TextArea.transform.Find("Selected").GetComponentsInChildren<Image>();
        Image[] ps = _picArea.transform.Find("Selected").GetComponentsInChildren<Image>();

        foreach (var img in ts)
        {
            img.color = BgColor * 0.5f;
        }

        foreach (var img in ps)
        {
            img.color = BgColor * 0.5f;
        }

        Text colorType = _colorInfo.transform.Find("Type").GetComponent<Text>();
        Text colorName = _colorInfo.transform.Find("Name").GetComponent<Text>();
        Text colorPY = _colorInfo.transform.Find("PY").GetComponent<Text>();

        Color c2 = new Color(200 / 255.0f, 200 / 255.0f, 200 / 255.0f, 0.5f);
        if (Define.GetIfUIFontBlack(BgColor))
        {
            c2 = new Color(50 / 255.0f, 50 / 255.0f, 50 / 255.0f, 0.5f);
        }

        colorType.DOColor(c2, 1.0f).SetEase(Ease.InSine);
        colorName.DOColor(c2, 1.0f).SetEase(Ease.InSine);
        colorPY.GetComponent<Text>().DOColor(c2, 1.0f).SetEase(Ease.InSine);

        //滤镜界面
        GameObject OC = _picArea.transform.Find("OK&Cancel").gameObject;

        Text[] BtnText = OC.GetComponentsInChildren<Text>();
        Color c = Define.GetUIFontColorByBgColor(BgColor, Define.eFontAlphaType.FONT_ALPHA_128);
        foreach (var t in BtnText)
        {
            t.color = c;
        }

        Image[] BtnImage = OC.GetComponentsInChildren<Image>();
        Color bc = Define.GetFixColor(c);
        foreach (var img in BtnImage)
        {
            if (img.name == "BtnImg")
                img.color = bc;
        }

        GameObject sl = _picArea.transform.Find("Selected").gameObject;
        Image[] b = sl.GetComponentsInChildren<Image>();
        foreach (var img in b)
        {
            img.color = BgColor * 0.5f;
        }

    }
    //切换情景触发的主题色调变化
    public void OnChangeThemeColor(Color BgColor)
    {
        ChangeColor(BgColor,true,true);
    }

    private void ChangeColor(Color BgColor, bool needAni,bool isChangeTheme = false)
    {
        OnChangeBGColor.Invoke(BgColor, isChangeTheme);

        if (!needAni)
        {
            _mainCamera.backgroundColor = BgColor;
            //短暂停留，自动收起菜单
            _mask.gameObject.SetActive(true);
            Invoke("ExpandTopMenu", 2.0f);
        }
        else
        {
            _mainCamera.DOColor(BgColor, 1.0f);
        }

        ChangeThemeColor(BgColor);

        DoTopMenuMoreBtnAni(BgColor);//重置更多btn动画

        if (isChangeTheme) return;//不更新颜色信息区域

        //设置界面需要知道colorid
        _setting.SetCurrentBGColorID(int.Parse(CurrentColorInfo[7]));
        //颜色信息区域
        Text colorType = _colorInfo.transform.Find("Type").GetComponent<Text>();
        Text colorName = _colorInfo.transform.Find("Name").GetComponent<Text>();
        Text colorPY = _colorInfo.transform.Find("PY").GetComponent<Text>();
        if (!needAni)
        {
            colorType.text = "【写给你的信】";
            colorName.text = "诗色";
            colorPY.text = "APP";
            RepeatRotateColorInfo(false);
        }
        else
        {
            List<string> cinfo = GetColorInfo(colorPY.text.Equals("APP"),true);

            _colorInfo.transform.DOKill(false);

            DOTween.Kill("ChangeColor", false);
            DOTween.Kill("RepeatRotateColorInfo");

            Quaternion r = _colorInfo.transform.localRotation;

            Sequence mySequence = DOTween.Sequence();
            mySequence.SetId("ChangeColor");
            mySequence
                .Append(_colorInfo.transform.DOLocalRotate(new Vector3(r.x, 180, r.z), 1.0f))
                .Join(colorType.DOText(cinfo[0], 1.0f))
                .Join(colorType.DOFade(0.05f, 1.0f))
                .Join(colorName.DOText(cinfo[1], 1.0f))
                .Join(colorName.DOFade(0.05f, 1.0f))
                .Join(colorPY.DOText(cinfo[2], 1.0f))
                .Join(colorPY.DOFade(0.05f, 1.0f))
                .Join(_colorInfo.transform.DOScale(0.8f, 1.0f))
                .SetEase(Ease.InSine)
                .Append(_colorInfo.transform.DOLocalRotate(new Vector3(r.x, 360, r.z), 1.0f))
                .Join(colorType.DOFade(0.5f, 1.0f))
                .Join(colorName.DOFade(0.5f, 1.0f))
                .Join(colorPY.DOFade(0.5f, 1.0f))
                .Join(_colorInfo.transform.DOScale(1.0f, 1.0f))
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    RepeatRotateColorInfo(true);
                });
        }
    }
    private List<string> GetColorInfo(bool app,bool needAni)
    {
        List<string> cinfo = new List<string>();

        string ctype = "";
        string cname = "";
        string cpy = "";

        if (app)
        {
            if (_themeCamera.gameObject.activeSelf || (needAni && Setting.GetTheme()))
            {
                ctype = "【朝夕相伴】";
                cname = Define.GetWeekName(Define.GetWeek());
                cpy = Define.GetHourName(Define.GetHourType(),true);
            }
            else
            {
                if (CurrentColorInfo[6] == "" + (int)HZManager.eColorType.CN)
                {
                    ctype = "【中国传统色】";
                }
                else if (CurrentColorInfo[6] == "" + (int)HZManager.eColorType.JP)
                {
                    ctype = "【日本传统色】";
                }
                else
                {
                    ctype = "【我的颜色】";
                }

                cname = CurrentColorInfo[0];
                cpy = CurrentColorInfo[1];
            }
        }
        else
        {
            ctype = "【写给你的信】";
            cname = "诗色";
            cpy = "APP";
        }

        cinfo.Add(ctype);
        cinfo.Add(cname);
        cinfo.Add(cpy);

        return cinfo;
    }
    private List<string> CurrentColorInfo;
    private void RepeatRotateColorInfo(bool needAni)
    {

        Text colorType = _colorInfo.transform.Find("Type").GetComponent<Text>();
        Text colorName = _colorInfo.transform.Find("Name").GetComponent<Text>();
        Text colorPY = _colorInfo.transform.Find("PY").GetComponent<Text>();

        List<string> cinfo = GetColorInfo(colorPY.text.Equals("APP"), needAni);

        Quaternion r = _colorInfo.transform.localRotation;

        DOTween.Kill("RepeatRotateColorInfo");
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("RepeatRotateColorInfo");
        mySequence
            .AppendInterval(HZManager.GetInstance().GenerateRandomInt(5, 15))
            .Append(_colorInfo.transform.DOLocalRotate(new Vector3(r.x, 180, r.z), 1.0f))
            .Join(colorType.DOText(cinfo[0], 1.0f))
            .Join(colorType.DOFade(0.05f, 1.0f))
            .Join(colorName.DOText(cinfo[1], 1.0f))
            .Join(colorName.DOFade(0.05f, 1.0f))
            .Join(colorPY.DOText(cinfo[2], 1.0f))
            .Join(colorPY.DOFade(0.05f, 1.0f))
            .Join(_colorInfo.transform.DOScale(0.8f, 1.0f))
            .SetEase(Ease.InSine)
            .Append(_colorInfo.transform.DOLocalRotate(new Vector3(r.x, 360, r.z), 1.0f))
            .Join(colorType.DOFade(0.5f, 1.0f))
            .Join(colorName.DOFade(0.5f, 1.0f))
            .Join(colorPY.DOFade(0.5f, 1.0f))
            .Join(_colorInfo.transform.DOScale(1.0f, 1.0f))
            .SetEase(Ease.OutSine)
            .OnComplete(() => {
                RepeatRotateColorInfo(true);
            });
    }

    private bool _UsingFX = false;
    [Serializable] public class OnSetParamEvent : UnityEvent<CJHZ.HZParam> { }
    public OnSetParamEvent OnSetParam;
    public void OnUseFX(bool use,List<string> sj)
    {
        _UsingFX = use;

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();

        if (use)
        {
            _TextHorizontalArea.gameObject.SetActive(false);
            _TextVerticalArea.gameObject.SetActive(false);
        }
        else
        {
            if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
               || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
            {
                _TextHorizontalArea.gameObject.SetActive(true);
            }
            else
            {
                _TextVerticalArea.gameObject.SetActive(true);
            }

            //直接刷新当前诗句，并设置可以点击
            UseAlignment(Setting.GetAlignmentType(), sj,true);
        }

        if (use && !_FXArea.activeSelf)
        {
            //切换普通模式到fx，需要设置当前参数
            OnSetParam.Invoke(_CurrentHZParam);
            bool show = Setting.GetShowLineSeparator();
            _FXArea.transform.Find("Viewport/Content/FXLine").gameObject.SetActive(show);

            if (!show)
            {
                int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_NO_FX_LINE_TIP, 0);
                if (tipCnt < Define.BASIC_TIP_CNT)
                {
                    ShowToast("当前设置为不显示红线，可到设置界面开启<b>显示行线</b>", 3f);
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_NO_FX_LINE_TIP, tipCnt + 1);
                }
            }
        }
    }

    [Serializable] public class OnSetSJEvent : UnityEvent<List<string>, bool> { }
    public OnSetSJEvent OnSetSJ;

    public void ChangeShiJu(List<string> tsscList,bool changeByFX = false)
    {
        if (tsscList.Count == 0) return;

        bool sal = Setting.GetShowAuthorLine();
        if (_UsingLike)
        {
            sal = _UsingShowAuthorLine;
        }

        //只有不是曲线模式触发的切换，才可以设置
        if (!changeByFX)
        {
            //曲线模式不支持显示作者行
            OnSetSJ.Invoke(tsscList, sal);
        }

        //需要判断当前是否显示曲线模式
        if (_UsingFX) return;

        //没有显示诗句区域，不操作
        if (!_TextArea.activeSelf) return;


        _leanSelect.SetActive(false);

        DestroyObj(_TextList);
        DestroyObj(_LineList);

        GameObject linePrefab = null;
        GameObject textPrefab = null;
        Transform textArea = null;

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        //选择排版
        if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
           || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
        {
            textArea = _TextHorizontalArea;
            linePrefab = _HorizontalLinePrefab;
            textPrefab = _HorizontalTextPrefab;
        }
        else
        {
            textArea = _TextVerticalArea;
            linePrefab = _VerticalLinePrefab;
            textPrefab = _VerticalTextPrefab;
        }

        //选择对齐方式
        if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL
            || alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL)
        {
            textArea.GetComponentInChildren<GridLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
            textArea.GetComponentInChildren<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.UpperLeft;
        }
        else
        {
            textArea.GetComponentInChildren<GridLayoutGroup>().childAlignment = TextAnchor.UpperRight;
            textArea.GetComponentInChildren<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.UpperRight;
        }

        float authorLineScale = 1.0f;
        if (sal)
        {
            if (tsscList.Count >= 2)
            {
                int maxSJLen = 0;
                //正文
                for (int i = 0; i < tsscList.Count - 1; i++)
                {
                    InitNewLine(linePrefab, textPrefab, textArea, tsscList[i], Define.SHIJU_FONTSIZE);
                    if(tsscList[i].Length > maxSJLen){
                        maxSJLen = tsscList[i].Length;
                    }
                }
                //作者
                InitNewLine(linePrefab, textPrefab, textArea, tsscList[tsscList.Count - 1], Define.AUTHOR_FONTSIZE);

                float r = 1.0f * Define.SHIJU_FONTSIZE / Define.AUTHOR_FONTSIZE;

                if (1.0f * tsscList[tsscList.Count - 1].Length / maxSJLen > r)
                {
                    //需要重新设置描点
                    RectTransform rt = _LineList[_LineList.Count - 1].GetComponent<RectTransform>();
                    if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL)
                    {
                        rt.pivot = new Vector2(0.0f, 1.0f);
                    }
                    else if(alignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
                    {
                        rt.pivot = new Vector2(1.0f, 1.0f);
                    }
                    else if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL)
                    {
                        //为了自适应作者行的长度
                        rt.pivot = new Vector2(0.0f, 1.0f);
                    }
                    else if (alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
                    {
                        //为了自适应作者行的长度
                        rt.pivot = new Vector2(1.0f, 1.0f);
                    }

                    float s =(r * maxSJLen / tsscList[tsscList.Count - 1].Length);
                    _LineList[_LineList.Count - 1].transform.localScale = new Vector3(s,s,1.0f);
                    authorLineScale = s;

                    //这里把分割线缩放设置回去
                    CJHZ []hzs = _LineList[_LineList.Count - 1].GetComponentsInChildren<CJHZ>();
                    foreach (var hz in hzs)
                    {
                        if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL 
                            || alignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
                        {

                            hz.SP.transform.localScale = new Vector3(1 / s, 1.0f, 1.0f);

                        }
                        else if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL 
                                 || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
                        {

                            hz.SP.transform.localScale = new Vector3(1.0f,1 / s, 1.0f);
                        }
                    }
                }

            }
            else
            {
                //只有一行
                InitNewLine(linePrefab, textPrefab, textArea, tsscList[0], Define.SHIJU_FONTSIZE);
            }
        }
        else
        {
            //正文
            for (int i = 0; i < tsscList.Count; i++)
            {

                InitNewLine(linePrefab, textPrefab, textArea, tsscList[i], Define.SHIJU_FONTSIZE);
            }
        }


        //适配屏幕
        FitScreen(textArea, tsscList, authorLineScale);

        //执行动画
        DoHZAnimation();
    }

    public GameObject _subMenuCtrl;
    public bool CheckCanSwipe(bool select = true)
    {
        // 设置界面显示时，不能切换
        if (_setting.gameObject.activeSelf
            || _likePanel.gameObject.activeSelf
            || _dict.gameObject.activeSelf
            || _article.gameObject.activeSelf
            || _chaos.gameObject.activeSelf
            || _subMenuCtrl.activeSelf
            || _pickColor.gameObject.activeSelf
            || _colorGame.gameObject.activeSelf)
        {
            return false;
        }

        if (select)
        {
            LeanSelectable tls = _TextArea.GetComponent<LeanSelectable>();
            LeanSelectable pls = _picArea.GetComponent<LeanSelectable>();
            if (!pls.IsSelected && !tls.IsSelected)
            {
                return true;
            }
        }

        return true;
    }

    public void ShowSwipeTip(bool sj,Color c){

        if (_mask.gameObject.activeSelf) return;//此时不能显示提示

        if (sj)
        {
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGSJ_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                ShowToast("打开<b>设置</b>，可修改<b>显示速度、行数、排版</b>等",c,3f,0.5f);
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGSJ_TIP, tipCnt + 1);
            }
        }
        else
        {
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGECOLOR_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                ShowToast("如果<b>看不清文字</b>，请点击<b>装饰->文字</b>修改字体颜色", c,3f,0.5f);
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGECOLOR_TIP, tipCnt + 1);
            }
        }
    }

    public void OnSwipeUp()
    {
        if (_ShowingFilter || (_themeCamera.gameObject.activeSelf && !_isInGaming))
        {
            //不能切换
            return;
        }

        if (CheckCanSwipe() && !_isInGaming)
        {
            //ResetUsingLike();
            ShowSwipeTip(false, ChangeColor());
        }
        else
        {
            if(_isInGaming)
            {
                OnSwipe.Invoke(Define.SWIPE_TYPE.UP);
            }
        }
    }
    public void OnSwipeDown()
    {
        if (_ShowingFilter || (_themeCamera.gameObject.activeSelf && !_isInGaming))
        {
            //不能切换
            return;
        }

        if (CheckCanSwipe() && !_isInGaming)
        {
            //ResetUsingLike();
            ShowSwipeTip(false, ChangeColor());

        }
        else
        {
            if (_isInGaming)
            {
                OnSwipe.Invoke(Define.SWIPE_TYPE.DOWN);
            }
        }
    }

    private void DoSwipe(bool toLeft)
    {
        if (_UsingFX)
        {
            //需要判断当前是否显示曲线模式
            //只有当文本区域没有被选中的时候才可以切换
            if (!DOTween.IsTweening("OnSelectDownSHIJU") && !DOTween.IsTweening("OnSelectUpSHIJU"))
            {
                OnSetSJ.Invoke(GetShiJuAndSign(), Setting.GetShowAuthorLine());
            }
            else
            {
                ShowToast("文本区域动画执行中，请结束后再切换诗词。");
            }
            return;
        }
        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        Setting.SpeedLevel speedLevel = Setting.GetSpeedLevel();
        //如果已经全部显示，则对每一行列执行消失动画
        if (_leanSelect.activeSelf)
        {
            float speed = (speedLevel == Setting.SpeedLevel.SPEED_LEVEL_8 ? 0.0f : 2.0f / ((int)speedLevel + 1));

            if (speedLevel == Setting.SpeedLevel.SPEED_LEVEL_8)
            {
                UseAlignment(alignmentType, GetShiJuAndSign());
            }
            else
            {
                _leanSelect.SetActive(false);
                //RectTransform rt = _LineList[0].GetComponent<RectTransform>();
                //float width = rt.rect.width * rt.localScale.x;// 已经缩放过
                float toX = 0.0f;

                if (toLeft)
                {
                    toX = 0;//-width;
                }
                else
                {
                    toX = Screen.width;// + width;
                }

                bool reserve = false;
                if (alignmentType == Setting.AlignmentType.RIGHT_VERTICAL && toLeft)
                {
                    reserve = true;
                }
                else if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL && !toLeft)
                {
                    reserve = true;
                }

                float delay = 0.0f;
                int lineCnt = _LineList.Count;
                for (int i = 0; i < lineCnt; i++)
                {
                    int idx = i;
                    Sequence mySequence = DOTween.Sequence();
                    if (reserve)
                    {
                        idx = lineCnt - 1 - i;
                    }

                    float currentRunTime = 0, prevRunTime = 0;

                    if (i == 0)
                    {
                        prevRunTime = 0.01f;
                        currentRunTime = speed;
                    }
                    else if (i == 1)
                    {
                        prevRunTime = speed;
                        currentRunTime = (1 - Mathf.Log(i, lineCnt)) * speed;
                    }
                    else
                    {
                        prevRunTime = (1 - Mathf.Log(i - 1, lineCnt)) * speed;
                        currentRunTime = (1 - Mathf.Log(i, lineCnt)) * speed;
                    }

                    delay += prevRunTime / 2;

                    CJHZ[] cjhz = _LineList[idx].GetComponentsInChildren<CJHZ>();

                    mySequence
                        .AppendInterval(delay)
                        .Append(_LineList[idx].transform.DOMoveX(toX, currentRunTime));

                    for (int ts = 0; ts < cjhz.Length; ts++)
                    {
                        mySequence
                            .Join(cjhz[ts].SP.DOFade(0.0f, currentRunTime))
                            .Join(cjhz[ts].HZ.DOFade(0.0f, currentRunTime));

                        if (_CurrentHZParam.zsImgPath != "")
                        {
                            mySequence.Join(cjhz[ts].ZhuangShiImg.DOFade(0.0f, currentRunTime));
                        }
                    }

                    mySequence
                        .SetEase(Ease.InSine)
                        .OnComplete(() =>
                        {
                            if (reserve)
                            {
                                if (idx <= 0)
                                {
                                    UseAlignment(alignmentType, GetShiJuAndSign());
                                }
                            }
                            else
                            {
                                if (idx >= _LineList.Count - 1)
                                {
                                    UseAlignment(alignmentType, GetShiJuAndSign());
                                }
                            }
                        });
                }

            }
        }
        else
        {
            UseAlignment(alignmentType, GetShiJuAndSign());
        }
    }

    [Serializable] public class OnSwipeEvent : UnityEvent<Define.SWIPE_TYPE> { }
    public OnSwipeEvent OnSwipe;//切换考题
    public void OnSwipeLeft()
    {
        if (_ShowingFilter)
        {
            //不能切换
            return;
        }

        //正在学习模式
        if (_eduArea.activeSelf)
        {
            OnSwipe.Invoke(Define.SWIPE_TYPE.LEFT);
        }
        else
        {
            if (CheckCanSwipe())
            {
                ResetUsingLike();
                DoSwipe(true);
                ShowSwipeTip(true,_mainCamera.backgroundColor);
            }

        }
    }

    public void OnSwipeRight()
    {
        if (_ShowingFilter)
        {
            //不能切换
            return;
        }

        //正在学习模式
        if (_eduArea.activeSelf)
        {
            OnSwipe.Invoke(Define.SWIPE_TYPE.RIGHT);
        }
        else
        {
            if (CheckCanSwipe())
            {
                ResetUsingLike();
                DoSwipe(false);
                ShowSwipeTip(true,_mainCamera.backgroundColor);
            }

        }
    }

    public Text _inputText;
    public void EditShiJu()
    {

        string shiju = _inputText.text;

        // shiju = "为你写诗\n为你做不可能的事\n某人";

        string[] shiJu = shiju.Split('\n');
        List<string> shiJuList = new List<string>();

        List<string> fmt = new List<string>();
        for (int i = 0; i < shiJu.Length; i++)
        {
            string s = Regex.Replace(shiJu[i], @"\s", "");
            if (s.Length != 0)
            {
                fmt.Add(shiJu[i]);
            }
        }

        if (fmt.Count == 0) return;

        //不限制长度，由输入者自行控制
        int showLine = fmt.Count;
        if (Setting.GetShowSJLine() != Setting.ShowSJLine.LINE_ALL)
        {
            showLine = (int)Setting.GetShowSJLine();
        }

        if (showLine >= fmt.Count)
        {
            showLine = fmt.Count;
        }
        else
        {
            ShowToast("输入行数超过设定的显示行数会被截掉，可到设置界面修改。",3f);
        }

        if(showLine == 0){
            return;
        }

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        string author = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.AUTHOR, "诗色");

        for (int i = 0; i < showLine; i++)
        {
            shiJuList.Add(fmt[i]);
        }

        if (Setting.GetShowAuthorLine())
        {
            string zz = "||" + author;
            if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
               || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
            {
                zz = "——" + author;
            }

            shiJuList.Add(zz);
        }

        ResetUsingLike();// 需要重置不再使用收藏参数
        UseAlignment(alignmentType, shiJuList);


        //如果当前是曲线模式，则提示
        if (string.Join("", shiJuList).Length > Define.MAX_FX_HZ_NUM)
        {
            if (_FXArea.activeSelf)
            {
                ShowToast("显示字数不能超过32个，超出部分自动截断。");
            }
        }
    }

    //重置诗句位置
    public GameObject _TextArea;
    private Vector3 _TextAreaPos;
    private Vector3 _PicAreaPos;
    private Vector3 _ColorInfoPos;
    public void RestAllArea()
    {
        if(CheckCanSwipe(false))
        {
            ResetSJ("SHIJU");
            ResetSJ("COLORINFO");
            ResetSJ("PIC");
        }
    }
    public void ResetSJ(string type)
    {

        Sequence mySequence = DOTween.Sequence();
        GameObject which = null;
        Vector3 pos = _TextAreaPos;

        if (type == "" + eSelectType.SHIJU)
        {
            which = _TextArea;
            pos = _TextAreaPos;
        }
        else if (type == "" + eSelectType.COLORINFO)
        {
            which = _colorInfo;
            pos = _ColorInfoPos;
        }
        else if (type == "" + eSelectType.PIC)
        {
            which = _picArea;
            pos = _PicAreaPos;
        }

        mySequence
            .Append(which.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 1.0f))
            .Join(which.transform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), 1.0f))
            .Join(which.transform.DOMove(pos, 1.0f))
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                // _leanSelect.SetActive(true);
            });
    }

    public void OnWillShowFXEvent()
    {
        _leanSelect.SetActive(false);

        _TextArea.transform.localScale = Vector3.one;
        _TextArea.transform.rotation = new Quaternion(0, 0, 0, 0);
        _TextArea.transform.position = _TextAreaPos;

        LeanSelectable ls = _TextArea.GetComponent<LeanSelectable>();
        ls.Deselect(false,false);
        DOTween.Kill("OnSelectDownSHIJU");
        DOTween.Kill("OnSelectUpSHIJU");

        Button zOrderBtn = _TextArea.transform.Find("Top/SetZOrderBtn").GetComponent<Button>();
        GameObject ws = _TextArea.transform.Find("Selected").gameObject;
        zOrderBtn.gameObject.SetActive(false);
        ws.SetActive(false);

        Image lcImg = _TextArea.GetComponent<Image>();
        lcImg.color = new Color(lcImg.color.r, lcImg.color.g, lcImg.color.b,0f);
    }

    private void DoHZAnimation()
    {

        for (int i = 0; i < _TextList.Count; i++)
        {
            int index = i;

            CJHZ cjhz = _TextList[i].GetComponent<CJHZ>();

            if (Setting.GetSpeedLevel() == Setting.SpeedLevel.SPEED_LEVEL_8)
            {
                if(!_UsingFX) //当前不是显示曲线的时候才能设置
                    _leanSelect.SetActive(true);
                cjhz.DoDirectShow();
            }
            else
            {
                cjhz.DoHZAnimation(Setting.GetUseHZAni(), () =>
                {
                    if (index == _TextList.Count - 1)
                    {
                        if (!_UsingFX) //当前不是显示曲线的时候才能设置
                            _leanSelect.SetActive(true);
                    }
                });
            }
        }
    }

    private void InitNewLine(GameObject linePrefab,
                             GameObject textPrefab,
                             Transform textArea,
                            string shiju,
                            int fontSizeSJ)
    {
        GameObject line = Instantiate(linePrefab, textArea) as GameObject;
        line.SetActive(true);

        _LineList.Add(line);

        GridLayoutGroup ly = line.GetComponentInChildren<GridLayoutGroup>();

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();

        //选择对齐方式
        if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL
            || alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL)
        {
            ly.childAlignment = TextAnchor.UpperLeft;
            ly.startCorner = GridLayoutGroup.Corner.UpperLeft;
        }
        else
        {
            ly.childAlignment = TextAnchor.UpperRight;
            ly.startCorner = GridLayoutGroup.Corner.UpperRight;
        }

        bool bold = Setting.GetFontBold();

        foreach (var hz in shiju)
        {
            GameObject txt = Instantiate(textPrefab, ly.transform) as GameObject;
            txt.SetActive(true);
            CJHZ sj = txt.GetComponent<CJHZ>();

            sj.Init(_TextList.Count, false, false,
                    Setting.GetShowLineSeparator(),
                    Setting.GetAlignmentType(),
                    Setting.GetSpeedLevel(),
                   _CurrentHZParam);
            sj.SetHZText("" + hz, fontSizeSJ, bold);

            _TextList.Add(txt);

            // 对于标点符号需要旋转// 暂时忽略
            /*
             hz == '“'
             || hz == '”'
            || hz == '、'
            || hz == '，'
            || hz == '。'
            || hz == '；'
            || hz == '！'
            || hz == '？'
            || hz == '：'
            || hz == '《'
            || hz == '》'
            */
            if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL
                || alignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
            {
                if (hz == '《' || hz == '“' || hz == '：'
                  || hz == '》' || hz == '”')
                {
                    //
                }
            }
            else
            {
                if (alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL
                    && (hz == '？' || hz == '，' || hz == '《' || hz == '》'))
                {
                    sj.transform.localRotation = new Quaternion(0, 180, 0, 0);
                }
            }
        }
    }

    private void FitScreen(Transform textArea, List<string> tsscList,float authorLineScale)
    {
        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();

        //单行大小
        Vector2 cellLine = textArea.GetComponentInChildren<GridLayoutGroup>().cellSize;
        Vector2 cellLineSpacing = textArea.GetComponentInChildren<GridLayoutGroup>().spacing;
        //设置锚点
        RectTransform rt = textArea.GetComponentInChildren<RectTransform>();
        switch (alignmentType)
        {
            case Setting.AlignmentType.LEFT_HORIZONTAL:
                rt.pivot = new Vector2(0.0f, 1.0f);
                break;
            case Setting.AlignmentType.LEFT_VERTICAL:
                rt.pivot = new Vector2(0.0f, 1.0f);
                break;
            case Setting.AlignmentType.RIGHT_HORIZONTAL:
                rt.pivot = new Vector2(1.0f, 1.0f);
                break;
            case Setting.AlignmentType.RIGHT_VERTICAL:
                rt.pivot = new Vector2(1.0f, 1.0f);
                break;
        }

        int maxLen = 0;
        for (int i = 0; i < tsscList.Count; i++)
        {
            if (maxLen < tsscList[i].Length)
            {
                maxLen = tsscList[i].Length;
            }
        }

        float maxX = 0;
        float maxY = 0;

        if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL
            || alignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
        {
            maxY = maxLen * cellLine.x * authorLineScale;//h
            maxX = tsscList.Count * cellLine.x + (tsscList.Count - 1) * cellLineSpacing.x;//w
        }
        else
        {
            maxX = maxLen * cellLine.y * authorLineScale;//h
            maxY = tsscList.Count * cellLine.y + (tsscList.Count - 1) * cellLineSpacing.y;//w
        }

        float x = rt.rect.width / maxX;
        float y = rt.rect.height / maxY;

        float s = 1.0f;
        if (x < 1.0f && y < 1.0f)
        {
            s = x < y ? x : y;
        }
        else if (x >= 1.0f && y < 1.0f)
        {
            s = y;
        }
        else if (x < 1.0f && y >= 1.0f)
        {
            s = x;
        }

        textArea.localScale = new Vector3(s, s, 1);
    }


    //获取诗句和签名
    private List<string> GetShiJuAndSign()
    {
        List<string> sj = new List<string>();

        //获取格式化后的诗词语句列表
        List<string> tsscData = HZManager.GetInstance().GetTSSC(HZManager.eShiCi.ALL);

        //测试
        //List<string> tsscData = HZManager.GetInstance().GetTSSC(HZManager.eShiCi.TANGSHI,290);

        List<string> tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[4]);

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        if (tsscList.Count == 0) return sj;
        //需要处理随机连续2、4的情况
        switch (Setting.GetShowSJLine())
        {
            case Setting.ShowSJLine.LINE_1:
                sj.Add(tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)]);
                break;
            case Setting.ShowSJLine.LINE_2:
                if (tsscList.Count >= 2)
                {
                    int startIndex = HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count - 2);
                    for (int i = startIndex; i < startIndex + 2; i++)
                    {
                        sj.Add(tsscList[i]);
                    }
                }
                else
                {
                    sj.AddRange(tsscList);
                }
                break;
            case Setting.ShowSJLine.LINE_4:
                if (tsscList.Count >= 4)
                {
                    int startIndex = HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count - 4);
                    for (int i = startIndex; i < startIndex + 4; i++)
                    {
                        sj.Add(tsscList[i]);
                    }
                }
                else
                {
                    sj.AddRange(tsscList);
                }
                break;
            case Setting.ShowSJLine.LINE_ALL:
                sj.AddRange(tsscList);
                break;
        }

        //  需要显示署名
        if (Setting.GetShowAuthorLine())
        {

            //作者
            string zz = tsscData[1] + "||" + tsscData[2] + "·" + tsscData[3];
            if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
               || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
            {
                zz = tsscData[1] + "——" + tsscData[2] + "·" + tsscData[3];
            }

            sj.Add(zz);
        }

        return sj;
    }

    //顶部菜单动画
    public GameObject _TopMenu;
    private Vector3 _TopMenuPos;
    public GameObject _ExpandBtn;
    public GameObject _MainMenuList;
    public Image _mask;
    public GameObject _ImageEffectMenu;
    public Image[] _MoreBtnImgs;
    private void DoTopMenuMoreBtnAni(Color bgColor)
    {
        DOTween.Kill("DoTopMenuMoreBtnAni");

        Color bc = Define.GetFixColor(bgColor);
        Color BgLightColor = bc;//Define.GetFixColor(bgColor * 1.1f);
        Color BgDarkColor = Define.GetFixColor(bc * 0.9f);

        Sequence saAni = DOTween.Sequence();
        saAni.SetId("DoTopMenuMoreBtnAni");

        foreach (var zd in _MoreBtnImgs)
        {
            zd.color = BgDarkColor;
        }

        saAni
            .Append(_MoreBtnImgs[0].DOColor(BgLightColor, 0.4f))
            .Append(_MoreBtnImgs[1].DOColor(BgLightColor, 0.4f))
            .Append(_MoreBtnImgs[2].DOColor(BgLightColor, 0.4f))

            .Append(_MoreBtnImgs[0].DOColor(BgDarkColor, 0.4f))
            .Append(_MoreBtnImgs[1].DOColor(BgDarkColor, 0.4f))
            .Append(_MoreBtnImgs[2].DOColor(BgDarkColor, 0.4f))
            .SetLoops(-1, LoopType.Restart);
    }

    //展开顶部菜单动画
    public void ExpandTopMenu()
    {
        Text expandBtnText = _ExpandBtn.GetComponentInChildren<Text>();
        bool expand = false;
        string expandTxt = "收起";

        //
        if(expandBtnText.text.Equals("返回"))
        {
            OnOpenEdu(false);
            DoTopMenuMoreBtnAni(_mainCamera.backgroundColor);
        }
        else{
            if (expandBtnText.text.Equals("收起"))
            {
                expand = false;
                expandTxt = "展开";
            }
            else
            {
                expand = true;
                expandTxt = "收起";//通过按钮触发只能是收起
            }

            Image topMenuListContentBg = _MainMenuList.transform.Find("Bg").GetComponent<Image>();
            GameObject topMenuListContent = _MainMenuList.transform.Find("MenuList/Viewport/Content").gameObject;

            ExpandTopMenu(expand, expandTxt, topMenuListContentBg, topMenuListContent);
        }
    }

    private void ExpandTopMenu(bool expand,string expandTxt, Image bg,GameObject content, Action cb = null)
    {
        Image[] expandBtnImg = _ExpandBtn.GetComponentsInChildren<Image>();
        Text expandBtnText = _ExpandBtn.GetComponentInChildren<Text>();
        Sequence mySequence = DOTween.Sequence();

        float toX = 0.0f;

        if (!expand)
        {
            toX = content.transform.localPosition.x - bg.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            toX = content.transform.localPosition.x + bg.GetComponent<RectTransform>().rect.width;
        }

        _mask.gameObject.SetActive(true);

        Text[] BtnTexts = content.GetComponentsInChildren<Text>();
        Image[] BtnImgs = content.GetComponentsInChildren<Image>();

        Color fc = Define.GetFixColor(_mainCamera.backgroundColor);

        Image BGForIPX = _TopMenu.transform.Find("BGForIPX").GetComponent<Image>();

        if (expand)
        {
            mySequence
                .Append(expandBtnImg[0].DOFade(50 / 255.0f, 0.2f))
                .Join(expandBtnImg[1].DOColor(new Color(fc.r, fc.g, fc.b, 1.0f), 0.2f))
                .Join(expandBtnText.DOFade(0.5f, 0.2f))
                .SetEase(Ease.OutSine)
                .Append(expandBtnText.DOText(expandTxt, 0.4f))
                .Join(expandBtnImg[1].transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.4f))
                .Join(expandBtnImg[1].DOFade(1.0f, 0.4f))
                .Join(content.transform.DOLocalMoveX(toX, 0.4f))
                .Join(bg.DOFade(50 / 255.0f, 0.4f))
                .Join(BGForIPX.DOFade(50 / 255.0f, 0.4f));

            for (int i = 0; i < BtnTexts.Length; i++)
            {
                if (BtnTexts[i].name == "BtnText")
                {
                    mySequence.Join(BtnTexts[i].DOFade(0.5f, 0.4f));
                }
            }
            for (int i = 0; i < BtnImgs.Length; i++)
            {
                if (BtnImgs[i].name == "BtnImg")
                {
                    mySequence.Join(BtnImgs[i].DOFade(1.0f, 0.4f));
                }
            }

            mySequence
                //子菜单元素动画
                .SetEase(Ease.OutSine)
                .OnComplete(() => { 
                    _mask.gameObject.SetActive(false);
                    cb?.Invoke();
                });
        }
        else
        {
            mySequence
                .Append(expandBtnText.DOText(expandTxt, 0.4f))
                .Join(expandBtnImg[1].transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 180.0f), 0.4f))
                .Join(content.transform.DOLocalMoveX(toX, 0.5f))
                .Join(bg.DOFade(0.0f, 0.5f))
                .Join(BGForIPX.DOFade(0f, 0.5f));

            for (int i = 0; i < BtnTexts.Length; i++)
            {
                if (BtnTexts[i].name == "BtnText")
                {
                    mySequence.Join(BtnTexts[i].DOFade(0.0f, 0.5f));
                }
            }
            for (int i = 0; i < BtnImgs.Length; i++)
            {
                if (BtnImgs[i].name == "BtnImg")
                {
                    mySequence.Join(BtnImgs[i].DOFade(0.0f, 0.5f));
                }
            }

            mySequence
                .Join(expandBtnImg[0].DOFade(0.0f, 0.7f))
                .Join(expandBtnImg[1].DOColor(fc * 0.9f, 0.4f))
                .Join(expandBtnText.DOFade(50 / 255.0f, 0.4f))
                .SetEase(Ease.InSine)
                .OnComplete(() => {
                    _mask.gameObject.SetActive(false);
                    cb?.Invoke();
                });
        }
    }

    //打开教育练习界面
    public GameObject _eduArea;
    public GameObject _EduMenuList;
    private bool _hasInitEduMenuListPos = false;
    private bool _IsPicAreaShowing;
    public GameObject _GameCenter;
    public void OnOpenEdu(bool open)
    {
        Image eduMenuListContentBg = _EduMenuList.transform.Find("Bg").GetComponent<Image>();
        GameObject eduMenuListContent = _EduMenuList.transform.Find("MenuList/Viewport/Content").gameObject;

        Image topMenuListContentBg = _MainMenuList.transform.Find("Bg").GetComponent<Image>();
        GameObject topMenuListContent = _MainMenuList.transform.Find("MenuList/Viewport/Content").gameObject;

        if (open)
        {
            //登陆gamecenter，不重复设置
            if (!_GameCenter.activeSelf)
            {
                _GameCenter.SetActive(true);
            }
            _colorInfo.SetActive(false);
            _TextArea.SetActive(false);
            _IsPicAreaShowing = _picArea.activeSelf;
            _picArea.SetActive(false);//
            _eduArea.SetActive(true);
            OnShowPic.Invoke(false);

            //_leanTouch.SetActive(!open);

            ExpandTopMenu(false,"展开", topMenuListContentBg, topMenuListContent, () => {
                _MainMenuList.SetActive(false);
                _EduMenuList.SetActive(true);
                if (!_hasInitEduMenuListPos)
                {
                    eduMenuListContentBg.color = new Color(eduMenuListContentBg.color.r,
                                                           eduMenuListContentBg.color.g,
                                                           eduMenuListContentBg.color.b,
                                                          0.0f);
                    _hasInitEduMenuListPos = true;
                    //第一次位置需要设置为移动前的值，这么做主要是为了ui界面布局可见，不用拖到屏幕以外
                    eduMenuListContent.transform.localPosition = new Vector3(eduMenuListContent.transform.localPosition.x - eduMenuListContentBg.GetComponent<RectTransform>().rect.width,
                                                                              eduMenuListContent.transform.localPosition.y,
                                                                             eduMenuListContent.transform.localPosition.z);
                }

                ExpandTopMenu(true,"返回", eduMenuListContentBg, eduMenuListContent);

            });

        }
        else
        {
            if (_isInGaming)
            {
                //
                Study st = _eduArea.GetComponent<Study>();
                st.OnClickBack();
            }
            else
            {
                RepeatRotateColorInfo(true);
                //_leanTouch.SetActive(!open);
                _colorInfo.SetActive(true);
                _TextArea.SetActive(true);
                _picArea.SetActive(_IsPicAreaShowing);//
                OnShowPic.Invoke(_IsPicAreaShowing);
                _eduArea.SetActive(false);

                ExpandTopMenu(false, "展开", eduMenuListContentBg, eduMenuListContent, () =>
                {
                    _EduMenuList.SetActive(false);
                    _MainMenuList.SetActive(true);

                    ExpandTopMenu(true, "收起", topMenuListContentBg, topMenuListContent);

                });
            }
        }
    }

    public void OnSaveCaiJian()
    {
        //需要首先隐藏ui
        //to do
        Sequence mySequence = DOTween.Sequence();

        mySequence
            .Append(_TopMenu.transform.DOLocalMoveY(_TopMenuPos.y + 200, 0.4f))
            .SetEase(Ease.InSine)
            .AppendInterval(0.1f)
            .OnComplete(() =>
            {
                StartCoroutine(CaptureScreen());
            });
    }


    private IEnumerator CaptureScreen()
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        // do something with texture

        var data = texture.EncodeToJPG();
        string fn = "CJ.jpg";
        StreamManager.SaveFile(fn, data, FolderLocations.Pictures, ((bool succeeded) =>
        {
            if (succeeded)
            {
                Tween topMenu = _TopMenu.transform.DOLocalMoveY(_TopMenuPos.y,
                                                                0.4f);

                _mask.GetComponent<MaskTips>().ShowSaveDone(_mainCamera.backgroundColor, topMenu,()=>{
                    SocialManager.Share(data, "GSSJ", "#诗色#", "诗色", "#诗色#", SocialShareDataTypes.Image_PNG);
                });
            };
        })
        );

        // cleanup
        Destroy(texture);
    }

    private bool GetIfUseOriAllPic()
    {
        GameObject oriBtn = _picArea.transform.Find("OK&Cancel/OriBtn").gameObject;
        Text bntTxt = oriBtn.GetComponentInChildren<Text>();
        if (bntTxt.text == "局部")//显示为局部到时候，图片是全图显示的
        {
            return true;
        }

        return false;
    }
    public void OnOriAllPicBtnClick()
    {
        //不可以调节显示范围
        //非全图的时候，可以滑动调节显示范围
        //<b>单指触摸</b>，滑动调整图片显示范围
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();

        GameObject oriBtn = _picArea.transform.Find("OK&Cancel/OriBtn").gameObject;
        Text bntTxt = oriBtn.GetComponentInChildren<Text>();

        float x = picAreaRt.rect.width / _PeiTuSprite.sprite.texture.width;
        float y = picAreaRt.rect.height / _PeiTuSprite.sprite.texture.height;

        //设置相机大小
        float s = 1.0f;
        if (x < 1.0f && y < 1.0f)
        {
            s = x < y ? x : y;
        }
        else if (x >= 1.0f && y < 1.0f)
        {
            s = y;
        }
        else if (x < 1.0f && y >= 1.0f)
        {
            s = x;
        }
        //需要放大到图片区域，不能使用原图，否则太小
        else if (x >= 1.0f && y >= 1.0f)
        {
            s = Math.Min(x, y);
        }

        if (bntTxt.text == "局部")
        {
            bntTxt.text = "全图";
            if (_PeiTuSprite.sprite.texture.width > _PeiTuSprite.sprite.texture.height)
            {
                s = picAreaRt.rect.height / (_PeiTuSprite.sprite.texture.height * s);
            }
            else
            {
                s = picAreaRt.rect.width / (_PeiTuSprite.sprite.texture.width * s);
            }

            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_PIC_POS_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                ShowToast("<b>双指触摸照片</b>，移动调整照片显示区域", 3.0f);
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_PIC_POS_TIP, tipCnt + 1);
            }
        }
        else
        {
            bntTxt.text = "局部";
            s = 1f;
        }

        _SelcetPic.transform.localPosition = Vector3.zero;
        _SelcetPic.transform.localScale = new Vector3(s, s, 1);

    }

    public void TestSprite(string path)
    {
        _OpenFilterImage = false;
        Sprite sp  = Resources.Load(path, typeof(Sprite)) as Sprite;
        ShowProcessPic(sp);
    }

    [Serializable] public class OnSetPicEvent : UnityEvent<Sprite,float> { }
    public OnSetPicEvent OnSetPic;
    private void ShowProcessPic(Sprite sp)
    {
        _ShowingFilter = true;
        _PeiTuSprite.sprite = sp;
        //pic大小
        //_SelcetPic.SetNativeSize();

        //RectTransform picRt = _SelcetPic.GetComponent<RectTransform>();
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();

        float x = picAreaRt.rect.width / _PeiTuSprite.sprite.texture.width;
        float y = picAreaRt.rect.height / _PeiTuSprite.sprite.texture.height;

        //设置相机大小
        float s = 1.0f;
        if (x < 1.0f && y < 1.0f)
        {
            s = x < y ? x : y;
        }
        else if (x >= 1.0f && y < 1.0f)
        {
            s = y;
        }
        else if (x < 1.0f && y >= 1.0f)
        {
            s = x;
        }
        //需要放大到图片区域，不能使用原图，否则太小
        else if (x >= 1.0f && y >= 1.0f)
        {
            s = Math.Min(x, y);
        }

        OnSetPic.Invoke(sp,s * (1024 / picAreaRt.rect.width));

        if (GetIfUseOriAllPic())
        {
            s = 1f;
        }
        else
        {
            if(_PeiTuSprite.sprite.texture.width > _PeiTuSprite.sprite.texture.height)
            {
                s = picAreaRt.rect.height / (_PeiTuSprite.sprite.texture.height * s);
            }
            else 
            {
                s = picAreaRt.rect.width / (_PeiTuSprite.sprite.texture.width * s);
            }
        }

        _pic.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        _SelcetPic.transform.localPosition = Vector3.zero;
        _SelcetPic.transform.localScale = new Vector3(s, s, 1);

        //只显示原图，首先调节范围
        if (!_OpenFilterImage)
        {
            //需要取消picarea的动画
            if (DOTween.IsTweening("OnSelectDown" + eSelectType.PIC)
                || DOTween.IsTweening("OnSelectUp" + eSelectType.PIC))
            {
                DOTween.Kill("OnSelectDown" + eSelectType.PIC);
                DOTween.Kill("OnSelectUp" + eSelectType.PIC);
            }

            _picArea.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            _picArea.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            _picArea.transform.position = _PicAreaPos;
            _picArea.GetComponent<Image>().color = new Color(0,0,0,0);
            _picArea.GetComponent<LeanSelectable>().enabled = false;

            _TopMenu.SetActive(false);
            _ImageEffectMenu.SetActive(true);

            _ImageEffectMenu.transform.localPosition = new Vector3(0, _TopMenuPos.y + 200, 0);
            Sequence mySequence = DOTween.Sequence();
            mySequence
                .Append(_ImageEffectMenu.transform.DOLocalMoveY(_TopMenuPos.y, 1.0f))
                .SetEase(Ease.OutBounce);

            _AdjustFilter.gameObject.SetActive(true);
            _SelcetPic.gameObject.SetActive(true);
            _picArea.SetActive(true);
            _pic.gameObject.SetActive(false);
            //_colorInfo.SetActive(false);  //显示底部
            _TextArea.SetActive(false);
            _TopMenu.SetActive(false);
            GameObject OC = _picArea.transform.Find("OK&Cancel").gameObject;
            OC.SetActive(true);

            _picArea.transform.Find("Top").gameObject.SetActive(false);
            GameObject sl = _picArea.transform.Find("Selected").gameObject;
            sl.SetActive(true);
            
        }

        HideMaskAfterSelect();
    }

    private  void HideMaskAfterSelect()
    {
        _OpenFilterImage = false;

        //只有>1024的图才会显示遮罩
        if (_mask.gameObject.activeSelf)
        {
            float r = Mathf.Sqrt(_PeiTuSprite.sprite.texture.width * _PeiTuSprite.sprite.texture.height) / 1024f;
            _mask.color = new Color(0f, 0f, 0f, 0.5f);
            Sequence loadingHide = DOTween.Sequence();
            loadingHide.SetId("HideMaskAfterSelect");
            loadingHide
                .AppendInterval(0.2f * Mathf.Sqrt(r))
                .Append(_mask.DOFade(0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _mask.gameObject.SetActive(false);
                });
        }
    }

    public void HideMaskAfterSelectByClick()
    {
        if (DOTween.IsTweening("HideMaskAfterSelect")) return;
        if (!_OnSelectingPic) return;
        HideMaskAfterSelect();
        _OnSelectingPic = false;
    }

    //根据当前是全图还是局部保存照片
    public void OnSaveFilterImageBtnClick()
    {
        StartCoroutine(SaveFilterImage());
    }

    private bool _OpenFilterImage = false;
    public void OnOpenFilterImageBtnClick()
    {
        _OpenFilterImage = true;
        OnOpenPic(true);
    }

    private IEnumerator SaveFilterImage()
    {
        yield return new WaitForEndOfFrame();

        RectTransform picRt = _SelcetPic.GetComponent<RectTransform>();
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        int wx = (int)(picAreaRt.rect.width * sh);
        int hy = (int)(picAreaRt.rect.height * sh);

        float start_x = Screen.width / 2 - wx / 2;
        float start_y = Screen.height / 2 - hy / 2;

        float x = picAreaRt.rect.width / _PeiTuSprite.sprite.texture.width;
        float y = picAreaRt.rect.height / _PeiTuSprite.sprite.texture.height;

        //设置相机大小
        float s = 1.0f;
        if (x < 1.0f && y < 1.0f)
        {
            s = x < y ? x : y;
        }
        else if (x >= 1.0f && y < 1.0f)
        {
            s = y;
        }
        else if (x < 1.0f && y >= 1.0f)
        {
            s = x;
        }
        //需要放大到图片区域，不能使用原图，否则太小
        else if (x >= 1.0f && y >= 1.0f)
        {
            s = Math.Min(x, y);
        }

        if (GetIfUseOriAllPic())
        {
            if (_PeiTuSprite.sprite.texture.width > _PeiTuSprite.sprite.texture.height)
            {
                start_y += (picAreaRt.rect.height / 2 - _PeiTuSprite.sprite.texture.height * s / 2) * sh;
                hy = (int)(_PeiTuSprite.sprite.texture.height * s * sh);
            }
            else
            {
                start_x += (picAreaRt.rect.width / 2 - _PeiTuSprite.sprite.texture.width * s / 2) * sh;
                wx = (int)(_PeiTuSprite.sprite.texture.width * s * sh);
            }
        }
        else
        {
            //
        }


        Texture2D screenShot = new Texture2D(wx, hy);
        screenShot.ReadPixels(new Rect(start_x, start_y, wx, hy), 0, 0);
        screenShot.Apply();

        /*
        //过滤掉背景色，可能不精准
        Color [] cs =  screenShot.GetPixels();
        for (int i = 0;i < cs.Length;i++)
        {
            Color ac = cs[i];
            if (cs[i] == _mainCamera.backgroundColor)
            {
                ac = new Color(ac.r, ac.g, ac.b, 0f);
            }
            screenShot.SetPixel(i % wx, i / wx, ac);
        }
        */

        var data = screenShot.EncodeToJPG();

        Destroy(screenShot);
        screenShot = null;

        StreamManager.SaveFile("Filter.jpg", data, FolderLocations.Pictures, ((bool succeeded) =>
        {
            if (succeeded)
            {
                SocialManager.Share(data, "GSSJ", "#诗色#", "诗色", "#诗色#", SocialShareDataTypes.Image_JPG);
            }
        })
        );
    }

    public RawImage _pic;
    public RawImage _SelcetPic;
    public GameObject _AdjustFilter;
    public SpriteRenderer _PeiTuSprite;
    [Serializable] public class OnShowPicEvent : UnityEvent<bool> { }
    public OnShowPicEvent OnShowPic;
    private Texture2D _tmpLoadImageTexture = null;
    private Sprite _tmpLoadImageSprite = null;

    public void imgSelectCallback(Stream stream, bool succeeded)
    {
        _OnSelectingPic = false;//只有有回调，就不执行回到前台的操作
        if (!succeeded)
        {
            if (stream != null) stream.Dispose();

            //不在滤镜界面的选图才可以使用1
            if (!_OpenFilterImage)
            {
                TestSprite("icon/1");
            }
            else
            {
                HideMaskAfterSelect();
            }

            return;
        }

        try
        {
            if (_tmpLoadImageTexture != null)
            {
                DestroyImmediate(_tmpLoadImageTexture);
                DestroyImmediate(_tmpLoadImageSprite);

                _tmpLoadImageTexture = null;
                _tmpLoadImageSprite = null;
                GC.Collect();
            }

            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            _tmpLoadImageTexture = new Texture2D(4, 4);
            _tmpLoadImageTexture.LoadImage(data);
            _tmpLoadImageTexture.Apply();

            _tmpLoadImageSprite = Sprite.Create(_tmpLoadImageTexture,
                                        new Rect(0, 0, _tmpLoadImageTexture.width, _tmpLoadImageTexture.height),
                                        new Vector2(.5f, .5f));

            ShowProcessPic(_tmpLoadImageSprite);
        }
        catch (Exception e)
        {
            if (!_OpenFilterImage)
            {
                TestSprite("icon/1");
            }
            else
            {
                HideMaskAfterSelect();
            }
        }
        finally
        {
            // NOTE: Make sure you dispose of this stream !!!
            if (stream != null) stream.Dispose();
        }
    }

    public void OnOpenPic(bool album)
    {
        _mask.gameObject.SetActive(true);
        _mask.color = new Color(0f,0f,0f,0.0f);
        _mask.DOFade(0.5f,0.5f);

        _OnSelectingPic = true;
        if (album)
        {
            StreamManager.LoadFileDialog(FolderLocations.Pictures,new string[] { ".png", ".jpg", ".jpeg" }, imgSelectCallback,true);
        }
        else
        {
            StreamManager.LoadCameraPicker(CameraQuality.Med, imgSelectCallback,true);
        }
    }

    private bool _OnSelectingPic = false;
    private void OnApplicationFocus(bool focus)
    {
        if (focus)  //当程序进入前台时
        {
            if (_OnSelectingPic)
            {
                HideMaskAfterSelect();
                _OnSelectingPic = false;
            }
        }
        else        //当程序进入到后台时
        {
        }
    }
#region 中间区域交互
    public void OnLongPressForDeSelect()
    {
        if (!CheckCanSwipe()) return;

        OnChangeZOrder(true);
    }
    //交换诗句和图片的层级
    public void OnChangeZOrder()
    {

        OnChangeZOrder(false);
    }

    private void switchZOrder()
    {
        int pic = _picArea.transform.GetSiblingIndex();
        int shiju = _TextArea.transform.GetSiblingIndex();

        int tmp = pic;
        pic = shiju;
        shiju = tmp;

        if (pic < shiju)
        {
            _picArea.transform.Find("Top/SetZOrderBtn").GetComponentInChildren<Text>().text = "置顶";
            _TextArea.transform.Find("Top/SetZOrderBtn").GetComponentInChildren<Text>().text = "置底";
        }
        else
        {
            _picArea.transform.Find("Top/SetZOrderBtn").GetComponentInChildren<Text>().text = "置底";
            _TextArea.transform.Find("Top/SetZOrderBtn").GetComponentInChildren<Text>().text = "置顶";
        }

        _picArea.transform.SetSiblingIndex(pic);
        _TextArea.transform.SetSiblingIndex(shiju);
    }

    public void OnChangeZOrder(bool longPress)
    {
        if (_ShowingFilter)
        {
            //不能切换
            return;
        }

        if (!_picArea.activeSelf || !_TextArea.activeSelf)
        {
            //图片区域不可见的时候无需执行切换操作
            return;
        }

        if (longPress)
        {
            LeanSelectable pls = _picArea.GetComponent<LeanSelectable>();
            LeanSelectable tls = _TextArea.GetComponent<LeanSelectable>();

            if (pls.IsSelected || tls.IsSelected)
            {
                return;
            }
        }

        //交换zorder
        switchZOrder();

        if (longPress) return;

        int pic = _picArea.transform.GetSiblingIndex();
        int shiju = _TextArea.transform.GetSiblingIndex();

        if (pic < shiju)
        {
            //取消选中
            LeanSelectable ls = _picArea.GetComponent<LeanSelectable>();
            ls.Deselect();

        }
        else
        {
            //取消选中
            LeanSelectable ls = _TextArea.GetComponent<LeanSelectable>();
            ls.Deselect();
        }
    }

    //手指离开屏幕即认为停止调整，此时重置调整框
    public void OnStopAdjustFilter()
    {
        _AdjustFilter.transform.localPosition = Vector3.zero;
    }

    public UnityEvent _ResetFilterEvent;
    public void OnResetFilter()
    {
        //执行动画，动画期间，不能操作
        if (DOTween.IsTweening("OnResetFilter")) return;

        Transform pic = _picArea.transform.Find("PicSelect");
        Transform st = _picArea.transform.Find("Selected");
        Sequence picAreaAni = DOTween.Sequence();
        picAreaAni.SetId("OnResetFilter");
        picAreaAni.Append(pic.DOScale(0.8f, 0.5f))
            .Join(st.DOScale(0.8f, 0.5f))
            .Append(pic.DOScale(1.0f, 0.5f))
            .Join(st.DOScale(1.0f, 0.5f))
            .SetEase(Ease.OutBounce);

        _ResetFilterEvent.Invoke();
    } 
    public void OnAdjustPic(Vector2 prevPos, Vector2 currentPos, PicAdjust.PicAdjustType type)
    {
        if (type == PicAdjust.PicAdjustType.ADJUST_PIC_POS)
        {
            OnAjdustPicPos(prevPos, currentPos);
        }
        else if (type == PicAdjust.PicAdjustType.ADJUST_FILTER)
        {
            OnAdjustPicFilter(prevPos, currentPos);
        }
        else if (type == PicAdjust.PicAdjustType.ADJUST_PIC_ALPHA)
        {
            OnAdjustPicAlpha(prevPos, currentPos);
        }
    }

    [Serializable] public class OnAdjustFilterEvent : UnityEvent<FilterAdjust.AdjustDirectionType,float> { }
    public OnAdjustFilterEvent OnAdjustFilter;
    private void OnAdjustPicFilter(Vector2 prevPos, Vector2 currentPos)
    {
        //限制在1024范围内，否则不予处理
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;

        float newX = currentPos.x;
        float newY = currentPos.y;

        if (Math.Abs(newX) > picAreaRt.rect.width * sh)
        {
            //不能再移动
            _AdjustFilter.transform.localPosition = prevPos;
            return;
        }
        
        if (Math.Abs(newY) > picAreaRt.rect.height * sh)
        {
            //不能再移动
            _AdjustFilter.transform.localPosition = prevPos;
            return;
        }

        _AdjustFilter.transform.localPosition = new Vector3(newX, newY, 0f);

        //只能同时移动一个方向，不能同时调整，不然会很乱
        float vaule = 0.0f;//是本次调节的变化值
        FilterAdjust.AdjustDirectionType dir = FilterAdjust.AdjustDirectionType.NONE;

        double a = Math.Atan2(currentPos.y - prevPos.y,currentPos.x - prevPos.x);
        if (a >= -Math.PI / 8 && a < Math.PI / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.RIGHT;
            vaule = (currentPos.x - prevPos.x) / (picAreaRt.rect.width * sh);
        }
        else if (a >= Math.PI / 8 && a < Math.PI * 3/ 8)
        {
            dir = FilterAdjust.AdjustDirectionType.UP_RIGHT;

            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x,2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }
        else if (a > Math.PI*3 / 8 && a < Math.PI * 5 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.UP;
            vaule = (currentPos.y - prevPos.y) / (picAreaRt.rect.height * sh);
        }
        else if (a >= Math.PI *5/ 8 && a < Math.PI*7 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.LEFT_UP;
            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }
        else if ((a >= Math.PI*7 / 8 && a <= Math.PI )|| (a > -Math.PI && a < -Math.PI * 7 /8))
        {
            dir = FilterAdjust.AdjustDirectionType.LEFT;
            vaule = (currentPos.x - prevPos.x) / (picAreaRt.rect.width * sh);
        }
        else if (a >= -Math.PI *7/ 8 && a < -Math.PI *5/ 8)
        {
            dir = FilterAdjust.AdjustDirectionType.DOWN_LEFT;
            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }
        else if (a >= -Math.PI *5/ 8 && a < -Math.PI *3/ 8)
        {
            dir = FilterAdjust.AdjustDirectionType.DOWN;
            vaule = (currentPos.y - prevPos.y) / (picAreaRt.rect.height * sh);
        }
        else if (a >= -Math.PI *3/ 8 && a < -Math.PI / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.DOWN_RIGHT;
            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }

        vaule = Mathf.Abs(vaule);
        if (vaule >= float.Epsilon)
        {
            OnAdjustFilter.Invoke(dir, vaule);
        }
    }


    private void OnAjdustPicPos(Vector2 prevPos, Vector2 currentPos)
    {
        if (GetIfUseOriAllPic())
        {
            _SelcetPic.transform.localPosition = prevPos;
            return;
        }

        float s = _SelcetPic.transform.localScale.x;
        //需要更新pic的texture范围，从selectpic中获取
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();

        float dw = picAreaRt.rect.width * (1 + s) / 2;//(picRt.rect.width * s  - picAreaRt.rect.width )/2;
        float dh = picAreaRt.rect.height * (1 + s) / 2;//(picRt.rect.height * s  - picAreaRt.rect.height)/2;

        if (_PeiTuSprite.sprite.texture.width > _PeiTuSprite.sprite.texture.height)
        {
            dh = picAreaRt.rect.height;
        }
        else
        {
            dw = picAreaRt.rect.width;
        }

        float newX = currentPos.x;
        float newY = currentPos.y;

        if (Math.Abs(newX) > dw)
        {
            //不能再移动
            _SelcetPic.transform.localPosition = prevPos;
            return;
        }

        if (Math.Abs(newY) > dh)
        {
            //不能再移动
            _SelcetPic.transform.localPosition = prevPos;
            return;
        }

        _SelcetPic.transform.localPosition = new Vector3(newX, newY, 0f);
    }
    private bool _ShowingFilter = false;
    public void OnAdjustOKBtnClick()
    {
        StartCoroutine(SetPicPixels());
    }

    private IEnumerator SetPicPixels()
    {
        yield return new WaitForEndOfFrame();

        //设置图片区域显示
        //需要更新pic的texture范围，从selectpic中获取
        RectTransform picRt = _SelcetPic.GetComponent<RectTransform>();
        RectTransform picAreaRt = _picArea.GetComponent<RectTransform>();

        // 读取屏幕像素信息并存储为纹理数据，
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        int wx = (int)(picAreaRt.rect.width * sh);

        float start_x = Screen.width / 2 - wx / 2;
        float start_y = Screen.height / 2 - wx / 2 ;


        Texture2D screenShot = new Texture2D(wx, wx);
        screenShot.ReadPixels(new Rect(start_x, start_y, wx, wx), 0, 0);
        screenShot.Apply();

        SetPicTexture(screenShot,true);
    }
    public UnityEvent _OnUsePickColorPic;
    public void OnUsePickColorPic(Texture2D t, List<string> colorInfo, string des)
    {
        //当前正在辨色游戏界面
        if (_colorGame.gameObject.activeSelf)
        {
            OpenPanel(false, _pickColor.gameObject,()=> {
                //通知辨色游戏关闭界面
                _OnUsePickColorPic.Invoke();
            });
        }
        else
        {
            OpenPanel(false, _pickColor.gameObject);//关闭取色界面
        }

        SetPicTexture(t,false);
        UsePickColor(colorInfo, des, !_colorGame.gameObject.activeSelf);
    }
    private void SetPicTexture(Texture2D t,bool needSelect)
    {
        if (_pic.texture != null)
        {
            Destroy(_pic.texture);
        }
        _pic.texture = null;

        _pic.texture = t;
        _pic.color = new Color(_SelcetPic.color.r, _SelcetPic.color.g, _SelcetPic.color.b, 1.0f);
        _TopMenu.transform.localPosition = new Vector3(0, FitUI.DESIGN_HEIGHT / 2 + 200 * FitUI.GetFitUIScale(), 0);

        if (needSelect)
        {
            Invoke("DelayShowPicArea", 0.1f);
        }
        else
        {
            ShowPicArea(false);
        }
    }
    public void ShowPicArea(bool needSelect)
    {
        _picArea.SetActive(true);

        _ShowingFilter = false;
        _AdjustFilter.gameObject.SetActive(false);
        _SelcetPic.gameObject.SetActive(false);
        _pic.gameObject.SetActive(true);
        //最好有个动画
        //_colorInfo.SetActive(true);
        _TextArea.SetActive(true);
        _TopMenu.SetActive(true);
        _ImageEffectMenu.SetActive(false);
        _picArea.transform.Find("OK&Cancel").gameObject.SetActive(false);
        _picArea.transform.Find("Selected").gameObject.SetActive(false);
        GameObject top = _picArea.transform.Find("Top").gameObject;
        top.SetActive(true);

        LeanSelectable tls = _picArea.GetComponent<LeanSelectable>();
        if (tls.IsSelected)
        {
            //btn需要false
            top.transform.Find("SetZOrderBtn").gameObject.SetActive(false);
            top.transform.Find("HideBtn").gameObject.SetActive(false);
            tls.Deselect(false, false);
        }

        if (needSelect)
        {
            tls.Select();
            //将图置顶
            int pic = _picArea.transform.GetSiblingIndex();
            int shiju = _TextArea.transform.GetSiblingIndex();
            if (pic < shiju) //如果图在文字下方，交换层级
            {
                switchZOrder();
            }
        }
        else
        {
            //将图置底
            int pic = _picArea.transform.GetSiblingIndex();
            int shiju = _TextArea.transform.GetSiblingIndex();
            if (pic > shiju)//如果图在文字上方，交换层级
            {
                switchZOrder();
            }
        }

        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(_TopMenu.transform.DOLocalMoveY(_TopMenuPos.y, 1.0f))
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {

                if (needSelect)
                {
                    string[] infos = { "当前选中图片区域，<b>单指左右滑动</b>可调节图片透明度。" ,
                        "未选中区域时，<b>长按屏幕</b>可以切换图/文层级。",
                        "当前选中图片区域，<b>双指触摸</b>可移动、旋转、缩放图片。",
                        "当前选中图片区域，<b>单指双击</b>可重置图片位置、大小、角度。",
                        "当前选中图片区域，<b>点击非图片区域</b>可取消选中。"};

                    int tipCnt = Setting.getPlayerPrefs("MORE" + Setting.SETTING_KEY.SHOW_PIC_TO_MAIN_TIP, 0);
                    if (tipCnt < Define.BASIC_TIP_CNT * 5)
                    {
                        ShowToast(infos[HZManager.GetInstance().GenerateRandomInt(0, infos.Length)], 3.0f);
                    }
                }
            });


        _picArea.GetComponent<LeanSelectable>().enabled = true;

        OnShowPic.Invoke(true);
    }
    public void DelayShowPicArea()
    {
        ShowPicArea(true);
    }
    public void OnAdjustCancelBtnClick()
    {
        _ShowingFilter = false;
        _AdjustFilter.gameObject.SetActive(false);
        _ImageEffectMenu.SetActive(false);
        _SelcetPic.gameObject.SetActive(false);
        _picArea.SetActive(false);
        _pic.gameObject.SetActive(false);
        //_colorInfo.SetActive(true);
        _TextArea.SetActive(true);
        _TopMenu.SetActive(true);
        _picArea.transform.Find("OK&Cancel").gameObject.SetActive(false);
        _picArea.transform.Find("Top").gameObject.SetActive(false);
        _picArea.transform.Find("Selected").gameObject.SetActive(false);

        //
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(_TopMenu.transform.DOLocalMoveY(_TopMenuPos.y, 1.0f))
            .SetEase(Ease.OutBounce);
        _TopMenu.transform.localPosition = new Vector3(0, FitUI.DESIGN_HEIGHT / 2 + 200 * FitUI.GetFitUIScale(), 0);

        _picArea.GetComponent<LeanSelectable>().enabled = true;

    }
    // 左右滑改变图片的透明度
    public void OnAdjustPicAlpha(Vector2 prevPos, Vector2 currentPos)
    {
        float x = currentPos.x - prevPos.x;

        float a = _pic.color.a * 255;//0-1

        a += x;
        if (a > 255) a = 255;
        if (a < 10) a = 10;

        _pic.color = new Color(_pic.color.r, _pic.color.g, _pic.color.b, a / 255.0f);

        //位置不变
        _pic.transform.localPosition = prevPos;
    }

    //区域选中、去选中动画
    public enum eSelectType
    {
        SHIJU,
        COLORINFO,
        PIC,
        TOP,
        //...
    }
    public void OnHideArea(string type)
    {
        GameObject which = null;
        if (type == "" + eSelectType.SHIJU)
        {
            which = _TextArea;
        }
        else if (type == "" + eSelectType.COLORINFO)
        {
            which = _colorInfo;
        }
        else if (type == "" + eSelectType.PIC)
        {
            which = _picArea;

            OnShowPic.Invoke(false);
        }

        which.SetActive(false);
    }
    public void OnSelectDown(string type)
    {
        if (_leanSelect == null) return;
        if (_ShowingFilter) return;
        //if (_subMenuCtrl.activeSelf) return;

        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("OnSelectDown" + type);
        GameObject which = null;
        Button zOrderBtn = null;
        Button hideBtn = null;

        if (type == "" + eSelectType.SHIJU)
        {
            which = _TextArea;
            zOrderBtn = _TextArea.transform.Find("Top/SetZOrderBtn").GetComponent<Button>();
            hideBtn = _TextArea.transform.Find("Top/HideBtn").GetComponent<Button>();
            zOrderBtn.gameObject.SetActive(true);
            hideBtn.gameObject.SetActive(false);//诗词区域不支持主动隐藏，需要设置
            // 如果图片没有显示，则隐藏置顶按钮
            if (!_picArea.activeSelf)
            {
                zOrderBtn.gameObject.SetActive(false);
            }
        }
        else if (type == "" + eSelectType.COLORINFO)
        {
            which = _colorInfo;
        }
        else if (type == "" + eSelectType.PIC)
        {
            which = _picArea;
            zOrderBtn = _picArea.transform.Find("Top/SetZOrderBtn").GetComponent<Button>();
            hideBtn = _picArea.transform.Find("Top/HideBtn").GetComponent<Button>();
            hideBtn.gameObject.SetActive(true);
            zOrderBtn.gameObject.SetActive(true);
        }

        //非c
        Color c = _mainCamera.backgroundColor * 0.5f;
        Image[] s = { };
        if (type != "" + eSelectType.COLORINFO)
        {
            GameObject ws = which.transform.Find("Selected").gameObject;

            ws.SetActive(true);

            s = ws.GetComponentsInChildren<Image>();
            foreach (var img in s)
            {
                img.color = new Color(c.r, c.g, c.b, 0.0f);
            }
        }

        //动画前显示
        if (zOrderBtn != null)
        {
            zOrderBtn.GetComponent<Image>().color = new Color(0, 0, 0, 150 / 255.0f);
            zOrderBtn.GetComponentInChildren<Text>().color = new Color(200 / 255.0f, 200 / 255.0f, 200 / 255.0f, 200 / 255.0f);
            hideBtn.GetComponent<Image>().color = new Color(0, 0, 0, 150 / 255.0f);
            hideBtn.GetComponentInChildren<Text>().color = new Color(200 / 255.0f, 200 / 255.0f, 200 / 255.0f, 200 / 255.0f);

            _leanSelect.SetActive(false);
        }

        Image lcImg = which.GetComponent<Image>();

        //禁止操作
        float cs = which.transform.localScale.x;
        mySequence
            .Append(which.transform.DOScale(cs * 0.8f, 1.0f))
            .Join(lcImg.DOFade(50 / 255.0f, 1.0f));

        foreach (var ss in s)
        {
            mySequence.Join(ss.DOFade(c.a, 1.0f));
        }

        mySequence
            .Append(which.transform.DOScale(cs, 1.0f))
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                _leanSelect.SetActive(true);
                CheckGestureTips();
            });
    }

    //长按取消选中
    public void OnLongPressDeSelect(string type)
    {
        if (_ShowingFilter)
        {
            //不能切换
            return;
        }

        if (type == "" + eSelectType.PIC)
        {
            //取消选中
            LeanSelectable ls = _picArea.GetComponent<LeanSelectable>();
            ls.Deselect();
        }
        else if (type == "" + eSelectType.SHIJU)
        {
            //取消选中
            LeanSelectable ls = _TextArea.GetComponent<LeanSelectable>();
            ls.Deselect();
        }
    }
    public void OnDeSelect(string type)
    {
        if (_leanSelect == null) return;

        if (_ShowingFilter)
        {
            //不能切换
            return;
        }

        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("OnSelectUp" + type);
        GameObject which = null;
        Button zOrderBtn = null;
        Button hideBtn = null;
        GameObject ws = null;

        if (type == "" + eSelectType.SHIJU)
        {
            which = _TextArea;
            zOrderBtn = _TextArea.transform.Find("Top/SetZOrderBtn").GetComponent<Button>();
            hideBtn = _TextArea.transform.Find("Top/HideBtn").GetComponent<Button>();
            hideBtn.gameObject.SetActive(false);
        }
        else if (type == "" + eSelectType.COLORINFO)
        {
            which = _colorInfo;
        }
        else if (type == "" + eSelectType.PIC)
        {
            which = _picArea;
            zOrderBtn = _picArea.transform.Find("Top/SetZOrderBtn").GetComponent<Button>();
            hideBtn = _picArea.transform.Find("Top/HideBtn").GetComponent<Button>();
            hideBtn.gameObject.SetActive(true);
        }

        Image[] s = { };
        if (type != "" + eSelectType.COLORINFO)
        {
            ws = which.transform.Find("Selected").gameObject;
            s = ws.GetComponentsInChildren<Image>();
        }


        Image lcImg = which.GetComponent<Image>();

        if (zOrderBtn != null)
        {
            float cs = which.transform.localScale.x;

            _leanSelect.SetActive(false);
            mySequence
                .Append(which.transform.DOScale(cs * 0.8f, 0.5f))
                .Append(which.transform.DOScale(cs, 0.5f))
                .Join(lcImg.DOFade(0.0f, 1.0f))
                .Join(zOrderBtn.GetComponent<Image>().DOFade(0.0f, 1.0f))
                .Join(zOrderBtn.GetComponentInChildren<Text>().DOFade(0.0f, 1.0f))
                .Join(hideBtn.GetComponent<Image>().DOFade(0.0f, 1.0f))
                .Join(hideBtn.GetComponentInChildren<Text>().DOFade(0.0f, 1.0f));

            foreach (var ss in s)
            {
                mySequence.Join(ss.DOFade(0.0f, 1.0f));
            }

            mySequence
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    if (ws != null)
                    {
                        ws.SetActive(false);
                    }
                    zOrderBtn.gameObject.SetActive(false);
                    _leanSelect.SetActive(true);
                });
        }
        else
        {
            float cs = which.transform.localScale.x;

            _leanSelect.SetActive(false);
            mySequence
                .Append(which.transform.DOScale(cs * 0.8f, 0.5f))
                .Append(which.transform.DOScale(cs, 0.5f))
                .Join(lcImg.DOFade(0.0f, 1.0f));

            foreach (var ss in s)
            {
                mySequence.Join(ss.DOFade(0.0f, 1.0f));
            }

            mySequence
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    if (ws != null)
                    {
                        ws.SetActive(false);
                    }
                    _leanSelect.SetActive(true);
                });
        }
    }
#endregion

#region 设置界面相关
    public Setting _setting;
    public void OnOpenSetting(bool open)
    {
        OpenPanel(open,_setting.gameObject);
    }

    public Dict _dict;
    public void OnOpenDict(bool open)
    {
        OpenPanel(open, _dict.gameObject);
    }

    //中文乱序
    public CnWordsChaos _chaos;
    public void OnOpenChaos(bool open)
    {
        OpenPanel(open, _chaos.gameObject);
    }

    //网文生成器
    public Article _article;
    public void OnOpenArticle(bool open)
    {
        OpenPanel(open, _article.gameObject);
    }

    public ColorGame _colorGame;
    public void OnOpenColorGame(bool open)
    {
        OpenPanel(open, _colorGame.gameObject);

        //登陆gamecenter，不重复设置
        if (!_GameCenter.activeSelf)
        {
            _GameCenter.SetActive(true);
        }
    }

    public PickColor _pickColor;
    //取色界面主动触发的返回
    public void OnClosePickColor(Action cb)
    {
        OpenPanel(false, _pickColor.gameObject,cb);
    }
    public void OnOpenPickColor(bool open)
    {
        OpenPanel(open, _pickColor.gameObject);
    }

    //辨色游戏触发制作信笺
    public void OnColorGameMakeXJ(List<string> colorInfo, string des)
    {
        UsePickColor(colorInfo, des,false);
    }

    //色库颜色不能被收藏--因为有固定的色库可以查看
    private void UsePickColor(List<string> colorInfo,string des,bool showTip)
    {
        CurrentColorInfo.Clear();
        CurrentColorInfo.AddRange(colorInfo);
        Color BgColor = new Color(int.Parse(CurrentColorInfo[3]) / 255.0f,
                          int.Parse(CurrentColorInfo[4]) / 255.0f,
                          int.Parse(CurrentColorInfo[5]) / 255.0f);

        //此处会重置字体颜色根据背景色，而不会采用当前颜色
        //_CurrentHZParam.zsHZColor = Define.GetUIFontColorByBgColor(BgColor,Define.eFontAlphaType.FONT_ALPHA_255);

        if (showTip)
        {
            ShowToast("修改<b>字体颜色</b>，请点击<b>装饰->文字</b>", BgColor, 3.0f, 0.5f);
        }

        ChangeColor(BgColor, true);

        string shiju = des;

        string[] shiJu = shiju.Split('\n');
        List<string> shiJuList = new List<string>();

        List<string> fmt = new List<string>();
        for (int i = 0; i < shiJu.Length; i++)
        {
            string s = Regex.Replace(shiJu[i], @"\s", "");
            if (s.Length != 0)
            {
                fmt.Add(shiJu[i]);
            }
        }

        if (fmt.Count == 0) return;

        //不限制长度，由输入者自行控制
        int showLine = fmt.Count;
        if (Setting.GetShowSJLine() != Setting.ShowSJLine.LINE_ALL)
        {
            showLine = (int)Setting.GetShowSJLine();
        }

        if (showLine >= fmt.Count)
        {
            showLine = fmt.Count;
        }
        else
        {
            ShowToast("描述超过设定<b>显示行数</b>被截掉，可到设置界面修改", BgColor, 3f,3f);
        }

        if (showLine == 0)
        {
            return;
        }

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        string author = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.AUTHOR, "诗色");

        for (int i = 0; i < showLine; i++)
        {
            shiJuList.Add(fmt[i]);
        }

        if (Setting.GetShowAuthorLine())
        {
            string zz = "||" + author;
            if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
               || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
            {
                zz = "——" + author;
            }

            shiJuList.Add(zz);
        }

        ResetUsingLike();// 需要重置不再使用收藏参数
        UseAlignment(alignmentType, shiJuList);


        //如果当前是曲线模式，则提示
        if (string.Join("", shiJuList).Length > Define.MAX_FX_HZ_NUM)
        {
            if (_FXArea.activeSelf)
            {
                ShowToast("显示字数不能超过32个，超出部分自动截断。");
            }
        }
    }

    private void OpenPanel(bool open,GameObject panel,Action finishCb = null)
    {
        Sequence mySequence = DOTween.Sequence();
        _mask.gameObject.SetActive(true);
        panel.gameObject.SetActive(true);
        float toX = 0;
        if (open)
        {
            panel.transform.localPosition = new Vector3(FitUI.GetIsPad() ? 1080 * 1.5f : 1080.0f,
                                              panel.transform.localPosition.y,
                                              panel.transform.localPosition.z);
            toX = 0.0f;
            mySequence
                .Append(panel.transform.DOLocalMoveX(toX, 1.2f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    panel.gameObject.SetActive(open);
                    _mask.gameObject.SetActive(false);
                    finishCb?.Invoke();
                });

        }
        else
        {
            panel.transform.localPosition = new Vector3(0,
                                              panel.transform.localPosition.y,
                                              panel.transform.localPosition.z);
            toX = 1080;
            if (FitUI.GetIsPad())
            {
                toX = 1080 * 1.5f;
            }

            mySequence
                .Append(panel.transform.DOLocalMoveX(toX, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    panel.gameObject.SetActive(open);
                    _mask.gameObject.SetActive(false);
                    finishCb?.Invoke();
                });
        }
    }
#endregion

#region 收藏界面相关
    //打开收藏界面
    public Like _likePanel;
    public void OnOpenPickColorLike()
    {
        //
        OnShowLike.Invoke(Like.eLikeType.ePickColor, int.Parse(CurrentColorInfo[7]), "", "");

        OnOpenLike(true);
    }

    public void OnOpenSysColorLike()
    {
        string title = "";
        string content = "";

        List<string> sj = GetCurrentShiJu();

        if (sj[sj.Count - 1].Contains("||") || sj[sj.Count - 1].Contains("——"))
        {
            //有标题且显示了才会有title
            if (Setting.GetShowAuthorLine())
            {
                title = sj[sj.Count - 1];
                title = title.Replace("||", "——");
            }

            List<string> sj2 = new List<string>();
            for (int i = 0; i < sj.Count - 1; i++)
            {
                sj2.Add(sj[i]);
            }

            content = String.Join("#", sj2.ToArray());
        }
        else
        {
            //无标题
            content = String.Join("#", sj.ToArray());
        }

        OnShowLike.Invoke(Like.eLikeType.eSysColor, int.Parse(CurrentColorInfo[7]), title, content);
        

        OnOpenLike(true);
    }
    public void OnOpenLike(bool open)
    {
        Sequence mySequence = DOTween.Sequence();

        float toX = 0;
        GameObject likes = _likePanel.transform.Find("Likes").gameObject;
        Image bg = _likePanel.transform.Find("Bg").gameObject.GetComponent<Image>();
        if (open)
        {
            //打开动画
            _leanTouch.SetActive(!open);
            _likePanel.gameObject.SetActive(open);

            likes.transform.localPosition = new Vector3(1080 * 1.5f,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = ((FitUI.DESIGN_HEIGHT / Screen.height) * Screen.width) / 2;
            mySequence
                .Append(bg.DOFade(0.0f, 0.0f))
                .AppendInterval(0.1f)//避免第一次显示时的短暂白屏
                .Append(likes.transform.DOLocalMoveX(toX, 1.2f))
                .SetEase(Ease.OutBounce)
                .Join(bg.DOFade(100 / 255.0f, 1.2f))
                .OnComplete(() =>
                {
                    //
                });

        }
        else
        {
            //关闭动画
            likes.transform.localPosition = new Vector3(((FitUI.DESIGN_HEIGHT / Screen.height) * Screen.width) / 2,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = 1080 * 1.5f;

            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 0.5f))
                .SetEase(Ease.InSine)
                .Join(bg.DOFade(0.0f, 0.5f))
                .OnComplete(() =>
                {
                    _leanTouch.SetActive(!open);
                    _likePanel.gameObject.SetActive(open);
                });
        }
    }

    [Serializable] public class OnShowLikeEvent : UnityEvent<Like.eLikeType,int, string, string> { }
    public OnShowLikeEvent OnShowLike;

    private bool _UsingLike = false;
    private bool _UsingShowAuthorLine = false;
    private void ResetUsingLike(bool usingLike = false,
                                bool usingShowAuthorLine = false)
    {
        _UsingShowAuthorLine = usingShowAuthorLine;
        _UsingLike = usingLike;
    }


    public void OnLikeItemClick(Like.eLikeType type, LikeItem.sLikeItem li)
    {
        OnOpenLike(false);

        CurrentColorInfo = PickColor.GetColorByID(li.ColorID);

        Color BgColor = new Color(int.Parse(CurrentColorInfo[3]) / 255.0f,
                                  int.Parse(CurrentColorInfo[4]) / 255.0f,
                                  int.Parse(CurrentColorInfo[5]) / 255.0f);

        if (type == Like.eLikeType.eSysColor)
        {
            ShowToast("收藏使用<b>字饰颜色</b>，修改请点击<b>装饰->文字</b>", BgColor, 3.0f);
        }
        else
        {
            ShowToast("如果看不清字，请点击<b>装饰->文字</b>修改字体颜色", BgColor, 3.0f);

        }

        ChangeColor(BgColor, true);

        string title = li.Title;
        string content = li.Content;
        List<string> sj = new List<string>();
        if (title != "" && type == Like.eLikeType.eSysColor)
        {
            // 使用标题
            Setting.AlignmentType al = Setting.GetAlignmentType();
            if (al == Setting.AlignmentType.LEFT_VERTICAL
                || al == Setting.AlignmentType.RIGHT_VERTICAL)
            {
                title = title.Replace("——", "||");
            }

            content += "#" + title;

            ResetUsingLike(true, true);
        }
        else
        {
            //不使用标题
            ResetUsingLike(true, false);
        }

        if (type == Like.eLikeType.eSysColor)
        {
            sj.AddRange(content.Split('#'));
        }
        else
        {
            //sj.AddRange(content.Split('\n'));
            string[] shiJu = content.Split('\n');
            List<string> fmt = new List<string>();
            for (int i = 0; i < shiJu.Length; i++)
            {
                string s = Regex.Replace(shiJu[i], @"\s", "");
                if (s.Length != 0)
                {
                    fmt.Add(shiJu[i]);
                }
            }

            if (fmt.Count == 0) return;

            //不限制长度，由输入者自行控制
            int showLine = fmt.Count;
            if (Setting.GetShowSJLine() != Setting.ShowSJLine.LINE_ALL)
            {
                showLine = (int)Setting.GetShowSJLine();
            }

            if (showLine >= fmt.Count)
            {
                showLine = fmt.Count;
            }
            else
            {
                ShowToast("描述超过设定<b>显示行数</b>被截掉，可到设置界面修改", BgColor, 3f, 3f);
            }

            if (showLine == 0)
            {
                return;
            }

            for (int i = 0; i < showLine; i++)
            {
                sj.Add(fmt[i]);
            }

        }

        ChangeShiJu(sj);
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        ShowToast(content,_mainCamera.backgroundColor, showTime, delay);
    }

    public void ShowToast(string content,Color c, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = c;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }

    private CJHZ.HZParam _CurrentHZParam;
    //防止卡顿
    private void DelayZS(){
        //将所有汉字的背景图片替换为选择图片，这里需要处理是否正在动画中
        if (_UsingFX) return;

        ChangeShiJu(GetCurrentShiJu());
    }

    public void OnLineRenderChangHSAlpha(float a)
    {
        _CurrentHZParam.hsImgColor = new Color(_CurrentHZParam.hsImgColor.r,
            _CurrentHZParam.hsImgColor.g,
            _CurrentHZParam.hsImgColor.b,a);

        // 此处不需要更新到文本
    }
    public void OnZSAdjustFinish(ZhuangShiBtnItem.ZSParam param)
    {
        if(param.btnType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            _CurrentHZParam.zsHZColor = param.color;
            Invoke("DelayZS", 0.2f);
        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _CurrentHZParam.zsImgPath = param.path;
            _CurrentHZParam.zsImgSize = param.size;
            _CurrentHZParam.zsImgColor = param.color;
            Invoke("DelayZS", 0.2f);
        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            _CurrentHZParam.hsImgSize = param.size;
            _CurrentHZParam.hsImgColor = param.color;
            if(param.size <= 2.0f)
            {
                ShowToast("当<b>行线宽度</b>过窄时，某些行线可能无法正确呈现");
            }

            Invoke("DelayZS", 0.2f);
        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
        {

        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.DianZhui)
        {

        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
        {

        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
        {

            if(_picArea.activeSelf)
            {
                if(param.path == "")
                {
                    _pic.material = Resources.Load("Shape/Material/F0", typeof(Material)) as Material;
                }
                else
                {
                    //设置pic的材质的mask 为选择的图
                    _pic.material = Resources.Load(param.path.Replace("Basic", "Material"), typeof(Material)) as Material;
                }
            }
            else
            {
                ShowToast("需要<b>先选择照片</b>用作配图后，才可以使用相框 :)");
            }

        }
    }

    #endregion

    private bool _isInGaming = false;
    public void OnInGaming(bool gameing){
        _isInGaming = gameing;
    }

    public void OnMakeXinJian(int sjID, string currentSJ, HZManager.eShiCi fw)
    {

        _isInGaming = false;
        ExpandTopMenu();

        if (sjID == -1) return;

        List<string> sj = new List<string>();

        //获取格式化后的诗词语句列表
        List<string> tsscData = HZManager.GetInstance().GetTSSC(fw, sjID);
        List<string> tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[4]);


        //只显示当前句
        sj.Add(currentSJ);

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        //  需要显示署名
        if (Setting.GetShowAuthorLine())
        {

            //作者
            string zz = tsscData[1] + "||" + tsscData[2] + "·" + tsscData[3];
            if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
               || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
            {
                zz = tsscData[1] + "——" + tsscData[2] + "·" + tsscData[3];
            }

            sj.Add(zz);
        }

        Sequence mySequence = DOTween.Sequence();
        mySequence
            .AppendInterval(1.0f)
            .AppendCallback(() =>
            {
                //要不要显示签名
                UseAlignment(Setting.GetAlignmentType(), sj);
            });
    }

#region 预览图制作相关
    ////////---------------------------制作屏幕截图------------------------------
    /// 
    /// 
    //获取屏幕截图用于应用市场
    public void GetScreenShot(bool showTop)
    {

        if (!showTop)
        {
            Sequence mySequence = DOTween.Sequence();

            mySequence
                .Append(_TopMenu.transform.DOLocalMoveY(_TopMenuPos.y + 200, 0.4f))
                .SetEase(Ease.InSine)
                .AppendInterval(0.1f)
                .OnComplete(() =>
                {
                    StartCoroutine(CaptureScreen2(showTop));
                });
        }
        else
        {
            StartCoroutine(CaptureScreen2(showTop));
        }
    }

    private IEnumerator CaptureScreen2(bool showTop)
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        // do something with texture

        var data = texture.EncodeToJPG();

        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

        string fn = Convert.ToInt64(ts.TotalSeconds).ToString() + ".jpg";
        StreamManager.SaveFile(fn, data, FolderLocations.Pictures, ((bool succeeded) =>
        {
            if (succeeded)
            {
                if (!showTop)
                {
                    Tween topMenu = _TopMenu.transform.DOLocalMoveY(_TopMenuPos.y,
                                                                    0.4f);
                }
            };
        })
        );

        // cleanup
        Destroy(texture);
    }

    public void InputText(string t)
    {
        string shiju = t;

        //0，支持文字、底图、行线颜色、透明度等调整#超多新颖照片相框，完美图文混排
        //1，支持多种排版，随心选择#超多国色可选，随心搭配
        //2，支持多种手势，制作信笺效果更灵动#多项参数可调，操作更方便简单
        //3，支持添加配图，可调层级透明度#支持输入文本，书写专属自己的诗

        //4，唐诗宋词应有尽有，诗词爱好者福音+++//欲寄彩笺兼尺素。#山长水阔知何处。#知否，知否，应是绿肥红瘦。

        //5，听/风说/信是有色的诗
        //如果颜色表达心情#那么文字就是心事
        //一个人/一封信/一个驿站
        //欲书又止/略略


        string[] shiJu = shiju.Split('#');
        List<string> shiJuList = new List<string>();

        List<string> fmt = new List<string>();
        for (int i = 0; i < shiJu.Length; i++)
        {
            string s = Regex.Replace(shiJu[i], @"\s", "");
            if (s.Length != 0)
            {
                fmt.Add(shiJu[i]);
            }
        }

        if (fmt.Count == 0) return;

        //不限制长度，由输入者自行控制
        int showLine = fmt.Count;
        if (Setting.GetShowSJLine() != Setting.ShowSJLine.LINE_ALL)
        {
            showLine = (int)Setting.GetShowSJLine();
        }

        if (showLine > fmt.Count)
        {
            showLine = fmt.Count;
        }

        Setting.AlignmentType alignmentType = Setting.GetAlignmentType();
        bool showAuthorLine = Setting.GetShowAuthorLine();

        if (showLine == 0)
        {
            string zz = "||" + "诗色";
            if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
               || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
            {
                zz = "——" + "诗色";
            }

            shiJuList.Add("写给你的信");
            if (showAuthorLine)
            {
                shiJuList.Add(zz);
            }
        }
        else if (showLine == 1)
        {
            if (showAuthorLine)
            {
                string zz = "||" + "诗色";
                if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
                   || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
                {
                    zz = "——" + "诗色";
                }

                shiJuList.Add(fmt[0]);
                shiJuList.Add(zz);
            }
            else
            {
                shiJuList.Add(fmt[0]);
            }
        }
        else
        {
            if(showLine == fmt.Count)//可以+1
            {
                for (int i = 0; i < showLine; i++)
                {
                    if (i == fmt.Count - 1)
                    {
                        //  最后一行为署名
                        string zz = "||" + fmt[i];
                        if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
                           || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
                        {
                            zz = "——" + fmt[i];
                        }

                        if (showAuthorLine)
                        {
                            shiJuList.Add(zz);
                        }
                        else
                        {
                            shiJuList.Add(fmt[i]);
                        }
                    }
                    else
                    {
                        shiJuList.Add(fmt[i]);
                    }
                }
            }
            else
            { // showLine < fmt.Count 
                for (int i = 0; i < showLine; i++)
                {
                    shiJuList.Add(fmt[i]);
                }

                if (showAuthorLine)
                {
                    //  最后一行为署名
                    string zz = "||" + fmt[fmt.Count - 1];
                    if (alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL
                       || alignmentType == Setting.AlignmentType.RIGHT_HORIZONTAL)
                    {
                        zz = "——" + fmt[fmt.Count -1];
                    }
                    shiJuList.Add(zz);
                }
            }
        }

        UseAlignment(alignmentType, shiJuList);
    }

    //保存曲线图标
    public FXLineRender LR;
    public void SaveIcon(float w)
    {
        _mainCamera.backgroundColor = new Color(0,0,0,0);
        GameObject fl =  _FXArea.transform.Find("Viewport/Content/FXLine").gameObject;
        RawImage rt = fl.GetComponent<RawImage>();
        rt.material.SetColor("_FilterColor",new Color(0,0,0));

        LR.SaveIcon(w);
        Invoke("DelaySave", 0.1f);
    }

    public void SaveGSSJIcon(float w)
    {
        Invoke("DelaySave", 0.1f);
    }

    private void DelaySave()
    {
        StartCoroutine(ReadData());
    }

    private IEnumerator ReadData()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenShot = new Texture2D(1024, 1024);
        screenShot.ReadPixels(new Rect(28, 1920/2-1024/2, 1024, 1024), 0, 0);
        screenShot.Apply();

        StreamManager.SaveFile("Icon/"+LR.GetIconName()+".jpg", screenShot.EncodeToJPG(), FolderLocations.Pictures, ((bool succeeded) =>
        {
        })
        );
    }
    #endregion
}
