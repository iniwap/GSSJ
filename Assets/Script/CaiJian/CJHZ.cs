using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CJHZ : MonoBehaviour
{
    public void Start()
    {

    }

    //汉字参数列表
    public struct HZParam
    {
        public Color zsImgColor;
        public string zsImgPath;
        public float zsImgSize;//这个是缩放

        public Color zsHZColor;
        public string hsImgPath;
        public Color hsImgColor;
        public float hsImgSize;//这个是宽度
    }

    private HZParam _CurrentHZParam;
    public Image ZhuangShiImg;//
    public Image SP;//分割线
    public Text HZ;//汉字

    public int ID { get; set; }
    public bool Animating { get; set; } // 是否正在执行动画
    public bool IsVisible { get; set; } //
    public bool ShowLineSeparator { get; set; }
    public Setting.AlignmentType AlignmentType { get; set; }
    public Setting.SpeedLevel SpeedLevel { get; set; }

    public void InitFXHZ(HZParam param)
    {
        string prevZSImgPath = _CurrentHZParam.zsImgPath;
        _CurrentHZParam = param;

        HZ.color = _CurrentHZParam.zsHZColor;
        if (_CurrentHZParam.zsImgPath == "")
        {
            ZhuangShiImg.gameObject.SetActive(false);
        }
        else
        {
            ZhuangShiImg.gameObject.SetActive(true);
            if (prevZSImgPath != param.zsImgPath)
            {
                ZhuangShiImg.sprite = Resources.Load(_CurrentHZParam.zsImgPath, typeof(Sprite)) as Sprite;
            }

            ZhuangShiImg.color = _CurrentHZParam.zsImgColor;
            ZhuangShiImg.transform.localScale = new Vector3(_CurrentHZParam.zsImgSize, _CurrentHZParam.zsImgSize, 1f);
        }
    }

    public void Init(int id,
                     bool animating,
                     bool isVisible,
                     bool showLineSeparator,
                     Setting.AlignmentType alignmentType,
                     Setting.SpeedLevel speedLevel,
                     HZParam param
                    )
    {
        ID = id;
        Animating = animating;
        AlignmentType = alignmentType;
        IsVisible = isVisible;

        ShowLineSeparator = showLineSeparator;
        SpeedLevel = speedLevel;


        _CurrentHZParam = param;

        if (_CurrentHZParam.zsImgPath == "")
        {
            ZhuangShiImg.gameObject.SetActive(false);
        }
        else
        {
            ZhuangShiImg.gameObject.SetActive(true);
            ZhuangShiImg.sprite = Resources.Load(param.zsImgPath, typeof(Sprite)) as Sprite;
            ZhuangShiImg.color = new Color(param.zsImgColor.r, param.zsImgColor.g, param.zsImgColor.b, 0.0f);
            ZhuangShiImg.transform.localScale = Vector3.zero;
        }


        float a = 0;
        if (SpeedLevel != Setting.SpeedLevel.SPEED_LEVEL_8)
        {
            a = 0.0f;
        }
        else
        {
            a = _CurrentHZParam.zsHZColor.a;
        }

        HZ.color = new Color( _CurrentHZParam.zsHZColor.r, _CurrentHZParam.zsHZColor.g, _CurrentHZParam.zsHZColor.b,a);

        SP.gameObject.SetActive(ShowLineSeparator);
        SP.color = _CurrentHZParam.hsImgColor;
        RectTransform rt = SP.GetComponent<RectTransform>();
        //行线宽窄
        if (AlignmentType == Setting.AlignmentType.LEFT_VERTICAL
            || AlignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
        {
            rt.sizeDelta = new Vector2(_CurrentHZParam.hsImgSize,rt.sizeDelta.y);
        }
        else
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, _CurrentHZParam.hsImgSize);
        }

        if (SpeedLevel != Setting.SpeedLevel.SPEED_LEVEL_8)
        {
            //设置sp位置到不可见
            if (AlignmentType == Setting.AlignmentType.LEFT_VERTICAL
                || AlignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
            {

                SP.transform.localPosition = new Vector3(SP.transform.localPosition.x,
                                                         SP.transform.localPosition.y + SP.rectTransform.sizeDelta.y,
                                                         SP.transform.localPosition.z);

            }
            else
            {
                if (AlignmentType == Setting.AlignmentType.LEFT_HORIZONTAL)
                {
                    SP.transform.localPosition = new Vector3(SP.transform.localPosition.x - SP.rectTransform.sizeDelta.x,
                                     SP.transform.localPosition.y,
                                     SP.transform.localPosition.z);

                }
                else
                {
                    SP.transform.localPosition = new Vector3(SP.transform.localPosition.x + SP.rectTransform.sizeDelta.x,
                    SP.transform.localPosition.y,
                    SP.transform.localPosition.z);

                }
            }
        }
    }

    public void SetHZText(string hz,int fontSizeSJ,bool bold)
    {
        HZ.text = "" + hz;
        HZ.fontSize = fontSizeSJ;
        SetHZFonStyle(bold);
    }

    public void SetHZText(string hz,bool bold)
    {
        HZ.text = "" + hz;

        SetHZFonStyle(bold);
    }

    public void SetHZFonStyle(bool bold)
    {
        if (bold)
        {
            HZ.fontStyle = FontStyle.Bold;
        }
        else
        {
            HZ.fontStyle = FontStyle.Normal;
        }
    }

    public void DoDirectShow(){
        if (_CurrentHZParam.zsImgPath != "")
        {
            ZhuangShiImg.color = _CurrentHZParam.zsImgColor;
            ZhuangShiImg.transform.localScale = new Vector3(_CurrentHZParam.zsImgSize, _CurrentHZParam.zsImgSize, 1.0f);
        }
    }

    public void DoHZAnimation(bool useHZAni,Action complete = null)
    {
        float speed = (SpeedLevel == Setting.SpeedLevel.SPEED_LEVEL_8 ? 0.0f : 2.0f / (Mathf.Pow(2, (int)SpeedLevel)));

        Sequence mySequence = DOTween.Sequence();

        Tween t = null;
        if (AlignmentType == Setting.AlignmentType.LEFT_VERTICAL
           || AlignmentType == Setting.AlignmentType.RIGHT_VERTICAL)
        {
            t = SP.transform.DOLocalMoveY(SP.transform.localPosition.y - SP.rectTransform.sizeDelta.y, speed);
        }
        else
        {
            if (AlignmentType == Setting.AlignmentType.LEFT_HORIZONTAL)
            {
                t = SP.transform.DOLocalMoveX(SP.transform.localPosition.x + SP.rectTransform.sizeDelta.x, speed);
            }
            else
            {
                t = SP.transform.DOLocalMoveX(SP.transform.localPosition.x - SP.rectTransform.sizeDelta.x, speed);
            }
        }

        float delay = speed * ID;

        if (ID == 0)
        {
            delay = 0.1f;
        }


        Animating = true;
        IsVisible = false;//应该在开始fade后就可见了，而不是结束

        mySequence
            .AppendInterval(delay)
            .Append(t)
            .Join(HZ.DOFade(_CurrentHZParam.zsHZColor.a, speed));

        if (useHZAni)
        {
            mySequence.Join(HZ.transform.DOShakeScale(speed, HZManager.GetInstance().GenerateRandomInt(0, 10) / 10.0f));
        }

        if(_CurrentHZParam.zsImgPath != "")
        {
            Sequence zsSequence = DOTween.Sequence();
            zsSequence
                .Join(ZhuangShiImg.DOFade(_CurrentHZParam.zsImgColor.a, speed))
                .Join(ZhuangShiImg.transform.DOScale(_CurrentHZParam.zsImgSize, speed))
                .SetEase(Ease.OutBounce);

            mySequence.Join(zsSequence);
        }

        mySequence
            .OnComplete(() =>
            {
                IsVisible = true;
                Animating = false;
                if (complete != null){
                    complete();
                }
            });

    } 

}
