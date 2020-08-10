using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FXLineRender : MonoBehaviour
{
    //LineRenderer
    private LineRenderer _LineRenderer;
    public Camera _FXCamera;
    public GameObject _HZPrefabs;
    public Transform _HZConten;

    [Range(1f/32, 1/2f)]
    public float FX_RES = 0.25f;//双倍宽度的1/2，默认0.25
    private int _Blank = 100;
    private float _CurrentPeriod = 1.0f;//周期的比例值
    private float _CurrentAY = 1.0f;//y最大值比例
    private CJHZ.HZParam _CurrentHZParam;
    private float _ShowTime = 0f;
    private Action _FXAniFinishCallback = null;
    private string _CurrentDrawSJ = "";
    private int _CurrentDrawWidth;
    private int _CurrentDrawHeight;

    public enum LineType
    {
        NONE,//直线

        Circel,//圆,
        TuoYuan,//椭圆
        Paowu,//抛物线
        Duishu,// 对数
        Sin,//正弦
        Tan,//正切
        Zhengtai,//正态分布
       
        Xin,//苹果
        Xin2,//心形
        Xin_dian_tu2,// 心电图
        Tan_huang,//弹簧
        Ci_sheng_bo,//狮吼功

        Ye_xing,//刀光
        Wan_yue,//月牙
        Shi_zi,//街口
    
        Jian_kai2,//旋风
        Xing_xing,//四方
        Gou_zi,//山路
        Wu_jiao_xing,//五星
        Palm,//手


        Huan_xing_luo_xuan,//海生物
        San_ye_xian,//三叶草
        San_jian_ban_xian,//暗器
        A_ji_mi_de_luo_xuan,//金钩
        Yi_feng_san_zhu_dian_xian,//深渊
        Ba_zi_xian,//篱笆墙
        Ba_zi_xian2,//葫芦
        Wen_xiang,//蚊香
        Zi_xing_xian,//花生
        Mei_hua_xian,//梅花
        Mei_hua_xian2,//水仙
        Mei_hua_xian3,//阡陌
        Mei_hua_xian4,//蜻蜓
        //Mei_hua_xian5,
        Hu_Die,//梳子
        Hu_Die2,//中国结
        Shuang_hu_wai_bai_xian,//福娃
        //Wai_bai_xian,
        TangGuo,//糖果
        Zan_xing_xian2,//隧道
        Zan_xing_xian3,//山脉

        Die_xing_tan_huang,//涟漪
        Jian_kai,//章鱼
        Yan_jian_wei,// 燕剪尾
        Xin_dian_tu,//紧箍咒
        Tu_zi,//兔子
        She_xing_xian,//青烟
        Zan_xing_xian,//发簪
        Shi_zi_jian_kai,//刺绣
        Wo_gui_xian,//丝线
        Hua_hui,//蝴蝶
        Fei_ji,// 飞机

        END,
    }

    public enum FXAddtionalType
    {
         FX_NONE = LineType.END + 1,
         FX_Xin2_1,
         FX_Xin2_2,
    }

    //如果相邻的两个字过于靠近，则进行一定的偏移，只处理相邻，隔开的不处理
    private void FixPos(GameObject hz)
    {
        Vector3 pp = _HZList[_HZList.Count - 2].transform.position;
        if ((hz.transform.position.x - pp.x) * (hz.transform.position.x - pp.x)
            + (hz.transform.position.y - pp.y) * (hz.transform.position.y - pp.y) < _Blank * _Blank /16)
        {
            int pos = _Blank / 4;           
            hz.transform.localRotation = Quaternion.Euler(0, 0, 45);
            float newX = hz.transform.position.x;
            float newY = hz.transform.position.y;
            if (hz.transform.position.x < Screen.width / 2)
            {
                newX = hz.transform.position.x + pos;
            }
            else
            {
                newX = hz.transform.position.x - pos;
            }

            if (hz.transform.position.y < Screen.height / 2)
            {
                newY = hz.transform.position.y + pos;
            }
            else
            {
                newY = hz.transform.position.y - pos;
            }

            hz.transform.position = new Vector3(newX,newY, hz.transform.position.z);
        }
    }


    private float GetFitScreenScale()
    {
        float ret = 1f;

        if (FitUI.getIsIPhoneX())
        {
            ret = 1.16f;
        }
        else if (FitUI.GetIsBigPad())
        {
            ret = 0.9f;
        }
        else if (FitUI.GetIsSmallPad())
        {
            ret = 1.2f;
        }

        return ret;
    }

    private float GetFitIPadScreenScale()
    {
        float ret = 1f;

        if (FitUI.GetIsBigPad())
        {
            ret = 0.8f;
        }
        else if (FitUI.GetIsSmallPad())
        {
            ret = 0.8f;
        }

        return ret;
    }

    public GameObject _FX;
    void Start()
    {

    }

    //public Material _LineMaterial;
    public void InitLineRender()
    {
        _LineRenderer = gameObject.GetComponent<LineRenderer>();
        if (_LineRenderer != null)
        {
            //Destroy(_LineRenderer);
            //_LineRenderer = null;
            _LineRenderer.positionCount = 0;
            return;
        }

        //添加LineRenderer组件
        _LineRenderer = gameObject.AddComponent<LineRenderer>();
        //设置材质
        _LineRenderer.material = new Material(Shader.Find("UI/Default"));// _LineMaterial;
        //设置颜色
        _LineRenderer.startColor = new Color(125 / 255f, 18 / 255f, 18 / 255f);
        _LineRenderer.endColor = new Color(125 / 255f, 18 / 255f, 18 / 255f);
        //设置宽度
        _LineRenderer.startWidth = 0.05f;
        _LineRenderer.endWidth = 0.05f;
    }

   
    public void DestroyObj(List<GameObject> objs)
    {
        for (int i = objs.Count - 1; i >= 0; i--)
        {
            Destroy(objs[i]);
        }
        objs.Clear();
    }

    private int MIN_HZ_SIZE = 16;//->8
    private int MAX_HZ_SIZE = 80;
    private float FIRST_HZ_SCALE = 1.5f;
    private float _HZSize = 40f;
    private bool _UseHZAni = false;
    public void ChangeHZSize(float s)
    {
        //[8,80]->[-1,1]*80
        for (int i = 0; i < _HZList.Count; i++)
        {
            RectTransform rt = _HZList[i].GetComponent<RectTransform>();
            if (i == 0)
            {
                _HZSize = (rt.sizeDelta.x + s * MAX_HZ_SIZE * FIRST_HZ_SCALE);
                if (_HZSize > MAX_HZ_SIZE * FIRST_HZ_SCALE) _HZSize = MAX_HZ_SIZE * FIRST_HZ_SCALE;
                if (_HZSize < MIN_HZ_SIZE * FIRST_HZ_SCALE) _HZSize = MIN_HZ_SIZE * FIRST_HZ_SCALE;

                rt.sizeDelta = new Vector2(_HZSize, _HZSize);
            }
            else
            {
                _HZSize = rt.sizeDelta.x + s * MAX_HZ_SIZE;
                if (_HZSize > MAX_HZ_SIZE) _HZSize = MAX_HZ_SIZE;
                if (_HZSize < MIN_HZ_SIZE) _HZSize = MIN_HZ_SIZE;

                rt.sizeDelta = new Vector2(_HZSize, _HZSize);
            }
        }

        SetHZSizeRange();
    }
    public void ChangeLineAlpha(float s)
    {
        float a = _CurrentHZParam.hsImgColor.a + s;
        if (a > 1f) a = 1f;
        if (a < 0f) a = 0f;

        _CurrentHZParam.hsImgColor = new Color(_CurrentHZParam.hsImgColor.r,
            _CurrentHZParam.hsImgColor.g,
            _CurrentHZParam.hsImgColor.b,a);

        SetLineAlphaRange();
    }

    public float GetLineAlpha()
    {
        return _CurrentHZParam.hsImgColor.a;
    }

    //改变周期
    public void ChangeLinePeriod(float s)
    {
        float a = _CurrentPeriod + s;
        if (a > 1f)
        {
            return;
        }

        if (a < float.Epsilon)
        {
            return;
        }

        _CurrentPeriod = a;

        //不能有动画
        float showTime = _ShowTime;
        //执行变化
        _ShowTime = 0;//不能有动画
        DrawLine(_CurrentDrawSJ, _CurrentDrawWidth, _CurrentDrawHeight);

        //复原
        _ShowTime = showTime;
    }

    //改变幅度
    public void ChangeLineAY(float s)
    {
        float a = _CurrentAY + s;
        if (a > 2f)
        {
            return;
        }

        if (a < float.Epsilon)
        {
            return;
        }

        _CurrentAY = a;

        //不能有动画
        float showTime = _ShowTime;
        //执行变化
        _ShowTime = 0;//不能有动画
        DrawLine(_CurrentDrawSJ, _CurrentDrawWidth, _CurrentDrawHeight);

        //复原
        _ShowTime = showTime;
    }

    public void SetParam(CJHZ.HZParam param)
    {
        _CurrentHZParam = param;
        foreach (var hz in _HZList)
        {
            CJHZ cjhz = hz.GetComponent<CJHZ>();
            cjhz.InitFXHZ(_CurrentHZParam);
        }

        SetHZAlphaRange();
        SetHZSizeRange();
        SetRandomRange();

        //行饰和普通的不一样，需要处理
        UpdateHSParam();
       
    }

    //行饰和普通的不一样，需要处理
    private void UpdateHSParam()
    {
        //包括大小和颜色
        _LineRenderer.startColor = _CurrentHZParam.hsImgColor;
        _LineRenderer.endColor = _CurrentHZParam.hsImgColor;

        float newW = _CurrentHZParam.hsImgSize / 100f /*摄像机缩放*/;//0->1

        _LineRenderer.startWidth = newW;
        _LineRenderer.endWidth = newW;


        SetLineSizeRange();
        SetLineAlphaRange();
    }

    public void UpdateHSParam(float s,Color c)
    {
        _CurrentHZParam.hsImgSize = s;
        _CurrentHZParam.hsImgColor = c;

        UpdateHSParam();
    }

    public void UpdateZSShapeParam(string path,float s, Color c)
    {
        _CurrentHZParam.zsImgPath = path;
        _CurrentHZParam.zsImgSize = s;
        _CurrentHZParam.zsImgColor = c;

        //update
        foreach (var hz in _HZList)
        {
            CJHZ cjhz = hz.GetComponent<CJHZ>();
            cjhz.InitFXHZ(_CurrentHZParam);
        }

        SetRandomRange();
    }

    public void UpdateZSParam(Color c)
    {
        _CurrentHZParam.zsHZColor = c;
        //update
        foreach (var hz in _HZList)
        {
            CJHZ cjhz = hz.GetComponent<CJHZ>();
            cjhz.InitFXHZ(_CurrentHZParam);
        }

        SetHZAlphaRange();
    }

    public void SetFontStyle(bool bold)
    {
        foreach (var hz in _HZList)
        {
            CJHZ cjhz = hz.GetComponent<CJHZ>();
            cjhz.SetHZFonStyle(bold);
        }
    }

    //设置曲线动画显示总的时长
    public void SetShowTime(float showTime)
    {
        _ShowTime = showTime;
    }

    public CJHZ.HZParam GetLineRenderCurrentHZParam()
    {
        return _CurrentHZParam;
    }
    public float GetLineRenderHZSize()
    {
        return _HZSize;
    }

    public void AdjustFXParam(float hzAlpahRange,
        float hzSizeRange,
        float lineAlphaRange,
        float lineSizeRange,
        float randomRange)
    {

        _CurrentHZAlphaRange = hzAlpahRange;
        _CurrentHZSizeRange = hzSizeRange;
        _CurrentLineAlphaRange = lineAlphaRange;
        _CurrentLineSizeRange = lineSizeRange;
        _CurrentRandomRange = randomRange;


        //汉字 - 透明度、大小 + 随机性
        SetHZAlphaRange();
        SetHZSizeRange();
        //线 - 透明度、大小 + 随机性
        SetLineAlphaRange();
        SetLineSizeRange();
        SetRandomRange();
    }

    private float _CurrentHZAlphaRange = 0.0f;
    private float _CurrentHZSizeRange = 0.0f;
    private float _CurrentLineAlphaRange = 0.0f;
    private float _CurrentLineSizeRange = 0.0f;
    private float _CurrentRandomRange = 0.0f;

    private void SetHZAlphaRange()
    {
        List<float> hzas = GetHZAlphaRangeAlpha(_HZList.Count);

        for (int i = 0; i < _HZList.Count; i++)
        {
            CJHZ cjhz = _HZList[i].GetComponent<CJHZ>();
            cjhz.HZ.color = new Color(_CurrentHZParam.zsHZColor.r, _CurrentHZParam.zsHZColor.g, _CurrentHZParam.zsHZColor.b, hzas[i]);
        }
    }
    private List<float> GetHZAlphaRangeAlpha(int hzCnt)
    {
        List<float> ret = new List<float>();

        Color zsHZColor = _CurrentHZParam.zsHZColor;
        int a0_pos = (int)(Mathf.Abs(_CurrentHZAlphaRange) * hzCnt);

        float aInv0 = 0;
        float aInv1 = 0f;
        if (a0_pos == hzCnt)
        {
            aInv1 = zsHZColor.a / hzCnt;
        }
        else if (a0_pos == 0)
        {

        }
        else
        {
            aInv0 = zsHZColor.a / a0_pos;
            aInv1 = zsHZColor.a / (hzCnt - a0_pos);
        }

        for (int i = 0; i < hzCnt; i++)
        {
            if (a0_pos == 0)
            {
                ret.Add(zsHZColor.a);
            }
            else if (a0_pos == hzCnt)
            {
                if (_CurrentHZAlphaRange > 0)
                {
                    ret.Add(Define.MIN_LINE_HZ_ALPHA + i * aInv1);
                }
                else
                {
                    ret.Add(Define.MIN_LINE_HZ_ALPHA + zsHZColor.a - i * aInv1);
                }
            }
            else
            {
                if (i < a0_pos)
                {
                    if (_CurrentHZAlphaRange > 0)
                    {
                        ret.Add(Define.MIN_LINE_HZ_ALPHA + zsHZColor.a - i * aInv0);
                    }
                    else
                    {
                        ret.Add(Define.MIN_LINE_HZ_ALPHA + i * aInv0);
                    }
                }
                else
                {
                    if (_CurrentHZAlphaRange > 0)
                    {
                        ret.Add(Define.MIN_LINE_HZ_ALPHA + (i + 1 - a0_pos) * aInv1);
                    }
                    else
                    {
                        ret.Add(Define.MIN_LINE_HZ_ALPHA + zsHZColor.a - (i + 1 - a0_pos) * aInv1);
                    }
                }
            }
        }

        return ret;
    }
    private void SetHZSizeRange()
    {
        List<float> hsrs = GetHZSizeRangeScale(_HZList.Count);

        for (int i = 0; i < _HZList.Count; i++)
        {
            _HZList[i].transform.localScale = new Vector3(hsrs[i], hsrs[i], 1.0f);
        }
    }
    private List<float> GetHZSizeRangeScale(int hzCnt)
    {
        List<float> ret = new List<float>();

        int s0_pos = (int)(Mathf.Abs(_CurrentHZSizeRange) * hzCnt);

        float sInv0 = 0;
        float sInv1 = 0f;
        if (s0_pos == hzCnt)//全部依次降低
        {
            sInv1 = _HZSize / hzCnt;
        }
        else if (s0_pos == 0)//保存原样
        {
        }
        else
        {
            sInv0 = _HZSize / s0_pos;
            sInv1 = _HZSize / (hzCnt - s0_pos);
        }

        for (int i = 0; i < hzCnt; i++)
        {
            if (s0_pos == 0)
            {
                ret.Add(1f);
            }
            else if (s0_pos == hzCnt)
            {
                if (_CurrentHZSizeRange > 0)
                {
                    ret.Add(Define.MIN_LINE_HZ_SIZE + i * sInv1 / _HZSize);
                }
                else
                {
                    ret.Add(Define.MIN_LINE_HZ_SIZE + 1 - i * sInv1 / _HZSize);
                }
            }
            else
            {
                if (i < s0_pos)
                {
                    if (_CurrentHZSizeRange > 0)
                    {
                        ret.Add(Define.MIN_LINE_HZ_SIZE + (_HZSize - i * sInv0) / _HZSize);
                    }
                    else
                    {
                        ret.Add(Define.MIN_LINE_HZ_SIZE + 1 - (_HZSize - i * sInv0) / _HZSize);
                    }
                }
                else
                {
                    if (_CurrentHZSizeRange > 0)
                    {
                        ret.Add(Define.MIN_LINE_HZ_SIZE + (i + 1 - s0_pos) * sInv1 / _HZSize);
                    }
                    else
                    {
                        ret.Add(Define.MIN_LINE_HZ_SIZE + 1 - (i + 1 - s0_pos) * sInv1 / _HZSize);
                    }
                }
            }
        }

        return ret;
    }

    private void SetLineAlphaRange()
    {
        float minAlpha = (1 - Mathf.Abs(_CurrentLineAlphaRange)) * _CurrentHZParam.hsImgColor.a;
        if (_CurrentLineAlphaRange < 0)
        {
            //翻转渐变
            _LineRenderer.startColor = new Color(_CurrentHZParam.hsImgColor.r,
                _CurrentHZParam.hsImgColor.g,
                _CurrentHZParam.hsImgColor.b,
                minAlpha);

            _LineRenderer.endColor = _CurrentHZParam.hsImgColor;
        }
        else
        {
            _LineRenderer.endColor = new Color(_CurrentHZParam.hsImgColor.r,
                _CurrentHZParam.hsImgColor.g,
                _CurrentHZParam.hsImgColor.b,
                minAlpha);

            _LineRenderer.startColor = _CurrentHZParam.hsImgColor;
        }
    }

    private void SetLineSizeRange()
    {
        float minSize = (1 - Mathf.Abs(_CurrentLineSizeRange)) * _CurrentHZParam.hsImgSize;

        if (_CurrentLineSizeRange < 0)
        {
            _LineRenderer.startWidth = minSize / 100f;
            _LineRenderer.endWidth = _CurrentHZParam.hsImgSize /100f;
        }
        else
        {
            _LineRenderer.startWidth = _CurrentHZParam.hsImgSize /100f;
            _LineRenderer.endWidth = minSize / 100f;
        }
    }

    private List<float> GetRandomRangeR(int hzCnt)
    {
        List<float> ret = new List<float>();
        //float 
        for (int i = 0; i < hzCnt; i++)
        {
            float r = HZManager.GetInstance().GenerateRandomInt((int)((1 - _CurrentRandomRange) * 100), 100) / 100f;
            ret.Add(_CurrentHZParam.zsImgSize * r);
        }

        return ret;
    }
    private void SetRandomRange()
    {
        //float
        List<float> rrr =  GetRandomRangeR(_HZList.Count);
        for (int i = 0; i < _HZList.Count; i++)
        {
            CJHZ cjhz = _HZList[i].GetComponent<CJHZ>();
            cjhz.ZhuangShiImg.transform.localScale = new Vector3(rrr[i],rrr[i],1f);
        }
    }
    ///------------------------------------------------------------------------
    // 起始点均为中心，如果支持其他形式较为麻烦，仅支持一个曲线
    public void ShowChangeFXText(string sj, int w, int h,float showTime,bool useHZAni, Action cb = null)
    {
        if (_CurrentLineType == LineType.NONE)
        {
            _FXAniFinishCallback?.Invoke();
            return;
        }

        ShowFXText(_CurrentLineType,sj,w,h, showTime, useHZAni, cb,true);

    }
    private LineType _CurrentLineType = LineType.NONE;
    public void ShowFXText(LineType type,string shiJu,int w,int h, float showTime,bool useHZAni, Action cb = null,bool must = false)
    {
        if (_CurrentLineType == type && !must) return;
        if (shiJu.Length == 0)
        {
            _FXAniFinishCallback?.Invoke();
            return;
        }

        _FXAniFinishCallback = cb;
        _UseHZAni = useHZAni;

        //直接停止两个动画
        if (DOTween.IsTweening("FXLineAni"))
        {
            DOTween.Kill("FXLineAni");
        }

        if (DOTween.IsTweening("FXHZAni"))
        {
            DOTween.Kill("FXHZAni");
        }

        SetShowTime(showTime);

        //不同曲线，重置临时调整参数，不然会有些难以理解
        if (_CurrentLineType != type)
        {
            _CurrentPeriod = 1f;
            _HZSize = 40f;
            _CurrentAY = 1f;
        }

        _FX.SetActive(true);
        _CurrentLineType = type;

        w = (int)(GetFitScreenScale() * w);

        string sj = shiJu;
        if (sj.Length > Define.MAX_FX_HZ_NUM)
        {
            sj = sj.Substring(0, Define.MAX_FX_HZ_NUM);//字数不能超过最大长度
            //可以给予提示
        }

        _CurrentDrawWidth = w;
        _CurrentDrawHeight = h;
        _CurrentDrawSJ = sj;
        DrawLine(_CurrentDrawSJ, _CurrentDrawWidth, _CurrentDrawHeight);
    }

    private void DrawLine(string sj,int w,int h)
    {
        switch (_CurrentLineType)
        {
            case LineType.NONE:
                _FX.SetActive(false);

                //原则上，此时就可以停止了
                if (DOTween.IsTweening("FXLineAni"))
                {
                    DOTween.Kill("FXLineAni");
                    _FXAniFinishCallback?.Invoke();
                }

                if (DOTween.IsTweening("FXHZAni"))
                {
                    DOTween.Kill("FXHZAni");
                    _FXAniFinishCallback?.Invoke();
                }

                break;
            case LineType.Circel:
                DrawCircel(sj, w, h);
                break;
            case LineType.TuoYuan:
                DrawTuoYuan(sj, w, h);
                break;
            case LineType.Duishu:
                DrawDuishu(sj, w, h);
                break;
            case LineType.Sin:
                DrawSin(sj, w, h);
                break;
            case LineType.Paowu:
                DrawPaowu(sj, w, h);
                break;
            case LineType.Zhengtai:
                DrawZhengtai(sj, w, h);
                break;
            case LineType.Tan:
                DrawTan(sj, w, h);
                break;
            case LineType.Xin:
                DrawXin(sj, w, h);
                break;
            case LineType.Xin2:
                DrawXin2(sj, w, h);
                break;
            case LineType.Xin_dian_tu:
                DrawXin_dian_tu(sj, w, h);
                break;
            case LineType.Xin_dian_tu2:
                DrawXin_dian_tu2(sj, w, h);
                break;
            case LineType.Ci_sheng_bo:
                DrawCi_sheng_bo(sj, w, h);
                break;
            case LineType.Ye_xing:
                DrawYe_xing(sj, w, h);
                break;
            case LineType.Jian_kai:
                DrawJian_kai(sj, w, h);
                break;
            case LineType.Jian_kai2:
                DrawJian_kai2(sj, w, h);
                break;
            case LineType.Xing_xing:
                DrawXing_xing(sj, w, h);
                break;
            case LineType.Wu_jiao_xing:
                DrawWu_jiao_xing(sj, w, h);
                break;
            case LineType.Huan_xing_luo_xuan:
                DrawHuan_xing_luo_xuan(sj, w, h);
                break;
            case LineType.San_ye_xian:
                DrawSan_ye_xian(sj, w, h);
                break;
            case LineType.San_jian_ban_xian:
                DrawSan_jian_ban_xian(sj, w, h);
                break;
            case LineType.A_ji_mi_de_luo_xuan:
                DrawA_ji_mi_de_luo_xuan(sj, w, h);
                break;
            case LineType.Yi_feng_san_zhu_dian_xian:
                DrawYi_feng_san_zhu_dian_xian(sj, w, h);
                break;
            case LineType.Ba_zi_xian:
                DrawBa_zi_xian(sj, w, h);
                break;
            case LineType.Ba_zi_xian2:
                DrawBa_zi_xian2(sj, w, h);
                break;
            case LineType.Wen_xiang:
                DrawWen_xiang(sj, w, h);
                break;
            case LineType.Zi_xing_xian:
                DrawZi_xing_xian(sj, w, h);
                break;
            case LineType.Mei_hua_xian:
                DrawMei_hua_xian(sj, w, h);
                break;
            case LineType.Mei_hua_xian2:
                DrawMei_hua_xian2(sj, w, h);
                break;
            case LineType.Mei_hua_xian3:
                DrawMei_hua_xian3(sj, w, h);
                break;
            case LineType.Mei_hua_xian4:
                DrawMei_hua_xian4(sj, w, h);
                break;
            // case LineType.Mei_hua_xian5:
            //     DrawMei_hua_xian5(sj, w, h);
            //     break;
            case LineType.Wan_yue:
                DrawWan_yue(sj, w, h);
                break;
            case LineType.Hu_Die:
                DrawHu_Die(sj, w, h);
                break;
            case LineType.Hu_Die2:
                DrawHu_Die2(sj, w, h);
                break;
            case LineType.Tan_huang:
                DrawTan_huang(sj, w, h);
                break;
            case LineType.Shuang_hu_wai_bai_xian:
                DrawShuang_hu_wai_bai_xian(sj, w, h);
                break;
            case LineType.TangGuo:
                DrawTangGuo(sj, w, h);
                break;
            //  case LineType.Wai_bai_xian:
            //      DrawWai_bai_xian(sj, w, h);
            //     break;
            case LineType.Zan_xing_xian:
                DrawZan_xing_xian(sj, w, h);
                break;
            case LineType.Zan_xing_xian2:
                DrawZan_xing_xian2(sj, w, h);
                break;
            case LineType.Zan_xing_xian3:
                DrawZan_xing_xian3(sj, w, h);
                break;
            case LineType.Die_xing_tan_huang:
                DrawDie_xing_tan_huang(sj, w, h);
                break;
            case LineType.Yan_jian_wei:
                DrawYan_jian_wei(sj, w, h);
                break;
            case LineType.Tu_zi:
                DrawTu_zi(sj, w, h);
                break;
            case LineType.Palm:
                DrawPalm(sj, w, h);
                break;
            case LineType.She_xing_xian:
                DrawShe_xing_xian(sj, w, h);
                break;
            case LineType.Shi_zi_jian_kai:
                DrawShi_zi_jian_kai(sj, w, h);
                break;
            case LineType.Wo_gui_xian:
                DrawWo_gui_xian(sj, w, h);
                break;
            case LineType.Hua_hui:
                DrawHua_hui(sj, w, h);
                break;
            case LineType.Gou_zi:
                DrawGou_zi(sj, w, h);
                break;
            case LineType.Fei_ji:
                DrawFei_ji(sj, w, h);
                break;
            case LineType.Shi_zi:
                DrawShi_zi(sj, w, h);
                break;
        }
    }

    #region  曲线绘制
    private List<GameObject> _HZList = new List<GameObject>();
    private List<Vector3> _LineScreenPos = new List<Vector3>();//显示字使用
    private Vector3 GetPos(LineType type,float t, FXAddtionalType fxAdd)//FXAddtionalType.FX_NONE
    {
        Vector3 pos = Vector3.zero;

        switch (type)
        {
            case LineType.Circel:
                pos = new Vector3(Mathf.Cos(t * Mathf.PI / 180f), Mathf.Sin(t * Mathf.PI / 180f), 0.0f);
                break;
            case LineType.TuoYuan:
                pos = new Vector3(Mathf.Cos(t * Mathf.PI / 180f), Mathf.Sin(t * Mathf.PI / 180f), 0f);                
                break;
            case LineType.Duishu:
                pos = new Vector3(t, Mathf.Log(t), 0f);//0.1->10，-1->1
                break;
            case LineType.Sin:
                pos = new Vector3(t * Mathf.PI / 180f, Mathf.Sin(t * Mathf.PI / 180f),0f);
                break;
            case LineType.Paowu:
                pos = new Vector2(t, Mathf.Pow(t, 2));
                break;
            case LineType.Zhengtai:
                pos = new Vector3(t, Mathf.Exp(-t * t),0f);
                break;
            case LineType.Tan:
                pos = new Vector3(t * Mathf.PI / 180f, Mathf.Tan(t * Mathf.PI / 180f), 0f);
                break;
            case LineType.Xin:
                pos = new Vector3(2 * Mathf.Sin(t * Mathf.PI / 180) - Mathf.Sin(2 * t * Mathf.PI / 180),
                   2 * Mathf.Cos(t * Mathf.PI / 180) - Mathf.Cos(2 * t * Mathf.PI / 180) ,0f);
                break;
            case LineType.Xin2:
                if (fxAdd == FXAddtionalType.FX_Xin2_1)
                {
                    pos = new Vector3(t, Mathf.Sqrt(1 - Mathf.Pow((Mathf.Abs(t) - 1), 2f)),0f);
                }
                else if (fxAdd == FXAddtionalType.FX_Xin2_2)
                {
                    pos = new Vector3(t, -3 * Mathf.Sqrt(1 - Mathf.Sqrt(Mathf.Abs(t) / 2f)),0f);
                }

                break;
            case LineType.Xin_dian_tu://参数太多，不调整了
                pos = new Vector3((Mathf.Sin(4 * t * Mathf.PI / 180) + 0.2f) * Mathf.Cos(10 + t * (12 * Mathf.PI / 180)),
                    (Mathf.Sin(4 * t * Mathf.PI / 180) + 0.2f) * Mathf.Sin(10 + t * (12 * Mathf.PI / 180)),0f);
                break;
            case LineType.Xin_dian_tu2:
                float theta = 10 + 12 * t * Mathf.PI / 180f;
                float r = Mathf.Sin(t * 4 * Mathf.PI / 180f) + 0.2f;

                // float x = r * Mathf.Cos(theta);
                float y = r * Mathf.Sin(theta);
                float z = 3 * t;

                pos = new Vector3(z, y,0f);

                break;
            case LineType.Ci_sheng_bo:
                pos = new Vector3(t, t * Mathf.Cos(t * 16 * Mathf.PI / 180),0f);
                break;
            case LineType.Ye_xing:
                pos = new Vector3(t / (1f + (t * t * t)),(t * t) / (1f + (t * t * t)),0f);
                break;
            case LineType.Jian_kai:
                pos = new Vector3(t * (Mathf.Cos(2 * t * Mathf.PI / 180f) + Mathf.Sin(2 * t * Mathf.PI / 180f)),
                    t * (Mathf.Sin(2 * t * Mathf.PI / 180f) - Mathf.Cos(2 * t * Mathf.PI / 180f)),0f);
                break;
            case LineType.Jian_kai2:
                pos = new Vector3(t * (Mathf.Cos(2 * t * Mathf.PI / 180f) + Mathf.Sin(2 * t * Mathf.PI / 180f)),
                    t * (Mathf.Sin(2 * t * Mathf.PI / 180f) - Mathf.Cos(2 * t * Mathf.PI / 180f)),0f);

                break;
            case LineType.Xing_xing:

                pos = new Vector3(Mathf.Pow(Mathf.Cos(2 * t * Mathf.PI / 180f), 3),Mathf.Pow(Mathf.Sin(2 * t * Mathf.PI / 180f), 3),0f);
                break;
            case LineType.Wu_jiao_xing:
                pos = new Vector3((25 + 4 * Mathf.Cos(8 * t * Mathf.PI / 180f) + 10 * Mathf.Cos(2 / 3f * 8 * t * Mathf.PI / 180f)),
                    (25 + 4 * Mathf.Sin(8 * t * Mathf.PI / 180f) - 6 * Mathf.Sin(2 / 3f * 8 * t * Mathf.PI / 180f)),0f);

                break;
            case LineType.Huan_xing_luo_xuan:
                pos = new Vector3((50 + 10 * Mathf.Sin(t * 30 * Mathf.PI / 180f)) * Mathf.Cos(t * 2 * Mathf.PI / 180f)
                    , (50 + 10 * Mathf.Sin(t * 30 * Mathf.PI / 180f)) * Mathf.Sin(t * 2 * Mathf.PI / 180f),0f);
                break;
            case LineType.San_ye_xian:
                theta = 380 / 180f * t * Mathf.PI / 180f;
                pos = new Vector3(Mathf.Cos(theta) * (4 * Mathf.Sin(theta) * Mathf.Sin(theta) - 1) * Mathf.Cos(theta),
                    Mathf.Cos(theta) * (4 * Mathf.Sin(theta) * Mathf.Sin(theta) - 1) * Mathf.Sin(theta),0f);
                break;
            case LineType.San_jian_ban_xian:
                theta = t * 2 * Mathf.PI / 180f;
                pos = new Vector3(2 * Mathf.Cos(theta) + Mathf.Cos(2 * theta),2 * Mathf.Sin(theta) - Mathf.Sin(2 * theta),0f);
                break;
            case LineType.A_ji_mi_de_luo_xuan:
                theta = t * 20 / 9f * Mathf.PI / 180f;
                pos = new Vector3(theta * Mathf.Cos(theta), theta * Mathf.Sin(theta),0f);
                break;
            case LineType.Yi_feng_san_zhu_dian_xian:
                float x = 3f * t - 1.5f;
                pos = new Vector3(x, Mathf.Pow(x * x - 1, 3) + 1,0f);
                break;
            case LineType.Ba_zi_xian:
                pos = new Vector3( Mathf.Cos(t * 2 * Mathf.PI / 180), Mathf.Sin(t * 10 * Mathf.PI / 180),0f);
                break;
            case LineType.Ba_zi_xian2:
               pos = new Vector3(Mathf.Sin(t * 10 * Mathf.PI / 180) + Mathf.Sin(t * 6 * Mathf.PI / 180),
                    3 * Mathf.Cos(t * 2 * Mathf.PI / 180) + Mathf.Cos(t * 6 * Mathf.PI / 180),0f);
                break;
            case LineType.Wen_xiang:
                r = t * (10 * Mathf.PI / 180f) + 1;
                theta = 10 + t * (20 * Mathf.PI / 180f);
                pos = new Vector3(r * Mathf.Cos(theta),r * Mathf.Sin(theta),0f);
                break;
            case LineType.Zi_xing_xian:
                theta = t * 2 * Mathf.PI / 180f;
                r = 10 + Mathf.Pow(8 * Mathf.Sin(theta), 2);
                pos = new Vector3(r * Mathf.Cos(theta) ,r * Mathf.Sin(theta),0f);
                break;
            case LineType.Mei_hua_xian:
                theta = t * 2 * Mathf.PI / 180f;
                r = 10 + Mathf.Pow(3 * Mathf.Sin(theta * 2.5f), 2);
                pos = new Vector3(r * Mathf.Cos(theta),r * Mathf.Sin(theta),0f);
                break;
            case LineType.Mei_hua_xian2:
                theta = t * 2 * Mathf.PI / 180f;
                r = 10 - Mathf.Pow(3 * Mathf.Sin(theta * 3f), 2);
                pos = new Vector3(r * Mathf.Cos(theta),r * Mathf.Sin(theta),0f);
                break;
            case LineType.Mei_hua_xian3:
                theta = 8 * t * Mathf.PI / 180f;
                x = 2 + 8 * Mathf.Cos(theta) + 10 * Mathf.Cos(2f / 3 * theta);
                y = 2 + 8 * Mathf.Sin(theta) - 10 * Mathf.Sin(2f / 3 * theta);
                pos = new Vector3(x, y,0f);
                break;
            case LineType.Mei_hua_xian4:
                theta = 2 * t * Mathf.PI / 180f;
                r = 10 + 10 * Mathf.Sin(6 * theta);
                x = r * Mathf.Cos(theta);
                y = r * Mathf.Sin(theta);
                z = 2 * Mathf.Sin(6 * theta);
                pos = new Vector3(z, x,0f);
                break;
                /*
            case LineType.Mei_hua_xian5:
                theta = 2 * t * Mathf.PI / 180f;
                r = 10 + 10 * Mathf.Sin(6 * theta);

                x = r * Mathf.Cos(theta);
                y = r * Mathf.Sin(theta);
                z = 2 * Mathf.Sin(6 * theta);
                pos = new Vector3(x , y,0f);
                break;
                */
            case LineType.Wan_yue:
                theta = 2 * t * Mathf.PI / 180f;
                x = Mathf.Cos(theta) + Mathf.Cos(2 * theta);
                y = 2 * Mathf.Sin(theta) + 2 * Mathf.Sin(theta);
                pos = new Vector3(x, y,0f);
                break;
            case LineType.Hu_Die:
                x = t * Mathf.Sin(8 * t * Mathf.PI / 180f) * Mathf.Cos(-16 * t * Mathf.PI / 180f);
                y = t * Mathf.Sin(8 * t * Mathf.PI / 180f) * Mathf.Sin(-16 * t * Mathf.PI / 180f);
                z = t * Mathf.Cos(8 * t * Mathf.PI / 180f);
                pos = new Vector3(z, y,0f);
                break;
            case LineType.Hu_Die2:
                x = t * Mathf.Sin(8 * t * Mathf.PI / 180f) * Mathf.Cos(-16 * t * Mathf.PI / 180f);
                y = t * Mathf.Sin(8 * t * Mathf.PI / 180f) * Mathf.Sin(-16 * t * Mathf.PI / 180f);
                z = t * Mathf.Cos(8 * t * Mathf.PI / 180f);
                pos = new Vector3(x, z, 0f);
                break;
            case LineType.Tan_huang:
                x = 4 * Mathf.Cos(t * 10 * Mathf.PI / 180f);
                y = 4 * Mathf.Sin(t * 10 * Mathf.PI / 180f);
                z = 6 * t;
                pos = new Vector3(z ,x,0f);
                break;
            case LineType.Shuang_hu_wai_bai_xian:
                x = 7.5f * Mathf.Cos(t * 2 * Mathf.PI / 180f) + 2.5f * Mathf.Cos(6 * t * Mathf.PI / 180f);
                y = 7.5f * Mathf.Sin(t * 2 * Mathf.PI / 180f) + 2.5f * Mathf.Sin(6 * t * Mathf.PI / 180f);
                pos = new Vector3(x,y,0f);
                break;
            case LineType.TangGuo:
                float f = 1;
                float a = 1.1f;
                float b = 0.666f;
                theta = 2 * t * Mathf.PI / 180f;
                float c = Mathf.Sin(theta);
                x = (a * a + f * f * c * c) * Mathf.Cos(theta) / a;
                y = (a * a - 2 * f + f * f * c * c) * Mathf.Sin(theta) / b;
                pos = new Vector3(x, y,0f) ;
                break;
                /*
            case LineType.Wai_bai_xian:
                theta = 20 * t * Mathf.PI / 180f;
                b = 8;
                a = 5;
                x = (a + b) * Mathf.Cos(theta) - b * Mathf.Cos((a / b + 1) * theta);
                y = (a + b) * Mathf.Sin(theta) - b * Mathf.Sin((a / b + 1) * theta);
                pos= new Vector2(x, y);
                break;
                */
            case LineType.Zan_xing_xian:
                r = 8 * t;
                theta = 8 * t * Mathf.PI / 180f;
                float phi = 8 * t * Mathf.PI / 180f;
                x = r * Mathf.Sin(theta) * Mathf.Cos(phi);
                y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                z = r * Mathf.Cos(phi);
                pos= new Vector3(x, z,0f);
                break;
            case LineType.Zan_xing_xian2:
                r = 8 * t;
                theta = 8 * t * Mathf.PI / 180f;
                phi = 8 * t * Mathf.PI / 180f;
                x = r * Mathf.Sin(theta) * Mathf.Cos(phi);
                y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                z = r * Mathf.Cos(phi);
                pos = new Vector3(x, y, 0f);
                break;
            case LineType.Zan_xing_xian3:
                r = 8 * t;
                theta = 8 * t * Mathf.PI / 180f;
                phi = 8 * t * Mathf.PI / 180f;
                x = r * Mathf.Sin(theta) * Mathf.Cos(phi);
                y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                z = r * Mathf.Cos(phi);
                pos = new Vector3(z, y, 0f);
                break;
            case LineType.Die_xing_tan_huang:
                r = 5;
                theta = t * 2 * Mathf.PI / 180f;
                x = r * Mathf.Cos(theta);
                y = r * Mathf.Sin(theta);
                z = Mathf.Sin(3.5f * theta - 0.5f * Mathf.PI / 180f) + 24f;
                pos = new Vector3(y, z,0f);
                break;
            case LineType.Yan_jian_wei:
                x = 3 * Mathf.Cos(t * 8 * Mathf.PI / 180f);
                y = 3 * Mathf.Sin(t * 6 * Mathf.PI / 180f);
                pos= new Vector3(x, y,0f);
                break;
            case LineType.Tu_zi:
                theta = t * 360 - 90;
                r = Mathf.Cos(360 * (t / (1 + Mathf.Pow(t, 6.5f))) * 6 * t * Mathf.PI / 180f) * 3.5f + 5f;
                x = r * Mathf.Cos(theta * Mathf.PI / 180f);
                y = r * Mathf.Sin(theta * Mathf.PI / 180f);
                pos = new Vector3(x, y,0f);
                break;
            case LineType.Palm:
                theta = t * 360 + 180;
                r = Mathf.Cos(360 * t * t * t * 6 * Mathf.PI / 180f) * 2 + 5f;
                x = r * Mathf.Cos(theta * Mathf.PI / 180f);
                y = r * Mathf.Sin(theta * Mathf.PI / 180f);
                pos = new Vector3(x, y,0f);
                break;
            case LineType.She_xing_xian:
                x = 2 * Mathf.Cos(t * 360 * 3 * Mathf.PI / 180f) * t;
                y = 2 * Mathf.Sin(t * 360 * 3 * Mathf.PI / 180f) * t;
                z = Mathf.Pow(t, 3 / 8f) * 5;
                pos = new Vector3(x, -z,0f);
                break;
            case LineType.Shi_zi_jian_kai:
                theta = t * 360 * 4;
                r = (Mathf.Cos(t * 360 * 16 * Mathf.PI / 180f) * 0.5f * t + 1) * t;
                x = r * Mathf.Cos(theta * Mathf.PI / 180f);
                y = r * Mathf.Sin(theta * Mathf.PI / 180f);
                pos = new Vector3(x , y,0f);
                break;
            case LineType.Wo_gui_xian:
                theta = t * 360 * 2;
                r = Mathf.Cos(t * 360 * 30 * Mathf.PI / 180f) * t * 0.5f + t * 2;
                x = r * Mathf.Cos(theta * Mathf.PI / 180f);
                y = r * Mathf.Sin(theta * Mathf.PI / 180f);
                pos = new Vector3(x, y,0f);
                break;
            case LineType.Hua_hui:
                r = 3 * t;
                theta = 10 * t * Mathf.PI / 180f;
                phi = 5 * t * Mathf.PI / 180f;
                x = r * Mathf.Sin(theta) * Mathf.Cos(phi);
                y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                z = r * Mathf.Cos(phi);
                pos = new Vector3(x , y ,0f);
                break;
            case LineType.Gou_zi:
                x = 5 * Mathf.Pow(Mathf.Cos(t * 360 * Mathf.PI / 180f), 3) * t;
                y = 5 * Mathf.Pow(Mathf.Sin(t * 360 * Mathf.PI / 180f), 3) * t;
                pos = new Vector2(x, y);
                break;
            case LineType.Fei_ji:
                x = Mathf.Cos(t * 360 * Mathf.PI / 180f) + Mathf.Cos(3 * t * 360 * Mathf.PI / 180f);
                y = Mathf.Sin(t * 360 * Mathf.PI / 180f) + Mathf.Sin(5 * t * 360 * Mathf.PI / 180f);
                pos = new Vector3(x, y);
                break;
            case LineType.Shi_zi:
                theta = t * 360 + 90;
                r = Mathf.Cos(360 * t * 4 * Mathf.PI / 180) * 0.5f + 1f;
                x = r * Mathf.Cos(theta * Mathf.PI / 180f);
                y = r * Mathf.Sin(theta * Mathf.PI / 180f);
                pos = new Vector3(x, y,0f);
                break;
        }

        return pos;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">线类型</param>
    /// <param name="realW">可显示范围，方形</param>
    /// <param name="sizeScale">实际显示范围比例</param>
    /// <param name="period">取值范围，可以是周期，也是可以数字</param>
    /// <param name="moreParam">呈现参数，没有则为空，优先控制y，其次x</param>
    /// <param name="begin">开始数值</param>
    /// <param name="fxAdd">多段函数使用</param>
    /// <param name="increase">是否是正着循环</param>
    /// <param name="resScale">分辨率倍数</param>
    private void InitLinePos(LineType type,int realW,
        float sizeScale, float period,float begin = 0,
        List<float> moreParam = null,
        FXAddtionalType fxAdd = FXAddtionalType.FX_NONE,
        bool increase = true,
        float resScale = 1f)
    {
        _LineScreenPos.Clear();

        int pointCnt = Mathf.CeilToInt(realW * FX_RES * resScale);
        float wh = realW * sizeScale;

        int tmp = 1;
        if (!increase)
        {
            tmp = -1;
        }
        for (float i = begin; _LineScreenPos.Count < pointCnt; i += tmp * 1.0f * period / (pointCnt - 1))
        {

            if (increase)
            {
                if (i > begin + period)
                {
                    i = begin + period;//x值不能超过取值范围
                }
            }
            else
            {
                if (i < begin - period)
                {
                    i = begin - period;
                }
            }

            Vector3 pos = Vector3.zero;
            pos = GetPos(type, i, fxAdd) * wh;

            if (moreParam != null)
            {
                if (moreParam.Count == 1)
                {
                    pos.y *= moreParam[0];
                }
                else if (moreParam.Count == 2)
                {
                    pos.y *= moreParam[0];
                    pos.x *= moreParam[1];
                }
            }

            _LineScreenPos.Add(pos);
        }
    }

    private void ShowLine(float offsetX,float offsetY)
    {
        Sequence lineAni = DOTween.Sequence();
        lineAni.SetId("FXLineAni");

        float invTime = _ShowTime / _LineScreenPos.Count;
        if (_ShowTime <= float.Epsilon)
        {
            _LineRenderer.positionCount = _LineScreenPos.Count;
            _FXAniFinishCallback?.Invoke();
        }
        else
        {
            _LineRenderer.positionCount = 0;
        }

        for (int i = 0; i < _LineScreenPos.Count; i++)
        {
            Vector3 screenPosition = new Vector3(_LineScreenPos[i].x + offsetX, _LineScreenPos[i].y + offsetY, 1.0f);
            Vector3 position = Camera.main.ScreenToWorldPoint(screenPosition);
            position = new Vector3(position.x, position.y, _FXCamera.transform.position.z);

            if (_ShowTime <= float.Epsilon)
            {
                _LineRenderer.SetPosition(i, position);
            }
            else
            {
                int index = i;
                lineAni.AppendInterval(invTime)
                    .AppendCallback(() => {
                        _LineRenderer.positionCount++;

                        if (_LineRenderer.positionCount > _LineScreenPos.Count)
                        {
                            Debug.Log("出错了");
                            return;
                        }

                        if (_LineRenderer.positionCount == _LineScreenPos.Count)
                        {
                            _FXAniFinishCallback?.Invoke();
                        }

                        _LineRenderer.SetPosition(index, position);
                    });
            }
        }
    }

    //必须在获取的screenpos之后
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sj">文字</param>
    /// <param name="offset">是否对字进行位置段偏移</param>
    /// <param name="reserve">是否增加保留空位</param>
    /// <param name="offsetX">文字位置偏移</param>
    /// <param name="offsetY">文字位置偏移</param>
    private void InitHZ(string sj,bool offset = false,int reserve = 0,float offsetX = 0,float offsetY = 0)
    {

        int inv = _LineScreenPos.Count;
        if (sj.Length > 1)
        {
            inv = _LineScreenPos.Count / (sj.Length + reserve - 1);//无法填满
        }

        inv = inv == 0 ? 1 : inv;//应该不会出现，不可能realW<32

        DestroyObj(_HZList);
        float s = 1.0f * Screen.width / Screen.height * GetFitIPadScreenScale();

        int starti = 0;
        if (offset)
        {
            int left = _LineScreenPos.Count - inv * (sj.Length + reserve - 1);
            starti = left / 2;//偏移一半
            if (left  == 1)
            {
                starti = 1;
            }
        }

        if (starti < 0) starti = 0;

        bool bold = Setting.GetFontBold();

        float speed = _ShowTime / sj.Length/2;

        Sequence FXHZAni = DOTween.Sequence();
        FXHZAni.SetId("FXHZAni");

        //当前参数下 字的缩放和透明度
        List<float> hzas = GetHZAlphaRangeAlpha(sj.Length);
        List<float> hsrs = GetHZSizeRangeScale(sj.Length);
        List<float> rrrs = GetRandomRangeR(sj.Length);
        
        for (int i = starti; i < _LineScreenPos.Count; i++)
        {
            if ((i-starti) % inv == 0)
            {
                int idx = (i - starti )/ inv;
                if (idx >= sj.Length) break;
             
                //此处需要显示一个汉字
                GameObject hz = Instantiate(_HZPrefabs, _HZConten) as GameObject;
                hz.SetActive(true);

                CJHZ cjhz = hz.GetComponentInChildren<CJHZ>();
                cjhz.InitFXHZ(_CurrentHZParam);
                cjhz.SetHZText(""+sj[idx], bold);
                _HZList.Add(hz);
                hz.transform.position = new Vector3(_LineScreenPos[i].x * s + Screen.width / 2 + offsetX,
                    _LineScreenPos[i].y * s + Screen.height / 2 + offsetY - 20/*初始化有个偏移20*/, _LineScreenPos[i].z);

                //字的透明度和大小以及底图大小 需要更具当前设置进行重新计算
                cjhz.HZ.color = new Color(cjhz.HZ.color.r, cjhz.HZ.color.g, cjhz.HZ.color.b,hzas[idx]);
                hz.transform.localScale = new Vector3(hsrs[idx], hsrs[idx],1.0f);

                RectTransform rt = hz.GetComponent<RectTransform>();
                if (idx == 0)
                {
                    rt.sizeDelta = new Vector2(_HZSize * FIRST_HZ_SCALE, _HZSize * FIRST_HZ_SCALE);
                }
                else
                {
                    rt.sizeDelta = new Vector2(_HZSize, _HZSize);
                    FixPos(hz);
                }

                if (_ShowTime > float.Epsilon)
                {
                    cjhz.HZ.color = new Color(cjhz.HZ.color.r, cjhz.HZ.color.g, cjhz.HZ.color.b, 0f);
                    cjhz.ZhuangShiImg.color = new Color(cjhz.ZhuangShiImg.color.r, cjhz.ZhuangShiImg.color.g, cjhz.ZhuangShiImg.color.b, 0f);
                    cjhz.ZhuangShiImg.transform.localScale = Vector3.zero;
                    //字体的动画
                    FXHZAni.AppendInterval(speed)
                        .Append(cjhz.HZ.DOFade(hzas[idx], speed));

                    if (_CurrentHZParam.zsImgPath != "")
                    {
                        Sequence zsImgAni = DOTween.Sequence();
                        zsImgAni.Join(cjhz.ZhuangShiImg.DOFade(_CurrentHZParam.zsImgColor.a, speed))
                                .Join(cjhz.ZhuangShiImg.transform.DOScale(rrrs[idx], speed))
                                .SetEase(Ease.OutBounce);

                        FXHZAni.Join(zsImgAni);
                    }

                    if (_UseHZAni)
                    {
                        FXHZAni.Join(cjhz.HZ.transform.DOShakeScale(speed, HZManager.GetInstance().GenerateRandomInt(0, 10) / 10.0f));
                    }
                }
            }
        }
    }

    public void DrawCircel(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float sizeScale = 1f;
        float period = 360 * _CurrentPeriod;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Circel, realW, sizeScale, period,0, moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj,false,1);
    }

    public void DrawTuoYuan(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float sizeScale = 1f;
        float period = 360 * _CurrentPeriod;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        //不再随机
        float ax = 1f;//HZManager.GetInstance().GenerateRandomInt(25, 75) / 100f;
        float by = 0.5f;////HZManager.GetInstance().GenerateRandomInt(25, 75) / 100f;//25->75

        List<float> moreParam = new List<float>();
        moreParam.Add(by * _CurrentAY);
        moreParam.Add(ax);

        InitLinePos(LineType.TuoYuan, realW, sizeScale, period,0,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false, 1);
    }
    
    public void DrawDuishu(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float sizeScale = 0.1f;
        float period = 10 * _CurrentPeriod;
        float begin = 0.01f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(2.0f * _CurrentAY);//y范围放大一倍，表现为高的


        InitLinePos(LineType.Duishu, realW, sizeScale, period,begin, moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);
    }

    public void DrawSin(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float sizeScale = 1 / Mathf.PI;
        float period = 360 * _CurrentPeriod;
        float begin = -180f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Sin, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false, 1);
    }

    public void DrawPaowu(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float sizeScale = 1f;
        float period = 2 * _CurrentPeriod;
        float begin = -1f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);//y不变
        moreParam.Add(0.5f);//x范围缩小一倍，表现为扁的

        InitLinePos(LineType.Paowu, realW, sizeScale, period, begin, moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, true);
    }

	public void DrawZhengtai(string sj, int w, int h)
	{
        int realW = w - _Blank * 2;
        float sizeScale = 0.5f;
        float period = 4 * _CurrentPeriod;
        float begin = -2f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(1.5f * _CurrentAY);//y拉高点

        InitLinePos(LineType.Zhengtai, realW, sizeScale, period, begin, moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);
	}

    public void DrawTan(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 160 * _CurrentPeriod;
        float sizeScale = 1.75f * Mathf.Tan(period * Mathf.PI / 180f) / Mathf.PI * 0.9f;
        float begin = -80f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Tan, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj,true,0);
    }
    public void DrawXin(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 360 * _CurrentPeriod;
        float sizeScale = 1f/ Mathf.PI / 6f;
        float begin = -180f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(6.0f*_CurrentAY);
        moreParam.Add(5.0f);


        InitLinePos(LineType.Xin, realW, sizeScale, period, begin, moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);
    }

    public void DrawXin2(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 4 * _CurrentPeriod;
        float sizeScale = 1/3f;
        float begin = -2;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        List<Vector3> tmpPos1 = new List<Vector3>();
        //第一段需要清除
        InitLinePos(LineType.Xin2, realW, sizeScale, period, begin, moreParam, FXAddtionalType.FX_Xin2_1);

        tmpPos1.AddRange(_LineScreenPos);

        //第二段不能清除
        begin = 2;
        List<Vector3> tmpPos2 = new List<Vector3>();
        InitLinePos(LineType.Xin2, realW, sizeScale, period, begin, moreParam, FXAddtionalType.FX_Xin2_2,false);
        tmpPos2.AddRange(_LineScreenPos);

        _LineScreenPos.Clear();

        _LineScreenPos.AddRange(tmpPos1);
        _LineScreenPos.AddRange(tmpPos2);

        ShowLine(offsetX, offsetY);
        InitHZ(sj);
    }
    public void DrawXin_dian_tu(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 90 * _CurrentPeriod;
        float sizeScale = 2f / Mathf.PI;
        float begin = -45f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Xin_dian_tu, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);

    }
    public void DrawXin_dian_tu2(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 360 * _CurrentPeriod;
        float sizeScale = 1f/540;
        float begin = -180f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(6*45f * _CurrentAY);

        InitLinePos(LineType.Xin_dian_tu2, realW, sizeScale,
            period, begin, moreParam, FXAddtionalType.FX_NONE, true, 2f);

        ShowLine(offsetX, offsetY);

        InitHZ(sj);

    }
    public void DrawCi_sheng_bo(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 360 * _CurrentPeriod;
        float sizeScale = 1f/360 * 0.9f;
        float begin = 0f;

        float offsetX = Screen.width / 2 - realW;
        float offsetY = Screen.height / 2;


        List<float> moreParam = new List<float>();
        moreParam.Add(0.5f * _CurrentAY);
        moreParam.Add(2.0f);

        InitLinePos(LineType.Ci_sheng_bo, realW, sizeScale,
            period, begin, moreParam, FXAddtionalType.FX_NONE, true, 2f);
        ShowLine(offsetX, offsetY);

        InitHZ(sj,false,0, (-1.0f*realW / 2)/GetFitScreenScale());
    }

    public void DrawYe_xing(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float sizeScale = 3f;
        float period = 1f * _CurrentPeriod;
        float begin = 0;

        float offsetX = 0;
        float offsetY = realW / 3;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Ye_xing, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        if (FitUI.GetIsBigPad())
        {
            InitHZ(sj, false, 0, -realW / 2 / GetFitScreenScale(), -realW * 5 / 12 / GetFitScreenScale());
        }
        else
        {
            InitHZ(sj, false, 0, -realW / 3 / GetFitScreenScale(), -realW * 5 / 12 / GetFitScreenScale());
        }
    }

    public void DrawJian_kai(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 360 * _CurrentPeriod;
        float sizeScale = 1f / 180 * 0.75f;
        float begin = -180f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Jian_kai, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);
    }

    public void DrawJian_kai2(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 180 * 0.75f;
        float begin = 0f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Jian_kai2, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);
    }

    public void DrawXing_xing(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 0.75f;
        float begin = 0f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Xing_xing, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj,false,1);
    }

    public void DrawWu_jiao_xing(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 135 * _CurrentPeriod;
        float sizeScale = 1/10f*0.75f;
        float begin = 0f;

        float offsetX = Screen.width/2 - 2*realW;
        float offsetY = Screen.height/2 - 2*realW;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Wu_jiao_xing, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        //fuck here
        InitHZ(sj,false,1, (-realW - realW / 10)/GetFitScreenScale(),
            (-realW - realW/10) / GetFitScreenScale());
    }

    public void DrawHuan_xing_luo_xuan(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 60;
        float begin = -90f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Huan_xing_luo_xuan, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj,false,1);

    }

    public void DrawSan_ye_xian(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 90 * _CurrentPeriod;
        float sizeScale = 1f;
        float begin = 0f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.San_ye_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,2);
    }

    public void DrawSan_jian_ban_xian(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1/3f;
        float begin = -90f;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.San_jian_ban_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false, 1);
    }
    public void DrawA_ji_mi_de_luo_xuan(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 90 * _CurrentPeriod;
        float sizeScale = 1 / Mathf.PI;
        float begin = 0f;

        float offsetX = Screen.width / 2 + _Blank/2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.A_ji_mi_de_luo_xuan, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false, 0, _Blank/2/GetFitScreenScale());
    }
    public void DrawYi_feng_san_zhu_dian_xian(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 1/2f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2 - realW;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Yi_feng_san_zhu_dian_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, true,0, 0,-realW/2 / GetFitScreenScale());
    }

    public void DrawBa_zi_xian(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 0.75f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Ba_zi_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    public void DrawBa_zi_xian2(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 90 * _CurrentPeriod;
        float sizeScale = 1/2f * 0.75f;
        float begin = 45;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2 + realW/2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Ba_zi_xian2, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,1,0,(realW/4 + 40f) / GetFitScreenScale());
    }


    public void DrawWen_xiang(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 90 * _CurrentPeriod;
        float sizeScale = 1/20f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Wen_xiang, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawZi_xing_xian(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f/80;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Zi_xing_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,1);
    }


    public void DrawMei_hua_xian(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f/20;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Mei_hua_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }


    public void DrawMei_hua_xian2(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 12;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Mei_hua_xian2, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawMei_hua_xian3(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 135 * _CurrentPeriod;
        float sizeScale = 1f / 20;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Mei_hua_xian3, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,1);

    }
    public void DrawMei_hua_xian4(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 20;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;


        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);
        moreParam.Add(2.0f);


        InitLinePos(LineType.Mei_hua_xian4, realW, sizeScale, period, begin, moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawMei_hua_xian5(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f/20;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        //InitLinePos(LineType.Mei_hua_xian5, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    public void DrawWan_yue(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 4;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Wan_yue, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawHu_Die(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 180f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Hu_Die, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawHu_Die2(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 180f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Hu_Die2, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawTan_huang(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 360 * _CurrentPeriod;
        float sizeScale = 1f/180/6;
        float begin = -180;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(135f*_CurrentAY);
        moreParam.Add(0.75f);

        InitLinePos(LineType.Tan_huang, realW, sizeScale,
            period, begin, moreParam, FXAddtionalType.FX_NONE, true, 2f);

        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawShuang_hu_wai_bai_xian(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 10;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Shuang_hu_wai_bai_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    public void DrawTangGuo(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 0.75f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.TangGuo, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    /*
    public void DrawWai_bai_xian(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 20;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;


        InitLinePos(LineType.Wai_bai_xian, realW, sizeScale, period, begin);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }
    */
    public void DrawZan_xing_xian(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f/180/8;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;


        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Zan_xing_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }
    public void DrawZan_xing_xian2(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 180/4;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2 - realW;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Zan_xing_xian2, realW, sizeScale,
            period, begin, moreParam, FXAddtionalType.FX_NONE, true, 2f);

        ShowLine(offsetX, offsetY);


        InitHZ(sj, false,0,0,-realW/2 / GetFitScreenScale());

    }

    public void DrawZan_xing_xian3(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f / 180/8;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2 - realW / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Zan_xing_xian3, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,0,0,-realW/4 / GetFitScreenScale());
    }

    public void DrawDie_xing_tan_huang(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 360 * _CurrentPeriod;
        float sizeScale = 1f / 5;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2 - 5*realW;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Die_xing_tan_huang, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,0,0,(-2.75f*realW)/GetFitScreenScale());
    }

    public void DrawYan_jian_wei(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 45 * _CurrentPeriod;
        float sizeScale = 1f / 3 * 0.75f;
        float begin = 45;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;


        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        List<Vector3> tmpPos1 = new List<Vector3>();
        //第一段需要清除
        InitLinePos(LineType.Yan_jian_wei, realW, sizeScale, period, begin, moreParam, FXAddtionalType.FX_NONE,false);

        tmpPos1.AddRange(_LineScreenPos);

        //第二段不能清除
        begin = 180;
        period = 45 * _CurrentPeriod;
        List<Vector3> tmpPos2 = new List<Vector3>();
        InitLinePos(LineType.Yan_jian_wei, realW, sizeScale, period, begin, moreParam, FXAddtionalType.FX_NONE, false);
        tmpPos2.AddRange(_LineScreenPos);

        _LineScreenPos.Clear();

        _LineScreenPos.AddRange(tmpPos1);
        _LineScreenPos.AddRange(tmpPos2);

        ShowLine(offsetX, offsetY);
        InitHZ(sj);

    }
    public void DrawTu_zi(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 1f / 8 * 0.75f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Tu_zi, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    public void DrawPalm(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 1f / 7 * 0.75f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Palm, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }
    public void DrawShe_xing_xian(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 0.4f*0.9f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2 + realW;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.She_xing_xian, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        //fuck here
        InitHZ(sj, false,0,0,(realW/2 + _Blank/2)/GetFitScreenScale());
    }

    public void DrawShi_zi_jian_kai(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 0.7f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Shi_zi_jian_kai, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj);
    }
    public void DrawWo_gui_xian(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 0.4f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Wo_gui_xian, realW, sizeScale,
            period, begin,moreParam,FXAddtionalType.FX_NONE,true,2f);

        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    public void DrawHua_hui(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 180 * _CurrentPeriod;
        float sizeScale = 1f/3/180 * 1.4f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Hua_hui, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    public void DrawGou_zi(string sj, int w, int h)
    {

        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 1f/4;
        float begin = 0;

        float offsetX = Screen.width / 2 - realW/4;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Gou_zi, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false,0,-realW/8 / GetFitScreenScale());
    }


    public void DrawFei_ji(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 3f / 8;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Fei_ji, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);

    }

    public void DrawShi_zi(string sj, int w, int h)
    {
        int realW = w - _Blank * 2;
        float period = 1 * _CurrentPeriod;
        float sizeScale = 0.5f;
        float begin = 0;

        float offsetX = Screen.width / 2;
        float offsetY = Screen.height / 2;

        List<float> moreParam = new List<float>();
        moreParam.Add(_CurrentAY);

        InitLinePos(LineType.Shi_zi, realW, sizeScale, period, begin,moreParam);
        ShowLine(offsetX, offsetY);

        InitHZ(sj, false);
    }

    #endregion


    ///--------------------生产icon使用--------------------------
    public void SaveIcon(float w)
    {
        _LineRenderer.startColor = new Color(1,1,1,1);
        _LineRenderer.endColor = new Color(1, 1, 1, 1);
        _FXCamera.backgroundColor = new Color(1,1,1,1);
        _LineRenderer.startWidth = w;
        _LineRenderer.endWidth = w;
        foreach (var hz in _HZList) { hz.SetActive(false); }
    }

    public string GetIconName()
    {
        return "" + _CurrentLineType;
    }
}

