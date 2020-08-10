using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Events;
using Reign;

public class XuanTian : TiMu
{
    public override void Start()
    {

    }

    public override void OnEnable()
    {
        _CanExist = false;
        CheckDaanBuyState();
    }

    private int PXZTTime;
    public override void OnChangeZTTimeType(Study.ZTTimeType zzt)
    {
        ZTTime = Study.ZTTimeType.ZT_60S;//zzt;//该模式采用60s
        PXZTTime = (int)ZTTime;
    }

    public override void InitTiMu(Action cb)
    {   
        _TotalScore = 0;
        _ChuangGuan = 0;
        PXZTTime = (int)ZTTime;

        InitTiMu(true,cb);
    }

    public XTPencil _Pencil;
    public void SetCanShowPencil(bool can)
    {
        _Pencil.SetCanShowPencil(can);
    }
    private List<GameObject> _HZList = new List<GameObject>();
    public Transform _HZContent;
    public GameObject _HZPrefab;

    private int CurrentShiCiID = -1;//当前显示的诗词id
    private string CurrentSJ = "";//整句
    private List<int> _CurrentSJIndex = new List<int>();// 主要用于答案显示
    private List<int> _CurrentSelectSJIndex = new List<int>();// 主要用于记录当前划选的字

    private int XT_HZ_MATRIX = Define.MAX_XT_HZ_MATRIX;

    private void InitTiMu(bool first,Action cb = null)
    {
        //首先选择诗句
        XT_HZ_MATRIX = 5;
        XT_HZ_MATRIX = XT_HZ_MATRIX > Define.MAX_XT_HZ_MATRIX ? Define.MAX_XT_HZ_MATRIX : XT_HZ_MATRIX;//超过最大没有意义，难度过高

        ///////////////////诗词数据初始化//////////////////////
        if (_ChuangGuan == 0)
        {
            if(_TiMuFW == eTiMuFanWei.GuShi)
            {
                CurrentSJ = "天苍苍，野茫茫。";
                CurrentShiCiID = 78;
            }
            else if(_TiMuFW == eTiMuFanWei.ShiJing)
            {
                CurrentSJ = "执子之手，与子偕老。";
                CurrentShiCiID = 30;
            }
            else if (_TiMuFW == eTiMuFanWei.SongCi)
            {
                CurrentSJ = "知否，知否？应是绿肥红瘦。";
                CurrentShiCiID = 174;
            }
            else if (_TiMuFW == eTiMuFanWei.TangShi)
            {
                CurrentSJ = "床前明月光，疑是地上霜。";
                CurrentShiCiID = 10;
            }
            else
            {
                InitSJ();
            }
        }
        else
        {
            InitSJ();
        }

        // 初始化界面
        InitUI();

        InitMatrixSP();

        ////////////////将诗句汉字按照一定的搜索顺序插入到方阵中///////////////////////
        //支持8个方向，需要记录这个列表序列 用于答案提示，可能会遇到汉字相同，但是路径不同的情况
        int currentIndex = HZManager.GetInstance().GenerateRandomInt(0, XT_HZ_MATRIX * XT_HZ_MATRIX);
        int col = currentIndex % XT_HZ_MATRIX;
        int row = currentIndex / XT_HZ_MATRIX;
        ///////////////////将剩下空着的方阵插入其他随机汉字///////////////////////////

        SearchSJPath(col, row, XT_HZ_MATRIX,(List<int> path) =>
        {
            _CurrentSJIndex.AddRange(path);

            //生成汉字序列
            string[] hzList = new string[XT_HZ_MATRIX * XT_HZ_MATRIX];
            for (int i = 0; i < hzList.Length; i++)
            {
                hzList[i] = "";
            }

            //设置搜索出的路径诗句
            for (int j = 0; j < _CurrentSJIndex.Count; j++)
            {
                hzList[_CurrentSJIndex[j]] = "" + CurrentSJ[j];
            }

            //插入干扰字
            InsertDisturbHZ(hzList);

            ////////////////////////////////////////////////////////////////////
            //生成字序列 -- 汉字方阵 根据诗句长度动态变化大小
            float wh = GetGridSizeWH();
            int fontSize = _HZPrefab.GetComponentInChildren<Text>().fontSize;
            float s = wh / _HZPrefab.GetComponent<RectTransform>().rect.width;
            GridLayoutGroup gl = _HZContent.GetComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(wh,wh);
            for (int i = 0; i < hzList.Length; i++)
            {
                //顶部汉字
                GameObject Hz = Instantiate(_HZPrefab, _HZContent) as GameObject;
                Hz.SetActive(true);

                XTHZ xtHZ = Hz.GetComponent<XTHZ>();
                xtHZ.InitHZ(i, hzList[i], _BgColor);
                _HZList.Add(Hz);

                RectTransform hzrt = Hz.GetComponent<RectTransform>();
                hzrt.sizeDelta = new Vector2(wh, wh);

                BoxCollider bc = Hz.GetComponent<BoxCollider>();
                bc.size = new Vector3(wh/2,wh/2,1);

                Hz.GetComponentInChildren<Text>().fontSize = (int)(s * fontSize);

                //该处无字
                if(hzList[i] == ""){
                    bc.enabled = false;//禁用碰撞检测
                }
            }

            /////  由于是多线程，必须在这里重置/////
            if (!first)
            {
                OnZuoTiTimer(true);
            }

            if(cb != null){
                cb();
            }
        });

    }

    private void InitUI(){
        DestroyObj(_HZList);
        _CurrentSJIndex.Clear();
        _CurrentSelectSJIndex.Clear();
        _HasShowTip = false;
        _score.SetActive(false);
        _ResultSJ.SetActive(false);
        if (_scorePlusLightPosY <= -1)
        {
            _scorePlusLightPosY = _currentScoreText.transform.position.y;
        }

        _scorePlusLight.transform.localScale = new Vector3(1f, 1f, 1f);
        _scorePlusLight.transform.position = new Vector3(_scorePlusLight.transform.position.x,
                                                         _scorePlusLightPosY,
                                                        _scorePlusLight.transform.position.z);
        _TipBrush.gameObject.SetActive(false);


        //制作信笺按钮只出现在回答失败时
        _makeXJBtn.SetActive(false);
        //只要新生成题目，分数计时一定是0
        _ScoreTime = 0;
        _CantShowDaAn = false;
        _TipInfo.color = new Color(_TipInfo.color.r, _TipInfo.color.g, _TipInfo.color.b, 0.0f);
        _TipInfo.text = "注意：含标点且需顺序连通(八个方向)，即不能跳跃划选";
        _TopMenu.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        _SearchDifficult = 0.0f;
        _DisturbCnt = 0;//干扰字个数

        //设置当前做题时间
        ResetZTTimeAndSpeed();
    }

