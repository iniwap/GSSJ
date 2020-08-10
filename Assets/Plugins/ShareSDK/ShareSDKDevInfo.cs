using UnityEngine;
using System.Collections;
using System;

namespace cn.sharesdk.unity3d 
{
	[Serializable]
	public class DevInfoSet
	{
		public SinaWeiboDevInfo sinaweibo;
		public Facebook facebook;
		public Twitter twitter;
		public Email email;
		public ShortMessage shortMessage;
		public Instagram instagram;

		public WhatsApp whatsApp;
		public Line line;
		public KakaoTalk kakaoTalk;
		public QQ qq;
		public WeChat wechat;
		public WeChatMoments wechatMoments; 

		public Youtube youtube;

		#if UNITY_ANDROID
		public Telegram telegram;
		#elif UNITY_IPHONE		
		public Copy copy;												
		#endif

	}

	public class DevInfo 
	{	
		public bool Enable = true;
	}

	[Serializable]
	public class SinaWeiboDevInfo : DevInfo 
	{
		#if UNITY_ANDROID
		public const int type = (int) PlatformType.SinaWeibo;
		public string SortId = "4";
		public string AppKey = "568898243";
		public string AppSecret = "38a4f8204cc784f81f9f0daaf31e02e3";
		public string RedirectUrl = "http://www.sharesdk.cn";
		public bool ShareByAppClient = false;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.SinaWeibo;
		public string app_key = "568898243";
		public string app_secret = "38a4f8204cc784f81f9f0daaf31e02e3";
		public string redirect_uri = "http://www.sharesdk.cn";
		public string auth_type = "both";	//can pass "both","sso",or "web"  
		#endif
	}

	[Serializable]
	public class QQ : DevInfo 
	{
		#if UNITY_ANDROID
		public const int type = (int) PlatformType.QQ;
		public string SortId = "2";
		public string AppId = "100371282";
		public string AppKey = "aed9b0303e3ed1e27bae87c33761161d";
		public bool ShareByAppClient = true;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.QQ;
		public string app_id = "100371282";
		public string app_key = "aed9b0303e3ed1e27bae87c33761161d";
		public string auth_type = "both";  //can pass "both","sso",or "web" 
		#endif
	}
	
	[Serializable]
	public class WeChat : DevInfo 
	{	
		#if UNITY_ANDROID
		public string SortId = "5";
		public const int type = (int) PlatformType.WeChat;
		public string AppId = "wx4868b35061f87885";
		public string AppSecret = "64020361b8ec4c99936c0e3999a9f249";
		public string UserName = "gh_afb25ac019c9@app";
		public string Path = "/page/API/pages/share/share";
		public bool BypassApproval = true;
		public bool WithShareTicket = true;
		public string MiniprogramType = "0";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WeChat;
		public string app_id = "wx4868b35061f87885";
		public string app_secret = "64020361b8ec4c99936c0e3999a9f249";
		#endif
	}

