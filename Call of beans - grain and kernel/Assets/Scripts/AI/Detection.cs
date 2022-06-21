using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Barrier")
        {
            transform.GetComponentInParent<ZombieBasics>().PlankDetection(other.GetComponent<Barrier>());
        }
        if(other.gameObject.tag == "Player")
        {
            transform.parent.GetComponent<ZombieBasics>().playerDetected = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Barrier")
        {
            transform.parent.GetComponent<ZombieBasics>().barrier = null;
        }
        if (other.gameObject.tag == "Player")
        {
            transform.parent.GetComponent<ZombieBasics>().playerDetected = false;
        }
    }
}
