using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupCooldown : MonoBehaviour
{

    public float SkullCooldownPickup;


    // Update is called once per frame
    void Update()
    {
        SkullCooldownPickup -= Time.deltaTime;
        if (SkullCooldownPickup <= 0) SkullCooldownPickup = 0;
    }
}
