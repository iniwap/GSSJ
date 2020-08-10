using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Like : MonoBehaviour
{
    public enum eLikeType
    {
        eSysColor,
        ePickColor,
    }

    private eLikeType _CurrentLikeType = eLikeType.eSysColor;

    public void Start()
    {
        float s = FitUI.GetFitUIScale();
        if(FitUI.GetIsPad()){
            s *= 0.75f;
        }
        _Likes.transform.localScale = new Vector3(s,s,1.0f);
    }

    public void OnEnable()
    {

    }

    private List<GameObject> _colorCardList = new List<GameObject>();
	public void OnInitPickColorLike(List<PickerColorItem.ColorItem> dt)
	{
        DestroyObj(_colorCardList);
        //色库需要提前初始化
        for (int i = dt.Count - 1; i >= 0; i--)
        {
            GameObject like = Instantiate(_pickColorPrefab, _pickColorContent) as GameObject;
            like.SetActive(true);
            PickColorLikeItem lk = like.GetComponent<PickColorLikeItem>();

            LikeItem.sLikeItem li;
            li.ID = ""+ (dt[i].colorCardId + HZManager.GetInstance().GetColorCnt());
            li.ToAdd = false;
            li.Title = dt[i].colorName;
            li.Content = dt[i].colorDes;
            li.ColorID = (dt[i].colorCardId + HZManager.GetInstance().GetColorCnt());
            li.LikeTime = "";
            lk.Init(li);
            _colorCardList.Add(like);
        }

        FixContentHeight(eLikeType.ePickColor);

    }

    public void OnAddPickColor(PickerColorItem.ColorItem item)
    {

        GameObject like = Instantiate(_pickColorPrefab, _pickColorContent) as GameObject;
        like.SetActive(true);
        PickColorLikeItem lk = like.GetComponent<PickColorLikeItem>();

        LikeItem.sLikeItem li;
        li.ID = "" + (item.colorCardId + HZManager.GetInstance().GetColorCnt());
        li.ToAdd = false;
        li.Title = item.colorName;
        li.Content = item.colorDes;
        li.ColorID = item.colorCardId + HZManager.GetInstance().GetColorCnt();
        li.LikeTime = "";
        lk.Init(li);
        _colorCardList.Add(like);

        //需要将这个添加的放到最前面
        foreach (var tmpcc in _colorCardList)
        {
            tmpcc.transform.SetSiblingIndex(tmpcc.transform.GetSiblingIndex() + 1);
        }

        like.transform.SetSiblingIndex(_colorCardList[0].transform.GetSiblingIndex() - 1);

        FixContentHeight(eLikeType.ePickColor);
    }

    public void OnUpdatePickColor(int colorCardId,string cname,string des,string py)
    {
        int fixColorID = colorCardId + HZManager.GetInstance().GetColorCnt();
        foreach (var cc in _colorCardList)
        {
            PickColorLikeItem lk = cc.GetComponent<PickColorLikeItem>();
            if (lk.GetColorId() == fixColorID)
            {
                lk.UpdateItem(cname,des,py);
                break;
            }
        }

        FixContentHeight(eLikeType.ePickColor);
    }
    public void OnDelPickColor(int colorCardId)
    {
        int fixColorID = colorCardId + HZManager.GetInstance().GetColorCnt();

        for (int i = 0;i< _colorCardList.Count;i++)
        {
            PickColorLikeItem lk = _colorCardList[i].GetComponent<PickColorLikeItem>();
            if (lk.GetColorId() == fixColorID)
            {
                Destroy(_colorCardList[i]);
                _colorCardList.RemoveAt(i);
                break;
            }
        }

        FixContentHeight(eLikeType.ePickColor);
    }

    public GameObject _Likes;
    public Image _TitleBg;
    public Text _limitText;
    public Text _Title;
    public GameObject _likePrefab;
    public GameObject _pickColorPrefab;
    public Transform _sysColorContent;
    public Transform _pickColorContent;
    public GameObject _sysColorView;
    public GameObject _pickColorView;
    private List<GameObject> _LikeList = new List<GameObject>();

    private int _CurrentColorID;
    private string _CurrentTitle;
    private string _CurrentContent;

    public void OnShowLike(eLikeType type,int colorID, string title, string content)
    {
        _CurrentLikeType = type;

        _TitleBg.color = GetColorByID(colorID);//* 0.9f;

        _Title.color = Define.GetUIFontColorByBgColor(_TitleBg.color, Define.eFontAlphaType.FONT_ALPHA_128);
        _limitText.color = Define.GetUIFontColorByBgColor(_TitleBg.color, Define.eFontAlphaType.FONT_ALPHA_128);

        if (_CurrentLikeType == eLikeType.eSysColor)
        {
            _limitText.gameObject.SetActive(true);
            _Title.text = "收藏夹";
            _sysColorView.SetActive(true);
            _pickColorView.SetActive(false);
            ShowSysColorLike(colorID, title, content);
        }
        else if (_CurrentLikeType == eLikeType.ePickColor)
        {
            _limitText.gameObject.SetActive(false);
            _Title.text = "色卡夹";
            _sysColorView.SetActive(false);
            _pickColorView.SetActive(true);
            ShowPickColorLike(colorID, title, content);
        }
    }

    private void ShowPickColorLike(int colorID, string title, string content)
    {
        //已经提前初始化过了
        // FixContentHeight(eLikeType.ePickColor);
        if (_colorCardList.Count == 0)
        {
            ShowToast("色卡夹空空如也，可在【取色器】界面创建、编辑色卡");
        }
    }

    private void ShowSysColorLike(int colorID,string title,string content)
    {
        Init(colorID,title,content);
    }
    private void Init(int colorID, string title, string content,bool needCheckSame = true)
    {
        if (needCheckSame && GetIsCurrent(colorID, title, content))
        {
            FixContentHeight(eLikeType.eSysColor);
            //同一个多次打开
            return;
        }

        DestroyObj(_LikeList);

        string idstr = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.LIKE_ID_LIST, "");
        //已经收藏的
        if (idstr != "")
        {
            string[] ids = idstr.Split('#');
            for (int i = ids.Length - 1; i >= 0; i--)
            {
                string id = ids[i];
                GameObject like = Instantiate(_likePrefab, _sysColorContent) as GameObject;
                like.SetActive(true);
                LikeItem lk = like.GetComponent<LikeItem>();

                LikeItem.sLikeItem li;
                li.ID = id;
                li.ToAdd = false;
                li.Title = Setting.getPlayerPrefs(id + "_Title", "");
                li.Content = Setting.getPlayerPrefs(id + "_Content", "");
                li.ColorID = Setting.getPlayerPrefs(id + "_ColorID", 0);
                li.LikeTime = Setting.getPlayerPrefs(id + "_LikeTime", "");
                lk.Init(li);
                _LikeList.Add(like);
            }
        }

        InitToAdd(colorID, title, content);

        FixContentHeight(eLikeType.eSysColor);
    }

    [Serializable] public class OnLikeItemClickEvent : UnityEvent<eLikeType, LikeItem.sLikeItem> { }
    public OnLikeItemClickEvent OnLikeItemClick;

    public void OnClickItem(string id)
    {
        LikeItem.sLikeItem li = GetLikeItem(id);
        OnLikeItemClick.Invoke(eLikeType.eSysColor, li); 
    }

    public void OnClickItem(LikeItem.sLikeItem item)
    {
        OnLikeItemClick.Invoke(eLikeType.ePickColor, item);
    }

    public void OnClickAddLikeItem(string id,bool del)
    {
        GameObject it = GetLikeItemObj(id);
        LikeItem lk = it.GetComponent<LikeItem>();

        string idstr = Setting.getPlayerPrefs("" + Setting.SETTING_KEY.LIKE_ID_LIST, "");
        string[] ids = idstr.Split('#');

        if (del){
            //删除该条
            if (GetIsCurrent(lk.SLikeItem.ColorID, lk.SLikeItem.Title, lk.SLikeItem.Content))
            {
                lk.SetAddBtn(true);
                lk.transform.SetSiblingIndex(0);//移动到最前面显示

                lk.SetLikeTime("点+添加到收藏");
            }
            else
            {
                _LikeList.Remove(it);
                Destroy(it);
            }
 
            //del
            idstr = "";
            foreach(var iid in ids){
                if(!iid.Equals(id)){
                    idstr += iid + "#";
                }
            }

            if (idstr != "")
            {
                idstr = idstr.Substring(0, idstr.Length - 1);
            }

            Setting.setPlayerPrefs("" + Setting.SETTING_KEY.LIKE_ID_LIST, idstr);
            Setting.delPlayerPrefs(id + "_Title");
            Setting.delPlayerPrefs(id + "_Content");
            Setting.delPlayerPrefs(id + "_ColorID");
            Setting.delPlayerPrefs(id + "_LikeTime");

            _limitText.text = "(" + (ids.Length - 1 )+ "/"+ Define.MAX_LIKE_NUM + ")";

            if(ids.Length - 1 == 9){
                bool exist = CheckIfExist();
                if (!exist){
                    //把当前的加进去
                    Init(_CurrentColorID, _CurrentTitle, _CurrentContent,false);
                }else{
                    FixContentHeight(eLikeType.eSysColor);
                }
            }else{
                FixContentHeight(eLikeType.eSysColor);
            }
        }
        else
        {
            lk.SetLikeTime(Define.GetFmtTime());

            if (idstr == ""){
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.LIKE_ID_LIST,id);
            }
            else{
                Setting.setPlayerPrefs("" + Setting.SETTING_KEY.LIKE_ID_LIST, idstr + "#" + id);
            }

            Setting.setPlayerPrefs(id + "_Title", lk.SLikeItem.Title);
            Setting.setPlayerPrefs(id + "_Content", lk.SLikeItem.Content);
            Setting.setPlayerPrefs(id + "_ColorID", lk.SLikeItem.ColorID);
            Setting.setPlayerPrefs(id + "_LikeTime", lk.SLikeItem.LikeTime);

            if (ids.Length + 1 == Define.MAX_LIKE_NUM)
            {
                _limitText.text = "(已满)";
            }
            else
            {
                if(idstr == ""){
                    _limitText.text = "(" + ids.Length + "/"+ Define.MAX_LIKE_NUM + ")";
                }
                else{
                    _limitText.text = "(" + (ids.Length + 1) + "/" + Define.MAX_LIKE_NUM + ")";
                }
            }
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

    public string GetNewID()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }

    // 不能存在找不到的id，否则出错
    public LikeItem.sLikeItem GetLikeItem(string id){
        LikeItem.sLikeItem li;
        li.ToAdd = true;
        li.ColorID = 0;
        li.Content = "";
        li.ID = "";
        li.LikeTime = "";
        li.Title = "";

        foreach (var it in _LikeList){
            LikeItem lk = it.GetComponent<LikeItem>();
            if(id == lk.SLikeItem.ID){
                li = lk.SLikeItem;
                break;
            }
        }

        return li;
    }

    public GameObject GetLikeItemObj(string id){
        foreach (var it in _LikeList)
        {
            LikeItem lk = it.GetComponent<LikeItem>();
            if (id == lk.SLikeItem.ID)
            {
                return it;
            }
        }

        return null;
    }

    private void InitToAdd(int colorID, string title, string content)
    {
        _CurrentColorID = colorID;
        _CurrentTitle = title;
        _CurrentContent = content;

        if (_LikeList.Count == Define.MAX_LIKE_NUM)
        {
            _limitText.text = "(已满)";
        }
        else
        {
            if (_LikeList.Count == 0)
            {
                _limitText.text = "(0"+ "/" + Define.MAX_LIKE_NUM + ")";
            }
            else
            {
                _limitText.text = "(" + _LikeList.Count + "/" + Define.MAX_LIKE_NUM + ")";
            }
        }

        //已经满了，不能再添加
        if (_LikeList.Count == Define.MAX_LIKE_NUM)
        {
            return;
        }

        if(CheckIfExist()){
            return;
        }

        //待添加的
        GameObject addLike = Instantiate(_likePrefab, _sysColorContent) as GameObject;
        addLike.SetActive(true);
        LikeItem addLk = addLike.GetComponent<LikeItem>();

        LikeItem.sLikeItem li;
        li.ToAdd = true;
        li.ColorID = colorID;
        li.Content = content;
        li.ID = GetNewID();
        li.LikeTime = Define.GetFmtTime();
        li.Title = title;
        addLk.Init(li);
        _LikeList.Insert(0,addLike);
        addLike.transform.SetSiblingIndex(0);
    }

    public Color GetColorByID(int id)
    {

        List<string> color = PickColor.GetColorByID(id);

        Color c = new Color(int.Parse(color[3]) / 255.0f, int.Parse(color[4]) / 255.0f, int.Parse(color[5]) / 255.0f);
        return c;
    }

    private void FixContentHeight(eLikeType type){
        //==0的时候不处理
        if (type == eLikeType.ePickColor)
        {
            if (_colorCardList.Count >= 1)
            {
                //修改content高度
                Vector2 ct = _pickColorContent.GetComponent<RectTransform>().sizeDelta;
                _pickColorContent.GetComponent<RectTransform>().sizeDelta = new Vector2(ct.x, _colorCardList[0].GetComponent<RectTransform>().sizeDelta.y * _colorCardList.Count
                                                                               + _pickColorContent.GetComponent<GridLayoutGroup>().spacing.y * (_colorCardList.Count - 1));
            }
        }
        else if (type == eLikeType.eSysColor)
        {
            if (_LikeList.Count >= 1)
            {
                //修改content高度
                Vector2 ct = _sysColorContent.GetComponent<RectTransform>().sizeDelta;
                _sysColorContent.GetComponent<RectTransform>().sizeDelta = new Vector2(ct.x, _LikeList[0].GetComponent<RectTransform>().sizeDelta.y * _LikeList.Count
                                                                               + _sysColorContent.GetComponent<GridLayoutGroup>().spacing.y * (_LikeList.Count - 1));
            }
        }
    }


    private bool GetIsCurrent(int colorID,string title,string content){
        if (colorID == _CurrentColorID
           // && title.Equals(_CurrentTitle) 不判断标题是否一样，没有意义
            && content.Equals(_CurrentContent))
        {
            return true;
        }

        return false;
    }

    private bool CheckIfExist(){
        bool exist = false;
        foreach (var ili in _LikeList)
        {
            LikeItem lk2 = ili.GetComponent<LikeItem>();
            if (GetIsCurrent(lk2.SLikeItem.ColorID, lk2.SLikeItem.Title, lk2.SLikeItem.Content))
            {
                exist = true;
                break;
            }
        }

        return exist;
    }

    [System.Serializable] public class OnShowToastEvent : UnityEvent<Toast.ToastData> { }
    public OnShowToastEvent OnShowToast;
    public void ShowToast(string content, float showTime = 2.0f)
    {
        ShowToast(content, _TitleBg.color, showTime, 0f);
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
