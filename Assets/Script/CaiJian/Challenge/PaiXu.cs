using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Reign;

public class PaiXu : TiMu
{
    public override void Start()
    {
    }

    public override void OnEnable()
    {
        _CanExist = false;
        CheckDaanBuyState();
    }

    //出题
    public GameObject _TopMenu;

    public Transform _TopLineContent;
    public GameObject _TopHZPrefab;
    public Transform _BottomLineContent;
    public GameObject _BottomHZPrefab;

    private List<GameObject> _TopLineHZList = new List<GameObject>();
    private List<GameObject> _BottomLineHZList = new List<GameObject>();

    private int CurrentShiCiID = -1;//当前显示的诗词id
    private string CurrentSJ = "";//整句
    public override void InitTiMu(Action cb)
    {
        InitTiMu(true);
        cb();
    }

    private void InitTiMu(bool first){
        DestroyObj(_TopLineHZList);
        DestroyObj(_BottomLineHZList);

        _TopMenu.SetActive(false);

        List<string> tsscData = new List<string>();
        //获取格式化后的诗词语句列表
        List<string> tsscList = new List<string>();
        //随机选取其中一句诗词作为考察对象
        do
        {
            tsscData = HZManager.GetInstance().GetTSSC(GetShiCiType(_TiMuFW));
            //获取格式化后的诗词语句列表
            tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[4]);

            CurrentSJ = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];

        } while (!CurrentSJ.Contains("，"));

        CurrentShiCiID = int.Parse(tsscData[0]);

        //生成字序列
        float s = GetHZScale();
        float inv = GetHZInterval();
        int cnt = 0;
        RectTransform rtHZ = _TopHZPrefab.GetComponentInChildren<RectTransform>();
        RectTransform rt = _TopLineContent.GetComponentInChildren<RectTransform>();
        foreach (var hz in CurrentSJ)
        {

            //顶部汉字
            GameObject topHz = Instantiate(_TopHZPrefab, _TopLineContent) as GameObject;
            topHz.SetActive(true);

            topHz.transform.localScale = new Vector3(s, s, 1.0f);
            topHz.transform.localPosition = new Vector3(-rt.rect.width / 2 + rtHZ.rect.width * s / 2 + inv / 2 + (inv + rtHZ.rect.width * s) * cnt
                                                        , _TopHZPrefab.transform.localPosition.y,
                                                        _TopHZPrefab.transform.localPosition.z);


            topHz.GetComponentInChildren<Text>().text = "" + hz;

            _TopLineHZList.Add(topHz);

            //底部汉字
            GameObject bottomHz = Instantiate(_BottomHZPrefab, _BottomLineContent) as GameObject;
            bottomHz.SetActive(true);
            Text bt = bottomHz.GetComponentInChildren<Text>();
            bt.gameObject.SetActive(false);// 底部的为答案，需要显示答案的时候才能显示，或顶部的掉落到底部时显示

            bottomHz.transform.localScale = new Vector3(s, s, 1.0f);
            bottomHz.transform.localPosition = new Vector3(-rt.rect.width / 2 + rtHZ.rect.width * s / 2 + inv / 2 + (inv + rtHZ.rect.width * s) * cnt
                                                        , _BottomHZPrefab.transform.localPosition.y,
                                                        _BottomHZPrefab.transform.localPosition.z);



            bt.text = "" + hz;
            _BottomLineHZList.Add(bottomHz);

            //开始时，不显示外框
            Image border = topHz.GetComponentInChildren<Image>(true);
            Text topText = topHz.GetComponentInChildren<Text>(true);
            Image border2 = bottomHz.GetComponentsInChildren<Image>(true)[1];
            border.DOFade(0.0f, 0.0f);
            border2.DOFade(0.0f, 0.0f);
            topText.color = new Color(topText.color.r, topText.color.g, topText.color.b, 0.0f);
            cnt++;
        }

        _chuZiText.text = "出自：" + GetInfo(_TiMuFW, CurrentShiCiID);
        _chuZiText.gameObject.SetActive(false);
        _BellDark.sprite = Resources.Load("icon/bell", typeof(Sprite)) as Sprite;
        _BellDark.GetComponentInChildren<Text>(true).gameObject.SetActive(true);
        _LeftInfoText.text = "请<b><size=36>熟记</size></b>下方诗句，稍后需排出正确顺序";
        _LeftInfoText.color = new Color(_LeftInfoText.color.r, _LeftInfoText.color.g, _LeftInfoText.color.b,1.0f);
        _MidTagText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        _MidTagText.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 45.0f), 0.0f);
        _CurrentX = 0;
        _isGaming = false;
        //制作信笺按钮只出现在回答失败时
        _makeXJBtn.SetActive(false);
        //只要新生成题目，分数计时一定是0
        _ScoreTime = 0;
        _CantShowDaAn = false;
        _NeedAddScoreTime = true;
        ResetZTTimeAndSpeed();
        //
        if (!first){
            OnZuoTiTimer(true);
        }else{
            _TotalScore = 0;
            _ChuangGuan = 0;

            //每次挑战结束，就重置
            PXZTTime = (int)ZTTime;
            SPEED = 1.0f;
        }

        //+位置，动画问题
        _score.transform.localScale = new Vector3(1f, 1f, 1f);
        _score.SetActive(false);
        if (_scorePlusLightPosY <= -1)
        {
            _scorePlusLightPosY = _currentScoreText.transform.position.y;
        }

        _scorePlusLight.transform.localScale = new Vector3(1f, 1f, 1f);
        _scorePlusLight.transform.position = new Vector3(_scorePlusLight.transform.position.x,
                                                         _scorePlusLightPosY,
                                                        _scorePlusLight.transform.position.z);
    }

    private  int PXZTTime;
    public override void OnChangeZTTimeType(Study.ZTTimeType zzt)
    {
        ZTTime = zzt;
        PXZTTime = (int)ZTTime;
    }
    //根据通过数，更新阅读题目的时间以及字掉落速度
    private void ResetZTTimeAndSpeed(){
        // 首先增加掉落速度
        if(_ChuangGuan <= 100)
        {
            //1.0->0.5
            SPEED = 1.0f - _ChuangGuan * 0.5f / 100;// 最低0.5
        }
        else
        {
            if (_ChuangGuan > 500) return;
            //0.5 -> 0.2 //最低0.2
            PXZTTime -= _ChuangGuan / 100;
            SPEED = 0.5f - _ChuangGuan * 0.3f / 400;

            if (PXZTTime < 5){
                PXZTTime = 5;
            }

            if(SPEED < 0.2){
                SPEED = 0.2f;
            }
        }
        //其次减少阅读时间，最低要有5s，不然太短，不合适
    }

    public Text _chuZiText;
    public GameObject _chuZiContent;
    public Text _LeftInfoText;
    public Image _BellDark;
    public override void OnZuoTiTimer(bool start)
    {
        if (!start)
        {
            _CurrentPassTime = 0;
            CancelInvoke("DoZuoTiTimeClock");

            if (_BellDark != null)
            {
                _BellDark.transform.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                _BellDark.GetComponentInChildren<Text>(true).transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }

            return;
        }
        _CurrentPassTime = 0;
        if (_BellDark != null)
        {
            _BellDark.GetComponentInChildren<Text>(true).text = "" + PXZTTime;
        }

        _LeftInfoText.text = "请<b><size=36>熟记</size></b>下方诗句，稍后需排出正确顺序";

        //字体动画
        Sequence textSeq = DOTween.Sequence();
        foreach (var hz in _TopLineHZList)
        {
            Text text2 = hz.GetComponentInChildren<Text>(true);
            textSeq.Join(text2.DOFade(1.0f, 1.0f));
            if (Setting.GetUseHZAni())
            {
                textSeq.Join(text2.transform.DOShakeScale(1.0f, HZManager.GetInstance().GenerateRandomInt(5, 10) / 10.0f));
            }
        }
        //剩余5s摇铃
        //
        //大于5s，仅执行倒计时
        InvokeRepeating("DoZuoTiTimeClock", 1.0f, 1.0f);
        //ShowMask(true);
    }

    protected override void DoZuoTiTimeClock()
    {
        _CurrentPassTime += 1;

        int ztt = PXZTTime - _CurrentPassTime;
        int half = PXZTTime / 2;
        if (ztt > half)
        {
            _BellDark.GetComponentInChildren<Text>(true).text = "" + ztt;
        }
        else if (ztt <= half && ztt > 0)
        {
            _BellDark.GetComponentInChildren<Text>(true).text = "" + ztt;
            _BellDark.transform.DOShakeRotation(0.5f, (half - ztt) / half * 100 + 20);
        }
        else
        {
            //超时了
            _BellDark.GetComponentInChildren<Text>(true).text = "0";

            if (ztt == 0)
            {
                //时间到了
                Sequence mySequence = DOTween.Sequence();
                for (int i = 0; i < _TopLineHZList.Count; i++)
                {
                    Image border = _TopLineHZList[i].GetComponentInChildren<Image>(true);
                    mySequence.Join(border.DOFade(1.0f, 1.0f));
                }
                mySequence.OnComplete(() => {
                    Text bellText = _BellDark.GetComponentInChildren<Text>(true);
                    Sequence smoothSeq = DOTween.Sequence();
                    smoothSeq
                        .Append(_LeftInfoText.DOFade(0.0f, 0.5f))
                        .Join(_BellDark.DOFade(0.0f,0.5f))
                        .Join(bellText.DOFade(0.0f,0.5f))
                        .AppendCallback(() => {
                            bellText.DOFade(1.0f, 0.0f);
                            bellText.gameObject.SetActive(false);
                            _LeftInfoText.text = "排序马上开始，请作好准备！";
                            _BellDark.sprite = Resources.Load("icon/sound", typeof(Sprite)) as Sprite; 
                        })
                        .Append(_LeftInfoText.DOFade(1.0f, 0.5f))
                        .Join(_BellDark.DOFade(1.0f, 0.5f))
                        .OnComplete(DoZuoTiStartAni);
                });
            }
            else
            {
                OnZuoTiTimer(false);//停止
            }
        }

        if (ztt < PXZTTime && ztt > 0)
        {
            float s = (half * 2 - ztt) / (half * 2) + 0.5f;
            _BellDark.GetComponentInChildren<Text>(true).transform.DOShakeScale(0.5f, new Vector3(s, s, 1.0f));
        }
    }

    private GameObject _NextHZ = null;
    private GameObject _CurrentHZ = null;
    private int _CurrentX = 0;
    private float SPEED = 1.0f;
    private bool _isGaming = false;

    public override void AbandonGame(Action cb = null)
    {

        _CanExist = false;
        DoKillAllAni();
        StopAllClock();

        CancelInvoke("UpdateHZPos");
        //这里可以稍微延时，防止闪屏错觉
        Invoke("GameEnd", 0.1f);
    }
    public override void ExistGame()
    {
        _score.transform.localScale = new Vector3(0f,0f,1f);
        _MidTagText.transform.localScale = new Vector3 (1.0f,1.0f,1.0f);
        _MidTagText.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 45.0f), 0.0f)
                   .OnComplete(() => AbandonGame());

    }

    protected override void StopAllClock()
    {
        DoScoreClockAni(false, true);
        OnZuoTiTimer(false);
        DoFailClockAni(false,true);
    }
    protected override string GetStudyName()
    {
        return gameObject.name;
    }
    #region 排序题

    public void OnDaAnClick()
    {
        if (_CantShowDaAn) return;

        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if (currentLeftShowDaanCnt > 0)
        {
            currentLeftShowDaanCnt--;
        }
        else
        {
            //答案次数已经用完了
            ShowToast("今天次数已用完，请明天再试(次日重置)");
            return;
        }


        _CantShowDaAn = true;

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

        foreach (var hz in _BottomLineHZList)
        {
            hz.GetComponentInChildren<Text>(true).gameObject.SetActive(true);
            Image HZBottomBorder = hz.GetComponentsInChildren<Image>(true)[0];
            HZBottomBorder.DOFade(0.0f, 1.0f).OnComplete(() => { /*HZBottomBorder.gameObject.SetActive(false);*/ });
        }
    }
    #endregion

    //------------------内部接口-----------------
    private void DoZuoTiStartAni(){
        DoHZMoveLeftDownAni(() =>
        {
            _chuZiText.gameObject.SetActive(true);
            DOTween.Kill("ChuZiTextAni");
            Sequence mySequence = DOTween.Sequence();
            mySequence.SetId("ChuZiTextAni");
            float wcztext = _chuZiText.GetComponentInChildren<RectTransform>().rect.width;
            float wczcontent = _chuZiContent.GetComponentInChildren<RectTransform>().rect.width;
            float off = (wcztext + wczcontent) / 2;

            _chuZiText.transform.localPosition = new Vector3(off,
                                                             _chuZiText.transform.localPosition.y,
                                                             _chuZiText.transform.localPosition.z);

            mySequence
                .Append(_chuZiText.transform.DOLocalMoveX(-off, 10.0f))
                .SetLoops(-1, LoopType.Restart);

            //删除标点符号
            RemoveNoneHZ();
            ShuffleSJList(_TopLineHZList);

            //平滑一些执行游戏开始
            Sequence smoothSeq = DOTween.Sequence();
            smoothSeq
                .Append(_TopMenu.transform.DOScale(0.0f, 0.0f))
                .Join(_chuZiText.DOFade(0.0f, 0.0f))
                .Append(_LeftInfoText.DOFade(0.0f, 0.5f))
                .AppendCallback(() => { _LeftInfoText.text = "左滑/右滑移动汉字，下滑加速。"; _TopMenu.SetActive(true); })
                .Append(_LeftInfoText.DOFade(1.0f, 0.5f))
                .Join(_TopMenu.transform.DOScale(1.0f, 0.5f))
                .Join(_chuZiText.DOFade(1.0f, 0.5f))
                .OnComplete(() => {
                    RectTransform rtHZ = _TopHZPrefab.GetComponentInChildren<RectTransform>();
                    //不知道什么原因，当句子是10个字时，出现最后一个字的位置被重置为错误
                    for (int j = 0; j < _TopLineHZList.Count; j++)
                    {
                        Text hztext2 = _TopLineHZList[j].GetComponentInChildren<Text>(true);
                        hztext2.color = new Color(hztext2.color.r, hztext2.color.g, hztext2.color.b, 1.0f);
                        hztext2.transform.localPosition = new Vector3(0f, rtHZ.rect.height / 2, 0f);
                    }
                    _NextHZ = _TopLineHZList[1];
                    _CurrentHZ = _TopLineHZList[0];
                    _CurrentX = _BottomLineHZList.Count / 2;
                    //依次执行toplist中的汉字

                    //确切的，应该移动到中间那个字的位置，而不是屏幕中间
                    float x = _BottomLineHZList[_CurrentX].transform.position.x;
                    _CurrentHZ.transform.DOMoveX(x, Define.PAI_XU_HZ_MOVECENTER_SPEED)
                              .SetEase(Ease.OutSine)
                              .OnComplete(() => {

                                  //ShowMask(false);
                                  _isGaming = true;
                                  _NextHZ.transform.DOMoveX(x, Define.PAI_XU_HZ_MOVECENTER_SPEED)
                                            .SetEase(Ease.OutSine);
                                  //开始掉落
                                  InvokeRepeating("UpdateHZPos", 0.0f, SPEED);
                              });
                });
        });
    }

    public GameObject _Bottom;
    private bool _NeedAddScoreTime = true;
    public override void OnSwipe(Define.SWIPE_TYPE type)
    {
        if (!_isGaming) return;
        if (_CurrentHZ == null) return;

        //如果当前汉字还没有开始掉落，不能执行左右以及加速操作
        RectTransform rtTop = _TopMenu.GetComponentInChildren<RectTransform>();
        if (_CurrentHZ.transform.position.y >= _TopMenu.transform.position.y - rtTop.rect.height/2
           || _CurrentHZ.transform.position.y <= _Bottom.transform.position.y)

            return;

        if (type == Define.SWIPE_TYPE.LEFT)
        {
            if (_CurrentX == 0) return;
            _CurrentX--;
        }
        else if (type == Define.SWIPE_TYPE.RIGHT)
        {
            if (_CurrentX >= _BottomLineHZList.Count - 1) return;
            _CurrentX++;
        }
        else if (type == Define.SWIPE_TYPE.DOWN)
        {
            CancelInvoke("UpdateHZPos");

            InvokeRepeating("UpdateHZPos", 0.1f, 0.1f);

            _NeedAddScoreTime = false;
        }

        float x = _BottomLineHZList[_CurrentX].transform.position.x;
        _CurrentHZ.transform.position = new Vector3(x, _CurrentHZ.transform.position.y, _CurrentHZ.transform.position.z);
    }
    private void UpdateHZPos(){
        RectTransform rtHZ = _TopHZPrefab.GetComponentInChildren<RectTransform>();

        _CurrentHZ.transform.localPosition = new Vector3(_CurrentHZ.transform.localPosition.x, 
                                                    _CurrentHZ.transform.localPosition.y - rtHZ.rect.height, 
                                                    _CurrentHZ.transform.localPosition.z);

        //只有非加速的时候才有效，需要添加判断
        if(_NeedAddScoreTime){
            _ScoreTime++;
        }

        if (_CurrentHZ.transform.position.y <= _Bottom.transform.position.y)
        {
            //判断正确性，
            _NeedAddScoreTime = true;
            //如果正确
            Text HZTop = _CurrentHZ.GetComponentInChildren<Text>(true);
            Text HZBottom = _BottomLineHZList[_CurrentX].GetComponentInChildren<Text>(true);
            Image HZBottomBorder = _BottomLineHZList[_CurrentX].GetComponentsInChildren<Image>(true)[0];


            //这里依然不够严谨，相同的字会有问题。后续优化
            if (HZTop.text == HZBottom.text && HZBottomBorder.gameObject.activeSelf)
            {

                HZTop.gameObject.SetActive(false);
                HZBottom.gameObject.SetActive(true);
                HZBottomBorder.DOFade(0.0f, 1.0f).OnComplete(()=> { HZBottomBorder.gameObject.SetActive(false); });

                if (_NextHZ == null){
                    // 已经全部结束
                    CancelInvoke("UpdateHZPos");
                    _isGaming = false;
                    //全部回答正确

                    //闪动外框后隐藏
                    Sequence borderSeq = DOTween.Sequence();
                    foreach (var hz in _BottomLineHZList)
                    {
                        Text text2 = hz.GetComponentInChildren<Text>(true);
                        borderSeq.Join(text2.DOFade(0.0f, 0.5f));
                    }

                    borderSeq.AppendInterval(0.1f);
                    foreach (var hz in _BottomLineHZList)
                    {
                        Text text2 = hz.GetComponentInChildren<Text>(true);
                        borderSeq.Append(text2.DOFade(1.0f, 3.0f/ _BottomLineHZList.Count));
                    }
                    //保存分数
                    SaveScore();
                    //显示结束面板
                    DoResult(true);
                    return;
                }

                _CurrentHZ = _NextHZ;
                _CurrentX = _BottomLineHZList.Count / 2;

                CancelInvoke("UpdateHZPos");

                int nidx = _TopLineHZList.IndexOf(_CurrentHZ) + 1;
                if (nidx < _TopLineHZList.Count)
                {
                    _NextHZ = _TopLineHZList[nidx];
                    float x = _BottomLineHZList[_CurrentX].transform.position.x;
                    _NextHZ.transform.DOMoveX(x, Define.PAI_XU_HZ_MOVECENTER_SPEED)
                            .SetEase(Ease.OutSine);

                }
                else if(nidx == _TopLineHZList.Count)
                {
                    //最后一个
                    _NextHZ = null;
                }

                InvokeRepeating("UpdateHZPos", 0f , SPEED);
            }
            else{
                _isGaming = false;

                //如果不正确
                CancelInvoke("UpdateHZPos");

                Image error = _BottomLineHZList[_CurrentX].GetComponentsInChildren<Image>(true)[2];
                Sequence borderSeq2 = DOTween.Sequence();
                error.color = new Color(error.color.r, error.color.g, error.color.b,0.0f);
                HZBottom.color = new Color(HZBottom.color.r, HZBottom.color.g, HZBottom.color.b, 0.0f);

                borderSeq2
                    .Append(HZTop.DOFade(0.0f, 1.0f))
                    .AppendCallback(() => { HZTop.gameObject.SetActive(false); error.gameObject.SetActive(true); })
                    .Append(error.DOFade(1.0f, 1.0f))
                    .Append(error.DOFade(0.0f, 1.0f))
                    .AppendCallback(() => { error.gameObject.SetActive(false); HZBottom.gameObject.SetActive(true); })
                    .Append(HZBottom.DOFade(1.0f, 1.0f));

                borderSeq2.AppendInterval(0.1f);
                //边框隐藏
                for (int i = 0; i < _BottomLineHZList.Count;i++)
                {
                    if (_CurrentX == i) continue;

                    Image border2 = _BottomLineHZList[i].GetComponentsInChildren<Image>(true)[0];
                    borderSeq2.Join(border2.DOFade(0.0f, 1.0f));
                }
                borderSeq2.AppendInterval(1.0f);
                for (int i = 0; i < _BottomLineHZList.Count; i++)
                {
                    Text text2 = _BottomLineHZList[i].GetComponentInChildren<Text>(true);
                    if(!text2.gameObject.activeSelf){
                        text2.gameObject.SetActive(true);
                        text2.color = new Color(text2.color.r, text2.color.g, text2.color.b,0.0f);
                        borderSeq2.Join(text2.DOFade(1.0f, 1.0f));
                    }
                }

                //显示结算
                DoResult(false);
            }
        }
    }
    private void DoHZMoveLeftDownAni(Action cb = null) {
        float s = GetHZScale();
        float inv = GetHZInterval();
        RectTransform rtHZ = _TopHZPrefab.GetComponentInChildren<RectTransform>();
        RectTransform rt = _TopLineContent.GetComponentInChildren<RectTransform>();

        float toX = -rt.rect.width / 2 - rtHZ.rect.width * s / 2 - inv / 2;

        float delay = 0.0f;
        int lineCnt = _TopLineHZList.Count;
        for (int i = 0; i < lineCnt; i++)
        {
            Sequence mySequence = DOTween.Sequence();

            float currentRunTime = 0, prevRunTime = 0;

            if (i == 0)
            {
                prevRunTime = 0.01f;
                currentRunTime = Define.PAI_XU_HZ_DOWNLEFT_SPEED;
            }
            else if (i == 1)
            {
                prevRunTime = Define.PAI_XU_HZ_DOWNLEFT_SPEED;
                currentRunTime = (1 - Mathf.Log(i, lineCnt)) * Define.PAI_XU_HZ_DOWNLEFT_SPEED;
            }
            else
            {
                prevRunTime = (1 - Mathf.Log(i - 1, lineCnt)) * Define.PAI_XU_HZ_DOWNLEFT_SPEED;
                currentRunTime = (1 - Mathf.Log(i, lineCnt)) * Define.PAI_XU_HZ_DOWNLEFT_SPEED;
            }

            delay += prevRunTime / 2;

            Image line = _BottomLineHZList[i].GetComponentsInChildren<Image>(true)[1];
            line.transform.localScale = new Vector3(0.0f,0.0f,1.0f);
            if (!HZManager.GetInstance().CheckIsHZ(_TopLineHZList[i].GetComponentInChildren<Text>(true).text)){
                //是标点符号，就移动所有
                mySequence
                    .AppendInterval(delay)
                    .Append(_TopLineHZList[i].transform.DOMoveY(_BottomLineHZList[i].transform.position.y, currentRunTime))
                    .Join(line.DOFade(1.0f, currentRunTime))
                    .Join(line.transform.DOScale(1.0f, currentRunTime));
            }
            else{
                //不是标点符号，就只移动外框
                Image border = _TopLineHZList[i].GetComponentInChildren<Image>(true);
                Image border2 = _BottomLineHZList[i].GetComponentsInChildren<Image>(true)[0];
                Text hztext = _TopLineHZList[i].GetComponentInChildren<Text>(true);
                mySequence
                    .AppendInterval(delay)
                    .Append(border.transform.DOMoveY(border2.transform.position.y, currentRunTime))
                    .Join(hztext.transform.DOMoveX(toX, currentRunTime))
                    .Join(hztext.DOFade(0.0f, currentRunTime))
                    .Join(line.DOFade(1.0f, currentRunTime))
                    .Join(line.transform.DOScale(1.0f, currentRunTime));
            }

            mySequence.SetEase(Ease.InSine);

            if (i == _TopLineHZList.Count - 1)
            {
                mySequence
                    .OnComplete(() =>
                    {
                        if (cb != null)
                        {
                            //重置并隐藏上方掉下来的外框，显示底部的外框
                            for (int j = 0; j < _TopLineHZList.Count; j++)
                            {
                                _TopLineHZList[j].transform.localPosition = new Vector3(toX,
                                                            _TopLineHZList[j].transform.localPosition.y,
                                                            _TopLineHZList[j].transform.localPosition.z);

                                Image b1 = _TopLineHZList[j].GetComponentInChildren<Image>(true);
                                b1.transform.localPosition = new Vector3(0f, 0f, 0f);
                                b1.gameObject.SetActive(false);
                                Text hztext2 = _TopLineHZList[j].GetComponentInChildren<Text>(true);
                                hztext2.color = new Color(hztext2.color.r, hztext2.color.g, hztext2.color.b,1.0f);
                                hztext2.transform.localPosition = new Vector3(0f, rtHZ.rect.height/2, 0f);
                                

                                Image b2 = _BottomLineHZList[j].GetComponentsInChildren<Image>(true)[0];
                                b2.gameObject.SetActive(true);

                                if (!HZManager.GetInstance().CheckIsHZ(_BottomLineHZList[j].GetComponentInChildren<Text>(true).text))
                                {
                                    _TopLineHZList[j].GetComponentInChildren<Text>(true).gameObject.SetActive(false);
                                    _BottomLineHZList[j].GetComponentInChildren<Text>(true).gameObject.SetActive(true);
                                    b2.DOFade(0.0f,1.0f).OnComplete(()=>{ b2.gameObject.SetActive(false); });
                                }
                            }

                            cb();
                        }
                    });
            }
        }
    }

    //结算面板
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

    public GameObject _MidTagText;
    protected override int GetCurrentScore()
    {
        //耗时后得分
        int baseScore = (int)((1 + _ChuangGuan / 20) * Define.BASE_SCORE_10S);
        int cs = (int)(baseScore * (1.0f - _ScoreTime / (11.0f * _TopLineHZList.Count)));//每个字可以掉落11格

        // 额外加分
        if (_TiMuFW == eTiMuFanWei.All)
        {
            cs += Define.PLUS_SCORE_ALL;
        }
        else if (_TiMuFW == eTiMuFanWei.TangShi)
        {
            cs += Define.PX_PLUS_SCORE_TANGSHI;
        }
        else if (_TiMuFW == eTiMuFanWei.SongCi)
        {
            cs += Define.PX_PLUS_SCORE_SONGCI;
        }
        else if (_TiMuFW == eTiMuFanWei.GuShi)
        {
            cs += Define.PLUS_SCORE_GUSHI;
        }
        else if (_TiMuFW == eTiMuFanWei.ShiJing)
        {
            cs += Define.PX_PLUS_SCORE_SHIJING;//原则上来说，排序模式的时候，诗经类型不能增加2分
        }

        //连续闯关加分
        cs += _ChuangGuan;

        return cs;
    }

    private void DoResult(bool right){
        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(_MidTagText.transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.5f))
            .Join(_MidTagText.transform.DOScale(0f, 0.5f))
            .Join(_LeftInfoText.DOFade(0.0f, 0.5f))
            .OnComplete(() => { 
                DoScoreAni(right);
                if(HZManager.GetInstance().GenerateRandomInt(0,2) == 0){
                    _LeftInfoText.text = "通关数增加，读题时间缩短，字落速度加快";
                }
                else{
                    _LeftInfoText.text = "做题耗时仅计算字掉落时非加速掉落时间";
                }
               
                _LeftInfoText.DOFade(1.0f, 0.5f);
            });
    }

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

            //每次挑战结束，就重置
            PXZTTime = (int)ZTTime;
            SPEED = 1.0f;
        }

        _score.SetActive(true);


        if (right)
        {
            _score.transform.localScale = new Vector3(0f, 0f, 0f);
            Sequence mySequence = DOTween.Sequence();
            mySequence.SetId("DoScoreClockAni" + GetStudyName());
            _scorePlusLight.gameObject.SetActive(true);
            mySequence
                .Append(_scorePlusLight.DOFade(0.0f, 0.0f))
                .Append(_score.transform.DOScale(1.0f, 1.0f))
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
            _score.transform.localScale = new Vector3(0f, 0f, 0f);
            _scorePlusLight.gameObject.SetActive(false);
            Sequence mySequence = DOTween.Sequence();
            mySequence.SetId("DoScoreClockAni" + GetStudyName());
            mySequence
                .Append(_score.transform.DOScale(1.0f, 1.0f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    //执行倒计时
                    DoScoreClockAni(true);

                    //此时可以直接返回
                    _CanExist = true;
                    _makeXJBtn.SetActive(true);
                    _makeXJBtn.transform.DOShakePosition(1.0f, 5);
                    //ShowMask(false);//允许点击
                });
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

            Sequence mySequence = DOTween.Sequence();

            mySequence
                .Append(_score.transform.DOScale(0.0f,0.5f))
                .Append(_MidTagText.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 45.0f),0.5f))
                .Join(_MidTagText.transform.DOScale(1.0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    //ShowMask(false);//允许点击

                    if (right)
                    {
                        InitTiMu(false);
                        _CanExist = false;
                    }
                    else
                    {
                        gameObject.SetActive(false);

                        _TSTiText.text = "得分";
                        _CSTiText.text = "本题";
                        _LXTiText.text = "闯关";


                        Invoke("GameEnd", 0.1f);
                    }
                });
        }
    }

    public void OnMakeXinJianClick()
    {
        _score.SetActive(false);

        DoKillAllAni();
        StopAllClock();

        _CanExist = false;

        OnMakeXinJian(CurrentShiCiID, CurrentSJ, GetShiCiType(_TiMuFW));
    }

    //---------------------------------------------------------------------
    private float GetHZScale(){
        float s = 1.0f;

        //_TopLineContent
        RectTransform rt = _TopLineContent.GetComponentInChildren<RectTransform>();
        RectTransform rtHZ = _TopHZPrefab.GetComponentInChildren<RectTransform>();

        //只有当最多能显示的字数小于当前诗句字数的时才缩放，也就是只缩小，不会放大
        //如果不需要缩小时候，可能会需要设置字 间隔，需要缩小则没有间隔
        if(rt.rect.width / rtHZ.rect.width < CurrentSJ.Length)
        {
            s = rt.rect.width / (CurrentSJ.Length * rtHZ.rect.width);
        }

        return s;
    }

    private float GetHZInterval(){
        float inv = 0;

        RectTransform rt = _TopLineContent.GetComponentInChildren<RectTransform>();
        RectTransform rtHZ = _TopHZPrefab.GetComponentInChildren<RectTransform>();

        if (CurrentSJ.Length * rtHZ.rect.width < rt.rect.width)
        {
            inv = (rt.rect.width - CurrentSJ.Length * rtHZ.rect.width) / (CurrentSJ.Length);
        }

        return inv;
    }

    public void RemoveNoneHZ()
    {
        for (int i = _TopLineHZList.Count - 1; i >= 0; i--)
        {
            if (!HZManager.GetInstance().CheckIsHZ(_TopLineHZList[i].GetComponentInChildren<Text>(true).text))
            {
                Destroy(_TopLineHZList[i]);
                _TopLineHZList.RemoveAt(i);
            };
        }
    }
}
