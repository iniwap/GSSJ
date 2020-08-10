/**************************************/
//FileName: Tool.cs
//Author: wtx
//Data: 23/03/2018
//Describe:  统计事件相关
/**************************************/
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJson;
using Reign;
using Umeng;

public class EventReport: MonoBehaviour{

	public enum EventType{
		NoneType,
        HorizontalRightBuyClick,
        ShowLineBuyClick,
        DaanBtnBuyClick,
        ZSBtnBuyClick,
        LJBtnBuyClick,
        ThemeBtnBuyClick,
        ColorStorageBtnBuyClick,
    };
	// Use this for initialization
	void Start () {
		#if UNITY_ANDROID

		#elif UNITY_IPHONE		

		GA.StartWithAppKeyAndReportPolicyAndChannelId ("5c3ebadcb465f5abe50001a0", Analytics.ReportPolicy.BATCH,"App Store");

#else

#endif
        //仍旧需要
        //UnityAppController.mm文件,使用头文件#import "UNUMConfigure.h"并在didFinishLaunchingWithOptions中添加
        //[UNUMConfigure initWithAppkey:@"5c3ebadcb465f5abe50001a0" channel:@"App Store"];
    }

    void OnDestroy(){
		
	}
	void OnEnable(){

	}
	void OnDisable(){
		
	}

	public void OnEventReport(EventType type){
		GA.Event (""+type);
	}
	public void OnEventReport(string type){
		GA.Event (type);
	}

	public enum BuyType{
		BuySuccess,//购买笔刷成功,0
		BuyFail,//购买笔刷失败,0
		BuyRestore,
	};
}
