using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : MonoBehaviour
{
    private Rigidbody rb;
    public Transform TargetPosition;
    public float EnemySpeed = 1f;

    void Update()
    {
        rb = GetComponent<Rigidbody>();
        if (TargetPosition != null)
        {
            var TargetDirection = (TargetPosition.position - transform.position).normalized; //gets the facing toward target

            rb.velocity += TargetDirection * EnemySpeed * Time.deltaTime; //moves towards target
            Debug.Log(TargetDirection);

            //rotates mob on 2-axis
            //left
            if (TargetDirection.x > 0) transform.rotation = Quaternion.Euler(245, 0, 180);
            //right
            if (TargetDirection.x < 0) transform.rotation = Quaternion.Euler(65, 0, 0);
        }


    }
}
