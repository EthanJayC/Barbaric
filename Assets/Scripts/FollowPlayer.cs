using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public float MovementSpeed;
    public Transform Player;
    public Vector3 DistanceFromTarget;

    void Update()
    {
        if (Player != null) {  //means the code doesn't break on Player death

        transform.position = Vector3.Lerp(transform.position, Player.position + DistanceFromTarget, MovementSpeed * Time.deltaTime);

        }

    }
}
