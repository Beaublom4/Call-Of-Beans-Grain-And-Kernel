using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TeamDeathMatchManager : MonoBehaviour
{
    PhotonView pv;

    public GamemodeScrObj gamemode;

    [System.Serializable]
    public class TeamSpawnPoints
    {
        public Transform[] spawnPoints;
    }
    [SerializeField] TeamSpawnPoints[] spawnPoints;
    [SerializeField] float checkRange;
    [SerializeField] LayerMask playerMask;

    [SerializeField] int[] teamPointsList;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public Transform GetStartSpawnPoint()
    {
        return spawnPoints[PhotonManager.team].spawnPoints[PhotonManager.teamPlace];
    }
    public Transform GetRespawnPoint()
    {
        List<Transform> possibleSpawns = new List<Transform>();
        int leastPlayers = -1;
        foreach (Transform spawnPoint in spawnPoints[PhotonManager.team].spawnPoints)
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

        foreach (Transform spawnPoint in spawnPoints[PhotonManager.team].spawnPoints)
        {
            Collider[] cols = Physics.OverlapSphere(spawnPoint.position, checkRange, playerMask);
            if (cols.Length == leastPlayers)
                possibleSpawns.Add(spawnPoint);
        }

        int randomPoint = Random.Range(0, possibleSpawns.Count);
        return possibleSpawns[randomPoint];
    }
    public void AddKill(PlayerManager pm)
    {
        teamPointsList[PhotonManager.team]++;
        HudManager.Instance.ShowGamePoints(teamPointsList);

        pv.RPC("RPC_AddKill", RpcTarget.Others, PhotonManager.playerId);

        HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerKill, 1, 0, 0);
        HudManager.Instance.GetPoints(gamemode.pointsPerKill);

        CheckIfWon();
    }
    [PunRPC]
    void RPC_AddKill(byte playerId)
    {
        teamPointsList[playerId]++;
        HudManager.Instance.ShowGamePoints(teamPointsList);
    }
    public void AddAssist(PlayerManager pm)
    {
        HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerAssist, 0, 0, 1);
        HudManager.Instance.GetPoints(gamemode.pointsPerAssist);
    }
    void CheckIfWon()
    {
        for (int i = 0; i < teamPointsList.Length; i++)
        {
            if (teamPointsList[i] >= gamemode.gamePointsToWin)
            {
                string teamName = "Team " + i;
                pv.RPC("RPC_GetWinner", RpcTarget.All, teamName);
            }
        }
    }
    [PunRPC]
    void RPC_GetWinner(string winnerName)
    {
        HudManager.Instance.scoreboardObj.SetActive(true);
        HudManager.Instance.WinnerName.text = winnerName + " won";
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
}
