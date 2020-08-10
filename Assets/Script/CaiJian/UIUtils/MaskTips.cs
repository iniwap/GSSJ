using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class MaskTips : MonoBehaviour
{
    public void Start()
    {

    }

    public Image _drag;
    public Image _dragTipes;
    public Text _upText;
    public Text _downText;
    public Text _rightText;
    public Text _leftText;

    public void OnShowMask(bool show){
        gameObject.SetActive(show);
    }

    public  void ShowDragTips(Color BgColor,Action cb)
    {
        Color fc = Define.GetFixColor(BgColor);

        _dragTipes.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
        _drag.color = new Color(fc.r, fc.g, fc.b, 0.0f);
        _upText.color = new Color(fc.r, fc.g, fc.b, 0.0f);
        _downText.color = new Color(fc.r, fc.g, fc.b, 0.0f);
        _rightText.color = new Color(fc.r, fc.g, fc.b, 0.0f);
        _leftText.color = new Color(fc.r, fc.g, fc.b, 0.0f);

        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(_dragTipes.DOFade(150 / 255.0f, 0.3f))
            .Join(_drag.DOFade(1.0f, 0.5f))
            .Join(_upText.DOFade(1.0f, 0.5f))
            .Join(_downText.DOFade(1.0f, 0.5f))
            .Join(_rightText.DOFade(1.0f, 0.5f))
            .Join(_leftText.DOFade(1.0f, 0.5f))
            .SetEase(Ease.OutBounce)
            .AppendInterval(2.5f)
            .Append(_drag.transform.DOScale(1.5f, 1.0f))
            .Join(_drag.DOFade(0.0f, 1.0f))

            .Join(_dragTipes.DOFade(0.0f, 1.0f))

            .Join(_upText.DOFade(0.0f, 1.0f))
            .Join(_upText.transform.DOMoveY(_upText.transform.position.y + 200.0f, 1.0f))

            .Join(_downText.DOFade(0.0f, 1.0f))
            .Join(_downText.transform.DOMoveY(_downText.transform.position.y - 200.0f, 1.0f))

            .Join(_rightText.DOFade(0.0f, 1.0f))
            .Join(_rightText.transform.DOMoveX(_rightText.transform.position.x + 200.0f, 1.0f))

            .Join(_leftText.DOFade(0.0f, 1.0f))
            .Join(_leftText.transform.DOMoveX(_leftText.transform.position.x - 200.0f, 1.0f))

            .SetEase(Ease.InSine)

            .OnComplete(() => {
                this.gameObject.SetActive(false);
                _dragTipes.gameObject.SetActive(false);
                if (cb != null)
                {
                    cb();
                }
            });
    }

    public Image _gestureTips;
    public Image _gesture;
    public Text _gestureText;

    //双指触摸：移动、缩放、旋转
    public void ShowGestureTips(Color BgColor,Action cb)
    {
        Color fc = Define.GetFixColor(BgColor);

        _gestureTips.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
        _gesture.color = new Color(fc.r, fc.g, fc.b, 0f);
        _gestureText.color = fc * 1.1f;
        _gestureText.color = new Color(_gestureText.color.r, _gestureText.color.g, _gestureText.color.b, 0f);
        Vector3 pos = _gesture.transform.localPosition;
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(_gestureTips.DOFade(150 / 255.0f, 0.3f))
            .Join(_gesture.DOFade(1f, 0.5f))
            .Join(_gestureText.DOFade(1f, 0.5f))
            .AppendInterval(1.0f)
            .Append(_gesture.transform.DOScale(1.5f, 1.0f))
            .Join(_gesture.transform.DOLocalRotate(new Vector3(0.0f,0.0f,45.0f),1.0f))
            .Join(_gesture.transform.DOLocalMove(new Vector3(pos.x + 50, pos.y + 50, pos.z), 1.0f))
            .Append(_gesture.transform.DOScale(1.0f, 1.0f))
            .Join(_gesture.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 1.0f))
            .Join(_gesture.transform.DOLocalMove(new Vector3(pos.x, pos.y, pos.z), 1.0f))
            .Append(_gesture.transform.DOScale(0.5f, 1.0f))
            .Join(_gesture.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, -45.0f), 1.0f))
            .Join(_gesture.transform.DOLocalMove(new Vector3(pos.x - 50, pos.y - 50, pos.z), 1.0f))
            .Append(_gesture.transform.DOScale(1.0f, 1.0f))
            .Join(_gesture.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 1.0f))
            .Join(_gesture.transform.DOLocalMove(new Vector3(pos.x, pos.y, pos.z), 1.0f))
            .AppendInterval(1.0f)
            .Append(_gesture.DOFade(0.0f, 1.0f))
            .Join(_gestureText.DOFade(0.0f, 1.0f))
            .Join(_gestureTips.DOFade(0.0f, 1.0f))
            .SetEase(Ease.InSine)
            .OnComplete(() => {
                _gestureTips.gameObject.SetActive(false);
                this.gameObject.SetActive(false);
                if (cb != null)
                {
                    cb();
                }
            });
    }

    public void ShowSaveDone(Color BgColor,Tween t,Action cb = null){

        Color fc = Define.GetFixColor(BgColor);

        this.gameObject.SetActive(true);
        Transform saveDone = this.gameObject.transform.Find("SaveDone");
        Image[] imgs = saveDone.GetComponentsInChildren<Image>();
        saveDone.gameObject.SetActive(true);
        Sequence mySequence = DOTween.Sequence();
        imgs[1].color = fc;
        imgs[2].color = fc;
        imgs[1].transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        imgs[2].transform.localScale = new Vector3(1.0f, 1.0f, 0.0f);
        mySequence
            .Append(imgs[0].DOFade(0.0f, 0.0f))
            .Join(imgs[1].DOFade(0.0f, 0.0f))
            .Join(imgs[2].DOFade(0.0f, 0.0f))
            .Append(t)
            .SetEase(Ease.OutBounce)
            .Join(imgs[0].DOFade(150 / 255.0f, 0.4f))
            .Join(imgs[1].DOFade(1.0f, 0.6f))
            .Join(imgs[1].transform.DOScale(1.0f, 0.3f))
            .AppendInterval(0.5f)
            .Append(imgs[1].transform.DOScale(1.5f, 0.3f))
            .Join(imgs[1].DOFade(0.0f, 0.3f))
            .Join(imgs[2].DOFade(1.0f, 0.8f))
            .SetEase(Ease.InSine)
            .AppendInterval(0.5f)
            .Append(imgs[0].DOFade(0.0f, 0.4f))
            .Join(imgs[2].DOFade(0.0f, 0.4f))
            .Join(imgs[2].transform.DOScale(0.5f, 0.4f))
            .OnComplete(() => {
                //
                this.gameObject.SetActive(false);
                saveDone.gameObject.SetActive(false);
                imgs[1].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                if(cb != null){
                    cb();
                }
            });
    }

    //----------------dialog-------------------------
    public enum eDialogBtnType
    {
        OK,
        CANCEL,
        CLOSE,//任意位置关闭
    }
    public enum eDialogType
    {
        NO_BTN,
        OK_BTN,
        OK_CANCEL_BTN,
    }
    public Image _dialogImg;
    public Image _dialogBg;
    public Image _infoIcon;
    public GameObject _dialog;
    public Transform _DialogContentText;
    public Transform _DialogContent2Text;
    public Text _DialogTitleText;
    public Text _OkBtnText;
    public Text _CancelBtnText;
    public Text _Ok2BtnText;
    public Button _OKBtn;
    public Button _OK2Btn;
    public Button _CancelBtn;
    public Button _clickToCloseBtn;
    private Action<eDialogBtnType> _dialogCallback;
    public struct DialogParam
    {
        public eDialogType type;
        public string content;
        public string title;
        public string okBtn;
        public string cancelBtn;
        public Action<eDialogBtnType> cb;
    }
    public static DialogParam GetDialogParam(string content,
        eDialogType type = eDialogType.NO_BTN,
        Action<eDialogBtnType> cb = null,
        string okBtn = "确定",
        string cancelBtn = "取消",
        string title = "温馨提示")
    {
        DialogParam dp;
        dp.content = content;
        dp.type = type;
        dp.cb = cb;
        dp.okBtn = okBtn;
        dp.cancelBtn = cancelBtn;
        dp.title = title;

        return dp;
    }

    public void ShowDialog(Color bg, DialogParam data)
    {
        if (DOTween.IsTweening("DoShowDialogAni")) return;

        _OkBtnText.text = data.okBtn;
        _Ok2BtnText.text = data.okBtn;
        _CancelBtnText.text = data.cancelBtn;
        _dialogCallback = data.cb;
        _DialogTitleText.text = data.title;

        _dialogBg.color = bg;
        _infoIcon.color = Define.GetDarkColor(Define.GetFixColor(bg));
        Color tc = Define.GetUIFontColorByBgColor(bg, Define.eFontAlphaType.FONT_ALPHA_128);
        _OkBtnText.color = tc;
        _Ok2BtnText.color = tc;
        _CancelBtnText.color = tc;
        _DialogTitleText.color = tc;

        Color bc = new Color(_infoIcon.color.r, _infoIcon.color.g, _infoIcon.color.b, 0.5f);
        _OKBtn.GetComponent<Image>().color = bc;
        _OK2Btn.GetComponent<Image>().color = bc;
        _CancelBtn.GetComponent<Image>().color = bc;

        if (data.type == eDialogType.NO_BTN)
        {
            _OKBtn.gameObject.SetActive(false);
            _CancelBtn.gameObject.SetActive(false);
            _OK2Btn.gameObject.SetActive(false);

            _DialogContentText.gameObject.SetActive(false);
            _DialogContent2Text.gameObject.SetActive(true);
            FitDialogContentText(_DialogContent2Text, data.content);
        }
        else if (data.type == eDialogType.OK_BTN)
        {
            _OKBtn.gameObject.SetActive(false);
            _CancelBtn.gameObject.SetActive(false);
            _OK2Btn.gameObject.SetActive(true);
            _DialogContentText.gameObject.SetActive(true);
            _DialogContent2Text.gameObject.SetActive(false);
            FitDialogContentText(_DialogContentText, data.content);
        }
        else if (data.type == eDialogType.OK_CANCEL_BTN)
        {
            _OKBtn.gameObject.SetActive(true);
            _CancelBtn.gameObject.SetActive(true);
            _OK2Btn.gameObject.SetActive(false);
            _DialogContentText.gameObject.SetActive(true);
            _DialogContent2Text.gameObject.SetActive(false);
            FitDialogContentText(_DialogContentText, data.content);
        }

        DoShowDialogAni(true);
    }

    public void OnHideDialog()
    {
        DoShowDialogAni(false);
    }

    private void EnableBtns(bool enable)
    {
        _OKBtn.interactable = enable;
        _CancelBtn.interactable = enable;
        _OK2Btn.interactable = enable;
        _clickToCloseBtn.interactable = enable;
    }
    private void DoShowDialogAni(bool show)
    {
        if (DOTween.IsTweening("DoShowDialogAni")) return;

        EnableBtns(false);

        Sequence mySequence = DOTween.Sequence();

        if (show)
        {
            gameObject.SetActive(true);
            mySequence.SetId("DoShowDialogAni");
            mySequence
                .Append(_dialogImg.DOFade(0.0f, 0.0f))
                .Join(_dialog.transform.DOScale(0.0f, 0.0f))
                .AppendCallback(() => _dialogImg.gameObject.SetActive(true))
                .Append(_dialogImg.DOFade(100 / 255.0f, 0.3f))
                .Append(_dialog.transform.DOScale(FitUI.GetFitUIScale(), 0.6f))
                .SetEase(Ease.OutBounce)
                .OnComplete(()=> EnableBtns(true));
        }
        else
        {
            mySequence.SetId("DoShowDialogAni");
            mySequence
                .Append(_dialogImg.DOFade(0.0f, 0.3f))
                .Join(_dialog.transform.DOScale(0.0f, 0.3f))
                .SetEase(Ease.InSine).OnComplete(() => {
                    EnableBtns(true);
                    gameObject.SetActive(false);
                    _dialogImg.gameObject.SetActive(false);
                    for (int i = _DialogContentTextList.Count - 1; i >= 0; i--)
                    {
                        Destroy(_DialogContentTextList[i]);
                    }
                    _DialogContentTextList.Clear();
                });
        }
    }

    private List<GameObject> _DialogContentTextList = new List<GameObject>();
    public GameObject _hzPrafab;
    private void FitDialogContentText(Transform content, string str)
    {
        List<string> jsstr = new List<string>();
        foreach (var s in str)
        {
            jsstr.Add("" + s);
        }

        float hzSize = _hzPrafab.GetComponentInChildren<Text>().transform.GetComponent<RectTransform>().rect.width;

        RectTransform jsrt = content.GetComponentInChildren<RectTransform>();
        GridLayoutGroup jsLyt = content.GetComponentInChildren<GridLayoutGroup>();
        float spaceArea = (jsLyt.padding.top + jsLyt.padding.bottom) * jsrt.rect.width;
        float jscellSize = (float)Math.Sqrt(((jsrt.rect.width - hzSize) * (jsrt.rect.height - hzSize) - spaceArea) / jsstr.Count);
        jscellSize = jscellSize > hzSize ? hzSize : jscellSize;
        jsLyt.cellSize = new Vector2(jscellSize, jscellSize);
        Color tc = Define.GetUIFontColorByBgColor(_dialogBg.color, Define.eFontAlphaType.FONT_ALPHA_128);

        for (int i = 0; i < jsstr.Count; i++)
        {
            GameObject txt = Instantiate(_hzPrafab, content) as GameObject;
            txt.SetActive(true);
            txt.GetComponentInChildren<Text>().text = jsstr[i];
            txt.GetComponentInChildren<Text>().color = tc;

            _DialogContentTextList.Add(txt);
        }
    }

    public void OnDialogClickOkBtn()
    {
        if (_dialogCallback != null)
        {
            _dialogCallback(eDialogBtnType.OK);
        }
        DoShowDialogAni(false);
    }

    public void OnDialogClickCancelBtn()
    {
        if (_dialogCallback != null)
        {
            _dialogCallback(eDialogBtnType.CANCEL);
        }

        DoShowDialogAni(false);
    }

    public void OnDialogClickCloseBtn()
    {
        if (_dialogCallback != null)
        {
            _dialogCallback(eDialogBtnType.CLOSE);
        }

        DoShowDialogAni(false);
    }
}
