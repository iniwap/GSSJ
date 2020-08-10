using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Reign;

public class TiMu : MonoBehaviour
{
    public enum eTiMuFanWei
    {
        All,
        TangShi,//默认唐诗
        SongCi,
        GuShi,
        ShiJing,
        //下面两个和上面有些不同
        DianGu,
        ChangShi,
    };

    protected eTiMuFanWei  _TiMuFW = eTiMuFanWei.TangShi;

    public void SetTiMuFW(eTiMuFanWei fw)
    {
        _TiMuFW = fw;
    }
    public eTiMuFanWei GetTiMuFW(){
        return _TiMuFW;
    }

    public virtual void Start()
    {

    }

    public virtual void OnEnable()
    {
        _CanExist = false;
        CheckDaanBuyState();
    }

    public virtual void OnChangeBgColor(Color c)
    {
        _BgColor = c;

        _BgLightColor = Define.GetFixColor(_BgColor * 1.1f);
        _BgDarkColor = Define.GetFixColor(_BgColor * 0.9f);
    }
    
    protected Color _BgColor;
    protected Color _BgLightColor;
    protected Color _BgDarkColor;

    public virtual void InitTiMu(Action cb)
    {

    }
    public virtual void OnSwipe(Define.SWIPE_TYPE type){

    }
    public virtual void DoZuoDaAni()
    {
        DOTween.Kill("ZUODA"+GetStudyName(), false);
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetId("ZUODA" + GetStudyName());

        Transform zuoda = gameObject.transform.Find("TiMu/ZuoDa");

        if (zuoda != null)
        {
            GameObject tip = zuoda.Find("Tip").gameObject;
            Image[] zdjd = tip.GetComponentsInChildren<Image>();
            foreach (var zd in zdjd)
            {
                zd.color = _BgDarkColor;
            }
            mySequence
                .Append(zdjd[0].DOColor(_BgLightColor, 0.4f))
                .Append(zdjd[1].DOColor(_BgLightColor, 0.4f))
                .Append(zdjd[2].DOColor(_BgLightColor, 0.4f))

                .Append(zdjd[0].DOColor(_BgDarkColor, 0.4f))
                .Append(zdjd[1].DOColor(_BgDarkColor, 0.4f))
                .Append(zdjd[2].DOColor(_BgDarkColor, 0.4f))
                .SetLoops(-1, LoopType.Restart);
        }
    }

    protected int _TotalScore = 0;
    protected int _ChuangGuan = 0;
    //假设10s的基础分是10分，那么20s的基础分只有5分
    protected virtual int GetCurrentScore()
    {
        int ztt = (int)ZTTime - _ScoreTime;

        //耗时后得分
        int cs = Define.BASE_SCORE_10S * (int)Study.ZTTimeType.ZT_10S / (int)ZTTime;
        cs = cs * ztt / (int)ZTTime;

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
            cs += Define.PLUS_SCORE_SHIJING;
        }

        //连续闯关加分
        cs += _ChuangGuan;

