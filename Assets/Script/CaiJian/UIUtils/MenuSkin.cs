using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class MenuSkin : MonoBehaviour
{
    public void Start()
    {

    }

    public GameObject _Menu;
    public void OnChangeMenuBtnImgColor(Color c,bool isChangeTheme)
    {
        OnChangeMenuBtnImgColor(c);
    }
    public void OnChangeMenuBtnImgColor(Color c){
        Image[] imgs = _Menu.GetComponentsInChildren<Image>(true);

        Color bc = Define.GetFixColor(c);

        foreach (var img in imgs)
        {
            if (img.name == "BtnImg" 
                || img.name == "KTTag"
                || img.name == "BuyLockBtnImg"
                || img.name == "BuyUnLockBtnImg")
            {
                img.color = bc;
            }
        }

        OnChangeMenuTextColor(c);
    }

    public void OnChangeMenuTextColor(Color bgColor)
    {
        Color c = Define.GetUIFontColorByBgColor(bgColor,Define.eFontAlphaType.FONT_ALPHA_128);

        Text[] txts = _Menu.GetComponentsInChildren<Text>(true);
        foreach (var txt in txts)
        {
            if (txt.name == "BtnText")
            {
                txt.color = c;
            }
            else if(txt.name == "HotTag" || txt.name == "NewTag")
            {
                txt.color = Define.GetFixColor(Define.GetLightColor(bgColor));
            }
        }

        Transform expandBtn = gameObject.transform.Find("ExpandBtn");

        if (expandBtn != null)
        {
            Text btnExpandText = expandBtn.GetComponentInChildren<Text>(true);
            Image[] btnExpandImgs = expandBtn.GetComponentsInChildren<Image>(true);
            //展开按钮不同
            if (btnExpandText.text.Equals("展开"))
            {
                btnExpandImgs[1].color = Define.GetFixColor( Define.GetDarkColor(bgColor));
                btnExpandText.color = new Color(btnExpandText.color.r, btnExpandText.color.g, btnExpandText.color.b, 50 / 255.0f);
            }
        }
    }
}
