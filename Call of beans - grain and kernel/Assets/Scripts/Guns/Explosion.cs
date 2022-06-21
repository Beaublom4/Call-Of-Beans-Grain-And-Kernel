using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Explosion : MonoBehaviour
{
    public void StartExplode(float size)
    {
        transform.localScale = new Vector3(size, size, size);
        Invoke("Destroy", 3);
    }
    void Destroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
