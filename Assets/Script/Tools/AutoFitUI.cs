/**************************************/
//FileName: AutoFitUI.cs
//Author: wtx
//Data: 07/28/2019
//Describe:  多分辨适配
//用于顶部底部充满，中间区域可以等比缩小的适配
/**************************************/
using UnityEngine;


public class AutoFitUI: MonoBehaviour{

	[System.Serializable]
	public enum eAreaType{
        AREA,
		TOP_MENU,
        MID_MENU,
		BOTTOM_MENU,
	};

    public enum ePanelType
    {
        PANEL,
        SPLASH_PANEL,
        MAIN_PANEL,
        GAME_PANEL,
        SHOP_PANEL,
        HELP_PANEL,
        DICT_PANEL,
        PICK_COLOR_PANEL,
    }

    public bool topNeedFixIphoneX = false;

	public eAreaType _areaType = eAreaType.AREA;
    public ePanelType _panelType = ePanelType.PANEL;
    
    public GameObject _topMenu;//顶部区域，需要填充屏幕
    public GameObject _midMenu;//中心区域可以等比例缩放
    public GameObject _bottomMenu;//底部区域需要填充屏幕


    // Use this for initialization
    public static float DESIGN_WIDTH = 1080.0f;
    public static float DESIGN_HEIGHT = 1920.0f;

    private int WIDTH;
    private int HEIGHT;

    void Start () {
        WIDTH = Screen.width;
        HEIGHT = Screen.height;
        OnFitUI();
	}

	void OnDestroy(){
		
	}
	void OnEnable(){

	}
	void OnDisable(){
		
	}

	public void Update()
	{

	}

    private Vector3 _UIPos;
    private void OnFitUI(){
        float CurrentWidth = DESIGN_WIDTH * HEIGHT / DESIGN_HEIGHT;
        float s = WIDTH / CurrentWidth;

        RectTransform trt = _topMenu.GetComponent<RectTransform>();
        RectTransform brt = _bottomMenu.GetComponent<RectTransform>();
        RectTransform mrt = _midMenu.GetComponent<RectTransform>();

        switch (_areaType)
        {
            case eAreaType.TOP_MENU:
                if (topNeedFixIphoneX)
                {
                    _topMenu.transform.position = new Vector3(_topMenu.transform.position.x,
                                                              _topMenu.transform.position.y - GetOffsetYIphoneX(true),
                                                             _topMenu.transform.position.z);
                }

                _topMenu.transform.localScale = new Vector3(s,s,1.0f);
                _UIPos = _topMenu.transform.localPosition;
                break;
            case eAreaType.MID_MENU:

                int plus = 1;
                if(_panelType == ePanelType.MAIN_PANEL || _panelType == ePanelType.GAME_PANEL)
                {
                    plus = GetIsPad() ? 0 : 1;
                }

                //高度需要先去掉上下区域 缩放后的高度
                float tbH = (trt.rect.height + brt.rect.height* plus) * s;
                if(topNeedFixIphoneX)
                {
                    tbH += GetOffsetYIphoneX(true);
                }

                float leftH = DESIGN_HEIGHT - tbH;

                float sh = leftH / mrt.rect.height;
                float sw = WIDTH / CurrentWidth;

                float sm = Mathf.Min(sh,sw);

                _midMenu.transform.localScale = new Vector3(sm, sm, 1.0f);

                //iphone x的时候需要调整中间区域的大小和位置
                if (getIsIPhoneX())
                {
                    float h = (leftH - leftH * sw)/2;
                    if (_panelType == ePanelType.HELP_PANEL)
                    {
                        Transform mige = _midMenu.transform.Find("Info");
                        Transform contact = _midMenu.transform.Find("Contact");
                        contact.transform.localPosition = new Vector3(contact.transform.localPosition.x,
                                                                  contact.transform.localPosition.y - h,
                                                                  0);

                        RectTransform mgrt = mige.GetComponent<RectTransform>();
                        mgrt.sizeDelta = new Vector2(mgrt.sizeDelta.x, mgrt.sizeDelta.y + 2 * h);

                        //fuck ipxs
                        _midMenu.transform.position = new Vector3(_midMenu.transform.position.x,
                                                                      _midMenu.transform.position.y - h / 2 + 50,
                                                                      _midMenu.transform.position.z);

                    }
                    else if (_panelType == ePanelType.SHOP_PANEL)
                    {
                        RectTransform ptrt = _midMenu.transform.Find("Product").GetComponent<RectTransform>();
                        ptrt.sizeDelta = new Vector2(ptrt.sizeDelta.x, ptrt.sizeDelta.y + 2 * h);

                        _midMenu.transform.position = new Vector3(_midMenu.transform.position.x,
                                              _midMenu.transform.position.y - 30,
                                              _midMenu.transform.position.z);
                    }
                    else if (_panelType == ePanelType.MAIN_PANEL)
                    {
                        //IPX重新设置mainmenu位置
                        Transform mainMenu = _midMenu.transform.Find("MainMenu");
                        float MH = _midMenu.GetComponent<RectTransform>().rect.height;
                        mainMenu.transform.localPosition = new Vector3(0, mainMenu.transform.localPosition.y - (1 - GetFitUIScale()) * MH / 4, 0);
                    }
                    else if (_panelType == ePanelType.DICT_PANEL)
                    {
                        //IPX重新设置mainmenu位置
                        //Transform mainMenu = _midMenu.transform.Find("MainMenu");
                        //float MH = _midMenu.GetComponent<RectTransform>().rect.height;
                        //mainMenu.transform.localPosition = new Vector3(0, mainMenu.transform.localPosition.y - (1 - GetFitUIScale()) * MH / 4, 0);
                    }
                    else if (_panelType == ePanelType.PICK_COLOR_PANEL)
                    {
                        RectTransform ptrt = _midMenu.transform.Find("ColorStorage").GetComponent<RectTransform>();
                        ptrt.sizeDelta = new Vector2(ptrt.sizeDelta.x, ptrt.sizeDelta.y + 2 * h);

                        _midMenu.transform.position = new Vector3(_midMenu.transform.position.x,
                                              _midMenu.transform.position.y - 20,
                                              _midMenu.transform.position.z);
                    }
                }

                _UIPos = _midMenu.transform.localPosition;

                break;
            case eAreaType.BOTTOM_MENU:
                if (topNeedFixIphoneX)
                {
                    _bottomMenu.transform.position = new Vector3(_bottomMenu.transform.position.x,
                                                              _bottomMenu.transform.position.y + GetOffsetYIphoneX(false) / 2,
                                                             _bottomMenu.transform.position.z);
                }
                _bottomMenu.transform.localScale = new Vector3(s, s, 1.0f);

                _UIPos = _bottomMenu.transform.localPosition;
                break;
        }
    }

