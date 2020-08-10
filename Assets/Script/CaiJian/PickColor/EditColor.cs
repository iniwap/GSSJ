/*
 *用于控制处理和装饰有关的逻辑
 *
 */
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.Text.RegularExpressions;

public class EditColor : MonoBehaviour
{
    public void Start()
    {

    }

    public void Init()
    {
        _OriPos = gameObject.transform.position;
    }

    public GameObject _editColorDialog;
    public Image _Mask;
    public Button _clickToCloseBtn;
    public Image _Bg;

    #region  设置参数调整界面的相关参数

    #endregion

    //在显示调节界面前，需要设置以上参数，否则呈现效果不正确
    public GameObject _colorBtn;//色库按钮位置
    private Vector3 _OriPos;
    private PickerColorItem.ColorItem _CurrentColorItem;
    private Action _CloseEditColorCb = null;
    public GameObject _updateBtn;
    public GameObject _fullCloseBtn;
    public GameObject _useBtn;
    public GameObject _saveBtn;
    public GameObject _sp;//分割线
    public void ShowEditColor(PickerColorItem.ColorItem item,bool full,Action closeCb = null)
    {
        _CurrentColorItem = item;
        _CloseEditColorCb = closeCb;
        //设置编辑界面相关参数
        _Bg.color = _CurrentColorItem.color;

        Text[] allText = gameObject.GetComponentsInChildren<Text>(true);
        Color textColor = Define.GetUIFontColorByBgColor(_Bg.color, Define.eFontAlphaType.FONT_ALPHA_128);
        Color c1 = new Color(textColor.r, textColor.g, textColor.b, 50 / 255f);

        Color fc = Define.GetFixColor(_Bg.color);

        Image[] allImg = gameObject.GetComponentsInChildren<Image>(true);

        foreach (var t in allText)
        {
            if (t.name.Contains("ECText"))
            {
                t.color = textColor;
            }
            else if (t.name == "Placeholder")
            {
                t.color = c1;
            }
        }

        foreach (var img in allImg)
        {
            if (img.name.Contains("ECImg"))
            {
                img.color = fc;
            }
        }

        //色库已满，如果不存在则不能保存，已经存在可以更新
        UpdateFullColseBtn(full);

        if (IAP.getHasBuy(IAP.IAP_COLORSTORAGE))
        {
            _fullCloseBtn.GetComponentInChildren<Text>().text = "色库已满，请先删除再创建";
        }
        else
        {
            _fullCloseBtn.GetComponentInChildren<Text>().text = "色库已满，是否<b>购买扩容</b>至"+Define.MAX_BUY_PICK_COLOR_NUM+"？";
        }

        UpdateParam();

        ShowEditColor(true);
    }
    public void UpdateFullColseBtn(bool full)
    {
        if (_CurrentColorItem.isColorCard)
        {
            _updateBtn.SetActive(true);

            //隐藏
            _fullCloseBtn.SetActive(false);
            _useBtn.SetActive(false);
            _saveBtn.SetActive(false);
            _sp.SetActive(false);
        }
        else
        {
            _updateBtn.SetActive(false);
            if (full && !_CurrentColorItem.hasExist)
            {
                //隐藏
                _fullCloseBtn.SetActive(true);
                _useBtn.SetActive(false);
                _saveBtn.SetActive(false);
                _sp.SetActive(false);
            }
            else
            {
                _fullCloseBtn.SetActive(false);
                _useBtn.SetActive(true);
                _saveBtn.SetActive(true);
                _sp.SetActive(true);
            }
        }
    }

    public Text _ColorNameText;
    public Text _ColorPYText;
    public Text _ColorDesText;
    public Text _ColorRGBText;
    public Text _ColorHEXText;


    public Text _UseSaveBtnText;
    public Text _SaveBtnText;
    public Image _SaveBtnImg;
    public InputField _PCNameInput;
    public InputField _PCPYInput;
    public InputField _PCDesInput;
    private void UpdateParam()
    {
        Color c = _CurrentColorItem.color;
        _ColorRGBText.text = "" + Define.GetIntColor(c.r) + "," + Define.GetIntColor(c.g) + "," + Define.GetIntColor(c.b);
        _ColorHEXText.text = Define.GetHexColor(c);

        _PCNameInput.SetTextWithoutNotify(_CurrentColorItem.colorName);
        _PCPYInput.SetTextWithoutNotify(_CurrentColorItem.py);
        _PCDesInput.SetTextWithoutNotify(_CurrentColorItem.colorDes);

        //按钮显示内容
        if (_CurrentColorItem.hasExist)
        {
            _UseSaveBtnText.text = "更新并使用";
            _SaveBtnText.text = "仅更新";
            _SaveBtnImg.sprite = Resources.Load("icon/check", typeof(Sprite)) as Sprite;
        }
        else
        {
            _UseSaveBtnText.text = "创建并使用";
            _SaveBtnText.text = "仅创建";
            _SaveBtnImg.sprite = Resources.Load("icon/pushpin", typeof(Sprite)) as Sprite;
        }
    }

