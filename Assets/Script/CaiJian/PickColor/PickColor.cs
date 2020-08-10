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
using System;
using System.Collections.Generic;
using Reign;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections;
using Mono.Data.Sqlite;

public class PickColor : MonoBehaviour {
    
    private static SQLiteHelper _ShiSeDB;
    // Use this for initialization
    private int _currentColorStorageCapacity = Define.MAX_FREE_PICK_COLOR_NUM;
    void Start()
    {
        _addPicView.SetActive(true);
        _picView.SetActive(false);
        _Picker.gameObject.SetActive(false);
        _thiefColorTips.gameObject.SetActive(true);
        _colorStorageText.gameObject.SetActive(false);
        _buyBtn.SetActive(false);
    }
    public void OnEnable()
    {
        if (IAP.getHasBuy(IAP.IAP_COLORSTORAGE))
        {
            _currentColorStorageCapacity = Define.MAX_BUY_PICK_COLOR_NUM;
        }
        else
        {
            _currentColorStorageCapacity = Define.MAX_FREE_PICK_COLOR_NUM;
        }
    }
    private void OnDestroy()
    {
        _ShiSeDB.CloseConnection();
    }

    public static bool CheckColorIDExist(int cId)
    {
        bool ret = false;

        int sysColorCnt = HZManager.GetInstance().GetColorCnt();
        if (cId >= sysColorCnt)
        {
            int pcId = cId - sysColorCnt;

            SqliteDataReader reader = _ShiSeDB.ReadTable(Define.PICK_COLOR_TABLE_NAME,
                 new string[] { "ID", "R", "G", "B", "NAME", "DES", "PY", "CTIME", "ETIME" },
                 new string[] { "ID" }, new string[] { "=" }, new string[] { "" + pcId });

            if (reader.Read())
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
        }
        else
        {
            ret = true;
        }

        return ret;
    }
    public static List<string> GetColorByID(int cId)
    {
        List<string> colorInfo = new List<string>();
        int sysColorCnt = HZManager.GetInstance().GetColorCnt();
        if (cId >= sysColorCnt)
        {
            int pcId = cId - sysColorCnt;

            SqliteDataReader reader = _ShiSeDB.ReadTable(Define.PICK_COLOR_TABLE_NAME,
                 new string[] { "ID", "R", "G", "B", "NAME", "DES", "PY" ,"CTIME","ETIME"},
                 new string[] { "ID" }, new string[] { "=" }, new string[] { ""+pcId });

            if (reader.Read())
            {
                colorInfo.Add(reader.GetString(reader.GetOrdinal("Name")));
                colorInfo.Add(reader.GetString(reader.GetOrdinal("PY")));
                Color c = new Color(reader.GetInt32(reader.GetOrdinal("R"))/255f,
                    reader.GetInt32(reader.GetOrdinal("G"))/255f,
                    reader.GetInt32(reader.GetOrdinal("B"))/255f);
                colorInfo.Add(Define.GetHexColor(c));
                colorInfo.Add("" + Define.GetIntColor(c.r));
                colorInfo.Add("" + Define.GetIntColor(c.g));
                colorInfo.Add("" + Define.GetIntColor(c.b));
                colorInfo.Add("" + ((int)HZManager.eColorType.PICKCOLOR));
                colorInfo.Add("" + cId);
                colorInfo.Add(reader.GetString(reader.GetOrdinal("DES")));

            }
            else
            {
                //不存在，说明存在问题，默认取白色
                colorInfo.AddRange(HZManager.GetInstance().GetColorByID(sysColorCnt));
                colorInfo.Add("");//添加描述
            }
        }
        else
        {
            colorInfo.AddRange(HZManager.GetInstance().GetColorByID(cId));
            colorInfo.Add("");//添加描述
        }

        return colorInfo;
    }
    public Text _colorCardTipText;
    [System.Serializable] public class OnLoadPickColorFinishEvent : UnityEvent<List<PickerColorItem.ColorItem>> { }
    public OnLoadPickColorFinishEvent OnLoadPickColorFinish;
    public void OnInit()
    {
        List<PickerColorItem.ColorItem> dt = new List<PickerColorItem.ColorItem>();
        //TIMESTAMP default (datetime('now', 'localtime')
        string qstr = "";
#if UNITY_EDITOR
        qstr = "data source=shise2.db";
#elif UNITY_IOS
        qstr = "data source=" + Application.persistentDataPath + "/shise2.db";
#endif

        _ShiSeDB = new SQLiteHelper(qstr);
        _ShiSeDB.CreateTable(Define.PICK_COLOR_TABLE_NAME, new string[] { "ID", "R", "G", "B", "NAME", "DES", "PY","CTIME","ETIME"},
            new string[] { "INTEGER primary key autoincrement", "INTEGER", "INTEGER", "INTEGER", "TEXT", "TEXT", "TEXT" ,"TEXT" , "TEXT" });

        SqliteDataReader reader = _ShiSeDB.ReadFullTable(Define.PICK_COLOR_TABLE_NAME);
        while (reader.Read())
        {
            PickerColorItem.ColorItem item;
            item.hasExist = true;
            item.colorCardId = reader.GetInt32(reader.GetOrdinal("ID"));
            item.itemId = -1;//此处事实上是没有itemID的
            item.color = new Color(reader.GetInt32(reader.GetOrdinal("R")) / 255f,
                reader.GetInt32(reader.GetOrdinal("G")) / 255f,
                reader.GetInt32(reader.GetOrdinal("B")) / 255f);
            item.colorName = reader.GetString(reader.GetOrdinal("NAME"));
            item.colorDes = reader.GetString(reader.GetOrdinal("DES"));
            item.py = reader.GetString(reader.GetOrdinal("PY"));

            item.isColorCard = true;//该值并没有用

            dt.Add(item);
        }

        InitColorCard(dt);

        _editColor.Init();

        OnLoadPickColorFinish.Invoke(dt);
    }

