using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScorestreakHealth : MonoBehaviour
{
    PhotonView pv;
    [SerializeField] ScoreStreakScrObj streak;
    public int team;
    [SerializeField] float health;
    [SerializeField] MeshRenderer[] renderers;
    [SerializeField] Material yourTeamColor;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void SetTeam(int _team)
    {
        pv.RPC("RPC_SetTeam", RpcTarget.All, _team);
    }
    [PunRPC]
    void RPC_SetTeam(int _team)
    {
        team = _team;
        if(team == PhotonManager.team)
        {
            foreach(MeshRenderer r in renderers)
            {
                r.material = yourTeamColor;
            }
        }
    }
    public void GetHit(float _damage)
    {
        pv.RPC("RPC_GetHit", RpcTarget.All, _damage, PhotonManager.playerId);
    }
    [PunRPC]
    void RPC_GetHit(float _damage, byte shotId)
    {
        health -= _damage;

        if(health <= 0)
        {
            if(shotId == PhotonManager.playerId)
            {
                GameManager.Instance.AddDestroy(streak.destroyPoints);
            }
            if (pv.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
