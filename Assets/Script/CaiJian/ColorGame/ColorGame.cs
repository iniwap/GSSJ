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
using System.Threading;

public class ColorGame: MonoBehaviour{

    // Use this for initialization
    void Start()
    {

    }
    //0.299*R + 0.587*G + 0.114*B=y
    private void OnEnable()
    {
        CheckDaanBuyState();

        _CurrentLevel = 0;
        _CurrentColorGameMode = eColorGameMode.NONE;

        _CurrentColorImg.gameObject.SetActive(false);
        _MakeXJBtn.SetActive(false);
        _MatrixColor.SetActive(false);
        _MatrixHSP.SetActive(false);
        _MatrixVSP.SetActive(false);
        _MatrixRule.SetActive(true);

        //
        if (scoreTexts == null)
        {
            //由于会改变次序，这里仅初始化一次
            scoreTexts = _ScoreResult.GetComponentsInChildren<Text>();
        }

        InitScore();
        DoStartArrowAni(true);
    }

    public void OnInit()
    {
        _SCGame.Init();
    }

    private void DoStartArrowAni(bool ani)
    {
        _BeginInfo.gameObject.SetActive(ani);
        DOTween.Kill("DoStartArrowAni");

        Color BgLightColor = Define.GetFixColor(_BG.color * 1.1f);
        Color BgDarkColor = Define.GetFixColor(_BG.color * 0.9f);

        if (ani)
        {
            Sequence saAni = DOTween.Sequence();
            saAni.SetId("DoStartArrowAni");

            Image[] zdjd = _BeginInfo.GetComponentsInChildren<Image>();
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
    
    }

    [System.Serializable] public class OnMakeXJByColorGameEvent : UnityEvent<List<string>, string> { }
    public OnMakeXJByColorGameEvent OnMakeXJByColorGame;
    public GameObject _MakeXJBtn;
    public void OnMakeXJBtnClick()
    {
        //判断当前颜色是否还存在？
        //不存在不允许使用
        //生成信笺
        if (!PickColor.CheckColorIDExist(_CurrentColor.colorID))
        {
            string hex = Define.GetHexColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
            ShowToast("啊哦～色卡[<color=" + hex + ">" + _CurrentColor.cname + "</color>]已被删除，不能用来制作信笺", 3f);
            return;
        }

        string content = "";
 
        if (_CurrentLevel == 0 || _GameEnd)
        {
            content = "返回主界面，使用当前诗色 【"+_CurrentColor.cname + "】制作信笺？";
        }
        else
        {
            content = "是否放弃闯关，使用当前诗色【" + _CurrentColor.cname + "】制作信笺？";
        }
        
        OnBackToMakeXJ(content,"制作",()=> {
            OnMakeXJByColorGame.Invoke(PickColor.GetColorByID(_CurrentColor.colorID), _CurrentColor.cdes);
            ShowToast("行线、诗词颜色均可在装饰里修改哟～");
        },
        ()=> {
            ShowToast("当发现喜欢的颜色或诗句时，制作信笺是不错的选择");
        });
    }

    public void OnUsePickColorPic()
    {
        if (!gameObject.activeSelf) return;//不在辨色界面，不需要处理

        string content = "";
        if (_CurrentColorImg.gameObject.activeSelf)
        {
            if (_CurrentLevel == 0 || _GameEnd)
            {
                content = "是否返回主界面，查看刚生成的信笺？";
            }
            else
            {
                content = "是否放弃当前闯关，查看刚生成的信笺？";
            }
        }
        else
        {
            content = "是否返回主界面，查看刚生成的信笺？";
        }

        OnBackToMakeXJ(content,"查看");
    }

    private void OnBackToMakeXJ(string content,string okName,Action okCb = null, Action cancelCb = null,Action closeCb = null)
    {

        OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam(content,
            MaskTips.eDialogType.OK_CANCEL_BTN,
            (MaskTips.eDialogBtnType type) =>
            {
                if (type == MaskTips.eDialogBtnType.OK)
                {
                    //一些清理、保存工作
                    _CurrentLevel = 0;
                    OnOpenColorGame.Invoke(false);
                    CancelInvoke("ShowDoTips");//不再提示

                    okCb?.Invoke();
                }
                else if (type == MaskTips.eDialogBtnType.CANCEL)
                {
                    cancelCb?.Invoke();
                }
                else
                {
                    closeCb?.Invoke();
                }
            }, okName));
    }

    [System.Serializable] public class OnShowRankEvent : UnityEvent<GameCenter.LeaderboardType> { }
    public OnShowRankEvent OnShowRank;
    public void OnRankBtnClick()
    {
        OnShowRank.Invoke(GameCenter.LeaderboardType.FindColor);
    }

    [System.Serializable] public class OnReportScoreEvent : UnityEvent<long, GameCenter.LeaderboardType> { }
    public OnReportScoreEvent OnReportScore;
    public void ReportScore(long score)
    {
        OnReportScore.Invoke(score, GetLBType());
    }

