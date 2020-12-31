using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocialNetworkButton : MonoBehaviour
{
    public bool YouTube;

    private void Awake()
    {
        if (YouTube)
            GetComponent<Button>().onClick.AddListener(() => Application.OpenURL("https://www.youtube.com/c/JLPMGameDev"));
        else
            GetComponent<Button>().onClick.AddListener(() => Application.OpenURL("https://twitter.com/JLPMdev"));
    }
}