	[Serializable]
	public class WeChatMoments : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "6";
		public const int type = (int) PlatformType.WeChatMoments;
		public string AppId = "wx4868b35061f87885";
		public string AppSecret = "64020361b8ec4c99936c0e3999a9f249";
		public bool BypassApproval = true;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WeChatMoments;
		public string app_id = "wx4868b35061f87885";
		public string app_secret = "64020361b8ec4c99936c0e3999a9f249";
		#endif
	}
		
	[Serializable]
	public class Facebook : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "8";
		public const int type = (int) PlatformType.Facebook;
		public string ConsumerKey = "1412473428822331";
		public string ConsumerSecret = "a42f4f3f867dc947b9ed6020c2e93558";
		public string RedirectUrl = "https://mob.com/";
		public bool ShareByAppClient = false;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Facebook;
		public string api_key = "107704292745179";
		public string app_secret = "38053202e1a5fe26c80c753071f0b573";
		public string auth_type = "both";  //can pass "both","sso",or "web" 
		public string display_name = "ShareSDK";//如果需要使用客户端分享，必填且需与FB 后台配置一样
		#endif
	}

	[Serializable]
	public class Twitter : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "9";
		public const int type = (int) PlatformType.Twitter;
		public string ConsumerKey = "LRBM0H75rWrU9gNHvlEAA2aOy";
		public string ConsumerSecret = "gbeWsZvA9ELJSdoBzJ5oLKX0TU09UOwrzdGfo9Tg7DjyGuMe8G";
		public string CallbackUrl = "http://mob.com";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Twitter;
		public string consumer_key = "LRBM0H75rWrU9gNHvlEAA2aOy";
		public string consumer_secret = "gbeWsZvA9ELJSdoBzJ5oLKX0TU09UOwrzdGfo9Tg7DjyGuMe8G";
		public string redirect_uri = "http://mob.com";
		#endif
	}

	[Serializable]
	public class Email : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "12";
		public const int type = (int) PlatformType.Mail;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Mail;
		#endif
	}

	[Serializable]
	public class ShortMessage : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "13";
		public const int type = (int) PlatformType.SMS;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.SMS;
		#endif
	}

	[Serializable]
	public class Instagram : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "26";
		public const int type = (int) PlatformType.Instagram;
		public string ClientId = "ff68e3216b4f4f989121aa1c2962d058";
		public string ClientSecret = "1b2e82f110264869b3505c3fe34e31a1";
		public string RedirectUri = "http://sharesdk.cn";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Instagram;
		public string client_id = "ff68e3216b4f4f989121aa1c2962d058";
		public string client_secret = "1b2e82f110264869b3505c3fe34e31a1";
		public string redirect_uri = "http://sharesdk.cn";
		#endif
	}

	[Serializable]
	public class Line : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "30";
		public const int type = (int) PlatformType.Line;
		public string ChannelID = "1477692153";
		public string ChannelSecret = "f30c036370f2e04ade71c52eef73a9af";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Line;
		#endif
	}

	[Serializable]
	public class KakaoTalk : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "31";
		public const int type = (int) PlatformType.KakaoTalk;
		public string AppKey = "48d3f524e4a636b08d81b3ceb50f1003";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.KakaoTalk;
		public string app_key = "48d3f524e4a636b08d81b3ceb50f1003";
		public string rest_api_key = "ac360fa50b5002637590d24108e6cb10";
		public string redirect_uri = "http://www.mob.com/oauth";
		public string auth_type = "both";   //can pass "both","sso",or "web" 
		#endif
	}

	
	[Serializable]
	public class WhatsApp : DevInfo 
	{
		#if UNITY_ANDROID
		public string SortId = "33";
		public const int type = (int) PlatformType.WhatsApp;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.WhatsApp;
		#endif
	}

	[Serializable]
	public class Copy : DevInfo 
	{
		#if UNITY_ANDROID
		public const int type = (int) PlatformType.Copy;
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Copy;
		#endif
	}

	[Serializable]
	public class Youtube : DevInfo
	{
		#if UNITY_ANDROID
		public string SortId = "53";
		public const int type = (int) PlatformType.Youtube;
		public string ClientID = "370141748022-bicrnsjfiije93bvdt63dh3728m4shas.apps.googleusercontent.com";
		public string AppSecret = "AIzaSyAO06g-0TDpHcsXXO918a7QE3Zdct2bB5E";
		public string RedirectUrl="http://localhost";
		public string ShareByAppClient = "true";
		#elif UNITY_IPHONE
		public const int type = (int) PlatformType.Youtube;
		public string client_id = "906418427202-jinnbqal1niq4s8isbg2ofsqc5ddkcgr.apps.googleusercontent.com";
		public string client_secret = "";
		public string redirect_uri = "http://localhost";
		#endif
	}
	
	[Serializable]		
	public class Telegram : DevInfo		
	{		
		#if UNITY_ANDROID		
		public string SortId = "47";		
		public const int type = (int) PlatformType.Telegram;		
		#elif UNITY_IPHONE		
		#endif		
	}

}