    public Vector3 GetUIPos()
    {
        return _UIPos;
    }

    public static float GetXScale(GameObject obj){
        return obj.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;
    }

    public static float GetOffsetYIphoneX(bool top)
    {
        float off = 0;
        if (getIsIPhoneX())
        {
            if (top)
            {
                off = 132 * DESIGN_HEIGHT / Screen.height;
                if (Screen.height < DESIGN_HEIGHT)
                {
                    off *= 2 / 3f * DESIGN_HEIGHT / 2436f;
                }
            }
            else
            {
                off = 102 * DESIGN_HEIGHT / Screen.height;
                if (Screen.height < DESIGN_HEIGHT)
                {
                    off *= 2 / 3f * DESIGN_HEIGHT / 2436f;
                }
            }
        }

        return off;
    }

    public static float GetFitUIScale(){

        float CurrentWidth = DESIGN_WIDTH * Screen.height / DESIGN_HEIGHT;
        float s = Screen.width / CurrentWidth;

        return s;
    }

    public static bool GetIsIphoneXByScreenSize()
    {
        if ((Screen.width == 1125 && Screen.height == 2436)
            || (Screen.width == 828 && Screen.height == 1792)
            || (Screen.width == 1242 && Screen.height == 2688))
        {
            return true;
        }

        return false;
    }

	public static bool getIsIPhoneX(){

		if (SystemInfo.deviceModel.Contains("iPhone10,3") 
            || SystemInfo.deviceModel.Contains("iPhone10,6")
            || SystemInfo.deviceModel.Contains("iPhone11,2")
           || SystemInfo.deviceModel.Contains("iPhone11,4")
           || SystemInfo.deviceModel.Contains("iPhone11,6")
           || SystemInfo.deviceModel.Contains("iPhone11,8")
           || GetIsIphoneXByScreenSize())
        {
			return true;
		}

		return false;
	}

    public static bool GetIsPad(){
        if ((Screen.width == 768 && Screen.height == 1024)
            || (Screen.width == 1536 && Screen.height == 2048)
            || (Screen.width == 640 && Screen.height == 960)
            || (Screen.width == 320 && Screen.height == 480)
            || (Screen.width == 2048 && Screen.height == 2732))
        {
            return true;
        }

        return false;
    }
}
