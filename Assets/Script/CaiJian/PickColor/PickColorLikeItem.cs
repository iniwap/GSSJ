using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PickColorLikeItem : MonoBehaviour
{
    public void Start()
    {

    }


    public LikeItem.sLikeItem SLikeItem { get; set; }

    public Text _title;
    public Image _titleBg;
    public Image _contentBg;
    public Text _colorDes;
    public Text _timeStamp;
    public Text _color;

    public bool Init(LikeItem.sLikeItem li)
    {
        SLikeItem = li;

        List<string> colorInfo = PickColor.GetColorByID(li.ColorID);
      
        _title.text = colorInfo[0];

        _colorDes.text = colorInfo[8];
        if (colorInfo[8] == "")
        {
            _colorDes.text = "该颜色尚未添加描述";
        }

        _timeStamp.text = "RGB("+ colorInfo[3]+"," + colorInfo[4] + "," + colorInfo[5]+")";

        _titleBg.color = GetColorByID(SLikeItem.ColorID) * 0.9f;
        _contentBg.color = GetColorByID(SLikeItem.ColorID);

        _color.color = Define.GetFixColor(_titleBg.color);
        _color.text = colorInfo[0] + "\n" + colorInfo[1];

        Color c = Define.GetUIFontColorByBgColor(_contentBg.color, Define.eFontAlphaType.FONT_ALPHA_128);
        _title.color = c;
        _colorDes.color = c;
        _timeStamp.color = new Color(c.r, c.g, c.b, 0.5f);

        return true;
    }

    [Serializable] public class OnLikeItemClickEvent : UnityEvent<LikeItem.sLikeItem> { }
    public OnLikeItemClickEvent OnLikeItemClick;
    public void OnClickItem(){
        OnLikeItemClick.Invoke(SLikeItem);//使用colorid
    }

    [Serializable] public class ShowToastEvent : UnityEvent<string,float> { }
    public ShowToastEvent ShowToast;

    public Color GetColorByID(int id)
    {

        List<string> color = PickColor.GetColorByID(id);

        Color c = new Color(int.Parse(color[3]) / 255.0f, int.Parse(color[4]) / 255.0f, int.Parse(color[5]) / 255.0f);
        return c;
    }

    public int GetColorId()
    {
        return SLikeItem.ColorID;
    }

    public void UpdateItem(string cname, string des, string py)
    {

    }
}
