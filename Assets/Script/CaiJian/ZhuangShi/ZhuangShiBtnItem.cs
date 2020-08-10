using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class ZhuangShiBtnItem : MonoBehaviour
{
    public enum eZSBtnType
    {
        ZSNone,
        ZiShi,//字饰调整
        DianZhui,//点缀调整
        HangShi,//行线调整
        Theme,

        ZiShiShape,//字饰图案调整
        DianZhuiShape,//点缀图案调整
        HangShiShape,//行线图案调整

        //把配套也作为装饰的一部分
        PeiTuShape,
    }

    public void Start()
    {

    }

    public struct ZSParam{
        public string path;
        public Color color;//r,g,b,a可以调节
        public eZSBtnType btnType;
        public float size;
    }

    private eZSBtnType _btnType;
    private string _btnID;

    public GameObject _buyBtn;
    public Text _btnText;
    public Image _btnImg;

    private ZSParam _CurrentZSItem;

    public void Init(bool needBuy,string btnID, eZSBtnType btnType,string btnText, ZSParam item)
    {
        _btnType = btnType;
        _btnID = btnID;
        if (btnID != "")
        {
            if (_btnType == eZSBtnType.DianZhuiShape)
            {
                _btnText.text = "DZ" + btnText;
            }
            else if (_btnType == eZSBtnType.ZiShiShape)
            {
                _btnText.text = "ZS" + btnText;
            }
            else if (_btnType == eZSBtnType.PeiTuShape)
            {
                _btnText.text = "PT" + btnText;
            }

            _btnImg.sprite = Resources.Load(_btnID, typeof(Sprite)) as Sprite;
        }else{
            if(btnType == eZSBtnType.ZiShiShape)
            {
                _btnText.text = "无底";
            }
            else if(btnType == eZSBtnType.PeiTuShape)
            {
                _btnText.text = "无框";
            }
            else
            {
                _btnText.text = "清空";
            }
            _adjustParam.SetActive(true);
            Image[] adj = _adjustParam.GetComponentsInChildren<Image>();
            adj[1].gameObject.SetActive(false);
            adj[2].gameObject.SetActive(false);
            _btnImg.sprite = Resources.Load("icon/clear", typeof(Sprite)) as Sprite;
        }

        _buyBtn.SetActive(needBuy);

        _CurrentZSItem = item;
    }

    public void UpdateAdjustParamColor(Color c)
    {
        Image[] adj = _adjustParam.GetComponentsInChildren<Image>();
        foreach(var img in adj){
            img.color = c;
        }
    }

    public void SetCurrentZSItem(ZSParam item){
        _CurrentZSItem = item;
    }

    public ZSParam GetCurrentZSItem(){
        return _CurrentZSItem;
    }

    public GameObject _adjustParam;
    [Serializable] public class OnItemClickEvent : UnityEvent<ZSParam> { }
    public OnItemClickEvent OnItemClick;
    public void OnClickItem()
    {
        OnItemClick.Invoke(_CurrentZSItem);
    }

    public string GetBtnID(){
        return _btnID;
    }

    public eZSBtnType GetBtnType()
    {
        return _btnType;
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
        if (show && _buyBtn.activeSelf)
        {
            _buyBtn.SetActive(true);
        }
        else
        {
            _buyBtn.SetActive(false);
        }
    }
}
