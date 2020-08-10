/**************************************/
//FileName: Setting.cs
//Author: wtx
//Data: 14/01/2019
//Describe:  设置界面
/**************************************/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using Reign;
using Lean.Touch;
using System;
using com.mob;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Setting: MonoBehaviour{
    // Use this for initialization
    void Start()
    {
        TitleMagic();

        _lineSP.gameObject.SetActive(_ShowLineSeparator);
        _tgl.isOn = _ShowLineSeparator;

        _authorText.gameObject.SetActive(_ShowAuthorLine);
        _authorLineText.text = "——"+getPlayerPrefs(""+SETTING_KEY.AUTHOR,"诗色");
        _tglAuthor.isOn = _ShowAuthorLine;

        if (_ShowAuthorLine)
        {
            _setAuthorInput.SetActive(true);
            _infoBg.SetActive(true);
        }
        else
        {
            _setAuthorInput.SetActive(false);
            _infoBg.SetActive(false);
        }


        _splashText.gameObject.SetActive(GetShowSplash());
        _tglSplash.isOn = GetShowSplash();

        if (GetFontBold())
        {
            _fontText.fontStyle = FontStyle.Bold;
            _tglFont.isOn = true;
        }
        else
        {
            _fontText.fontStyle = FontStyle.Normal;
            _tglFont.isOn = false;
        }

        if (GetTheme())
        {
            _themeText.text = "时间主题·随时间变化";
            _tglTheme.isOn = true;
        }
        else
        {
            _themeText.text = "纯色背景·不随时间变化";
            _tglTheme.isOn = false;
        }


        if (_AlignmentType == AlignmentType.LEFT_VERTICAL)
        {
            VL.interactable = false;
            VR.interactable = true;
            HL.interactable = true;
            HR.interactable = IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT);

        }
        else if (_AlignmentType == AlignmentType.RIGHT_VERTICAL)
        {
            VL.interactable = true;
            VR.interactable = false;
            HL.interactable = true;
            HR.interactable = IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT);
        }
        else if (_AlignmentType == AlignmentType.LEFT_HORIZONTAL)
        {
            VL.interactable = true;
            VR.interactable = true;
            HL.interactable = false;
            HR.interactable = IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT);
        }
        else if (_AlignmentType == AlignmentType.RIGHT_HORIZONTAL)
        {
            VL.interactable = true;
            VR.interactable = true;
            HL.interactable = true;
            HR.interactable = false;
        }

        ChangeAligmentImgColor(true);

        DoZanDonateAni();
        DoAlignmentAni();
        DoSpeedLevelValueChange(true);
        DoShowSJLineValueChange(true);

        //启动默认的是false
        if (_UseHZAni)
        {
            //
            _UseHZAniBtn.interactable = false;
            EasyTween[] ets = _UseHZAniBtn.GetComponentsInChildren<EasyTween>(true);
            foreach (var et in ets)
            {
                et.OpenCloseObjectAnimation();

                //原则上可以执行字体动画，由于不是主动触发，不再执行字体动画
            }
        }
        else
        {

        }
    }

    void OnDestroy()
    {

    }
    void OnEnable()
    {
        InitBuy();

        //如果之前保存了非默认启动色，则自动切换一下
        EasyTween etDft = _DefaultColor.GetComponent<EasyTween>();
        EasyTween etCrt = _CurrentColor.GetComponent<EasyTween>();
        if (_CurrentBGColorID != _StartColorID)
        {
            if(etDft.animationParts.ObjectState == UITween.AnimationParts.State.OPEN)
            {
                etDft.OpenCloseObjectAnimation();//关闭默认
            }

            if (etCrt.animationParts.ObjectState == UITween.AnimationParts.State.CLOSE)
            {
                etCrt.OpenCloseObjectAnimation();//显示当前
            }
        }
        else
        {
            if (etDft.animationParts.ObjectState == UITween.AnimationParts.State.CLOSE)
            {
                etDft.OpenCloseObjectAnimation();//显示默认
            }

            if (etCrt.animationParts.ObjectState == UITween.AnimationParts.State.OPEN)
            {
                etDft.OpenCloseObjectAnimation();//关闭当前
            }
        }
    }
    void OnDisable()
    {

    }

    public enum SETTING_KEY{
        START_CNT,
        OPEN_SETTING_CNT,
        SHOW_APP_CNT,
        SHOW_AUTHOR_LINE,
        USE_HZ_ANI,
        SHOW_LINE_SEPARATORE,
        SPEED_LEVEL,
        ALIGNMENT_TYPE,
        SHOW_SJ_LINE,
        LIKE_ID_LIST,
        HAS_SHOW_GESTURE,
        AUTHOR,
        FONT_BOLD,
        TIME_THEME,//主题场景
        SHOW_SPLASH,
        HIGHEST_SCORE,//最高得分
        HIGHEST_CG,//最多连续闯关
        ACHIEVEMENT_COMPLETE_PERCENT,//成就完成比
        ALL_HIGHEST_CG,//所有最高闯关数
        SHOW_DAAN_CNT_RECORD,//查看答案记录
        SHOW_DAAN_CNT,//查看答案次数
        BUY_SHOWDAAN_ANI,//引导购买动画次数，5次

        SHOW_ADJUST_PARAM_TIP,//提示再点一次可以调节更多参数
        SHOW_CHANGSJ_TIP,//提示再点一次可以调节更多参数
        SHOW_CHANGECOLOR_TIP,//提示再点一次可以调节更多参数
        SHOW_CHANGEZS_SJCOLOR_TIP,//提示修改诗句颜色
        SHOW_ADJUST_PIC_POS_TIP,
        SHOW_ADJUST_FILTER_TIP,
        SHOW_PIC_TO_MAIN_TIP,

        SHOW_FX_BASIC_TIP,//点击fxbtn的时候的提示
        SHOW_FX_TIP,// 点击具体曲线时候的提示
        SHOW_NO_FX_LINE_TIP,//当前没有显示行线，给予提示
        SHOW_FX_ADJUST_ONE_DIR_TIP,

        START_COLOR,//默认启动色

        TIME_THEME_TIPS,//主题场景提示
        SHOW_THEME_CUSTOM_TIPS,
        SHOW_THEME_ADJUST_CUSTOM_TIPS,

        HAS_SET_CUSTOM_THEME,//是否已经设置过情景
        YG_ADJUST_SETTING,
        TY_ADJUST_SETTING,
        TD_ADJUST_SETTING,
        DPX_ADJUST_SETTING,
        POS_ADJUST_SETTING,
        SHOW_SAVE_THEME_SETTING_TIPS,
        SHOW_PICKCOLOR_CLICK_EDIT_TIPS,

        //每周限免功能
        EVERYWEEK_FREE_REC,
        EVERYWEEK_FREE_FILTER,//滤镜 + 7个
        EVERYWEEK_FREE_FX,//曲线 + 7个
        EVERYWEEK_FREE_XK,//相框 +7个

        //辨色游戏
        COLOR_GAME_FINDCOLOR_SCORE,
        COLOR_GAME_SINGLE_SCORE,
        COLOR_GAME_MULTI_SCORE,
        COLOR_GAME_LINK_SCORE,
        COLOR_GAME_DECOMPOSE_SCORE,
        COLOR_GAME_COMPOSE_SCORE,

        //功能提示
        SHOW_CPY_CHOAS_TXT_TIPS,
    }

    public enum ShowSJLine
    {
        LINE_1 = 1,
        LINE_2 = 2,
        LINE_4 = 4,
        LINE_ALL,
    }

    public enum SpeedLevel
    {
        SPEED_LEVEL_1,//2s
        SPEED_LEVEL_2,//1s
        SPEED_LEVEL_3,//0.5s
        SPEED_LEVEL_4,//0.25
        SPEED_LEVEL_5,//0.125s
        SPEED_LEVEL_6,//0.0625
        SPEED_LEVEL_7,//0.03125
        SPEED_LEVEL_8,//无动画
    }

    //文本排列设置
    public enum AlignmentType
    {
        LEFT_VERTICAL,//靠左竖排
        RIGHT_VERTICAL,//靠右竖排
        LEFT_HORIZONTAL,//靠左横排
        RIGHT_HORIZONTAL,//靠左横排排
    }

    private bool _HasInited = false;
    //参数
    public static int _StartColorID = Define.DEFAULT_START_COLORID;
    public static bool _ShowAuthorLine = true;
    public static bool _UseHZAni = false;
    public static bool _ShowLineSeparator = true;
    public static SpeedLevel _SpeedLevel = SpeedLevel.SPEED_LEVEL_3;
    public static bool _openTimeTheme = false;
    public static bool _useFontBold = true;

    public static AlignmentType _AlignmentType = AlignmentType.RIGHT_VERTICAL;
    public static ShowSJLine _ShowSJLine = ShowSJLine.LINE_2;

    public void OnInit()
    {
        //
        if (!_HasInited) {
            //从本地读取
            _ShowAuthorLine = getPlayerPrefs("" + SETTING_KEY.SHOW_AUTHOR_LINE, 1) == 1;
            _UseHZAni = getPlayerPrefs("" + SETTING_KEY.USE_HZ_ANI, 0) == 1;
            _ShowLineSeparator = getPlayerPrefs("" + SETTING_KEY.SHOW_LINE_SEPARATORE, 1) == 1;

            _SpeedLevel = (SpeedLevel)getPlayerPrefs("" + SETTING_KEY.SPEED_LEVEL, 2);

            _AlignmentType = (AlignmentType)getPlayerPrefs("" + SETTING_KEY.ALIGNMENT_TYPE, 1);
            _ShowSJLine = (ShowSJLine)getPlayerPrefs("" + SETTING_KEY.SHOW_SJ_LINE, 2);

            _StartColorID = getPlayerPrefs("" + SETTING_KEY.START_COLOR, Define.DEFAULT_START_COLORID);

            _oriZanBtnPosx = _zanBtn.transform.localPosition;

            _openTimeTheme = getPlayerPrefs("" + SETTING_KEY.TIME_THEME, 0) == 1;
            _useFontBold = getPlayerPrefs("" + SETTING_KEY.FONT_BOLD, 1) == 1;

        }
        //
        _HasInited = true;

        //Invoke("InitFree",5f);//5s之后初始化
    }


    //
    private void InitFree()
    {
        Define.GetNetDateTime((string dt) =>
        {
            //没有获取到，极有可能没有联网，这种情况无法使用【查看答案】次数
            if (dt == string.Empty)
            {
                setPlayerPrefs("" + SETTING_KEY.EVERYWEEK_FREE_FILTER, "");
                setPlayerPrefs("" + SETTING_KEY.EVERYWEEK_FREE_FX, "");
                setPlayerPrefs("" + SETTING_KEY.EVERYWEEK_FREE_XK, "");
                return;
            }

            string rec = getPlayerPrefs("" + SETTING_KEY.EVERYWEEK_FREE_REC, "");
            DateTime datetime = Convert.ToDateTime(dt);

            if (Define.GetWeek(datetime.DayOfWeek) == Define.eWeekType.Sunday)//周日重置
            {
                if (rec == "")
                {
                    ShowToast("每周随机限免7个(相框、滤镜、红线)，<b>周日打开应用可重置</b>");
                    setPlayerPrefs("" + SETTING_KEY.EVERYWEEK_FREE_REC, dt);
                    SetFree();
                }
                else
                {
                    DateTime datetime2 = Convert.ToDateTime(rec);
                    //不是同一天打开
                    if (datetime2.Day != datetime.Day)
                    {
                        setPlayerPrefs("" + SETTING_KEY.EVERYWEEK_FREE_REC, dt);
                        ShowToast("每周随机限免7个(相框、滤镜、红线)，<b>周日打开应用可重置</b>");
                        SetFree();
                    }
                }
            }
        });
    }
    private void SetFree()
    {
        //相框
        //随机7个不重复的数字[11-30]

        //滤镜
        //随机7个不重复的数字[]

        //红线
        //随机7个不重复的数字

    }
    //玩玩
    private string[] _titleMore = {
        "字里行间",// 字里行间
        "彩笺谁寄",//彩笺谁寄
        "只如初见",//启动动画
        "粗中有细",//字体粗细
        "字字珠玑",//字字珠玑
        "朝夕相伴",//主题场景
        "词颜诗色",//词颜诗色
        "纵横相宜",//纵横相宜
        "细品慢咽",//细品慢咽
        "千言万语",//千言万语
        "应有尽有",//应有尽有
        "知否知否",//知否知否
        "慷慨解囊",//慷慨解囊
        "墙外佳人",//墙外佳人
        "欲书与我",//欲书与我
        "心有感激",//心有感激
    };

    public Text _dZDSTitle;//点赞打赏标题，审核
    public void TitleMagic(){

        _More.SetActive(false);
        _donateBtn.gameObject.SetActive(false);
        _dZDSTitle.text = "点赞支持";
        //=>_titleMore
        int cnt = getPlayerPrefs("" + SETTING_KEY.OPEN_SETTING_CNT, 0);
        if (cnt > 0)
        {
            int r = HZManager.GetInstance().GenerateRandomInt(0, 100);
            if(cnt > r)
            {
                if(cnt > 20)
                {
                    Invoke("DoTitleMagic", 5.0f);
                }
                else
                {
                    Invoke("DoTitleMagic", 5+HZManager.GetInstance().GenerateRandomInt(20-cnt, 20));
                }
            }
        }

        setPlayerPrefs("" + SETTING_KEY.OPEN_SETTING_CNT, cnt + 1);


        //SHOW_APP_CNT
        int cnt2 = getPlayerPrefs("" + SETTING_KEY.SHOW_APP_CNT, 0);
        //启动应该不会出现3次？
        if (cnt2 > 1){
            _More.SetActive(true);
            _donateBtn.gameObject.SetActive(true);
            _dZDSTitle.text = "点赞打赏";
        }

        setPlayerPrefs("" + SETTING_KEY.SHOW_APP_CNT, cnt2 + 1);
    }

    public GameObject _Titles;
    public void DoTitleMagic(){
        DOTween.Kill("TitleMagic", true);
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("TitleMagic");
        Text [] titles = _Titles.GetComponentsInChildren<Text>(true);
        // 执行标题替换动画
        int cnt = 0;
        for (int i = 0; i < titles.Length; i++)
        {
            if (titles[i].name == "Title")
            {
                mySequence
                .AppendInterval(Mathf.Sqrt(cnt) * 0.2f)
                .Join(titles[i].DOText(_titleMore[cnt], 0.5f));
                cnt++;
            }
        }
    } 

    //
    public GameObject _settingPanel;
    public GameObject _ContentBg;
    public Image _TitleBtn;
    public Image[] _needChangeWhithSJBColor;
    public Image[] _neenChangeWhithBGColor;

    public Image[] _alignmentImg;
    public Button _UseHZAniBtn;
    public GameObject _More;

    public Image _BG;

    public void OnChangeBGColor(Color color,bool isChangeTheme){
        Color fc = Define.GetFixColor(color);

        _BG.color = color;
        _ContentBg.GetComponent<Image>().color = color;
        _TitleBtn.color = fc;
        //
        foreach(var img in _neenChangeWhithBGColor){
            img.color = fc;
        }

        Image[] AllImgs = gameObject.GetComponentsInChildren<Image>(true);
        foreach (var img in AllImgs)
        {
            if (img.name.Contains("Dark"))
            {
                img.color = Define.GetDarkColor(fc);
            }
            else if(img.name.Contains("Light"))
            {
                img.color = Define.GetLightColor(fc);
            }
        }

        ChangeAligmentImgColor(false);

        UpdateUIFontColor();
    }

    public void ChangeAligmentImgColor(bool isInit){

        Color lc0 = Define.GetLightColor(_BG.color);
        Color dc0 = Define.GetDarkColor(_BG.color);

        Color lc = Define.GetFixColor(lc0);
        Color dc = Define.GetFixColor(dc0);

        int alignment = (int)_AlignmentType;

        int startIndex = alignment * 3;
        for (int i = 0; i < _alignmentImg.Length;i += 3){
            if(startIndex == i){
                _alignmentImg[i].DOColor(lc,0.5f);
                _alignmentImg[i + 1].DOColor(lc, 0.5f);

                if (isInit)
                {
                    EasyTween sImg = _alignmentImg[i + 2].GetComponent<EasyTween>();
                    sImg.OpenCloseObjectAnimation();
                }
            }
            else{
                _alignmentImg[i].DOColor(dc, 0.5f);
                _alignmentImg[i + 1].DOColor(dc, 0.5f);
            }
        }
    }

    //----------------------参数获取--------------------------------------------
    public Text _splashText;
    public Toggle _tglSplash;
    public void SetShowSplash(bool show)
    {
        if (GetShowSplash() == show)
        {
            return;
        }

        setPlayerPrefs("" + SETTING_KEY.SHOW_SPLASH, show ? 1 : 0);

        //改变字体呈现
        Sequence mySequence = DOTween.Sequence();

        _tglSplash.interactable = false;
        if (show)
        {
            _splashText.gameObject.SetActive(true);
            mySequence
                .Append(_splashText.DOFade(0.0f, 0.0f))
                .Append(_splashText.DOFade(0.5f, 0.5f))
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    _tglSplash.interactable = true;
                });
        }
        else
        {
            _splashText.gameObject.SetActive(true);
            mySequence
                .Append(_splashText.DOFade(0.5f, 0.0f))
                .Append(_splashText.DOFade(0.0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _splashText.gameObject.SetActive(false);
                    _tglSplash.interactable = true;
                });
        }
    }
    public static bool GetShowSplash()
    {
        return getPlayerPrefs(""+SETTING_KEY.SHOW_SPLASH,1) == 1;
    }


    public Text _fontText;
    public Toggle _tglFont;
    [System.Serializable] public class OnChangeFontBoldEvent : UnityEvent<bool> { }
    public OnChangeFontBoldEvent OnChangeFontBold;
    public void SetFontBold(bool bold)
    {
        if (GetFontBold() == bold)
        {
            return;
        }

        _useFontBold = bold;
        setPlayerPrefs("" + SETTING_KEY.FONT_BOLD, bold ? 1 : 0);
        if (bold)
        {
            _fontText.fontStyle = FontStyle.Bold;
        }
        else
        {
            _fontText.fontStyle = FontStyle.Normal;
        }

        OnChangeFontBold.Invoke(bold);
    }
    public static bool GetFontBold()
    {
        return _useFontBold;
    }

    public Text _themeText;
    public Toggle _tglTheme;
    [Serializable] public class OnOpenThemeEvent : UnityEvent<bool> { }
    public OnOpenThemeEvent OnOpenTheme;
    public void SetTheme(bool open)
    {
        if (GetTheme() == open)
        {
            return;
        }

        _openTimeTheme = open;
        setPlayerPrefs("" + SETTING_KEY.TIME_THEME, open ? 1 : 0);
        if (open)
        {
            _themeText.text = "时间主题·随时间变化";
        }
        else
        {
            _themeText.text = "纯色背景·不随时间变化";
        }

        OnOpenTheme.Invoke(open);

        //TIME_THEME_TIPS
        int tipCnt = getPlayerPrefs("" + SETTING_KEY.TIME_THEME_TIPS, 0);
        if (tipCnt < Define.BASIC_TIP_CNT * 2)
        {
            if (open)
            {
                ShowToast("主题根据当前时间选择，可手动在<b>装饰</b>里选择");
            }
            else
            {
                ShowToast("主题为纯色背景，不随时间变化，诗文界面上/下滑动切换");
            }
            setPlayerPrefs("" + SETTING_KEY.TIME_THEME_TIPS, tipCnt + 1);
        }
        
    }
    public static bool GetTheme()
    {
        return _openTimeTheme;
    }


    [System.Serializable] public class OnChangeShowAuthorLineEvent : UnityEvent<bool> { }
    public OnChangeShowAuthorLineEvent OnChangeShowAuthorLine;
    public Text _authorText;
    public Toggle _tglAuthor;
    public GameObject _infoBg;
    public void SetShowAuthorLine(bool show){
        if (_ShowAuthorLine == show)
        {
            return;
        }

        _ShowAuthorLine = show;

        if (_ShowAuthorLine)
        {
            _setAuthorInput.SetActive(true);
            _infoBg.SetActive(true);
        }
        else
        {
            _setAuthorInput.SetActive(false);
            _infoBg.SetActive(false);
        }

        setPlayerPrefs(""+SETTING_KEY.SHOW_AUTHOR_LINE,show?1:0);

        //改变字体呈现
        Sequence mySequence = DOTween.Sequence();

        _tglAuthor.interactable = false;
        if (_ShowAuthorLine)
        {
            _authorText.gameObject.SetActive(true);
            mySequence
                .Append(_authorText.DOFade(0.0f, 0.0f))
                .Append(_authorText.DOFade(0.5f, 0.5f))
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    _tglAuthor.interactable = true;
                });
        }
        else
        {
            _authorText.gameObject.SetActive(true);
            mySequence
                .Append(_authorText.DOFade(0.5f, 0.0f))
                .Append(_authorText.DOFade(0.0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _authorText.gameObject.SetActive(false);
                    _tglAuthor.interactable = true;
                });
        }


        OnChangeShowAuthorLine.Invoke(show);
    }
    public static bool GetShowAuthorLine()
    {
        return _ShowAuthorLine;
    }

    //设置作者
    public Text _inputText;
    public Text _authorLineText;
    public GameObject _setAuthorInput;
    //设置作者行，仅适用于自输入
    public void SetAuthor()
    {
        //AUTHOR
        string s = Regex.Replace(_inputText.text, @"\s", "");
        if (s.Length == 0)
        {
            return;
        }

        setPlayerPrefs("" + SETTING_KEY.AUTHOR, s);
        _authorLineText.DOText("——"+s,1.0f);
    }

    public Text _ryrx;//若隐若现
    public Text _yrzs;//跃然纸上
    public void SetUseHZAni(bool use)
    {
        _UseHZAni = use;
        _UseHZAniBtn.interactable = false;
        //执行字体动画
        Sequence mySequence = DOTween.Sequence();

        if (_UseHZAni)
        {
            mySequence
                .Append(_yrzs.DOFade(0.0f, 0.0f))
                .Append(_yrzs.DOFade(1.0f, 0.5f))
                .Join(_yrzs.transform.DOShakeScale(0.5f, HZManager.GetInstance().GenerateRandomInt(5, 10) / 10.0f))
                .OnComplete(() =>
                {
                    _UseHZAniBtn.interactable = true;
                });
        }
        else
        {
            mySequence
                .AppendInterval(1.2f)
                .Append(_ryrx.DOFade(0.0f, 0.8f))
                .Append(_ryrx.DOFade(1.0f, 0.8f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    _UseHZAniBtn.interactable = true;
                });
        }

        setPlayerPrefs("" + SETTING_KEY.USE_HZ_ANI, use ? 1 : 0);

    }
    public static bool GetUseHZAni()
    {
        return _UseHZAni;
    }

    [System.Serializable] public class OnChangeShowLineSeparatorEvent : UnityEvent<bool> { }
    public OnChangeShowLineSeparatorEvent OnChangeShowLineSeparator;
    public Image _lineSP;
    public Toggle _tgl;
    public void SetShowLineSeparator(bool show)
    {
        if(_ShowLineSeparator == show){
            return;
        }

        _ShowLineSeparator = show;
        setPlayerPrefs("" + SETTING_KEY.SHOW_LINE_SEPARATORE, show ? 1 : 0);

        //改变字体呈现
        Sequence mySequence = DOTween.Sequence();

        _tgl.interactable = false;
        if (_ShowLineSeparator)
        {
            _lineSP.gameObject.SetActive(true);
            mySequence
                .Append(_lineSP.DOFade(0.0f,0.0f))
                .Append(_lineSP.DOFade(1.0f, 0.5f))
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    _tgl.interactable = true;
                });
        }
        else
        {
            _lineSP.gameObject.SetActive(true);
            mySequence
                .Append(_lineSP.DOFade(1.0f, 0.0f))
                .Append(_lineSP.DOFade(0.0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _lineSP.gameObject.SetActive(false);
                    _tgl.interactable = true;
                });
        }

        OnChangeShowLineSeparator.Invoke(show);
    }
    public static bool GetShowLineSeparator()
    {
        return _ShowLineSeparator;
    }

    private void UpdateUIFontColor()
    {
        // 此时应该改变界面上所有字体颜色
        Text[] allTxt = _settingPanel.GetComponentsInChildren<Text>(true);
        Sequence mySequence = DOTween.Sequence();

        Color c = Define.GetUIFontColorByBgColor(_BG.color, Define.eFontAlphaType.FONT_ALPHA_128);

        foreach (var txt in allTxt)
        {
            if (txt.name == "Footer")
            {
                mySequence.Join(txt.DOColor(new Color(c.r, c.g, c.b, 50 / 255.0f), 0.8f));
            }
            else
            {
                // 使用黑字
                if (!txt.name.Contains("Hide"))
                {
                    mySequence.Join(txt.DOColor(c, 0.5f));
                }
            }
        }

        foreach (var img in _needChangeWhithSJBColor)
        {
            mySequence.Join(img.DOColor(c, 0.8f));
        }

        if (Define.GetIfUIFontBlack(_BG.color))
        {
            mySequence.Join(_lineSP.DOColor(Define.LIGHTBG_SP_COLOR, 0.8f));
        }
        else
        {
            mySequence.Join(_lineSP.DOColor(Define.DARKBG_SP_COLOR, 0.8f));
        }

        mySequence.SetEase(Ease.OutSine);
    }

    public Text _speedLevelText;
    public Slider _speedLevelSlider;
    public void DoSpeedLevelValueChange(bool isInit)
    {
        //执行相关动画
        if (_SpeedLevel == SpeedLevel.SPEED_LEVEL_8)
        {
            _speedLevelText.DOText("xX(直接)", 0.5f);
            _speedLevelText.fontSize = 32;
        }
        else
        {
            _speedLevelText.DOText("x" + ((int)_SpeedLevel + 1), 0.5f);
            _speedLevelText.fontSize = 36;
        }

        if(isInit){
            _speedLevelSlider.value = (int)_SpeedLevel;
        }

        //延时执行
        Invoke("DoSpeedLevelAni", 0.5f);
    }
    public LeanFingerSwipe _swpieToBack;
    public void OnSpeedLevelValueChange(float v){
        _swpieToBack.enabled = false;
        CancelInvoke("EnableSwpie");
        Invoke("EnableSwpie", 3.0f);
        //没变化
        if ((int)_SpeedLevel == (int)v) return;

        SetSpeedLevel((SpeedLevel)(int)v);

        DoSpeedLevelValueChange(false);
    }
    public void EnableSwpie(){
        _swpieToBack.enabled = true;
    }
    public Text[] _speedLevelExText;
    public void DoSpeedLevelAni(){
        DOTween.Kill("DoSpeedLevelAni", true);
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("DoSpeedLevelAni");

        float speed = (_SpeedLevel == SpeedLevel.SPEED_LEVEL_8 ? 0.0f : 2.0f / (Mathf.Pow(2, (int)_SpeedLevel)));


        if (_SpeedLevel == SpeedLevel.SPEED_LEVEL_8)
        {
            mySequence.AppendInterval(0.1f);
            for (int i = 0; i < _speedLevelExText.Length; i++)
            {
                mySequence.Join(_speedLevelExText[i].DOFade(0.5f, speed));
            }
        }
        else
        {
            mySequence.AppendInterval(0.1f);
            for (int i = 0; i < _speedLevelExText.Length; i++)
            {
                mySequence.Join(_speedLevelExText[i].DOFade(0.0f, 0.8f));
            }

            for (int i = 0; i < _speedLevelExText.Length; i++)
            {
                mySequence
                   // .AppendInterval(i * speed)
                    .Append(_speedLevelExText[i].DOFade(0.5f, speed));
            }

            mySequence.AppendInterval(0.5f);
            mySequence.SetLoops(-1, LoopType.Restart);
        }
    }

    public void SetSpeedLevel(SpeedLevel speedLevel)
    {
        _SpeedLevel = speedLevel;
        setPlayerPrefs("" + SETTING_KEY.SPEED_LEVEL, (int)_SpeedLevel);
    }
    public static SpeedLevel GetSpeedLevel()
    {
        return _SpeedLevel;
    }

    [System.Serializable] public class OnChangeAlignmentTypeEvent : UnityEvent<AlignmentType> { }
    public OnChangeAlignmentTypeEvent OnChangeAlignmentType;
    public Button VL;
    public Button VR;
    public Button HL;
    public Button HR;

    public void OnAlignmentBtnClick(string type){

        AlignmentType prev = _AlignmentType;
        // 可实现按钮文字的排版动画
        if (type == ""+AlignmentType.LEFT_VERTICAL)
        {
            SetAlignmentType(AlignmentType.LEFT_VERTICAL);
            VL.interactable = false;
            VR.interactable = true;
            HL.interactable = true;
            HR.interactable = IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT);

        }
        else if (type == "" + AlignmentType.RIGHT_VERTICAL)
        {
            SetAlignmentType(AlignmentType.RIGHT_VERTICAL);
            VL.interactable = true;
            VR.interactable = false;
            HL.interactable = true;
            HR.interactable = IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT);
        }
        else if (type == "" + AlignmentType.LEFT_HORIZONTAL)
        {
            SetAlignmentType(AlignmentType.LEFT_HORIZONTAL);
            VL.interactable = true;
            VR.interactable = true;
            HL.interactable = false;
            HR.interactable = IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT);
        }
        else if (type == "" + AlignmentType.RIGHT_HORIZONTAL)
        {
            SetAlignmentType(AlignmentType.RIGHT_HORIZONTAL);
            VL.interactable = true;
            VR.interactable = true;
            HL.interactable = true;
            HR.interactable = false;
        }

        //先把前一个close掉
        EasyTween sImg = _alignmentImg[(int)prev * 3 + 2].GetComponent<EasyTween>();
        sImg.OpenCloseObjectAnimation();

        ChangeAligmentImgColor(false);
    }

    public Text[] _AllAlignText;
    public void DoAlignmentAni(){

        /*
        VL:0213 VR:1302,HL:0123,HR:1032
        */

        DOTween.Kill("DoAlignmentAni", true);
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("DoAlignmentAni");

        mySequence.AppendInterval(0.01f);
        for (int i = 0; i < _AllAlignText.Length;i++)
        {
            mySequence.Join(_AllAlignText[i].DOFade(0.0f, 0.5f));
        }

        mySequence.AppendInterval(0.01f);
        for (int i = 0; i < _AllAlignText.Length; i ++)
        {
            if(i % 4 == 0){
                mySequence.AppendInterval(1.0f);
            }
            mySequence.Join(_AllAlignText[i].DOFade(0.5f, 1.0f));
        }

        mySequence.SetLoops(-1, LoopType.Restart);
    }

    public void SetAlignmentType(AlignmentType type)
    {
        _AlignmentType = type;
        setPlayerPrefs("" + SETTING_KEY.ALIGNMENT_TYPE, (int)_AlignmentType);

        OnChangeAlignmentType.Invoke(type);
    }
    public static AlignmentType GetAlignmentType()
    {
        return _AlignmentType;
    }

    public static bool CheckSettingCnt(SETTING_KEY k,int maxCnt,bool needAdd = true)
    {
        bool ret = false;
        int cnt = getPlayerPrefs("" + k, 0);
        if (cnt < maxCnt)
        {
            ret = true;
        }

        if (needAdd)
        {
            setPlayerPrefs("" + k, cnt + 1);
        }

        return ret;
    }

    [System.Serializable] public class OnChangeShowSJLineEvent : UnityEvent<ShowSJLine> { }
    public OnChangeShowSJLineEvent OnChangeShowSJLine;
    public Text _showLineText;
    public Slider _showLineSlider;
    public void DoShowSJLineValueChange(bool isInit){

        int ln = 2;
        int vv = 0;
        switch (_ShowSJLine)
        {
            case ShowSJLine.LINE_1:
                ln = 1;
                vv = 0;
                break;
            case ShowSJLine.LINE_2:
                ln = 2;
                vv = 1;
                break;
            case ShowSJLine.LINE_4:
                ln = 4;
                vv = 2;
                break;
            case ShowSJLine.LINE_ALL:
                ln = 5;
                vv = 3;
                break;
        }


        //执行相关动画
        if (_ShowSJLine == ShowSJLine.LINE_ALL)
        {
            _showLineText.DOText("xX" + "(全文)", 0.5f);
            _showLineText.fontSize = 32;
        }
        else
        {
            _showLineText.DOText("x" + ln, 0.5f);
            _showLineText.fontSize = 36;
        }

        if(isInit){
            _showLineSlider.value = vv;
        }

        //延时执行
        Invoke("DoShowLineAni", 0.5f);
    }
    public void OnShowSJLineValueChange(float v)
    {
        _swpieToBack.enabled = false;
        CancelInvoke("EnableSwpie");
        Invoke("EnableSwpie", 3.0f);

        int vv = (int)v;

        ShowSJLine tmp = ShowSJLine.LINE_2;
        if(vv == 0){
            tmp =  ShowSJLine.LINE_1;
        }else if(vv == 1){
            tmp = ShowSJLine.LINE_2;
        }else if (vv == 2)
        {
            tmp = ShowSJLine.LINE_4;
        }else if(vv == 3){
            tmp = ShowSJLine.LINE_ALL;
        }

        //没变化
        if (_ShowSJLine == tmp) return;

        SetShowSJLine(tmp);

        DoShowSJLineValueChange(false);
    }

    public Text[] _showLineExText;
    public void DoShowLineAni()
    {
        DOTween.Kill("DoShowLineAni", true);
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("DoShowLineAni");

        float speed = (_SpeedLevel == SpeedLevel.SPEED_LEVEL_8 ? 0.0f : 2.0f / ((int)_SpeedLevel + 1));

        int ln = 2;
        switch(_ShowSJLine){
            case ShowSJLine.LINE_1:
                ln = 1;
                break;
            case ShowSJLine.LINE_2:
                ln = 2;
                break;
            case ShowSJLine.LINE_4:
                ln = 4;
                break;
            case ShowSJLine.LINE_ALL:
                ln = 5;
                break;
        }

        for (int i = 0; i < _showLineExText.Length; i++){
            if (i > ln - 1)
            {
                _showLineExText[i].gameObject.SetActive(false);
            }else{
                _showLineExText[i].gameObject.SetActive(true);
            }
            _showLineExText[i].color = new Color(_showLineExText[i].color.r,
                                                _showLineExText[i].color.g,
                                                _showLineExText[i].color.b
                                                ,0.0f);
        }

        for (int i = 0; i < _showLineExText.Length;i++){
            if(_showLineExText[i].gameObject.activeSelf){
                mySequence
                    .AppendInterval(Mathf.Sqrt(i) * 0.2f)
                    .Join(_showLineExText[i].DOFade(0.5f, 0.5f));
            }
        }
    }

    public void SetShowSJLine(ShowSJLine showSJLine)
    {
        _ShowSJLine = showSJLine;
        setPlayerPrefs("" + SETTING_KEY.SHOW_SJ_LINE, (int)_ShowSJLine);
        OnChangeShowSJLine.Invoke(showSJLine);
    }
    public static ShowSJLine GetShowSJLine()
    {
        return _ShowSJLine;
    }

    //需要知道当前的colorid
    private int _CurrentBGColorID = Define.DEFAULT_START_COLORID;
    public Image _DefaultColor;
    public Image _PrevColor;
    public Image _CurrentColor;
    public Text _PrevColorText;
    public Text _DefaultCurrentColorText;
    public Button _UseBgColorBtn;

    public void SetCurrentBGColorID(int cid){
        _CurrentBGColorID = cid;

        //设置背景色的时候就应该改变其中一个按钮的颜色
        List<string> cinfo  = PickColor.GetColorByID(GetStartColorID());//雪白
        List<string> dcinfo = PickColor.GetColorByID(Define.DEFAULT_START_COLORID);//雪白
        List<string> ctinfo = PickColor.GetColorByID(_CurrentBGColorID);//雪白


        Color c1 = new Color(int.Parse(cinfo[3]) / 255.0f,
                          int.Parse(cinfo[4]) / 255.0f,
                          int.Parse(cinfo[5]) / 255.0f);

        Color c2 = new Color(int.Parse(dcinfo[3]) / 255.0f,
                          int.Parse(dcinfo[4]) / 255.0f,
                          int.Parse(dcinfo[5]) / 255.0f);

        Color c3 = new Color(int.Parse(ctinfo[3]) / 255.0f,
                          int.Parse(ctinfo[4]) / 255.0f,
                          int.Parse(ctinfo[5]) / 255.0f);


        _PrevColorText.text = "已设定："+cinfo[0];
        _PrevColorText.color = Define.GetUIFontColorByBgColor(c1,Define.eFontAlphaType.FONT_ALPHA_128);

        _PrevColor.color = Define.GetFixColor(c1 * 0.9f);
        _DefaultColor.color = Define.GetFixColor(c2 * 0.9f);
        _CurrentColor.color = Define.GetFixColor(c3 * 0.9f);

        if(_CurrentBGColorID != _StartColorID)
        {
            _DefaultCurrentColorText.text = "使用当前：" + ctinfo[0];
            _DefaultCurrentColorText.color = Define.GetUIFontColorByBgColor(c3, Define.eFontAlphaType.FONT_ALPHA_128);
        }
        else
        {
            _DefaultCurrentColorText.text = "使用默认：" + dcinfo[0];
            _DefaultCurrentColorText.color = Define.GetUIFontColorByBgColor(c2, Define.eFontAlphaType.FONT_ALPHA_128);
        }
    }
    public void UseStartColor()
    {
        if (_CurrentBGColorID >= HZManager.GetInstance().GetColorCnt())
        {
            EasyTween etDft = _DefaultColor.GetComponent<EasyTween>();
            EasyTween etCrt = _CurrentColor.GetComponent<EasyTween>();
            if (etDft.animationParts.ObjectState == UITween.AnimationParts.State.OPEN)
            {
                etDft.OpenCloseObjectAnimation();//关闭默认
            }

            if (etCrt.animationParts.ObjectState == UITween.AnimationParts.State.CLOSE)
            {
                etCrt.OpenCloseObjectAnimation();//显示当前
            }

            ShowToast("色库颜色不能用作启动色，只有内置颜色才可以哦～",4f);
            return;
        }

        if(_StartColorID == Define.DEFAULT_START_COLORID)
        {
            _StartColorID = _CurrentBGColorID;//设置为当前的cid
        }
        else
        {
            if(_CurrentBGColorID == _StartColorID)
            {
                _StartColorID = Define.DEFAULT_START_COLORID;
            }
            else
            {
                _StartColorID = _CurrentBGColorID;
            }
        }

        SetStartColorID(_StartColorID);
    }
    //设置默认主题色
    private  void SetStartColorID(int cid)
    {
        _StartColorID = cid;
        setPlayerPrefs("" + SETTING_KEY.START_COLOR, _StartColorID);

        List<string> cinfo = PickColor.GetColorByID(_StartColorID);//雪白
        List<string> dcinfo = PickColor.GetColorByID(Define.DEFAULT_START_COLORID);//雪白
        List<string> ctinfo = PickColor.GetColorByID(_CurrentBGColorID);//雪白


        Color c1 = new Color(int.Parse(cinfo[3]) / 255.0f,
                             int.Parse(cinfo[4]) / 255.0f,
                             int.Parse(cinfo[5]) / 255.0f);

        Color c2 = new Color(int.Parse(dcinfo[3]) / 255.0f,
                          int.Parse(dcinfo[4]) / 255.0f,
                          int.Parse(dcinfo[5]) / 255.0f);

        Color c3 = new Color(int.Parse(ctinfo[3]) / 255.0f,
                          int.Parse(ctinfo[4]) / 255.0f,
                          int.Parse(ctinfo[5]) / 255.0f);


        _PrevColorText.text = "已设定："+ cinfo[0];
        _PrevColorText.color = Define.GetUIFontColorByBgColor(c1, Define.eFontAlphaType.FONT_ALPHA_128);
        _PrevColor.color = Define.GetFixColor(c1 * 0.9f);

        if (_DefaultCurrentColorText.text.Contains("当前"))
        {
            _DefaultCurrentColorText.text = "使用默认：" + dcinfo[0];
            _DefaultCurrentColorText.color = Define.GetUIFontColorByBgColor(c2, Define.eFontAlphaType.FONT_ALPHA_128);
        }
        else
        {
            _DefaultCurrentColorText.text = "使用当前：" + ctinfo[0];
            _DefaultCurrentColorText.color = Define.GetUIFontColorByBgColor(c3, Define.eFontAlphaType.FONT_ALPHA_128);
        }
    }
    public static int GetStartColorID()
    {
        return _StartColorID;
    }

    public Button _donateBtn;
    public Button _zanBtn;
    private Vector3 _oriZanBtnPosx;
    public void DoZanDonateAni(){

        DOTween.Kill("DoZanDonateAni", true);
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("DoZanDonateAni");
        _zanBtn.transform.localPosition = _oriZanBtnPosx;

        mySequence
            .AppendInterval(HZManager.GetInstance().GenerateRandomInt(3,6))
            .Append(_zanBtn.transform.DOLocalMoveX(_donateBtn.transform.localPosition.x - _donateBtn.GetComponent<RectTransform>().sizeDelta.x,2.0f))
            .SetEase(Ease.InSine)
            .Append(_zanBtn.transform.DOLocalRotate(new Vector3(0.0f,0.0f,-45.0f),0.5f))
            .Join(_zanBtn.transform.DOScale(0.8f, 0.5f))
            .Join(_donateBtn.transform.DOScale(1.2f,0.5f))
            .SetEase(Ease.OutBounce)
            .SetLoops(-1,LoopType.Yoyo)
            .OnComplete(()=>{

        });
    }

    public void OnSendEmailClick(){
        EmailManager.Send("ia_fun@163.com", "建议反馈", "任何建议、反馈、问题、合作，欢迎联系我们～");
    }

    public void OnOpenWeiboClick()
    {
        //
        Application.OpenURL("http://weibo.com/u/3845616134");
    }

    public void OnClickTipix(){

        openStore("717545399");// Pass in your AppID "xxxxxxxxx"
    }

    public void OnClickMapix()
    {
        openStore("941023990");// Pass in your AppID "xxxxxxxxx"
    }

    public void OnClickTipixel()
    {
        openStore("1325480389");// Pass in your AppID "xxxxxxxxx"
    }

    public void OnClickHanZi()
    {
        openStore("1450896243");// Pass in your AppID "xxxxxxxxx"
    }

    public void OnClickCYM()
    {
        openStore("1469655291");// Pass in your AppID "xxxxxxxxx"
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = _BG.color;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }

    public void OnClickDonate()
    {
        string url = "alipayqr://platformapi/startapp?saId=10000007&clientVersion=3.7.0.0718&qrcode=HTTPS://QR.ALIPAY.COM/FKX07597YLUWKETRZIV510";

        if (ShareRECIOS.canOpenUrl(url))
        {
            Application.OpenURL(url);
        }
        else{
            ShowToast("感谢点赞支持！");
        }

    }

    public void OnClickZan()
    {
        openStore("1449364884",true);
    }
    private void openStore(string appId,bool forReview = false){
        var desc = new MarketingDesc();

        desc.Editor_URL = "";// Any full URL
        desc.Win8_PackageFamilyName = "";// This is the "Package family name" that can be found in your "Package.appxmanifest".
        desc.WP8_AppID = "";// This is the "App ID" that can be found in your "Package.appxmanifest" under "Package Name".
                            // NOTE: For Windows Phone 8.0 you don't need to set anything...

        desc.iOS_AppID = appId;// Pass in your AppID "xxxxxxxxx"
        desc.BB10_AppID = "";// You pass in your AppID "xxxxxxxx".

        desc.Android_MarketingStore = MarketingStores.GooglePlay;
        desc.Android_GooglePlay_BundleID = "";// Pass in your bundle ID "com.Company.AppName"
        desc.Android_Amazon_BundleID = "";// Pass in your bundle ID "com.Company.AppName"
        desc.Android_Samsung_BundleID = "";// Pass in your bundle ID "com.Company.AppName"

        if (forReview)
        {
            MarketingManager.OpenStoreForReview(desc);
        }
        else
        {
            MarketingManager.OpenStore(desc);
        }
    }

    public IAP _iap;
    public GameObject _hrLock;
    public GameObject _showLineLock;
    public GameObject _showLineSliderHandle;
    public GameObject _restoreBtn;

    public void InitBuy(){
        //hr
        if (IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT)){
            _hrLock.SetActive(false);
            HR.interactable = true;
            HR.GetComponent<Image>().enabled = true;
        }
        else{
            _hrLock.SetActive(true);
            HR.interactable = false;
            HR.GetComponent<Image>().enabled = false;
        }

        //showline
        if (IAP.getHasBuy(IAP.IAP_SHOWLINE))
        {
            _showLineLock.SetActive(false);
            _showLineSlider.interactable = true;
            _showLineSliderHandle.GetComponent<Image>().enabled = true;
        }
        else
        {
            _showLineLock.SetActive(true);
            _showLineSlider.interactable = false;
            _showLineSliderHandle.GetComponent<Image>().enabled = false;
        }

        if(IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT) 
           && IAP.getHasBuy(IAP.IAP_SHOWLINE)
           && IAP.getHasBuy(IAP.IAP_DAAN)
           && IAP.getHasBuy(IAP.IAP_ZISHI)
           && IAP.getHasBuy(IAP.IAP_PEITU)
           && IAP.getHasBuy(IAP.IAP_LVJING))
        {
            if (_restoreBtn.activeSelf)
            {
                _donateBtn.transform.localPosition = _restoreBtn.transform.localPosition;
                _zanBtn.transform.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                _restoreBtn.SetActive(false);
                Invoke("DoZanDonateAni", 0.1f);
            }
        }
        else{
            _restoreBtn.SetActive(true);
        }
    }

    public void OnBuyBtn(string appID)
    {
        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            _iap.onBuyClick(IAP.GetAppIDByName(appID));
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍后...");
        }

    }

    // 恢复购买
    public void OnRestoreBtn(){
        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            _iap.onRestoreClick();
        }
        else
        {
            ShowToast("恢复购买正在处理中，请稍后...");
        }
    }

    //-----------------------内购-------------------------------
    private bool _ProcessingPurchase = false;
    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        _ProcessingPurchase = false;
        if (ret)
        {
            // 购买成功，根据inAppID，刷新界面
            if (!string.IsNullOrEmpty(receipt))
            {
                //说明是真正的购买，其他可能是正在购买或者已经购买过了
            }

            UnLockBuy(inAppID);

            OnReportEvent(ret, inAppID, EventReport.BuyType.BuySuccess);
        }
        else
        {
            //简单提示，购买失败
            ShowToast("购买失败，请稍后再试：(");
            OnReportEvent(ret, inAppID, EventReport.BuyType.BuyFail);
        }
    }

    public void OnRestoreCallback(bool ret, string inAppID)
    {
        _ProcessingPurchase = false;

        if (ret)
        {
            UnLockBuy(inAppID);
        }
        else
        {
            //MessageBoxManager.Show("", "恢复购买失败，请确认是否购买过？");
        }

        OnReportEvent(ret, inAppID, EventReport.BuyType.BuyRestore);
    }

    public void OnGetInAppPriceInfoCallback(bool ret, string info)
    {
        if (ret)
        {
            // 
        }
        else
        {
            //
        }
    }

    private void UnLockBuy(string inAppID){
        if (inAppID == IAP.IAP_HORIZONTALRIGHT)
        {
            //如果lock不可见，不必执行动画
            if (_hrLock.activeSelf){
                DoUnLockAni(_hrLock,()=>{
                    //hr
                    HR.interactable = true;
                    HR.GetComponent<Image>().enabled = true;
                    _hrLock.SetActive(false);
                });
            }
        }
        else if (inAppID == IAP.IAP_SHOWLINE)
        {

            //如果lock不可见，不必执行动画
            if (_showLineLock.activeSelf)
            {
                DoUnLockAni(_showLineLock,()=>{
                    _showLineLock.SetActive(false);
                    _showLineSlider.interactable = true;
                    _showLineSliderHandle.GetComponent<Image>().enabled = true;
                });
            }
        }

        if (IAP.getHasBuy(IAP.IAP_HORIZONTALRIGHT)
            && IAP.getHasBuy(IAP.IAP_SHOWLINE)
            && IAP.getHasBuy(IAP.IAP_DAAN)
            && IAP.getHasBuy(IAP.IAP_ZISHI)
            && IAP.getHasBuy(IAP.IAP_PEITU)
            && IAP.getHasBuy(IAP.IAP_LVJING))
        {
            //需要更新点赞按钮的原始位置
            if (_restoreBtn.activeSelf)
            {
                _donateBtn.transform.localPosition = _restoreBtn.transform.localPosition;
                _zanBtn.transform.localRotation = new Quaternion(0.0f,0.0f,0.0f,0.0f);
                _restoreBtn.SetActive(false);
                Invoke("DoZanDonateAni", 0.1f);
            }
        }
        else
        {
            _restoreBtn.SetActive(true);
        }
    }

    private void DoUnLockAni(GameObject lockObj,Action cb = null){
        Image bg = lockObj.GetComponent<Image>();
        Image[] ll = lockObj.GetComponentsInChildren<Image>(true);

        Image lockImg = ll[0];
        Image unlockImg = ll[1];
        foreach(var img in ll){
            if(img.name == "LockImg"){
                lockImg = img;
            }
            if(img.name == "UnLockImg"){
                unlockImg = img;
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
            .Append(unlockImg.transform.DOShakeRotation(1.0f,45.0f))
            .Append(unlockImg.DOFade(0.0f, 0.5f))
            .Join(bg.DOFade(0.0f, 1.0f))
            .SetEase(Ease.InSine)
            .OnComplete(()=>{

            if(cb != null){
                    cb();
            }
        });
    }


    [System.Serializable] public class OnEventReport : UnityEvent<string> { }
    public OnEventReport ReportEvent;
    public void OnReportEvent(bool success,string inAppID,
                            EventReport.BuyType buyType)
    {

        EventReport.EventType eventType = EventReport.EventType.NoneType;
        if (inAppID == IAP.IAP_HORIZONTALRIGHT)
        {
            eventType = EventReport.EventType.HorizontalRightBuyClick;
        }
        else if (inAppID == IAP.IAP_SHOWLINE)
        {
            eventType = EventReport.EventType.ShowLineBuyClick;
        }

        ReportEvent.Invoke(buyType +"_"+ eventType+"_"+ success);
    }
    //----------------------数据存取--------------------------------------------
    public static void setPlayerPrefs(string key,string value){
		PlayerPrefs.SetString (key,value);
	}

	public static string getPlayerPrefs(string key,string dft){
		return PlayerPrefs.GetString (key, dft);
	}

	public static void setPlayerPrefs(string key,int value){
		PlayerPrefs.SetInt (key,value);
	}

	public static int getPlayerPrefs(string key,int dft){
		return PlayerPrefs.GetInt(key, dft);
	}

	public static void setPlayerPrefs(string key,float value){
		PlayerPrefs.SetFloat (key,value);
	}

	public static float getPlayerPrefs(string key,float dft){
		return PlayerPrefs.GetFloat (key, dft);
	}

	public static void delPlayerPrefs(string key){
		PlayerPrefs.DeleteKey(key);
	}

	public static void delAllPlayerPrefs(){
		PlayerPrefs.DeleteAll();
	}
}
