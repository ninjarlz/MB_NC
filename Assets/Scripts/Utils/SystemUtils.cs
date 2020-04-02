
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class SystemUtils
    {
        private SystemUtils() { }

        private static SystemUtils _instance;

        public static SystemUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SystemUtils();
#if UNITY_ANDROID && !UNITY_EDITOR                   
                    AndroidJavaClass UnityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
                    _instance._currentActivity = UnityPlayer.GetStatic<AndroidJavaObject> ("currentActivity"); 
                    _instance._context = _instance._currentActivity.Call<AndroidJavaObject>("getApplicationContext");
#endif                    
                }
                return _instance;
            }
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        private string _toastText;
        private AndroidJavaObject _currentActivity;
        private AndroidJavaObject _context;
        private static string COPIED_MSG = "Copied to clipboard!";
#endif 
        
        public void CopyToSystemClipboard(string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass Context = new AndroidJavaClass ("android.content.Context");
            AndroidJavaObject CLIPBOARD_SERVICE = Context.GetStatic<AndroidJavaObject> ("CLIPBOARD_SERVICE");
            AndroidJavaObject clipboardMgr = _currentActivity.Call<AndroidJavaObject> ("getSystemService", CLIPBOARD_SERVICE);
            AndroidJavaClass ClipData = new AndroidJavaClass ("android.content.ClipData");
            AndroidJavaObject clipData = ClipData.CallStatic<AndroidJavaObject> ("newPlainText", "simple text", text);
            clipboardMgr.Call ("setPrimaryClip", clipData);
            showToastOnUiThread(COPIED_MSG);
#endif 
        }
        
        public void showToastOnUiThread(string toastString)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _toastText = toastString;
            _currentActivity.Call ("runOnUiThread", new AndroidJavaRunnable(showToast));
#endif
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        private void showToast()
        {
            AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", _toastText);
            AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject> ("makeText", _context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
            toast.Call ("show");
        }
#endif
    }

        
}

