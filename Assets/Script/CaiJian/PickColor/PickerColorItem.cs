using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class PickerColorItem : MonoBehaviour
{
    public void Start()
    {

    }

    public enum eColorIDType{
        PickingColorID,//0
        MainColorID,//1
        PaletteColorID,//2
    }

    public Image _ColorImg;
    public Text _ColorNameText;
    public Text _RGBText;
    public Text _HexText;
    public Text _ColorNameText0;
    public Text _RGBText0;
    public Text _HexText0;
    public Text _ColorDes;
    public Text _EditText;
    public Image _EditImg;
    public Text _TagText;
    public Text _ColorNameTitleText;//

    //只有color card才有的
    public Image _imgEdit;
    public Image _imgDel;

    public void UpdateItem(int colorCardId,string cname,bool hasExist,string des,string py, bool isColorCard)
    {
        Init(colorCardId, _CurrentColorItem.itemId, _CurrentColorItem.color,cname,hasExist,des,py, isColorCard);
    }
    public void Init(int cid,int itemID,Color c,string cname,bool hasExist,string des,string py,bool isColorCard)
    {
        _ColorImg.color = c;
        _ColorNameText.text = cname;

        _RGBText.text = "" + Define.GetIntColor(c.r) +"," + Define.GetIntColor(c.g) + "," + Define.GetIntColor(c.b);
        _HexText.text = Define.GetHexColor(c);

        Color textColor = Define.GetUIFontColorByBgColor(c, Define.eFontAlphaType.FONT_ALPHA_128);
        _ColorNameText.color = textColor;
        _RGBText.color = textColor;
        _HexText.color = textColor;
        _ColorNameText0.color = textColor;
        _RGBText0.color = textColor;
        _HexText0.color = textColor;
        _ColorDes.color = textColor;

        if (hasExist)
        {
            _EditText.color = textColor;
            _EditImg.color = textColor;
            _EditText.gameObject.SetActive(false);
            _EditImg.gameObject.SetActive(false);

            _ColorDes.gameObject.SetActive(true);
            _CurrentColorItem.colorName = cname;
            _CurrentColorItem.py = py;
            _CurrentColorItem.colorDes = des;
        }
        else
        {
            _EditText.color = textColor;
            _EditImg.color = textColor;
            _EditText.gameObject.SetActive(true);
            _EditImg.gameObject.SetActive(true);
            _ColorDes.gameObject.SetActive(false);
            _CurrentColorItem.colorName = "";
            _CurrentColorItem.py = "";

            _CurrentColorItem.colorDes = "";
        }

        _ColorDes.text = des;

        _CurrentColorItem.hasExist = hasExist;
        _CurrentColorItem.color = c;

        _CurrentColorItem.colorCardId = cid;
        _CurrentColorItem.itemId = itemID;//此时两者相同

        if (itemID == 0)
        {
            _TagText.text = "取";
        }
        else if (itemID == 1)
        {
            _TagText.text = "主";
        }
        else
        {
            _TagText.text = "副";
        }
        _TagText.color = textColor;

        _CurrentColorItem.isColorCard = isColorCard;

        if (isColorCard)
        {
            _imgEdit.color = textColor;
            _imgDel.color = textColor;

            //色卡的时候需要修改拼音以及标题
            _ColorNameText0.text = "拼音";
            _ColorNameText.text = py;
            //只有色卡的时候才有标题名称
            _ColorNameTitleText.color = textColor;
            _ColorNameTitleText.gameObject.SetActive(true);
            _ColorNameTitleText.text = cname;
        }
        else
        {
            _ColorNameText0.text = "名称";
            _ColorNameText.text = cname;
        }

    }
    public void UpdateId(int itemID)
    {
        if (itemID > 1)//只有副色才需要更新
        {
            if (!_CurrentColorItem.hasExist)
            {
                _ColorNameText.text = "副色(" + (itemID - 1) + ")";
            }
            else
            {
                _ColorNameText.text = _CurrentColorItem.colorName;
            }

            _CurrentColorItem.itemId = itemID;
        }
    }
    public Color GetPC()
    {
        return _CurrentColorItem.color;
    }
    public string GetHex()
    {
        return _HexText.text;
    }
    public int GetColorID()
    {
        return _CurrentColorItem.colorCardId;
    }
    public int GetItemID()
    {
        return _CurrentColorItem.itemId;
    }
    public bool GetIsColorCard()
    {
        return _CurrentColorItem.isColorCard;
    }
    public ColorItem GetColorItem()
    {
        return _CurrentColorItem;
    }
    //
    public struct ColorItem
    {
        public bool isColorCard;
        public bool hasExist;
        public int colorCardId;//此id指的是itemid，并不是色库id，只有是色卡的时候才是对应的id
        public int itemId;
        public Color color;
        public string colorName;
        public string colorDes;
        public string py;
    }
    public ColorItem _CurrentColorItem;
    [Serializable] public class OnItemClickEvent : UnityEvent<ColorItem> { }
    public OnItemClickEvent OnItemClick;
    public void OnClickItem()
    {
        OnItemClick.Invoke(_CurrentColorItem);
    }

    [Serializable] public class OnDelItemClickEvent : UnityEvent<ColorItem> { }
    public OnDelItemClickEvent OnDelItemClick;
    public void OnClickDelItem()
    {
        OnDelItemClick.Invoke(_CurrentColorItem);
    }
}
