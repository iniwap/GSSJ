using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using Reign;

public class Study : MonoBehaviour
{
    public void Start()
    {
        DoKTTagAni();
        ChangeZTTimeType( ZTTimeType.ZT_10S);
    }

    //诗词学习类型
    public enum StudyType{
        PANDUAN,//判断题 - 判断诗词正确与否 一定概率替换单字/多字/单句--采用限时作答形式
        XUANZE,//选择题 - 普通选择题，可单字，也可单句//采用限时作答形式
        TIANKONG,//填空题 - 没有提示，需要自行填空//普通填空题
        PAIXU,//排序题 - 打乱的诗词顺序，需要正确排序//采用依次掉落限时完成模式
        XUANTIAN,//选填题 - 有一定数量的提示字，正确答案在其中//采用混排挑选模式-也可依次随机显示，挑选
        END,
    }

    //出题类型，是扣字还是扣句
    public enum ZiOrJuType
    {
        ZI,
        JU,
    }

    //做题限时
    public enum ZTTimeType{
        ZT_10S = 10,
        ZT_20S = 20,
        ZT_30S = 30,
        ZT_60S = 60,
    }

    //诗词学习模式均为双句测试，即上，下句形。不考虑其他形式。
    //当然如果有扩展的需求，可以考虑增加整篇等形式，包括作者、朝代、题目等

    public StudyType _StudyType;
    public ZiOrJuType ZiOrJu { get; set; }

    public Image[] _KTTags;
    public Text _PlusScoreTangShi;
    public Text _PlusScoreSongCi;
    public Text _PlusScoreShiJing;

    public GameObject[] _studyPanel;
    public GameObject _begingInfo;

    public void OnEnable()
    {
        //
        ShowBegin(true);
    }

    //左右滑动
    //只适用于选填排序
    public void OnSwipe(Define.SWIPE_TYPE type){
        //只有排序模式才处理滑动操作
        if(_StudyType == StudyType.PAIXU){
            _studyPanel[(int)_StudyType].GetComponent<TiMu>().OnSwipe(type);
        }
    }

