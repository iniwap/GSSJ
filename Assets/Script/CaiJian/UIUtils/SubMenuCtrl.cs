using UnityEngine;

public class SubMenuCtrl : MonoBehaviour
{
    public enum eSubMenuType
    {
        SubMenu_ZhuangShi,
        SubMenu_FX,
        SubMenu_More,
    }

    public GameObject _fxSubMenu;
    public GameObject _zsSubMenu;
    public GameObject _moreSubMenu;
    public void SubMenuClickToClose()
    {
        _fxSubMenu.SetActive(false);
        gameObject.SetActive(false);
        _zsSubMenu.SetActive(false);
        _moreSubMenu.SetActive(false);
    }

    public void OnClickShowSubMenu(string sm)
    {
        ShowSubMenu(GetSubMenuTypeByName(sm),!gameObject.activeSelf);
    }

    public void ShowSubMenu(eSubMenuType sm,bool show)
    {
        _fxSubMenu.SetActive(false);
        gameObject.SetActive(show);
        _zsSubMenu.SetActive(false);
        _moreSubMenu.SetActive(false);

        switch (sm)
        {
            case eSubMenuType.SubMenu_ZhuangShi:
                _zsSubMenu.SetActive(show);
                break;
            case eSubMenuType.SubMenu_FX:
                _fxSubMenu.SetActive(show);
                break;
            case eSubMenuType.SubMenu_More:
                _moreSubMenu.SetActive(show);
                break;
        }
    }

    private eSubMenuType GetSubMenuTypeByName(string sm)
    {
        eSubMenuType type = eSubMenuType.SubMenu_More;
        switch (sm)
        {
            case "ZhuangShi":
                type = eSubMenuType.SubMenu_ZhuangShi;
                break;
            case "FX":
                type = eSubMenuType.SubMenu_FX;
                break;
            case "More":
                type = eSubMenuType.SubMenu_More;
                break;
        }

        return type;
    }
}