    //从当前的色库中随机获取一条颜色
    public List<string> GetPickColor()
    {
        List<string> ret = new List<string>();

        //有色卡的时候才返回颜色
        if (_ColorCardList.Count > 0)
        {
            int index = (int)(UnityEngine.Random.value * _ColorCardList.Count);
            index = index == _ColorCardList.Count ? _ColorCardList.Count - 1 : index;
            PickerColorItem item = _ColorCardList[index].GetComponent<PickerColorItem>();
            PickerColorItem.ColorItem citem = item.GetColorItem();

            ret.Add(citem.colorName);
            ret.Add(citem.py);
            ret.Add(Define.GetHexColor(citem.color));
            ret.Add("" + Define.GetIntColor(citem.color.r));
            ret.Add("" + Define.GetIntColor(citem.color.g));
            ret.Add("" + Define.GetIntColor(citem.color.b));
            ret.Add("" + ((int)HZManager.eColorType.PICKCOLOR));
            ret.Add("" + (citem.colorCardId + HZManager.GetInstance().GetColorCnt()));
            ret.Add(citem.colorDes);
        }

        return ret;
    }
    public int GetPickColorCnt()
    {
        return _ColorCardList.Count;
    }

    private List<GameObject> _ColorCardList = new List<GameObject>();
    public Transform _ColorCardContent;
    public GameObject _ColorCardPrefabs;
    private void InitColorCard(List<PickerColorItem.ColorItem> dt)
    {
        //初始化色卡 //原则是应该是时间顺序或者使用频率顺序-这里仅仅倒叙，也就是保存顺序
        for (int i = dt.Count - 1; i >= 0; i--)
        {
            AddColorCardToList(dt[i],true);
        }

        SetColorCordContentSize();

        _colorCardTipText.gameObject.SetActive(dt.Count == 0);
    }

    private void AddColorCardToList(PickerColorItem.ColorItem pc,bool init)
    {
        GameObject cc = Instantiate(_ColorCardPrefabs, _ColorCardContent) as GameObject;
        cc.SetActive(true);
        _ColorCardList.Add(cc);
        PickerColorItem pci = cc.GetComponent<PickerColorItem>();
        pci.Init(pc.colorCardId, pc.colorCardId, pc.color, pc.colorName, true, pc.colorDes, pc.py, true);

        if (!init)
        {
            //需要将这个添加的放到最前面
            foreach (var tmpcc in _ColorCardList)
            {
                tmpcc.transform.SetSiblingIndex(tmpcc.transform.GetSiblingIndex() + 1);
            }

            cc.transform.SetSiblingIndex(_ColorCardList[0].transform.GetSiblingIndex() - 1);
        }
    }

