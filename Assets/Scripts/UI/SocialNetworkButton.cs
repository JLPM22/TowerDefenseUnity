using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SocialNetworkButton : MonoBehaviour
{
    public bool YouTube;

    [DllImport("__Internal")]
    private static extern void OpenNewTab(string url);

    public void openIt(string url)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             OpenNewTab(url);
#endif
    }

    private void Awake()
    {
        if (YouTube)
            GetComponent<Button>().onClick.AddListener(() => OpenURL("https://www.youtube.com/c/JLPMGameDev"));
        else
            GetComponent<Button>().onClick.AddListener(() => OpenURL("https://twitter.com/JLPMdev"));
    }

    private void OpenURL(string url)
    {
        openIt(url);
    }
}
