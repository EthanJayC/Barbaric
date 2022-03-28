using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blood : MonoBehaviour
{
    public float SplatterSpd;
    public float TimeUntilDestroy;

    void Update()
    {
        transform.position += Random.insideUnitSphere * SplatterSpd * Time.deltaTime;

        TimeUntilDestroy -= Time.deltaTime;

        if (TimeUntilDestroy <= 0) Destroy(gameObject);
        
    }
}
