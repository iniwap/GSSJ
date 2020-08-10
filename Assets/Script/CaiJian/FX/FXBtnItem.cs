using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class FXBtnItem : MonoBehaviour
{
    public void Start()
    {

    }

    public struct FXParam{
        public FXLineRender.LineType lineType;
        //参数调节界面可以调整的参数如下
        public float HZAlphaRange;//[0,1]字alpha渐变，百分比为降到最低的位置
        public float HZSizeRange;//[0,1]字size渐变，百分比为降到最低的位置，最低后再到最大
        public float RandomRange;//[0,1]大小随机性，事实上可以只针对底图，因为字体已经有变化范围，如果再加上比例有点奇怪
        public float LineAlphaRange;//[0,1]线条的alpha渐变，指的是起始alpha和终止alpha的比例
        public float LineSizeRange;//
    }

    private string _btnID;

    public GameObject _buyBtn;
    public Text _btnText;
    public Image _btnImg;
    public FXLineRender.LineType _LineType = FXLineRender.LineType.END;

    private FXParam _CurrentFXItem;

    private  string[] FXName = {
        "直线",
        "圆",
        "椭圆",
        "抛物线",
        "对数",
        "正弦",
        "正切",
        "正态分布",
        "苹果",
        "爱心",
        "心电图",
        "弹簧",
        "狮吼功",
        "刀光",
        "弯月",
        "街口",
        "旋风",
        "四方",
        "山路",
        "五星",
        "挥手",

        "海生物",
        "三叶草",
        "暗器",
        "金钩",
        "深渊",
        "篱笆墙",
        "葫芦",
        "蚊香",
        "花生",
        "梅花",
        "水仙",
        "阡陌",
        "蜻蜓",
        "梳篦",
        "中国结",
        "襁褓",
        "糖果",
        "隧道",
        "山脉",
        "涟漪",
        "章鱼",
        "燕剪尾",
        "紧箍咒",
        "玉兔",
        "青烟",
        "发簪",
        "刺绣",
        "丝线",
        "蝴蝶",
        "寒蝉",
    };

    public void Init(bool needBuy,string btnID,string btnText, FXParam item, FXLineRender.LineType type)
    {
        _btnID = btnID;
        _btnText.text = FXName[(int)type];
        if (btnID != "")
        {
            _btnImg.sprite = Resources.Load("FX/Icon/"+type, typeof(Sprite)) as Sprite;
        }
        else
        {  
            _adjustParam.SetActive(true);
            Image[] adj = _adjustParam.GetComponentsInChildren<Image>();
            adj[1].gameObject.SetActive(false);
            adj[2].gameObject.SetActive(false);
            _btnImg.sprite = Resources.Load("icon/minus", typeof(Sprite)) as Sprite;

            _btnImg.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        _buyBtn.SetActive(needBuy);

        _CurrentFXItem = item;
        _CurrentFXItem.lineType = type;

        _LineType = type;
    }

    public void UpdateAdjustParamColor(Color c)
    {
        Image[] adj = _adjustParam.GetComponentsInChildren<Image>();
        foreach(var img in adj){
            img.color = c;
        }
    }

    public void SetCurrentZSItem(FXParam item){
        _CurrentFXItem = item;
    }

    public FXParam GetCurrentZSItem(){
        return _CurrentFXItem;
    }

    public GameObject _adjustParam;
    [Serializable] public class OnItemClickEvent : UnityEvent<FXParam> { }
    public OnItemClickEvent OnItemClick;
    public void OnClickItem()
    {
        OnItemClick.Invoke(_CurrentFXItem);
    }

    public FXLineRender.LineType GetLineType()
    {
        return _LineType;
    }

    public string GetBtnID(){
        return _btnID;
    }

    public bool GetCanAdjust(){
        return _adjustParam.activeSelf;
    }
    public void SetCanAdjust(bool can){
        //设置该参数可以调整
        _adjustParam.SetActive(can);
    }

    public void SetSelected(bool can)
    {
        //设置选中
        _adjustParam.SetActive(can);
        _adjustParam.transform.Find("Mask").gameObject.SetActive(can);
        _adjustParam.transform.Find("Slider").gameObject.SetActive(false);
        _adjustParam.transform.Find("Point").gameObject.SetActive(false);
    }


    public bool GetIfShowingBuyBtn(){
        return _buyBtn.activeSelf;
    }

    public void ShowBuyBtn(bool show)
    {
        if (show && _buyBtn.activeSelf){
            _buyBtn.SetActive(true);
        }else{
            _buyBtn.SetActive(false);
        }
    }
}