    private void SetColorCordContentSize()
    {
        //设置高度
        int row = _ColorCardList.Count / 2;
        if (_ColorCardList.Count % 2 != 0)
        {
            row += 1;
        }

        RectTransform jsrt = _ColorCardContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _ColorCardContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);
    }

    [System.Serializable] public class OnUsePickColorPicEvent : UnityEvent<Texture2D, List<string>, string> { }
    public OnUsePickColorPicEvent OnUsePickColorPic;
    public void OnUsePickColorBtnClick(PickerColorItem.ColorItem item)
    {
        int cid = GetPickColorID(item.color);
        if (cid == -1 && _ColorCardList.Count >= _currentColorStorageCapacity)
        {
            ShowToast("保存失败，色库最多可以保存" + _currentColorStorageCapacity + "个色卡。", 3f);
            return;
        }

        //需要设置背景色为当前
        OnChangeBGColor(item.color,false);
        //使用这个颜色+照片组合
        //需要跳转到主界面
        //截图作为配图
        RectTransform picAreaRt = _picView.GetComponent<RectTransform>();

        float startx = 0;
        float starty = 0;
        int ssw = (int)(picAreaRt.rect.width * _UIScale);
        startx = _picView.transform.position.x - ssw / 2;
        starty = _picView.transform.position.y - ssw / 2;

        Rect screenRect = new Rect(startx, starty, ssw, ssw);

        StartCoroutine(GetPicTexture(screenRect, ssw,ssw, item));
    }

    private IEnumerator GetPicTexture(Rect screenRect, int ssw, int ssh, PickerColorItem.ColorItem item )
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenShot = new Texture2D(ssw, ssh);
        screenShot.ReadPixels(screenRect, 0, 0);
        screenShot.Apply();

        int cid = AddUpdatePickColor(item);

        List<string> colorInfo = new List<string>();
        colorInfo.Add(item.colorName);
        colorInfo.Add(item.py);
        colorInfo.Add(Define.GetHexColor(item.color));
        colorInfo.Add("" + Define.GetIntColor(item.color.r));
        colorInfo.Add("" + Define.GetIntColor(item.color.g));
        colorInfo.Add("" + Define.GetIntColor(item.color.b));
        colorInfo.Add("" + ((int)HZManager.eColorType.PICKCOLOR));

        colorInfo.Add("" + (cid + HZManager.GetInstance().GetColorCnt()));

        OnUsePickColorPic.Invoke(screenShot,colorInfo, item.colorDes);
    }

    [System.Serializable] public class OnAddPickColorEvent : UnityEvent<PickerColorItem.ColorItem> { }
    public OnAddPickColorEvent OnAddPickColor;
    [System.Serializable] public class OnUpdatePickColorEvent : UnityEvent<int, string, string, string> { }
    public OnUpdatePickColorEvent OnUpdatePickColor;
    [System.Serializable] public class OnDelPickColorEvent : UnityEvent<int> { }
    public OnDelPickColorEvent OnDelPickColor;

    private int AddUpdatePickColor(PickerColorItem.ColorItem item)
    {
        //应该分为色库里的编辑颜色和取色器里的编辑颜色
        int cid = GetPickColorID(item.color);

        if (item.isColorCard)
        {
            //需要同时更新取色器界面可能存在的颜色
            if (Define.GetHexColor(item.color) == _pickingColor.GetHex())
            {
                _pickingColor.UpdateItem(cid, item.colorName, true, item.colorDes, item.py, false);
            }
            //一定要检查色系列表，因为取色可能和色系中的相同，所以这里不是else
            //else
            {
                foreach (var tc2 in _ThiefColorList)
                {
                    PickerColorItem pc2 = tc2.GetComponent<PickerColorItem>();
                    if (pc2.GetHex() == Define.GetHexColor(item.color))
                    {
                        pc2.UpdateItem(cid, item.colorName, true, item.colorDes, item.py, false);
                        break;
                    }
                }
            }
        }
        else
        {
            if (item.itemId == 0)
            {
                _pickingColor.UpdateItem(cid, item.colorName, true, item.colorDes, item.py, false);
            }
            else
            {
                GameObject tc = _ThiefColorList[item.itemId - 1];
                PickerColorItem pc = tc.GetComponent<PickerColorItem>();
                pc.UpdateItem(cid, item.colorName, true, item.colorDes, item.py, false);
            }
        }

        if (cid == -1)
        {
            //不存在，添加
            _ShiSeDB.InsertValues(Define.PICK_COLOR_TABLE_NAME,
                new string[] { "NULL",
                    "" +Define.GetIntColor(item.color.r),
                    "" + Define.GetIntColor(item.color.g),
                    "" + Define.GetIntColor(item.color.b),
                    "'"+item.colorName+"'",
                    "'"+item.colorDes+"'",
                    "'"+item.py+"'",
                "'"+ Define.GetFmtTime()+"'",
                 "'"+Define.GetFmtTime()+"'"});

            //只有添加才需要生成color-card
            PickerColorItem.ColorItem colorCardItem;
            colorCardItem.hasExist = true;
            colorCardItem = item;

            colorCardItem.colorCardId = GetLastInsertID();//需要修改colorid为对应的色卡id
            AddColorCardToList(colorCardItem, false);
            SetColorCordContentSize();
            _colorCardTipText.gameObject.SetActive(false);//删除的时候需要设置

            OnAddPickColor.Invoke(colorCardItem);

            return colorCardItem.colorCardId;
        }
        else
        {
            _ShiSeDB.UpdateValues(Define.PICK_COLOR_TABLE_NAME,
                new string[] { "NAME", "DES", "PY" ,"ETIME"},
                new string[] { "'" + item.colorName + "'", "'" + item.colorDes + "'", "'" + item.py + "'","'" + Define.GetFmtTime()+ "'" },
                "ID", "=", "" + cid);

            //存在，更新，只有名字、拼音、描述可以更新
            //更新color-card
            foreach (var cc in _ColorCardList)
            {
                PickerColorItem tmp = cc.GetComponent<PickerColorItem>();

                if (cid == tmp.GetColorID())
                {
                    tmp.UpdateItem(cid, item.colorName, true, item.colorDes, item.py, true);
                    break;
                }
            }

            OnUpdatePickColor.Invoke(item.colorCardId,item.colorName,item.colorDes,item.py);

            return cid;
        }
    }
    public void OnSavePickColor(PickerColorItem.ColorItem item)
    {
        int cid = GetPickColorID(item.color);
        if (cid == -1 && _ColorCardList.Count >= _currentColorStorageCapacity)
        {
            ShowToast("保存失败，色库最多可以保存"+ _currentColorStorageCapacity + "个色卡。", 3f);
            return;
        }

        AddUpdatePickColor(item);
    }

    private int GetLastInsertID()
    {
        int sid = -1;
        SqliteDataReader reader2 = _ShiSeDB.LastInsertRowid(Define.PICK_COLOR_TABLE_NAME);
        if (reader2.Read())
        {
            sid = reader2.GetInt32(0);
        }

        return sid;
    }
    private bool GetIfColorExist(Color c)
    {
        return GetPickColorID(c) != -1;
    }

    private int GetPickColorID(Color c)
    {
        int index = -1;
        for (int i = 0;i< _ColorCardList.Count;i++)
        {
            PickerColorItem tmp = _ColorCardList[i].GetComponent<PickerColorItem>();
            PickerColorItem.ColorItem item =  tmp.GetColorItem();
            if (CheckEquals(item.color.r, c.r)
                && CheckEquals(item.color.g, c.g)
                && CheckEquals(item.color.b, c.b))
            {
                index = item.colorCardId;
                break;
            }
        }

        return index;
    }

    private void GetPickColorItemByColor(Color c,ref int cid, ref string colorName, ref string colorDes, ref string py)
    {
        for (int i = 0; i < _ColorCardList.Count; i++)
        {
            PickerColorItem tmp = _ColorCardList[i].GetComponent<PickerColorItem>();
            PickerColorItem.ColorItem item = tmp.GetColorItem();
            if (CheckEquals(item.color.r, c.r)
                && CheckEquals(item.color.g, c.g)
                && CheckEquals(item.color.b, c.b))
            {
                cid = item.colorCardId;
                colorName = item.colorName;
                colorDes = item.colorDes;
                py = item.py;
                break;
            }
        }
    }

    private bool CheckEquals(float c1,float c2)
    {
        return (int)(c1 * 255) == (int)(c2 * 255);
    }

    public EditColor _editColor;
    [System.Serializable] public class OnShowDialogEvent : UnityEvent<Color, MaskTips.DialogParam> { }
    public OnShowDialogEvent OnShowDialog;
    public void OnDelColorItem(PickerColorItem.ColorItem item)
    {
        OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam("删除不可恢复，确定删除吗？",
            MaskTips.eDialogType.OK_CANCEL_BTN,
            (MaskTips.eDialogBtnType type) =>
            {
                if (type == MaskTips.eDialogBtnType.OK)
                {

                    // 这里需要做较多的事情，诸如更新收藏等相关用到的colorid

                    //首先删除colorcard
                    for (int i = 0; i < _ColorCardList.Count; i++)
                    {
                        PickerColorItem tcc = _ColorCardList[i].GetComponent<PickerColorItem>();
                        if (tcc.GetColorID() == item.colorCardId)
                        {
                            Destroy(_ColorCardList[i]);
                            _ColorCardList.RemoveAt(i);
                            break;
                        }
                    }

                    SetColorCordContentSize();

                    if (_ColorCardList.Count == 0)
                    {
                        _colorCardTipText.gameObject.SetActive(true);
                    }

                    //更新取色器界面可能的颜色
                    if (Define.GetHexColor(item.color) == _pickingColor.GetHex())
                    {
                        _pickingColor.UpdateItem(-1, "尚未取色", false, "", "尚未取色", false);
                    }
                    //一定要检查色系列表，因为取色可能和色系中的相同，所以这里不是else
                    //else
                    {
                        foreach (var tc2 in _ThiefColorList)
                        {
                            PickerColorItem pc2 = tc2.GetComponent<PickerColorItem>();
                            if (pc2.GetHex() == Define.GetHexColor(item.color))
                            {
                                bool isMainColor = pc2.GetItemID() == (int)PickerColorItem.eColorIDType.MainColorID;
                                string tcname = "主色";
                                if (!isMainColor) tcname = "副色" + "(" + (pc2.GetItemID() - 1) + ")";
                                pc2.UpdateItem(-1, tcname, false, "", "", false);
                                break;
                            }
                        }
                    }

                    _ShiSeDB.DeleteValuesAND(Define.PICK_COLOR_TABLE_NAME,
                        new string[] { "ID" }, new string[] { "=" }, new string[] { "" + item.colorCardId });

                    OnDelPickColor.Invoke(item.colorCardId);

                    _colorStorageText.text = "(" + _ColorCardList.Count + "/" + _currentColorStorageCapacity + ")";

                }
                else
                {
                    //关闭弹窗
                }
            }));
    }

    public void OnClickColorItem(PickerColorItem.ColorItem item)
    {
        //色卡不需要判断
        if (!item.isColorCard)
        {
            if (!_hasPickedColor && item.itemId == (int)PickerColorItem.eColorIDType.PickingColorID)
            {
                if (_ThiefColorList.Count == 0)
                {
                    ShowToast("取色后才能编辑色卡，请先添加照片取色", 3f);
                }
                else
                {
                    ShowToast("请<b>单指触摸</b>照片滑动取色，再编辑色卡", 3f);
                }

                return;
            }

            //显示颜色编辑界面
            _Picker.gameObject.SetActive(false);
            _editColor.ShowEditColor(item, _ColorCardList.Count == _currentColorStorageCapacity, () =>
            {
                _Picker.gameObject.SetActive(true);
            });
        }
        else
        {
            //打开色卡编辑，本质上只有更新，因为色卡里一定是保存过的
            _editColor.ShowEditColor(item, _ColorCardList.Count == _currentColorStorageCapacity);
        }
    }

    //从辨色游戏打开色库
    private bool _colorStorageOpenByColorGame = false;
    public void OnColorGameOpenColorStorage()
    {
        OnClickOpenColorPanel();

        _colorStorageOpenByColorGame = true;//重新设置为true
    }
    //此处是展示全屏颜色列表，并且可编辑
    //主界面的颜色列表为非全屏，仅可选择使用，不能编辑信息
    public void OnClickOpenColorPanel()
    {
        _colorStorageOpenByColorGame = false;//这个接口先设置为false

        _colorStorageView.SetActive(true);
        _pickColorView.SetActive(false);
        _colorStorageBtn.SetActive(false);
        _openBtn.SetActive(false);
        _titleText.text = "色卡库";
        _colorStorageText.gameObject.SetActive(true);
        //同时更新目前的容量数显示
        if (!IAP.getHasBuy(IAP.IAP_COLORSTORAGE))
        {
            _buyBtn.SetActive(true);
        }
        else
        {
            _buyBtn.SetActive(false);
        }

        _colorStorageText.text = "("+_ColorCardList.Count +"/"+_currentColorStorageCapacity+")";
    }

    public IAP _iap;
    private bool _ProcessingPurchase = false;

    //此处需要判断购买的是哪个内购
    public void OnBuyBtnClick()
    {

        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            _iap.onBuyClick(IAP.IAP_COLORSTORAGE);
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍后...");
        }
    }

    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        if (inAppID != IAP.IAP_COLORSTORAGE)
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

            _currentColorStorageCapacity = Define.MAX_BUY_PICK_COLOR_NUM;
            _colorStorageText.text = "(" + _ColorCardList.Count + "/" + _currentColorStorageCapacity + ")";
            _buyBtn.SetActive(false);

            OnReportEvent(ret, EventReport.BuyType.BuySuccess);

            ShowToast("购买成功，色库容量扩充为" + _currentColorStorageCapacity + "个");

            if (_editColor.transform.parent.gameObject.activeSelf)
            {
                _editColor.UpdateFullColseBtn(false);
            }
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
        if (inAppID != IAP.IAP_COLORSTORAGE)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            _currentColorStorageCapacity = Define.MAX_BUY_PICK_COLOR_NUM;
            _colorStorageText.text = "(" + _ColorCardList.Count + "/" + _currentColorStorageCapacity + ")";
            _buyBtn.SetActive(false);

            OnReportEvent(ret, EventReport.BuyType.BuySuccess);

            if (_editColor.transform.parent.gameObject.activeSelf)
            {
                _editColor.UpdateFullColseBtn(false);
            }
        }
        else
        {
            //MessageBoxManager.Show("", "恢复购买失败，请确认是否购买过？");
        }

        OnReportEvent(ret, EventReport.BuyType.BuyRestore);
    }

    [System.Serializable] public class OnEventReport : UnityEvent<string> { }
    public OnEventReport ReportEvent;
    public void OnReportEvent(bool success,
                            EventReport.BuyType buyType)
    {
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.ColorStorageBtnBuyClick + "_" + success);
    }

    [Serializable] public class OnBackEevnt : UnityEvent<Action> { }
    public OnBackEevnt OnBack;
    public GameObject _openBtn;
    public GameObject _colorStorageBtn;
    public GameObject _buyBtn;
    public Text _titleText;
    public GameObject _pickColorView;
    public GameObject _colorStorageView;
    public Text _colorStorageText;
    public void OnBackBtnClick()
    {
        if (_colorStorageView.activeSelf)
        {
            if (_colorStorageOpenByColorGame)
            {
                _colorStorageOpenByColorGame = false;
                //此设置应该在界面关闭以后触发
                OnBack.Invoke(BackToPickColorView);
            }
            else
            {
                BackToPickColorView();
            }
        }
        else
        {
            OnBack.Invoke(null);
        }
    }
    private void BackToPickColorView()
    {
        _colorStorageView.SetActive(false);
        _pickColorView.SetActive(true);
        _colorStorageBtn.SetActive(true);
        _openBtn.SetActive(true);
        _titleText.text = "取色器";
        _buyBtn.SetActive(false);
        _colorStorageText.gameObject.SetActive(false);
    }

    public void OnSelectPicBtnClick()
    {
        _Picker.gameObject.SetActive(false);

        StreamManager.LoadFileDialog(FolderLocations.Pictures, new string[] { ".png", ".jpg", ".jpeg" }, imgSelectCallback, true);
    }

    private Texture2D _tmpLoadImageTexture = null;
    private Sprite _tmpLoadImageSprite = null;
    public void imgSelectCallback(Stream stream, bool succeeded)
    {
        if (!succeeded)
        {
            if (stream != null) stream.Dispose();

            //不选择认为不想取色，这里仅仅是为了测试，内置图属性也要修改不能读取
            //TestSprite("icon/1");
            if (!_hasSelectPic)
            {
#if UNITY_EDITOR
                TestSprite("icon/1");
#else
                ShowToast("选择图片才能取色哟 :)");
#endif
            }
            else
            {
                _Picker.gameObject.SetActive(true);
            }

            return;
        }

        try
        {
            if (_tmpLoadImageTexture != null)
            {
                DestroyImmediate(_tmpLoadImageTexture);
                DestroyImmediate(_tmpLoadImageSprite);

                _tmpLoadImageTexture = null;
                _tmpLoadImageSprite = null;
                GC.Collect();
            }

            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            _tmpLoadImageTexture = new Texture2D(4, 4);
            _tmpLoadImageTexture.LoadImage(data);
            _tmpLoadImageTexture.Apply();

            _tmpLoadImageSprite = Sprite.Create(_tmpLoadImageTexture,
                                        new Rect(0, 0, _tmpLoadImageTexture.width, _tmpLoadImageTexture.height),
                                        new Vector2(.5f, .5f));

            ShowProcessPic(_tmpLoadImageSprite);

            //设置为已经选过图了
            _hasSelectPic = true;
        }
        catch (Exception e)
        {
            ShowToast("读取图片出错了，请重试:(");
        }
        finally
        {
            // NOTE: Make sure you dispose of this stream !!!
            if (stream != null) stream.Dispose();
        }
    }
    private bool _isPickingColor = false;
    public void OnStopPickColor()
    {
        if (_isPickingColor)
        {
            _Picker.GetComponent<Image>().color = new Color(0f,0f,0f, 0f);

            _isPickingColor = false;
            //给予提示
            int tipCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_PICKCOLOR_CLICK_EDIT_TIPS, 0);

            //一半概率触发提示
            if (UnityEngine.Random.value < 0.5f)
            {
                if (tipCnt < Define.BASIC_TIP_CNT)
                {
                    ShowToast("<b>点击色卡</b>，编辑色卡信息及创建添加到色库。", 3f);
                    Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_PICKCOLOR_CLICK_EDIT_TIPS, tipCnt + 1);
                }
            }

            Color pc = _pickingColor.GetPC();
            string colorName = "尚未命名";
            string colorDes = "";
            string py = "";
            int pcid = -1;

            GetPickColorItemByColor(pc, ref pcid, ref colorName, ref colorDes, ref py);
            _pickingColor.Init(pcid, 0, pc, colorName, pcid != -1,colorDes, py, false);
        }
    }

    public Image _SelcetPic;
    public Picker _Picker;
    public PickerColorItem _pickingColor;
    private bool _hasPickedColor = false;
    private bool _hasSelectPic = false;
    public Text _thiefColorTips;
    public void OnPickColor()
    {
        _isPickingColor = true;
        
        try
        {
            int picw = _SelcetPic.sprite.texture.width;
            int pich = _SelcetPic.sprite.texture.height;
            float s = _SelcetPic.transform.localScale.x;
            Rect rt = _Picker.GetPicRect();

            float x = _Picker.transform.position.x - rt.x;
            float y = _Picker.transform.position.y - rt.y;

            int picx = (int)(x / s/ _UIScale);
            int picy = (int)(y / s/ _UIScale);
            // x,y需要转换为对应的照片的x,y
            if (picx > picw) picx = picw - 1;
            if (picy > pich) picy = pich - 1;

            if (picx < 0) picx = 0;
            if (picy < 0) picy = 0;

            Color c = _SelcetPic.sprite.texture.GetPixel(picx,picy);//_PicColors[picx * picw + picy];

            _pickingColor.Init(-1,0,c,"正在取色",false, "正在取色", "",false);// 吸取颜色过程中，不判断颜色是否已经存在

            _hasPickedColor = true;

            _Picker.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1.0f);


        }
        catch (Exception e)
        {
            //Debug.Log(e);
        }
    }

    public GameObject _picView;
    public GameObject _addPicView;
    public void TestSprite(string path)
    {
        Sprite sp = Resources.Load(path, typeof(Sprite)) as Sprite;
        ShowProcessPic(sp);
    }
    private float _UIScale = 1.0f;
    private void SetUIScale()
    {
        _UIScale = gameObject.transform.Find("Mid").localScale.x * Screen.height / AutoFitUI.DESIGN_HEIGHT;
    }
    private void ShowProcessPic(Sprite sp)
    {
        _addPicView.SetActive(false);
        _picView.SetActive(true);

        _SelcetPic.sprite = sp;
        _SelcetPic.SetNativeSize();
        RectTransform picAreaRt = _picView.GetComponent<RectTransform>();

        float x = picAreaRt.rect.width / _SelcetPic.sprite.texture.width;
        float y = picAreaRt.rect.height / _SelcetPic.sprite.texture.height;

        //设置相机大小
        float s = 1.0f;
        if (x < 1.0f && y < 1.0f)
        {
            s = x < y ? x : y;
        }
        else if (x >= 1.0f && y < 1.0f)
        {
            s = y;
        }
        else if (x < 1.0f && y >= 1.0f)
        {
            s = x;
        }
        //需要放大到图片区域，不能使用原图，否则太小
        else if (x >= 1.0f && y >= 1.0f)
        {
            s = Math.Min(x, y);
        }

        _SelcetPic.transform.localPosition = new Vector3(picAreaRt.rect.width/2,-picAreaRt.rect.height / 2,0);
        _SelcetPic.transform.localScale = new Vector3(s, s, 1);

        //设置可吸取颜色范围rect
        SetUIScale();//原则上只需要设置一次
        float startx = 0;
        float starty = 0;
        int fixoff = 4;//为了防止截图大于实际图出现底图偏差
        int awh = (int)(picAreaRt.rect.width * _UIScale);
        int ssw = (int)(_SelcetPic.sprite.texture.width * s) - fixoff;
        int ssh = (int)(_SelcetPic.sprite.texture.height * s) - fixoff;

        if (_SelcetPic.sprite.texture.width <= _SelcetPic.sprite.texture.height)
        {
            ssw = (int)(_SelcetPic.sprite.texture.width * s * _UIScale);
            ssh = awh;

            startx = _picView.transform.position.x - ssw/ 2;
            starty = _picView.transform.position.y - ssh/2;
        }
        else
        {
            ssw = awh;
            ssh = (int)(_SelcetPic.sprite.texture.height * s * _UIScale);

            startx = _picView.transform.position.x - ssw/2;
            starty = _picView.transform.position.y - ssh / 2;
        }

        ssw -= fixoff;
        ssh -= fixoff;

        Rect screenRect = new Rect(startx + fixoff/2, starty + fixoff/2, ssw,ssh);
        _Picker.SetRect(screenRect);

        StartCoroutine(GetPixels(screenRect,ssw,ssh));
    }

    private IEnumerator GetPixels(Rect screenRect,int ssw,int ssh)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenShot = new Texture2D(ssw, ssh);
        screenShot.ReadPixels(screenRect, 0, 0);
        screenShot.Apply();

        Color32[] pixels = screenShot.GetPixels32();

        Destroy(screenShot);
        screenShot = null;

        UpdateThiefColorAsync(ssw, ssh, pixels);
    }

    private void HideThiefColorTips(float waitTime)
    {
        _thiefColorTips.GetComponent<Text>().text = "正在获取图片主色系及副色系...";
        _thiefColorTips.gameObject.SetActive(true);
        float a = _thiefColorTips.color.a;
        Sequence loadingHide = DOTween.Sequence();
        loadingHide
            .AppendInterval(waitTime)
            .Append(_thiefColorTips.DOFade(0f, 0.5f))
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                _thiefColorTips.color = new Color(_thiefColorTips.color.r, _thiefColorTips.color.g, _thiefColorTips.color.b, a);
                _thiefColorTips.gameObject.SetActive(false);
            });
    }

    //10种颜色，太多的意义不大
    private int MAX_PALETTE_COLOR_NUM = 10;//+1
    public GameObject _thiefColorPrefabs;
    public Transform _thiefColorContent;
    private List<GameObject> _ThiefColorList = new List<GameObject>();
    public void UpdateThiefColorAsync(int w,int h,Color32[] pixels)
    {
        DestroyObj(_ThiefColorList);

        HideThiefColorTips(1f);

        var palette = new PickColorThief();

        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                //主色必须是5个的为最好
                var dominantColor = palette.GetColor(w, h, pixels).UnityColor;
                //次要颜色
                List<Color> colors = palette.GetPaletteColorList(w, h, pixels, MAX_PALETTE_COLOR_NUM + 1, (int)(Math.Sqrt(w * h / 10000)));

                Loom.QueueOnMainThread((param) =>
                {
                    UpdateThiefColor(dominantColor, colors);
                    Array.Clear(pixels, 0, pixels.Length);
                    pixels = null;
                    palette = null;
                    GC.Collect();
                }, null);
            });

            thread.Start();
        });
    }
    public void UpdateThiefColor(Color dominantColor, List<Color> colors)
    {
        AddThiefColorItem(1, dominantColor, "主色");

        //不超过 MAX_PALETTE_COLOR_NUM
        int cnt = colors.Count >= MAX_PALETTE_COLOR_NUM? MAX_PALETTE_COLOR_NUM : colors.Count;
        for (int i = 0; i < cnt; i++)
        {
            AddThiefColorItem(i + 2, colors[i], "副色(" + (i + 1) + ")");
        }

        //去除重复的颜色
        try
        {
            RemoveDuplicate();
        }
        catch (Exception e)
        {
            //Debug.Log("");
        }

        RectTransform rt = _thiefColorContent.GetComponent<RectTransform>();
        GridLayoutGroup gl = _thiefColorContent.GetComponent<GridLayoutGroup>();
        float rtw = _ThiefColorList.Count * gl.cellSize.x + gl.spacing.x * (_ThiefColorList.Count - 1) + gl.padding.left + gl.padding.right;
        rt.sizeDelta = new Vector2(rtw, rt.sizeDelta.y);

        _Picker.gameObject.SetActive(true);
        _Picker.GetComponent<Image>().color = new Color(0f,0f,0f, 0.0f);
    }

    private void RemoveDuplicate()
    {
        for (int i = _ThiefColorList.Count - 1;i >= 0;i--)
        {
            for (int j = _ThiefColorList.Count - 1; j >= 0; j--)
            {
                if (i == j) continue;

                PickerColorItem ipc = _ThiefColorList[i].GetComponent<PickerColorItem>();
                PickerColorItem jpc = _ThiefColorList[j].GetComponent<PickerColorItem>();
                if (ipc.GetHex() == jpc.GetHex())
                {
                    Destroy(_ThiefColorList[i]);
                    _ThiefColorList.RemoveAt(i);
                }
            }
        }

        //重新设置cid --只有副色才需要更新id
        for (int i = 0;i < _ThiefColorList.Count;i++)
        {
            PickerColorItem ipc = _ThiefColorList[i].GetComponent<PickerColorItem>();
            ipc.UpdateId(i+1);
        }
    }

    private void AddThiefColorItem(int cid,Color c,string cname)
	{
		GameObject tc = Instantiate(_thiefColorPrefabs, _thiefColorContent) as GameObject;
		tc.SetActive(true);
		_ThiefColorList.Add(tc);
		PickerColorItem pci = tc.GetComponent<PickerColorItem>();

        string colorName = cname;
        string colorDes = "";
        string py = "";
        int pcid = -1;

        GetPickColorItemByColor(c, ref pcid, ref colorName, ref colorDes, ref py);

        pci.Init(pcid, cid, c, colorName, pcid != -1, colorDes, py, false);

    }
    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    public Image _BG;
    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f, float delay = 0.0f)
    {
        ShowToast(content, _BG.color, showTime, delay);
    }

    public void ShowToast(string content, Color c, float showTime = 2.0f, float delay = 0.0f)
    {
        Toast.ToastData data;
        data.c = c;
        data.delay = delay;
        data.im = true;
        data.showTime = showTime;
        data.content = content;

        OnShowToast.Invoke(data);
    }
    public void ShowToast2(string content, float showTime = 2.0f)
    {
        ShowToast(content, showTime);
    }

    public void OnChangeBGColor(Color color, bool isChangeTheme)
    {
        Color fc = Define.GetFixColor(color);

        _BG.color = color;
        Image[] AllImgs = gameObject.GetComponentsInChildren<Image>(true);
        foreach (var img in AllImgs)
        {
            if (img.name.Contains("Dark"))
            {
                img.color = Define.GetDarkColor(fc);
                if (img.name.Contains("Picker"))
                {
                    //改变颜色的时候，吸取笔一定是不可见的，这里设置为透明
                    _Picker.GetComponent<Image>().color = new Color(0f,0f,0f, 0.0f);
                }
            }
            else if (img.name.Contains("Light"))
            {
                img.color = Define.GetLightColor(fc);
            }
            else if (img.name == "BtnImg")
            {
                img.color = fc;
            }
        }

        UpdateUIFontColor();

        //设置取色板
        if (!_hasPickedColor)
        {
            _pickingColor.Init(-1,0, color, "尚未取色", false, "尚未取色", "",false);
        }
    }
    private void UpdateUIFontColor()
    {
        // 此时应该改变界面上所有字体颜色
        Text[] allTxt = gameObject.GetComponentsInChildren<Text>(true);
        Color c0 = Define.GetUIFontColorByBgColor(_BG.color, Define.eFontAlphaType.FONT_ALPHA_128);
        Color c1 = new Color(c0.r, c0.g, c0.b, 50 / 255f);
        foreach (var txt in allTxt)
        {
            if (txt.name.Contains("Color"))
            {
                continue;
            }

            if (txt.name == "Footer")
            {
                txt.color = c1;
            }
            else
            {
                // 使用黑字
                if (!txt.name.Contains("Placeholder")
                    && !txt.name.Contains("ResultTip"))
                {
                    txt.color = c0;
                }
                else
                {
                    txt.color = c1;
                }
            }
        }
    }
}
