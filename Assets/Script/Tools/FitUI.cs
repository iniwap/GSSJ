/**************************************/
//FileName: FitUI.cs
//Author: wtx
//Data: 05/06/2018
//Describe:  多分辨适配
/**************************************/
using System;
using UnityEngine;
using DG.Tweening;


public class FitUI: MonoBehaviour{

	[System.Serializable]
	public enum UI_TYPE{
		TOP_MENU,
        MID_MENU,
		BOTTOM_MENU,
        IMG_EFFECT_BOTTOM,
	};

	[System.Serializable]
	public enum PANEL_TYPE{
		MAIN_PANEL,
		SETTING_PANEL,
        MASK_PANEL,
        SPLASH_PANEL,
        LIKE_PANEL,
        DICT_PANEL,
	};

    public bool topNeedFixIphoneX = true;

	public UI_TYPE _uiType;
	public PANEL_TYPE _panelType;


    public GameObject _topMenu;//setting panel need
    public GameObject _bottomMenu;//setting panel need


    // Use this for initialization
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

    public static float DESIGN_WIDTH = 1080.0f;
    public static float DESIGN_HEIGHT = 1920.0f;

    private int WIDTH;
    private int HEIGHT;
    private void OnFitUI(){
        switch(_uiType){
            case UI_TYPE.TOP_MENU:

                float sx = gameObject.transform.GetComponent<RectTransform>().rect.width/ DESIGN_WIDTH;
                gameObject.transform.localScale = new Vector3(sx, sx, gameObject.transform.localScale.z);
                if(getIsIPhoneX())
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                gameObject.transform.position.y - GetOffsetYIphoneX(true),
                                                               gameObject.transform.position.z);
                }
                break;
            case UI_TYPE.MID_MENU:
                if(_panelType == PANEL_TYPE.SETTING_PANEL)
                {
                    float sxb = gameObject.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;

                    gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);


                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y - GetOffsetYIphoneX(true)
                                                                    + _topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb),
                                                                   gameObject.transform.position.z);

                        RectTransform vrt = gameObject.transform.Find("Viewport").GetComponent<RectTransform>();
                        vrt.sizeDelta = new Vector2(vrt.sizeDelta.x, vrt.sizeDelta.y + 230);

                    }
                    else
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y
                                                                    + _topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb),
                                                                    gameObject.transform.position.z);


                        //fuck pad
                        if (GetIsPad())
                        {
                            sxb = 0.9f;
                            gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);

                            float off = 0;
                            if (GetIsBigPad()) off = 20;
                            gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                gameObject.transform.position.y - off,
                                gameObject.transform.position.z);
                        }

                        return;
                    }
                }
                else if (_panelType == PANEL_TYPE.DICT_PANEL)
                {
                    float sxb = gameObject.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;

                    gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);


                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y - GetOffsetYIphoneX(true)
                                                                    + _topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb),
                                                                   gameObject.transform.position.z);

                    }
                    else
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y
                                                                    + _topMenu.GetComponent<RectTransform>().rect.height * (1 - sxb),
                                                                    gameObject.transform.position.z);


                        //fuck pad
                        if (GetIsPad())
                        {
                            sxb = 0.9f;
                            gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);
                        }

                        return;
                    }
                }
                else if(_panelType == PANEL_TYPE.LIKE_PANEL)
                {

                    float th = _topMenu.GetComponent<RectTransform>().rect.height * _topMenu.transform.localScale.y;

                    float sxx = (DESIGN_HEIGHT - th) / gameObject.GetComponent<RectTransform>().rect.height;

                    //0.8f
                    if (getIsIPhoneX())
                    {
                        gameObject.transform.localScale = new Vector3(sxx * 0.8f,
                                                                      sxx * 0.8f, 
                                                                      gameObject.transform.localScale.z);

    

                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                            gameObject.transform.position.y - GetOffsetYIphoneX(true),
                                           gameObject.transform.position.z);

                    }else{

                        gameObject.transform.localScale = new Vector3(sxx, sxx, gameObject.transform.localScale.z);

                        if (GetIsPad())
                        {
                            //
                            float h = gameObject.GetComponent<RectTransform>().rect.height * (1 - sxx);
                            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                                            gameObject.transform.localPosition.y - h,
                                                                            gameObject.transform.localPosition.z);
                        }
                    }


                }
                else if(_panelType == PANEL_TYPE.MAIN_PANEL)
                {
                    //dh->h  Screen
                    float sx1 = GetFitUIScale();
                    gameObject.transform.localScale = new Vector3(sx1, sx1, gameObject.transform.localScale.z);

                    if (GetIsPad())
                    {
                        //
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                                        gameObject.transform.localPosition.y - 50,
                                                                        gameObject.transform.localPosition.z);
                    }

                }
                break;
            case UI_TYPE.BOTTOM_MENU:
                if(_panelType == PANEL_TYPE.MAIN_PANEL)
                {
                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y + GetOffsetYIphoneX(false),
                                                                   gameObject.transform.position.z);
                    }
                }
                else if(_panelType== PANEL_TYPE.SETTING_PANEL || _panelType == PANEL_TYPE.DICT_PANEL)
                {
                    float sxb = gameObject.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;
                    gameObject.transform.localScale = new Vector3(sxb, sxb, gameObject.transform.localScale.z);
                    if (getIsIPhoneX())
                    {
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x,
                                                                    gameObject.transform.position.y + GetOffsetYIphoneX(false),
                                                                   gameObject.transform.position.z);
                    }
                }

                break;
            case UI_TYPE.IMG_EFFECT_BOTTOM:

                float ss = GetFitUIScale();
                gameObject.transform.localScale = new Vector3(ss, ss, gameObject.transform.localScale.z);


                if (GetIsPad())
                {
                    gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                gameObject.transform.localPosition.y - 100,
                                                gameObject.transform.localPosition.z);

                }
                if (getIsIPhoneX())
                {
                    gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                                     gameObject.transform.localPosition.y + GetOffsetYIphoneX(false),
                                                                     gameObject.transform.localPosition.z);
                }
                break;
        }
    }

    public static float GetXScale(GameObject obj){
        return obj.transform.GetComponent<RectTransform>().rect.width / DESIGN_WIDTH;
    }

    public static float GetOffsetYIphoneX(bool top){
        float off = 0;
        if(getIsIPhoneX())
        {
            if(top)
            {
                off =  132 * DESIGN_HEIGHT / Screen.height;
                if (Screen.height < DESIGN_HEIGHT)
                {
                    off *= 2 / 3f  * DESIGN_HEIGHT / 2436f;
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

        float s = Screen.height / DESIGN_HEIGHT;
        float w = s * DESIGN_WIDTH;
        float sx1 = Screen.width / w;

        if(GetIsPad()){
            sx1 *= 0.9f;
        }

        return sx1;
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

    public static bool getIsIPhoneX()
    {
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
        if (GetIsBigPad() || GetIsSmallPad())
        {
            return true;
        }

        return false;
    }

    public static bool GetIsBigPad()
    {
        if ((Screen.width == 1536 && Screen.height == 2048)
            || (Screen.width == 2048 && Screen.height == 2732)
            || (Screen.width == 1668 && Screen.height == 2224))
        {
            return true;
        }

        return false;
    }

    public static bool GetIsSmallPad()
    {
        if ((Screen.width == 768 && Screen.height == 1024)
            || (Screen.width == 640 && Screen.height == 960)
            || (Screen.width == 320 && Screen.height == 480))
        {
            return true;
        }

        return false;
    }

    public static bool Overlaps(RectTransform a, RectTransform b)
    {
        return WorldRect(a).Overlaps(WorldRect(b));
    }
    public static bool Overlaps(RectTransform a, RectTransform b, bool allowInverse)
    {
        return WorldRect(a).Overlaps(WorldRect(b), allowInverse);
    }

    public static Rect WorldRect(RectTransform rectTransform)
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        Vector3 position = rectTransform.position;
        return new Rect(position.x + rectTransformWidth * rectTransform.pivot.x, 
                        position.y - rectTransformHeight * rectTransform.pivot.y, 
                        rectTransformWidth, rectTransformHeight);
    }
}
