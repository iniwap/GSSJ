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
using SimpleJson;
using System.Threading;
using com.mob;
using PanGu;

public class CnWordsChaos : MonoBehaviour {

    private int MAX_HZ_WORD_NUM = 150;
    // Use this for initialization
    void Start()
    {
        //不重新生成，这里提前生成
        for(int i = 0;i < MAX_HZ_WORD_NUM; i++)
        {
            GameObject hzObj = Instantiate(_HZPrefab, _HZContent) as GameObject;
            hzObj.SetActive(false);
            _HZBtnList.Add(hzObj);
        }
    }

    public InputField _inputField; 
    public enum eChaosLevelType
    {
        Level_25,
        Level_50,
        Level_75,
        Level_100,
    }

    public eChaosLevelType _chaosLevelType = eChaosLevelType.Level_100;//默认100%混乱
    public void OnDropDownValueChanged(int v)
    {
        _chaosLevelType = (eChaosLevelType)v;
        //ShowToast("人名、英文数字、成语等不会乱序");
    }

    public float GetLevelByType(eChaosLevelType type)
    {
        float l = 1.0f;

        switch (type)
        {
            case eChaosLevelType.Level_25:
                l = 0.25f;
                break;
            case eChaosLevelType.Level_50:
                l = 0.5f;
                break;
            case eChaosLevelType.Level_75:
                l = 0.75f;
                break;
            case eChaosLevelType.Level_100:
                l = 1f;
                break;
        }

        return l;
    }

