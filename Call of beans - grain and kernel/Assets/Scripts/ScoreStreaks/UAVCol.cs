using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVCol : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(other.GetComponentInChildren<Health>().team != PhotonManager.team && !other.GetComponentInChildren<Health>().ghost)
                other.GetComponentInChildren<MiniMapPlayer>().ShowMeYourLoc();
        }
    }
}