        return cs;
    }

    protected Study.ZTTimeType ZTTime;
    public virtual void OnChangeZTTimeType(Study.ZTTimeType zzt)
    {
        ZTTime = zzt;
    }
    protected int _ScoreTime = 0;//记录当前答题耗时 //对于排序题，该参数含义不同，需要重写处理
    protected int _CurrentPassTime = 0;
    public virtual void OnZuoTiTimer(bool start)
    {
        Transform t = gameObject.transform.Find("TiMu/Time/BellDark");

        if (!start)
        {
            _CurrentPassTime = 0;
            CancelInvoke("DoZuoTiTimeClock");

            if (t != null)
            {
                t.localRotation = new Quaternion(0.0f,0.0f,0.0f,0.0f);
                t.GetComponentInChildren<Text>().transform.localScale = new Vector3(1.0f,1.0f,1.0f);
            }

            return;
        }
        _CurrentPassTime = 0;

        if (t != null)
        {
            t.GetComponentInChildren<Text>().text = "" + (int)ZTTime;
        }

        RestTiMu();
        //剩余5s摇铃
        //
        //大于5s，仅执行倒计时
        InvokeRepeating("DoZuoTiTimeClock", 1.0f, 1.0f);
    }

    protected void RestTiMu(){
        Transform tm = gameObject.transform.Find("TiMu");//TiText
        if (tm != null)
        {
            GameObject tmobj = tm.Find("TiText").gameObject;
            Text[] timu = tmobj.GetComponentsInChildren<Text>();
            foreach (var tt in timu)
            {
                tt.color = _BgLightColor;// 重置透明度
            }
        }
    }

    protected virtual void DoZuoTiTimeClock()
    {
        _CurrentPassTime += 1;
        _ScoreTime += 1;

        if(_ScoreTime >= (int)ZTTime){
            _ScoreTime = (int)ZTTime;
        }

        Transform t = gameObject.transform.Find("TiMu/Time/BellDark");
        if (t != null)
        {
            int ztt = (int)ZTTime - _CurrentPassTime;
            int half = (int)Study.ZTTimeType.ZT_10S / 2;
            if (ztt > half)
            {
                t.GetComponentInChildren<Text>().text = "" + ztt;
            }
            else if (ztt <= half && ztt > 0)
            {
                t.GetComponentInChildren<Text>().text = "" + ztt;
                t.DOShakeRotation(0.5f, (half - ztt) / half * 100 + 20);
            }
            else
            {
                //超时了
                t.GetComponentInChildren<Text>().text = "0";

                if (ztt == 0)
                {
                    Transform tm = gameObject.transform.Find("TiMu");//TiText
                    if (tm != null)
                    {
                        GameObject tmobj = tm.Find("TiText").gameObject;
                        Text[] timu = tmobj.GetComponentsInChildren<Text>();

                        GameObject tmclockobj = tm.Find("FailClockTiText").gameObject;
                        tmclockobj.SetActive(true);
                        tmclockobj.GetComponent<Text>().text = "请在<size=64><b>" + GetThinkTime() + "s</b></size>内作答，否则视为闯关失败！";

                        Text tmc = tmclockobj.GetComponent<Text>();
                        tmc.color = new Color(tmc.color.r, tmc.color.g, tmc.color.b, 0.0f);

                        Sequence mySequence = DOTween.Sequence();
                        mySequence.SetId("DoZuoTiTimeClockAni"+GetStudyName());
                        foreach (var tt in timu)
                        {
                            mySequence.Join(tt.DOFade(0.0f, 1.0f));
                        }

                        mySequence
                            .Append(tmc.DOFade(1.0f, 0.5f))
                            .OnComplete(() =>
                            {
                                //请在10s内给出答案，否则视为闯关失败
                                DoFailClockAni(true);
                            });
                    }
                }
                else
                {
                    OnZuoTiTimer(false);//停止
                }
            }

            if (ztt < (int)Study.ZTTimeType.ZT_10S && ztt > 0)
            {
                float s = (half * 2 - ztt) / (half * 2) + 0.5f;
                t.GetComponentInChildren<Text>().transform.DOShakeScale(0.5f, new Vector3(s, s, 1.0f));
            }
        }
    }

    protected int _CurrentFailClockTime = 0;
    protected void DoFailClockAni(bool start,bool needStopAni = false)
    {
        _CurrentFailClockTime = 0;

        if (!start)
        {
            CancelInvoke("DoFailClock");


            if (needStopAni)
            {
                //
                DOTween.Kill("DoZuoTiTimeClockAni" + GetStudyName());
            }

            return;
        }

        DoShowDaanBtnAni();
        InvokeRepeating("DoFailClock", 1.0f, 1.0f);
    }
    public UnityEvent OnHideDialog;
    protected void DoFailClock()
    {
        _CurrentFailClockTime += 1;
        int fc = GetThinkTime() - _CurrentFailClockTime;

        Transform tm = gameObject.transform.Find("TiMu");//TiText
        if (tm != null)
        {
            GameObject tmclockobj = tm.Find("FailClockTiText").gameObject;
            if (fc > 0)
            {
                tmclockobj.GetComponent<Text>().text = "请在<size=64><b>" + fc + "s</b></size>内作答，否则视为闯关失败！";
            }
            else
            {
                tmclockobj.GetComponent<Text>().text = "您已经超时，闯关失败！";
                DoFailClockAni(false);

                //关闭弹窗，如果存在的话

                OnHideDialog.Invoke();

                //闯关失败
                TimeOutFail();
            }
        }
    }

    protected int GetThinkTime(){
        if(GetStudyName() == "TianKong"){
            return (int)Study.ZTTimeType.ZT_20S;
        }

        return (int)Study.ZTTimeType.ZT_10S / 2;
    }
    //超时失败
    protected virtual void TimeOutFail(){

    }

    protected virtual void SaveScore(){
        int hs = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_SCORE + GetStudyName(), 0);
        int lx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_CG + GetStudyName(), 0);

        int hlx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.ALL_HIGHEST_CG, 0);

        _ChuangGuan += 1;
        int cs = GetCurrentScore();
        _TotalScore += cs;

        if (lx < _ChuangGuan)
        {
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_CG + GetStudyName(), _ChuangGuan);
        }

        if (hs < _TotalScore)
        {
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.HIGHEST_SCORE + GetStudyName(), _TotalScore);

            ReportScore(_TotalScore);
        }

        if(hlx < _ChuangGuan){
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.ALL_HIGHEST_CG, _ChuangGuan);
        }

        //不能一起上报，成就需要等到 一定时机上报
        //保存更新最高闯关数
        //ReportAchievement();
    }

    //放弃当前游戏
    public virtual void AbandonGame(Action cb = null){

    }

    //结算面板倒计时的时候可以返回
    public virtual void ExistGame()
    {

    }

    public UnityEvent OnDOTweenKillAll;
    protected virtual void DoKillAllAni()
    {
        OnDOTweenKillAll.Invoke();
        DOTween.KillAll();
    }
    protected virtual void StopAllClock()
    {

    }

    protected bool _CanExist = false;
    public bool GetCanExist(){
        return _CanExist;
    }

    public UnityEvent OnGameEnd;
    protected void GameEnd(){
        OnGameEnd.Invoke();

        //每次挑战结束时上报成就
        ReportAchievement();
    }

    [System.Serializable] public class OnShowMaskEvent : UnityEvent<bool> { }
    public OnShowMaskEvent OnShowMask;
    public void ShowMask(bool show){
        OnShowMask.Invoke(show);
    }

    public GameObject _makeXJBtn;
    [System.Serializable] public class OnMakeXJEvent : UnityEvent<int, string, HZManager.eShiCi> { }
    public OnMakeXJEvent OnMakeXJ;
    public void OnMakeXinJian(int sjID,string currentSJ, HZManager.eShiCi fw)
    {
        OnMakeXJ.Invoke(sjID,currentSJ,fw);
    }

    //最高得分
    [System.Serializable] public class OnReportScoreEvent : UnityEvent<long> { }
    public OnReportScoreEvent OnReportScore;
    public void ReportScore(long score)
    {
        OnReportScore.Invoke(score);
    }

    //连续闯关数 - 不分题目类型
    [System.Serializable] public class OnReportAchievementEvent : UnityEvent<float, GameCenter.AchievementType> { }
    public OnReportAchievementEvent OnReportAchievement;
    public void ReportAchievement()
    {
        GameCenter.AchievementType type = GameCenter.AchievementType.TongSheng;
        float percent = 0;

        bool allComplete = true;
        for (int i = (int)GameCenter.AchievementType.TongSheng; i <= (int)GameCenter.AchievementType.ZhuangYuan;i++){
            if(Setting.getPlayerPrefs(Setting.SETTING_KEY.ACHIEVEMENT_COMPLETE_PERCENT + ""+((GameCenter.AchievementType)i), 0f) < 100f){
                type = ((GameCenter.AchievementType)i);
                allComplete = false;
                break;
            }
        }

        //所有成就已经获得，不再上报
        if (allComplete) 
            return;

        int hlx = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.ALL_HIGHEST_CG, 0);

        if (type == GameCenter.AchievementType.TongSheng)
        {
            //童生
            percent = hlx / 10.0f;
        }
        else if (type == GameCenter.AchievementType.XiuCai)
        {
            //秀才
            percent = hlx / 20.0f;
        }
        else if (type == GameCenter.AchievementType.JuRen)
        {
            //举人
            percent = hlx / 50.0f;
        }
        else if (type == GameCenter.AchievementType.GongShi)
        {
            //贡士
            percent = hlx / 100.0f;
        }
        else if (type == GameCenter.AchievementType.JinShi)
        {
            //进士
            percent = hlx / 200.0f;
        }
        else if (type == GameCenter.AchievementType.TanHua)
        {
            //探花
            percent = hlx / 400.0f;
        }
        else if (type == GameCenter.AchievementType.BangYan)
        {
            //榜眼
            percent = hlx / 700.0f;
        }
        else if (type == GameCenter.AchievementType.ZhuangYuan)
        {
            //状元
            percent = hlx / 1000.0f;
        }

        percent *= 100;

        if(percent > 100f){
            percent = 100f;
        }

        OnReportAchievement.Invoke(percent, type);
    }

    //由于排序没有超时的概念，不方便展示该动画，不作提示
    //购买查看答案功能
    public IAP _iap;
    private bool _ProcessingPurchase = false;
    public Button _buyDaanBtn;
    public Text _leftDaanTime;//剩余查看答案次数
    public Button _showDaanBtn;
    protected bool _CantShowDaAn = false;
    //只有未购买的时候才提示可以查看答案，引导购买
    protected virtual void DoShowDaanBtnAni(){
        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if(currentLeftShowDaanCnt <= 0){
            //如果剩余次数为0，不必再提示 查看答案动画
            return;
        }

        if (!IAP.getHasBuy(IAP.IAP_DAAN)){
            //如果没有购买
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.BUY_SHOWDAAN_ANI, 0);
            if(tipCnt >= 3){
                return;
            }

            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.BUY_SHOWDAAN_ANI, tipCnt + 1);

            Image[] ll = _buyDaanBtn.GetComponentsInChildren<Image>(true);
            Image bg = ll[1];
            Image lockImg = ll[2];

            Sequence sq = DOTween.Sequence();
            sq
                .Append(lockImg.DOFade(0.1f, 0.5f))
                .Join(lockImg.transform.DOScale(2.0f, 0.5f))
                .Join(bg.DOFade(0.1f, 0.5f))
                .Append(_showDaanBtn.transform.DOShakePosition(1.0f, 5))
                .Append(lockImg.DOFade(1.0f, 0.5f))
                .Join(lockImg.transform.DOScale(1.0f, 0.5f))
                .Join(bg.DOFade(100 / 255.0f, 0.5f));
        }
        else{
            //已经购买的动画
            _showDaanBtn.transform.DOShakePosition(1.0f, 5);
        }
    }

    protected void EnableShowDaanBtn(bool enable){
        _showDaanBtn.interactable = enable;
        if (!enable)
        {
            _leftDaanTime.color = _BgDarkColor;
        }
        else
        {
            int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
            if (currentLeftShowDaanCnt == 0)
            {
                _leftDaanTime.color = _BgDarkColor;
            }
            else
            {
                _leftDaanTime.color = _BgLightColor;
            }
        }
    }

    public void UpdateLeftDaanTime(int n)
    {
        _leftDaanTime.text = ""+n;

        if (n == 0)
        {
            _leftDaanTime.color = _BgDarkColor;
        }
        else
        {
            _leftDaanTime.color = _BgLightColor;
        }
    }

    protected void CheckDaanBuyState(){
        if(IAP.getHasBuy(IAP.IAP_DAAN)){
            _buyDaanBtn.gameObject.SetActive(false);
        }
        else{
            _buyDaanBtn.gameObject.SetActive(true);
        }

        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);

        _leftDaanTime.text = "" + currentLeftShowDaanCnt;

        if (currentLeftShowDaanCnt == 0)
        {
            _leftDaanTime.color = _BgDarkColor;
        }
        else
        {
            _leftDaanTime.color = _BgLightColor;
        }
    }

    public void OnBuyDaanBtnClick(){
    
        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            _iap.onBuyClick(IAP.IAP_DAAN);
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍后...");
        }
    }

    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        if(inAppID != IAP.IAP_DAAN){
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
            if (_buyDaanBtn.gameObject.activeSelf)
            {
                DoUnLockAni(_buyDaanBtn.gameObject, () => {
                    _buyDaanBtn.gameObject.SetActive(false);
                });
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
        if (inAppID != IAP.IAP_DAAN)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            //如果lock不可见，不必执行动画
            if (_buyDaanBtn.gameObject.activeSelf)
            {
                DoUnLockAni(_buyDaanBtn.gameObject, () => {
                    _buyDaanBtn.gameObject.SetActive(false);
                });
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
            if (img.name == "BuyLockTagLight")
            {
                lockImg = img;
            }
            if (img.name == "BuyUnLockTagLight")
            {
                unlockImg = img;
            }
            if(img.name == "Bg")
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
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.DaanBtnBuyClick + "_" + success);
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f,float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = _BgColor;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }
    //-----------------------一些接口------------------------
    protected virtual string GetStudyName(){
       // Debug.Log("ERROR GetStudyName:" + gameObject.name);//error if call here
        return gameObject.name;
    }

    public string GetInfo(eTiMuFanWei fw,int sjID)
    {
        List<string> tsscData = HZManager.GetInstance().GetTSSC(GetShiCiType(fw), sjID);
        string zz = tsscData[2] + "·" + tsscData[3] + "《" + tsscData[1] + "》";
        return zz;
    }

    public HZManager.eShiCi GetShiCiType(eTiMuFanWei fw){
        switch (fw)
        {
            case eTiMuFanWei.All:
                return HZManager.eShiCi.ALL;
            case eTiMuFanWei.GuShi:
                return HZManager.eShiCi.GUSHI;
            case eTiMuFanWei.ShiJing:
                return HZManager.eShiCi.SHIJING;
            case eTiMuFanWei.SongCi:
                return HZManager.eShiCi.SONGCI;
            case eTiMuFanWei.TangShi:
                return HZManager.eShiCi.TANGSHI;
        }

        return HZManager.eShiCi.TANGSHI;
    }

    //不包括最后一个标点符号
    public string GetRandomHalfShiJu(bool whole = false){
        string jz = "";

        string tssc = GetRandomShiJu();
        string[] jzs2 = tssc.Split('，');

        int indx2 = HZManager.GetInstance().GenerateRandomInt(0, jzs2.Length);
        jz = jzs2[indx2];

        if (!whole)
        {
            if (!HZManager.GetInstance().CheckIsHZ(jz[jz.Length - 1]))
            {
                jz = jz.Substring(0, jz.Length - 1);
            }
        }

        return jz;
    }

    //获取整句诗，包括标点
    public string GetRandomShiJu(){
        string tssc = "";
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

        return tssc;
    }
    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    public void ShuffleSJList(List<GameObject> myList)
    {
        int index = 0;
        GameObject temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }

    public void ShuffleSJList(List<string> myList)
    {
        int index = 0;
        string temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }

    public void ShuffleSJList(List<int> myList)
    {
        int index = 0;
        int temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }

    public void ShuffleSJList(List<object> myList)
    {
        int index = 0;
        object temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }
}
