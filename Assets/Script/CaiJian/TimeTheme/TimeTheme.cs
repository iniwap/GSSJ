using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class TimeTheme : MonoBehaviour
{
    public void Start()
    {

    }

    public void OnEnable()
    {
        foreach (var zsObj in _ThemeBtnList)
        {
            ThemeBtnItem btn = zsObj.GetComponent<ThemeBtnItem>();
            if (btn.GetIsTitle()) continue;
            btn.ShowBuyBtn(!IAP.getHasBuy(IAP.IAP_THEME));
        }
    }

    public Light _Light;
    public Camera _themeCamera;

    public Transform _ThemeBtnContent;
    public GameObject _ThemeTitlePrefabs;
    public GameObject _ThemeBtnPrefabs;
    private List<GameObject> _ThemeBtnList = new List<GameObject>();

    public void OnInit()
    {
        bool hasBuy = IAP.getHasBuy(IAP.IAP_THEME);
        bool hasSetCustom = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.HAS_SET_CUSTOM_THEME, 0) == 1;

        //是否购买了自定义，购买了自定义则放在最前面，否则放在后面
        if (hasBuy)
        {
            //先添加自定义
            InitBtn(false, Define.eWeekType.Custom, hasSetCustom);
            //加载所有主题-->星期一到星期日，清晨/早上、中午、下午/黄昏、晚上
            for (int i = (int)Define.eWeekType.Monday; i <= (int)Define.eWeekType.Sunday; i++)
            {
                InitBtn(false,(Define.eWeekType)i, hasSetCustom);
            }
        }
        else
        {
            for (int i = (int)Define.eWeekType.Monday; i <= (int)Define.eWeekType.Sunday; i++)
            {
                InitBtn(false,(Define.eWeekType)i, hasSetCustom);
            }
            //后添加自定义
            InitBtn(true, Define.eWeekType.Custom, hasSetCustom);
        }

        //根据设置选择，如果开启的了话
        OnOpenTheme(Setting.GetTheme());

        _ThemeParamAdjust.OnInit();

        //加载动态主题资源
        for (int i = 1; i <= MAX_DYNAMIC_THEME; i++)
        {
            _DynamicTheme.Add(Resources.Load("Theme/Dynamic/Materials/" + i, typeof(Material)) as Material);
        }
    }

    private void InitBtn(bool showBuy,Define.eWeekType week,bool hasSetCustom)
    {
        GameObject obj = Instantiate(_ThemeTitlePrefabs, _ThemeBtnContent) as GameObject;
        obj.SetActive(true);
        _ThemeBtnList.Add(obj);
        ThemeBtnItem btn = obj.GetComponent<ThemeBtnItem>();
        btn.Init(false, true, week, Define.eHourType.None, hasSetCustom);

        for (int i = (int)Define.eHourType.Morning; i <= (int)Define.eHourType.Night; i++)
        {
            GameObject obj2 = Instantiate(_ThemeBtnPrefabs, _ThemeBtnContent) as GameObject;
            obj2.SetActive(true);
            _ThemeBtnList.Add(obj2);
            ThemeBtnItem btn2 = obj2.GetComponent<ThemeBtnItem>();
            btn2.Init(week == Define.eWeekType.Custom && showBuy, false, week, (Define.eHourType)i, hasSetCustom);
        }
    }

    public void ShowTheme(bool show)
    {
        _themeCamera.gameObject.SetActive(show);
        if (!show)
        {
            //CancelInvoke("UpdateTYPosByTime");
        }
    }

    public void OnOpenTheme(bool open)
    {
        if (open == _themeCamera.gameObject.activeSelf) return;

        if (open)
        {
            OnClickItem(Define.GetWeek(), Define.GetHourType());
        }
        else
        {
            OnCloseThemeBtnClick();
        }
    }

    //-------------切换主题---------------------------------
    public void OnClickItem(Define.eWeekType week,Define.eHourType hour)
    {
        _CloseThemeBtnAdjust.SetActive(false);
        ShowTheme(true);
        SetTheme(week,hour);
    }

    public int MAX_DYNAMIC_THEME = 8;
    private List<Material> _DynamicTheme = new List<Material>();
    private Color[] _DynamicThemeColor = {
        new Color(184/255f,154/255f,178/255f),
        new Color(107/255f,209/255f,214/255f),
        new Color(80/255f,111/255f,117/255f),
        new Color(66/255f,119/255f,169/255f),
        new Color(58/255f,119/255f,239/255f),
        new Color(110/255f,81/255f,130/255f),
        new Color(96/255f,212/255f,176/255f),
        new Color(114/255f,67/255f,99/255f),
        new Color(0.75f,0.75f,0.75f),
        new Color(2/255f,0,32/255f)};
    public void OnClickDynamicTheme(int index)
    {
        HideAllDynamicAjust();
        OnCloseThemeBtnClick();
        _CloseThemeBtnAdjust.SetActive(false);
        ShowTheme(true);

        RenderSettings.skybox = _DynamicTheme[index];
        _ThemeBtnContent.transform.Find("D" + (index + 1) + "/Adjust").gameObject.SetActive(true);
        //使用天空颜色作为标准
        OnChangeThemeColor.Invoke(_DynamicThemeColor[index]);
        OnChangeTheme.Invoke();

        InvokeRepeating("DynamicSky", 0.0f,1/60f);//每秒更新太阳的形态
    }
    private void HideAllDynamicAjust()
    {
        CancelInvoke("DynamicSky");
        for(int i = 1;i <= MAX_DYNAMIC_THEME;i++)
        {
            _ThemeBtnContent.transform.Find("D"+i+ "/Adjust").gameObject.SetActive(false);
        }
    }

    public void DynamicSky()
    {
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        int second = DateTime.Now.Second;

        float RX = 0;
        float RY = 0;


        //0->12 -90->0
        //12->24 0->-90
        if (hour >= 0 && hour < 12) // 12个小时
        {
            //白天时间[-90->0]
            //12点最高
            RX = (1 - hour / 12f) * (-90f)  + (60 * minute + second) * 7.5f / 3600f;
        }
        else //12个小时
        {
            //夜晚时间
            RX = (hour - 12) / 12f * (-90f) - (60 * minute + second) * 7.5f / 3600f;
        }

        RY = (_themeCamera.transform.eulerAngles.y + 1/60f) % 360;

        _themeCamera.transform.rotation = Quaternion.Euler(RX, RY, 0);

    }

    //根据时间设置太阳的高度/大小等关键形态参数来表达时间变化
    public void UpdateTYPosByTime()
    {
        float lightRX = 0;
        float lightRY = 0;
        //lightRX [-18,18]  [-90,90] - 高度
        //lightRY [160,200] [120,240] - 东西

        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        int second = DateTime.Now.Second;

        if (hour >= 6 && hour <= 18) // 12个小时
        {
            //白天时间
            //12点最高
            //6->12 [-18,18]
            float hinvx = 6f;
            if (hour < 12)
            {
                lightRX = (hour - 6) / 6f * 36 - 18 + (60 * minute + second)* hinvx / 3600f;
            }
            //12->18 [18,-18]
            else
            {
                lightRX = -(hour - 12) / 6f * 36 + 18 + (60 * minute + second) * hinvx / 3600f;
            }

            //[150,210]
            float hinvy = 5f;
            lightRY = (hour - 6) * 5f + 150 + (60 * minute + second) * hinvy / 3600f;

        }
        else //12个小时
        {
            //夜晚时间
            //18->24 [150,180]
            //0->6 [180,210]
            //0点最高
            //6->12 [-18,18]
            float hinvx = 6f;
            if (hour < 24)
            {
                lightRX = (hour - 18) / 6f * 36 - 18 + (60 * minute + second) * hinvx / 3600f;
            }
            //12->18 [18,-18]
            else
            {
                lightRX = -(hour - 0) / 6f * 36 + 18 + (60 * minute + second) * hinvx / 3600f;
            }

            float hinvy = 5f;
            if (hour < 24)
            {
                lightRY = (hour - 18) * 5f + 150 + (60 * minute + second) * hinvy / 3600f;
            }
            else
            {
                lightRY = (hour - 0) * 5f + 180 + (60 * minute + second) * hinvy / 3600f;
            }
        }

        _Light.transform.localRotation = Quaternion.Euler(lightRX, lightRY, 0);
    }


    private bool _fixFilterColorBug = false;
    public ThemeParamAdjust _ThemeParamAdjust;
    [System.Serializable] public class OnChangeThemeColorEvent : UnityEvent<Color> { }
    public OnChangeThemeColorEvent OnChangeThemeColor;
    public UnityEvent OnChangeTheme;
    public UnityEvent OnFixFilterColorBug;
    private void SetTheme(Define.eWeekType week,Define.eHourType hour)
    {
        HideAllDynamicAjust();
        foreach (var theme in _ThemeBtnList)
        {
            ThemeBtnItem btn = theme.GetComponent<ThemeBtnItem>();

            if (btn.GetIsTitle()) continue;

            if (btn.GetWeekType() == week && btn.GetHourType() == hour)
            {
                if (btn.GetWeekType() == Define.eWeekType.Custom)
                {
                    if (btn.GetCanAdjust())
                    {
                        //可以调整
                        //弹出调整框
                        _ThemeParamAdjust.ShowAdjust(true);
                    }
                    else
                    {
                        //CancelInvoke("UpdateTYPosByTime");
                        RenderSettings.skybox = btn.GetMaterial();
                        if (!_fixFilterColorBug)//只有这个自定义白天有bug
                        {
                            OnFixFilterColorBug.Invoke();
                            _fixFilterColorBug = true;
                        }

                        //使用天空颜色作为标准
                        OnChangeThemeColor.Invoke(btn.GetSkyTopColor());

                        if (week == Define.eWeekType.Custom)
                        {
                            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_THEME_ADJUST_CUSTOM_TIPS, 0);

                            if (tipCnt < Define.BASIC_TIP_CNT)
                            {
                                ShowToast("<b>再点一次</b>调节情景详细参数、效果");
                                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_THEME_ADJUST_CUSTOM_TIPS, tipCnt + 1);
                            }
                        }
                        OnChangeTheme.Invoke();
                    }
                }
                else
                {
                    //CancelInvoke("UpdateTYPosByTime");
                    //InvokeRepeating("UpdateTYPosByTime",0.0f,1.0f);//每秒更新太阳的形态
                    //UpdateTYPosByTime();
                    RenderSettings.skybox = btn.GetMaterial();
                    //使用天空颜色作为标准
                    OnChangeThemeColor.Invoke(btn.GetSkyTopColor());
                    OnChangeTheme.Invoke();
                }

                btn.SetSelected(true);
            }
            else
            {
                btn.SetSelected(false);
            }
        }

    }

    public GameObject _CloseThemeBtnAdjust;
    public void OnCloseThemeBtnClick()
    {
        HideAllDynamicAjust();
        OnChangeThemeColor.Invoke(_BGColor);

        ShowTheme(false);
        _CloseThemeBtnAdjust.SetActive(true);
        //需要更新当前选中？
        foreach (var theme in _ThemeBtnList)
        {
            ThemeBtnItem btn = theme.GetComponent<ThemeBtnItem>();

            if (btn.GetIsTitle()) continue;
            btn.SetSelected(false);
        }
    }

    private Color _BGColor;
    public void OnChangeBgColor(Color c,bool isChangeTheme)
    {
        if (!isChangeTheme)
        {
            _BGColor = c;//仅仅用作恢复用

        }

        _ThemeParamAdjust.ChangeBGColor(c);
    }

    //----------------------------调整主题参数---------------------------------
    #region 购买情景
    public IAP _iap;
    private bool _ProcessingPurchase = false;

    //此处需要判断购买的是哪个内购
    public void OnBuyBtnClick(string inAppID)
    {

        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            _iap.onBuyClick(IAP.GetAppIDByName(inAppID));
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍后...");
        }
    }

    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        if (inAppID != IAP.IAP_THEME)
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
                if (inAppID == IAP.IAP_THEME)
                {
                    foreach (var zsObj in _ThemeBtnList)
                    {
                        ThemeBtnItem zsBtn = zsObj.GetComponent<ThemeBtnItem>();
                        if (zsBtn.GetWeekType() != Define.eWeekType.Custom || zsBtn.GetIsTitle()) continue;

                        DoUnLockAni(zsBtn.gameObject, () => {
                            zsBtn.ShowBuyBtn(false);
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
        if (inAppID != IAP.IAP_THEME)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            //如果lock不可见，不必执行动画
            if (gameObject.activeSelf)
            {
                if (inAppID == IAP.IAP_THEME)
                {
                    foreach (var zsObj in _ThemeBtnList)
                    {
                        ThemeBtnItem zsBtn = zsObj.GetComponent<ThemeBtnItem>();
                        if (zsBtn.GetWeekType() != Define.eWeekType.Custom || zsBtn.GetIsTitle()) continue;

                        DoUnLockAni(zsBtn.gameObject, () => {
                            zsBtn.ShowBuyBtn(false);
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
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.ThemeBtnBuyClick + "_" + success);
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        Color c = _BGColor;
        if (!_CloseThemeBtnAdjust.activeSelf)
        {
            foreach (var theme in _ThemeBtnList)
            {
                ThemeBtnItem btn = theme.GetComponent<ThemeBtnItem>();

                if (btn.GetIsTitle()) continue;
                if (btn.GetCanAdjust())
                {
                    c = btn.GetSkyBottomColor();
                    break;
                }
            }
        }

        Toast.ToastData data;
        data.c = c;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }
    #endregion
}
