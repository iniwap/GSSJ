
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;



namespace Umeng
{

    public class DplusAgent 
    {
		
#if UNITY_ANDROID
        static AndroidJavaObject _DplusAgent = new AndroidJavaClass("com.umeng.analytics.dplus.UMADplus");
        static AndroidJavaObject Context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        static AndroidJavaClass  UnityUtil = new AndroidJavaClass("com.umeng.analytics.UnityUtil");
#endif



        //iOS Android Universal API



        public static void track(String eventName)
        {

#if UNITY_EDITOR

#elif UNITY_IPHONE
             _track(eventName);
#elif UNITY_ANDROID
            _DplusAgent.CallStatic("track", Context, eventName);
#endif


        }


        public static void track(String eventName, JSONObject jsonObject )
        {
#if UNITY_EDITOR

#elif UNITY_IPHONE
			_track2(eventName,jsonObject.ToString());
#elif UNITY_ANDROID
			UnityUtil.CallStatic("track", Context, eventName, jsonObject.ToString());
#endif

        }




        public static void registerSuperProperty(JSONObject jsonObject)
        {
#if UNITY_EDITOR

#elif UNITY_IPHONE
			_registerSuperProperty(jsonObject.ToString());
#elif UNITY_ANDROID
			UnityUtil.CallStatic("registerSuperPropertyAll", Context, jsonObject.ToString());
#endif

        }



        public static void unregisterSuperProperty(String propertyName)
        {
#if UNITY_EDITOR

#elif UNITY_IPHONE
             _registerSuperProperty(propertyName);
#elif UNITY_ANDROID
            _DplusAgent.CallStatic("unregisterSuperProperty", Context, propertyName);
#endif

        }


        public static JSONNode getSuperProperty(String propertyName)
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IPHONE
			var jsonStr = _getSuperProperty(propertyName);
			return JSON.Parse(jsonStr)["__umeng_internal_data_"];

#elif UNITY_ANDROID

			var jsonStr = UnityUtil.CallStatic<String>("getSuperProperty",Context, propertyName);
			return JSON.Parse(jsonStr)["__umeng_internal_data_"];

#endif



        }


        public static JSONObject getSuperProperties()
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IPHONE
            return (JSONObject)JSONObject.Parse(_getSuperProperties());
#elif UNITY_ANDROID
			var jsonStr=_DplusAgent.CallStatic<String>("getSuperProperties", Context);
			return (JSONObject)JSON.Parse(jsonStr);
#endif


        }

        public static void clearSuperProperties()
        {
#if UNITY_EDITOR

#elif UNITY_IPHONE
             _clearSuperProperties();
#elif UNITY_ANDROID
            _DplusAgent.CallStatic("clearSuperProperties", Context);
#endif

        }


        public static void setFirstLaunchEvent(string[] trackID)
        {
#if UNITY_EDITOR

#elif UNITY_IPHONE

             _setFirstLaunchEvent(String.Join(";=umengUnity=;", trackID));
#elif UNITY_ANDROID
            UnityUtil.CallStatic("setFirstLaunchEvent", Context,String.Join(";=umengUnity=;", trackID));
#endif


        }

#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void _track(string propertyName);
        [DllImport("__Internal")]
        private static extern void _track2(string propertyName,string jsonStr);
        [DllImport("__Internal")]
        private static extern string _registerSuperProperty(string jsonStr);
        [DllImport("__Internal")]
        private static extern string _unregisterSuperProperty(string propertyName);
        [DllImport("__Internal")]
        private static extern string _getSuperProperty(string propertyName);
        [DllImport("__Internal")]
        private static extern string _getSuperProperties();
        [DllImport("__Internal")]
        private static extern void _clearSuperProperties();
        [DllImport("__Internal")]
        private static extern void _setFirstLaunchEvent(string propertyName);
#endif


    }




       

}