    //初始化分割线，根据方阵大小变化
    public GameObject _MatrixHSP;
    public GameObject _MatrixVSP;
    public GameObject _Matrix;
    public GameObject _MHSP;//分割线，假如固定为4
    public GameObject _MVSP;//分割线，假如固定为4
    private void InitMatrixSP(){
        Image[] hsp = _MatrixHSP.GetComponentsInChildren<Image>(true);
        Image[] vsp = _MatrixVSP.GetComponentsInChildren<Image>(true);

        HorizontalLayoutGroup hl = _MatrixVSP.GetComponent<HorizontalLayoutGroup>();
        VerticalLayoutGroup vl = _MatrixHSP.GetComponent<VerticalLayoutGroup>();

        for (int i = 0; i < hsp.Length; i++)
        {
            hsp[i].gameObject.SetActive(false);
            vsp[i].gameObject.SetActive(false);
        }

        for (int i = 0; i <= XT_HZ_MATRIX;i++){
            hsp[i].gameObject.SetActive(true);
            vsp[i].gameObject.SetActive(true);
        }

        float wh = GetGridSizeWH();
        hl.spacing = wh;
        vl.spacing = wh;
    }

    private float GetGridSizeWH(){
        RectTransform mhsp = _MHSP.GetComponent<RectTransform>();
        RectTransform mvsp = _MVSP.GetComponent<RectTransform>();
        RectTransform mrt = _Matrix.GetComponent<RectTransform>();
        float wh = (mrt.rect.width - mhsp.rect.height * (XT_HZ_MATRIX + 1)) / XT_HZ_MATRIX;

        return wh;
    }

    private void InitSJ(){
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

        } while (!CurrentSJ.Contains("，") || CurrentSJ.Length > XT_HZ_MATRIX * XT_HZ_MATRIX);//不能超过XT_HZ_MATRIX*XT_HZ_MATRIX

        CurrentShiCiID = int.Parse(tsscData[0]);

