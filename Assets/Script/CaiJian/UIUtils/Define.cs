//常量定义
using System;
using System.Net;
using UnityEngine;

public static class Define
{
    public enum SWIPE_TYPE{
        LEFT,
        RIGHT,
        UP,
        DOWN,
    }

    public static int SHIJU_FONTSIZE = 64;
    public static int AUTHOR_FONTSIZE = 48;

    public static Color BG_COLOR_50 = new Color(0f,0f,0f,50/255.0f);
    public static Color BG_COLOR_100 = new Color(0f, 0f, 0f, 100 / 255.0f);

    //背景明暗色的时候行分割线的颜色，这里用于默认情况
    public static Color DARKBG_SP_COLOR = new Color(20 / 255.0f, 20 / 255.0f, 20 / 255.0f, 200/255.0f);
    public static Color LIGHTBG_SP_COLOR = new Color(125 / 255.0f, 18 / 255.0f, 18 / 255.0f, 1.0f);

    public enum eFontAlphaType{
        FONT_ALPHA_255,
        FONT_ALPHA_200,
        FONT_ALPHA_128,
        FONT_ALPHA_50,
        FONT_ALPHA_0,
    }

    public static int MAX_LIKE_NUM = 10;

    public static int BASE_SCORE_10S = 10;//答题时间10s的基础分为10分
    public static int PLUS_SCORE_ALL = 3;
    public static int PLUS_SCORE_TANGSHI = 0;
    public static int PLUS_SCORE_SONGCI = 1;
    public static int PLUS_SCORE_GUSHI = 1;
    public static int PLUS_SCORE_SHIJING = 2;

    //排序模式，题目范围额外加分和其他模式不同
    public static int PX_PLUS_SCORE_TANGSHI = 1;
    public static int PX_PLUS_SCORE_SONGCI = 2;
    public static int PX_PLUS_SCORE_SHIJING = 0;

    public static int FREEUSE_DAAN_PER_DAY = 5;

    public static float PAI_XU_HZ_DOWNLEFT_SPEED = 1.0f;
    public static float PAI_XU_HZ_MOVECENTER_SPEED = 4.0f;

    public static int MAX_XT_HZ_MATRIX = 8;

    public static int XT_NO_TIP = 50;

    public static int BASIC_TIP_CNT = 3;

    public static int DEFAULT_START_COLORID = 283;

    public static int DEFAULT_HS_SIZE = 6;
    public static int MIN_DEFAULT_HS_SIZE = 0;//->0
    public static int MAX_DEFAULT_HS_SIZE = 120;//->2
    public static float MIN_LINE_HZ_ALPHA = 0.1f;//曲线汉字渐变最低透明度0.1
    public static float MIN_LINE_HZ_SIZE = 0.2f;//曲线汉字渐变最低大小0.2

    public static int MAX_FX_HZ_NUM = 32;//最多支持32字

    public static int MAX_FREE_PICK_COLOR_NUM = 20;//免费最大可保存色库颜色数
    public static int MAX_BUY_PICK_COLOR_NUM = 200;//最大可保存色库颜色数


    public static string PICK_COLOR_TABLE_NAME = "pickcolor";//色库颜色表名

    public static Color GetLightColor(Color c,bool needAlpha = false){
        Color lc = c * 1.1f;

        if(lc.r > 1.0f && lc.g > 1.0f && lc.b > 1.0f)
        {
            lc = new Color(0.96f, 0.96f, 0.96f, lc.a);
        }

        if(!needAlpha){
            lc = new Color(lc.r, lc.g, lc.b, 1.0f);
        }

        return lc;
    }

    public static Color GetDarkColor(Color c, bool needAlpha = false)
    {
        Color lc = c * 0.9f;
        if (!needAlpha)
        {
            lc = new Color(lc.r, lc.g, lc.b, 1.0f);
        }
        return lc;
    }

    public static Color GetUIFontColorByBgColor(Color bgColor, eFontAlphaType fa)
    {
        Color c = Color.black;

        float l = 0.3f * bgColor.r + 0.6f * bgColor.g + 0.1f * bgColor.b;

        float a = 1.0f;
        switch(fa){
            case eFontAlphaType.FONT_ALPHA_0:
                a = 0.0f;
                break;
            case eFontAlphaType.FONT_ALPHA_128:
                a = 0.5f;
                break;
            case eFontAlphaType.FONT_ALPHA_200:
                a = 200/255.0f;
                break;
            case eFontAlphaType.FONT_ALPHA_255:
                a = 1.0f;
                break;
            case eFontAlphaType.FONT_ALPHA_50:
                a = 50/255.0f;
                break;
        }


        //进一步修正alpha
        float b = GetBrightness(bgColor);
        if(b > 100.0f / 255.0f && b < 150.0f / 255.0f){
            a = b + 50 / 255.0f;
        }

        //亮色的时候使用黑色字体
        if(GetIfUIFontBlack(bgColor))
        {
            c = new Color(50 / 255.0f, 50 / 255.0f, 50 / 255.0f, a);
        }else{
            c = new Color(200 / 255.0f, 200 / 255.0f, 200 / 255.0f, a);
        }

        return c;
    }

    public static float GetBrightness(Color bgColor)
    {
        float l = 0.3f * bgColor.r + 0.6f * bgColor.g + 0.1f * bgColor.b;
        return l;
    }

    public static bool GetIfUIFontBlack(Color bgColor)
    {
        if(GetBrightness(bgColor) > 100/255.0f){//亮度低于100才使用白色字体，否则有些刺眼

            return true;
        }

        return false;
    }

