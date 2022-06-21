using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ZombieManager : MonoBehaviour
{
    PhotonView pv;
    public Transform[] playerSpawnPoints;
    public GameObject[] players;

    public Transform[] ZombieSpawnPoints;
    public GameObject[] zombies;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public Transform GetStartSpawnPoint()
    {
        return playerSpawnPoints[PhotonManager.playerId];
        pv.RPC("UpdatePlayerList", RpcTarget.Others);
    }
    public Transform GetRespawnPoint()
    {
        int randomPoint = Random.Range(0, playerSpawnPoints.Length);
        return playerSpawnPoints[randomPoint];
        pv.RPC("UpdatePlayerList", RpcTarget.Others);
    }

    [PunRPC]
    public void UpdatePlayerList()
    {
        players = null;
        players = GameObject.FindGameObjectsWithTag("Player");
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            
        }
        else
            return;
    }
    public void SelectZombie()
    {

    }
    public void SpawnZombie(GameObject zombieToSpawn)
    {
        int zombieID = Random.Range(0, zombies.Length);
        GameObject zombie = zombies[zombieID];
        PhotonNetwork.Instantiate(zombie.name, ZombieSpawnPoints[Random.Range(0, ZombieSpawnPoints.Length)].transform.position, Quaternion.identity);
    }
}
