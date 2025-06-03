#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class JsReader
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void InjectionJs(string url);

    [DllImport("__Internal")]
    public static extern void InjectionCSS(string url);
#endif


    public static void Initialize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        {
            var url = "https://www.gstatic.com/firebasejs/10.7.0/firebase-app-compat.js";
            InjectionJs(url);
            url = "https://www.gstatic.com/firebasejs/10.7.0/firebase-firestore-compat.js";
            InjectionJs(url);
        }
#endif
    }

}