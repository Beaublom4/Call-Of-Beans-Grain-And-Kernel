using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GunGameManager : MonoBehaviour
{
    PhotonView pv;

    public GamemodeScrObj gamemode;

    [SerializeField] Transform[] spawnPoints;

    [SerializeField] float checkRange;
    [SerializeField] LayerMask playerMask;

    [SerializeField] int[] playerGamePointsList;

    public GunScrObj[] gunList;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        playerGamePointsList = new int[PhotonNetwork.CurrentRoom.PlayerCount];
    }
    public Transform GetStartSpawnPoint()
    {
        return spawnPoints[PhotonManager.playerId];
    }
    public Transform GetRespawnPoint()
    {
        int leastPlayers = -1;
        foreach (Transform spawnPoint in spawnPoints)
        {
            Collider[] cols = Physics.OverlapSphere(spawnPoint.position, checkRange, playerMask);
            int currentInRange = cols.Length;
            if (leastPlayers == -1)
            {
                leastPlayers = currentInRange;
            }
            else if (leastPlayers > currentInRange)
            {
                leastPlayers = currentInRange;
            }
        }

        List<Transform> possibleSpawns = new List<Transform>();
        foreach (Transform spawnPoint in spawnPoints)
        {
            Collider[] cols = Physics.OverlapSphere(spawnPoint.position, checkRange, playerMask);
            if (cols.Length == leastPlayers)
                possibleSpawns.Add(spawnPoint);
        }

        int randomPoint = Random.Range(0, possibleSpawns.Count);
        return possibleSpawns[randomPoint];
    }
    public void AddKill(PlayerManager pm, bool wasKnife)
    {
        if (!wasKnife)
        {
            playerGamePointsList[PhotonManager.playerId]++;
            HudManager.Instance.ShowGamePoints(playerGamePointsList);

            pv.RPC("RPC_AddKill", RpcTarget.Others, PhotonManager.playerId);

            pm.player.GetComponent<PlayerShoot>().ChangeGun(GameManager.Instance.GetLoadout());

            HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerKill, 0, 0, 0);
        }
        else
        {
            if (playerGamePointsList[PhotonManager.playerId] + 1 >= gamemode.gamePointsToWin)
            {
                playerGamePointsList[PhotonManager.playerId]++;
                HudManager.Instance.ShowGamePoints(playerGamePointsList);

                pv.RPC("RPC_AddKill", RpcTarget.Others, PhotonManager.playerId);

                pm.player.GetComponent<PlayerShoot>().ChangeGun(GameManager.Instance.GetLoadout());

                HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerKill, 0, 0, 0);
            }
        }
        HudManager.Instance.SyncScoreboardNumbers(pm, 0, 1, 0, 0);
        HudManager.Instance.GetPoints(gamemode.pointsPerKill);

        CheckIfWon();
    }
    [PunRPC]
    void RPC_AddKill(byte playerId)
    {
        playerGamePointsList[playerId]++;
        HudManager.Instance.ShowGamePoints(playerGamePointsList);
    }
    public void RemoveKill(PlayerManager pm, byte playerId)
    {
        if(playerGamePointsList[playerId] > 0)
        {
            playerGamePointsList[PhotonManager.playerId]--;
            HudManager.Instance.ShowGamePoints(playerGamePointsList);
            pv.RPC("RPC_RemoveKill", RpcTarget.Others, PhotonManager.playerId);
        }
    }
    [PunRPC]
    void RPC_RemoveKill(byte playerId)
    {
        playerGamePointsList[playerId]--;
        HudManager.Instance.ShowGamePoints(playerGamePointsList);
    }
    void CheckIfWon()
    {
        if (playerGamePointsList[PhotonManager.playerId] >= gamemode.gamePointsToWin)
        {
            pv.RPC("RPC_GetWinner", RpcTarget.All, PhotonNetwork.NickName);
        }
    }
    [PunRPC]
    void RPC_GetWinner(string winnerName)
    {
        HudManager.Instance.scoreboardObj.SetActive(true);
        HudManager.Instance.WinnerName.text = "Winner: " + winnerName;
        HudManager.Instance.WinnerName.gameObject.SetActive(true);

        Time.timeScale = 0;
        StartCoroutine(WaitToExit());
    }
    IEnumerator WaitToExit()
    {
        yield return new WaitForSecondsRealtime(5);
        Time.timeScale = 1;
        yield return new WaitForEndOfFrame();
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(0);
    }
    public void AddAssist(PlayerManager pm)
    {
        HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerAssist, 0, 0, 1);
        HudManager.Instance.GetPoints(gamemode.pointsPerAssist);
    }
    public int GetYourGamePoints(byte playerId)
    {
        return playerGamePointsList[playerId];
    }
}
