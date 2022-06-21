using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    PhotonView pv;

    public static GameManager Instance;
    int currentPlayerId;

    [SerializeField] private GameObject playerManager;
    PlayerManager yourPlayerManager;
    [SerializeField] private GameObject[] managers;

    public Transform[] uavSpot;
    public Transform[] chopperSpot;
    public Transform uavCol;
    public Transform[] uavCheckMoveSpots;

    public bool gamemodeLoadout;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        Instance = this;
    }
    private void Start()
    {
        GameObject g = PhotonNetwork.Instantiate(playerManager.name, Vector3.zero, Quaternion.identity);
        yourPlayerManager = g.GetComponent<PlayerManager>();

        int points = 0;
        switch (MultiplayerManager.staticGamemodeId)
        {
            case 0:
                //Free for all
                points = managers[0].GetComponent<FreeForAllManager>().gamemode.gamePointsToWin;
                yourPlayerManager.hasScorestreaks = managers[0].GetComponent<FreeForAllManager>().gamemode.hasKillStreaks;
                break;
            case 1:
                //Domination
                points = managers[1].GetComponent<DominationManager>().gamemode.gamePointsToWin;
                yourPlayerManager.hasScorestreaks = managers[1].GetComponent<DominationManager>().gamemode.hasKillStreaks;
                break;
            case 2:
                //Gun game
                points = managers[2].GetComponent<GunGameManager>().gamemode.gamePointsToWin;
                yourPlayerManager.hasScorestreaks = managers[2].GetComponent<GunGameManager>().gamemode.hasKillStreaks;
                gamemodeLoadout = true;
                break;
            case 3:
                //TeamDeathMatch
                points = managers[3].GetComponent<TeamDeathMatchManager>().gamemode.gamePointsToWin;
                yourPlayerManager.hasScorestreaks = managers[3].GetComponent<TeamDeathMatchManager>().gamemode.hasKillStreaks;
                break;

        }
        HudManager.Instance.pointsToWin.text = points + " points to win";
    }
    public string GetGameName()
    {
        switch (MultiplayerManager.staticGamemodeId)
        {
            case 0:
                //Free for all
                return managers[0].GetComponent<FreeForAllManager>().gamemode.gamemodeName;
            case 1:
                //Domination
                return managers[1].GetComponent<DominationManager>().gamemode.gamemodeName;
            case 2:
                //Gun game
                return managers[2].GetComponent<GunGameManager>().gamemode.gamemodeName;
            case 3:
                //Gun game
                return managers[3].GetComponent<TeamDeathMatchManager>().gamemode.gamemodeName;
            default:
                return "Default";
        }
    }
    public Transform GetStartSpawnPoint()
    {
        if (!PhotonManager.zombies)
        {
            switch (MultiplayerManager.staticGamemodeId)
            {
                case 0:
                    //Free for all
                    return managers[0].GetComponent<FreeForAllManager>().GetStartSpawnPoint();
                case 1:
                    //Domination
                    return managers[1].GetComponent<DominationManager>().GetStartSpawnPoint();
                case 2:
                    //Gun game
                    return managers[2].GetComponent<GunGameManager>().GetStartSpawnPoint();
                case 3:
                    //Team death match
                    return managers[3].GetComponent<TeamDeathMatchManager>().GetStartSpawnPoint();
                default:
                    return transform;
            }
        }
        else
        {
            return managers[0].GetComponent<ZombieManager>().GetStartSpawnPoint();
        }
    }
    public Vector3 GetUAVPoint()
    {
        Vector3 newPos = new Vector3(Random.Range(uavSpot[0].position.x, uavSpot[1].position.x), Random.Range(uavSpot[0].position.y, uavSpot[1].position.y), Random.Range(uavSpot[0].position.z, uavSpot[1].position.z));
        return newPos;
    }
    public Vector3 GetChopperPoint()
    {
        Vector3 newPos = new Vector3(Random.Range(chopperSpot[0].position.x, chopperSpot[1].position.x), Random.Range(chopperSpot[0].position.y, chopperSpot[1].position.y), Random.Range(chopperSpot[0].position.z, chopperSpot[1].position.z));
        return newPos;
    }
    public Transform GetRespawnPoint()
    {
        if (!PhotonManager.zombies)
        {
            switch (MultiplayerManager.staticGamemodeId)
            {
                case 0:
                    //Free for all
                    return managers[0].GetComponent<FreeForAllManager>().GetRespawnPoint();
                case 1:
                    //Domination
                    return managers[1].GetComponent<DominationManager>().GetRespawnPoint();
                case 2:
                    //Gun game
                    return managers[2].GetComponent<GunGameManager>().GetRespawnPoint();
                case 3:
                    //Team death match
                    return managers[3].GetComponent<TeamDeathMatchManager>().GetRespawnPoint();
                default:
                    return transform;
            }
        }
        else
        {
            return managers[0].GetComponent<ZombieManager>().GetRespawnPoint();
        }
    }
    public GameObject[] GetLoadout()
    {
        switch (MultiplayerManager.staticGamemodeId)
        {
            case 2:
                //Gun game
                List<GameObject> loadout = new List<GameObject>();
                int yourPoints = managers[2].GetComponent<GunGameManager>().GetYourGamePoints(PhotonManager.playerId);
                if (yourPoints >= managers[2].GetComponent<GunGameManager>().gunList.Length)
                    return  new GameObject[3];
                loadout.Add(managers[2].GetComponent<GunGameManager>().gunList[yourPoints].gunPrefab);
                Debug.Log(loadout[0].name);
                loadout.Add(null);
                loadout.Add(null);
                return loadout.ToArray();
            default:
                return new GameObject[3];
        }
    }
    public void AddKill(bool wasKnife)
    {
        yourPlayerManager.killsThisLive++;
        switch (MultiplayerManager.staticGamemodeId)
        {
            case 0:
                //Free for all
                managers[0].GetComponent<FreeForAllManager>().AddKill(yourPlayerManager);
                break;
            case 1:
                //Domination
                managers[1].GetComponent<DominationManager>().AddKill(yourPlayerManager);
                break;
            case 2:
                //Gun game
                managers[2].GetComponent<GunGameManager>().AddKill(yourPlayerManager, wasKnife);
                break;
            case 3:
                //Team death match
                managers[3].GetComponent<TeamDeathMatchManager>().AddKill(yourPlayerManager);
                break;
            default:
                break;
        }
    }
    public void AddAssist()
    {
        PlayerInfoManager.assists++;
        switch (MultiplayerManager.staticGamemodeId)
        {
            case 0:
                //Free for all
                managers[0].GetComponent<FreeForAllManager>().AddAssist(yourPlayerManager);
                break;
            case 1:
                //Domination
                managers[1].GetComponent<DominationManager>().AddAssist(yourPlayerManager);
                break;
            case 2:
                //Gun game
                managers[2].GetComponent<GunGameManager>().AddAssist(yourPlayerManager);
                break;
            case 3:
                //Team death match
                managers[3].GetComponent<TeamDeathMatchManager>().AddAssist(yourPlayerManager);
                break;
            default:
                break;
        }
    }
    public void DiedCheck(bool wasKnife)
    {
        switch (MultiplayerManager.staticGamemodeId)
        {
            case 2:
                //Gun game
                if (wasKnife)
                    managers[2].GetComponent<GunGameManager>().RemoveKill(yourPlayerManager, PhotonManager.playerId);
                break;
        }
    }
    public void AddDestroy(int points)
    {
        PlayerInfoManager.destroys++;
        HudManager.Instance.SyncScoreboardNumbers(yourPlayerManager, points, 0, 0, 0);
        HudManager.Instance.GetPoints(points);
    }
    public void AddCapture(int flag)
    {
        PlayerInfoManager.captures++;
        if (MultiplayerManager.staticGamemodeId == 1)
        {
            managers[1].GetComponent<DominationManager>().AddCapturePoints(yourPlayerManager, flag);
        }
    }
    public void AddCapt()
    {
        if(MultiplayerManager.staticGamemodeId == 1)
        {
            managers[1].GetComponent<DominationManager>().AddFlagCaptPoints(yourPlayerManager);
        }
    }
    public void StopCaptures()
    {
        if(MultiplayerManager.staticGamemodeId == 1)
        {
            managers[1].GetComponent<DominationManager>().StopCapture();
        }
    }
}
