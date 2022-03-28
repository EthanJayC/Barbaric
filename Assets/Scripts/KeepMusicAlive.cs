using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepMusicAlive : MonoBehaviour
{
    
   
    //Singleton to keep only one music object running across the game :) 
    public static KeepMusicAlive Instance = null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else if (Instance != this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}