    public Text _chaosText;
    public Text _tip;
    public GameObject _spImg;
    //public Text 
    private bool _geningChaos = false;
    public void OnGenChaosBtnClick()
    {
        if (_geningChaos)
        {
            ShowToast("正在打乱汉字顺序，请稍后再试...");
            return;
        }

        if (_inputField.text.Length < 5 || _inputField.text.Length > MAX_HZ_WORD_NUM)
        {
            ShowToast("文章题目字数范围是5-150，请重新输入。");
            return;
        }

        _geningChaos = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        float t = 0f;
        if (_inputField.text.Length == 0)
        {
            _tip.gameObject.SetActive(true);//只有第一次才显示默认的提示
            t = 0.5f;
            _spImg.SetActive(false);
        }
        else
        {
            _tip.gameObject.SetActive(false);//只有第一次才显示默认的提示
            t = 0f;
            _spImg.SetActive(true);
        }

        _chaosText.gameObject.SetActive(false);
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.DOFade(0f, t).OnComplete(() =>
        {
            _tip.gameObject.SetActive(false);
            //根据类型执行查询，这里需要采用多线程，并即时更新界面
            _chaosText.gameObject.SetActive(false);
            _chaosText.text = GetChaosWords();
            _chaosText.gameObject.SetActive(true);
            _chaosText.color = new Color(_chaosText.color.r, _chaosText.color.g, _chaosText.color.b, 0f);
            _chaosText.DOFade(0.5f, 0.5f);
            //需要重新设置text的高度
            _geningChaos = false;
        });

        if (Setting.CheckSettingCnt(Setting.SETTING_KEY.SHOW_CPY_CHOAS_TXT_TIPS, 3))
        {
            ShowToast("<b>长按屏幕</b>可复制生成的结果到剪贴板 :)",2f,1f);
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

    // 长按全屏查看模式

    public UnityEvent _CloseChaos;
    public void OnSwipeLeft()
    {
        _CloseChaos.Invoke();
    }

    public void OnLongPress()
    {
        if (!gameObject.activeSelf) return;
        if (_chaosText.text.Length == 0) return;

        ShowToast("打乱的句子已复制到剪贴板 :)");

        string txt = _chaosText.text.Replace("<size=52>","");
        txt = txt.Replace("</size>", "");
        txt = txt.Replace("<b>", "");
        txt = txt.Replace("</b>", "");
        ShareREC.setClipbord(txt);
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
                    if (!txt.name.Contains("Chaos"))//分词文字不能改变颜色
                    {
                        txt.color = c0;
                    }
                }
                else
                {
                    txt.color = c1;
                }
            }
        }
    }

    public GameObject _HZPrefab;
    public Transform _HZContent;
    private List<GameObject> _HZBtnList = new List<GameObject>();
    private string GetCYTypeName(POS type)
    {
        string ret = "";

        switch (type)
        {
            case POS.POS_D_A:
                ret = "形容词";
                break;
            case POS.POS_D_B:
                ret = "区别词";
                break;
            case POS.POS_D_C:
                ret = "连词";
                break;
            case POS.POS_D_D:
                ret = "副词";
                break;
            case POS.POS_D_E:
                ret = "叹词";
                break;
            case POS.POS_D_F:
                ret = "方位词";
                break;
            case POS.POS_D_I:
                ret = "成语";
                break;
            case POS.POS_D_L:
                ret = "习语";
                break;
            case POS.POS_A_M:
                ret = "数词";
                break;
            case POS.POS_D_MQ:
                ret = "数量词";
                break;
            case POS.POS_D_N:
                ret = "名词";
                break;
            case POS.POS_D_O:
                ret = "拟声词";
                break;
            case POS.POS_D_P:
                ret = "介词";
                break;
            case POS.POS_A_Q:
                ret = "量词";
                break;
            case POS.POS_D_R:
                ret = "代词";
                break;
            case POS.POS_D_S:
                ret = "处所词";
                break;
            case POS.POS_D_T:
                ret = "时间词";
                break;
            case POS.POS_D_U:
                ret = "助词";
                break;
            case POS.POS_D_V:
                ret = "动词";
                break;
            case POS.POS_D_W:
                ret = "标点符号";
                break;
            case POS.POS_D_Y:
                ret = "语气词";
                break;
            case POS.POS_D_Z:
                ret = "状态词";
                break;
            case POS.POS_A_NR:
                ret = "人名";
                break;
            case POS.POS_A_NS:
                ret = "地名";
                break;
            case POS.POS_A_NT:
                ret = "机构团体";
                break;
            case POS.POS_A_NX:
                ret = "外文字符";
                break;
            case POS.POS_A_NZ:
                ret = "其他专用名词";
                break;
            case POS.POS_D_H:
                ret = "前接部分";
                break;
            case POS.POS_D_K:
                ret = "后接部分";
                break;
            case POS.POS_D_X:
            case POS.POS_UNK:
                ret = "不确定类型";//不做标记
                break;
        }

        return ret;
    }
    private void InitSegList(ICollection<WordInfo> wis)
    {
        //DestroyObj(_HZBtnList);
        for (int i = 0; i < MAX_HZ_WORD_NUM; i++)
        {
            _HZBtnList[i].SetActive(false);
        }

        //抽取对应数量type的颜色
        List<POS> pos = new List<POS>();
        Dictionary<POS, Color> cyColor = new Dictionary<POS, Color>();
        foreach (var wordInfo in wis)
        {
            if (!pos.Contains(wordInfo.Pos))
            {
                pos.Add(wordInfo.Pos);
            }
        }

        List<int> colorIDs = HZManager.GetInstance().GenerateRandomNoRptIntList(pos.Count, 0, HZManager.GetInstance().GetColorCnt() - 1);

        for(int i = 0;i< pos.Count;i++)
        {
            List<string> cInfo = HZManager.GetInstance().GetColorByID(colorIDs[i]);

            Color c = new Color(int.Parse(cInfo[3]) / 255.0f,
                          int.Parse(cInfo[4]) / 255.0f,
                          int.Parse(cInfo[5]) / 255.0f);

            cyColor.Add(pos[i],c );
        }

        int cnt = 0;
        foreach (var wordInfo in wis)
        {
            GameObject hzObj = _HZBtnList[cnt++];
            hzObj.SetActive(true);

            Button hzBtn = hzObj.GetComponent<Button>();
            hzBtn.onClick.RemoveAllListeners();
            hzBtn.onClick.AddListener(delegate () {
                if (wordInfo.WordType == WordType.Symbol)
                {
                    ShowToast("该字词的类型是->标点/符号");
                }
                else if (wordInfo.WordType == WordType.Space)
                {
                   // ShowToast("该字词的类型是->空格");
                }
                else if (wordInfo.WordType == WordType.Numeric)
                {
                    ShowToast("该字词的类型是->数字");
                }
                else if (wordInfo.WordType == WordType.English)
                {
                    ShowToast("该字词的类型是->英文");
                }
                else
                {
                    ShowToast("该字词的类型是->" + GetCYTypeName(wordInfo.Pos));
                }
            });

            Text hzText = hzObj.GetComponentInChildren<Text>();
            hzText.text = wordInfo.Word;
            hzText.color = Define.GetUIFontColorByBgColor(cyColor[wordInfo.Pos], Define.eFontAlphaType.FONT_ALPHA_200);
            hzObj.GetComponent<Image>().color = cyColor[wordInfo.Pos];//设置词性颜色

            //显示动画
            hzObj.transform.localScale = Vector3.zero;
            hzObj.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
        }

        //设置高度
        int row = wis.Count / 7;
        if (wis.Count % 7 != 0)
        {
            row += 1;
        }

        RectTransform jsrt = _HZContent.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = _HZContent.GetComponentInChildren<GridLayoutGroup>();
        float h = jsLyt.cellSize.y * row + jsLyt.padding.top + jsLyt.padding.bottom + jsLyt.spacing.y * row;
        jsrt.sizeDelta = new Vector2(jsrt.sizeDelta.x, h);
    }
    public string GetChaosWords()
    {

        float cl = GetLevelByType(_chaosLevelType);

        Segment seg = new Segment();
        PanGu.Match.MatchOptions opts = new PanGu.Match.MatchOptions();
        opts.FilterStopWords = false;
        opts.ChineseNameIdentify = true;

        var ret = seg.DoSegment(_inputField.text, opts);

        List<string> wis = new List<string>();
        foreach (WordInfo wordInfo in ret)
        {
            if (wordInfo == null)
            {
                continue;
            }

            if (wordInfo.Position == 0 || wordInfo.Position == ret.Count - 1)
            {
                wis.Add(wordInfo.Word);
                continue;
            }

            if (wordInfo.WordType == WordType.Symbol && wordInfo.Position == ret.Count - 2)
            {
                wis.Add(wordInfo.Word);
                continue;
            }

            if (wordInfo.OriginalWordType == WordType.SimplifiedChinese
                || wordInfo.OriginalWordType == WordType.TraditionalChinese)
            {
                if (UnityEngine.Random.value < cl)
                {
                    List<string> splitWord = new List<string>();
                    foreach (var sw in wordInfo.Word)
                    {
                        splitWord.Add("" + sw);
                    }

                    splitWord.Reverse();
                    wis.Add(String.Join("", splitWord));
                }
                else
                {
                    wis.Add(wordInfo.Word);
                }
            }
            else
            {
                wis.Add(wordInfo.Word);
            }
        }

        InitSegList(ret);

        return String.Join("",wis);
    }
}
