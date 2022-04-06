using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacking : MonoBehaviour
{
    //Attacking
    public List<Transform> EnemiesInRange = new List<Transform>();
    private float AttackZoneDecay = 0.01f;
    public Game game;
    

    //Collision to get within Weapon range of player
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ChasingPlayer")
        {
            EnemiesInRange.Add(other.transform);
        }

        if (other.tag == "Gor")
        {
            EnemiesInRange.Add(other.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        AttackZoneDecay -= Time.deltaTime;
        if (AttackZoneDecay <= 0)
        {
            Destroy(gameObject);
            game.DoAttack(EnemiesInRange);
        }
    }
}
