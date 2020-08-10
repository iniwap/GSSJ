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
using System.Text.RegularExpressions;

public class Dict: MonoBehaviour{
    // Use this for initialization
    void Start()
    {
        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;
        float ipad = FitUI.GetIsPad() ? 0.9f : 1.0f;

        RectTransform rt = _ResultSJInfo.transform.Find("ResultSJInfo").GetComponent<RectTransform>();
        nw = nw * ipad - 40;

        rt.localScale = new Vector3(nw / rt.sizeDelta.y, nw / rt.sizeDelta.y, 1.0f);
    }

    public Text _inputText;
    private HZManager.eShiCi _searchType = HZManager.eShiCi.TANGSHI;
    public void OnDropDownValueChanged(int v)
    {
        _searchType = (HZManager.eShiCi)(v+1);
        if (_searchType == HZManager.eShiCi.GUSHI)
        {
            ShowToast("非唐宋诗词归为古诗范畴");
        }
        else
        {
            if (UnityEngine.Random.value < 0.3f)
            {
                //ShowToast("");
            }
        }
    }

    public GameObject _ClickToClose;
    public GameObject _ResultSJInfo;
    public void OnClickToClose()
    {
        //可以动画隐藏
        ShowSJInfo(null,false);
    }

    public GameObject _HZPrefab;
    public Transform _HZContent;
    private List<GameObject> _HZBtnList = new List<GameObject>();
    private bool _isSearching = false;
    public void OnSearchBtnClick()
    {
        if (_isSearching)
        {
            ShowToast("正在查询中，请稍后再试...");
            return;
        }

        Regex regChina = new Regex("^[^\x00-\xFF]");
        Regex regEnglish = new Regex("^[a-zA-Z]");

        if (!regChina.IsMatch(_inputText.text))
        {
            ShowToast("请输入正确的汉字，不然会查不到哟 :(");//这里不确定范围，只提示
        }

        if (_inputText.text.Length < 1 || _inputText.text.Length > 7)
        {
            ShowToast("请输入诗句/单字/词语，1-7个汉字");
            return;
        }

        _isSearching = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        //直到完成
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.gameObject.SetActive(true);
        _tip.DOFade(0f, 0.5f).OnComplete(() => {
            _tip.gameObject.SetActive(false);
            HZManager.GetInstance().SearchTSSC(_searchType, _inputText.text, SearchingTSSCCallback, () => {
                _isSearching = false;
                if (_HZBtnList.Count == 0)
                {
                    string info = "没有查询到包含搜索内容的诗词，请重新输入 :(";
                    ShowToast(info);
                }
            });
        });

        DestroyObj(_HZBtnList);
    }

    public Text _tip;
    public void SearchingTSSCCallback(List<string> scInfo)
    {
        if (scInfo.Count == 0) return;

        GameObject hzObj = Instantiate(_HZPrefab, _HZContent) as GameObject;
        hzObj.SetActive(true);
        _HZBtnList.Add(hzObj);

        Button hzBtn = hzObj.GetComponent<Button>();
        hzBtn.onClick.AddListener(delegate () {
            ShowHZInfo(scInfo);
        });

        Text hzText = hzObj.GetComponentInChildren<Text>();
        //这里可以多重颜色来表达汉字的常见度
        string sj = scInfo[(int)HZManager.eTSSCColName.TSSC_END];//最后一条存放搜索到的相应诗句，用于按钮缩略显示
        hzText.text = sj.Replace("，","，\n");
        
        //设置高度
        int row = _HZBtnList.Count / 2;
        if (_HZBtnList.Count % 2 != 0)
        {
            row += 1;
        }

        RectTransform jsrt = _HZContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HZContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);