    public static Color GetFixColor(Color c){
        Color bc = c;
        float off = 100 / 255.0f;
        if (GetBrightness(c) < off)
        {
            bc = new Color(c.r + off / 4, c.g + off / 4, c.b + off / 4, c.a);
        }

        return bc;
    }
    /// <summary>
    /// 获取网络日期时间
    /// </summary>
    /// <returns></returns>
    public static void GetNetDateTime(Action <string> cb)
    {
        WebRequest request = null;
        WebResponse response = null;
        WebHeaderCollection headerCollection = null;
        string datetime = string.Empty;
        try
        {
            request = WebRequest.Create("http://www.baidu.com");
            request.Timeout = 3000;
            //request.UseDefaultCredentials = true;
            //request.Credentials = CredentialCache.DefaultCredentials;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;

            response = request.GetResponse();
            headerCollection = response.Headers;
            foreach (var h in headerCollection.AllKeys)
            { 
                if (h == "Date") 
                { 
                    datetime = headerCollection[h]; 
                } 
            }
            cb(datetime);
        }
        catch (Exception) 
        {
            cb(datetime); 
        }
        finally
        {
            if (request != null)
            { 
                request.Abort(); 
            }
            if (response != null)
            { 
                response.Close();
            }
            if (headerCollection != null)
            { 
                headerCollection.Clear(); 
            }
        }
    }

    public enum eWeekType
    {
        Custom,//自定义
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,

        //放到最后
        Dynamic,
    }

    public static eWeekType GetWeek()
    {
        return GetWeek(DateTime.Now.DayOfWeek);
    }

    public static eWeekType GetWeek(DayOfWeek day)
    {
        eWeekType week = eWeekType.Sunday;
        switch ((int)day)
        {
            case 0:
                week = eWeekType.Sunday;
                break;
            case 1:
                week = eWeekType.Monday;
                break;
            case 2:
                week = eWeekType.Tuesday;
                break;
            case 3:
                week = eWeekType.Wednesday;
                break;
            case 4:
                week = eWeekType.Thursday;
                break;
            case 5:
                week = eWeekType.Friday;
                break;
            case 6:
                week = eWeekType.Saturday;
                break;
        }
        return week;
    }

    public static string GetWeekName(eWeekType wk)
    {
        string week = "自定义";
        switch (wk)
        {
            case eWeekType.Monday:
                week = "星期一";
                break;
            case eWeekType.Tuesday:
                week = "星期二";
                break;
            case eWeekType.Wednesday:
                week = "星期三";
                break;
            case eWeekType.Thursday:
                week = "星期四";
                break;
            case eWeekType.Friday:
                week = "星期五";
                break;
            case eWeekType.Saturday:
                week = "星期六";
                break;
            case eWeekType.Sunday:
                week = "星期天";
                break;
            case eWeekType.Custom:
                week = "自定义";
                break;
        }
        return week;
    }
    public enum eHourType
    {
        None,//自定义
        Morning,
        Day,
        Sunset,
        Night,
    }
    public static eHourType GetHourType()
    {
        string dt = DateTime.Now.ToString("HH:mm:ss");
        int th = int.Parse(dt.Split(':')[0]);
        eHourType hour = eHourType.None;
        if (th > 5 && th <= 8)
        {
            hour = eHourType.Morning;
        }
        else if (th > 8 && th <= 15)
        {
            hour = eHourType.Day;
        }
        else if (th > 15 && th <= 18)
        {
            hour = eHourType.Sunset;
        }
        else if ((th > 18 && th < 24) || (th >= 0 && th <= 5))
        {
            hour = eHourType.Night;
        }

        return hour;
    }

    public static string GetHourName(eHourType hr,bool more = false)
    {
        string hrname = "";
        switch (hr)
        {
            case eHourType.Morning:
                if (more)
                {
                    hrname = "清晨·早上";
                }
                else
                {
                    hrname = "早晨";
                }
                break;
            case eHourType.Day:
                if (more)
                {
                    hrname = "中午·下午";
                }
                else
                {
                    hrname = "白天";
                }
                break;
            case eHourType.Sunset:
                if (more)
                {
                    hrname = "黄昏·傍晚";
                }
                else
                {
                    hrname = "傍晚";
                }
                break;
            case eHourType.Night:
                if (more)
                {
                    hrname = "晚上·夜间";
                }
                else
                {
                    hrname = "夜里";
                }
                break;
        }
        return hrname;
    }

    public static string GetFmtTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
    }

    public static Color GetUnityColor(int r,int g,int b,int a = 255)
    {
        return new Color(r/255f,g/255f,b/255f,a/255f);
    }
    public static int GetIntColor(float c)
    {
        return (int)(c * 255);
    }
    public static string GetHexColor(int r,int g,int b)
    {
        Color c = GetUnityColor(r,g,b);
        return GetHexColor(c);
    }
    public static string GetHexColor(Color c)
    {
        return "#" + GetIntColor(c.r).ToString("X2") + GetIntColor(c.g).ToString("X2") + GetIntColor(c.b).ToString("X2");
    }

    public static bool CheckColorEq(Color c1,Color c2)
    {
        if ((int)(c1.r * 255) == (int)(c2.r * 255)
            && (int)(c1.g * 255) == (int)(c2.g * 255)
            && (int)(c1.b * 255) == (int)(c2.b * 255))
        {
            return true;
        }

        return false;
    }
}
