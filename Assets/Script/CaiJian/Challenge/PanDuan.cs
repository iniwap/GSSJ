using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using Reign;

public class PanDuan : TiMu
{
    public override void Start()
    {
    }
    public override void OnEnable()
    {
        DoZuoDaAni();

        _CanExist = false;
        CheckDaanBuyState();
    }
    //出题
    private Dictionary<string, int> _tsscHZ = new Dictionary<string, int>();//扣掉的字列表，用于加入游戏逻辑以及检测
    private bool _TiMuRight;
    private int DIF = 1;//替换字越多，难度越低
    //出题
    public override void InitTiMu(Action cb)
    {
        InitTiMu(true);
        cb();
    }

    public GameObject _failClockText;
    public void OnZuoDaClick(bool right){
        ShowMask(true);
        EnableShowDaanBtn(false);
        EnableZuoDaBtn(false);
        StopAllClock();

        RestTiMu();
        _TiMu.gameObject.SetActive(true);
        _failClockText.SetActive(false);

        string res = "";
        if (right == _TiMuRight){

            //回答正确 - 恭喜回答正确

            if(_TiMuRight){
                res = _TiMuText.text + "\n<size=72>回答正确! </size>出自：\n"+GetInfo(_TiMuFW,CurrentShiCiID);
            }
            else{
                res = _TiMuText.text + "\n<size=72>回答正确! </size>" + "原句：\n"+GetResult();
            }

            //统计分数
            SaveScore();
            DoDaTiAni(right,true, res);

        }else{
            //回答错误
            if (_TiMuRight)
            {
                res = _TiMuText.text + "\n<size=72>回答错误! </size>出自：\n" + GetInfo(_TiMuFW, CurrentShiCiID);
            }
            else
            {
                res = _TiMuText.text + "\n<size=72>回答错误! </size>" + "原句：\n" + GetResult();
            }

            DoDaTiAni(right, false,res);
        }
    }
    //超时失败
    protected override void TimeOutFail()
    {
        ShowMask(true);
        EnableZuoDaBtn(false);
        EnableShowDaanBtn(false);

        Invoke("Fail", 1.0f);
    }

    public override void AbandonGame(Action cb = null)
    {
        DoKillAllAni();
        StopAllClock();

        _CanExist = false;

        RestTiMu();
        _TiMu.gameObject.SetActive(true);
        _failClockText.SetActive(false);


        //这里可以稍微延时，防止闪屏错觉
        Invoke("GameEnd", 0.1f);
    }

    protected override void StopAllClock(){
        DoScoreClockAni(false,true);
        OnZuoTiTimer(false);
        DoFailClockAni(false,true);
    }
    //结算面板倒计时的时候可以返回
    public override void ExistGame()
    {
        Transform sc = _score.transform.Find("Viewport/Content");

        if (sc != null)
        {
            RectTransform scRt = sc.GetComponent<RectTransform>();
            sc.transform.localPosition = new Vector3(sc.localPosition.x + scRt.rect.width, 
                                                     sc.localPosition.y,sc.localPosition.z);
        }

        AbandonGame();
    }


    public void OnMakeXinJianClick()
    {
        Transform sc = _score.transform.Find("Viewport/Content");

        if (sc != null)
        {
            RectTransform scRt = sc.GetComponent<RectTransform>();
            sc.transform.localPosition = new Vector3(sc.localPosition.x + scRt.rect.width,
                                                     sc.localPosition.y, sc.localPosition.z);
        }


        DoKillAllAni();
        StopAllClock();

        _CanExist = false;

        RestTiMu();
        _TiMu.gameObject.SetActive(true);
        _failClockText.SetActive(false);


        OnMakeXinJian(CurrentShiCiID, CurrentSJ, GetShiCiType(_TiMuFW));
    }

