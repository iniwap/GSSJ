/**************************************/
//FileName: XTHZ.cs
//Author: wtx
//Data: 03/27/2019
//Describe:  选题模式使用的汉字，主要用于生成显示诗句汉字
/**************************************/
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class XTHZ: MonoBehaviour{

    // Use this for initialization
    private int _HZID;
    public  Text _HZText;
    public Text _IDText;//这个id是诗句顺序id，不是方阵索引
    public Image _IDImage;
    private bool _IsSelect;
    private Color _BgDarkColor;
    private Color _BgLightColor;
    private bool _IsTipHZ;//是否是提示字，已经提示的字，不能取消选中

    void Start()
    {

    }

    void OnDestroy()
    {

    }
    void OnEnable()
    {

    }
    void OnDisable()
    {

    }

    public void Update()
    {

    }

    public void InitHZ(int hzId,string hz,Color c){
        _IsTipHZ = false;
        _HZID = hzId;
        _HZText.text = hz;
        //_IDText.text = ""+hzId;
        _IsSelect = false;
        _BgDarkColor = Define.GetFixColor(c * 0.9f);
        _BgLightColor = Define.GetFixColor(c * 1.1f);
        _HZText.color = new Color(_HZText.color.r, _HZText.color.g, _HZText.color.b,0.0f);

        _IDImage.color = new Color(0f, 0f, 0f, 0f);
        _IDText.color = new Color(c.r, c.g, c.b, 0.0f);
    }

    public void SetIsSelect(bool s ,bool needAni = true){
        _IsSelect = s;

        if(!needAni )return;

        if (_IsSelect)
        {
            _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);


            //DOTween.Kill("HZSelect");
            Sequence hzTextAni = DOTween.Sequence();
            hzTextAni.SetId("HZSelect"+ _HZID);
            hzTextAni.Append(_HZText.transform.DOScale(0.8f,0.5f))
                     .Append(_HZText.transform.DOScale(1.2f, 0.5f))
                     .SetEase(Ease.OutBounce)
                     .Append(_HZText.transform.DOScale(1.0f, 0.5f));

            _HZText.DOColor(_BgLightColor, 0.5f);


            _IDImage.DOFade(25/255.0f,0.5f);
            _IDText.DOFade(1.0f,0.5f);
        }
        else{
            DOTween.Kill("HZSelect"+ _HZID);
            _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            _HZText.DOColor(_BgDarkColor, 0.5f);

            _IDImage.DOFade(0f, 0.5f);
            _IDText.DOFade(0f, 0.5f);
        }
    }

    public void UpdateSelectID(int id){
        _IDText.text = "" + id;
    }

    public bool GetIsSelect(){
        return _IsSelect;
    }

    public void RestAni(){
        DOTween.Kill("HZSelect" + _HZID);
        _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (_IsSelect){
            _HZText.color = _BgLightColor;
        }
        else{
            _HZText.color = _BgDarkColor;

        }
    }


    public void ResetHZ(bool onlyStopAni)
    {
        if (onlyStopAni)
        {
            DOTween.Kill("HZSelect" + _HZID);
            return;
        }
        _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Color c = _BgLightColor;
        _HZText.color = new Color(c.r,c.g,c.b,0.0f);

        HideID();
    }

    public void ResetHZ()
    {

        DOTween.Kill("HZSelect" + _HZID);
        _HZText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        _HZText.color = _BgDarkColor;

        HideID();
    }

    //用于结束时的全部隐藏
    public void HideID(bool ani = false){
        if (ani)
        {
            Color c2 = _BgDarkColor;
            _IDImage.color = new Color(0f, 0f, 0f, 0f);
            _IDText.color = new Color(c2.r, c2.g, c2.b, 0.0f);
        }else{
            _IDImage.DOFade(0f, 0.5f);
            _IDText.DOFade(0f, 0.5f);
        }
    }

    public Tween DoHZColor(Color v, float t)
    {
        return _HZText.DOColor(v, t);
    }

    public Tween DoHZFade(float v,float t){
        return _HZText.DOFade(v,t);
    }

    public Tween DoHZIDFade(float v, float t)
    {
        return _IDText.DOFade(v, t);
    }

    public Tween DoHZIDBGFade(float v, float t)
    {
        return _IDImage.DOFade(v, t);
    }

    //这里不处理本身的，为了更好的控制
    void OnTriggerEnter(Collider c)
    {
        //进入触发器执行的代码
    }
    void OnCollisionEnter(Collision collision)
    {
        //进入碰撞器执行的代码

    }

    public string GetHZ(){
        return _HZText.text;
    }
    public int GetHZID(){
        return _HZID;
    }
}
