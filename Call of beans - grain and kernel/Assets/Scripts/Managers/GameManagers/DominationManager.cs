using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DominationManager : MonoBehaviour
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

    [SerializeField] int pointsPerCapture, pointsPerFlagCapt;
    [SerializeField] float getGamePointsTimer;

    [SerializeField] CapturePoint[] flags;

    [SerializeField] int[] teamPointsList;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if (MultiplayerManager.staticGamemodeId == 1)
        {
            for (int i = 0; i < flags.Length; i++)
            {
                flags[i].transform.parent.gameObject.SetActive(true);
                flags[i].pointsId = i;
            }
            if(PhotonNetwork.IsMasterClient)
                InvokeRepeating("AddGamePoints", getGamePointsTimer, getGamePointsTimer);
        }
    }
    public Transform GetStartSpawnPoint()
    {
        return spawnPoints[PhotonManager.team].spawnPoints[PhotonManager.teamPlace];
    }
    public Transform GetRespawnPoint()
    {
        List<Transform> possibleSpawnPoints = new List<Transform>();

        for (int i = 0; i < flags.Length; i++)
        {
            if(flags[i].currentTeam == PhotonManager.team)
            {
                for (int u = 0; u < spawnPoints[i].spawnPoints.Length; u++)
                {
                    possibleSpawnPoints.Add(spawnPoints[i].spawnPoints[u]);
                }
            }
        }

        if(possibleSpawnPoints.Count == 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                foreach (Transform t in spawnPoints[i].spawnPoints)
                {
                    possibleSpawnPoints.Add(t);
                }
            }
        }

        int leastPlayers = -1;

        foreach (Transform spawnPoint in possibleSpawnPoints)
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
        foreach (Transform spawnPoint in possibleSpawnPoints)
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
        HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerKill, 1, 0, 0);
        HudManager.Instance.GetPoints(gamemode.pointsPerKill);
    }
    public void AddAssist(PlayerManager pm)
    {
        HudManager.Instance.SyncScoreboardNumbers(pm, gamemode.pointsPerAssist, 0, 0, 1);
        HudManager.Instance.GetPoints(gamemode.pointsPerAssist);
    }
    public void AddCapturePoints(PlayerManager pm, int flag)
    {
        HudManager.Instance.SyncScoreboardNumbers(pm, pointsPerCapture, 0, 0, 0);
        HudManager.Instance.GetPoints(pointsPerCapture);
        pv.RPC("RPC_CaptureMessage", RpcTarget.All, PhotonManager.team, flag);
    }
    [PunRPC]
    void RPC_CaptureMessage(int team, int flag)
    {
        HudManager.Instance.SpawnMessage("Team " + team + " captured flag " + flag);
    }
    public void StopCapture()
    {
        foreach(CapturePoint point in flags)
        {
            point.inRange = false;
        }
    }
    public void AddFlagCaptPoints(PlayerManager pm)
    {
        HudManager.Instance.SyncScoreboardNumbers(pm, pointsPerFlagCapt, 0, 0, 0);
        HudManager.Instance.GetPoints(pointsPerFlagCapt);
    }
    public void AddGamePoints()
    {
        for (int i = 0; i < teamPointsList.Length; i++)
        {
            int addPoints = 0;
            foreach (CapturePoint c in flags)
            {
                if (c.currentTeam == i)
                {
                    addPoints++;
                }
            }
            pv.RPC("RPC_AddGamePoints", RpcTarget.All, i, addPoints);
        }

        CheckIfWon();
    }
    [PunRPC]
    void RPC_AddGamePoints(int teamId, int points)
    {
        teamPointsList[teamId] += points;
        if (teamId == PhotonManager.team)
        {
            for (int i = 0; i < points; i++)
            {
                GameManager.Instance.AddCapt();
            }
        }
        HudManager.Instance.ShowTeamPoints(teamPointsList);
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
