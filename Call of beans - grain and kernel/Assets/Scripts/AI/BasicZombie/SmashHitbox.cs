using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashHitbox : MonoBehaviour
{
    ZombieBasics zombiebasics;
    float damage;
    private void Start()
    {
        zombiebasics = GetComponentInParent<ZombieBasics>();
        damage = zombiebasics.basicSmashDamage;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //other.gameObject.GetComponent<Health>().GetHit(damage, null, false, Vector3.zero, false, "Zombie");
        }
    }

}
