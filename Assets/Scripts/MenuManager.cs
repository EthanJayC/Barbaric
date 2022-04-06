using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LevelComplete()
    {
        SceneManager.LoadScene("End Level");
    }

    public void ChooseDifficulty()
    {
        SceneManager.LoadScene("Choose Difficulty");
    }

    public void Quit()
    {
        //doesn't work in Unity, but there for post-release
        Application.Quit();
    }



}

