using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScavengerPack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                Gun currentGun = other.GetComponentInParent<PlayerShoot>().currentGunScript;
                float ammo = currentGun.gun.magSize * .5f;
                currentGun.currentAmmo += Mathf.CeilToInt(ammo);
                HudManager.Instance.ShowAmmo(currentGun);
                Destroy(gameObject);
            }
        }
    }
}
