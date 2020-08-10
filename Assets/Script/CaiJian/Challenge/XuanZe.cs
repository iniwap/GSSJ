using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Reign;

public class XuanZe : TiMu
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
    private int CurrentShiCiID = -1;//当前显示的诗词id
    private string CurrentSJ = "";//整句
    private int _TiMuRight;
    private List<string> _XuanXiang = new List<string>();
    private string _BlankTiMu;//扣掉的句子
    //出题
    public override void InitTiMu(Action cb)
    {
        InitTiMu(true);
        cb();
    }

    private void InitTiMu(bool first)
    {
        //哪个选项为正确选项
        _TiMuRight = HZManager.GetInstance().GenerateRandomInt(1, 5);

        _XuanXiang.Clear();
        CurrentShiCiID = -1;
        CurrentSJ = "";
        string tssc = "";

        if (_TiMuFW == eTiMuFanWei.ChangShi || _TiMuFW == eTiMuFanWei.DianGu)
        {
            //典故和常识考察
        }
        else
        {
            List<string> tsscData = new List<string>();
            //获取格式化后的诗词语句列表
            List<string> tsscList = new List<string>();
            //随机选取其中一句诗词作为考察对象
            do
            {
                tsscData = HZManager.GetInstance().GetTSSC(GetShiCiType(_TiMuFW));
                //获取格式化后的诗词语句列表
                tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[4]);

                tssc = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];

            } while (!tssc.Contains("，"));

            CurrentShiCiID = int.Parse(tsscData[0]);
            CurrentSJ = tssc;

            string[] jzs = tssc.Split('，');
            int indx = HZManager.GetInstance().GenerateRandomInt(0, jzs.Length);
            _BlankTiMu = jzs[indx];

            string bk = "";
            for (int j = 0; j < _BlankTiMu.Length; j++)
            {
                if (HZManager.GetInstance().CheckIsHZ(_BlankTiMu[j]))
                {
                    bk += "_";
                }else{
                    bk += _BlankTiMu[j];
                }
            }
            jzs[indx] = bk;
            tssc = String.Join("，", jzs);

            // 首先添加题目
            _XuanXiang.Add(tssc);//

            for (int i = 1; i < 5; i++)
            {
                if (_TiMuRight == i)
                {

                    if (!HZManager.GetInstance().CheckIsHZ(_BlankTiMu[_BlankTiMu.Length - 1]))
                    {
                        _BlankTiMu = _BlankTiMu.Substring(0, _BlankTiMu.Length - 1);
                    }

                    _XuanXiang.Add(_BlankTiMu);

                }
                else
                {
                    int deadCnt = 0;
                    string dead = "";
                    string jz = "";

                    do
                    {
                        jz = GetRandomHalfShiJu();

                        dead = jz;

                        deadCnt++;

                        if(deadCnt >= 10){ //最多查找10次，是否存在字数相等的诗句，如果不存在，则从原诗中查找
                            break;
                        }
                    } while (_TiMuFW != eTiMuFanWei.SongCi && jz.Length != _BlankTiMu.Length);

                    deadCnt = 0;
                    if (_TiMuFW != eTiMuFanWei.SongCi  && jz.Length != _BlankTiMu.Length)
                    {
                        //如果长度不相等，则直接从原来的诗句中提取一句
                        string tssc3 = "";
                        //随机选取其中一句诗词
                        do
                        {
                            tssc3 = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];
                            deadCnt++;
                            if(deadCnt >= 5){
                                break;
                            }
                        } while (!tssc3.Contains("，") || tssc3.Equals(CurrentSJ));

                        //陷入死循环，随便选取一句
                        if (deadCnt >= 5)
                        {
                            jz = dead;
                        }
                        else
                        {
                            string[] jzs3 = tssc3.Split('，');
                            jz = jzs3[HZManager.GetInstance().GenerateRandomInt(0, jzs3.Length)];

                            if (!HZManager.GetInstance().CheckIsHZ(jz[jz.Length - 1]))
                            {
                                jz = jz.Substring(0, jz.Length - 1);
                            }
                        }
                    }

                    _XuanXiang.Add(jz);

                }
            }

            //不能存在相同的选项
            for (int i = 1; i < 5; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    if(_XuanXiang[i] == _XuanXiang[j] 
                       && i != j
                       && _TiMuRight != i){
                        
                        string jz = "";
                        do
                        {
                            jz = GetRandomHalfShiJu();
                        } while (CheckIfSJInXX(jz));

                        _XuanXiang[i] = jz;
                    }
                }
            }

        }

        //设置正确答案
        SetRightDaAn();

        SetTiMu();

        EnableZuoDaBtn(true);

        if (!first)
        {
            OnZuoTiTimer(true);//开启
        }else{
            _TotalScore = 0;
            _ChuangGuan = 0;
        }

        //只要新生成题目，分数计时一定是0
        _ScoreTime = 0;

        EnableShowDaanBtn(true);
    }
    #region 选择题
    public GameObject _da;
    public void OnDaAnClick()
    {
        if (_da.activeSelf) return;

        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if (currentLeftShowDaanCnt > 0)
        {
            currentLeftShowDaanCnt--;
        }
        else
        {
            ShowToast("今天次数已用完，请明天再试(次日重置)");
            //答案次数已经用完了
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

    private void SetRightDaAn(){
        _da.SetActive(false);//需要隐藏

        Transform da = _da.transform.Find("DaAn/DanAnTiText");
        Text daText = da.gameObject.GetComponent<Text>();

        daText.text = GetXXTag(_TiMuRight) + _BlankTiMu + "\n出自："+ GetInfo(_TiMuFW, CurrentShiCiID);
    }

    public GameObject _TiMu;
    private void SetTiMu(){
        Text[] XXTexts = _TiMu.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < _XuanXiang.Count; i++)
        {
            if(i == 0){
                XXTexts[i].text = GetXXTag(i,false) + _XuanXiang[i];
            }
            else{
                XXTexts[i].text = GetXXTag(i) + _XuanXiang[i];
            }
        }
    }

    public GameObject _zuoDa;
    private void EnableZuoDaBtn(bool enable)
    {
        Button[] btns = _zuoDa.GetComponentsInChildren<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = enable;
        }
    }
    public GameObject _failClockText;
    public void OnZuoDaClick(int xx)
    {

        ShowMask(true);
        EnableShowDaanBtn(false);
        EnableZuoDaBtn(false);
        StopAllClock();

        RestTiMu();
        _TiMu.gameObject.SetActive(true);
        _failClockText.SetActive(false);

        if (xx == _TiMuRight)
        {
            //回答正确 - 恭喜回答正确
            //统计分数
            SaveScore();
            DoDaTiAni(xx, true);
        }
        else
        {
            //回答错误
            DoDaTiAni(xx, false);
        }

    }

    public Text _xzLight;//选项上升动画
    public Image _midDark;//到达过度动画
    public Image _smileLight;//正确与否动画
    public Image _resBg;//遮罩

    private void DoDaTiAni(int xx, bool right)
    {
        _xzLight.gameObject.SetActive(true);
        _resBg.gameObject.SetActive(true);
        _midDark.gameObject.SetActive(false);
        _smileLight.gameObject.SetActive(false);


        _xzLight.transform.position = new Vector3(_xzLight.transform.position.x,
                                                  _zuoDa.transform.position.y,
                                                 _xzLight.transform.position.z);

        _xzLight.text = GetXXTag(xx,false);


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
            .Join(_smileLight.DOFade(0.0f, 0.0f))
            .Join(_smileLight.transform.DOScale(1.0f, 0.0f))
            .Join(_midDark.transform.DOScale(0.0f, 0.0f))
            .Join(_midDark.DOFade(50 / 255.0f, 0.0f))
            .Join(_resBg.DOFade(0.0f, 0.0f))

            .Append(_xzLight.DOFade(1.0f, 0.2f))
            .Append(_xzLight.transform.DOMoveY(_TiMu.transform.position.y, speed / 2))
            .Join(_xzLight.transform.DOScale(1.0f, speed / 2))
            .Join(_resBg.DOFade(50 / 255.0f, speed / 2))

            .AppendCallback(() => _midDark.gameObject.SetActive(true))
            .Append(_midDark.DOFade(0.0f, speed / 2))
            .Join(_xzLight.DOFade(0.0f, 0.2f))
            .Join(_midDark.transform.DOScale(4.0f, speed / 2))

            .AppendCallback(() => _smileLight.gameObject.SetActive(true))
            .Append(_smileLight.DOFade(1.0f, speed / 2))
            .Append(_smileLight.transform.DOScale(2.0f, speed))
            .Join(_smileLight.DOFade(0.0f, speed))
            .Join(_resBg.DOFade(0.0f, speed))
            .OnComplete(() => DoResultAni(xx,right));

    }

    private void DoResultAni(int xx,bool right)
    {
        Text[] XXTexts = _TiMu.GetComponentsInChildren<Text>(true);
        Sequence mySequence = DOTween.Sequence();

        //首先隐藏非选中项目，如果正确则不变，如果错误，则再隐藏错误项目，并显示正确的项目
        for (int i = 1; i < XXTexts.Length; i++)
        {
            if (i != xx)
            {
                mySequence.Join(XXTexts[i].DOFade(0.0f, 1.0f));
            }
        }

        if (right)
        {
            mySequence
                .AppendInterval(1.0f)
                .AppendCallback(() => {
                    //进入下一题 - nexttm
                    _xzLight.gameObject.SetActive(false);
                    _midDark.gameObject.SetActive(false);
                    _smileLight.gameObject.SetActive(false);
                    _resBg.gameObject.SetActive(false);
                    DoScoreAni(right);
            });
        }
        else
        {
            if(xx != 0){
                mySequence
                    .Append(XXTexts[xx].DOFade(0.0f, 0.5f))
                    .Append(XXTexts[xx].DOFade(1.0f, 0.5f))
                    .Append(XXTexts[xx].DOFade(0.0f, 0.5f))
                    .Append(XXTexts[xx].DOFade(1.0f, 0.5f))
                    .Append(XXTexts[xx].DOFade(0.0f, 0.5f));
            }

            mySequence
                .Append(XXTexts[_TiMuRight].DOFade(1.0f, 1.0f))
                .AppendInterval(1.0f)
                .AppendCallback(() => {
                    //进入下一题 - nexttm
                    _xzLight.gameObject.SetActive(false);
                    _midDark.gameObject.SetActive(false);
                    _smileLight.gameObject.SetActive(false);
                    _resBg.gameObject.SetActive(false);
                    DoScoreAni(right);
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
    private void DoScoreAni(bool right)
    {
        if (right)
        {
            int cs = GetCurrentScore();
            _scoreClockText.text = "<b>5s</b>\n<size=42>下一题</size>";
            _totalScoreText.text = "" + (_TotalScore - cs);//to ts then to newts
            _currentScoreText.text = "" + cs;// to cs
            _chuangGuanText.text = "" + _ChuangGuan;

        }
        else
        {
            //最高得分
            //本次得分
            //连续
            int hs = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_SCORE + GetStudyName(), 0);
            int lx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_CG + GetStudyName(), 0);

            _scoreClockText.text = "<b>10s</b>\n<size=42>自动返回</size>";

            _totalScoreText.text = "" + hs + "/" + lx;
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

        if (sc != null)
        {
            RectTransform scRt = sc.GetComponent<RectTransform>();

            if (_scorePlusLightPosY <= -1)
            {
                _scorePlusLightPosY = _scorePlusLight.transform.position.y;
            }

            if (right)
            {
                _scorePlusLight.transform.position = new Vector3(_scorePlusLight.transform.position.x,
                                                                _scorePlusLightPosY,
                                                                _scorePlusLight.transform.position.z);

                sc.transform.localPosition = new Vector3(scRt.rect.width, sc.transform.localPosition.y,sc.transform.localPosition.z);

                Sequence mySequence = DOTween.Sequence();
                mySequence.SetId("DoScoreClockAni" + GetStudyName());
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
            }
            else
            {
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
                        _makeXJBtn.transform.DOShakePosition(1.0f,5);
                        ShowMask(false);//允许点击
                    });
            }
        }
    }


    private int _CurrentScoreTime = 0;
    private void DoScoreClockAni(bool start, bool needStopAni = false)
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
        if (_scoreClockText.text.Contains("下一题"))
        {
            right = true;
            fc = (int)Study.ZTTimeType.ZT_10S / 2 - _CurrentScoreTime;
        }
        else
        {

            right = false;
            //如果是结束返回，则倒计时10s
            fc = (int)Study.ZTTimeType.ZT_10S - _CurrentScoreTime;
        }

        if (fc > 0)
        {
            if (right)
            {
                _scoreClockText.text = "<b>" + fc + "s</b>\n<size=42>下一题</size>";
            }
            else
            {
                _scoreClockText.text = "<b>" + fc + "s</b>\n<size=42>自动返回</size>";
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
                    .Append(sc.DOLocalMoveX(scRt.rect.width, 0.5f));

                Text[] XXTexts = _TiMu.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < XXTexts.Length; i++)
                {
                    mySequence.Join(XXTexts[i].DOFade(0.0f, 1.0f));
                }

                mySequence
                    .SetEase(Ease.InSine)
                    .OnComplete(() =>
                    {
                        ShowMask(false);//允许点击

                        if (right)
                        {
                            InitTiMu(false);
                            Sequence mySequence2 = DOTween.Sequence();
                            for (int i = 0; i < XXTexts.Length; i++)
                            {
                                mySequence2.Join(XXTexts[i].DOFade(1.0f, 1.0f));
                            }

                            _CanExist = false;
                        }
                        else
                        {
                            _TiMu.SetActive(true);

                            Sequence mySequence2 = DOTween.Sequence();
                            for (int i = 0; i < XXTexts.Length; i++)
                            {
                                XXTexts[i].gameObject.SetActive(true);
                                mySequence2.Join(XXTexts[i].DOFade(1.0f, 1.0f));
                            }

                            gameObject.SetActive(false);

                            _TSTiText.text = "得分";
                            _CSTiText.text = "本题";
                            _LXTiText.text = "闯关";

                            //这里可以稍微延时，防止闪屏错觉
                            Invoke("GameEnd", 0.1f);
                        }
                    });
            }
        }
    }

    private void Fail()
    {
        Transform tm = gameObject.transform.Find("TiMu");//TiText
        if (tm != null)
        {
            GameObject tmclockobj = tm.Find("FailClockTiText").gameObject;
            tmclockobj.SetActive(false);
        }

        _TiMu.SetActive(true);

        Text[] XXTexts = _TiMu.GetComponentsInChildren<Text>(true);
        Sequence mySequence2 = DOTween.Sequence();
        for (int i = 0; i < XXTexts.Length; i++)
        {
            XXTexts[i].gameObject.SetActive(true);
            mySequence2.Join(XXTexts[i].DOFade(1.0f, 1.0f));
        }

        DoResultAni(0, false);
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

    protected override void StopAllClock()
    {
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
                                                     sc.localPosition.y, sc.localPosition.z);
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

    #endregion

    //----------------------------一些接口---------------------------------
    private string GetXXTag(int indx,bool withDot = true){
        string xxtag = "";

        if (indx == 1)
        {
            xxtag += "A";
        }
        else if (indx == 2)
        {
            xxtag += "B";
        }
        else if (indx == 3)
        {
            xxtag += "C";
        }
        else if (indx == 4)
        {
            xxtag += "D";
        }

        if(withDot){
            xxtag += ".";
        }

        return xxtag;
    }

    private bool CheckIfSJInXX(string jz){
        bool exist = false;

        for (int j = 1; j < 5; j++)
        {
            if (jz.Equals(_XuanXiang[j]))
            {
                exist = true;
                break;
            }
        }

        return exist;
    }
}