    public GameCenter.LeaderboardType GetLBType()
    {
        if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
        {
            return GameCenter.LeaderboardType.FindColor;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor)
        {
            return GameCenter.LeaderboardType.SingleColor;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeMultiColor)
        {
            return GameCenter.LeaderboardType.MultiColor;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            return GameCenter.LeaderboardType.LinkColor;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            return GameCenter.LeaderboardType.DecomposeColor;
        }

        return GameCenter.LeaderboardType.FindColor;
    }
    public GameObject _ScoreResult;
    private Text[] scoreTexts;
    private void InitScore()
    {
        int findHS = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_FINDCOLOR_SCORE, 0);
        int singleHS = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_SINGLE_SCORE, 0);
        int multiHS = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_MULTI_SCORE, 0);
        int linkHS = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_LINK_SCORE, 0);
        int decHS = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_DECOMPOSE_SCORE, 0);
        List<int> hs = new List<int>();
        hs.Add(findHS);
        hs.Add(singleHS);
        hs.Add(multiHS);
        hs.Add(linkHS);
        hs.Add(decHS);

        Color c = Define.GetUIFontColorByBgColor(_BG.color, Define.eFontAlphaType.FONT_ALPHA_128);

        for (int i = (int)eColorGameMode.NONE + 1; i < (int)eColorGameMode.END; i++)
        {
            scoreTexts[(i-1) * 3].color = c;
            scoreTexts[(i-1) * 3 + 1].color = c;
            scoreTexts[(i-1) * 3 + 2].color = c;

            scoreTexts[(i - 1) * 3].text = scoreTexts[(i - 1) * 3].text.Replace("<b>", "");
            scoreTexts[(i - 1) * 3].text = scoreTexts[(i - 1) * 3].text.Replace("</b>", "");
            scoreTexts[(i-1) * 3 + 1].text = "0";
            scoreTexts[(i-1) * 3 + 2].text = ""+ hs[i - 1];
        }
    }

    private void HighlightScoreMode()
    {
        Color c = Define.GetUIFontColorByBgColor(_BG.color, Define.eFontAlphaType.FONT_ALPHA_128);

        //排到最前面
        for (int i = (int)eColorGameMode.NONE + 1; i < (int)eColorGameMode.END; i++)
        {
            scoreTexts[(i - 1) * 3].color = c;
            scoreTexts[(i - 1) * 3 + 1].color = c;
            scoreTexts[(i - 1) * 3 + 2].color = c;

            scoreTexts[(i - 1) * 3].text = scoreTexts[(i - 1) * 3].text.Replace("<b>","");
            scoreTexts[(i - 1) * 3].text = scoreTexts[(i - 1) * 3].text.Replace("</b>", "");
            scoreTexts[(i - 1) * 3 + 1].text = "0";//非选中模式当前为0
            scoreTexts[(i - 1) * 3 + 2].text = scoreTexts[(i - 1) * 3 + 2].text.Replace("<b>", "");
            scoreTexts[(i - 1) * 3 + 2].text = scoreTexts[(i - 1) * 3 + 2].text.Replace("</b>", "");


            Transform t = _ScoreResult.transform.Find(""+((eColorGameMode)i));
            t.SetSiblingIndex(i + 1);
        }

        Transform t2 = _ScoreResult.transform.Find("" + _CurrentColorGameMode);
        t2.SetSiblingIndex(0);

        //高亮列
        Color dc = Define.GetLightColor(Define.GetFixColor(_BG.color));
        scoreTexts[((int)_CurrentColorGameMode - 1) * 3].color = dc;
        scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 1].color = dc;
        scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 2].color = dc;

        scoreTexts[((int)_CurrentColorGameMode - 1) * 3].text = "<b>"+ scoreTexts[((int)_CurrentColorGameMode - 1) * 3].text + "</b>";
        scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 1].text = "<b>" + _CurrentLevel + "</b>";
        scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 2].text = "<b>" + scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 2].text + "</b>";
    }

    private void SaveScore()
    {
        //标题动画，依次执行数字动画
        DoScoreTextAni(scoreTexts[((int)_CurrentColorGameMode - 1) * 3], scoreTexts[((int)_CurrentColorGameMode - 1) * 3].text,0.3f);

        DoScoreTextAni(scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 1], "<b>" + _CurrentLevel + "</b>", 0.6f);

        int score = 0;
        if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
        {
            score = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_FINDCOLOR_SCORE, 0);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor)
        {
            score = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_SINGLE_SCORE, 0);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeMultiColor)
        {
            score = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_MULTI_SCORE, 0);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            score = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_LINK_SCORE, 0);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            score = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_DECOMPOSE_SCORE, 0);
        }

        if (score < _CurrentLevel)
        {
            DoScoreTextAni(scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 2], "<b>" + _CurrentLevel + "</b>", 1f);

            ReportScore(_CurrentLevel);

            if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
            {
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_FINDCOLOR_SCORE, _CurrentLevel);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor)
            {
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_SINGLE_SCORE, _CurrentLevel);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeMultiColor)
            {
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_MULTI_SCORE, _CurrentLevel);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
            {
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_LINK_SCORE, _CurrentLevel);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
            {
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.COLOR_GAME_DECOMPOSE_SCORE, _CurrentLevel);
            }
        }
    }

    private void DoScoreTextAni(Text t,string s,float delay = 0f)
    {
        Sequence textAniSeq = DOTween.Sequence();
        textAniSeq.AppendInterval(delay)
                  .AppendCallback(()=>t.text = s)
                  .Join(t.transform.DOScale(1.1f, 0.5f))
                  //.Append(t.DOFade(0.5f, 0.5f))
                  .Append(t.transform.DOScale(0.9f, 0.5f))
                  //.Append(t.DOFade(1.0f, 0.5f))
                  .Append(t.transform.DOScale(1.0f, 0.5f))
                  .SetEase(Ease.OutBounce);
    }
    [System.Serializable] public class OnOpenColorGameEvent : UnityEvent<bool> { }
    public OnOpenColorGameEvent OnOpenColorGame;
    public void OnBackBtnClick()
    {
        if (_CurrentLevel != 0 && !_GameEnd)
        {
            OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam("当前正在闯关，是否放弃？",
                MaskTips.eDialogType.OK_CANCEL_BTN,
                (MaskTips.eDialogBtnType type) =>
            {
                if (type == MaskTips.eDialogBtnType.OK)
                {
                    //一些清理、保存工作
                    _CurrentLevel = 0;
                    OnOpenColorGame.Invoke(false);
                    CancelInvoke("ShowDoTips");//不再提示
                }
            }));
        }
        else
        {
            OnOpenColorGame.Invoke(false);
            CancelInvoke("ShowDoTips");//不再提示
        }
    }

    private bool CheckIsPickColor()
    {
        return _CurrentColor.colorID >= HZManager.GetInstance().GetColorCnt();
    }

    [System.Serializable] public class OnOpenPickColorEvent : UnityEvent<bool> { }
    public OnOpenPickColorEvent OnOpenPickColor;
    public UnityEvent OnOpenPickColorStorage;
    [System.Serializable] public class OnOpenDictEvent : UnityEvent<bool> { }
    public OnOpenDictEvent OnOpenDict;
    public void OnCurrentColorBtnClick()
    {
        //分色没有诗句关联
        if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor) return;

        string content = "";
        if (CheckIsPickColor())
        {
            content = "该颜色为色库颜色，可前往色库编辑颜色。(不会同步更新该颜色信息)";
            if (!PickColor.CheckColorIDExist(_CurrentColor.colorID))
            {
                ShowToast("色卡已被删除，无法查看更多信息:(");
                return;
            }
        }
        else
        {
            content = "该颜色为内置颜色，可前往查询诗词信息。(需手动输入色块上的诗句)";
        }

        OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam(content,
            MaskTips.eDialogType.OK_CANCEL_BTN,
            (MaskTips.eDialogBtnType type) =>
            {
                float r = UnityEngine.Random.value;

                if (type == MaskTips.eDialogBtnType.OK)
                {
                    CancelInvoke("ShowDoTips");//不再提示

                    if (CheckIsPickColor())
                    {
                        OnOpenPickColor.Invoke(true);
                        OnOpenPickColorStorage.Invoke();
                        if (r < 0.7)
                        {
                            ShowToast("编辑、删除色卡不会同步更新当前展示的颜色信息");
                        }
                        else
                        {
                            ShowToast("编辑色卡信息将在下一关被选中时生效，不会同步当前", 3f);
                        }
                    }
                    else
                    {
                        OnOpenDict.Invoke(true);
                    }
                }
                else if (type == MaskTips.eDialogBtnType.CANCEL)
                {
                    if (CheckIsPickColor())
                    {
                        if (r < 0.3f)
                        {
                            ShowToast("色库界面可随时编辑、删除、创建色卡");
                        }
                        else if (r > 0.6f)
                        {
                            ShowToast("赋予某种颜色以心情，创建属于自己的颜色日记");
                        }
                        else
                        {
                            ShowToast("色库颜色描述为自书写，因此不提供诗词闯关机会");
                        }
                    }
                    else
                    {
                        if (r < 0.3f)
                        {
                            ShowToast("为加强诗词记忆，前往查询需手动输入颜色诗句");
                        }
                        else if (r > 0.6f)
                        {
                            ShowToast("内置颜色关联诗句在闯关失败，回答问题时会用到");
                        }
                        else
                        {
                            ShowToast("每局答题闯关仅有一次机会，且仅与颜色诗词有关");
                        }
                    }
                }
            },"前往"));
    }

    //需要与ui保持一致
    public enum eColorGameMode
    {
        NONE,//默认
        ModeFindColor,//找色
        ModeSingleColor,//单色
        ModeMultiColor,//多色
        ModeLinkColor,//连色
        ModeDecomposeColor,//分色
        END,
    }
    private string GetColorGameModeName(eColorGameMode mode)
    {
        string mname = "";
        switch (mode)
        {
            case eColorGameMode.ModeFindColor:
                mname = "找色";
                break;
            case eColorGameMode.ModeMultiColor:
                mname = "多色";
                break;
            case eColorGameMode.ModeSingleColor:
                mname = "单色";
                break;
            case eColorGameMode.ModeDecomposeColor:
                mname = "分色";
                break;
            case eColorGameMode.ModeLinkColor:
                mname = "连色";
                break;
        }

        return mname;
    }
    private eColorGameMode _CurrentColorGameMode = eColorGameMode.NONE;
    private int _CurrentLevel = 0;
    private void StartGame(eColorGameMode mode)
    {
        if (_GameEnd)
        {
            _GameEnd = false;
            _CurrentLevel = 0;
        }

        if (_CurrentLevel != 0)
        {
            string content = "";
            if (_CurrentColorGameMode == mode)
            {
                content = "是否放弃当前闯关，重新开始？";
            }
            else
            {
                content = "是否放弃当前闯关，切换到 【"+ GetColorGameModeName(mode)+ "】玩法？";
            }

            OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam(content,
                MaskTips.eDialogType.OK_CANCEL_BTN,
                (MaskTips.eDialogBtnType type) =>
            {
                if (type == MaskTips.eDialogBtnType.OK)
                {
                    //一些清理、保存工作
                    InitGame(mode, GetColorFrom(mode));
                }
                else
                {
                    
                }
            }));
        }
        else
        {
            //如果没有格子总数发生变化，是不需要每次调用的，事实上这里是固定的
            InitGame(mode, GetColorFrom(mode));
        }
    }

    private void InitGame(eColorGameMode mode, eColorGameMode from)
    {
        _HasAnswered = false;//重置没有使用过问答
        _CurrentLevel = 0;
        _RGBRightCnt = 0;

        _CurrentColorGameMode = mode;
        _CurrentColorImg.gameObject.SetActive(true);
        _MakeXJBtn.SetActive(true);
        DoStartArrowAni(false);

        InitMatrix(from,true);

        ShowDoTips();
    }

    private void ShowDoTips()
    {
        string hex = Define.GetHexColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
        if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
        {
            ShowToast("请找出与左上方[<color=" + hex + ">" + _CurrentColor.cname + "</color>]<size=42><b>相同</b>的色块</size>", 3f);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            ShowToast("请找出所有与左上方[<color=" + hex + ">" + _CurrentColor.cname + "</color>]<size=42><b>相同</b>的色块</size>", 3f);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor || _CurrentColorGameMode == eColorGameMode.ModeMultiColor)
        {
            ShowToast("请找出与左上方[<color=" + hex + ">" + _CurrentColor.cname + "</color>]<size=42><b>不相同</b></size>的色块", 3f);
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            string hexr = Define.GetHexColor(_CurrentColor.r, 0, 0);
            string hexg = Define.GetHexColor(0, _CurrentColor.g, 0);
            string hexb = Define.GetHexColor(0, 0, _CurrentColor.b);

            ShowToast("分别找出与左上方[<color=" + hexr + ">红</color>/"+ "<color=" + hexg + ">绿</color>/" + "<color=" + hexb + ">蓝</color>]<size=42><b>对应相同</b></size>的色块", 3f);
        }
    }
    private void InitMatrix(eColorGameMode from,bool start,bool highlight = true)
    {
        _HasUsedCKDA = false;//新的一关，可以使用查看答案

        if (highlight) HighlightScoreMode();
        InitMatrixSP();

        List<Color> rcs = new List<Color>();

        if (start)
        {
            InitColor(from);
            if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
            {
                //0-10000
                //
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                rcs.Add(ec);
                InitMatrixColor(GetListEC(ec), rcs);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
            {
                InitLinkColorMatrix();
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor
                || _CurrentColorGameMode == eColorGameMode.ModeMultiColor)
            {
                //单色模式颜色不变
                //生成颜色方阵
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                Color rc = GetColorByLevel(ec, _CurrentLevel);
                //如果完全一致，说明到底了，此时出错，应该结束
                rcs.Add(rc);
                InitMatrixColor(GetListEC(ec), rcs);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
            {
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                Color ecr = Define.GetUnityColor(_CurrentColor.r, 0, 0);
                Color ecg = Define.GetUnityColor(0, _CurrentColor.g, 0);
                Color ecb = Define.GetUnityColor(0, 0, _CurrentColor.b);
                //如果完全一致，说明到底了，此时出错，应该结束
                rcs.Add(ecr);
                rcs.Add(ecg);
                rcs.Add(ecb);
                InitMatrixColor(GetListEC(ec), rcs);
            }
        }
        else
        {
            if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
            {
                InitColor(from);

                //0-10000
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                rcs.Add(ec);
                InitMatrixColor(GetListEC(ec), rcs);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
            {
                InitColor(from);
                InitLinkColorMatrix();
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor)
            {
                //单色模式颜色不变
                //生成颜色方阵
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                Color rc = GetColorByLevel(ec, _CurrentLevel);
                //如果完全一致，说明到底了，此时出错，应该结束
                rcs.Add(rc);
                InitMatrixColor(GetListEC(ec), rcs);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeMultiColor)
            {
                InitColor(from);
                //生成颜色方阵
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                Color rc = GetColorByLevel(ec, _CurrentLevel);
                //如果完全一致，说明到底了，此时出错，应该结束
                rcs.Add(rc);
                InitMatrixColor(GetListEC(ec),rcs);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
            {
                InitColor(from);
                //生成颜色方阵
                Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
                Color ecr = Define.GetUnityColor(_CurrentColor.r, 0, 0);
                Color ecg = Define.GetUnityColor(0, _CurrentColor.g, 0);
                Color ecb = Define.GetUnityColor(0, 0, _CurrentColor.b);
                //如果完全一致，说明到底了，此时出错，应该结束
                rcs.Add(ecr);
                rcs.Add(ecg);
                rcs.Add(ecb);
                InitMatrixColor(GetListEC(ec), rcs);
            }
        }

    }

    private List<int> _LinkColorRightIndex = new List<int>();
    private void InitLinkColorMatrix()
    {
        int level = GetGridLevel();
        int currentIndex = HZManager.GetInstance().GenerateRandomInt(0, level * level);
        int col = currentIndex % level;
        int row = currentIndex / level;
        SearchSJPath(col, row, level, (List<int> path) =>
        {
            List<Color> rcs = new List<Color>();
            //hzcnt个汉字的连通路径
            int hzCnt = _CurrentColor.cdes.Length;

            _LinkColorRightIndex.Clear();
            _LinkColorRightIndex.AddRange(path);

            Color ec = Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b);
            InitMatrixColor(GetListEC(ec), rcs);

            //设置选定色块的颜色
            for (int i = 0; i < level; i++)
            {
                for (int j = 0; j < level; j++)
                {
                    GameObject obj = _ColorBtnList[i * level + j];
                    obj.SetActive(true);
                    obj.transform.localScale = Vector3.zero;

                    Button cbtn = obj.GetComponent<Button>();

                    //隐藏名字
                    Text cname = cbtn.GetComponentInChildren<Text>(true);
                    cname.gameObject.SetActive(false);
                    cname.text = "";
                    for (int p = 0; p < path.Count; p++)
                    {
                        if (j == path[p] % level && i == path[p] / level)
                        {
                            cbtn.GetComponent<Image>().color = ec;
                            obj.name = "right";
                            cname.text = "" + _CurrentColor.cdes[p] + "<size=20>"+(p+1)+"</size>";
                        }
                    }
                }
            }

        });
    }
    private List<Color> GetListEC(Color ec)
    {
        List<Color> ecs = new List<Color>();
        int level = GetGridLevel();
        int noRptPosCnt = 1;
        if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            noRptPosCnt = 3;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            noRptPosCnt = 0;//连色模式全部填满
        }

        int cnt = level * level - noRptPosCnt;
        for (int i = 0; i < cnt; i++)
        {
            if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor
                || _CurrentColorGameMode == eColorGameMode.ModeMultiColor)
            {
                ecs.Add(ec);
            }
            else if(_CurrentColorGameMode == eColorGameMode.ModeFindColor)
            {
                //
                Color rc = GetColorByLevel(ec, HZManager.GetInstance().GenerateRandomInt(0, (int)Math.Sqrt(_CurrentLevel)));
                ecs.Add(rc);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
            {
                //
                Color rc = GetColorByLevel(ec, HZManager.GetInstance().GenerateRandomInt(0, (int)Math.Sqrt(_CurrentLevel)));
                ecs.Add(rc);
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
            {
                if (i < cnt / 3)
                {
                    //红
                    float rgb = GetRGBByLevel(ec.r, (int)Math.Sqrt(_CurrentLevel));

                    ecs.Add(new Color(rgb,0,0));
                }
                else if (i < 2 * cnt / 3)
                {
                    //黄
                    float rgb = GetRGBByLevel(ec.g, (int)Math.Sqrt(_CurrentLevel));

                    ecs.Add(new Color(0, rgb, 0));
                }
                else
                {
                    //蓝
                    float rgb = GetRGBByLevel(ec.b,(int)Math.Sqrt(_CurrentLevel));

                    ecs.Add(new Color(0, 0, rgb));
                }

            }
        }

        return ecs;
    }
    public GameObject _ColorPrefabs;
    private List<GameObject> _ColorBtnList = new List<GameObject>();
    private void InitMatrixColor(List<Color> ecs, List<Color> rcs)
    {
        if (rcs.Count == 1 && ecs[0].Equals(rcs[0]))
        {
            //分色模式没有提示，需要判断全部，暂且不提示了
            ShowToast("这不科学！你尽管蒙！因为已没有答案或全是正确答案", 5f);
        }

        //性能优化，不删除列表，仅生成一次
        //DestroyObj(_ColorBtnList);
        if (_ColorBtnList.Count == 0)
        {
            for (int i = 0; i < Define.MAX_XT_HZ_MATRIX; i++)
            {
                for (int j = 0; j < Define.MAX_XT_HZ_MATRIX; j++)
                {
                    GameObject obj = Instantiate(_ColorPrefabs, _MatrixColor.transform) as GameObject;
                    obj.SetActive(false);
                    _ColorBtnList.Add(obj);
                    obj.transform.localScale = Vector3.zero;
                    obj.name = "error";

                    Button cbtn = obj.GetComponent<Button>();
                    cbtn.onClick.AddListener(delegate ()
                    {
                        CheckGameResult(obj);
                    });
                }
            }
        }
        else
        {
            DOTween.Kill("ResultAni");
            foreach (var obj in _ColorBtnList)
            {
                obj.SetActive(false);
                obj.name = "error";

                Text cname = obj.GetComponentInChildren<Text>(true);
                cname.transform.localScale = Vector3.one;
                cname.color = new Color(cname.color.r, cname.color.g, cname.color.b,0.5f);
            }
        }


        float wh = GetGridLevelSizeWH();
        GridLayoutGroup gl = _MatrixColor.GetComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(wh, wh);
        int level = GetGridLevel();
        int noRptPosCnt = 1;
        if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            noRptPosCnt = 3;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            noRptPosCnt = 0;
        }

        List<int> noRptPosList = HZManager.GetInstance().GenerateRandomNoRptIntList(noRptPosCnt, 0,level*level);
        int cnt = 0;
        for (int i = 0; i < level; i++)
        {
            for (int j = 0; j < level; j++)
            {
                GameObject obj = _ColorBtnList[i * level + j];
                obj.SetActive(true);
                obj.transform.localScale = Vector3.zero;

                Button cbtn = obj.GetComponent<Button>();

                //隐藏名字
                Text cname = cbtn.GetComponentInChildren<Text>(true);
                cname.gameObject.SetActive(false);
                cname.text = "";
                bool ext = false;
                for (int p = 0; p < noRptPosList.Count; p++)
                {
                    if (j == noRptPosList[p]%level && i == noRptPosList[p] / level)
                    {
                        if (_CurrentColorGameMode != eColorGameMode.ModeDecomposeColor)
                        {
                            cbtn.GetComponent<Image>().color = rcs[0];//如果不是分色模式，那只有一个正确颜色
                        }
                        else
                        {
                            cbtn.GetComponent<Image>().color = rcs[p];//有三个正确颜色
                        }
                        obj.name = "right";
                        //cbtn.onClick.RemoveAllListeners();
                        if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
                        {
                            cname.text = GetCurrentCName();
                        }
                        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
                        {
                            //连色先初始化全部，这里并不设置
                        }
                        else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor
                            || _CurrentColorGameMode == eColorGameMode.ModeMultiColor)
                        {
                            cname.text = "非\n·\n" + _CurrentColor.cname;
                        }
                        else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
                        {
                            if (p == 0)
                            {
                                cname.text = "R·红\n"+_CurrentColor.r;
                                obj.name += "-r";
                            }
                            else if (p == 1)
                            {
                                cname.text = "G·绿\n" + _CurrentColor.g;
                                obj.name += "-g";
                            }
                            else if (p == 2)
                            {
                                cname.text = "B·蓝\n" + _CurrentColor.b;
                                obj.name += "-b";
                            }
                        }

                        ext = true;

                        break;
                    }
                }
                if (!ext)
                {
                    cbtn.GetComponent<Image>().color = ecs[cnt];
                    cnt++;
                }
            }
        }

        //可以有动画
        Sequence colorBtnAni = DOTween.Sequence();
        foreach (var btn in _ColorBtnList)
        {
            if(btn.activeSelf)
                colorBtnAni.Join(btn.transform.DOScale(1.0f,0.5f));
        }

        colorBtnAni.SetEase(Ease.InSine);
    }

    public SCGame _SCGame;//诗词闯关
    private bool _GameEnd = false;
    private int _RGBRightCnt = 0;

    //该处需要区分处理分色玩法，分色玩法需要收集三种颜色
    private void DoResultColorAni(bool ckda,float delay = 0f)
    {
        DoScoreAni();
        CancelInvoke("ShowDoTips");//先取消
        if(!ckda)
            ShowToast("恭喜选择正确，请继续闯关吧 :)",0.5f);
        Sequence aniSJSeq = DOTween.Sequence();
        aniSJSeq.SetId("ResultAni");
        aniSJSeq.AppendInterval(delay)
            .OnComplete(GoNextLevel);

    }
    private void DoLinkColorResultAni(float delay = 0f)
    {
        DoScoreAni();
        //需要禁止任何操作
        //所有的执行动画，完成后再执行下一关
        Sequence aniSJSeq = DOTween.Sequence();
        aniSJSeq.SetId("ResultAni");
        float speed = 5f / _CurrentColor.cdes.Length;
        if (speed > 0.5f) speed = 0.5f;
        aniSJSeq.AppendInterval(0.5f);
        aniSJSeq.AppendInterval(0.01f);

        for (int i = 0; i < _LinkColorRightIndex.Count; i++)
        {
            GameObject cobj = _ColorBtnList[_LinkColorRightIndex[i]];
            //隐藏名字
            Text cname = cobj.GetComponentInChildren<Text>(true);
            aniSJSeq.Join(cname.DOFade(0f, 1f));
        }
        for (int i = 0; i < _LinkColorRightIndex.Count; i++)
        {
            GameObject cobj = _ColorBtnList[_LinkColorRightIndex[i]];
            //隐藏名字
            Text cname = cobj.GetComponentInChildren<Text>(true);
            aniSJSeq.Append(cname.DOFade(0.5f, speed))
                .Join(cname.transform.DOScale(1.5f, speed));
        }

        aniSJSeq.AppendInterval(delay);
        aniSJSeq.OnComplete(GoNextLevel);
    }
    private void CheckGameResult(GameObject obj)
    {
        if (DOTween.IsTweening("ResultAni"))
        {
            //此时不能再选择
            return;
        }

        if (obj.name.Contains("right"))
        {
            //分色模式支持答题过于繁琐，不支持诗句问答
            if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
            {
                if (_GameEnd)
                {
                    GameEnd("本局已结束。是否重新开始？");
                    return;
                }

                _RGBRightCnt++;

                obj.name = "rgb";

                Text rightColorName = obj.GetComponentInChildren<Text>(true);
                rightColorName.gameObject.SetActive(true);
                rightColorName.color = GetTextColor(obj.GetComponent<Image>().color);

                if (_RGBRightCnt == 3)
                {
                    DoResultColorAni(false,0.5f);
                }
                else
                {
                    if (obj.name.Contains("-r"))
                    {
                        string hexr = Define.GetHexColor(_CurrentColor.r, 0, 0);
                        ShowToast("找到[<color=" + hexr + ">红</color>]色分量，请继续寻找剩余分量色块");
                        _RText.gameObject.SetActive(true);
                    }
                    else if (obj.name.Contains("-g"))
                    {
                        string hexg = Define.GetHexColor(0, _CurrentColor.g, 0);
                        ShowToast("找到[<color=" + hexg + ">绿</color>]色分量，请继续寻找剩余分量色块");
                        _GText.gameObject.SetActive(true);
                    }
                    else if (obj.name.Contains("-b"))
                    {
                        string hexb = Define.GetHexColor(0, 0, _CurrentColor.b);
                        ShowToast("找到[<color=" + hexb + ">蓝</color>]色分量，请继续寻找剩余分量色块");
                        _BText.gameObject.SetActive(true);
                    }
                }
            }
            else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
            {
                if (_GameEnd)
                {
                    GameEnd("本局已结束。是否重新开始？");
                    return;
                }

                _RGBRightCnt++;

                obj.name = "rgb";

                Text rightColorName = obj.GetComponentInChildren<Text>(true);
                rightColorName.gameObject.SetActive(true);
                rightColorName.color = GetTextColor(obj.GetComponent<Image>().color);

                //全部找到
                if (_RGBRightCnt == _CurrentColor.cdes.Length)
                {
                    CancelInvoke("ShowDoTips");//先取消
                    ShowToast("恭喜找对全部色块，来品读一遍原诗句吧:)",3f);
                    DoLinkColorResultAni(0.5f);
                }
            }
            else
            {
                if (_GameEnd)
                {
                    if (CheckIsPickColor())
                    {
                        GameEnd("本局已结束。是否重新开始？");
                    }
                    else
                    {
                        //此处出一道题，回答正确，可继续闯关
                        //否则只能重新开始，不能再取消。
                        //
                        if (_HasAnswered)
                        {
                            GameEnd("本局已结束。是否重新开始？");
                            return;
                        }

                        _CurrentColorDes.DOFade(0.0f, 0.5f);
                        _SCGame.ShowSCGame(_CurrentColor.scType, _CurrentColor.scID, _CurrentColor.cdes, (bool right, bool answered) =>
                        {
                            _CurrentColorDes.DOFade(0.5f, 0.5f);
                            //只有作答了才会调用，普通关闭不会调用
                            _HasAnswered = answered;
                            if (_HasAnswered)
                            {
                                if (right)
                                {
                                    //回答正确，继续闯关
                                    ShowToast("恭喜回答正确，直接进入下一关！本局不再提供额外答题", 2f);
                                    DoScoreAni();
                                    GoNextLevel();
                                }
                                else
                                {
                                    obj.name = "error";
                                    ShowToast("很遗憾没能答对呢，请选择一种玩法重新开始闯关吧");
                                }
                            }
                        });
                    }

                    return;
                }

                Text rightColorName = obj.GetComponentInChildren<Text>(true);
                rightColorName.gameObject.SetActive(true);
                rightColorName.color = GetTextColor(obj.GetComponent<Image>().color);

                DoResultColorAni(false,0.5f);
            }
        }
        else
        {
            //点击错误的色块，如果游戏已结束，就提示结束
            if (_GameEnd)
            {
                GameEnd("本局已结束。是否重新开始？");
                return;
            }

            if (obj.name == "rgb")
            {
                ShowToast("该色块已经被选择过，请选择其他色块");
                return;
            }

            CancelInvoke("ShowDoTips");//先取消
            //显示答案
            ShowColorResult();

            GameEnd("啊哦～选错了，本局闯关结束。是否重新开始？");
        }
    }
    
    //显示正确的颜色答案
    private void ShowColorResult()
    {
        foreach (var cbtn in _ColorBtnList)
        {
            if (cbtn.name.Contains("right"))
            {
                //显示其名字
                Text rightColorName = cbtn.GetComponentInChildren<Text>(true);
                rightColorName.gameObject.SetActive(true);
                rightColorName.color = GetTextColor(cbtn.GetComponent<Image>().color);
            }
        }
    }
    private void DoScoreAni()
    {
        _GameEnd = false;
        _RGBRightCnt = 0;
        _CurrentLevel++;

        //分数增加
        SaveScore();
    }
    private void GoNextLevel()
    {
        //继续下一关
        InitMatrix(GetColorFrom(_CurrentColorGameMode), false, false);

        CancelInvoke("ShowDoTips");//先取消
        Invoke("ShowDoTips", 10f);//再定时
    }
    private void GameEnd(string info)
    {
        _GameEnd = true;
        //选择错误了，结束游戏

        OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam(info,
            MaskTips.eDialogType.OK_CANCEL_BTN,
            (MaskTips.eDialogBtnType type) =>
        {
            if (type == MaskTips.eDialogBtnType.OK)
            {
                //一些清理、保存工作
                _HasAnswered = false;
                _CurrentLevel = 0;
                _GameEnd = false;
                _RGBRightCnt = 0;

                InitMatrix(GetColorFrom(_CurrentColorGameMode), false);

                if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
                {
                    ShowToast("找色玩法在闯关时，可以浏览不同的诗句、色卡日记", 3f);
                }
                else if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor)
                {
                    ShowToast("单色玩法在闯关时，可以着重品读、记忆某句诗词", 3f);
                }
                else if (_CurrentColorGameMode == eColorGameMode.ModeMultiColor)
                {
                    ShowToast("多色玩法在闯关时，可以翻阅、回顾不同的色卡日记", 3f);
                }
                else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
                {
                    ShowToast("连色玩法在闯关时，可以通过颜色达成诗词关联记忆", 3f);
                }
                else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
                {
                    ShowToast("分色玩法将颜色拆分为相应的RGB(红/绿/蓝)分量");
                }
            }
            else if (type == MaskTips.eDialogBtnType.CANCEL)
            {
                if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
                {
                    ShowToast("分色玩法有助于了解颜色由红/绿/蓝三原色组成");
                    return;
                }
                else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
                {
                    ShowToast("观察比较诗句字数，更容易判断要选择的颜色哦");
                    return;
                }
                //只有内置颜色才有问题可以回答
                float r = UnityEngine.Random.value;
                if (CheckIsPickColor())
                {
                    if (r < 0.3f)
                    {
                        ShowToast("点击找色/单色/多色，开始新的闯关");
                    }
                    else if (r > 0.5f)
                    {
                        ShowToast("在取色器界面创建色卡，书写属于自己的颜色日记");
                    }
                    else
                    {
                        ShowToast("仅当答案为内置色时，才有机会回答问题继续闯关");
                    }
                }
                else
                {
                    if (_HasAnswered)
                    {
                        if (r < 0.2f)
                        {
                            ShowToast("诗词题目都很简单，善于利用诗词查询功能哟");
                        }
                        else if (r < 0.8f)
                        {
                            ShowToast("一局闯关只能使用一次答题，请重新开始游戏吧");
                        }
                        else
                        {
                            ShowToast("问答目的不在闯关难度，是想大家多学习点诗词啦",3f);
                        }
                    }
                    else
                    {
                        ShowToast("不想放弃？点击正确色块，回答问题，可继续闯关");
                    }
                }
            }
        }));
    }
    //初始化分割线，根据方阵大小变化
    public GameObject _MatrixRule;
    public GameObject _MatrixColor;
    public GameObject _MatrixHSP;
    public GameObject _MatrixVSP;
    public GameObject _Matrix;
    private void InitMatrixSP()
    {
        _MatrixColor.SetActive(true);
        _MatrixHSP.SetActive(true);
        _MatrixVSP.SetActive(true);
        _MatrixRule.SetActive(false);
        Image[] hsp = _MatrixHSP.GetComponentsInChildren<Image>(true);
        Image[] vsp = _MatrixVSP.GetComponentsInChildren<Image>(true);

        HorizontalLayoutGroup hl = _MatrixVSP.GetComponent<HorizontalLayoutGroup>();
        VerticalLayoutGroup vl = _MatrixHSP.GetComponent<VerticalLayoutGroup>();

        for (int i = 0; i < hsp.Length; i++)
        {
            hsp[i].gameObject.SetActive(false);
            vsp[i].gameObject.SetActive(false);
        }

        int level = GetGridLevel();
        for (int i = 0; i <= level; i++)
        {
            hsp[i].gameObject.SetActive(true);
            vsp[i].gameObject.SetActive(true);
        }

        float wh = GetGridLevelSizeWH();
        hl.spacing = wh;
        vl.spacing = wh;
    }
    private float MSPWH = 4;//行线的宽度，这里固定
    private float GetGridLevelSizeWH()
    {
        int level = GetGridLevel();

        RectTransform mrt = _Matrix.GetComponent<RectTransform>();
        VerticalLayoutGroup vl = _MatrixHSP.GetComponent<VerticalLayoutGroup>();

        float wh = (mrt.rect.width - 4/*vl纠正*/ - MSPWH * (level + 1)) / level;

        return wh;
    }

    private int GetGridLevel()
    {
        int level = GetFixedLevel() + 2;

        level = level > Define.MAX_XT_HZ_MATRIX ? Define.MAX_XT_HZ_MATRIX : level;

        return level;
    }

    private int GetFixedLevel()
    {
        int level = 0;
        if (_CurrentColorGameMode == eColorGameMode.ModeSingleColor
            || _CurrentColorGameMode == eColorGameMode.ModeMultiColor)
        {
            level = _CurrentLevel;
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeFindColor)
        {
            level = (int)Math.Log(_CurrentLevel + 1);//_CurrentLevel / 10;//找色模式格子增长不能太快
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            level = (int)Math.Log(_CurrentLevel + 1);
            if (level < 3) level = 3;//连色格子数最低5*5
        }
        else if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            level = (int)Math.Log(_CurrentLevel + 1);//_CurrentLevel / 10;//找色模式格子增长不能太快
        }
        return level;
    }

    [System.Serializable] public class OnShowDialogEvent : UnityEvent<Color, MaskTips.DialogParam> { }
    public OnShowDialogEvent OnShowDialog;
    //单色模式，仅从当前背景色或者内置颜色选一种，一直闯过到底，颜色不会变化
    public void OnSingleColorBtnClick()
    {
        //内置颜色一定存在，此处判断可省略
        StartGame(eColorGameMode.ModeSingleColor);
    }
    //多色模式，从色库中的颜色，随机选取作为某一关，一直到底，每关颜色变化
    public void OnMultiColorBtnClick()
    {
        if (_PickColor.GetPickColorCnt() == 0)
        {
            OnShowDialog.Invoke(_BG.color, MaskTips.GetDialogParam("当前色库为空，需要先创建色卡，才能继续多色玩法。是否前往创建色卡？",
                MaskTips.eDialogType.OK_CANCEL_BTN,
                (MaskTips.eDialogBtnType type) =>
                {
                    if (type == MaskTips.eDialogBtnType.OK)
                    {
                        OnOpenPickColor.Invoke(true);
                    }
                },"前往"));
            return;
        }

        StartGame(eColorGameMode.ModeMultiColor);

    }

    public void OnModeDecomposeColorBtnClick()
    {
        StartGame(eColorGameMode.ModeDecomposeColor);
    }

    //找出与指定颜色相同的颜色
    public void OnFindColorBtnClick()
    {
        //来自二者
        //原则是优先色库颜色，这里一半概率

        StartGame(eColorGameMode.ModeFindColor);
    }
    public void OnLinkColorBtnClick()
    {
        //来自二者
        //原则是优先色库颜色，这里一半概率

        StartGame(eColorGameMode.ModeLinkColor);
    }
    private eColorGameMode GetColorFrom(eColorGameMode mode)
    {
        eColorGameMode from = eColorGameMode.ModeSingleColor;

        if (mode == eColorGameMode.ModeSingleColor
            || mode == eColorGameMode.ModeMultiColor)
        {
            from = mode;
        }
        else if (mode == eColorGameMode.ModeFindColor
            || mode == eColorGameMode.ModeDecomposeColor)
        {
            int sysCnt = HZManager.GetInstance().GetColorCnt();
            if (UnityEngine.Random.value < 1.0f * sysCnt / (sysCnt + _PickColor.GetPickColorCnt()))
            {
                from = eColorGameMode.ModeSingleColor;
            }
            else
            {
                if (_PickColor.GetPickColorCnt() == 0)
                {
                    from = eColorGameMode.ModeSingleColor;
                }
                else
                {
                    from = eColorGameMode.ModeMultiColor;
                }
            }
        }
        else if (mode == eColorGameMode.ModeLinkColor)
        {
            from = eColorGameMode.ModeSingleColor;//连色模式只能从内置颜色获取
        }

        return from;
    }
    //获取一种颜色，仅用于需要切换颜色的时候
    private struct sColor
    {
        public int colorID;
        public int r;
        public int g;
        public int b;
        public string cname;
        public string cdes;//诗句或颜色描述
        public string py;

        //诗词测试使用
        public HZManager.eShiCi scType;
        public int scID;
    }

    private sColor _CurrentColor;
    public PickColor _PickColor;//色库

    //初始化颜色，根据当前的玩法模式
    public Text _BeginInfo;
    public Image _CurrentColorImg;
    public Text _CurrentColorName;
    public Text _CurrentColorDes;
    public Text _CurrentColorRGB;
    public Text _CurrentColorRGBTitle;
    public Image _CurrentColorSp;//分割线
    public GameObject _RGBDec;
    public Text _RText;
    public Text _GText;
    public Text _BText;
    private void InitColor(eColorGameMode type)
    {
        InitColorFrom(type);
        Color c = new Color(_CurrentColor.r / 255f, _CurrentColor.g / 255f, _CurrentColor.b / 255f);
        Color tc = GetTextColor();
        float a = tc.a;
        tc = new Color(tc.r,tc.g,tc.b,0f);
        _CurrentColorName.text = GetCurrentCName();
        _CurrentColorName.color = tc;
        _CurrentColorDes.text = "";
        _CurrentColorDes.color = tc;
        _CurrentColorRGB.text = _CurrentColor.r + "," + _CurrentColor.g + "," + _CurrentColor.b;
        _CurrentColorRGB.color = tc;
        _CurrentColorRGBTitle.color = tc;
        _CurrentColorSp.transform.localScale = Vector3.zero;

        _RGBDec.SetActive(_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor);

        _ColorCircleImg.color = new Color(1,1,1,0);
        UpdateColorCircle();
        //跟随绿色
        if (_CurrentColorGameMode == eColorGameMode.ModeDecomposeColor)
        {
            _RText.gameObject.SetActive(false);
            _GText.gameObject.SetActive(false);
            _BText.gameObject.SetActive(false);

            _RText.text = "R·红\n" + _CurrentColor.r;
            _GText.text = "G·绿\n" + _CurrentColor.g;
            _BText.text = "B·蓝\n" + _CurrentColor.b;

            _RText.color = Define.GetUIFontColorByBgColor(Define.GetUnityColor(_CurrentColor.r,0,0), Define.eFontAlphaType.FONT_ALPHA_128);
            _GText.color = Define.GetUIFontColorByBgColor(Define.GetUnityColor(0, _CurrentColor.g, 0), Define.eFontAlphaType.FONT_ALPHA_128);
            _BText.color = Define.GetUIFontColorByBgColor(Define.GetUnityColor(0, 0, _CurrentColor.b), Define.eFontAlphaType.FONT_ALPHA_128);
        }

        _CurrentColorDes.gameObject.SetActive(_CurrentColorGameMode != eColorGameMode.ModeDecomposeColor);

        Sequence initColorAni = DOTween.Sequence();
        initColorAni
            .Join(_CurrentColorImg.DOColor(c, 0.5f))
            .Join(_CurrentColorDes.DOText(_CurrentColor.cdes, 1.0f))
            .Join(_CurrentColorName.DOFade(a, 0.75f))
            .Join(_CurrentColorDes.DOFade(a, 0.5f))
            .Join(_CurrentColorRGB.DOFade(a, 0.75f))
            .Join(_ColorCircleImg.DOFade(1,0.75f))
            .Join(_CurrentColorRGBTitle.DOFade(a, 0.75f))
            .Join(_CurrentColorSp.transform.DOScale(1.0f,1.0f));
    }
    private Color GetTextColor()
    {
        return GetTextColor(Define.GetUnityColor(_CurrentColor.r, _CurrentColor.g, _CurrentColor.b));
    }
    private Color GetTextColor(Color bg)
    {
        Color tc = Define.GetUIFontColorByBgColor(bg, Define.eFontAlphaType.FONT_ALPHA_128);

        return tc;
    }
    private string GetCurrentCName()
    {
        string cname = "";
        for (int i = 0; i < _CurrentColor.cname.Length; i++)
        {
            if (i != _CurrentColor.cname.Length - 1)
            {
                cname += _CurrentColor.cname[i] + "\n";
            }
            else
            {
                cname += _CurrentColor.cname[i];
            }
        }
        return cname;
    }

    private void InitColorFrom(eColorGameMode from)
    {
        if (from == eColorGameMode.ModeSingleColor)
        {
            //单色模式只从内置颜色获取，且闯关不结束颜色保存不变
            List<string> cinfo = HZManager.GetInstance().GetColor(HZManager.eColorType.CN);
            _CurrentColor.colorID = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_ID]);
            _CurrentColor.r = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_R]);
            _CurrentColor.g = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_G]);
            _CurrentColor.b = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_B]);
            _CurrentColor.cname = cinfo[(int)HZManager.eColorColName.COLOR_NAME];
            _CurrentColor.py = cinfo[(int)HZManager.eColorColName.COLOR_PINYIN];

            //随机填充一句诗词
            do
            {
                HZManager.eShiCi scType = HZManager.eShiCi.ALL;
                scType = (HZManager.eShiCi)HZManager.GetInstance().GenerateRandomInt((int)HZManager.eShiCi.ALL + 1, (int)HZManager.eShiCi.END);
                List<string> tsscData = HZManager.GetInstance().GetTSSC(scType);
                List<string> tsscList = HZManager.GetInstance().GetFmtShiCi(tsscData[(int)HZManager.eTSSCColName.TSSC_NeiRong]);


                _CurrentColor.cdes = tsscList[HZManager.GetInstance().GenerateRandomInt(0, tsscList.Count)];// 随机一句诗词
                _CurrentColor.scType = scType;
                _CurrentColor.scID = int.Parse(tsscData[(int)HZManager.eTSSCColName.TSSC_ID]);

            } while (_CurrentColor.cdes.Length < 5 || _CurrentColor.cdes.Length > 16/*不超过16字*/);//随机到的诗句长度不能小于4，否则重新随机
        }
        else if (from == eColorGameMode.ModeMultiColor)
        {
            //来自色库
            List<string> cinfo = _PickColor.GetPickColor();
            if (cinfo.Count != 0)
            {
                _CurrentColor.colorID = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_ID]);
                _CurrentColor.r = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_R]);
                _CurrentColor.g = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_G]);
                _CurrentColor.b = int.Parse(cinfo[(int)HZManager.eColorColName.COLOR_B]);
                _CurrentColor.cname = cinfo[(int)HZManager.eColorColName.COLOR_NAME];
                _CurrentColor.py = cinfo[(int)HZManager.eColorColName.COLOR_PINYIN];
                _CurrentColor.cdes = cinfo[(int)HZManager.eColorColName.COLOR_DES];

                _CurrentColor.scType = HZManager.eShiCi.ALL;
                _CurrentColor.scID = -1;
            }
        }
    }

    //代码设置混合颜色
    public RawImage _ColorCircleImg;
    public void UpdateColorCircle()
    {
        Texture2D tex = new Texture2D(210,210);
        Color32[] ccs = tex.GetPixels32();
        for (int y = -105; y < 105; y++)
        {
            for (int x = -105; x < 105; x++)
            {
                bool inR = InR(x,y);
                bool inG = InG(x, y);
                bool inB = InB(x, y);

                Color32 c = new Color32((byte)_CurrentColor.r, (byte)_CurrentColor.g, (byte)_CurrentColor.b, 0);
                if (inR && inG && inB)
                {
                    c = new Color32((byte)_CurrentColor.r, (byte)_CurrentColor.g, (byte)_CurrentColor.b, 0);
                }
                else if (inR && inG)
                {
                    c = new Color32((byte)_CurrentColor.r, (byte)_CurrentColor.g, 0, 255);
                }
                else if (inR && inB)
                {
                    c = new Color32((byte)_CurrentColor.r, 0, (byte)_CurrentColor.b, 255);
                }
                else if (inG && inB)
                {
                    c = new Color32(0, (byte)_CurrentColor.g, (byte)_CurrentColor.b, 255);
                }
                else if (inR)
                {
                    c = new Color32((byte)_CurrentColor.r, 0,0, 255);
                }
                else if (inG)
                {
                    c = new Color32(0, (byte)_CurrentColor.g,0, 255);

                }
                else if (inB)
                {
                    c = new Color32(0,0, (byte)_CurrentColor.b, 255);
                }
                else
                {
                    //c = new Color32(0,0,0,0);
                    c = new Color32((byte)_CurrentColor.r, (byte)_CurrentColor.g, (byte)_CurrentColor.b, 0);
                }
                ccs[(y + 105) * 210 + (x + 105)] = c;
            }
        }

        tex.SetPixels32(ccs);
        tex.Apply();

        if (_ColorCircleImg.texture != null)
        {
            Destroy(_ColorCircleImg.texture);
            _ColorCircleImg.texture = null;
        }

        _ColorCircleImg.texture = tex;
    }
    private bool InR(int x,int y)
    {
        //红
        //x ^ 2 + (y - 35) ^ 2 = 4900;
        return x * x + (y - 35) * (y - 35) <= 4900;
    }
    private bool InG(int x, int y)
    {
        //绿
        //(x + 35) ^ 2 + (y + 35) ^ 2 = 4900;
        return (x + 35) * (x + 35) + (y + 35) * (y + 35) <= 4900;
    }
    private bool InB(int x, int y)
    {

        //蓝
        //(x - 35) ^ 2 + (y + 35) ^ 2 = 4900;
        return (x - 35) * (x - 35) + (y + 35) * (y + 35) <= 4900;
    }

    //查看答案按钮，此处和诗词的查看答案一样，次数限制也一样
    //也可以作为独立的查看答案按钮
    private bool _HasUsedCKDA = false;
    private bool _HasAnswered = false;
    public void OnCKDABtnClick()
    {
        if (_HasUsedCKDA)
        {
            ShowToast("这关已使用过查看答案，无需重复使用。");
            return;
        }

        if (!_CurrentColorImg.gameObject.activeSelf)
        {
            ShowToast("请先从右侧菜单中选择玩法开始游戏:)");
            return;
        }

        if (_GameEnd)
        {
            ShowToast("本次挑战已结束，请重新开始游戏:)");
            return;
        }

        if (DOTween.IsTweening("ResultAni"))
        {
            //此时不能使用答案
            return;
        }

        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);
        if (currentLeftShowDaanCnt > 0)
        {
            currentLeftShowDaanCnt--;
        }
        else
        {
            //答案次数已经用完了
            ShowToast("今天次数已用完，请明天再试(次日重置)");
            return;
        }

        Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, currentLeftShowDaanCnt);

        UpdateLeftDaanTime(currentLeftShowDaanCnt);

        CancelInvoke("ShowDoTips");//先取消
                                   //显示答案
        ShowColorResult();

        if (_CurrentColorGameMode == eColorGameMode.ModeLinkColor)
        {
            DoLinkColorResultAni(1.0f);
        }
        else
        {
            DoResultColorAni(true,1.0f);
        }

        ShowToast("正确色块已经标出，即将进入下一关。");


        _HasUsedCKDA = true;
    }

    //其他模式

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
                    img.color = new Color(img.color.r, img.color.g, img.color.b,0.5f);
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

        //需要最后调用，因为和上面的有重叠
        _SCGame.ChangeBGColor(color);
        UpdateLeftTimeTextColor();//更新特别的剩余次数颜色

        //需要特别更新箭头动画，如果正在执行的话，即正在显示
        if (DOTween.IsTweening("DoStartArrowAni"))
        {
            DoStartArrowAni(true);
        }

        //高亮分数行需要特殊处理
        if (_CurrentColorGameMode != eColorGameMode.NONE)
        {
            //
            Color dc = Define.GetLightColor(Define.GetFixColor(_BG.color));
            scoreTexts[((int)_CurrentColorGameMode - 1) * 3].color = dc;
            scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 1].color = dc;
            scoreTexts[((int)_CurrentColorGameMode - 1) * 3 + 2].color = dc;
        }
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
                if (txt.name.Contains("SelfSet")) continue;
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

    //-----------------------------------颜色生成算法----------------------------
    private int COLOR_LEVEL = 60;
    public Color GetColorByLevel(Color oriColor,int level2)
    {
        /**
         * 计算差值，分数越高，差值越小，越难分辨
         */

        float level = 1f * level2;
        float dx = (COLOR_LEVEL + (level + level) * (COLOR_LEVEL / (level + 2))) / (level + 1) + COLOR_LEVEL / (level + 1);
        Color destColor = oriColor;

        //随机明暗
        int label = UnityEngine.Random.value < 0.5f?1:-1;

        float r = Math.Abs(oriColor.r * 255f + label * dx);
        float g = Math.Abs(oriColor.g * 255f + label * dx);
        float b = Math.Abs(oriColor.b * 255f + label * dx);

        //if (r > 255) r = 2 * 255 - r;
        //if (g > 255) g = 2 * 255 - g;
        //if (b > 255) b = 2 * 255 - b;

        destColor.r = r / 255f;
        destColor.g = g / 255f;
        destColor.b = b / 255f;

        return destColor;
    }
    public float GetRGBByLevel(float oriColor, int level2)
    {
        /**
         * 计算差值，分数越高，差值越小，越难分辨
         */

        float level = 1f * level2;
        float dx = (COLOR_LEVEL + (level + level) * (COLOR_LEVEL / (level + 2))) / (level + 1) + COLOR_LEVEL / (level + 1);
        float destColor = oriColor;

        //随机明暗
        int label = UnityEngine.Random.value < 0.5f ? 1 : -1;

        if (oriColor * 255 < 20) label = 1;

        float rgb = oriColor * 255f + label * dx;

        //过暗无法辨别，只有增加，不减少
        if (rgb > 255)
        {
            rgb = oriColor * 255f - dx;
        }

        if (rgb < 0)
        {
            rgb = oriColor * 255f + dx;
        }

        destColor = rgb / 255f;

        return destColor;
    }
    //-------------------购买----------------------
    private bool _ProcessingPurchase = false;
    public Button _buyDaanBtn;
    public Text _leftDaanTime;//剩余查看答案次数
    public Button _showDaanBtn;

    public void UpdateLeftDaanTime(int n)
    {
        _leftDaanTime.text = "+" + n;

        UpdateLeftTimeTextColor();
    }

    private void UpdateLeftTimeTextColor()
    {
        if (_leftDaanTime.text == "+0")
        {
            _leftDaanTime.color = Define.GetFixColor(_BG.color * 0.9f);
        }
        else
        {
            _leftDaanTime.color = Define.GetFixColor(_BG.color * 1.1f);
        }
    }

    protected void CheckDaanBuyState()
    {
        if (IAP.getHasBuy(IAP.IAP_DAAN))
        {
            _buyDaanBtn.gameObject.SetActive(false);
        }
        else
        {
            _buyDaanBtn.gameObject.SetActive(true);
        }

        int currentLeftShowDaanCnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.SHOW_DAAN_CNT, 0);

        UpdateLeftDaanTime(currentLeftShowDaanCnt);
    }

    [System.Serializable] public class OnBuyEvent : UnityEvent<string> { }
    public OnBuyEvent OnBuy;
    public void OnBuyDaanBtnClick()
    {

        if (!_ProcessingPurchase)
        {
            _ProcessingPurchase = true;
            OnBuy.Invoke(IAP.IAP_DAAN);
            ShowToast("此查看答案功能和【挑战】里的统一，详细信息见挑战",3f);
        }
        else
        {
            ShowToast("购买正在处理进行中，请稍后...");
        }
    }

    public void OnBuyCallback(bool ret, string inAppID, string receipt)
    {
        if (inAppID != IAP.IAP_DAAN)
        {
            return;
        }

        _ProcessingPurchase = false;
        if (ret)
        {
            // 购买成功，根据inAppID，刷新界面
            if (!string.IsNullOrEmpty(receipt))
            {
                //说明是真正的购买，其他可能是正在购买或者已经购买过了
            }

            //如果lock不可见，不必执行动画
            if (_buyDaanBtn.gameObject.activeSelf)
            {
                DoUnLockAni(_buyDaanBtn.gameObject, () => {
                    _buyDaanBtn.gameObject.SetActive(false);
                });
            }

            OnReportEvent(ret, EventReport.BuyType.BuySuccess);
        }
        else
        {
            //简单提示，购买失败
            ShowToast("购买失败，请稍后再试：(");
            OnReportEvent(ret, EventReport.BuyType.BuyFail);
        }
    }

    public void OnRestoreCallback(bool ret, string inAppID)
    {
        if (inAppID != IAP.IAP_DAAN)
        {
            return;
        }

        _ProcessingPurchase = false;

        if (ret)
        {
            //如果lock不可见，不必执行动画
            if (_buyDaanBtn.gameObject.activeSelf)
            {
                DoUnLockAni(_buyDaanBtn.gameObject, () => {
                    _buyDaanBtn.gameObject.SetActive(false);
                });
            }
        }
        else
        {
            //MessageBoxManager.Show("", "恢复购买失败，请确认是否购买过？");
        }

        OnReportEvent(ret, EventReport.BuyType.BuyRestore);
    }

    private void DoUnLockAni(GameObject lockObj, Action cb = null)
    {
        Image[] ll = lockObj.GetComponentsInChildren<Image>(true);

        Image bg = ll[1];
        Image lockImg = ll[2];
        Image unlockImg = ll[3];
        foreach (var img in ll)
        {
            if (img.name == "BuyLockTagDark")
            {
                lockImg = img;
            }
            if (img.name == "BuyUnLockTagDark")
            {
                unlockImg = img;
            }
            if (img.name == "Bg")
            {
                bg = img;
            }
        }

        Sequence mySequence = DOTween.Sequence();
        unlockImg.gameObject.SetActive(true);
        mySequence
            .Append(unlockImg.DOFade(0.0f, 0.0f))
            .Append(unlockImg.transform.DOScale(1.5f, 0.0f))
            .Append(lockImg.DOFade(0.0f, 0.5f))
            .Join(lockImg.transform.DOScale(1.5f, 0.5f))
            .SetEase(Ease.InSine)
            .Append(unlockImg.DOFade(1.0f, 0.8f))
            .Join(unlockImg.transform.DOScale(1.0f, 0.8f))
            .Append(unlockImg.transform.DOShakeRotation(1.0f, 45.0f))
            .Append(unlockImg.DOFade(0.0f, 0.5f))
            .Join(bg.DOFade(0.0f, 1.0f))
            .SetEase(Ease.InSine)
            .OnComplete(() => {

                if (cb != null)
                {
                    cb();
                }
            });
    }
    [System.Serializable] public class OnEventReport : UnityEvent<string> { }
    public OnEventReport ReportEvent;
    public void OnReportEvent(bool success,
                            EventReport.BuyType buyType)
    {
        ReportEvent.Invoke(buyType + "_" + EventReport.EventType.DaanBtnBuyClick + "_" + success);
    }


    //---------------------诗句路径搜索-----------------------------------
    private int X = 7;
    private int Y = 7;
    private int N = 5;
    private Action<bool, int> SearchingCb = null;//参数：当前节点是否合法
    /// <summary>
    /// 自动搜索一定区域内合法的笔画/字根组装序列
    /// </summary>
    /// <param name="x">搜索起始点坐标x</param>
    /// <param name="y">搜索起始点坐标y</param>
    /// <param name="n">搜索半径</param>
    /// <param name="fininshCallback">搜索完成回调，向ui主线程返回搜索结果</param>
    /// <param name="searchingCallback">搜索中回调，如果搜索复杂度较大，可通过该回调给予提示</param>
    public void SearchSJPath(int x, int y, int n,
                        Action<List<int>> fininshCallback,
                        Action<bool, int> searchingCallback = null)
    {
        //初始化参数
        X = x;
        Y = y;
        N = n;

        SearchingCb = searchingCallback;

        List<int> xy = new List<int>();
        for (int i = 0; i < N * N; i++)
        {
            xy.Add(0);//全部设置为未搜索
        }

        //将起始字点设置为已经搜索
        xy[Y * N + X] = 1;
        List<int> searched = new List<int>
        {
            Y * N + X
        };

        //子线程中进行搜索，不会阻塞ui
        Loom.RunAsync(() =>
        {
            Thread thread = new Thread(() =>
            {
                if (!DoSearch(xy, null, searched, X, Y))
                {
                    //找不到合法序列，清空searched列表
                    searched.Clear();
                }

                Loom.QueueOnMainThread((param) =>
                {
                    if (fininshCallback != null)
                    {
                        fininshCallback(searched);//返回合法的索引id，即对应到界面节点
                    }
                }, null);
            });

            thread.Start();
        });
    }

    struct SD
    {
        public int index;
        public int priority;
    }

    private List<int> GetNextSearch(List<int> xy, int x, int y, List<int> path)
    {
        List<int> searching = new List<int>();
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= N) continue;
            if (i <= X - N || i >= X + N) continue;

            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= N) continue;// 超出边界
                if (j <= Y - N || j >= Y + N) continue;//超出搜索半径

                if (i == x && j == y) continue;//同一个点

                if (xy[j * N + i] == 0)
                {
                    searching.Add(j * N + i);
                }
            }
        }

        //可搜索的点少于2个，直接返回该点
        if (searching.Count <= 1)
        {
            return searching;
        }

        //洗牌搜索序列，防止固定的方向
        ShuffleSJList(searching);

        //优选搜索周围空白的字最多的字
        List<SD> sds = new List<SD>();
        for (int i = 0; i < searching.Count; i++)
        {
            SD sd;
            sd.index = searching[i];
            sd.priority = GetHZPriority(searching[i], xy, path);
            sds.Add(sd);
        }

        sds.Sort((a, b) => {
            return a.priority - b.priority;//升排列，也就是周围字最多，最难的路线
        });

        searching.Clear();
        foreach (var sd in sds)
        {
            searching.Add(sd.index);
        }


        int sindex = (int)((1 - 0.25f) * searching.Count);//0=< 1-_SearchDifficult <1
        if (sindex >= searching.Count - 1) sindex = searching.Count - 1;

        List<int> searching1 = new List<int>();
        List<int> searching2 = new List<int>();

        for (int i = 0; i < sindex; i++)
        {
            searching1.Add(searching[i]);
        }

        for (int i = sindex; i < searching.Count; i++)
        {
            searching2.Add(searching[i]);
        }

        searching.Clear();
        searching.AddRange(searching2);// 优先往简单的方向搜索
        searching1.Reverse();
        searching.AddRange(searching1);//再往越来越难的方向搜索

        return searching;
    }

    // 获取文字的搜索权重
    private int GetHZPriority(int index, List<int> xy, List<int> path)
    {
        int p = 0;
        int p1 = 0;
        int p2 = 0;

        int x = index % N;
        int y = index / N;


        //使用距离加权难度系数
        for (int i = 0; i < path.Count; i++)
        {
            int xx = path[i] % N;
            int yy = path[i] / N;
            p1 += (int)Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
        }

        //使用文字密集成度加权难度系数
        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= N) continue;
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= N) continue;// 超出边界

                if (i == x && j == y) continue;//同一个点

                if (xy[j * N + i] == 0)
                {
                    p2++;
                }
            }
        }


        //100关以内，适当降低p2的比重，否则会出现往中间集中，难度过高
        //达到插入干扰字以后，搜索权重的比例，按照干扰字的增多降低，直到为0
        //这个控制是为当需要插入字时，诗句更分散，难度更高
        //否则当搜索难度最高时，诗句全部靠在一起，插入干扰字失去意义
        p2 = (int)(p2 * (0.5f + 0.25f * 0.5f));//0.625-1.0

        // 需要找到更加科学的 两个参数的 比例或者公式，这里是简单相加
        p = p1 + p2;

        return p;
    }

    //文字密集成度
    private int GetHZPriority(string[] xy, int index)
    {
        int p = 0;

        int x = index % N;
        int y = index / N;

        for (int i = x - 1; i <= x + 1; i++)
        {
            if (i < 0 || i >= N) continue;
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (j < 0 || j >= N) continue;// 超出边界

                if (i == x && j == y) continue;//同一个点

                ////这里如果有字则增加权重，而不是空
                /// 这样可以保证插入字干扰性更大，否则在边界时误差太大
                if (xy[j * N + i] != "")
                {
                    p++;
                }
            }
        }

        return p;
    }
    private bool DoSearch(List<int> xy, List<int> searching, List<int> searched, int x, int y)
    {
        //根节点
        if (searching == null)
        {
            return DoSearch(xy, GetNextSearch(xy, x, y, searched), searched, x, y);
        }

        //检测是否搜索完成
        if (searched.Count == _CurrentColor.cdes.Length)
        {
            return true;
        }

        //没有可以搜索的，终止//已经到达死角或者终结
        if (searching.Count == 0 || searched.Count >= N * N /*范围内全部节点*/)
        {
            return false;
        }

        //对当前的进行检测
        foreach (var sc in searching)
        {
            int xx = sc % N;
            int yy = sc / N;

            searched.Add(sc);
            xy[sc] = 1;

            //这里做一些视觉效果
            Loom.QueueOnMainThread((param) =>
            {
                if (SearchingCb != null)
                {

                    SearchingCb(true, sc);
                }
            }, null);

            //System.Threading.Thread.Sleep(1000);

            if (DoSearch(xy, GetNextSearch(xy, xx, yy, searched), searched, xx, yy))
            {
                return true;
            }
            else
            {
                //这个节点不合法，后退到前一个
                xy[sc] = 0;
                searched.RemoveAt(searched.Count - 1);

                Loom.QueueOnMainThread((param) =>
                {
                    if (SearchingCb != null)
                    {
                        SearchingCb(false, sc);
                    }
                }, null);
            }
        }

        return false;
    }

    public void ShuffleSJList(List<int> myList)
    {
        int index = 0;
        int temp;
        for (int i = 0; i < myList.Count; i++)
        {

            index = HZManager.GetInstance().GenerateRandomInt(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
    }
}
