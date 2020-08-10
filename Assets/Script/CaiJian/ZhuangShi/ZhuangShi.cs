/*
 *用于控制处理和装饰有关的逻辑
 *
 */
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;
using Reign;

public class ZhuangShi : MonoBehaviour
{
    public void Start()
    {
        _IsPicShowing = false;
    }

    public void OnEnable()
    {
        foreach (var zsObj in _ZSBtnList)
        {
            ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();
            zsBtn.ShowBuyBtn(!IAP.getHasBuy(IAP.IAP_ZISHI));
        }

        foreach (var zsObj in _PTBtnList)
        {
            ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();
            zsBtn.ShowBuyBtn(!IAP.getHasBuy(IAP.IAP_PEITU));
        }
    }

    public ZSParamAdjust _zsParamAdjust;//装饰参数调整

    public Transform _ZSContent;
    public Transform _DZContent;
    public Transform _PTContent;
    public Transform _HSContent;

    private List<GameObject> _ZSBtnList = new List<GameObject>();
    private List<GameObject> _DZBtnList = new List<GameObject>();
    private List<GameObject> _PTBtnList = new List<GameObject>();//配图BTN
    private List<GameObject> _HSBtnList = new List<GameObject>();

    public enum LoadType
    {
        LOAD_ZS,
        LOAD_DZ,
        LOAD_HS,
        LOAD_PT,
    }

    float inv = 120f;
    public void OnInit()
    {
        //需要初始化调整界面参数
        _zsParamAdjust.OnInit();

        int cnt = 0;
#if UNITY_EDITOR
        cnt = EditorLoad(LoadType.LOAD_ZS);
#elif UNITY_IOS
        cnt = IOSLoad(LoadType.LOAD_ZS);
#endif
        if(cnt != 0)
        {
            RectTransform rt = _ZSContent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(inv * cnt, rt.rect.height);
        }
        //-----------------------初始化点缀按钮列表-------------------------------
#if UNITY_EDITOR
        cnt = EditorLoad(LoadType.LOAD_DZ);
#elif UNITY_IOS
        cnt = IOSLoad(LoadType.LOAD_DZ);
#endif
        if(cnt != 0)
        {
            RectTransform rt = _DZContent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(inv * cnt, rt.rect.height);
        }
        //----------------------初始化配图列表----------------------------------
#if UNITY_EDITOR
        cnt = EditorLoad(LoadType.LOAD_PT);
#elif UNITY_IOS
        cnt = IOSLoad(LoadType.LOAD_PT);
#endif
        if (cnt != 0)
        {
            RectTransform rt = _PTContent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(inv * cnt, rt.rect.height);
        }
        //----------------------初始化行线列表----------------------------------
#if UNITY_EDITOR
        cnt = EditorLoad(LoadType.LOAD_HS);
#elif UNITY_IOS
        cnt = IOSLoad(LoadType.LOAD_HS);
#endif
        if (cnt != 0)
        {
            RectTransform rt = _HSContent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(inv * cnt, rt.rect.height);
        }
    }

