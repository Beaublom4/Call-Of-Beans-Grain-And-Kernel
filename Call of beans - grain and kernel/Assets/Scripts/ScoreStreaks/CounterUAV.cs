using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CounterUAV : MonoBehaviour
{
    PhotonView pv;
    public float aliveTime;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if (pv.IsMine)
            pv.RPC("RPC", RpcTarget.Others);
    }
    [PunRPC]
    void RPC()
    {
        if (GetComponent<ScorestreakHealth>().team != PhotonManager.team)
            HudManager.Instance.counterUAV = this;
    }
}