        //显示动画
        hzObj.transform.localScale = Vector3.zero;
        hzObj.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
    }

    private List<GameObject> _jieShiList = new List<GameObject>();
    private void ShowHZInfo(List<string> scInfo)
    {
        ShowSJInfo(scInfo, true);
    }

	public Text _TiMuText;
	public Text _ZZCDText;
    List<GameObject> _sjList = new List<GameObject>();
    public GameObject _sjPrefabs;
    public GameObject _ywPrefabs;
    public Transform _sjInfoContent;
	private void SetSJ(List<string> sjInfo)
    {
        DestroyObj(_sjList);
		string tm = sjInfo[(int)HZManager.eTSSCColName.TSSC_TiMu];
        string zzcd = sjInfo[(int)HZManager.eTSSCColName.TSSC_ChaoDai] + "·" + sjInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe];
        string nr = sjInfo[(int)HZManager.eTSSCColName.TSSC_NeiRong];
        string yw = "  译文："+sjInfo[(int)HZManager.eTSSCColName.TSSC_YiWen];

        int totalLineCnt = 2;

        _TiMuText.text = tm;
        _ZZCDText.text = zzcd;
        int cnt = 0;
        List<string> sj = new List<string>();
        nr = nr.Replace("#","");
        for (int i = 0; i < nr.Length; i++)
        {
            cnt++;
            if ("" + nr[i] == "。" || "" + nr[i] == "；" || "" + nr[i] == "！")
            {
                sj.Add(nr.Substring(i + 1 - cnt, cnt));
                cnt = 0;
            }
            else if ("" + nr[i] == "？")
            {
                string subSj = nr.Substring(i + 1 - cnt, cnt);
                if (subSj.Contains("，"))// 问号是前半句，不能做单句处理
                {
                    sj.Add(subSj);
                    cnt = 0;
                }
            }
        }

        foreach (var s in sj)
        {
            GameObject sjText = Instantiate(_sjPrefabs, _sjInfoContent) as GameObject;
            sjText.SetActive(true);
            sjText.GetComponentInChildren<Text>().text = s;
            _sjList.Add(sjText);
        }

        totalLineCnt += sj.Count;

        yw = yw.Replace("N/A","暂无。");

        List<string> ywList = new List<string>();
        int everyLineHZCnt = 770/32;
        int lcnt = yw.Length / everyLineHZCnt;  //770/44 = 17.5
        bool toolong = true;
        for (int c = 32; c >= 16; c--)
        {
            everyLineHZCnt = 770 / c;
            lcnt = yw.Length / everyLineHZCnt;  //770/44 = 17.5

            if (totalLineCnt + lcnt > 80)
            {
                continue;
            }

            toolong = false;

            break;
        }

        if (!toolong)
        {
            if (yw.Length % everyLineHZCnt != 0) lcnt += 1;

            for (int i = 0; i < lcnt; i++)
            {
                if (i == lcnt - 1)
                {
                    ywList.Add(yw.Substring(i * everyLineHZCnt, yw.Length - i * everyLineHZCnt));
                }
                else
                {
                    ywList.Add(yw.Substring(i * everyLineHZCnt, everyLineHZCnt));
                }
            }

            foreach (var s in ywList)
            {
                GameObject sjText = Instantiate(_ywPrefabs, _sjInfoContent) as GameObject;
                sjText.SetActive(true);
                sjText.GetComponentInChildren<Text>().text = s;
                _sjList.Add(sjText);
            }
        }

        totalLineCnt += ywList.Count;

        //设置大小
        RectTransform rt = _sjInfoContent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x,100 * totalLineCnt);
    }

    public Image _dialogImg;
    public void ShowSJInfo(List<string> sjInfo,bool open)
    {
        if (_ResultSJInfo.activeSelf == open) return;

        Sequence mySequence = DOTween.Sequence();

        float toX = 0;

        GameObject likes = _ResultSJInfo.transform.Find("ResultSJInfo").gameObject;
        RectTransform rt = likes.GetComponent<RectTransform>();

        float sh = Screen.height / FitUI.DESIGN_HEIGHT;
        float nw = Screen.width / sh;

        if (open)
        {
            SetSJ(sjInfo);
            //设置参数面板标题
            //打开动画
            _ResultSJInfo.SetActive(true);

            likes.transform.localPosition = new Vector3(nw / 2 + rt.rect.width * rt.localScale.x,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = nw / 2;
            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 1.0f))
                .Join(_dialogImg.DOFade(127 / 255.0f, 0.5f))
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    //
                });

        }
        else
        {
            //关闭动画
            likes.transform.localPosition = new Vector3(nw / 2,
                                  likes.transform.localPosition.y,
                                  likes.transform.localPosition.z);
            toX = nw / 2 + rt.rect.width * rt.localScale.x;

            mySequence
                .Append(likes.transform.DOLocalMoveX(toX, 0.5f))
                .Join(_dialogImg.DOFade(0f, 0.5f))
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _ResultSJInfo.SetActive(false);
                });
        }
    }

    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    public  Image _BG;
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
                if (img.name.Contains("128"))//半透明的按钮，防止亮度过高
                {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);
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
        
    }
    private void UpdateUIFontColor()
    {
        // 此时应该改变界面上所有字体颜色
        Text[] allTxt = gameObject.GetComponentsInChildren<Text>(true);
        Color c0 = Define.GetUIFontColorByBgColor(_BG.color, Define.eFontAlphaType.FONT_ALPHA_128);
        Color c1 = new Color(c0.r, c0.g, c0.b, 50 / 255f);
        foreach (var txt in allTxt)
        {
            if (txt.name == "Footer")
            {
                txt.color = c1;
            }
            else
            {
                // 使用黑字
                if (!txt.name.Contains("Placeholder") && !txt.name.Contains("ResultTip"))
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