    private void DoKTTagAni(){
        for (int i = 0; i < (int)StudyType.END; i++)
        {
            if (i == (int)_StudyType)
            {
                _KTTags[i].gameObject.SetActive(true);
            }
            else
            {
                _KTTags[i].gameObject.SetActive(false);
            }

            _KTTags[i].color = _BgColor;
            //
            DOTween.Kill("KTTAG" + i, false);
            Sequence mySequence = DOTween.Sequence();
            mySequence.SetId("KTTAG" + i);

            mySequence
                .AppendInterval(0.5f)
                .Append(_KTTags[i].DOFade(0.2f, 1.2f))
                .AppendInterval(0.5f)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    [System.Serializable] public class OnChangeZTTimeTypeEvent : UnityEvent<ZTTimeType> { }
    public OnChangeZTTimeTypeEvent OnChangeZTTimeType;

    public void ChangeZTTimeType(ZTTimeType zzt)
    {
        OnChangeZTTimeType.Invoke(zzt);
    }

    [System.Serializable] public class OnShowDialogEvent : UnityEvent<Color,MaskTips.DialogParam> { }
    public OnShowDialogEvent OnShowDialog;
    public void OnStudyClick(string studyType){
        StudyType st = GetStudyType(studyType);
        if(st == _StudyType){
            return;
        }

        //不在游戏中
        if (_begingInfo.activeSelf)
        {
            StudyClick(st);
        }
        else
        {
            if (_studyPanel[(int)_StudyType].GetComponent<TiMu>().GetCanExist())
            {
                //先结束当前游戏
                _studyPanel[(int)_StudyType].GetComponent<TiMu>().ExistGame();
                StudyClick(st);
            }
            else
            {
                OnShowDialog.Invoke(_BgColor, MaskTips.GetDialogParam("确定放弃当前挑战吗？",
                    MaskTips.eDialogType.OK_CANCEL_BTN,
                    (MaskTips.eDialogBtnType type) =>
                {
                    if (type == MaskTips.eDialogBtnType.OK)
                    {
                        //先结束当前游戏
                        _studyPanel[(int)_StudyType].GetComponent<TiMu>().AbandonGame();
                        StudyClick(st);
                    }
                    else
                    {
                        //关闭弹窗
                    }
                }));
            }
        }
    }

    public void OnClickBack(){

        if(_StudyType == StudyType.XUANTIAN){
            //连句（选填）模式，铅笔不能显示此时
            _studyPanel[(int)_StudyType].GetComponent<XuanTian>().SetCanShowPencil(false);
        }

        if (_studyPanel[(int)_StudyType].GetComponent<TiMu>().GetCanExist())
        {
            //先结束当前游戏
            _studyPanel[(int)_StudyType].GetComponent<TiMu>().ExistGame();
            ShowBegin(true);
        }
        else
        {
            //显示到begin
            
            OnShowDialog.Invoke(_BgColor, MaskTips.GetDialogParam("确定放弃当前挑战吗？", MaskTips.eDialogType.OK_CANCEL_BTN,
                (MaskTips.eDialogBtnType type) =>
            {
                if (type == MaskTips.eDialogBtnType.OK)
                {
                    //先结束当前游戏
                    _studyPanel[(int)_StudyType].GetComponent<TiMu>().AbandonGame();
                    ShowBegin(true);
                }
                else
                {
                    //关闭弹窗
                }
            }));
        }
    }
    private void StudyClick(StudyType st)
    {
        _StudyType = st;

        for (int i = 0; i < (int)StudyType.END; i++)
        {
            if (i == (int)_StudyType)
            {
                _KTTags[i].gameObject.SetActive(true);
            }
            else
            {
                _KTTags[i].gameObject.SetActive(false);
            }
        }

        ShowBegin(false);

    }

    private string[] _BeginTitleTxt = { "判断题","选择题","填空题","排序题","连句题"};
    private string[] _BeginRuleTxt = { "<b>规则如下：</b>\n1，在规定时间内对诗句的正确性作出判断\n2，判断正确则进入下一题，错误则闯关失败\n3，每题基础分10分，耗时越短得分越高\n4，连续闯关以及不同题目范围，有额外加分\n5，答题超时仍有思考时间，再次超出视为失败\n6，计入【诗词判官】榜单，得分越高金榜越高",
        "<b>规则如下：</b>\n1，在规定时间内选择正确的答案补全诗句\n2，选择正确则进入下一题，错误则闯关失败\n3，每题基础分10分，耗时越短得分越高\n4，连续闯关以及不同题目范围，有额外加分\n5，答题超时仍有思考时间，再次超出视为失败\n6，计入【诗词甄选】榜单，得分越高金榜越高",
        "<b>规则如下：</b>\n1，在规定时间内完成诗词填空，或单句或单字\n2，填写正确则进入下一题，错误则闯关失败\n3，每题基础分10分，耗时越短得分越高\n4，连续闯关以及不同题目范围，有额外加分\n5，答题超时仍有思考时间，再次超出视为失败\n6，计入【诗词圣手】榜单，得分越高金榜越高",
        "<b>规则如下：</b>\n1，在字掉落时左/右滑动将其放置在正确位置\n2，顺序正确则进入下一题，错误则闯关失败\n3，每题基础分10*闯关/20，耗时越短得分越高\n4，连续闯关以及不同题目范围，有额外加分\n5，计入【诗词守护】榜单，得分越高金榜越高",
        "<b>规则如下：</b>\n1，划选顺序连通的字构成诗句，周围八个方向\n2，连句正确则进入下一题，可多次尝试\n3，每题基础分10*闯关/20，用时越少得分越高\n4，连续闯关以及不同题目范围，有额外加分\n5，难度随闯关数增加(干扰字、连通难度、提示)\n6，计入【诗词伯乐】榜单，得分越高金榜越高" };
    public Text _beginTitle;
    public Text _beginRuleText;

    public GameObject _DianGuBtn;
    public GameObject _ChangShiBtn;
    private void ShowBegin(bool ani){
        //切换挑战类型，首先显示begin的介绍界面
        _begingInfo.SetActive(true);

        Transform rule = _begingInfo.transform.Find("Rule");
        Transform ready = _begingInfo.transform.Find("Ready");

        for (int i = 0; i < (int)StudyType.END; i++)
        {
            _studyPanel[i].SetActive(false);
        }

        //只有选择题，才有典故和常识选项，其他不好处理，暂时不支持
        if(_StudyType == StudyType.XUANZE){
            //_DianGuBtn.SetActive(true);
            //_ChangShiBtn.SetActive(true);
            _DianGuBtn.SetActive(false);
            _ChangShiBtn.SetActive(false);
        }
        else{
            _DianGuBtn.SetActive(false);
            _ChangShiBtn.SetActive(false);
        }

        if(_StudyType != StudyType.PAIXU)
        {
            _PlusScoreTangShi.text = "+"+Define.PLUS_SCORE_TANGSHI;
            _PlusScoreSongCi.text = "+" + Define.PLUS_SCORE_SONGCI;
            _PlusScoreShiJing.text = "+" + Define.PLUS_SCORE_SHIJING;
        }
        else
        {
            _PlusScoreTangShi.text = "+" + Define.PX_PLUS_SCORE_TANGSHI;
            _PlusScoreSongCi.text = "+" + Define.PX_PLUS_SCORE_SONGCI;
            _PlusScoreShiJing.text = "+" + Define.PX_PLUS_SCORE_SHIJING;
        }

        //更新选中的题目范围
        ChangeFWColor();

        //界面切换，需要展示动画
        if (ani){
            OnShowMask.Invoke(true);
            //点击准备开始后，再显示相应的挑战界面
            _beginTitle.text = _BeginTitleTxt[(int)_StudyType];
            _beginRuleText.text = _BeginRuleTxt[(int)_StudyType];
            Sequence mySequence = DOTween.Sequence();

            //显示rule
            mySequence.Append(rule.transform.DOScale(0.0f,0.0f));
            mySequence.Join(ready.transform.DOScale(0.0f, 0.0f));

            mySequence.AppendCallback(()=>{
                rule.gameObject.SetActive(true);
                ready.gameObject.SetActive(true);
            });

            mySequence.AppendInterval(0.2f);
            mySequence.Append(rule.transform.DOScale(1.0f, 1.0f)).SetEase(Ease.OutSine);
          
            //显示reay
            mySequence.AppendInterval(0.2f);
            mySequence.Append(ready.transform.DOScale(1.0f, 1.0f)).SetEase(Ease.OutBounce)
                      .OnComplete(()=> OnShowMask.Invoke(false));

        }
        else
        {
            Sequence mySequence = DOTween.Sequence();
            mySequence
                .Append(_beginRuleText.DOFade(0.0f, 0.4f))
                .AppendCallback(() =>
                {
                    _beginRuleText.text = _BeginRuleTxt[(int)_StudyType];
                })
                .Append(_beginTitle.DOText(_BeginTitleTxt[(int)_StudyType], 0.6f))
                .Join(_beginRuleText.DOFade(1.0f, 0.6f));
        }
    }

    public GameObject _GO;
    [System.Serializable] public class OnShowMaskEvent : UnityEvent<bool> { }
    public OnShowMaskEvent OnShowMask;

    [System.Serializable] public class OnInGamingEvent : UnityEvent<bool> { }
    public OnInGamingEvent OnInGaming;

    public  void GameStart(){
        OnShowMask.Invoke(true);
        OnInGaming.Invoke(true);

        _GO.SetActive(true);
        Transform rule = _begingInfo.transform.Find("Rule");
        Transform ready = _begingInfo.transform.Find("Ready");
        rule.gameObject.SetActive(false);
        ready.gameObject.SetActive(false);

        //执行reay go动画
        //结束后开始考试
        Sequence mySequence = DOTween.Sequence();
        Image bg = _GO.GetComponentInChildren<Image>();
        bg.color = new Color(0,0,0,0);
        Text[] rg = _GO.GetComponentsInChildren<Text>();

        //fuck here 
        float offset = Screen.width;
        if(FitUI.GetIsPad()){
            offset += 200;
        }

        rg[0].transform.localPosition = new Vector3(-offset, 
                                                    rg[0].transform.localPosition.y, 
                                                    rg[0].transform.localPosition.z);
        rg[1].transform.localPosition = new Vector3(-offset,
                                            rg[1].transform.localPosition.y,
                                            rg[1].transform.localPosition.z);


        mySequence
            .Append(bg.DOFade(100 / 255.0f, 0.5f))
            .Join(rg[0].transform.DOLocalMoveX(0, 0.5f))
            .SetEase(Ease.InSine)
            .OnComplete(InitTiMu);//显示ready的时候生成题目

    }

    public void OnGameEnd(){
        DoKTTagAni();//由于结束时停掉了所有动画，此处再次启动
        ShowBegin(true);
        OnInGaming.Invoke(false);
    }


    [System.Serializable] public class OnMakeXJEvent : UnityEvent<int, string, HZManager.eShiCi> { }
    public OnMakeXJEvent OnMakeXinJian;
    public void OnMakeXJ(int sjID, string currentSJ, HZManager.eShiCi fw)
    {
        DoKTTagAni();//由于结束时停掉了所有动画，此处再次启动
        OnMakeXinJian.Invoke(sjID, currentSJ,fw);
    }

    private void InitTiMu(){
        _studyPanel[(int)_StudyType].GetComponent<TiMu>().InitTiMu(()=>{
            Image bg = _GO.GetComponentInChildren<Image>();
            Text[] rg = _GO.GetComponentsInChildren<Text>();
            //fuck here 
            float offset = Screen.width;
            if (FitUI.GetIsPad())
            {
                offset += 200;
            }

            Sequence mySequence = DOTween.Sequence();
            mySequence
                .AppendInterval(0.5f)//由于初始化题目会有一定耗时，这里只停留0.5s即可
                .Append(rg[0].transform.DOLocalMoveX(offset, 0.3f))
                .AppendInterval(0.1f)
                .Append(rg[1].transform.DOLocalMoveX(0, 0.4f))
                .AppendInterval(1.0f)
                .Append(rg[1].transform.DOLocalMoveX(offset, 0.2f))
                .Join(bg.DOFade(0.0f, 0.2f))
                .AppendInterval(0.1f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _GO.SetActive(false);
                    _begingInfo.SetActive(false);
                    ShowStudy();
                });
        });
    }

    private void ShowStudy(){
        for (int i = 0; i < (int)StudyType.END; i++)
        {
            if (i == (int)_StudyType)
            {
                _studyPanel[i].SetActive(true);
                _studyPanel[i].transform.localScale = new Vector3(0.0f,0.0f,1.0f);
                Sequence mySequence = DOTween.Sequence();
                //显示rule
                mySequence
                    .Append(_studyPanel[i].transform.DOScale(1.0f, 1.0f))
                    .SetEase(Ease.OutBounce)
                    .OnComplete(()=>{
                        //倒计时，应该在开始答题时启动
                        OnShowMask.Invoke(false);
                        //启动该游戏定时器
                        _studyPanel[(int)_StudyType].GetComponent<TiMu>().OnZuoTiTimer(true);
                    });
            }
            else
            {
                _studyPanel[i].SetActive(false);
            }
        }
    }
    private Color _BgColor;
    [System.Serializable] public class OnChangeBGColorEvent : UnityEvent<Color> { }
    public OnChangeBGColorEvent OnChangeBGColor;
    public void OnChangeColor(Color c,bool isChangeTheme){
        _BgColor = c;

        Color dc = Define.GetFixColor(_BgColor * 0.9f);
        Color lc = Define.GetFixColor(_BgColor * 1.1f);

        Button[] btns = gameObject.GetComponentsInChildren<Button>(true);
        foreach(var btn in btns){
            if(btn.name.Contains("BtnDark")){
                btn.image.color = dc;
            }else if (btn.name.Contains("BtnLight"))
            {
                btn.image.color = lc;
            }
        }

        Image[] titles = gameObject.GetComponentsInChildren<Image>(true);
        foreach (var t in titles)
        {
            if (t.name == "TitleBg")
            {
               // t.color = _BgColor*0.9f;
            }

            if (t.name == "TiMuBg")
            {
                //t.color = _BgColor * 1.1f;
            }

            if(t.name.Contains("Light")){
                t.color = lc;
            }
            else if(t.name.Contains("Dark")){
                t.color = dc;
            }
            else if (t.name.Contains("XTLine"))
            {
                t.color = _BgColor * 0.9f;
            }
        }

        Text[] txts = gameObject.GetComponentsInChildren<Text>(true);
        foreach (var txt in txts)
        {
            if (txt.name.Contains("TitleText"))
            {
                txt.DOColor(dc, 1.0f).SetEase(Ease.InSine);


                //DanAnTiText 如果是答案需要单独修改错误的字/句为高亮，其他为暗
                //richtext
            }

            if (txt.name.Contains("TiText"))
            {
                txt.DOColor(lc, 1.0f).SetEase(Ease.InSine);
            }
        }

        ChangeFWColor();

        OnChangeBGColor.Invoke(_BgColor);
    }

    public GameObject _tmFw;
    public void OnTiMuFanWeiClick(string fanwei){
        _studyPanel[(int)_StudyType].GetComponent<TiMu>().SetTiMuFW(GetTiMuFWType(fanwei));
        ChangeFWColor();

        //设置题目范围，即取题从对应的数据库选取
    }

    private void ChangeFWColor()
    {
        Color dc = Define.GetFixColor(_BgColor * 0.9f);
        Color lc = Define.GetFixColor(_BgColor * 1.1f);

        float off = 20 / 255.0f;
        if(Define.GetBrightness(lc) - Define.GetBrightness(dc)  < off){
            lc = new Color(lc.r + off, lc.g + off, lc.b + off, lc.a);
        }

        string fanwei = ""+_studyPanel[(int)_StudyType].GetComponent<TiMu>().GetTiMuFW();

        Image[] titles = _tmFw.GetComponentsInChildren<Image>(true);
        foreach (var t in titles)
        {
            if (t.name == "Icon")
            {
                t.color = dc;
            }
        }

        Text[] txts = _tmFw.GetComponentsInChildren<Text>(true);
        foreach (var txt in txts)
        {
            txt.color = dc;
        }

        Button[] btns = _tmFw.GetComponentsInChildren<Button>(true);
        foreach (var btn in btns)
        {
            if (btn.name == fanwei)
            {
                Image[] btitles = btn.GetComponentsInChildren<Image>(true);
                foreach (var t in btitles)
                {
                    if (t.name == "Icon"){
                        t.color = lc;
                    }
                }

                Text[] btxts = btn.GetComponentsInChildren<Text>(true);
                foreach (var txt in btxts)
                {
                    txt.color = lc;
                }

                break;
            }
        }
    }

    //排行榜相关处理
    [System.Serializable] public class ReportScoreEvent : UnityEvent<long, GameCenter.LeaderboardType> { }
    public ReportScoreEvent ReportScore;
    public  void OnReportScore(long score)
    {
        ReportScore.Invoke(score, GetLBType());
    }

    //连续闯关数 - 不分题目类型
    [System.Serializable] public class ReportAchievementEvent : UnityEvent<float, GameCenter.AchievementType> { }
    public ReportAchievementEvent ReportAchievement;
    public void OnReportAchievement(float percent, GameCenter.AchievementType type)
    {
        //
        ReportAchievement.Invoke(percent,type);
    }

    [System.Serializable] public class OnUpdateCKDALeftTimeEvent : UnityEvent<int> { }
    public OnUpdateCKDALeftTimeEvent OnUpdateCKDALeftTime;
    public void OnRequestAchievementsCallback(bool isReport,Achievement[] achievements)
    {

        //Debug.Log("OnRequestAchievementsCallback===>"+(isReport?1:0));
        //当前获取的成就个数
        int prev = GetCurrentAch();

        //Debug.Log("之前成就数:"+prev);

        foreach (var achievement in achievements)
        {
            Setting.setPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + achievement.ID, achievement.PercentComplete);
        }

        //上报之后获取的成就个数
        int affter = GetCurrentAch();

        //Debug.Log("之后成就数:" + affter);

        if (isReport)
        {
            if (affter != prev) //有新的成就获取
            {
                int cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
                cnt++;
                //由于是依次获取成就，所以此处只对当前次数+1即可
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, cnt);
                //仍然需要更新界面
                if(!_begingInfo.activeSelf)
                {
                    _studyPanel[(int)_StudyType].GetComponent<TiMu>().UpdateLeftDaanTime(cnt);
                }

                OnUpdateCKDALeftTime.Invoke(cnt);
               // Debug.Log("获取成就后次数:" + cnt);
            }
        }
        else
        {
            //启动登陆
            InitDaanTimes();
        }
    }

    public UnityEvent OnResetUserAchievementsProgress;
    //初始化可用【查看答案】次数
    private void InitDaanTimes()
    {
        Define.GetNetDateTime((string dt) =>
        {
            //没有获取到，极有可能没有联网，这种情况无法使用【查看答案】次数
            if (dt == string.Empty)
            {
                //Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);

                if (!_begingInfo.activeSelf)
                {
                    //_studyPanel[(int)_StudyType].GetComponent<TiMu>().UpdateLeftDaanTime(0);
                    
                }


                //同步时间错误，没有联网
                MessageBoxManager.Show("", "同步时间错误，请确认可访问网络并彻底退出重新打开，否则可能无法正常使用【查看答案】功能");

                return;
            }
            
            string rec = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT_RECORD, "");

            DateTime datetime = Convert.ToDateTime(dt);
            int day = datetime.Day;
            int month = datetime.Month;
            int year = datetime.Year;

            int cnt = 0;
            if (rec == "") //第一次打开
            {
                cnt = GetPerDayShowDaanCnt();
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, cnt);
                //设置时间
                rec = "" + year + "#" + month + "#" + day;

                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT_RECORD, rec);

                //Debug.Log("第一次打开："+cnt);

                //reset user achievement
                //OnResetUserAchievementsProgress.Invoke();
            }
            else
            {
                string[] ymd = rec.Split('#');
                int oyear = int.Parse(ymd[0]);
                int omonth = int.Parse(ymd[1]);
                int oday = int.Parse(ymd[2]);

                if (oyear == year && omonth == month && oday == day)
                {
                    //同一天，不处理使用次数
                    cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);

                    //Debug.Log("同一天打开：" + cnt);
                }
                else
                {
                    //不是同一天，重置使用次数
                    cnt = GetPerDayShowDaanCnt();
                    rec = "" + year + "#" + month + "#" + day;
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT,cnt );
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT_RECORD, rec);
                    //Debug.Log("不同一天打开：" + cnt);
                }
            }

            if (!_begingInfo.activeSelf)
            {
                _studyPanel[(int)_StudyType].GetComponent<TiMu>().UpdateLeftDaanTime(cnt);
            }

            OnUpdateCKDALeftTime.Invoke(cnt);
        });
    }

    private int GetPerDayShowDaanCnt()
    {
        int cnt = Define.FREEUSE_DAAN_PER_DAY;

        //额外增加对应成就的次数

        cnt += GetCurrentAch();

        return cnt;
    }
    private int GetCurrentAch()
    {
        int currentAch = 0;
        for (int i = (int)GameCenter.AchievementType.ZhuangYuan; i >= (int)GameCenter.AchievementType.TongSheng; i--)
        {
            if (Setting.getPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + "" + ((GameCenter.AchievementType)i), 0f) >= 100f)
            {
                currentAch = i + 1;
                break;
            }
        }

        return currentAch;
    }

    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        //原则上不在学习界面，可以不处理
        if (!gameObject.activeSelf || _begingInfo.activeSelf) 
            return;

        _studyPanel[(int)_StudyType].GetComponent<TiMu>().OnBuyCallback(ret, inAppID, receipt);
    }

    public void OnRestoreCallback(bool ret, string inAppID)
    {
        if (!gameObject.activeSelf || _begingInfo.activeSelf)
            return;

        _studyPanel[(int)_StudyType].GetComponent<TiMu>().OnRestoreCallback(ret,inAppID);
    }

    public void OnResetUserAchievementsCallback(bool succeeded)
    {
        //do nothing
    }
    //------------------一些接口-----------------------
    private StudyType GetStudyType(string studyType)
    {
        if(studyType == ""+ StudyType.PAIXU)
        {
            return StudyType.PAIXU;
        }
        else if (studyType == "" + StudyType.PANDUAN)
        {
            return StudyType.PANDUAN;
        }
        else if (studyType == "" + StudyType.TIANKONG)
        {
            return StudyType.TIANKONG;
        }
        else if (studyType == "" + StudyType.XUANTIAN)
        {
            return StudyType.XUANTIAN;
        }
        else if (studyType == "" + StudyType.XUANZE)
        {
            return StudyType.XUANZE;
        }

        return StudyType.PANDUAN;
    }

    private TiMu.eTiMuFanWei GetTiMuFWType(string fw)
    {
        if (fw == "" + TiMu.eTiMuFanWei.All)
        {
            return TiMu.eTiMuFanWei.All;
        }
        else if (fw == "" + TiMu.eTiMuFanWei.ChangShi)
        {
            return TiMu.eTiMuFanWei.ChangShi;
        }
        else if (fw == "" + TiMu.eTiMuFanWei.DianGu)
        {
            return TiMu.eTiMuFanWei.DianGu;
        }
        else if (fw == "" + TiMu.eTiMuFanWei.GuShi)
        {
            return TiMu.eTiMuFanWei.GuShi;
        }
        else if (fw == "" + TiMu.eTiMuFanWei.ShiJing)
        {
            return TiMu.eTiMuFanWei.ShiJing;
        }
        else if (fw == "" + TiMu.eTiMuFanWei.SongCi)
        {
            return TiMu.eTiMuFanWei.SongCi;
        }
        else if (fw == "" + TiMu.eTiMuFanWei.TangShi)
        {
            return TiMu.eTiMuFanWei.TangShi;
        }

        return TiMu.eTiMuFanWei.TangShi;
    }

    public GameCenter.LeaderboardType GetLBType(){
        if (_StudyType == StudyType.PANDUAN)
        {
            return GameCenter.LeaderboardType.PanDuan;
        }
        else if (_StudyType == StudyType.XUANZE)
        {
            return GameCenter.LeaderboardType.XuanZe;
        }
        else if (_StudyType == StudyType.TIANKONG)
        {
            return GameCenter.LeaderboardType.TianKong;
        }
        else if (_StudyType == StudyType.XUANTIAN)
        {
            return GameCenter.LeaderboardType.XuanTian;
        }
        else if (_StudyType == StudyType.PAIXU)
        {
            return GameCenter.LeaderboardType.PaiXu;
        }

        return GameCenter.LeaderboardType.PanDuan;
    }
}
