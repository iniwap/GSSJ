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

public class Article : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        LoadArticleData();
        float s = Screen.height / AutoFitUI.DESIGN_HEIGHT;
        _SmallViewBtn.transform.localPosition = new Vector3(-Screen.width/2/s +20,-AutoFitUI.DESIGN_HEIGHT/2 + 20, 0f);

        if (AutoFitUI.getIsIPhoneX())
        {
            _FullViewText.transform.localPosition = new Vector3(_FullViewText.transform.localPosition.x,
                _FullViewText.transform.localPosition.y - AutoFitUI.GetOffsetYIphoneX(true), 0f);
        }
        _FullViewContent.transform.localScale = new Vector3(AutoFitUI.GetFitUIScale(), AutoFitUI.GetFitUIScale(), 1f);
    }

    public Text _inputText;
    public enum eArticleLenType
    {
        ARTICLE_200,
        ARTICLE_500,
        ARTICLE_800,
        ARTICLE_1000,
        ARTICLE_2000,
        ARTICLE_5000
    }
    public eArticleLenType _articleType = eArticleLenType.ARTICLE_800;//默认800字高考作文
    public void OnDropDownValueChanged(int v)
    {
        _articleType = (eArticleLenType)v;
    }
    public Text _tip;
    public Text _articleText;
    private bool _geningArticle = false;
    public GameObject _smallContent;
    public void OnGenArticleBtnClick()
    {
        if (_geningArticle)
        {
            ShowToast("正在生成文章，请稍后再试...");
            return;
        }

        if (_inputText.text.Length < 2 || _inputText.text.Length > 15)
        {
            ShowToast("文章题目字数范围是2-15，请重新输入。");
            return;
        }

        _geningArticle = true;
        //根据类型执行查询，这里需要采用多线程，并即时更新界面
        float t = 0f;
        if (_articleText.text.Length == 0)
        {
            _tip.gameObject.SetActive(true);//只有第一次才显示默认的提示
            t = 0.5f;
        }
        else
        {
            _tip.gameObject.SetActive(false);//只有第一次才显示默认的提示
            t = 0f;
        }

        _articleText.gameObject.SetActive(false);
        _tip.color = new Color(_tip.color.r, _tip.color.g, _tip.color.b, 0.5f);
        _tip.DOFade(0f, t).OnComplete(() => {
            _tip.gameObject.SetActive(false);

            Loom.RunAsync(() =>
            {
                Thread thread = new Thread(() =>
                {
                    int tmpCnt = 0;
                    List<string> dls = GenArticle(out tmpCnt);

                    Loom.QueueOnMainThread((param) =>
                    {
                        string fmtArticle = GetFmtArticle(dls, tmpCnt, false);
                        string fmtFullArticle = GetFmtArticle(dls, tmpCnt, true);

                        _articleText.text = fmtArticle;
                        _articleText.gameObject.SetActive(true);
                        _articleText.color = new Color(_articleText.color.r, _articleText.color.g, _articleText.color.b,0f);
                        _articleText.DOFade(0.5f,0.5f);
                        //需要重新设置text的高度
                        _geningArticle = false;
                        int lineCnt = fmtArticle.Length / 42;

                        //大于500就需要设置
                        RectTransform scrt = _smallContent.GetComponent<RectTransform>();
                        scrt.sizeDelta = new Vector2(scrt.sizeDelta.x,84*(lineCnt + GetEnterCnt(fmtArticle)) + 120);


                        _FullViewText.text = fmtFullArticle;
                        lineCnt = fmtFullArticle.Length / 42;
                        RectTransform fvrt = _FullViewContent.GetComponent<RectTransform>();
                        fvrt.sizeDelta = new Vector2(fvrt.sizeDelta.x, 84 * (lineCnt + GetEnterCnt(fmtFullArticle)) + 120);

                        _FullViewBtn.SetActive(true);


                    }, null);
                });

                thread.Start();
            });
        });
    }
    private int GetTitleSpaceCnt(bool full)
    {
        int ret = 0;

        //此处没有对齐，ipad
        ret = 9 - (_inputText.text.Length + 2) / 2;
        if (_inputText.text.Length % 2 == 0)
        {
            ret += 1;
        }

        return ret ;
    }
    public int GetEnterCnt(string txt)
    {
        int ret = 0;
        foreach (var t in txt)
        {
            if ("" + t == "\n")
            {
                ret++;
            }
        }
        return ret;
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
    public GameObject _FullView;
    public GameObject _FullViewContent;
    public Text _FullViewText;
    public GameObject _FullViewBtn;
    public GameObject _SmallViewBtn;
    public Image _FullViewBg;
    public UnityEvent _CloseArticle;
    public void OnShowFullView()
    {
        //当前不是在文章界面
        if (!gameObject.activeSelf) return;
        
        //长按可以切换全屏/非全屏查看文章
        if (_FullView.activeSelf)
        {
            _FullView.SetActive(false);
        }
        else
        {
            _FullView.SetActive(true);
            ShowToast("<b>右滑或点击左下角</b>，返回上一级");
        }
    }

    public void OnSwipeLeft()
    {
        if (_FullView.activeSelf)
        {
            OnShowFullView();
        }
        else
        {
            _CloseArticle.Invoke();
        }
    }

    public void OnLongPress()
    {
        if (!gameObject.activeSelf) return;
        if (_articleText.text.Length == 0) return;

        ShowToast("文章已复制到剪贴板 :)");

        string txt = _articleText.text.Replace("<size=52>","");
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
        _FullViewBg.color = color;
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

    //----------------------------内部接口-----------------------------------
    public int GetLenByType(eArticleLenType type)
    {
        int l = 100;

        switch (type)
        {
            case eArticleLenType.ARTICLE_200:
                l = 200;
                break;
            case eArticleLenType.ARTICLE_500:
                l = 500;
                break;
            case eArticleLenType.ARTICLE_800:
                l = 800;
                break;
            case eArticleLenType.ARTICLE_1000:
                l = 1000;
                break;
            case eArticleLenType.ARTICLE_2000:
                l = 2000;
                break;
            case eArticleLenType.ARTICLE_5000:
                l = 5000;
                break;
        }

        return l;
    }

    //---------------------------自动生成网文算法-----------------------------
    private List<string> GenArticle(out int tmpCnt)
    {
        tmpCnt = 0;
        int l = GetLenByType(_articleType);
        int famousCnt = _articleData.famous.Count;
        int boshesCnt = _articleData.boshes.Count;
        int beforeCnt = _articleData.befores.Count;
        int afterCnt = _articleData.afters.Count;

        List<string> dls = new List<string>();
        foreach (var a in _inputText.text)
        {
            string duanLuo = "";
            while (duanLuo.Length < l)
            {
                //需要判断加上当前段落的时候长度是否超过总长度，超过则停止
                int r = HZManager.GetInstance().GenerateRandomInt(0, 100);
                if (r < 5 && duanLuo.Length > 190)
                {
                    if (""+duanLuo[duanLuo.Length - 1] == "，")
                    {
                        duanLuo += GetRandomAfter(afterCnt);
                    }
                    //另起一段
                    duanLuo += "\n";
                    duanLuo += "\u3000\u3000";
                    tmpCnt += 5;
                }
                else if (r < 15f)
                {
                    //来句名言
                    string famous = _articleData.famous[HZManager.GetInstance().GenerateRandomInt(0, famousCnt)];
                    famous = famous.Replace("a",GetRandomBefore(beforeCnt));
                    famous = famous.Replace("b", GetRandomAfter(afterCnt));

                    duanLuo += famous;
                }
                else if (r < 25 + (int)_articleType / 500)
                {
                    int r2 = HZManager.GetInstance().GenerateRandomInt(0, 100);
                    //来句古诗词
                    HZManager.eShiCi type = HZManager.eShiCi.ALL;
                    if (r2 < 40)
                    {
                        type = HZManager.eShiCi.TANGSHI;
                    }
                    else if (r2 < 70)
                    {
                        type = HZManager.eShiCi.SONGCI;
                    }
                    else if (r2 < 90)
                    {
                        type = HZManager.eShiCi.GUSHI;
                    }
                    else
                    {
                        type = HZManager.eShiCi.SHIJING;
                    }

                    List<string> tsscData = HZManager.GetInstance().GetTSSC(type);
                    List<string> tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[(int)HZManager.eTSSCColName.TSSC_NeiRong]);
                    string cd = tsscData[(int)HZManager.eTSSCColName.TSSC_ChaoDai];
                    string zz = tsscData[(int)HZManager.eTSSCColName.TSSC_ZuoZhe];
                    string tm = "《"+tsscData[(int)HZManager.eTSSCColName.TSSC_TiMu] + "》";
                    string sj = "“"+tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)] + "”";

                    int r3 = HZManager.GetInstance().GenerateRandomInt(0, 100);
                    int r4 = HZManager.GetInstance().GenerateRandomInt(0, 100);
                    if (r3 < 25)
                    {
                        //只显示诗句、作者
                        if (zz != "佚名")
                        {
                            if (r4 < 50)
                            {
                                duanLuo += zz + "的诗写道：" + sj + "。";
                            }
                            else
                            {
                                duanLuo += zz + "有诗云：" + sj;
                            }
                        }
                        else
                        {
                            if (r4 <50)
                            {
                                duanLuo += "古有诗写道：" + sj;
                            }
                            else
                            {
                                duanLuo += "古有诗云：" + sj;
                            }
                        }
                    }
                    else if(r3 < 50)
                    {
                        //显示诗句、作者、朝代
                        if (zz != "佚名")
                        {
                            if (r4 < 50)
                            {
                                duanLuo += cd + "诗人" + zz + "的诗写道：" + sj;
                            }
                            else
                            {
                                duanLuo += cd + "诗人" + zz + "有诗云：" + sj;
                            }
                        }
                        else
                        {
                            if (r4 < 50)
                            {
                                duanLuo += cd + "佚名人士有诗写道：" + sj;
                            }
                            else
                            {
                                duanLuo += cd + "佚名人士有诗云：" + sj;
                            }
                        }
                    }
                    else if(r3 < 75)
                    {
                        //显示诗句、作者、题目
                        if (zz != "佚名")
                        {
                            if (r4 < 50)
                            {
                                duanLuo += "诗人" + zz + "在"+tm+"中写道：" + sj;
                            }
                            else
                            {
                                sj = sj.Replace("。","");
                                duanLuo += "诗人" + zz + "有诗云：" + sj+"（出自"+tm+"）。";
                            }
                        }
                        else
                        {
                            if (r4 < 50)
                            {
                                duanLuo += "古代佚名人士在"+tm+"中写道：" + sj;
                            }
                            else
                            {
                                sj = sj.Replace("。", "");
                                duanLuo += "古有诗云：" + sj + "（出自" + tm + "）。";
                            }
                        }
                    }
                    else
                    {
                        //显示诗句、作者、题目、年代
                        if (zz != "佚名")
                        {
                            if (r4 < 50)
                            {
                                duanLuo += cd + "诗人" + zz + "在" + tm + "中写道：" + sj;
                            }
                            else
                            {
                                sj = sj.Replace("。", "");
                                duanLuo += cd + "诗人" + zz + "有诗云：" + sj + "（出自" + tm + "）。";
                            }
                        }
                        else
                        {
                            if (r4 < 50)
                            {
                                duanLuo += cd + "佚名人士在" + tm + "中写道：" + sj;
                            }
                            else
                            {
                                sj = sj.Replace("。", "");
                                duanLuo += cd + "佚名人士有诗云：" + sj + "（出自" + tm + "）。";
                            }
                        }
                    }
                }
                else
                {
                    //来句废话
                    string bosh = _articleData.boshes[HZManager.GetInstance().GenerateRandomInt(0, boshesCnt)];
                    duanLuo += bosh;
                }
            }

            duanLuo = duanLuo.Replace("x", _inputText.text);
            
            dls.Add(duanLuo);
        }


        return dls;
    }
    private string GetFmtArticle(List<string> dls,int tmpCnt, bool full)
    {
        string article = "";
        int l = GetLenByType(_articleType);
        int afterCnt = _articleData.afters.Count;

        string space = "";
        int spaceCnt = GetTitleSpaceCnt(full);
        for (int i = 0; i < spaceCnt; i++)
        {
            space += "\u3000";
        }
        article += "<b><size=52>" + space + "《" + _inputText.text + "》</size></b>\n";
        for (int i = 0; i < dls.Count; i++)
        {
            if (i == 0)
            {
                article += "\u3000\u3000" + dls[i] + "\n";
                tmpCnt += 5;
            }
            else if (i == dls.Count - 1)
            {
                if (article.Length - tmpCnt > l)
                {
                    break;
                }

                article += dls[i];

            }
            else
            {
                if (article.Length - tmpCnt > l)
                {
                    break;
                }

                article += dls[i] + "\n";
                tmpCnt++;
            }
        }

        string last1 = "" + article[article.Length - 1];
        string last2 = "" + article[article.Length - 2];
        if (last1 == "，")
        {
            article += GetRandomAfter(afterCnt);
        }
        else if (last1 == "\n" && last2 == "，")
        {
            article = article.Substring(0, article.Length - 1) + GetRandomAfter(afterCnt);
        }

        return article;
    }
    private string GetRandomBefore(int max)
    {
        string ret = _articleData.befores[HZManager.GetInstance().GenerateRandomInt(0, max)];

        return ret;
    }
    private string GetRandomAfter(int max)
    {
        string ret = _articleData.afters[HZManager.GetInstance().GenerateRandomInt(0, max)];

        return ret;
    }
    //--------------------------json解析------------------------------------
    [Serializable]
    public class ArticleData
    {
        public List<string> famous;
        public List<string> boshes;
        public List<string> afters;
        public List<string> befores;

        public static ArticleData deserialize(string json)
        {
            return JsonUtility.FromJson<ArticleData>(json);
        }
    };

    private ArticleData _articleData = null;
    public int _REPEATE = 2;
    private void LoadArticleData()
    {
        if (_articleData == null)
        {
            TextAsset article = (TextAsset)Resources.Load("Data/article");

            JsonObject tmp = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(article.text);
            _articleData = ArticleData.deserialize(tmp.ToString());

            //重复度，默认值为2
            while (--_REPEATE > 0)
            {
                _articleData.famous.AddRange(_articleData.famous);
                _articleData.boshes.AddRange(_articleData.boshes);
            }
            
            //洗牌这两个数组
            _articleData.famous = HZManager.GetInstance().ListRandom(_articleData.famous);
            _articleData.boshes = HZManager.GetInstance().ListRandom(_articleData.boshes);
        }
    }
}
