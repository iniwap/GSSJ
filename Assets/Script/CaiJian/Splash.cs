using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;
using PanGu;

public class Splash : MonoBehaviour
{
    public void Start()
    {
        //test
        //Setting.delAllPlayerPrefs();
        Init();
    }

    public void LoadData()
    {
        //加载资源
        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.TANGSHI, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("唐诗数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SONGCI, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("宋词数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.GUSHI, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("古诗数据库加载完毕");
        });

        HZManager.GetInstance().LoadRes(HZManager.eLoadResType.SHIJING, (HZManager.eLoadResType type) =>
        {
            //Debug.Log("诗经数据库加载完毕");
        });

        //初始化分词
        Segment.Init();
    }

    public UnityEvent OnInit;

    public GameObject _leanTouch;
    public GameObject _LinePrefab;
    public GameObject _TextPrefab;
    public Transform _TextArea;
    public Image _Sp;
    public Image _Border;
    public Text _AppName;
    public Image _bg;

    private List<GameObject> _LineList = new List<GameObject>();
    private List<GameObject> _TextList = new List<GameObject>();


    public void Init(){
        _leanTouch.SetActive(false);

        if (!Setting.GetShowSplash())
        {
            Sequence mySequence = DOTween.Sequence();
            mySequence
                .Join(_Sp.DOFade(1.0f, 0.5f))
                .Join(_Border.DOFade(200 / 255.0f,0.5f))
                .Join(_AppName.DOFade(200 / 255.0f, 0.5f))
                .AppendCallback(LoadData)
                .OnComplete(OnInit.Invoke);
        }
        else
        {

            //启动屏幕有固定色
            List<string> tsscList = new List<string> { };

            List<string> tsscList0 = new List<string>{"信是有色的诗#||风说", "你的来信#便是斑斓的诗#||等",
            "写给他/她的信#||略略",
            "写一封信#寄往一个驿站#||一个人",
        "欲书又止#||某小某",
        "彩色的信笺#欢喜的颜色#||旧念"};

            int index = 0;
            if (Setting.getPlayerPrefs("" + Setting.SETTING_KEY.START_CNT, 0) == 0)
            {
                index = 0;

                //只默认显示一次，如果有需要用户才去设置里开启
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.SHOW_SPLASH, 0);
            }
            else
            {
                index = HZManager.GetInstance().GenerateRandomInt(0, tsscList0.Count);
            }

            //index = 0;

            string[] sp = tsscList0[index].Split('#');
            foreach (var s in sp)
            {
                tsscList.Add(s);
            }

            //正文
            for (int i = 0; i < tsscList.Count - 1; i++)
            {

                InitNewLine(_LinePrefab, _TextPrefab, _TextArea, tsscList[i], 64);
            }

            //作者
            InitNewLine(_LinePrefab, _TextPrefab, _TextArea, tsscList[tsscList.Count - 1], 48);

            //适配屏幕
            FitScreen(_TextArea, tsscList);

            //执行动画
            DoHZAnimation();
        }
    }


    private void InitNewLine(GameObject linePrefab,
                             GameObject textPrefab,
                             Transform textArea,
                            string shiju,
                            int fontSizeSJ)
    {
        GameObject line = Instantiate(linePrefab, textArea) as GameObject;
        line.SetActive(true);

        _LineList.Add(line);

        GridLayoutGroup ly = line.GetComponentInChildren<GridLayoutGroup>();

        Setting.AlignmentType alignmentType =  Setting.AlignmentType.RIGHT_VERTICAL;

        //选择对齐方式
        if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL
            || alignmentType == Setting.AlignmentType.LEFT_HORIZONTAL)
        {
            ly.childAlignment = TextAnchor.UpperLeft;
            ly.startCorner = GridLayoutGroup.Corner.UpperLeft;
        }
        else
        {
            ly.childAlignment = TextAnchor.UpperRight;
            ly.startCorner = GridLayoutGroup.Corner.UpperRight;
        }

        CJHZ.HZParam p;
        p.zsImgPath = "";
        p.zsImgColor = Define.BG_COLOR_50;
        p.zsImgSize = 1.0f;
        p.zsHZColor = Color.black;

        p.hsImgPath = "";
        p.hsImgSize = Define.DEFAULT_HS_SIZE;
        p.hsImgColor = Define.LIGHTBG_SP_COLOR;

        Setting.SpeedLevel speedLevel = GetSpeedLevel();
        foreach (var hz in shiju)
        {
            GameObject txt = Instantiate(textPrefab, ly.transform) as GameObject;
            txt.SetActive(true);
            CJHZ sj = txt.GetComponent<CJHZ>();

            sj.Init(_TextList.Count, false, false,true,
                    Setting.AlignmentType.RIGHT_VERTICAL,
                    speedLevel,p);
            sj.SetHZText("" + hz, fontSizeSJ,true);
            ZhuangShiBtnItem.ZSParam item;
            item.btnType = ZhuangShiBtnItem.eZSBtnType.ZiShiShape;
            item.path = "";
            item.color = Define.BG_COLOR_50;//此处应为调节后的参数
            item.size = 1.0f;
            _TextList.Add(txt);
        }
    }

    private void DoHZAnimation()
    {
        Setting.SpeedLevel speedLevel = GetSpeedLevel();

        for (int i = 0; i < _TextList.Count; i++)
        {
            int index = i;

            CJHZ cjhz = _TextList[i].GetComponent<CJHZ>();

            cjhz.DoHZAnimation(false, () => {
                if (index == _TextList.Count - 1)
                {
                    if ((int)speedLevel >= (int)Setting.SpeedLevel.SPEED_LEVEL_6)
                    {
                        Invoke("DoSwipe", 1.0f);
                    }
                    else{
                        //DoSwipe();
                        Invoke("DoSwipe", 0.5f);
                    }
                }
            });
        }
    }


    public Image _sendXJ;
    private void DoSwipe()
    {
        Sequence mySequence = DOTween.Sequence();

        mySequence
            .AppendCallback(() => _sendXJ.gameObject.SetActive(true))
            .Append(_TextArea.transform.DOScale(0.0f, 0.5f))
            .Join(_TextArea.transform.DOLocalRotate(new Vector3(45.0f,45.0f,0.0f),0.5f))
            .Join(_sendXJ.DOFade(1.0f,0.5f));

        for (int i = 0; i < 2; i++)
        {
            CJHZ[] cjhz = _LineList[i].GetComponentsInChildren<CJHZ>();

            for (int ts = 0; ts < cjhz.Length; ts++)
            {
                mySequence
                    .Join(cjhz[ts].SP.DOFade(0.0f, 0.5f))
                    .Join(cjhz[ts].HZ.DOFade(0.0f, 0.5f));
            }
        }

        mySequence
            .Append(_sendXJ.transform.DOShakeScale(0.2f,0.5f))
            .Append(_sendXJ.DOFade(0.2f,1.6f))
            .Append(_sendXJ.DOFade(0.0f, 0.4f))
            .Join(_Sp.DOFade(1.0f, 1.0f))
            .Join(_Border.DOFade(200 / 255.0f, 1.0f))
            .Join(_AppName.DOFade(200 / 255.0f, 1.0f))
            .OnComplete(() =>
             {
                 DestroyObj(_TextList);
                 DestroyObj(_LineList);

                 LoadData();//首先加载诗词数据，这里同步阻塞，不要异步
                 OnInit.Invoke();
             });
        return;
    }

    private void FitScreen(Transform textArea, List<string> tsscList)
    {
        Setting.AlignmentType alignmentType = Setting.AlignmentType.RIGHT_VERTICAL;

        //单行大小
        Vector2 cellLine = textArea.GetComponentInChildren<GridLayoutGroup>().cellSize;
        Vector2 cellLineSpacing = textArea.GetComponentInChildren<GridLayoutGroup>().spacing;
        //设置锚点
        RectTransform rt = textArea.GetComponentInChildren<RectTransform>();
        switch (alignmentType)
        {
            case Setting.AlignmentType.LEFT_HORIZONTAL:
                rt.pivot = new Vector2(0.0f, 1.0f);
                break;
            case Setting.AlignmentType.LEFT_VERTICAL:
                rt.pivot = new Vector2(0.0f, 1.0f);
                break;
            case Setting.AlignmentType.RIGHT_HORIZONTAL:
                rt.pivot = new Vector2(1.0f, 1.0f);
                break;
            case Setting.AlignmentType.RIGHT_VERTICAL:
                rt.pivot = new Vector2(1.0f, 1.0f);
                break;
        }

        int maxLen = 0;
        for (int i = 0; i < tsscList.Count; i++)
        {
            if (maxLen < tsscList[i].Length)
            {
                maxLen = tsscList[i].Length;
            }
        }

        float maxX = 0;
        float maxY = 0;

        if (alignmentType == Setting.AlignmentType.LEFT_VERTICAL
            || alignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
        {
            maxY = maxLen * cellLine.x;//h
            maxX = tsscList.Count * cellLine.x + (tsscList.Count - 1) * cellLineSpacing.x;//w
        }
        else
        {
            maxX = maxLen * cellLine.y;//h
            maxY = tsscList.Count * cellLine.y + (tsscList.Count - 1) * cellLineSpacing.y;//w
        }

        float x = rt.rect.width / maxX;
        float y = rt.rect.height / maxY;

        float s = 1.0f;
        if (x < 1.0f && y < 1.0f)
        {
            s = x < y ? x : y;
        }
        else if (x >= 1.0f && y < 1.0f)
        {
            s = y;
        }
        else if (x < 1.0f && y >= 1.0f)
        {
            s = x;
        }

        textArea.localScale = new Vector3(s, s, 1);
    }


    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    //结束显示启动场景
    public void OnExitSplash(Color c)
    {
        float t = 0.5f;

        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(_Sp.DOFade(0.0f, t))
            .Join(_Border.DOFade(0.0f,t))
            .Join(_AppName.DOFade(0.0f,t))

            .Join(_bg.DOColor(c, t))
            .SetEase(Ease.InSine)
            .OnComplete(()=>{
                _leanTouch.SetActive(true);

                this.gameObject.SetActive(false);
        });
    }

    private Setting.SpeedLevel GetSpeedLevel(bool fix = true){

        //由于不同速度存在一定偏差，固定采用speed4
        if (fix)
        {
            return Setting.SpeedLevel.SPEED_LEVEL_4;
        }
        else
        {
            int cnt = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.START_CNT, 0);
            Setting.SpeedLevel speedLevel = Setting.SpeedLevel.SPEED_LEVEL_3;

            if (cnt + (int)(Setting.SpeedLevel.SPEED_LEVEL_3) >= (int)Setting.SpeedLevel.SPEED_LEVEL_6)
            {
                speedLevel = Setting.SpeedLevel.SPEED_LEVEL_6;
            }
            else
            {
                speedLevel = (Setting.SpeedLevel)(cnt + (int)(Setting.SpeedLevel.SPEED_LEVEL_3));
            }

            return speedLevel;
        }
    }
}
