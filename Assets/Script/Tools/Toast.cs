/**************************************/
//FileName: FitUI.cs
//Author: wtx
//Data: 05/06/2018
//Describe:  多分辨适配
/**************************************/
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{

    private List<GameObject> _toastList = new List<GameObject>();

    public GameObject _toastPanel;
    public Text _toastText;
    public Image _toastBG;

    public struct ToastData
    {
        public string content;
        public Color c;
        public float showTime;
        public float delay;
        public bool im;
    };

    public void ShowToast(ToastData data)
    {
        if (_toastList.Count == 0)
        {
            DOTween.Kill("ShowToast");

            //立即显示
            _toastPanel.SetActive(true);
            _toastText.text = data.content;
            _toastText.color = new Color(200/255f,200/255f,200/255f,0f);//Define.GetFixColor(Define.GetLightColor(data.c));//
            _toastBG.color = new Color(_toastBG.color.r, _toastBG.color.g, _toastBG.color.b, 0.0f);

            //_toastBG.transform.localScale = new Vector3(0.0f,0.0f,0.0f);
            Sequence sequence = DOTween.Sequence();
            sequence.SetId("ShowToast");

            sequence
                .AppendInterval(data.delay)
                .Append(_toastText.DOFade(1.0f, 1.0f))
                .Join(_toastBG.DOFade(150 / 255.0f, 1.0f))
                .AppendInterval(data.showTime)
                .AppendCallback(() => {
                    //去除副文本的颜色
                    string txt = _toastText.text;
                    if (txt.Contains("<color="))
                    {
                        for (int i = 0; i < txt.Length; i++)
                        {
                            if (i > 1 && "" + txt[i] == "=" && "" + txt[i-1] == "r")
                            {
                                string hexc = txt.Substring(i + 1, 7);
                                txt = txt.Replace(hexc, "");
                            }
                        }

                        txt = txt.Replace("<color=>", "");
                        txt = txt.Replace("</color>", "");
                        _toastText.text = txt;
                    }
                })
                .Append(_toastText.DOFade(0.0f, 1.0f))
                .Join(_toastBG.DOFade(0.0f, 1.0f))
                .OnComplete(() => _toastPanel.SetActive(false));

        }
        else
        {
            //等待其他显示完成

        }
    }
}