    public int BasicShapeCnt = 30;
    public int BasicFreeCnt = 10;
    private int IOSLoad(LoadType type)
    {
        Transform content = _ZSContent;
        GameObject btnPrefabs = _ZSBtnPrefabs;
        ZhuangShiBtnItem.eZSBtnType btnType = ZhuangShiBtnItem.eZSBtnType.ZiShiShape;

        switch (type)
        {
            case LoadType.LOAD_ZS:
                btnPrefabs = _ZSBtnPrefabs;
                content = _ZSContent;
                btnType = ZhuangShiBtnItem.eZSBtnType.ZiShiShape;
                break;
            case LoadType.LOAD_HS:
                btnType = ZhuangShiBtnItem.eZSBtnType.HangShiShape;
                return 0;
               // break;
            case LoadType.LOAD_PT:
                btnPrefabs = _PTBtnPrefabs;
                content = _PTContent;
                btnType = ZhuangShiBtnItem.eZSBtnType.PeiTuShape;
                break;
            case LoadType.LOAD_DZ:
                btnType = ZhuangShiBtnItem.eZSBtnType.DianZhuiShape;
                return 0;//暂不支持
                // break;
        }
        //------------------------------初始化字饰按钮列表------------------------
        //首先添加“不使用”
        GameObject obj = Instantiate(btnPrefabs, content) as GameObject;
        obj.SetActive(true);

        if (btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _ZSBtnList.Add(obj);
        }
        else if (btnType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
        {
            _DZBtnList.Add(obj);
        }
        else if (btnType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
        {
            _HSBtnList.Add(obj);
        }
        else if (btnType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
        {
            _PTBtnList.Add(obj);
        }

        ZhuangShiBtnItem btn = obj.GetComponent<ZhuangShiBtnItem>();
        btn.Init(false, "", btnType, "" + 0,
                   GetDefaultItem("", btnType));
                  
        for (int i = 1; i <= BasicShapeCnt; i++)
        {
            //fi.
            obj = Instantiate(btnPrefabs, content) as GameObject;
            obj.SetActive(true);

            if (btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
            {
                _ZSBtnList.Add(obj);
            }
            else if (btnType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
            {
                _DZBtnList.Add(obj);
            }
            else if (btnType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
            {
                _HSBtnList.Add(obj);
            }
            else if (btnType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
            {
                _PTBtnList.Add(obj);
            }

            btn = obj.GetComponent<ZhuangShiBtnItem>();

            string btnID = "Shape/Basic/F" + i;
            if(i > BasicFreeCnt)
            {
                btnID = "Shape/Basic/NF" + i; 
            }
            btn.Init(i > BasicFreeCnt, btnID, btnType, "" + i,
                       GetDefaultItem(btnID, btnType));

        }

        return BasicShapeCnt + 1;
    }

    private int EditorLoad(LoadType type)
    {
        string app = Application.dataPath;

        DirectoryInfo basicDir = new DirectoryInfo(app + "/Resources/Shape/Basic");
        FileInfo[] fillImg = basicDir.GetFiles("*.png", SearchOption.AllDirectories);

        Transform content = _ZSContent;
        GameObject btnPrefabs = _ZSBtnPrefabs;
        ZhuangShiBtnItem.eZSBtnType btnType = ZhuangShiBtnItem.eZSBtnType.ZiShiShape;

        switch (type)
        {
            case LoadType.LOAD_ZS:
                btnPrefabs = _ZSBtnPrefabs;
                content = _ZSContent;
                btnType = ZhuangShiBtnItem.eZSBtnType.ZiShiShape;
                break;
            case LoadType.LOAD_HS:
                btnType = ZhuangShiBtnItem.eZSBtnType.HangShiShape;
                return 0;
            // break;
            case LoadType.LOAD_PT:
                btnPrefabs = _PTBtnPrefabs;
                content = _PTContent;
                btnType = ZhuangShiBtnItem.eZSBtnType.PeiTuShape;
                break;
            case LoadType.LOAD_DZ:
                btnType = ZhuangShiBtnItem.eZSBtnType.DianZhuiShape;
                return 0;//暂不支持
                // break;
        }

        //------------------------------初始化字饰按钮列表------------------------
        int cnt = 1;
        //首先添加“不使用”
        GameObject obj = Instantiate(btnPrefabs, content) as GameObject;
        obj.SetActive(true);
        if (btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _ZSBtnList.Add(obj);
        }
        else if (btnType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
        {
            _DZBtnList.Add(obj);
        }
        else if (btnType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
        {
            _HSBtnList.Add(obj);
        }
        else if (btnType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
        {
            _PTBtnList.Add(obj);
        }
        ZhuangShiBtnItem btn = obj.GetComponent<ZhuangShiBtnItem>();
        btn.Init(false, "", btnType, "" + cnt,GetDefaultItem("", btnType));

        foreach (var fi in fillImg)
        {
            //fi.
            obj = Instantiate(btnPrefabs, content) as GameObject;
            obj.SetActive(true);

            if (btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
            {
                _ZSBtnList.Add(obj);
            }
            else if (btnType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
            {
                _DZBtnList.Add(obj);
            }
            else if (btnType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
            {
                _HSBtnList.Add(obj);
            }
            else if (btnType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
            {
                _PTBtnList.Add(obj);
            }

            btn = obj.GetComponent<ZhuangShiBtnItem>();

            string btnID = "Shape/Basic/" + fi.Name;
            string fmtBtnID = btnID.Substring(0, btnID.IndexOf('.'));
            btn.Init(cnt > BasicFreeCnt, fmtBtnID, btnType, "" + cnt, GetDefaultItem(fmtBtnID, btnType));

            cnt++;
        }

        return cnt;
    }

    public ZhuangShiBtnItem.ZSParam GetDefaultItem(string btnID, ZhuangShiBtnItem.eZSBtnType btnType)
    {
        ZhuangShiBtnItem.ZSParam item;
        item.btnType = btnType;
        item.path = btnID;
        item.color = Define.BG_COLOR_50;
        item.size = 1.0f;

        return item;
    }

    public SubMenuCtrl _subMenuCtrl;

    public GameObject[]_ZSMenuBtnList;//最后两个为配图的按钮-相机、相册
    public void OnClickZhuangShiBtn(bool zs/*是装饰还是配图*/){
        if(gameObject.activeSelf){
            _subMenuCtrl.ShowSubMenu(SubMenuCtrl.eSubMenuType.SubMenu_ZhuangShi,false);
        }
        else{

            _subMenuCtrl.ShowSubMenu(SubMenuCtrl.eSubMenuType.SubMenu_ZhuangShi, true);

            RectTransform rt = gameObject.transform.Find("MenuList").GetComponent<RectTransform>();
            GameObject sp = gameObject.transform.Find("SP").gameObject;

            if (zs)
            {
                //提示可以点击字饰按钮修改文字颜色
                int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGEZS_SJCOLOR_TIP, 0);
                if (tipCnt < Define.BASIC_TIP_CNT)
                {
                    //一半的概率触发显示
                    if (HZManager.GetInstance().GenerateRandomInt(0, 100) <= 50)
                    {
                        ShowToast("点击<b>文字</b>按钮，可以修改<b>字体颜色</b>", 2f, 0.5f);
                        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_CHANGEZS_SJCOLOR_TIP, tipCnt + 1);
                    }
                }
                _ZSMenuBtnList[0].SetActive(true);
                _ZSMenuBtnList[1].SetActive(true);
                _ZSMenuBtnList[2].SetActive(false);//还不支持点缀
                _ZSMenuBtnList[3].SetActive(false);
                _ZSMenuBtnList[4].SetActive(false);
                _ZSMenuBtnList[5].SetActive(true);//主题

                sp.transform.localPosition = new Vector3(-177, sp.transform.localPosition.y, sp.transform.localPosition.z);
                rt.sizeDelta = new Vector2(360, rt.sizeDelta.y);

                if (_PrevCurrentSubMenu == ZhuangShiBtnItem.eZSBtnType.Theme)
                {
                    OnTHBtnClick();
                }
                else
                {
                    OnZSBtnClick();// 默认显示字饰
                }
            }
            else
            {
                _ZSMenuBtnList[0].SetActive(false);
                _ZSMenuBtnList[1].SetActive(false);
                _ZSMenuBtnList[2].SetActive(false);//还不支持点缀
                _ZSMenuBtnList[3].SetActive(true);
                _ZSMenuBtnList[4].SetActive(true);
                _ZSMenuBtnList[5].SetActive(false);//主题

                rt.sizeDelta = new Vector2(240, rt.sizeDelta.y);
                sp.transform.localPosition = new Vector3(-297, sp.transform.localPosition.y, sp.transform.localPosition.z);
                OnPTBtnClick();
            }
        }
    }

    public GameObject _ZSBtnPrefabs;
    public GameObject _PTBtnPrefabs;
    public GameObject _subZSMenu;
    private ZhuangShiBtnItem.eZSBtnType _PrevCurrentSubMenu = ZhuangShiBtnItem.eZSBtnType.ZSNone;
    public void OnZSBtnClick(){
        _subDZMenu.SetActive(false);
        _subHSMenu.SetActive(false);
        _subPTMenu.SetActive(false);
        _subTHMenu.SetActive(false);

        _subZSMenu.SetActive(true);
        _PrevCurrentSubMenu = ZhuangShiBtnItem.eZSBtnType.ZiShi;
    }

    public GameObject _subDZMenu;
    public void OnDZBtnClick(){
        _subZSMenu.SetActive(false);
        _subHSMenu.SetActive(false);
        _subPTMenu.SetActive(false);
        _subTHMenu.SetActive(false);

        _subDZMenu.SetActive(true);
        _PrevCurrentSubMenu = ZhuangShiBtnItem.eZSBtnType.DianZhui;
    }

    public GameObject _subHSMenu;
    public void OnHSBtnClick()
    {
        _subDZMenu.SetActive(false);
        _subZSMenu.SetActive(false);
        _subPTMenu.SetActive(false);
        _subTHMenu.SetActive(false);

        _subHSMenu.SetActive(true);
        _PrevCurrentSubMenu = ZhuangShiBtnItem.eZSBtnType.HangShi;
    }

    public GameObject _subPTMenu;
    public void OnPTBtnClick()
    {
        _subDZMenu.SetActive(false);
        _subHSMenu.SetActive(false);
        _subZSMenu.SetActive(false);
        _subTHMenu.SetActive(false);

        _subPTMenu.SetActive(true);
        _PrevCurrentSubMenu = ZhuangShiBtnItem.eZSBtnType.PeiTuShape;
    }

    public GameObject _subTHMenu;
    public void OnTHBtnClick()
    {
        _subDZMenu.SetActive(false);
        _subHSMenu.SetActive(false);
        _subZSMenu.SetActive(false);
        _subPTMenu.SetActive(false);

        _subTHMenu.SetActive(true);

        int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_THEME_CUSTOM_TIPS, 0);

        if (tipCnt < Define.BASIC_TIP_CNT)
        {
            ShowToast("<b>自定义</b>意境，可调整<b>天空</b>、<b>地面</b>、<b>太阳/月亮</b>等超多参数、效果",5f);
            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_THEME_CUSTOM_TIPS, tipCnt + 1);
        }

        _PrevCurrentSubMenu = ZhuangShiBtnItem.eZSBtnType.Theme;
    }

    //是否显示了配图，没有显示配图的时候，不能调用切换底图操作
    private bool _IsPicShowing = false;
    public void OnShowPic(bool show)
    {
        _IsPicShowing = show;
    }
    public void OnItemClick(ZhuangShiBtnItem.ZSParam item)
    {
        if (item.btnType == ZhuangShiBtnItem.eZSBtnType.DianZhuiShape)
        {

        }
        else if (item.btnType == ZhuangShiBtnItem.eZSBtnType.HangShiShape)
        {

        }
        else if (item.btnType == ZhuangShiBtnItem.eZSBtnType.PeiTuShape)
        {
            if(_IsPicShowing)
            {
                foreach (var zsObj in _PTBtnList)
                {
                    ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();
                    if (zsBtn.GetBtnID() != item.path)
                    {
                        zsBtn.SetSelected(false);
                    }
                    else
                    {
                        zsBtn.SetSelected(true);
                        OnZSAdjustFinish.Invoke(item);//所有参数也需要传递
                    }
                }
            }
            else
            {
               // OnZSAdjustFinish.Invoke(item);//测试
                ShowToast("需要<b>先选择照片</b>用作配图后，才可以使用相框 :)");
            }
        }
        else if (item.btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            //将除当前以外的所有可以调节设置为不可见
            foreach (var zsObj in _ZSBtnList)
            {
                ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();
                if (zsBtn.GetBtnID() != item.path)
                {
                    zsBtn.SetCanAdjust(false);
                }
                else
                {
                    if (zsBtn.GetCanAdjust())
                    {
                        //当前是选中状态，显示调节界面
                        if (zsBtn.GetCurrentZSItem().path != "")
                        {
                            _zsParamAdjust.ShowZSParamAdjust(true, zsBtn.GetBtnType());
                        }
                        else
                        {
                            _zsParamAdjust.SetZSImg("");//隐藏此时的底图
                        }
                    }
                    else
                    {
                        //当前不是选中状态，切换到该装饰，是否通知界面更新？
                        zsBtn.SetCanAdjust(true);
                        OnZSAdjustFinish.Invoke(item);//所有参数也需要传递

                        //设置当前调节参数为字饰底图参数
                        _zsParamAdjust.SetZSImg(item.path);
                        _zsParamAdjust.SetZSImgSize(item.size, false);
                        _zsParamAdjust.SetZSImgColor(item.color, false);
                        int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_PARAM_TIP, 0);

                        if (item.path != "" && tipCnt < Define.BASIC_TIP_CNT)
                        {
                            if (HZManager.GetInstance().GenerateRandomInt(0, 100) <= 50)
                            {
                                ShowToast("<b>再点一次</b>，可以调节更详细的<b>装饰</b>参数");
                                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_ADJUST_PARAM_TIP, tipCnt + 1);
                            }
                        }
                    }
                }
            }

        }
    }

    //非选中状态，需要禁用按钮的呼吸动画
    public void OnZiShiBtnClick()
    {
        _zsParamAdjust.ShowZSParamAdjust(true,ZhuangShiBtnItem.eZSBtnType.ZiShi);
        OnZSBtnClick();
    }

    public void OnDianZhuiBtnClick()
    {
       // OnDZBtnClick();
    }

    public void OnHangShiBtnClick(){
        _zsParamAdjust.ShowZSParamAdjust(true, ZhuangShiBtnItem.eZSBtnType.HangShi);

        //OnHSBtnClick();//目前不支持行线图形
    }

    private Color _BGColor;
    public void OnChangeBGColor(Color color,bool isChangeTheme)
    {
        _BGColor = color;
        _zsParamAdjust.ChangeBGColor(_BGColor);

        /* 效果不是很好，暂时不使用明暗变化
        Color d = new Color(0f,0f,0f,50/255f);
        Color l = new Color(200/255f, 200/255f, 200/255.0f, 50 / 255f);
        Color dl = Define.GetIfUIFontBlack(color)?d:l;
        foreach (var zsObj in _ZSBtnList)
        {
            ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();
            zsBtn.UpdateAdjustParamColor(dl);
        }
        */
    }
    public void OnLineRenderChangHSAlpha(float a)
    {
        _zsParamAdjust.LineRenderChangeHSAlpha(a);
    }

    [Serializable] public class OnZiShiClickEvent : UnityEvent<ZhuangShiBtnItem.ZSParam> { }
    public OnZiShiClickEvent OnZSAdjustFinish;
    public void OnZSParamAdjustFinish(Color c, float s)
    {
        ZhuangShiBtnItem.ZSParam param;
        param.btnType = _zsParamAdjust.GetCurrentAdjustType();
        param.path = "";
        param.size = 1.0f;
        param.color = Define.BG_COLOR_50;
        if (_zsParamAdjust.GetCurrentAdjustType() == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            ZhuangShiBtnItem zsBtn = null;
            foreach (var zsObj in _ZSBtnList)
            {
                zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();
                if (zsBtn.GetCanAdjust())
                {
                    break;
                }
            }

            if (zsBtn == null) return;
           
            param.path = zsBtn.GetBtnID();
            param.color = c;
            param.size = s;

            zsBtn.SetCurrentZSItem(param);
        }
        else if (_zsParamAdjust.GetCurrentAdjustType() == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            // 调节汉字的参数
            param.color = c;
        }
        else if (_zsParamAdjust.GetCurrentAdjustType() == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            param.color = c;
            param.size = s;
        }

        OnZSAdjustFinish.Invoke(param);
    }
    //-------------------------------购买装饰-----------------------------------
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
        if (inAppID != IAP.IAP_ZISHI && inAppID != IAP.IAP_PEITU)
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
                if(inAppID == IAP.IAP_ZISHI)
                {
                    foreach (var zsObj in _ZSBtnList)
                    {
                        ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();

                        DoUnLockAni(zsBtn.gameObject, () => {
                            zsBtn.ShowBuyBtn(false);
                        });
                    }
                }
                else if (inAppID == IAP.IAP_PEITU)
                {
                    foreach (var zsObj in _PTBtnList)
                    {
                        ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();

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
        if (inAppID != IAP.IAP_ZISHI && inAppID != IAP.IAP_PEITU)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            //如果lock不可见，不必执行动画
            if (gameObject.activeSelf)
            {
                if (inAppID == IAP.IAP_ZISHI)
                {
                    foreach (var zsObj in _ZSBtnList)
                    {
                        ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();

                        DoUnLockAni(zsBtn.gameObject, () => {
                            zsBtn.ShowBuyBtn(false);
                        });
                    }
                }
                else if (inAppID == IAP.IAP_PEITU)
                {
                    foreach (var zsObj in _PTBtnList)
                    {
                        ZhuangShiBtnItem zsBtn = zsObj.GetComponent<ZhuangShiBtnItem>();

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
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.ZSBtnBuyClick + "_" + success);
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = _BGColor;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }
}
