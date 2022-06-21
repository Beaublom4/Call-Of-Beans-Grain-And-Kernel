using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KnifeHitBox : MonoBehaviour
{
    Transform mainCam;
    float knifeDamage;
    PlayerShoot ps;

    public void SetUp(Transform _mainCam, float _knifeDamage, PlayerShoot _ps)
    {
        mainCam = _mainCam;
        knifeDamage = _knifeDamage;
        ps = _ps;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PhotonView>())
            if (other.GetComponent<PhotonView>().IsMine)
                return;

        if (other.tag == "Player")
        {
            other.GetComponent<Health>().GetHit(knifeDamage, ps, false, mainCam.position, false, "Knife", false, true);
        }
        else if (other.tag == "Glasses" && other.transform.root.tag == "Player")
        {
            other.transform.root.GetComponent<Health>().GetHit(knifeDamage * 2, ps, false, mainCam.position, false, "Knife", false, true);
        }
        else if (other.tag == "Zombie")
        {
            other.GetComponent<Health>().GetHit(knifeDamage, ps, true, transform.position, false, "Knife", false, true);
        }
    }
}
