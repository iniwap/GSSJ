using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace com.mob
{
	/// <summary>
	/// Share rec For iOS.
	/// </summary>
	public static class ShareRECIOS 
	{	
		[DllImport("__Internal")]
		private static extern void __iosShareRECStartRecording();

		[DllImport("__Internal")]
		private static extern void __iosShareRECStopRecording (string observer);

		[DllImport("__Internal")]
		private static extern void __iosShareRECPlayLastRecording ();

		[DllImport("__Internal")]
		private static extern void __iosSaveVideoToGif (int quility);

        [DllImport("__Internal")]
        private static extern void __iosSaveVideoToLivePhoto();

		[DllImport("__Internal")]
		private static extern void __iosShareRECSetBitRate (int bitRate);

		[DllImport("__Internal")]
		private static extern void __iosShareRECSetFPS (int fps);

		[DllImport("__Internal")]
		private static extern string __iosShareRECLastRecordingPath ();

		[DllImport("__Internal")]
		private static extern void __iosShareRECEditLastRecording (string title, string userData, string observer);

        [DllImport("__Internal")]
        private static extern void __iosSetClipbord(string c);

        [DllImport("__Internal")]
		private static extern void __iosShareRECEditLastRecordingNew (string observer);

        [DllImport("__Internal")]
        private static extern bool __iosCanOpenUrl(string url);

        [DllImport("__Internal")]
		private static extern void __iosShareRECSocialOpen(string title, string userData, int pageType, string observer);

		[DllImport("__Internal")]
		private static extern void __iosShareRECSetMinimumRecordingTime(float time);

		[DllImport("__Internal")]
		private static extern void __iosShareRECSetSyncAudioComment (bool syncAudioComment);

//		[DllImport("__Internal")]
//		private static extern void __iosShareRECSocial(string platformName, string observer);

		[DllImport("__Internal")]
		private static extern void __iosShareRECSocialOpenWithShareResult(string title, string userData, int pageType, string observer);

		[DllImport("__Internal")]
		private static extern void __iosShareRECSocialSetShareAfterUploadCompleted(bool flag);

		private static string _callbackObjectName = "Main Camera";

		private static FinishedRecordEvent _finishedRecordHandler = null;
		private static CloseEvent _closeHandler = null;
		private static EditResultEvent _editResultHandler = null;
		private static CustomPlatformEvent _customPlatformEvent = null;
		private static ShareEvent _shareHandler = null;

		/// <summary>
		/// Sets the name of the callback object.
		/// </summary>
		/// <param name="objectName">Object name.</param>
		public static void setCallbackObjectName (string objectName)
		{
			_callbackObjectName = objectName;
		}

		/// <summary>
		/// Callback the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public static void shareRECCallback (string data)
		{
			object dataObj = MiniJSON.jsonDecode (data);
			if (dataObj is Hashtable)
			{
				Hashtable dataTable = dataObj as Hashtable;
				if (dataTable != null && dataTable.ContainsKey("name"))
				{
					string name = dataTable ["name"] as string;
					switch (name)
					{

					case "StopRecordingFinished":
					{
						Exception ex = null;
						if (dataTable.ContainsKey("error"))
						{
							string errorMessage = null;

							Hashtable error = dataTable["error"] as Hashtable;
							if (error.ContainsKey("message"))
							{
								errorMessage = error["message"] as string;
							}

							ex = new Exception(errorMessage);
						}

						//finished record
						if (_finishedRecordHandler != null)
						{
							_finishedRecordHandler (ex);
						}
						break;
					}

					case "SocialClose":
						if (_closeHandler != null)
						{
							_closeHandler ();
						}
						break;
					case "EditResult":
						bool cancelled = false;
						if (dataTable.ContainsKey("cancelled"))
						{
							cancelled = Convert.ToBoolean(dataTable["cancelled"]);
						}

						if (_editResultHandler != null)
						{
							_editResultHandler (cancelled);
						}
						break;
					case "CustomPlatformResult":
						
						break;
					case "ShareResult":
						if (_shareHandler != null)
						{
														_shareHandler ((SocialShareState)(Convert.ToInt64(dataTable[@"shareState"])));
						}
						break;
					}


				}
			}
		}
		
		/// <summary>
		/// Starts the recording.
		/// </summary>
		public static void startRecording()
		{
			__iosShareRECStartRecording();
		}

		/// <summary>
		/// Stops the recording.
		/// </summary>
		/// <param name="evt">Evt.</param>
		public static void stopRecording(FinishedRecordEvent evt)
		{
			_finishedRecordHandler = evt;

			__iosShareRECStopRecording (_callbackObjectName);
		}

		/// <summary>
		/// Plaies the last recording.
		/// </summary>
		public static void playLastRecording()
		{
			__iosShareRECPlayLastRecording ();
		}

		/// <summary>
		/// Convert video to gif
		/// </summary>
		public static void saveVideoToGif(int quility)
		{
			__iosSaveVideoToGif (quility);
		}

        /// <summary>
        /// Convert video to gif
        /// </summary>
        public static void saveVideoToLivePhoto()
        {
            __iosSaveVideoToLivePhoto();
        }
		/// <summary>
		/// Sets the bit rate.
		/// </summary>
		/// <param name="bitRate">Bit rate.</param>
		public static void setBitRate(int bitRate)
		{
			__iosShareRECSetBitRate(bitRate);
		}

		/// <summary>
		/// Sets the FPS.
		/// </summary>
		/// <param name="fps">Fps.</param>
		public static void setFPS(int fps)
		{
			__iosShareRECSetFPS(fps);
		}

		/// <summary>
		/// Sets the minimum recording time.
		/// </summary>
		/// <param name="time">Time.</param>
		public static void setMinimumRecordingTime(float time)
		{
			__iosShareRECSetMinimumRecordingTime(time);
		}

		/// <summary>
		/// Lasts the recording path.
		/// </summary>
		/// <returns>The recording path.</returns>
		public static string lastRecordingPath()
		{
			return __iosShareRECLastRecordingPath();
		}

		/// <summary>
		/// Edits the last recording.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="userData">User data.</param>
		/// <param name="evt">Evt.</param>
		public static void editLastRecording(string title, Hashtable userData, CloseEvent evt)
		{
			_closeHandler = evt;
			
			string userDataStr = null;
			if (userData != null)
			{
				userDataStr = MiniJSON.jsonEncode(userData);
			}

			__iosShareRECEditLastRecording(title, userDataStr, _callbackObjectName);
		}
        public static void setClipbord(string c)
        {
            __iosSetClipbord(c);
        }

        /// <summary>
        /// Edits the last recording.
        /// </summary>
        /// <param name="evt">Evt.</param>
        public static void editLastRecording(EditResultEvent evt)
		{
			_editResultHandler = evt;
			__iosShareRECEditLastRecordingNew (_callbackObjectName);
		}

        public static bool canOpenUrl(string url)
        {
            return __iosCanOpenUrl(url);
        }

        /// <summary>
        /// Sets the sync audio comment.
        /// </summary>
        /// <param name="flag">If set to <c>true</c> flag.</param>
        public static void setSyncAudioComment(bool flag)
		{
			__iosShareRECSetSyncAudioComment(flag);
		}

		/// <summary>
		/// Open the specified title, userData, pageType and evt.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="userData">User data.</param>
		/// <param name="pageType">Page type.</param>
		/// <param name="evt">Evt.</param>
		public static void openSocial(string title, Hashtable userData, SocialPageType pageType, CloseEvent evt)
		{
			_closeHandler = evt;

			string userDataStr = null;
			if (userData != null)
			{
				userDataStr = MiniJSON.jsonEncode(userData);
			}
			__iosShareRECSocialOpen(title, userDataStr, (int)pageType, _callbackObjectName); 
		}

		public static void addCustomPlatform(string platformName, CustomPlatformEvent evt)
		{
			_customPlatformEvent = evt;

			// __iosShareRECSocial(platformName, _callbackObjectName);
		}

		public static void openSocialWithShareResult(string title, Hashtable userData, SocialPageType pageType, CloseEvent evt, ShareEvent shareEvt)
		{
			_closeHandler = evt;

			string userDataStr = null;
			if (userData != null)
			{
				userDataStr = MiniJSON.jsonEncode(userData);
			}

			__iosShareRECSocialOpenWithShareResult(title, userDataStr, (int)pageType, _callbackObjectName);
		}

		public static void setShareAfterUploadCompleted(bool flag)
		{
			__iosShareRECSocialSetShareAfterUploadCompleted(flag);
		}
	}

}