    private void ShowEditColor(bool show,bool needAddAni = false,Action finishCB = null)
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
                .SetEase(Ease.OutBounce)
                .OnComplete(()=>finishCB?.Invoke());
        }
        else
        {
            _clickToCloseBtn.interactable = false;

            mySequence
                .Append(_Mask.DOFade(0.0f, 0.3f))
                .Join(gameObject.transform.DOScale(0.0f, 0.3f));

            //需要添加到色库动画，也就是新增色卡
            if (needAddAni)
            {
                mySequence.Join(gameObject.transform.DOMove(_colorBtn.transform.position, 0.3f));
            }

            mySequence
                .SetEase(Ease.InSine).OnComplete(() =>
                {
                    gameObject.SetActive(true);
                    _clickToCloseBtn.interactable = true;
                    _Mask.gameObject.SetActive(true);
                    _editColorDialog.SetActive(false);

                    gameObject.transform.position = _OriPos;
                    gameObject.transform.localScale = Vector3.one;

                    _CloseEditColorCb?.Invoke();
                    finishCB?.Invoke();
                });
        }
    }

    [Serializable] public class OnUsePickColorItemEvent : UnityEvent<PickerColorItem.ColorItem> { }
    public OnUsePickColorItemEvent OnUsePickColorItem;
    private void UseSaveUpdate(bool add,bool use = false,Action finishCB = null)
    {
        if (!CheckValid())
        {
            return;
        }

        SetParam();
        OnSavePickColorItem.Invoke(_CurrentColorItem);

        if (!use)
        {
            if (!add && finishCB == null)
            {
                ShowToast("色卡信息更新成功:)");
            }
            else
            {
                ShowToast("创建色卡成功，可到色库查看、编辑");
            }
        }

        ShowEditColor(false, add, finishCB);
    }
    public void OnUseBtnClick()
    {
        //这里可能是添加或者更新
        UseSaveUpdate(!_CurrentColorItem.hasExist,true, ()=> {
            //保存更新
            //使用
            OnUsePickColorItem.Invoke(_CurrentColorItem);
        });
    }

    public void OnSaveBtnClick()
    {
        //可能是添加或者更新
        UseSaveUpdate(!_CurrentColorItem.hasExist);
    }
    public void OnUpdateBtnClick()
    {
        //只能是更新，不需要添加动画
        UseSaveUpdate(false);
    }
    public void OnClickToCloseClick()
    {
        ShowEditColor(false, false);
    }
    private bool CheckValid()
    {
        bool v = true;

        if (_ColorNameText.text.Length == 0)
        {
            ShowToast("为了更好的展示效果，名字不能为空");
            _PCNameInput.transform.DOShakeScale(1.0f, 0.2f);
            return false;
        }

        if (_ColorPYText.text.Length == 0)
        {
            ShowToast("为了更好的展示效果，拼音不能为空");
            _PCPYInput.transform.DOShakeScale(1.0f, 0.2f);
            return false;
        }

        //合法性检测
        Regex regChina = new Regex("^[^\x00-\xFF]");
        Regex regEnglish = new Regex("^[a-zA-Z]");

        if (!regEnglish.IsMatch(_ColorPYText.text))
        {
            ShowToast("请输入拼音，仅包含英文字母");//这里不确定范围，只提示
            v = false;
            _PCPYInput.transform.DOShakeScale(1.0f,0.2f);
        }

        if (!regChina.IsMatch(_ColorNameText.text))
        {
            ShowToast("请输入中文名字，仅包含汉字");//这里不确定范围，只提示
            v = false;
            _PCNameInput.transform.DOShakeScale(1.0f, 0.2f);
        }

        return v;
    }

    //设置参数
    private void SetParam()
    {
        if (_ColorNameText.text.Length > 5)
        {
            _CurrentColorItem.colorName = _ColorNameText.text.Substring(0, 5);
        }
        else
        {
            if (_ColorNameText.text.Length != 0)
            {
                _CurrentColorItem.colorName = _ColorNameText.text;
            }
            else
            {
                _CurrentColorItem.colorName = "";//可以添加默认值
            }
        }

        if (_ColorDesText.text.Length > 32)
        {
            _CurrentColorItem.colorDes = _ColorDesText.text.Substring(0, 32);
        }
        else
        {
            if (_ColorDesText.text.Length != 0)
            {
                _CurrentColorItem.colorDes = _ColorDesText.text;
            }
            else
            {
                _CurrentColorItem.colorDes = "";//可以添加默认值
            }
        }

        if (_ColorPYText.text.Length > 30)
        {
            _CurrentColorItem.py = _ColorPYText.text.Substring(0, 30);
        }
        else
        {
            if (_ColorPYText.text.Length != 0)
            {
                _CurrentColorItem.py = _ColorPYText.text;
            }
            else
            {
                _CurrentColorItem.py = "N/A";//可以添加默认值
            }
        }

        //保存为大写字母
        _CurrentColorItem.py = _CurrentColorItem.py.ToUpper();
    }

    [Serializable] public class OnSavePickColorItemEvent : UnityEvent<PickerColorItem.ColorItem> { }
    public OnSavePickColorItemEvent OnSavePickColorItem;

    [System.Serializable] public class OnShowToastEvent : UnityEvent<string,float> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f)
    {
        OnShowToast.Invoke(content, showTime);
    }

    public UnityEvent OnBuyColorStorage;
    public void OnClickCloseFullBtn()
    {
        if (!IAP.getHasBuy(IAP.IAP_COLORSTORAGE))
        {
            ShowToast("正在处理购买，请稍后...");
            OnBuyColorStorage.Invoke();
        }
        else
        {
            ShowEditColor(false, false);
        }
    }
}
