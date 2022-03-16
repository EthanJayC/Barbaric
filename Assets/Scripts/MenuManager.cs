using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void SceneChange()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenOptions()
    {
        //needs option stuff
    }

    public void Quit()
    {
        //doesn't work in Unity, but there for post-release
        Application.Quit();
    }



}