       //Debug.Log(CurrentSJ);
    }
    //显示诗句结果序列
    public GameObject _RSJTextPrefab;
    public GameObject _ResultSJ;
    public Image _ResultSJBG;
    public Transform _SJ;
    private void DoResultSJAni(bool right){
        DoTipInfoAni("出自：" + GetInfo(_TiMuFW, CurrentShiCiID));

        RectTransform rtRSJ = _ResultSJ.GetComponentInChildren<RectTransform>();
        RectTransform rtSJ = _RSJTextPrefab.GetComponentInChildren<RectTransform>();

        Sequence resultSJAni = DOTween.Sequence();
        _ResultSJ.SetActive(true);
        _ResultSJBG.color = new Color(_ResultSJBG.color.r, _ResultSJBG.color.g, _ResultSJBG.color.b,0.0f);
        resultSJAni.Join(_ResultSJBG.DOFade(100.0f/255,0.0f));
        for (int i = 0; i < CurrentSJ.Length;i++){
            GameObject Hz = Instantiate(_RSJTextPrefab, _SJ) as GameObject;
            Text hzText = Hz.GetComponent<Text>();
            hzText.text = "" + CurrentSJ[i];
            Hz.SetActive(true);
            hzText.color = _BgLightColor;
            Hz.transform.position = _HZList[_CurrentSJIndex[i]].transform.position;

            //将原字设置成透明
            _HZList[_CurrentSJIndex[i]].GetComponentInChildren<Text>().color = new Color(0f,0f,0f,0f);

            float offset = 20f;
            float w = (rtRSJ.rect.width - offset) / CurrentSJ.Length;
            
            float s = w / rtSJ.rect.width;
            float x = offset / 2  + w * i;
            //计算出该汉字对应的诗句位置
            resultSJAni.Join(Hz.transform.DOLocalMove(new Vector3(x - rtRSJ.rect.width/2 + w / 2, 0.0f, 0.0f),1.0f))
                       .Join(Hz.transform.DOScale(s,0.5f));
        }
        resultSJAni
            .SetEase(Ease.InSine)
            .OnComplete(()=>{
                DoResult(right);
            });
    }

    //划选结束
    public void OnPencilSubmit(){
        OnPencilSubmit(false);
    }
    private void OnPencilSubmit(bool timeout)
    {
        //取消自动提示汉字
        if (timeout)
        {
            ShowChuangGuanInfo(false);

            _Pencil.SetCanShowPencil(false);
            _Pencil.GetComponent<Image>().color = new Color(_BgLightColor.r, _BgLightColor.g, _BgLightColor.b, 0.0f);

            Sequence resultAni = DOTween.Sequence();
            for (int i = 0; i < _HZList.Count; i++)
            {
                XTHZ xTHZ = _HZList[i].GetComponentInChildren<XTHZ>();
                resultAni.Join(xTHZ.DoHZFade(0.0f, 0.5f))
                         .Join(xTHZ.DoHZIDFade(0.0f, 0.5f))
                         .Join(xTHZ.DoHZIDBGFade(0.0f, 0.5f));
            }
            resultAni.AppendInterval(0.2f);
            for (int i = 0; i < _CurrentSJIndex.Count; i++)
            {
                XTHZ xTHZ = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<XTHZ>();
                resultAni.Append(xTHZ.DoHZColor(_BgLightColor, 5.0f / _CurrentSJIndex.Count));
            }

            resultAni.OnComplete(() => DoResultSJAni(false));
        }
        else
        {
            //有选择汉字，则判断是否为正确的诗句
            if (_CurrentSelectSJIndex.Count > 0)
            {
                //判断是否连通，如果不连通，视为取消选择
                if (CheckIfConnect())
                {
                    //包含标点符号，整句需要匹配
                    bool right = CheckIfRight();

                    if (!right)
                    {
                        for (int i = _CurrentSelectSJIndex.Count - 1; i >= 0; i--)
                        {
                            XTHZ xtHZ2 = _HZList[_CurrentSelectSJIndex[i]].GetComponent<XTHZ>();
                            xtHZ2.SetIsSelect(false);
                            _CurrentSelectSJIndex.RemoveAt(i);
                        }

                        _CurrentSelectSJIndex.Clear();
                    }
                    else
                    {
                        _CantShowDaAn = true;
                        OnZuoTiTimer(false);//停止

                        _Pencil.SetCanShowPencil(false);

                        Sequence resultAni = DOTween.Sequence();
                        resultAni.AppendInterval(0.5f);
                        for (int i = 0; i < _HZList.Count; i++)
                        {
                            //kill all 
                            XTHZ xtHZ = _HZList[i].GetComponent<XTHZ>();
                            xtHZ.RestAni();
                            if (!_CurrentSelectSJIndex.Contains(i))
                            {
                                XTHZ xTHZ = _HZList[i].GetComponentInChildren<XTHZ>();
                                resultAni.Join(xTHZ.DoHZFade(0.0f, 0.5f));
                            }
                        }
                        resultAni.AppendInterval(1.0f);
                        if (right)
                        {
                            //保存分数
                            SaveScore();

                            ShowChuangGuanInfo(true);

                            //resultAni
                            for (int i = 0; i < _CurrentSJIndex.Count; i++)
                            {
                                XTHZ xTHZ = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<XTHZ>();

                                resultAni.Join(xTHZ.DoHZFade(0.0f, 0.5f))
                                         .Join(xTHZ.DoHZIDFade(0.0f, 0.5f))
                                         .Join(xTHZ.DoHZIDBGFade(0.0f, 0.5f));
                            }
                            resultAni.AppendInterval(0.2f);
                            for (int i = 0; i < _CurrentSJIndex.Count; i++)
                            {
                                XTHZ xTHZ = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<XTHZ>();
                                resultAni.Append(xTHZ.DoHZFade(1.0f, 5.0f / _CurrentSJIndex.Count));
                            }
                        }
                        else
                        {
                            ShowChuangGuanInfo(false);

                            for (int i = 0; i < _CurrentSelectSJIndex.Count; i++)
                            {
                                XTHZ xTHZ = _HZList[_CurrentSelectSJIndex[i]].GetComponentInChildren<XTHZ>();

                                if (!_CurrentSJIndex.Contains(_CurrentSelectSJIndex[i]))
                                {
                                    resultAni.Join(xTHZ.DoHZColor(_BgDarkColor, 0.5f))
                                             .Join(xTHZ.DoHZIDFade(0.0f, 0.5f))
                                             .Join(xTHZ.DoHZIDBGFade(0.0f, 0.5f));
                                }
                                else
                                {
                                    resultAni.Join(xTHZ.DoHZFade(0.0f, 0.5f))
                                             .Join(xTHZ.DoHZIDFade(0.0f, 0.5f))
                                             .Join(xTHZ.DoHZIDBGFade(0.0f, 0.5f)); ;
                                }
                            }
                            resultAni.AppendInterval(0.2f);
                            for (int i = 0; i < _CurrentSJIndex.Count; i++)
                            {
                                XTHZ xTHZ = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<XTHZ>();
                                resultAni.Append(xTHZ.DoHZColor(_BgLightColor, 5.0f / _CurrentSJIndex.Count));
                            }
                        }

                        resultAni.OnComplete(() => DoResultSJAni(right));
                    }
                }
                else
                {
                    // 给出提示，划选不连通，请重新划选

                    string content = "";
                    if (_CurrentSelectSJIndex.Count < 4)
                    {
                        content = "所划选字数过少，构不成诗句，请重新划选。";
                    }
                    else
                    {
                        content = "所划选诗句不连通，存在跳选，请重新划选。";
                    }

                    for (int i = _CurrentSelectSJIndex.Count - 1; i >= 0; i--)
                    {
                        XTHZ xtHZ2 = _HZList[_CurrentSelectSJIndex[i]].GetComponent<XTHZ>();
                        xtHZ2.SetIsSelect(false);
                        _CurrentSelectSJIndex.RemoveAt(i);
                    }

                    _CurrentSelectSJIndex.Clear();

                    //显示提示
                    ShowToast(content);
                }
            }
            else
            {
                //提示
                int ztt = PXZTTime - _CurrentPassTime;
                if(ztt < PXZTTime / 2 && ztt > (int)Study.ZTTimeType.ZT_10S / 2/*剩余时间必须要大于5s，否则没有意义了*/)
                {

                    int cnt = GetCurrentTipHZCnt();
                    if (cnt <= 0)
                    {
                        DoTipInfoAni("注意：当前闯关数大于" + Define.XT_NO_TIP + "，不再提示");
                    }
                    else
                    {
                        DoTipInfoAni("提示：随着闯关数增加，给予提示的字减少，且得分打折", DoTip);
                    }
                }
            }
        }
    }
    //铅笔选中汉字 - 保存选中列表，有次序
    public void OnPencilSelect(bool isSelect,int index){
        StopFirstTip();
        if (index >= 0 && index < _HZList.Count){
            XTHZ xtHZ = _HZList[index].GetComponent<XTHZ>();
            //如果是选
            if(isSelect){
                _CurrentSelectSJIndex.Add(index);
                xtHZ.SetIsSelect(true);
                xtHZ.UpdateSelectID(_CurrentSelectSJIndex.Count);//更新划选次序id
            }
            else{
                //如果是取消选中，则取消该点后面的所有汉字
                if(_CurrentSelectSJIndex.Count >= 2){

                    int startIndex = -1;
                    for (int i = 0; i < _CurrentSelectSJIndex.Count; i++)
                    {
                        if(_CurrentSelectSJIndex[i] == index){
                            startIndex = i;
                        }
                    }

                    for (int i = _CurrentSelectSJIndex.Count - 1; i > startIndex; i--)
                    {
                        XTHZ xtHZ2 = _HZList[_CurrentSelectSJIndex[i]].GetComponent<XTHZ>();
                        xtHZ2.SetIsSelect(false);
                        _CurrentSelectSJIndex.RemoveAt(i);
                    }
                    //
                    if (_CurrentSelectSJIndex.Count == 1)
                    {
                        //仅剩下一个的时候，全部取消选中
                        xtHZ.SetIsSelect(false);
                        _CurrentSelectSJIndex.Clear();
                    }
                }
                else{
                    //仅剩下一个
                    if(_CurrentSelectSJIndex.Count == 1){
                        xtHZ.SetIsSelect(false);
                        _CurrentSelectSJIndex.Clear();
                    }else{
                        //不可能出现
                    }
                }
            }
        }
    }
    private void ShowChuangGuanInfo(bool right){

        string tips = "提示：题目难度会随着闯关数增加哟";
        string content = "恭喜闯关成功！";
        int r = HZManager.GetInstance().GenerateRandomInt(0, 100);
        if(r < 20){
            tips = "提示：划选时<b>划回</b>上一个字视为取消划选该字";
        }
        else if (r >= 20 && r < 40)
        {
            tips = "提示：划选时直接<b>划回起始字</b>视为全部取消划选";
        }
        else if (r >= 40 && r < 60)
        {
            tips = "提示：划选<b>少于4个字</b>时视为无效划选";
        }
        else if (r >= 60 && r < 80)
        {
            tips = "提示：划选时左上角<b>角标</b>为字在诗句中的顺序";
        }else{
            //
        }

        if (right)
        {
            if (_ChuangGuan == Define.XT_NO_TIP / 2)
            {
                tips = "注意：难度提升，不再提示<b>起始字</b>";
            }
            else if(_ChuangGuan == Define.XT_NO_TIP)
            {
                tips = "注意：难度提升，不再提示<b>划选顺序</b>";
            }
            else if (_ChuangGuan == 100)
            {
                tips = "注意：难度提升，将逐渐插入<b>干扰字</b>";
            }
            else if (_ChuangGuan == 400)
            {
                tips = "注意：难度提升，将逐渐减少<b>挑战时间</b>";
            }else if(_ChuangGuan >= 1000)
            {
                tips = "提示：你太厉害了，居然达到了这里";
            }

            content = "恭喜闯关成功！请再细读一遍原句吧～";
        }
        else{
            tips = "提示：仔细观察<b>标点位置</b>以及<b>字的连通性</b>，更容易划出正确答案";

            content = "好可惜，闯关失败。请一定要熟记原句哟～";
        }

        DoTipInfoAni(tips);

        //显示提示
        ShowToast(content, 2.0f,1.0f);
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

    private void DoResult(bool right)
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

        RectTransform rtRSJ = _ResultSJ.GetComponentInChildren<RectTransform>();
        RectTransform rtScore = _score.GetComponentInChildren<RectTransform>();
        float y = rtScore.transform.localPosition.y - rtScore.rect.height / 2 - rtRSJ.rect.height / 2;
        _score.SetActive(true);
        _score.transform.localScale = new Vector3(0.0f,0.0f,0.0f);
        Sequence resultSJAni2 = DOTween.Sequence();
        // 显示得分面板
        resultSJAni2
            .Append(_ResultSJ.transform.DOLocalMoveY(y, 1.5f))
            .Join(_ResultSJ.transform.DOScale(rtScore.rect.width / rtRSJ.rect.width, 1.5f))
            .Join(_score.transform.DOScale(1.0f,1.5f))
            .SetEase(Ease.OutBounce).OnComplete(()=>{
                //
                DoScoreAni(right);
            });
    }

    private void DoScoreAni(bool right)
    {
        if (right)
        {
            Sequence mySequence = DOTween.Sequence();
            mySequence.SetId("DoScoreClockAni" + GetStudyName());
            _scorePlusLight.gameObject.SetActive(true);
            mySequence
                .Append(_scorePlusLight.DOFade(0.0f, 0.0f))
                .Join(_scorePlusLight.DOFade(1.0f, 0.2f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    //+号动画，结束后执行分数+动画
                    //本题得分动画从0->实际分数
                    //总分从原来分数到+本题分数
                    //开启进入下一题倒计时，如果是错误，则返回开始界面
                    Sequence mySequence2 = DOTween.Sequence();
                    mySequence2
                    .Append(_scorePlusLight.transform.DOMoveY(_totalScoreText.transform.position.y, 0.4f))
                    .Append(_scorePlusLight.DOFade(0.0f, 0.3f))
                    .Join(_scorePlusLight.transform.DOScale(2.0f, 0.3f))
                    .SetEase(Ease.InSine)
                    .OnComplete(() =>
                    {
                        _totalScoreText.DOText("" + _TotalScore, 0.5f);
                    });

                    //执行倒计时
                    DoScoreClockAni(true);
                });
        }
        else
        {
            //bug fix
            _scorePlusLight.gameObject.SetActive(false);
            //执行倒计时
            DoScoreClockAni(true);

            //此时可以直接返回
            _CanExist = true;
            _makeXJBtn.SetActive(true);
            _makeXJBtn.transform.DOShakePosition(1.0f, 5);
            //ShowMask(false);//允许点击
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
            _score.SetActive(false);
            _ResultSJ.SetActive(false);
            _ResultSJ.transform.position = new Vector3(_ResultSJ.transform.position.x,
                                                       _score.transform.position.y, 
                                                       _ResultSJ.transform.position.z);
            _ResultSJ.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
            Text[] RSJ = _SJ.GetComponentsInChildren<Text>();
            foreach(var sj in RSJ)
            {
                Destroy(sj);//删除所有节点
            }
            DestroyObj(_HZList);

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

                _Pencil.DisablePencil();

                Invoke("GameEnd", 0.1f);
            }
        }
    }
    public void OnMakeXinJianClick()
    {
        _score.SetActive(false);
        _ResultSJ.SetActive(false);
        _ResultSJ.transform.position = new Vector3(_ResultSJ.transform.position.x,
                                                   _score.transform.position.y,
                                                   _ResultSJ.transform.position.z);
        _ResultSJ.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Text[] RSJ = _SJ.GetComponentsInChildren<Text>();
        foreach (var sj in RSJ)
        {
            Destroy(sj);//删除所有节点
        }
        DestroyObj(_HZList);

        DoKillAllAni();
        StopAllClock();
        _Pencil.DisablePencil();
        _CanExist = false;

        OnMakeXinJian(CurrentShiCiID, CurrentSJ, GetShiCiType(_TiMuFW));
    }

    public Text _TipInfo;
    public GameObject _TopMenu;
    public override void OnZuoTiTimer(bool start)
    {
        if (!start)
        {
            _CurrentPassTime = 0;
            CancelInvoke("DoZuoTiTimeClock");


            return;
        }

        Sequence resultAni = DOTween.Sequence();
        for (int i = 0; i < _HZList.Count; i++)
        {
            resultAni.Join(_HZList[i].GetComponentInChildren<Text>().DOFade(1.0f, 0.5f));
        }

        _Pencil.GetComponent<Image>().color = new Color(_BgLightColor.r, _BgLightColor.g, _BgLightColor.b, 0.0f);

        resultAni.Join(_TipInfo.DOFade(1.0f,0.5f))
                 .Join(_TopMenu.transform.DOScale(1.0f, 0.5f));

        resultAni.OnComplete(()=>{
            _Pencil.gameObject.SetActive(true);
            _Pencil.SetCanShowPencil(true);
        });

        ////------定时器---------
        _CurrentPassTime = 0;

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
            //当1/4已经耗时
            if (_CurrentPassTime == 5)//5s钟后提示
            {
                if (_CurrentSelectSJIndex.Count == 0)
                {
                    if (_ChuangGuan <= Define.XT_NO_TIP / 2)
                    {
                        DoTipInfoAni("注意：诗句<b>起始字</b>已提示，稍后仍有其他提示", DoFirstTip);
                    }
                    else
                    {
                        DoTipInfoAni("注意：闯关超过" + Define.XT_NO_TIP / 2 + "关，不再提示起始字，稍后仍有部分提示");
                    }
                }
            }
        }
        else if (ztt <= half && ztt > 0)
        {
            //后半时开始计算耗时
            _ScoreTime += 1;
            if (_ScoreTime >= half)
            {
                _ScoreTime = half;
            }

            if (ztt == half){
            
                _TipInfo.DOFade(0.0f,0.5f)
                        .OnComplete(()=>{
                            if(_CurrentSelectSJIndex.Count == 0){
                                DoTip();
                            }
                            else{
                                _TipInfo.text = "注意：若需提示，请取消所有选中并松开(原路划回)";
                            }
                            
                            _TipInfo.DOFade(1.0f,0.5f);
                        });

                //此时应该取消按压状态
                //最后5s为最后答题时间
            }
        }
        else
        {
            //
            _ScoreTime = half;
            _CurrentPassTime = PXZTTime;
        }
    }

    private bool _HasShowTip;
    public Image _TipBrush;
    [System.Serializable] public class OnShowClickWaveEvent : UnityEvent<Transform> { }
    public OnShowClickWaveEvent OnShowClickWave;
    private void StopFirstTip(){
        if(DOTween.IsTweening("DoFirstTip")){
            DOTween.Kill("DoFirstTip");//停止
            _TipBrush.gameObject.SetActive(false);
        }
    }
    private void StopTip(){
        if (DOTween.IsTweening("DoTip"))
        {
            DOTween.Kill("DoTip");//停止
            _TipBrush.gameObject.SetActive(false);
            _Pencil.SetCanShowPencil(true);
        }
    }
    private void DoFirstTip(){

        //第一个字的提示，时间达到1/4时间时，超过25关不再提示第一个字
        XTHZ xTHZ = _HZList[_CurrentSJIndex[0]].GetComponent<XTHZ>();
        Text hzText = _HZList[_CurrentSJIndex[0]].GetComponentInChildren<Text>();
        Vector3 pos = new Vector3(hzText.transform.position.x + 50, hzText.transform.position.y + 50, 0.0f);

        Color c = _BgLightColor;
        _TipBrush.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.SetId("DoFirstTip");
        _TipBrush.color = new Color(c.r, c.g, c.b, 0.0f);
        _TipBrush.transform.position = pos;
        sequence.Append(_TipBrush.DOFade(1.0f, 0.5f));

        int x = _CurrentSJIndex[0] % XT_HZ_MATRIX;
        int y = _CurrentSJIndex[0] / XT_HZ_MATRIX;
        List<int> moving = new List<int>();
        //上
        if(y - 1 >= 0)
        {
            moving.Add((y-1) * XT_HZ_MATRIX + x);
        }
        //右上
        if(y - 1 >= 0 && x + 1 < XT_HZ_MATRIX)
        {
            moving.Add((y-1) * XT_HZ_MATRIX + x+1);
        }

        //右
        if (x + 1 < XT_HZ_MATRIX)
        {
            moving.Add(y * XT_HZ_MATRIX + x + 1);
        }

        //右下
        if (y + 1 < XT_HZ_MATRIX && x + 1 < XT_HZ_MATRIX)
        {
            moving.Add((y + 1) * XT_HZ_MATRIX + x + 1);
        }
        //下
        if (y + 1 < XT_HZ_MATRIX)
        {
            moving.Add((y+1) * XT_HZ_MATRIX + x);
        }

        // 左下
        if (y + 1 < XT_HZ_MATRIX && x - 1 >= 0)
        {
            moving.Add((y + 1) * XT_HZ_MATRIX + x - 1);
        }

        //左
        if ( x - 1 >= 0)
        {
            moving.Add(y * XT_HZ_MATRIX + x-1);
        }

        //左上
        if (x - 1 >= 0 && y - 1 >=0)
        {
            moving.Add((y - 1)* XT_HZ_MATRIX + x - 1);
        }

        float speed = 0.5f;
        for (int p = 0; p < moving.Count;p++){
            Text hzText2 = _HZList[moving[p]].GetComponentInChildren<Text>();
            if (hzText2.text == "") continue;
            Vector3 pos2 = new Vector3(hzText2.transform.position.x + 50, hzText2.transform.position.y + 50, 0.0f);
            sequence.Append(_TipBrush.transform.DOMove(pos2, speed))
                    .Join(hzText2.transform.DOScale(1.2f, speed))
                    .Append(_TipBrush.transform.DOMove(pos, speed))
                    .Join(hzText2.transform.DOScale(1.0f, speed));
        }

        sequence.Append(_TipBrush.DOFade(0.0f, 0.2f))
                .OnComplete(()=>{
                    _TipBrush.gameObject.SetActive(false);
                });

        OnShowClickWave.Invoke(hzText.transform);
    }
    private void DoTip(){
        int cnt = GetCurrentTipHZCnt();

        _HasShowTip = true;
        //动画执行部分提示字，此时不能操作
        _Pencil.SetCanShowPencil(false);
        _TipBrush.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.SetId("DoTip");
        Color c = _BgLightColor;
        float speed = 5.0f;
        for (int i = 0; i < cnt; i++)
        {
            XTHZ xTHZ = _HZList[_CurrentSJIndex[i]].GetComponent<XTHZ>();
            _CurrentSelectSJIndex.Add(_CurrentSJIndex[i]);
            xTHZ.SetIsSelect(true, false);
            xTHZ.UpdateSelectID(_CurrentSelectSJIndex.Count);//更新划选次序id

            Text hzText = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<Text>();
            Vector3 pos = new Vector3(hzText.transform.position.x + 50, hzText.transform.position.y + 50, 0.0f);
            sequence.Append(xTHZ.DoHZColor(c, speed / cnt))
                    .Join(xTHZ.DoHZIDFade(1.0f, speed / cnt))
                    .Join(xTHZ.DoHZIDBGFade(25/255.0f, speed / cnt))
                    .Join(hzText.transform.DOShakeScale(speed / cnt,0.2f));
            if(i == 0){
                _TipBrush.color = new Color(c.r, c.g, c.b,0.0f);
                _TipBrush.transform.position = pos;
                sequence.Join(_TipBrush.DOFade(1.0f, speed / cnt))
                        .Join(_TipBrush.transform.DOShakePosition(speed / cnt));
                       
            }
            else{

                sequence.Join(_TipBrush.transform.DOMove(pos, speed / cnt));
                if (i == cnt - 1)
                {
                    sequence.Join(_TipBrush.DOFade(0.0f, speed / cnt));
                }
            }


            //执行动画，结束时，方可点击
        }
        sequence.OnComplete(() => {
            _TipBrush.gameObject.SetActive(false);
            _Pencil.SetCanShowPencil(true);

            ShowToast("已提示部分诗句连接，请划选剩余部分。");
        });
    }

    private void DoTipInfoAni(string info,Action cb = null){
        _TipInfo.DOFade(0.0f, 0.5f)
        .OnComplete(() => {
            _TipInfo.text = info;
            if(cb != null){
                cb();
            }
            _TipInfo.DOFade(1.0f, 0.5f);
        });
    }

    private float _SearchDifficult = 0.0f;
    private int _DisturbCnt = 0;//干扰字个数
    //根据通过数，更新阅读题目的时间以及字掉落速度
    private void ResetZTTimeAndSpeed()
    {
        // 开始插入干扰字
        if (_ChuangGuan > 100) //
        {
            int blank = XT_HZ_MATRIX * XT_HZ_MATRIX - CurrentSJ.Length;

            if (_ChuangGuan > 400) {
                if (_ChuangGuan > 1000) return;
                //600 - 1000 之间，缩短思考时间
                PXZTTime -= (_ChuangGuan - 400) / 15;//每15关减1s

                if (PXZTTime < (int)Study.ZTTimeType.ZT_20S)
                {
                    PXZTTime = (int)Study.ZTTimeType.ZT_20S;// 最少20s
                }

                _DisturbCnt = blank;
                _SearchDifficult = 0.25f;//固定在0.25
            }
            else{
                //超过100关时，开始插入干扰字，用比例表达
                //100 - 400
                _DisturbCnt = (int)(blank * (_ChuangGuan - 100) / 300.0f);//也就是说400以后才是插入满的
                if (_DisturbCnt + CurrentSJ.Length > XT_HZ_MATRIX * XT_HZ_MATRIX)
                {
                    _DisturbCnt = blank;
                }

                //当大于100关时，开始减少_SearchDifficult，使得更分散，插入干扰字，难度变大
                //最低降到1.0 - 0.25f
                _SearchDifficult = 1.0f - ((_ChuangGuan - 100) / 300.0f) * 0.75f;//0-0.75
            }

        }
        else{
            //搜索难度控制出现在100关以内，超出100关，均才有最大难度搜索
            _SearchDifficult = _ChuangGuan / 100.0f;
        }
    }
    private int GetCurrentTipHZCnt(){

        int cnt = 0;

        cnt = (int)(CurrentSJ.Length * GetTipHZPercent());//至少20%不能提示

        cnt = cnt < 0 ? 0 : cnt;
        return cnt;
    }

    private float GetTipHZPercent(){
        float p = 0.0f;

        if (_ChuangGuan > Define.XT_NO_TIP) //  超过50关，不再有划选提示，这里起始也可以设置购买点
        {
            return p;
        }

        return 0.9f - 1.0f * _ChuangGuan / Define.XT_NO_TIP * 0.9f;
    }

    protected override int GetCurrentScore()
    {
        //耗时后得分
        int baseScore = (int)((1 + _ChuangGuan / 20) * Define.BASE_SCORE_10S);
        int cs = (int)(baseScore * (1 - 1.0f * _ScoreTime/ (PXZTTime/2)));//前30s答对，得到满分，超过后，根据已经提示的字扣分

        if(_HasShowTip){
            cs = (int)(cs * (1.0f - GetTipHZPercent()));
        }

        if (cs <= 0) cs = 1;//最低给予1分

        // 额外加分
        if (_TiMuFW == eTiMuFanWei.All)
        {
            cs += Define.PLUS_SCORE_ALL;
        }
        else if (_TiMuFW == eTiMuFanWei.TangShi)
        {
            cs += Define.PLUS_SCORE_TANGSHI;
        }
        else if (_TiMuFW == eTiMuFanWei.SongCi)
        {
            cs += Define.PLUS_SCORE_SONGCI;
        }
        else if (_TiMuFW == eTiMuFanWei.GuShi)
        {
            cs += Define.PLUS_SCORE_GUSHI;
        }
        else if (_TiMuFW == eTiMuFanWei.ShiJing)
        {
            cs += Define.PLUS_SCORE_SHIJING;//原则上来说，排序模式的时候，诗经类型不能增加2分
        }

        //连续闯关加分
        cs += _ChuangGuan;

        return cs;
    }

    public override void AbandonGame(Action cb = null)
    {
        _score.SetActive(false);
        _ResultSJ.SetActive(false);
        _ResultSJ.transform.position = new Vector3(_ResultSJ.transform.position.x,
                                                   _score.transform.position.y,
                                                   _ResultSJ.transform.position.z);
        _ResultSJ.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Text[] RSJ = _SJ.GetComponentsInChildren<Text>();
        foreach (var sj in RSJ)
        {
            Destroy(sj);//删除所有节点
        }
        DestroyObj(_HZList);

        _CanExist = false;
        DoKillAllAni();
        StopAllClock();

        _Pencil.DisablePencil();
        //这里可以稍微延时，防止闪屏错觉
        Invoke("GameEnd", 0.1f);
    }

    public override void ExistGame()
    {
        _score.transform.localScale = new Vector3(0f, 0f, 1f);
        AbandonGame();
    }

    protected override void StopAllClock()
    {
        DoScoreClockAni(false, true);
        OnZuoTiTimer(false);
        DoFailClockAni(false, true);
    }

    protected override string GetStudyName()
    {
        return gameObject.name;
    }
    #region 选填题

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

        StopFirstTip();
        StopTip();
        //这里显示答案，实际是直接跳过该题功能，即直接执行划选正确，提交
        OnZuoTiTimer(false);//停止

        //保存分数
        SaveScore();

        _Pencil.SetCanShowPencil(false);
        Sequence resultAni = DOTween.Sequence();
        for (int i = 0; i < _HZList.Count; i++)
        {
            resultAni.Join(_HZList[i].GetComponentInChildren<Text>().DOFade(0.0f, 0.5f));
            XTHZ xtHZ = _HZList[i].GetComponent<XTHZ>();
            xtHZ.ResetHZ(true);
        }
        resultAni.AppendInterval(0.5f)
                 .AppendCallback(()=>{
                     for (int i = 0; i < _HZList.Count; i++)
                     {
                         XTHZ xtHZ = _HZList[i].GetComponent<XTHZ>();
                         xtHZ.ResetHZ(false);
                     }
                 });

        for (int i = 0; i < _CurrentSJIndex.Count; i++)
        {
            XTHZ xtHZ = _HZList[_CurrentSJIndex[i]].GetComponent<XTHZ>();
            resultAni.Append(xtHZ.DoHZFade(1.0f, 5.0f / _CurrentSJIndex.Count));
        }

        resultAni.OnComplete(() => DoResultSJAni(true));
    }
    #endregion


    //-----------------内部使用接口-----------------------
    //---------------------诗句路径搜索-----------------------------------
    private int X = 7;
    private int Y = 7;
    private int N = 5;
    private Action<bool, int> SearchingCb = null;//参数：当前节点是否合法

    /// <summary>
    /// 自动搜索一定区域内合法的笔画/字根组装序列
    /// </summary>
    /// <param name="x">搜索起始点坐标x</param>
    /// <param name="y">搜索起始点坐标y</param>
    /// <param name="n">搜索半径</param>
    /// <param name="fininshCallback">搜索完成回调，向ui主线程返回搜索结果</param>
    /// <param name="searchingCallback">搜索中回调，如果搜索复杂度较大，可通过该回调给予提示</param>
    public void SearchSJPath(int x, int y, int n,
                        Action<List<int>> fininshCallback,
                        Action<bool, int> searchingCallback = null)
    {
        //初始化参数
        X = x;
        Y = y;
        N = n;

        SearchingCb = searchingCallback;

        List<int> xy = new List<int>();
        for (int i = 0; i < XT_HZ_MATRIX * XT_HZ_MATRIX; i++)
        {
            xy.Add(0);//全部设置为未搜索
        }

        //将起始字点设置为已经搜索
        xy[Y * XT_HZ_MATRIX + X] = 1;
        List<int> searched = new List<int>
        {
            Y * XT_HZ_MATRIX + X
        };

        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                if (!DoSearch(xy, null, searched, X, Y))
                {
                    //找不到合法序列，清空searched列表
                    searched.Clear();
                }

                Loom.QueueOnMainThread((param) =>
                {
                    if (fininshCallback != null)
                    {
                        fininshCallback(searched);//返回合法的索引id，即对应到界面节点
                    }
                }, null);
            });

            thread.Start();
        });
    }

    struct SD
    {
        public int index;
        public int priority;
    }

    private List<int> GetNextSearch(List<int> xy, int x, int y, List<int> path)
    {
        List<int> searching = new List<int>();
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            if (i <= X - N || i >= X + N) continue;

            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界
                if (j <= Y - N || j >= Y + N) continue;//超出搜索半径

                if (i == x && j == y) continue;//同一个点

                if (xy[j * XT_HZ_MATRIX + i] == 0)
                {
                    searching.Add(j * XT_HZ_MATRIX + i);
                }
            }
        }

        //可搜索的点少于2个，直接返回该点
        if(searching.Count <= 1){
            return searching;
        }

        //洗牌搜索序列，防止固定的方向
        ShuffleSJList(searching);

        //优选搜索周围空白的字最多的字
        List<SD> sds = new List<SD>();
        for (int i = 0; i < searching.Count;i++){
            SD sd;
            sd.index = searching[i];
            sd.priority = GetHZPriority(searching[i], xy,path);
            sds.Add(sd);
        }

        sds.Sort((a,b)=>{
            return a.priority - b.priority;//升排列，也就是周围字最多，最难的路线
        });

        searching.Clear();
        foreach(var sd in sds){
            searching.Add(sd.index);
        }


        int sindex = (int)((1 - _SearchDifficult) * searching.Count);//0=< 1-_SearchDifficult <1
        if (sindex >= searching.Count - 1) sindex = searching.Count - 1;

        List<int> searching1 = new List<int>();
        List<int> searching2 = new List<int>();

        for (int i = 0; i < sindex;i++)
        {
            searching1.Add(searching[i]);
        }

        for (int i = sindex; i < searching.Count; i++)
        {
            searching2.Add(searching[i]);
        }

        searching.Clear();
        searching.AddRange(searching2);// 优先往简单的方向搜索
        searching1.Reverse();
        searching.AddRange(searching1);//再往越来越难的方向搜索

        return searching;
    }

    // 获取文字的搜索权重
    private int GetHZPriority(int index, List<int> xy, List<int> path)
    {
        int p = 0;
        int p1 = 0;
        int p2 = 0;

        int x = index % XT_HZ_MATRIX;
        int y = index / XT_HZ_MATRIX;


        //使用距离加权难度系数
        for (int i = 0; i < path.Count;i++){
            int xx = path[i] % XT_HZ_MATRIX;
            int yy = path[i] / XT_HZ_MATRIX;
            p1 += (int)Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
        }

        //使用文字密集成度加权难度系数
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界

                if (i == x && j == y) continue;//同一个点

                if (xy[j * XT_HZ_MATRIX + i] == 0)
                {
                    p2++;
                }
            }
        }


        //100关以内，适当降低p2的比重，否则会出现往中间集中，难度过高
        //达到插入干扰字以后，搜索权重的比例，按照干扰字的增多降低，直到为0
        //这个控制是为当需要插入字时，诗句更分散，难度更高
        //否则当搜索难度最高时，诗句全部靠在一起，插入干扰字失去意义
        p2 = (int)(p2 * (0.5f + _SearchDifficult * 0.5f));//0.625-1.0

        // 需要找到更加科学的 两个参数的 比例或者公式，这里是简单相加
        p = p1 + p2;

        return p;
    }

    //文字密集成度
    private int GetHZPriority(string[] xy, int index)
    {
        int p = 0;

        int x = index % XT_HZ_MATRIX;
        int y = index / XT_HZ_MATRIX;

        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= XT_HZ_MATRIX) continue;
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= XT_HZ_MATRIX) continue;// 超出边界

                if (i == x && j == y) continue;//同一个点

                ////这里如果有字则增加权重，而不是空
                /// 这样可以保证插入字干扰性更大，否则在边界时误差太大
                if (xy[j * XT_HZ_MATRIX + i] != "")
                {
                    p++;
                }
            }
        }

        return p;
    }
    private bool DoSearch(List<int> xy, List<int> searching, List<int> searched, int x, int y)
    {
        //根节点
        if (searching == null)
        {
            return DoSearch(xy, GetNextSearch(xy, x, y, searched), searched, x, y);
        }

        //检测是否搜索完成
        if (searched.Count == CurrentSJ.Length)
        {
            return true;
        }

        //没有可以搜索的，终止//已经到达死角或者终结
        if (searching.Count == 0 || searched.Count >= N * N /*范围内全部节点*/)
        {
            return false;
        }

        //对当前的进行检测
        foreach (var sc in searching)
        {
            int xx = sc % XT_HZ_MATRIX;
            int yy = sc / XT_HZ_MATRIX;

            searched.Add(sc);
            xy[sc] = 1;

            //这里做一些视觉效果
            Loom.QueueOnMainThread((param) =>
            {
                if (SearchingCb != null)
                {

                    SearchingCb(true, sc);
                }
            }, null);

            //System.Threading.Thread.Sleep(1000);

            if (DoSearch(xy, GetNextSearch(xy, xx, yy, searched), searched, xx, yy))
            {
                return true;
            }
            else
            {
                //这个节点不合法，后退到前一个
                xy[sc] = 0;
                searched.RemoveAt(searched.Count - 1);

                Loom.QueueOnMainThread((param) =>
                {
                    if (SearchingCb != null)
                    {
                        SearchingCb(false, sc);
                    }
                }, null);
            }
        }

        return false;
    }

    //
    private bool CheckIfRight(){
        bool right = true;
        if (_CurrentSelectSJIndex.Count == _CurrentSJIndex.Count)
        {
            for (int i = 0; i < _CurrentSelectSJIndex.Count; i++)
            {
                //相同字的情况需要特殊处理
                if (_CurrentSelectSJIndex[i] != _CurrentSJIndex[i])
                {
                    string txt1 = _HZList[_CurrentSelectSJIndex[i]].GetComponentInChildren<Text>().text;
                    string txt2 = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<Text>().text;
                    if (txt1.Equals(txt2))
                    {
                        //如果字相同，说明同样是正确的，这里将原始正确索引更新为选择的索引
                        _CurrentSJIndex[i] = _CurrentSelectSJIndex[i];
                    }
                    else
                    {
                        right = false;
                        break;
                    }
                }
            }
        }
        else
        {
            right = false;
        }

        return right;
    }

    //检测序列是否连通
    public bool CheckIfConnect(){
        bool conn = true;

        //原则上小于4个字，是无法构成诗句的(包含标点)，这里认为取消划选，从新选中
        if (_CurrentSelectSJIndex.Count >= 4){
            for (int i = 0; i <= _CurrentSelectSJIndex.Count - 2; i++)
            {
                if(!CheckIfConnect(_CurrentSelectSJIndex[i],_CurrentSelectSJIndex[i+1])){
                    conn = false;
                    break;
                }
            }
        }else{
            conn = false;
        }

        return conn;
    }

    public bool CheckIfConnect(int p1,int p2){
        bool conn = false;

        int col1 = p1 % XT_HZ_MATRIX;
        int row1 = p1 / XT_HZ_MATRIX;

        int col2 = p2 % XT_HZ_MATRIX;
        int row2 = p2 / XT_HZ_MATRIX;

        if((col1 == col2 && row1 - 1 == row2)
           || (col1+1 == col2 && row1 - 1 == row2)
          || (col1+1 == col2 && row1 == row2)
          || (col1+1 == col2 && row1 + 1 == row2)
          || (col1 == col2 && row1 + 1 == row2)
          || (col1-1 == col2 && row1 + 1 == row2)
          || (col1-1 == col2 && row1 == row2)
          || (col1-1 == col2 && row1 - 1 == row2)
          )
        {
            conn = true;
        }

        return conn;
    }

    private int GetRightCnt(){
        int rightCnt = 0;
        for (int i = 0; i < _CurrentSelectSJIndex.Count; i++)
        {
            //相同字的情况需要特殊处理
            if (_CurrentSelectSJIndex[i] != _CurrentSJIndex[i])
            {
                string txt1 = _HZList[_CurrentSelectSJIndex[i]].GetComponentInChildren<Text>().text;
                string txt2 = _HZList[_CurrentSJIndex[i]].GetComponentInChildren<Text>().text;
                if (txt1.Equals(txt2))
                {
                    rightCnt++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                rightCnt++;
            }
        }

        return rightCnt;
    }

    //插入干扰字
    private void InsertDisturbHZ(string []hzList){

        List<int> searching = new List<int>();

        if (_DisturbCnt == 0) return;

        int blankCnt = XT_HZ_MATRIX * XT_HZ_MATRIX - CurrentSJ.Length;
        //已经填满了
        if (blankCnt == 0) return;

        List<string> otherHz = new List<string>();
        do
        {
            string sj = GetRandomShiJu();
            foreach (var thz in sj)
            {
                otherHz.Add("" + thz);
            }

        } while (otherHz.Count < blankCnt);

        ShuffleSJList(otherHz);//可能总数会超过方阵剩余的空白数

        //需要根据当前的干扰字个数插入
        for (int i = 0; i < hzList.Length; i++)
        {
            if (hzList[i] == "")
            {
                searching.Add(i);
            }
        }

        ShuffleSJList(searching);

        //优选搜索周围空白的字最多的字
        List<SD> sds = new List<SD>();
        for (int i = 0; i < searching.Count; i++)
        {
            SD sd;
            sd.index = searching[i];
            sd.priority = GetHZPriority(hzList,searching[i]);
            sds.Add(sd);
        }

        sds.Sort((a, b) => {
            //return b.priority - a.priority;//降序排列，周围字最多的排前面，也是干扰最大的字
            return b.priority - a.priority;//降序排列，周围字最多的排前面，也是干扰最大的字
        });

        searching.Clear();
        foreach (var sd in sds)
        {
            searching.Add(sd.index);
        }

        ////由于不好处理 干扰字 放到不连通的位
        /// 所以直接全部优先从最难的字开始放置
        /*
        int sindex = (int)(_DisturbCntSearchDifficult * searching.Count);//0=< 1-_SearchDifficult <1
        if (sindex >= searching.Count - 1) sindex = searching.Count - 1;

        List<int> searching1 = new List<int>();
        List<int> searching2 = new List<int>();

        for (int i = 0; i < sindex; i++)
        {
            searching1.Add(searching[i]);
        }

        for (int i = sindex; i < searching.Count; i++)
        {
            searching2.Add(searching[i]);
        }

        searching.Clear();
        searching.AddRange(searching2);// 优先往难的方向搜索
        searching1.Reverse();
        searching.AddRange(searching1);//再往越来越简单的方向搜索
*/

        int cnt = 0;
        for (int i = 0; i < searching.Count; i++)
        {
            if (cnt >= _DisturbCnt) break;
            hzList[searching[i]] = otherHz[cnt];
            cnt++;
        }

        return;
    }
}
