using UnityEngine;
using System.Collections;
using System;

namespace com.mob
{
	/// <summary>
	/// Finished record event.
	/// </summary>
	public delegate void FinishedRecordEvent (Exception ex);

	/// <summary>
	/// Close event.
	/// </summary>
	public delegate void CloseEvent ();

	/// <summary>
	/// Edit result event.
	/// </summary>
	public delegate void EditResultEvent (bool cancelled);

	/// <summary>
	///Custom platform event
	/// </summary>
	public delegate void CustomPlatformEvent(string platformName, string title, Hashtable recording);


	public delegate void ShareEvent (SocialShareState state);


	public enum SocialPageType
	{
		Share = 0,			// share
		ViewCenter = 1,		// video center
		Profile = 2			// profile
	}

	public enum SocialShareState
	{
		ShareStateBegin = 0,
		ShareStateSuccess = 1,
		ShareStateFail = 2,
		ShareStateCancel = 3
	}

	/// <summary>
	/// Share rec.
	/// </summary>
	public class ShareREC : MonoBehaviour 
	{
		/// <summary>
		/// _callback the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		private void shareRECCallback (string data)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				#if UNITY_IPHONE
				ShareRECIOS.shareRECCallback(data);
				#endif
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				#if UNITY_ANDROID
				#endif
			}
		}

		/// <summary>
		/// Sets the name of the callback object.
		/// </summary>
		/// <param name="objectName">Object name.</param>
		public static void setCallbackObjectName(string objectName)
		{
			if (objectName == null)
			{
				objectName = "Main Camera";
			}
			
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				#if UNITY_IPHONE
				ShareRECIOS.setCallbackObjectName(objectName);
				#endif
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				#if UNITY_ANDROID
				#endif
			}
		}
		
		/// <summary>
		/// Starts the recoring.
		/// </summary>
		public static void startRecoring ()
		{
			#if UNITY_IPHONE
			ShareRECIOS.startRecording();
			#elif	UNITY_ANDROID
			
			#endif
		}
		
		/// <summary>
		/// Stops the recording.
		/// </summary>
		public static void stopRecording (FinishedRecordEvent evt)
		{
			#if UNITY_IPHONE
			ShareRECIOS.stopRecording(evt);
			#elif	UNITY_ANDROID
			
			#endif
		}
		
		/// <summary>
		/// Plaies the last recording.
		/// </summary>
		public static void playLastRecording()
		{
			#if UNITY_IPHONE
			ShareRECIOS.playLastRecording();
			#elif	UNITY_ANDROID
			
			#endif
		}

		/// <summary>
		/// SAVE GIF
		/// </summary>
		public static void saveVideoToGif(int quility)
		{
			#if UNITY_IPHONE
			ShareRECIOS.saveVideoToGif(quility);
			#elif	UNITY_ANDROID

			#endif
		}

        /// <summary>
        /// SAVE GIF
        /// </summary>
        public static void saveVideoToLivePhoto()
        {
        #if UNITY_IPHONE
            ShareRECIOS.saveVideoToLivePhoto();
        #elif UNITY_ANDROID

        #endif
        }

		/// <summary>
		/// Sets the bit rate.
		/// </summary>
		/// <param name="bitRate">Bit rate.</param>
		public static void setBitRate(int bitRate)
		{
			#if UNITY_IPHONE
			ShareRECIOS.setBitRate(bitRate);
			#elif	UNITY_ANDROID
			
			#endif
		}
		
		/// <summary>
		/// Sets the FPS.
		/// </summary>
		/// <param name="fps">Fps.</param>
		public static void setFPS(int fps)
		{
			#if UNITY_IPHONE
			ShareRECIOS.setFPS(fps);
			#elif	UNITY_ANDROID
			
			#endif
		}

		/// <summary>
		/// Sets the minimum recording time.
		/// </summary>
		/// <param name="time">Time.</param>
		public static void setMinimumRecordingTime(float time)
		{
			#if UNITY_IPHONE
			ShareRECIOS.setMinimumRecordingTime(time);
			#elif	UNITY_ANDROID
			
			#endif
		}
		
		/// <summary>
		/// Lasts the recording path.
		/// </summary>
		/// <returns>The recording path.</returns>
		public static string lastRecordingPath()
		{
			#if UNITY_IPHONE
			return ShareRECIOS.lastRecordingPath();
			#elif	UNITY_ANDROID
			
			#endif

			return null;
		}

		/// <summary>
		/// Edits the lasting recording.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="userData">User data.</param>
		/// <param name="evt">Evt.</param>
		public static void editLastingRecording(string title, Hashtable userData, CloseEvent evt)
		{
			#if UNITY_IPHONE
			ShareRECIOS.editLastRecording(title, userData, evt);
			#elif	UNITY_ANDROID
			
			#endif
		}
        public static void setClipbord(string c)
        {
#if UNITY_IPHONE
            ShareRECIOS.setClipbord(c);
#elif UNITY_ANDROID
			
#endif
        }

        /// <summary>
        /// Edits the last recording.
        /// </summary>
        /// <param name="evt">Evt.</param>
        public static void editLastRecording(EditResultEvent evt)
		{
			#if UNITY_IPHONE
			ShareRECIOS.editLastRecording(evt);
			#elif	UNITY_ANDROID
			
			#endif
		}

		/// <summary>
		/// Sets the sync audio comment.
		/// </summary>
		/// <param name="flag">If set to <c>true</c> flag.</param>
		public static void setSyncAudioComment(bool flag)
		{
			#if UNITY_IPHONE
			ShareRECIOS.setSyncAudioComment(flag);
			#elif	UNITY_ANDROID
			
			#endif
		}

		/// <summary>
		/// Opens the social.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="userData">User data.</param>
		/// <param name="pageType">Page type.</param>
		/// <param name="evt">Evt.</param>
		public static void openSocial(string title, Hashtable userData, SocialPageType pageType, CloseEvent evt)
		{
			#if UNITY_IPHONE
			ShareRECIOS.openSocial(title, userData, pageType, evt);
			#elif	UNITY_ANDROID
			
			#endif
		}

		public static void addCustomPlatform(string platformName, CustomPlatformEvent evt)
		{
			#if UNITY_IPHONE
//			ShareRECIOS.addCustomPlatform();
			#elif UNITY_ANDROID
			#endif
		}

		public static void closeSocial()
		{
			#if UNITY_IPHONE
//			ShareRECIOS.
			#elif UNITY_ANDROID

			#endif
		}

		public static void setShareAfterUploadCompleted(bool flag)
		{
			#if UNITY_IPHONE
			ShareRECIOS.setShareAfterUploadCompleted(flag);
			#elif UNITY_ANDROID

			#endif
		}
		
		public static void openSocialWithShareResult (string title, Hashtable userData, SocialPageType pageType, CloseEvent evt, ShareEvent shareEvt)
		{
			#if UNITY_IPHONE
			ShareRECIOS.openSocialWithShareResult(title, userData, pageType, evt, shareEvt);
			#elif UNITY_ANDROID
			
			#endif
		}
	}

}
