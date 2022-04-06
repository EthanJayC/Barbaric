using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blood : MonoBehaviour
{
    public float SplatterSpd;

    void Update()
    {
        transform.position += Random.insideUnitSphere * SplatterSpd * Time.deltaTime;
        
    }
}
