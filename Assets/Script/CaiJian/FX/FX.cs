/*
 *用于控制处理和曲线有关的逻辑
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

public class FX : MonoBehaviour
{
    public void Start()
    {

    }

    public void OnEnable()
    {
        foreach (var zsObj in _FXBtnList)
        {
            FXBtnItem btn = zsObj.GetComponent<FXBtnItem>();
            btn.ShowBuyBtn(!IAP.getHasBuy(IAP.IAP_FX));
        }
    }

    public GameObject _leanSelect;
    public Transform _FXBtnContent;
    public GameObject _FXBtnPrefabs;
    private List<GameObject> _FXBtnList = new List<GameObject>();

    public void OnInit()
    {
        LoadFXBtn();

        RectTransform rt = _FXBtnContent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120 * (int)FXLineRender.LineType.END, rt.rect.height);

        _FXLineRender.InitLineRender();
    }

    public int BasicFreeCnt = 20;
    private void LoadFXBtn()
    {
        //------------------------------初始化字饰按钮列表------------------------
        //首先添加“不使用”
        GameObject obj = Instantiate(_FXBtnPrefabs, _FXBtnContent) as GameObject;
        obj.SetActive(true);
        _FXBtnList.Add(obj);


        FXBtnItem btn = obj.GetComponent<FXBtnItem>();
        btn.Init(false, "", "" + 0,GetDefaultItem(""),FXLineRender.LineType.NONE);
                  
        for (int i = (int)(FXLineRender.LineType.NONE) + 1; i < (int)FXLineRender.LineType.END; i++)
        {
            //fi.
            obj = Instantiate(_FXBtnPrefabs, _FXBtnContent) as GameObject;
            obj.SetActive(true);

            _FXBtnList.Add(obj);

            btn = obj.GetComponent<FXBtnItem>();

            string btnID = "FX/Icon/F" + i;
            if(i > BasicFreeCnt)
            {
                btnID = "FX/Icon/NF" + i; 
            }
            btn.Init(i > BasicFreeCnt, btnID, "" + i,GetDefaultItem(btnID),(FXLineRender.LineType)i);

        }

    }

    public void OnChangeFontBold(bool bold)
    {
        _FXLineRender.SetFontStyle(bold);
    }

    public void OnSetParam(CJHZ.HZParam param)
    {
        _FXLineRender.SetParam(param);
    }
    public void OnZSAdjustFinish(ZhuangShiBtnItem.ZSParam param)
    {
        if (param.btnType == ZhuangShiBtnItem.eZSBtnType.ZiShi)
        {
            _FXLineRender.UpdateZSParam(param.color);
        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.ZiShiShape)
        {
            _FXLineRender.UpdateZSShapeParam(param.path, param.size, param.color);
        }
        else if (param.btnType == ZhuangShiBtnItem.eZSBtnType.HangShi)
        {
            _FXLineRender.UpdateHSParam(param.size, param.color);
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
    }

    public void OnFXParamAdjustFinish(FXBtnItem.FXParam param)
    {
        _FXLineRender.AdjustFXParam(param.HZAlphaRange, param.HZSizeRange, param.LineAlphaRange, param.LineSizeRange, param.RandomRange);
    }

    public GameObject _AdjustFX;
    private FilterAdjust.AdjustDirectionType _CurrentAdjustDirType = FilterAdjust.AdjustDirectionType.NONE;
    //手指离开屏幕即认为停止调整，此时重置调整框
    public void OnStopAdjustFX()
    {
        _AdjustFX.transform.localPosition = Vector3.zero;
        _CurrentAdjustDirType = FilterAdjust.AdjustDirectionType.NONE;
    }
    public void OnAdjustFX(Vector2 prevPos, Vector2 currentPos, PicAdjust.PicAdjustType type)
    {
        if (type != PicAdjust.PicAdjustType.ADJUST_FX) return;

        //限制在1024范围内，否则不予处理
        RectTransform picAreaRt = _FXArea.GetComponent<RectTransform>();
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;

        float newX = currentPos.x;
        float newY = currentPos.y;

        if (Math.Abs(newX) > picAreaRt.rect.width * sh)
        {
            //不能再移动
            _AdjustFX.transform.localPosition = prevPos;
            return;
        }

        if (Math.Abs(newY) > picAreaRt.rect.height * sh)
        {
            //不能再移动
            _AdjustFX.transform.localPosition = prevPos;
            return;
        }

        _AdjustFX.transform.localPosition = new Vector3(newX, newY, 0f);

        //只能同时移动一个方向，不能同时调整，不然会很乱
        float vaule = 0.0f;//是本次调节的变化值
        FilterAdjust.AdjustDirectionType dir = FilterAdjust.AdjustDirectionType.NONE;

        double a = Math.Atan2(currentPos.y - prevPos.y, currentPos.x - prevPos.x);
        if (a >= -Math.PI / 8 && a < Math.PI / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.RIGHT;
            vaule = (currentPos.x - prevPos.x) / (picAreaRt.rect.width * sh);
        }
        else if (a >= Math.PI / 8 && a < Math.PI * 3 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.UP_RIGHT;

            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }
        else if (a > Math.PI * 3 / 8 && a < Math.PI * 5 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.UP;
            vaule = (currentPos.y - prevPos.y) / (picAreaRt.rect.height * sh);
        }
        else if (a >= Math.PI * 5 / 8 && a < Math.PI * 7 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.LEFT_UP;
            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }
        else if ((a >= Math.PI * 7 / 8 && a <= Math.PI) || (a > -Math.PI && a < -Math.PI * 7 / 8))
        {
            dir = FilterAdjust.AdjustDirectionType.LEFT;
            vaule = (currentPos.x - prevPos.x) / (picAreaRt.rect.width * sh);
        }
        else if (a >= -Math.PI * 7 / 8 && a < -Math.PI * 5 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.DOWN_LEFT;
            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }
        else if (a >= -Math.PI * 5 / 8 && a < -Math.PI * 3 / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.DOWN;
            vaule = (currentPos.y - prevPos.y) / (picAreaRt.rect.height * sh);
        }
        else if (a >= -Math.PI * 3 / 8 && a < -Math.PI / 8)
        {
            dir = FilterAdjust.AdjustDirectionType.DOWN_RIGHT;
            float xy = Mathf.Sqrt(Mathf.Pow(currentPos.x - prevPos.x, 2) + Mathf.Pow(currentPos.y - prevPos.y, 2));
            vaule = xy / (picAreaRt.rect.width * sh * Mathf.Sqrt(2));
        }

        vaule = Mathf.Abs(vaule);
        if (vaule >= float.Epsilon)
        {
            AdjustFX(dir, vaule);
        }
    }


    private float GetRealValue(FilterAdjust.AdjustDirectionType type, float v)
    {
        if (type == FilterAdjust.AdjustDirectionType.LEFT
            || type == FilterAdjust.AdjustDirectionType.DOWN
            || type == FilterAdjust.AdjustDirectionType.DOWN_LEFT
            || type == FilterAdjust.AdjustDirectionType.LEFT_UP
            )
        {
            return -v;
        }

        return v;
    }

    [Serializable] public class OnLineRenderChangHSAlphaEvent : UnityEvent<float> { }
    public OnLineRenderChangHSAlphaEvent OnLineRenderChangHSAlpha;
    private void AdjustFX(FilterAdjust.AdjustDirectionType type,float v)
    {
        float realV = GetRealValue(type, v);//[-1,1]

        if (_CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.NONE)
        {
            _CurrentAdjustDirType = type;

            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_ADJUST_ONE_DIR_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                //一半的概率触发显示
                ShowToast("一次触摸仅能调整一个参数，请保持" + GetDirNameByType(_CurrentAdjustDirType) + "滑动");
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_ADJUST_ONE_DIR_TIP, tipCnt + 1);
            }
        }

        if (type == FilterAdjust.AdjustDirectionType.LEFT || type == FilterAdjust.AdjustDirectionType.RIGHT)
        {
            //优先使用左右调整
            //周期
            if (_CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.LEFT || _CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.RIGHT)
            {
                _FXLineRender.ChangeLinePeriod(realV);
            }
            else
            {
                //ShowToast("一次触摸调整仅能调整一个参数，请保持"+ GetDirNameByType(_CurrentAdjustDirType) +"滑动");
            }
        }
        else if (type == FilterAdjust.AdjustDirectionType.DOWN || type == FilterAdjust.AdjustDirectionType.UP)
        {
            //其次上下调整
            if (_CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.DOWN || _CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.UP)
            {
                _FXLineRender.ChangeLineAY(realV);
            }
            else
            {
                //ShowToast("一次触摸调整仅能调整一个参数，请保持" + GetDirNameByType(_CurrentAdjustDirType) + "滑动");
            }
        }
        else if (type == FilterAdjust.AdjustDirectionType.UP_RIGHT || type == FilterAdjust.AdjustDirectionType.DOWN_LEFT)
        {
            //再次右上左下
            if (_CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.UP_RIGHT || _CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.DOWN_LEFT)
            {
                _FXLineRender.ChangeHZSize(realV);
            }
            else
            {
                //ShowToast("一次触摸调整仅能调整一个参数，请保持" + GetDirNameByType(_CurrentAdjustDirType) + "滑动");
            }
        }
        else if (type == FilterAdjust.AdjustDirectionType.LEFT_UP || type == FilterAdjust.AdjustDirectionType.DOWN_RIGHT)
        {
            //最次左上右下
            if (_CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.LEFT_UP || _CurrentAdjustDirType == FilterAdjust.AdjustDirectionType.DOWN_RIGHT)
            {
                _FXLineRender.ChangeLineAlpha(realV);
                OnLineRenderChangHSAlpha.Invoke(_FXLineRender.GetLineAlpha());
            }
            else
            {
               // ShowToast("一次触摸调整仅能调整一个参数，请保持" + GetDirNameByType(_CurrentAdjustDirType) + "滑动");
            }
        }
    }
    private string GetDirNameByType(FilterAdjust.AdjustDirectionType type)
    {
        string ret = "左右/上下/斜方向";

        if (type == FilterAdjust.AdjustDirectionType.LEFT || type == FilterAdjust.AdjustDirectionType.RIGHT)
        {
            ret = "水平";
        }
        else if (type == FilterAdjust.AdjustDirectionType.DOWN || type == FilterAdjust.AdjustDirectionType.UP)
        {
            ret = "上下";
        }
        else if (type == FilterAdjust.AdjustDirectionType.UP_RIGHT || type == FilterAdjust.AdjustDirectionType.DOWN_LEFT)
        {
            ret = "右上/左下";
        }
        else if (type == FilterAdjust.AdjustDirectionType.LEFT_UP || type == FilterAdjust.AdjustDirectionType.DOWN_RIGHT)
        {
            ret = "右下/左上";
        }

        return ret;
    }
    public FXBtnItem.FXParam GetDefaultItem(string btnID)
    {
        FXBtnItem.FXParam item;
        item.lineType = FXLineRender.LineType.NONE;

        item.HZAlphaRange = 0.0f;
        item.RandomRange = 0;
        item.HZSizeRange = 0.0f;

        item.LineAlphaRange = 0.0f;
        item.LineSizeRange = 0.0f;
        return item;
    }

    public SubMenuCtrl _subMenuCtrl;

    public void OnClickFXBtn(){
        if(gameObject.activeSelf)
        {
            _subMenuCtrl.ShowSubMenu(SubMenuCtrl.eSubMenuType.SubMenu_FX, false );
        }
        else
        {
            _subMenuCtrl.ShowSubMenu(SubMenuCtrl.eSubMenuType.SubMenu_FX , true);

            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_BASIC_TIP, 0);
            if (tipCnt < Define.BASIC_TIP_CNT)
            {
                //一半的概率触发显示
                if (HZManager.GetInstance().GenerateRandomInt(0, 100) <= 50)
                {
                    ShowToast("<b>装饰</b>界面调整行线、字体等同样适用于<b>红线</b>模式", 4f);
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_BASIC_TIP, tipCnt + 1);
                }
            }
        }
    }

    private List<string> _CurrentSJList = new List<string>();
    private string _CurrentSJ = "";
    //需要切换回去时把诗句传回去
    public UnityEvent OnWillShowFXEvent;//切换、显示曲线，此时需要立即重置textarea
    public void OnSetSJ(List<string> sj,bool sal)
    {
        //保存原数据
        _CurrentSJ = "";
        _CurrentSJList.Clear();
        _CurrentSJList.AddRange(sj);

        //去掉作者行
        List<string> noAuthorLine = new List<string>();
        noAuthorLine.AddRange(sj);
        if (sal)
        {
            if (sj.Count > 1)
            {
                noAuthorLine.RemoveAt(noAuthorLine.Count - 1);
            }
        }

        foreach (var s in noAuthorLine)
        {
            string tmp = _CurrentSJ;
            _CurrentSJ = _CurrentSJ + s;
            if (_CurrentSJ.Length > Define.MAX_FX_HZ_NUM)
            {
                _CurrentSJ = tmp;
                break;
            }
        }

        if (_CurrentSJ.Length == 0)
        {
            if (noAuthorLine[0].Length > Define.MAX_FX_HZ_NUM)
            {
                if (_FXArea.activeSelf)
                    ShowToast("为了更好的显示体验，超过32个字会被自动截断", 3f);
                _CurrentSJ = noAuthorLine[0].Substring(0, Define.MAX_FX_HZ_NUM);
            }
            else
            {
                if (_FXArea.activeSelf)
                    ShowToast("至少显示一个字，如果想要只有红线的效果，请调整文字透明度。", 3f);

                _CurrentSJ = "诗色·字数不能超过三十二个。";
            }
        }

        //诗句切换，需要刷新
        if (_FXArea.activeSelf)
        {
            RectTransform fxAreaRt = _FXArea.GetComponent<RectTransform>();
            Setting.SpeedLevel SpeedLevel = Setting.GetSpeedLevel();
            float speed = (SpeedLevel == Setting.SpeedLevel.SPEED_LEVEL_8 ? 0.0f : 2.0f / (Mathf.Pow(2, (int)SpeedLevel)));

            float showTime = _CurrentSJ.Length * speed;
            _leanSelect.SetActive(false);//禁用leanselect

            float sh = 1.0f*Screen.height / FitUI.DESIGN_HEIGHT;

            OnWillShowFXEvent.Invoke();

            _FXLineRender.ShowChangeFXText(_CurrentSJ,
                (int)(fxAreaRt.sizeDelta.x * sh),
                (int)(fxAreaRt.sizeDelta.y * sh),
                showTime,
                Setting.GetUseHZAni(),
                ()=> {
                _leanSelect.SetActive(true);
            });
        }

    }
    public FXLineRender _FXLineRender;

    [Serializable] public class OnUseFXEvent : UnityEvent<bool,List<string> > { }
    public OnUseFXEvent OnUseFX;

    public GameObject _FXArea;
    public FXParamAdjust _FXParamAdjust;
    public void OnItemClick(FXBtnItem.FXParam item)
    {

        //只有当文本区域没有被选中的时候才可以切换
        if (DOTween.IsTweening("OnSelectDownSHIJU") || DOTween.IsTweening("OnSelectUpSHIJU"))
        {
            ShowToast("文本区域动画执行中，请结束后再切换曲线。");
            return;
        }

        foreach (var zsObj in _FXBtnList)
        {
            FXBtnItem zsBtn = zsObj.GetComponent<FXBtnItem>();
            if (zsBtn.GetLineType() != item.lineType)
            {
                zsBtn.SetCanAdjust(false);
            }
            else
            {
                if (zsBtn.GetCanAdjust())
                {
                    //当前是选中状态，显示调节界面
                    _FXParamAdjust.ShowFXParamAdjust(zsBtn.GetLineType(),
                        _FXLineRender.GetLineRenderCurrentHZParam(),
                        _FXLineRender.GetLineRenderHZSize());

                }
                else
                {
                    //当前不是选中状态，切换到该曲线
                    zsBtn.SetCanAdjust(true);
                    //首先调用
                    OnUseFX.Invoke(item.lineType != FXLineRender.LineType.NONE, _CurrentSJList);

                    if (item.lineType == FXLineRender.LineType.NONE)
                    {
                        _FXArea.SetActive(false);
                    }
                    else
                    {
                        _FXArea.SetActive(true);
                    }

                    float sh = Screen.height / FitUI.DESIGN_HEIGHT;

                    RectTransform fxAreaRt = _FXArea.GetComponent<RectTransform>();
                    Setting.SpeedLevel SpeedLevel = Setting.GetSpeedLevel();
                    float speed = (SpeedLevel == Setting.SpeedLevel.SPEED_LEVEL_8 ? 0.0f : 2.0f / (Mathf.Pow(2, (int)SpeedLevel)));

                    float showTime = _CurrentSJ.Length * speed;

                    _leanSelect.SetActive(false);//禁用leanselect
                    OnWillShowFXEvent.Invoke();
                    _FXLineRender.ShowFXText(item.lineType, _CurrentSJ,
                        (int)(fxAreaRt.sizeDelta.x * sh),
                        (int)(fxAreaRt.sizeDelta.y * sh),
                        showTime,
                        Setting.GetUseHZAni(),
                        () => {
                            _leanSelect.SetActive(true);
                        });

                    int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_TIP, 0);
                    if (tipCnt < Define.BASIC_TIP_CNT)
                    {
                        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_TIP, tipCnt + 1);
                        if (HZManager.GetInstance().GenerateRandomInt(0, 2) == 0)
                        {
                            ShowToast("<b>再点一次</b>可以调整更多红线效果。", 3f);
                        }
                        else
                        {
                            ShowToast("点选文字区域，<b>单指触摸</b>上下/左右/斜方向滑动调整效果", 3f);
                        }
                    }
                    else if (tipCnt < 3 * Define.BASIC_TIP_CNT)
                    {
                        int r = HZManager.GetInstance().GenerateRandomInt(0, 100);
                        if (r < 50)
                        {
                            ShowToast("点击<b>写信</b>制作自己的情书，点击<b>配图</b>添加照片。", 3f);
                            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_TIP, tipCnt + 1);
                        }
                        else if (r > 70)
                        {
                            ShowToast("<b>打开设置</b>，可改变红线绘制速度以及显隐。", 3f);
                            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_TIP, tipCnt + 1);
                        }
                        else
                        {
                            ShowToast("<b>打开设置</b>，可调整是否使用字体<b>粗体</b>。", 3f);
                            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_FX_TIP, tipCnt + 1);
                        }
                    }

                }
            }
        }
    }


    private Color _BGColor;
    public RawImage _FX;
    public Camera _FXCamera;
    private bool _HasSetFilterColorMaterial = false;
    public void OnChangeBGColor(Color color, bool isChangeTheme)
    {
        _BGColor = color;
        _FXParamAdjust.ChangeBGColor(_BGColor);

        //更新函数摄像机以及材质shader过滤颜色
        _FXCamera.backgroundColor = _BGColor;
      //  _FX.color = _BGColor;

        if (!_HasSetFilterColorMaterial)
        {
            _FX.material = new Material(Shader.Find("Custom/FilterColor"));
            _HasSetFilterColorMaterial = true;
        }

        _FX.material.SetColor("_FilterColor", _BGColor);
    }

    //-------------------------------购买曲线-----------------------------------
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
        if (inAppID != IAP.IAP_FX)
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
                if(inAppID == IAP.IAP_FX)
                {
                    foreach (var zsObj in _FXBtnList)
                    {
                        FXBtnItem zsBtn = zsObj.GetComponent<FXBtnItem>();

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
        if (inAppID != IAP.IAP_FX)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            //如果lock不可见，不必执行动画
            if (gameObject.activeSelf)
            {
                if (inAppID == IAP.IAP_FX)
                {
                    foreach (var zsObj in _FXBtnList)
                    {
                        FXBtnItem zsBtn = zsObj.GetComponent<FXBtnItem>();

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
