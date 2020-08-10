using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class LikeItem : MonoBehaviour
{
    public void Start()
    {

    }

    public struct sLikeItem
    {
        public bool ToAdd;//是待添加还是已经添加
        public string ID;//时间戳
        public string Title;//#作者、朝代、题目
        public string Content;//#->最多2行，超过两行，第二行最后+显示为……
        public int ColorID;
        public string LikeTime;//收藏时间==id
    };

    public sLikeItem SLikeItem { get; set; }

    public Text _title;
    public Image _titleBg;
    public Image _contentBg;
    public Text _line1;
    public Text _line2;
    public Text _line3;
    public Text _color;

    public GameObject _Add;

    public bool Init(sLikeItem li)
    {
        SLikeItem = li;

        if (SLikeItem.ColorID >= HZManager.GetInstance().GetColorCnt())
        {
            //_Add.gameObject.SetActive(false);
            _line3.gameObject.SetActive(false);
        }

        //
        Image img = _Add.transform.Find("BtnImg").GetComponent<Image>();
        img.color = Define.GetFixColor(GetColorByID(SLikeItem.ColorID));
        if (SLikeItem.ToAdd)
        {
            img.sprite = Resources.Load("icon/plus", typeof(Sprite)) as Sprite;
            gameObject.GetComponent<Button>().interactable = false;//待添加不能选择
            SetLikeTime("点+添加到收藏");
        }
        else
        {
            img.sprite = Resources.Load("icon/minus", typeof(Sprite)) as Sprite;
            gameObject.GetComponent<Button>().interactable = true;//待添加不能选择
            SetLikeTime(li.LikeTime);
        }


        _title.text = SLikeItem.Title;
        string[] cs = SLikeItem.Content.Split('#');

        if (cs.Length == 0)
        {
            return false;
        }

        _line1.text = cs[0];

        if (cs.Length == 1)
        {
            _line2.gameObject.SetActive(false);
        }
        else if (cs.Length == 2)
        {
            _line2.text = cs[1];
        }
        else
        {
            _line2.text = cs[1] + "  ……";//
        }

        _line3.text = SLikeItem.LikeTime;

        _titleBg.color = GetColorByID(SLikeItem.ColorID) * 0.9f;
        _contentBg.color = GetColorByID(SLikeItem.ColorID);

        _color.color = Define.GetFixColor(_titleBg.color);
        _color.text = GetColorInfo(SLikeItem.ColorID);

        Color c = Define.GetUIFontColorByBgColor(_contentBg.color, Define.eFontAlphaType.FONT_ALPHA_128);
        _title.color = c;
        _line1.color = c;
        _line2.color = c;
        _line3.color = new Color(c.r, c.g, c.b, 0.5f);

        return true;
    }

    [Serializable] public class OnLikeItemClickEvent : UnityEvent<LikeItem.sLikeItem> { }
    public OnLikeItemClickEvent OnLikeItemClick;
    public void OnClickItem()
    {
        OnLikeItemClick.Invoke(SLikeItem);//使用colorid
    }

    [Serializable] public class OnLikeItemAddClickEvent : UnityEvent<string,bool> { }
    public OnLikeItemAddClickEvent OnLikeItemAddClick;
    [Serializable] public class ShowToastEvent : UnityEvent<string,float> { }
    public ShowToastEvent ShowToast;
    public void OnClickAddLikeItem()
    {
        bool del = false;
        Image img = _Add.transform.Find("BtnImg").GetComponent<Image>();
        if (SLikeItem.ToAdd)
        {
            if (SLikeItem.ColorID >= HZManager.GetInstance().GetColorCnt())
            {
                ShowToast.Invoke("非内置颜色不能收藏，【色卡】里选用即可",3f);
                return;
            }

            img.sprite = Resources.Load("icon/minus", typeof(Sprite)) as Sprite;
            //
            sLikeItem it = SLikeItem;
            it.ToAdd = false;
            SLikeItem = it;

            gameObject.GetComponent<Button>().interactable = true;//已添加，可以选择
        }
        else
        {
            //del
            del = true;
        }

        OnLikeItemAddClick.Invoke(SLikeItem.ID,del);
    }

    public Color GetColorByID(int id)
    {

        List<string> color = PickColor.GetColorByID(id);

        Color c = new Color(int.Parse(color[3]) / 255.0f, int.Parse(color[4]) / 255.0f, int.Parse(color[5]) / 255.0f);
        return c;
    }

    public String GetColorInfo(int id)
    {

        List<string> color = PickColor.GetColorByID(id);
        string c = color[0]+"\n"+ color[1];

        return c;
    }

    public void SetAddBtn(bool add){

        sLikeItem it = SLikeItem;
        it.ToAdd = add;
        SLikeItem = it;

        Image img = _Add.transform.Find("BtnImg").GetComponent<Image>();
        if (add)
        {
            img.sprite = Resources.Load("icon/plus", typeof(Sprite)) as Sprite;
            gameObject.GetComponent<Button>().interactable = false;//待添加不能选择
        }
        else
        {
            img.sprite = Resources.Load("icon/minus", typeof(Sprite)) as Sprite;
            gameObject.GetComponent<Button>().interactable = true;//待添加不能选择
        }
    }

    public void SetLikeTime(string lt){
        sLikeItem it = SLikeItem;
        it.LikeTime = lt;
        SLikeItem = it;

        _line3.text = lt;
    }
}
