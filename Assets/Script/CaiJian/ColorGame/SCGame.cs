/*
 *闯关诗词测试
 *
 */
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SCGame : MonoBehaviour
{
    public void Start()
    {

    }

    public void Init()
    {
        _OriPos = gameObject.transform.position;
        _CurrentTiMu.needInit = true;
    }
    public void ChangeBGColor(Color c)
    {
        _BG.color = c;
        Text[] allText = gameObject.GetComponentsInChildren<Text>(true);
        Color textColor = Define.GetUIFontColorByBgColor(_BG.color, Define.eFontAlphaType.FONT_ALPHA_128);
        Color c1 = new Color(textColor.r, textColor.g, textColor.b, 50 / 255f);

        Color fc = Define.GetFixColor(_BG.color);

        Image[] allImg = gameObject.GetComponentsInChildren<Image>(true);

        foreach (var t in allText)
        {
            if (t.name.Contains("CBYBGText"))
            {
                t.color = textColor;
                if (t.name.Contains("Light"))
                {
                    t.color = Define.GetLightColor(fc);
                }
            }
            else if (t.name == "Placeholder")
            {
                t.color = c1;
            }
        }

        foreach (var img in allImg)
        {
            if (img.name.Contains("Dark"))
            {
                img.color = Define.GetDarkColor(fc);
            }
            else if (img.name.Contains("Light"))
            {
                img.color = Define.GetLightColor(fc);
            }
        }
    }

    public GameObject _DT;//答题区
    private void DoDaTiAreaAni()
    {
        for (int i = (int)(eTMType.NONE) + 1; i < (int)(eTMType.END); i++)
        {
            DOTween.Kill("DoDaTiAreaAni"+ _CurrentTiMu.tMType);
            _DT.transform.Find("" + ((eTMType)i)).gameObject.SetActive(i == (int)_CurrentTiMu.tMType);
        }

        Color BgLightColor = Define.GetFixColor(_BG.color * 1.1f);
        Color BgDarkColor = Define.GetFixColor(_BG.color * 0.9f);


        Sequence saAni = DOTween.Sequence();
        saAni.SetId("DoDaTiAreaAni" + _CurrentTiMu.tMType);

        Image[] zdjd = _DT.transform.Find("" + _CurrentTiMu.tMType).Find("Tip").GetComponentsInChildren<Image>();
        foreach (var zd in zdjd)
        {
            zd.color = BgDarkColor;
        }

        saAni
            .Append(zdjd[0].DOColor(BgLightColor, 0.4f))
            .Append(zdjd[1].DOColor(BgLightColor, 0.4f))
            .Append(zdjd[2].DOColor(BgLightColor, 0.4f))

            .Append(zdjd[0].DOColor(BgDarkColor, 0.4f))
            .Append(zdjd[1].DOColor(BgDarkColor, 0.4f))
            .Append(zdjd[2].DOColor(BgDarkColor, 0.4f))
            .SetLoops(-1, LoopType.Restart);

    }

    public GameObject _editColorDialog;
    public Image _Mask;
    public Button _clickToCloseBtn;
    public Image _BG;

    public Text[] _TMTexts;
    private void InitTiMu()
    {
        _CurrentTiMu.tMType = (eTMType)HZManager.GetInstance().GenerateRandomInt((int)eTMType.NONE + 1,(int)eTMType.END);
        _CurrentTiMu.tMSubType = (eTMSubType)HZManager.GetInstance().GenerateRandomInt((int)eTMSubType.NONE + 1, (int)eTMSubType.END);
        

        for (int i = 1; i < _TMTexts.Length; i++)
        {
            _TMTexts[i].text = "";
        }

        if ( _CurrentTiMu.tMType == eTMType.PanDuan)
        {
            _TMTexts[0].text = "判断题：以下描述/诗句是否正确？";

            List<string> txtLine = GetSplitLineTxt(GetPanDuanTi());
            for (int i = 0; i < txtLine.Count; i++)
            {
                _TMTexts[i+1].text = txtLine[i];
            }
        }
        else if (_CurrentTiMu.tMType == eTMType.XuanZe)
        {
            List<string> xz = GetXunaZeTi();
            _TMTexts[0].text = "选择题：" + xz[0];
            _TMTexts[1].text = "A." + xz[1];
            _TMTexts[2].text = "B." + xz[2];
            _TMTexts[3].text = "C." + xz[3];
            _TMTexts[4].text = "D." + xz[4];
        }
        else if (_CurrentTiMu.tMType == eTMType.TianKong)
        {
            _TMTexts[0].text = "填空题：请填写空白部分";

            List<string> txtLine = GetSplitLineTxt(GetTianKongTi());
            for (int i = 0; i < txtLine.Count; i++)
            {
                _TMTexts[i+1].text = txtLine[i];
            }
        }
    }

    private List<string> GetSplitLineTxt(string fmtTxt)
    {
        List<string> ret = new List<string>();

        if (fmtTxt.Length <= 18)
        {
            ret.Add(fmtTxt);
        }
        else if (fmtTxt.Length <= 36)
        {
            ret.Add(fmtTxt.Substring(0,18));
            ret.Add(fmtTxt.Substring(18, fmtTxt.Length - 18));
        }
        else if (fmtTxt.Length <= 54)
        {
            ret.Add(fmtTxt.Substring(0, 18));
            ret.Add(fmtTxt.Substring(18, 18));
            ret.Add(fmtTxt.Substring(36, fmtTxt.Length - 36));
        }
        else
        {
            ret.Add(fmtTxt.Substring(0, 18));
            ret.Add(fmtTxt.Substring(18, 18));
            ret.Add(fmtTxt.Substring(36, 18));
            ret.Add(fmtTxt.Substring(54, fmtTxt.Length - 54));
        }

        return ret;
    }

    private string GetPanDuanTi()
    {
        List<string> scInfo = HZManager.GetInstance().GetTSSC(_CurrentTiMu.type, _CurrentTiMu.scID);

        float r = UnityEngine.Random.value;
        string tm = "";
        if (_CurrentTiMu.tMSubType == eTMSubType.SJ)
        {
            _CurrentTiMu.PDAnswer = r < 0.5f;
            if (_CurrentTiMu.PDAnswer)
            {
                tm = _CurrentTiMu.sc;
            }
            else
            {
                tm = _CurrentTiMu.sc;
                int hzCnt = HZManager.GetInstance().GetHZCnt(tm);
                //=====>替换错误字，形式为：1-x个字，或者整句
                //选中对应字数的汉字，该接口不会出现选中同一个汉字的情况
                List<int> ids = HZManager.GetInstance().GenerateRandomNoRptIntList(1, 0, hzCnt);

                int dff = 0;
                for (int i = 0; i < tm.Length; i++)
                {
                    //判断是否是标点符号
                    if (!HZManager.GetInstance().CheckIsHZ(tm[i]))
                    {
                        continue;
                    }

                    foreach (int index in ids)
                    {
                        if (dff == index)
                        {
                            Regex rex = new Regex("" + tm[i]);
                            string jz = GetRandomHalfShiJu();
                            tm = rex.Replace(tm, "" + GetHZ(tm[i], jz), 1, index);
                        }
                    }

                    dff++;
                }
            }
        }
        else if (_CurrentTiMu.tMSubType == eTMSubType.Author)
        {
            _CurrentTiMu.PDAnswer = r < 0.5f;

            if (_CurrentTiMu.PDAnswer)
            {
                tm = "“" + _CurrentTiMu.sc + "”是" + scInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe] + "的诗句。";
            }
            else
            {
                tm = "“" + _CurrentTiMu.sc + "”是" + GetRandomAuthor(scInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe]) + "的诗句。";
            }
        }
        else if (_CurrentTiMu.tMSubType == eTMSubType.Title)
        {
            _CurrentTiMu.PDAnswer = r < 0.5f;

            if (_CurrentTiMu.PDAnswer)
            {
                tm = "“" + _CurrentTiMu.sc + "”出自《" + scInfo[(int)HZManager.eTSSCColName.TSSC_TiMu] + "》。";
            }
            else
            {
                tm = "“" + _CurrentTiMu.sc + "”出自《" + GetRandomChuChu(scInfo[(int)HZManager.eTSSCColName.TSSC_TiMu]) + "》。";
            }
        }

        return tm;
    }

    private bool CheckExistAuthorOrTitle(List<string> authors,string author)
    {
        foreach (var ta in authors)
        {
            if (ta == author) return true;
        }

        return false;
    }
    private List<string> GetXunaZeTi()
    {
        List<string> scInfo = HZManager.GetInstance().GetTSSC(_CurrentTiMu.type, _CurrentTiMu.scID);
        List<string> tsscList = HZManager.GetInstance().GetFmtShiCi(scInfo[(int)HZManager.eTSSCColName.TSSC_NeiRong]);
        string oriAuthor = scInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe];
        string oriTitle = scInfo[(int)HZManager.eTSSCColName.TSSC_TiMu];
        List<string> tm = new List<string>();

        float r = UnityEngine.Random.value;
        int rightIndex = 0;
        if (r < 0.25)
        {
            _CurrentTiMu.XZAnswer = "A";
            rightIndex = 1;
        }
        else if (r < 0.5)
        {
            _CurrentTiMu.XZAnswer = "B";
            rightIndex = 2;
        }
        else if (r < 0.75)
        {
            _CurrentTiMu.XZAnswer = "C";
            rightIndex = 3;
        }
        else
        {
            _CurrentTiMu.XZAnswer = "D";
            rightIndex = 4;
        }

        //提前返回
        if (_CurrentTiMu.tMSubType == eTMSubType.Author)
        {
            tm.Add("“"+_CurrentTiMu.sc + "”的作者是____。");
            for (int i = 1; i < 5; i++)
            {
                if (i == rightIndex)
                {
                    tm.Add(oriAuthor);
                }
                else
                {
                    string ra = "";
                    do
                    {
                        ra = GetRandomAuthor(oriAuthor);

                    } while (CheckExistAuthorOrTitle(tm,ra));

                    tm.Add(ra);
                }
            }
            return tm;
        }
        else if (_CurrentTiMu.tMSubType == eTMSubType.Title)
        {
            tm.Add("“" + _CurrentTiMu.sc + "”出自《____》。");
            for (int i = 1; i < 5; i++)
            {
                if (i == rightIndex)
                {
                    tm.Add(oriTitle);
                }
                else
                {
                    string ra = "";
                    do
                    {
                        ra = GetRandomChuChu(oriTitle);

                    } while (CheckExistAuthorOrTitle(tm, ra));

                    tm.Add(ra);
                }
            }
            return tm;
        }

        //以下是诗句类型的选项

        //包含逗号，才扣整句，否则扣单字
        if (_CurrentTiMu.sc.Contains("，"))
        {
            string tmpsc = _CurrentTiMu.sc;
            string _BlankTiMu = "";

            string[] jzs = _CurrentTiMu.sc.Split('，');
            int indx = HZManager.GetInstance().GenerateRandomInt(0, jzs.Length);
            _BlankTiMu = jzs[indx];

            string bk = "";
            for (int j = 0; j < _BlankTiMu.Length; j++)
            {
                if (HZManager.GetInstance().CheckIsHZ(_BlankTiMu[j]))
                {
                    bk += "_";
                }
                else
                {
                    bk += _BlankTiMu[j];
                }
            }
            jzs[indx] = bk;
            tmpsc = String.Join("，", jzs);

            tm.Add(tmpsc);

            for (int i = 1; i < 5; i++)
            {
                if (rightIndex == i)
                {

                    if (!HZManager.GetInstance().CheckIsHZ(_BlankTiMu[_BlankTiMu.Length - 1]))
                    {
                        _BlankTiMu = _BlankTiMu.Substring(0, _BlankTiMu.Length - 1);
                    }

                    tm.Add(_BlankTiMu);

                }
                else
                {
                    int deadCnt = 0;
                    string dead = "";
                    string jz = "";

                    do
                    {
                        jz = GetRandomHalfShiJu();

                        dead = jz;

                        deadCnt++;

                        if (deadCnt >= 10)
                        { //最多查找10次，是否存在字数相等的诗句，如果不存在，则从原诗中查找
                            break;
                        }
                    } while (_CurrentTiMu.type != HZManager.eShiCi.SONGCI && jz.Length != _BlankTiMu.Length);

                    deadCnt = 0;
                    if (_CurrentTiMu.type != HZManager.eShiCi.SONGCI && jz.Length != _BlankTiMu.Length)
                    {
                        //如果长度不相等，则直接从原来的诗句中提取一句
                        string tssc3 = "";
                        //随机选取其中一句诗词
                        do
                        {
                            tssc3 = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];
                            deadCnt++;
                            if (deadCnt >= 5)
                            {
                                break;
                            }
                        } while (!tssc3.Contains("，") || tssc3.Equals(_CurrentTiMu.sc));

                        //陷入死循环，随便选取一句
                        if (deadCnt >= 5)
                        {
                            jz = dead;
                        }
                        else
                        {
                            string[] jzs3 = tssc3.Split('，');
                            jz = jzs3[HZManager.GetInstance().GenerateRandomInt(0, jzs3.Length)];

                            if (!HZManager.GetInstance().CheckIsHZ(jz[jz.Length - 1]))
                            {
                                jz = jz.Substring(0, jz.Length - 1);
                            }
                        }
                    }

                    tm.Add(jz);

                }
            }

            //不能存在相同的选项
            List<string> tmp = new List<string>();
            for (int i = 1; i < 5; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    if (tm[i] == tm[j]
                       && i != j
                       && rightIndex != i)
                    {
                        string jz = "";
                        do
                        {
                            jz = GetRandomHalfShiJu();
                        } while (CheckIfSJInXX(tm,jz));

                        tm[i] = jz;
                    }
                }
            }
        }
        else
        {
            string rtm = "";
            string hz = "";
            int index = 0;
            do
            {
                index = HZManager.GetInstance().GenerateRandomInt(0, _CurrentTiMu.sc.Length);
                hz = "" + _CurrentTiMu.sc[index];

            } while (!HZManager.GetInstance().CheckIsHZ(hz));

            for (int i = 0; i < _CurrentTiMu.sc.Length; i++)
            {
                if (i == index)
                {
                    rtm += "____";
                }
                else
                {
                    rtm += _CurrentTiMu.sc[i];
                }
            }

            tm.Add(rtm);

            //添加选项
            tm.Add("");
            tm.Add("");
            tm.Add("");
            tm.Add("");

            for (int i = 1; i < 5; i++)
            {
                if (rightIndex == i)
                {
                    tm[rightIndex] = hz;
                }
                else
                {
                    tm[i] = ""+GetHZ(hz[0], GetRandomHalfShiJu());
                }
            }
        }

        return tm;
    }

    private bool CheckIfSJInXX(List<string> tm,string jz)
    {
        bool exist = false;

        for (int j = 1; j < 5; j++)
        {
            if (jz.Equals(tm[j]))
            {
                exist = true;
                break;
            }
        }

        return exist;
    }

    private string GetTianKongTi()
    {
        string tm = "";
        List<string> scInfo = HZManager.GetInstance().GetTSSC(_CurrentTiMu.type, _CurrentTiMu.scID);
        if (_CurrentTiMu.tMSubType == eTMSubType.Author)
        {
            tm = "“"+_CurrentTiMu.sc + "”是____的诗句。";
            _CurrentTiMu.TKAnswer = scInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe];
        }
        else if (_CurrentTiMu.tMSubType == eTMSubType.Title)
        {
            tm = "“" + _CurrentTiMu.sc + "”出自《____》。";
            _CurrentTiMu.TKAnswer = scInfo[(int)HZManager.eTSSCColName.TSSC_TiMu];
        }
        else if (_CurrentTiMu.tMSubType == eTMSubType.SJ)
        {
            tm = _CurrentTiMu.sc;
            //如果包含逗号，则补全半句，不包含则补全某个字
            if (_CurrentTiMu.sc.Contains("，"))
            {
                string[] jzs = tm.Split('，');
                int indx = HZManager.GetInstance().GenerateRandomInt(0, jzs.Length);
                _CurrentTiMu.TKAnswer = jzs[indx];

                string bk = "";
                for (int j = 0; j < _CurrentTiMu.TKAnswer.Length; j++)
                {
                    if (HZManager.GetInstance().CheckIsHZ(_CurrentTiMu.TKAnswer[j]))
                    {
                        bk += "_";
                    }
                    else
                    {
                        bk += _CurrentTiMu.TKAnswer[j];
                    }
                }
                jzs[indx] = bk;
                tm = String.Join("，", jzs);

                //不包括标点
                if (!HZManager.GetInstance().CheckIsHZ(_CurrentTiMu.TKAnswer[_CurrentTiMu.TKAnswer.Length - 1]))
                {
                    _CurrentTiMu.TKAnswer = _CurrentTiMu.TKAnswer.Substring(0, _CurrentTiMu.TKAnswer.Length - 1);
                }
            }
            else
            {
                tm = "";
                string hz = "";
                int index = 0;
                do
                {
                    index = HZManager.GetInstance().GenerateRandomInt(0, _CurrentTiMu.sc.Length);
                    hz = ""+ _CurrentTiMu.sc[index];

                } while (!HZManager.GetInstance().CheckIsHZ(hz));

                _CurrentTiMu.TKAnswer = hz;

                for (int i = 0; i < _CurrentTiMu.sc.Length; i++)
                {
                    if (i == index)
                    {
                        tm += "____";
                    }
                    else
                    {
                        tm += _CurrentTiMu.sc[i];
                    }
                }
            }
        }

        return tm;
    }

    public string GetRandomAuthor(string oriZZ)
    {
        string zz = "";
        do
        {
            List<string> scInfo = HZManager.GetInstance().GetTSSC(HZManager.eShiCi.ALL);
            zz = scInfo[(int)HZManager.eTSSCColName.TSSC_ZuoZhe];

        } while (oriZZ == zz);

        return zz;
    }

    public string GetRandomChuChu(string oriCC)
    {
        string cc = "";
        do
        {
            List<string> scInfo = HZManager.GetInstance().GetTSSC(HZManager.eShiCi.ALL);
            cc = scInfo[(int)HZManager.eTSSCColName.TSSC_TiMu];

        } while (oriCC == cc);

        return cc;
    }

    public string GetRandomHalfShiJu(bool whole = false)
    {
        string jz = "";

        string tssc = GetRandomShiJu();
        string[] jzs2 = tssc.Split('，');

        int indx2 = HZManager.GetInstance().GenerateRandomInt(0, jzs2.Length);
        jz = jzs2[indx2];

        if (!whole)
        {
            if (!HZManager.GetInstance().CheckIsHZ(jz[jz.Length - 1]))
            {
                jz = jz.Substring(0, jz.Length - 1);
            }
        }

        return jz;
    }
    //获取整句诗，包括标点
    public string GetRandomShiJu()
    {
        string tssc = "";
        List<string> tsscData = new List<string>();
        //获取格式化后的诗词语句列表
        List<string> tsscList = new List<string>();
        //随机选取其中一句诗词作为考察对象
        do
        {
            tsscData = HZManager.GetInstance().GetTSSC(_CurrentTiMu.type);
            //获取格式化后的诗词语句列表
            tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[4]);

            tssc = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];

        } while (!tssc.Contains("，"));

        return tssc;
    }
    private char GetHZ(char ori, string sj)
    {
        char hz = '-';

        do
        {
            hz = sj[HZManager.GetInstance().GenerateRandomInt(0, sj.Length)];
        } while (hz == ori || !HZManager.GetInstance().CheckIsHZ(hz));

        return hz;
    }
    //在显示调节界面前，需要设置以上参数，否则呈现效果不正确
    private Vector3 _OriPos;
    public struct sCurrentTiMu
    {
        public bool needInit;
        public HZManager.eShiCi type;
        public int scID;
        public string sc;

        public eTMType tMType;
        public eTMSubType tMSubType;

        //判断题
        public bool PDAnswer;
        //选择题
        public string XZAnswer;
        //填空题答案
        public string TKAnswer;

        public bool IsCorrect;//是否回答正确
    }

    private sCurrentTiMu _CurrentTiMu;
    public void ShowSCGame(HZManager.eShiCi type, int id,string sc,Action<bool,bool> closeCb = null)
    {
        _closeCb = closeCb;

        //诗句都不一样了，说明已经切换了，即时题目还没有回答，也要换题
        if (sc != _CurrentTiMu.sc)
        {
            _CurrentTiMu.needInit = true;
        }

        //只有重新出题才需要赋值
        _CurrentTiMu.type = type;
        _CurrentTiMu.scID = id;
        _CurrentTiMu.sc = sc;
        _CurrentTiMu.IsCorrect = false;

        if (_CurrentTiMu.needInit)
        {
            InitTiMu();//初始化题目
            _CurrentTiMu.needInit = false;
        }

        ShowEditColor(true,false);
        DoDaTiAreaAni();
    }

    public void OnCloseSCGameClick()
    {
        ShowEditColor(false,false);
    }
    private void ShowEditColor(bool show,bool answered)
    {
        if (show == _editColorDialog.activeSelf) return;

        Sequence mySequence = DOTween.Sequence();
        if (show)
        {
            gameObject.transform.position = _OriPos;
            gameObject.transform.localScale = Vector3.one;

            gameObject.SetActive(false);
            _editColorDialog.SetActive(true);
            _Mask.gameObject.SetActive(false);
            mySequence
                .Append(gameObject.transform.DOScale(0.0f, 0.0f))
                .Join(_Mask.DOFade(0.0f, 0.0f))
                .AppendCallback(() => { gameObject.SetActive(true); _Mask.gameObject.SetActive(true); })
                .Append(_Mask.DOFade(100 / 255.0f, 0.3f))
                .Append(gameObject.transform.DOScale(FitUI.GetFitUIScale(), 0.6f))
                .SetEase(Ease.OutBounce);
        }
        else
        {
            _clickToCloseBtn.interactable = false;
            mySequence
                .Append(_Mask.DOFade(0.0f, 0.3f))
                .Join(gameObject.transform.DOScale(0.0f, 0.3f))
                .SetEase(Ease.InSine).OnComplete(() =>
                {
                    gameObject.SetActive(true);
                    _clickToCloseBtn.interactable = true;
                    _Mask.gameObject.SetActive(true);
                    _editColorDialog.SetActive(false);
                    _closeCb?.Invoke(_CurrentTiMu.IsCorrect, answered);
                });
            
        }
    }

    public enum eTMType
    {
        NONE,
        PanDuan,
        XuanZe,
        TianKong,
        //...
        END,
    }

    //题目子类型，单字、单句、作者、标题
    public enum eTMSubType
    {
        NONE,
        SJ,
        Author,//作者
        Title,//标题
        //...
        END,
    }

    private eTMType _CurrentTMType = eTMType.NONE;
    //选择可以有，选择上下句，作者，题目
    public void OnXuanZeDaanClick(string ans)
    {
        _CurrentTiMu.IsCorrect = (ans == _CurrentTiMu.XZAnswer);
        ShowEditColor(false, true);
        _CurrentTiMu.needInit = true;
    }

    private Action<bool,bool> _closeCb = null;
    //判断可以有错字，错句，错误作者，题目
    public void OnPanDuanDaanClick(bool ans)
    {
        _CurrentTiMu.IsCorrect = (ans == _CurrentTiMu.PDAnswer);
        ShowEditColor(false, true);
        _CurrentTiMu.needInit = true;
    }

    public Text _tkText;
    //填空可以是单字，单句，作者，题目
    public void OnTianKongDaanClick()
    {
        if (_tkText.text.Length == 0)
        {
            return;
        }

        _CurrentTiMu.IsCorrect = (_tkText.text == _CurrentTiMu.TKAnswer);
        ShowEditColor(false, true);
        _CurrentTiMu.needInit = true;
    }

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
}
