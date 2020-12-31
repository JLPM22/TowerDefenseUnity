using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    public string Scene;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(Scene));
    }
}
