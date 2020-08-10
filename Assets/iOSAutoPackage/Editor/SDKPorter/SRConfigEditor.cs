using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;


namespace com.mob
{
		[CustomEditor(typeof(ShareREC))]
		[ExecuteInEditMode]
		public class SRConfigEditor : Editor {

				private ShareRECConfig config;

				void Awake()
				{
					Prepare ();
				}
						
				public override void OnInspectorGUI()
				{
						EditorGUILayout.Space ();
						config.appKey = EditorGUILayout.TextField ("MobAppKey", config.appKey);
						config.appSecret = EditorGUILayout.TextField ("MobAppSecret", config.appSecret);
						Save ();
				}

				private void Prepare()
				{
						string filePath = Application.dataPath + "/iOSAutoPackage/Editor/SDKPorter/ShareRECConfig.bin";
						try
						{
								BinaryFormatter formatter = new BinaryFormatter();
								Stream destream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
								ShareRECConfig config = (ShareRECConfig)formatter.Deserialize(destream);
								destream.Flush();
								destream.Close();
								this.config = config;
						}
						catch(Exception)
						{
								this.config = new ShareRECConfig ();
						}
				}

				private void Save()
				{
						try
						{
								string filePath = Application.dataPath + "/iOSAutoPackage/Editor/SDKPorter/ShareRECConfig.bin";
								BinaryFormatter formatter = new BinaryFormatter();
								Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
								formatter.Serialize(stream, this.config);
								stream.Flush();
								stream.Close();
						}
						catch (Exception e) 
						{
								Debug.Log ("save error:" + e);
						}
				}

		}
}