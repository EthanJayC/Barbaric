using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepMusicAlive : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

}
