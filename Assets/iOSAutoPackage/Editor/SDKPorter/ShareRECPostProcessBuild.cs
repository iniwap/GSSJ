using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using cn.sharesdk.unity3d.sdkporter;
using System.IO;
using com.mob;

public static class ShareSDKPostProcessBuild 
{
	//[PostProcessBuild]
	[PostProcessBuildAttribute(88)]
	public static void onPostProcessBuild(BuildTarget target,string targetPath)
	{
		string unityEditorAssetPath = Application.dataPath;

		if (target != BuildTarget.iOS) 
		{
			Debug.LogWarning ("Target is not iPhone. XCodePostProcess will not run");
			return;
		}

		XCProject project = new XCProject (targetPath);
		//var files = System.IO.Directory.GetFiles( unityEditorAssetPath, "*.projmods", System.IO.SearchOption.AllDirectories );
		var files = System.IO.Directory.GetFiles( unityEditorAssetPath + "/iOSAutoPackage/Editor/SDKPorter", "*.projmods", System.IO.SearchOption.AllDirectories);
		foreach( var file in files ) 
		{
			project.ApplyMod( file );
		}

		//如需要预配置Xocode中的URLScheme 和 白名单,请打开下两行代码,并自行配置相关键值
		string projPath = Path.GetFullPath (targetPath);
		EditInfoPlist (projPath);

		//Finally save the xcode project
		project.Save();
	}
	

	private static void EditInfoPlist(string projPath)
	{
		ShareRECConfig theConfig;

		try
		{
			string filePath = Application.dataPath + "/iOSAutoPackage/Editor/SDKPorter/ShareRECConfig.bin";
			BinaryFormatter formatter = new BinaryFormatter();
			Stream destream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			ShareRECConfig config = (ShareRECConfig)formatter.Deserialize(destream);
			destream.Flush();
			destream.Close();
			theConfig = config;
		}
		catch(Exception)
		{
			theConfig = new ShareRECConfig ();
		}
						
		XCPlist plist = new XCPlist (projPath);
		string AppKey = @"<key>MOBAppkey</key> <string>" + theConfig.appKey + "</string>";
		string AppSecret = @"<key>MOBAppSecret</key> <string>" + theConfig.appSecret + "</string>";
		string CameraUse = @"<key>NSCameraUsageDescription</key><string>" + "允许拍照用作配图" + "</string>";
		string ContactsUse = @"<key>NSContactsUsageDescription</key><string>" + "允许访问注册账号" + "</string>";
		string MicrophoneUse = @"<key>NSMicrophoneUsageDescription</key><string>" + "允许录制制作视频" + "</string>";
		string PhotoLibraryUse = @"<key>NSPhotoLibraryUsageDescription</key><string>" + "允许选择照片用作配图" + "</string>";
		string PhotoLibraryAdd = @"<key>NSPhotoLibraryAddUsageDescription</key><string>" + "允许保存信笺到相册" + "</string>";
		string LocationWhenInUse = @"<key>NSLocationWhenInUseUsageDescription</key><string>" + "分享图片时可能用到，不授权不会影响使用" + "</string>";

		//在plist里面增加配置
		plist.AddKey(AppKey);
		plist.AddKey(AppSecret);
		plist.AddKey (CameraUse);
		plist.AddKey (ContactsUse);
		plist.AddKey (MicrophoneUse);
		plist.AddKey (PhotoLibraryUse);
		plist.AddKey (PhotoLibraryAdd);
		plist.AddKey (LocationWhenInUse);
		plist.Save();
	}


}