    protected override string GetStudyName()
    {
        return gameObject.name;
    }
    //--------------内部接口----------------------------
    private int CurrentShiCiID = -1;//当前显示的诗词id
    private string CurrentSJ = "";//整句
    private void InitTiMu(bool first){

        //当前题目是正确还是错误
        _TiMuRight = HZManager.GetInstance().GenerateRandomInt(0, 2) == 1;

        CurrentSJ = "";
        CurrentShiCiID = -1;
        _tsscHZ.Clear();

        List<string> tsscData = HZManager.GetInstance().GetTSSC(GetShiCiType(_TiMuFW));
        CurrentShiCiID = int.Parse(tsscData[0]);
        //获取格式化后的诗词语句列表
        List<string> tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[4]);
        //随机选取其中一句诗词作为考察对象
        string tssc = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];
        CurrentSJ = tssc;

        if (!_TiMuRight)//题目错误，需要处理，否则不需要处理
        {
            //获取诗句的字数，用于等概率随机选中某些字
            int hzCnt = HZManager.GetInstance().GetHZCnt(tssc);
            //=====>替换错误字，形式为：1-x个字，或者整句
            //选中对应字数的汉字，该接口不会出现选中同一个汉字的情况
            List<int> ids = HZManager.GetInstance().GenerateRandomNoRptIntList(DIF, 0, hzCnt);

            int dff = 0;
            for (int i = 0; i < tssc.Length; i++)
            {
                //判断是否是标点符号
                if (!HZManager.GetInstance().CheckIsHZ(tssc[i]))
                {
                    continue;
                }

                foreach (int index in ids)
                {
                    if (dff == index)
                    {
                        _tsscHZ.Add("" + tssc[i], i);
  
                        Regex r = new Regex(""+tssc[i]);
                        string jz = GetRandomHalfShiJu();
                        tssc = r.Replace(tssc, ""+ GetHZ(tssc[i],jz), 1, index);
                    }
                }

                dff++;
            }
        }

        //设置题目
        SetTiMu(tssc);
        //设置正确答案
        SetRightDaAn();
        EnableZuoDaBtn(true);

        if (!first){
            OnZuoTiTimer(true);//开启
        }else{
            _TotalScore = 0;
            _ChuangGuan = 0;
        }

        //只要新生成题目，分数计时一定是0
        _ScoreTime = 0;

        EnableShowDaanBtn(true);
    }
    private void Fail(){

        Transform tm = gameObject.transform.Find("TiMu");//TiText
        if (tm != null)
        {
            GameObject tmclockobj = tm.Find("FailClockTiText").gameObject;
            tmclockobj.SetActive(false);
        }

        _TiMu.SetActive(true);
        _TiMuText.gameObject.SetActive(true);
        _TiMuText.DOFade(1.0f,0.0f);

        string res = "";
        if(_TiMuRight){
            res = _TiMuText.text + "\n<size=72>您已超时!</size>" + " 该题正确，出自：\n" + GetInfo(_TiMuFW, CurrentShiCiID);
        }
        else{
            res = _TiMuText.text + "\n<size=72>您已超时!</size>" + " 该题错误，答案：\n" + GetResult();
        }

        DoResultAni(false, res);
    }

    public Image _xzLight;//选项上升动画
    public Image _midDark;//到达过度动画
    public Image _smileLight;//正确与否动画
    public Image _resBg;//遮罩

    private void DoDaTiAni(bool select,bool right,string res){
        _xzLight.gameObject.SetActive(true);
        _resBg.gameObject.SetActive(true);
        _midDark.gameObject.SetActive(false);
        _smileLight.gameObject.SetActive(false);


        _xzLight.transform.position = new Vector3(_xzLight.transform.position.x,
                                                  _zuoDa.transform.position.y,
                                                 _xzLight.transform.position.z);
        if (select){
            _xzLight.sprite = Resources.Load("icon/check", typeof(Sprite)) as Sprite;
        }
        else{
            _xzLight.sprite = Resources.Load("icon/close", typeof(Sprite)) as Sprite;
        }

        if (right)
        {
            _smileLight.sprite = Resources.Load("icon/smile", typeof(Sprite)) as Sprite;
        }
        else
        {
            _smileLight.sprite = Resources.Load("icon/frown", typeof(Sprite)) as Sprite;
        }

        float speed = 0.8f;

        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(_xzLight.DOFade(0.0f, 0.0f))
            .Join(_xzLight.transform.DOScale(0.0f, 0.0f))
            .Join(_smileLight.DOFade(0.0f,0.0f))
            .Join(_smileLight.transform.DOScale(1.0f, 0.0f))
            .Join(_midDark.transform.DOScale(0.0f, 0.0f))
            .Join(_midDark.DOFade(50/255.0f, 0.0f))
            .Join(_resBg.DOFade(0.0f,0.0f))

            .Append(_xzLight.DOFade(1.0f, 0.2f))
            .Append(_xzLight.transform.DOMoveY(_TiMu.transform.position.y, speed/2))
            .Join(_xzLight.transform.DOScale(1.0f, speed/2))
            .Join(_resBg.DOFade(50 / 255.0f, speed/2))

            .AppendCallback(() => _midDark.gameObject.SetActive(true))
            .Append(_midDark.DOFade(0.0f, speed/2))
            .Join(_xzLight.DOFade(0.0f, 0.2f))
            .Join(_midDark.transform.DOScale(4.0f, speed/2))

            .AppendCallback(() => _smileLight.gameObject.SetActive(true))
            .Append(_smileLight.DOFade(1.0f, speed/2))
            .Append(_smileLight.transform.DOScale(2.0f, speed))
            .Join(_smileLight.DOFade(0.0f, speed))
            .Join(_resBg.DOFade(0.0f, speed))
            .OnComplete(() => DoResultAni(right, res));
            
    }

    private void DoResultAni(bool right,string res){
        if (right)
        {
            //执行动画
            _TiMuText.DOText(res, 2.0f).OnComplete(() => {
                //进入下一题 - nexttm
                _xzLight.gameObject.SetActive(false);
                _midDark.gameObject.SetActive(false);
                _smileLight.gameObject.SetActive(false);
                _resBg.gameObject.SetActive(false);
                DoScoreAni(true);
            });
        }
        else
        {
            _TiMuText.DOText(res, 2.0f).OnComplete(() => {
                //提供选择 返回首页 or 重新开始挑战
                _xzLight.gameObject.SetActive(false);
                _midDark.gameObject.SetActive(false);
                _smileLight.gameObject.SetActive(false);
                _resBg.gameObject.SetActive(false);
                DoScoreAni(false);
            });
        }
    }
    public GameObject _score;
    public Text _totalScoreText;
    public Text _currentScoreText;
    public Text _chuangGuanText;
    public Text _scoreClockText;
    public Image _scorePlusLight;
    private float _scorePlusLightPosY = -1;

    public Text _TSTiText;
    public Text _CSTiText;
    public Text _LXTiText;

    private void DoScoreAni(bool right){
        if (right){
            int cs = GetCurrentScore();
            _scoreClockText.text = "<b>5s</b>\n<size=42>下一题</size>";
            _totalScoreText.text = "" + (_TotalScore - cs);//to ts then to newts
            _currentScoreText.text = "" + cs;// to cs
            _chuangGuanText.text = "" + _ChuangGuan;

        }
        else{
            //最高得分
            //本次得分
            //连续
            int hs = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_SCORE + GetStudyName(), 0);
            int lx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_CG + GetStudyName(), 0);

            _scoreClockText.text = "<b>10s</b>\n<size=42>自动返回</size>";

            _totalScoreText.text = "" + hs+"/"+lx;
            _currentScoreText.text = "" + _TotalScore;
            _chuangGuanText.text = "" + _ChuangGuan;

            _TSTiText.text = "最高/闯关";
            _CSTiText.text = "本次得分";
            _LXTiText.text = "本次闯关";

            _TotalScore = 0;
            _ChuangGuan = 0;
        }


        _score.SetActive(true);

        Transform sc = _score.transform.Find("Viewport/Content");
        
        if(sc != null){
            RectTransform scRt = sc.GetComponent<RectTransform>();

            if(_scorePlusLightPosY <= -1){
                _scorePlusLightPosY = _scorePlusLight.transform.position.y;
            }

            if (right)
            {
                _scorePlusLight.transform.position = new Vector3(_scorePlusLight.transform.position.x,
                                                                _scorePlusLightPosY,
                                                                _scorePlusLight.transform.position.z);
                sc.transform.localPosition = new Vector3(scRt.rect.width, sc.transform.localPosition.y, sc.transform.localPosition.z);

                Sequence mySequence = DOTween.Sequence();
                mySequence.SetId("DoScoreClockAni"+GetStudyName());
                _scorePlusLight.gameObject.SetActive(true);
                mySequence
                    .Append(_scorePlusLight.DOFade(0.0f, 0.0f))
                    .Join(_scorePlusLight.transform.DOScale(1.0f, 0.0f))
                    .Append(sc.DOLocalMoveX(0, 1.0f))
                    .Join(_scorePlusLight.DOFade(1.0f, 1.0f))
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() =>
                    {
                        //+号动画，结束后执行分数+动画
                        //本题得分动画从0->实际分数
                        //总分从原来分数到+本题分数
                        //开启进入下一题倒计时，如果是错误，则返回开始界面
                        Sequence mySequence2 = DOTween.Sequence();
                            mySequence2
                            .Append(_scorePlusLight.transform.DOMoveY(_totalScoreText.transform.position.y, 0.5f))
                            .Append(_scorePlusLight.DOFade(0.0f, 0.4f))
                            .Join(_scorePlusLight.transform.DOScale(2.0f, 0.4f))
                            .SetEase(Ease.InSine)
                            .OnComplete(() =>
                            {
                                _totalScoreText.DOText("" + _TotalScore, 1.0f);
                            });

                        //执行倒计时
                        DoScoreClockAni(true);
                    });
            }else{

                //bug fix
                _scorePlusLight.gameObject.SetActive(false);

                Sequence mySequence = DOTween.Sequence();
                mySequence.SetId("DoScoreClockAni" + GetStudyName());
                mySequence
                    .Append(sc.DOLocalMoveX(0, 1.0f))
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() =>
                    {
                        //执行倒计时
                        DoScoreClockAni(true);

                        //此时可以直接返回
                        _CanExist = true;
                        _makeXJBtn.SetActive(true);
                        _makeXJBtn.transform.DOShakePosition(1.0f, 5);
                        ShowMask(false);//允许点击
                    });
            }
        }
    }
    private int _CurrentScoreTime = 0;
    private void DoScoreClockAni(bool start,bool needStopAni = false)
    {
        _CurrentScoreTime = 0;

        if (!start)
        {
            CancelInvoke("DoScoreClock");

            if (needStopAni)
            {
                //
                _TSTiText.text = "得分";
                _CSTiText.text = "本题";
                _LXTiText.text = "闯关";
                DOTween.Kill("DoScoreClockAni" + GetStudyName());
            }

            //制作信笺按钮只出现在回答失败时
            _makeXJBtn.SetActive(false);
            return;
        }

        InvokeRepeating("DoScoreClock", 0.1f, 1.0f);
    }

    private void DoScoreClock()
    {
        _CurrentScoreTime += 1;
        int fc = 0;
        
        bool right = false;
        if(_scoreClockText.text.Contains("下一题")){
            right = true;
            fc = (int)Study.ZTTimeType.ZT_10S / 2 - _CurrentScoreTime;
        }
        else{

            right = false;
            //如果是结束返回，则倒计时10s
            fc = (int)Study.ZTTimeType.ZT_10S - _CurrentScoreTime;
        }

        if (fc > 0)
        {
            if(right){
                _scoreClockText.text = "<b>" + fc + "s</b>\n<size=42>下一题</size>";
            }
            else{
                _scoreClockText.text = "<b>"+fc + "s</b>\n<size=42>自动返回</size>";
            }
        }
        else
        {
            if (right)
            {
                _scoreClockText.text = "进入\n下一题";
            }
            else
            {
                _scoreClockText.text = "返回\n开始界面";
            }

            DoScoreClockAni(false);
            //超时进入下一题或者返回开始界面

            Transform sc = _score.transform.Find("Viewport/Content");

            if (sc != null)
            {
                RectTransform scRt = sc.GetComponent<RectTransform>();

                Sequence mySequence = DOTween.Sequence();

                mySequence
                    .Append(sc.DOLocalMoveX(scRt.rect.width, 0.5f))
                    .Join(_TiMuText.DOFade(0.0f, 1.0f))
                    .SetEase(Ease.InSine)
                    .OnComplete(() =>
                    {
                        ShowMask(false);//允许点击

                        if (right)
                        {
                            InitTiMu(false);
                            _TiMuText.DOFade(1.0f, 1.0f);
                            _CanExist = false;
                        }
                        else
                        {
                            _TiMu.SetActive(true);
                            _TiMuText.gameObject.SetActive(true);
                            _TiMuText.DOFade(1.0f,0.0f);
                            gameObject.SetActive(false);

                            _TSTiText.text = "得分";
                            _CSTiText.text = "本题";
                            _LXTiText.text = "闯关";

                            Invoke("GameEnd",0.1f);
                        }
                    });

            }
        }
    }

    public GameObject _zuoDa;
    private void EnableZuoDaBtn(bool enable){
        Button[] btns = _zuoDa.GetComponentsInChildren<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = enable;
        }
    }
    private char GetHZ(char ori,string sj){
        char hz = '-';

        do
        {
            hz = sj[HZManager.GetInstance().GenerateRandomInt(0, sj.Length)];
        } while (hz == ori || !HZManager.GetInstance().CheckIsHZ(hz));

        return hz;
    }
    #region 判断
    public GameObject _da;
    public void OnDaAnClick()
    {
        if (_da.activeSelf) return;

        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if (currentLeftShowDaanCnt > 0){
            currentLeftShowDaanCnt--;
        }else{
            //答案次数已经用完了
            ShowToast("今天次数已用完，请明天再试(次日重置)");
            return;
        }

        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, currentLeftShowDaanCnt);

        _leftDaanTime.text = "" + currentLeftShowDaanCnt;

        if (currentLeftShowDaanCnt == 0)
        {
            _leftDaanTime.color = _BgDarkColor;
        }
        else
        {
            _leftDaanTime.color = _BgLightColor;
        }

        _da.SetActive(true);
    }
    public Text _TiMuText;
    public GameObject _TiMu;
    private void SetTiMu(string tm){
        _TiMuText.text = tm;
    }
    private void SetRightDaAn()
    {
        _da.SetActive(false);//需要隐藏

        Transform da = _da.transform.Find("DaAn/DanAnTiText");
        Text daText = da.gameObject.GetComponent<Text>();
        if (_TiMuRight)
        {
            daText.text = "<b>该题正确</b>，出自：\n" + GetInfo(_TiMuFW, CurrentShiCiID);
        }
        else
        {
            //错字需要高亮

            daText.text = GetResult();
        }
    }

    private string GetResult(){
        string daTxt = "";
        for (int i = 0; i < CurrentSJ.Length; i++)
        {
            bool isRight = true;
            foreach (var idx in _tsscHZ.Values)
            {
                if (i == idx)
                {
                    isRight = false;
                    break;
                }
            }

            if (!isRight)
            {
                daTxt += "<size=64><b>" + CurrentSJ[i] + "</b></size>";
            }
            else
            {
                daTxt += CurrentSJ[i];
            }
        }
        return daTxt;
    }
    #endregion
}
