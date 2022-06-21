using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
    public int team;

    [Header("Stats")]
    public int points;
    public int kills;
    public int deaths;
    public int assists;

    [Space]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] public GameObject player;
    //GameObject zombieSpawner;
    public int killsThisLive;

    [Header("ScoreStreaks")]
    public int pointsThisLive;
    [HideInInspector] public bool hasScorestreaks;
    [System.Serializable]
    public class ScoreStreaksHolder
    {
        public bool unlocked = false;
        public int addedPoints;
        public ScoreStreakScrObj streak;
    }
    [SerializeField] ScoreStreaksHolder[] scoreStreakHolders;

    int selectedScoreStreak = 0;

    [SerializeField] GameObject uavPrefab;
    [SerializeField] GameObject counterUAVPrefab;
    [SerializeField] GameObject chopperPrefab;

    [Header("Death cam")]
    [SerializeField] float respawnTime;
    [SerializeField] GameObject deathCamPrefab;
    GameObject currentDeahtCam;

    public GameObject fakePlayer;
    [HideInInspector] public GameObject currentFakePlayer;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            team = PhotonManager.team;
            pv.RPC("RPC_SyncTeam", RpcTarget.OthersBuffered, team);
        }
    }
    [PunRPC]
    void RPC_SyncTeam(int _team)
    {
        team = _team;
    }
    private void Start()
    {
        if (!pv.IsMine)
            return;

        Transform spawnPoint = GameManager.Instance.GetStartSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<PlayerShoot>().SetUp(this);
        player.GetComponent<Health>().pm = this;

        if (hasScorestreaks)
            SyncScoreStreaks();
        else
        {
            HudManager.Instance.HideScoreStreaks();
        }
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;

        if (!hasScorestreaks)
            return;

        if (Input.GetButtonDown("Up"))
        {
            selectedScoreStreak++;
            if (selectedScoreStreak >= scoreStreakHolders.Length)
                selectedScoreStreak = 0;
            HudManager.Instance.SelectScoreStreak(selectedScoreStreak);
        }
        else if (Input.GetButtonDown("Down"))
        {
            selectedScoreStreak--;
            if (selectedScoreStreak < 0)
                selectedScoreStreak = scoreStreakHolders.Length - 1;
            HudManager.Instance.SelectScoreStreak(selectedScoreStreak);
        }
    }
    #region ScoreStreaks
    public void PointsThisLive(int addPoints)
    {
        int unlocks = 0;
        foreach(ScoreStreaksHolder s in scoreStreakHolders)
        {
            if (s.unlocked)
                unlocks++;
        }
        if (unlocks == scoreStreakHolders.Length)
            return;

        int hardline = 0;
        if (PlayerInfoManager.perk2 != null && PlayerInfoManager.perk2.perkId == 0 && MultiplayerManager.hasPerks)
            hardline = 20;
        pointsThisLive += addPoints + hardline;
        SyncBar();
    }
    void SyncBar()
    {
        int nextToUnlock = 1000000;
        for (int i = 0; i < scoreStreakHolders.Length; i++)
        {
            if (!scoreStreakHolders[i].unlocked)
            {
                int calcPoints = scoreStreakHolders[i].streak.streakWantedPoints + scoreStreakHolders[i].addedPoints;
                if (calcPoints < nextToUnlock)
                {
                    nextToUnlock = calcPoints;
                }

                if (pointsThisLive >= calcPoints)
                {
                    scoreStreakHolders[i].unlocked = true;
                    HudManager.Instance.UnlockScoreStreak(i);
                    int nextInLine = 1 + i;
                    if (nextInLine < scoreStreakHolders.Length)
                        nextToUnlock = scoreStreakHolders[nextInLine].streak.streakWantedPoints;
                    else
                        nextToUnlock = 1;
                }
            }
        }
        HudManager.Instance.PointsToScoreStreak(pointsThisLive, nextToUnlock);
    }
    public void SyncScoreStreaks()
    {
        for (int i = 0; i < scoreStreakHolders.Length; i++)
        {
            HudManager.Instance.SyncScoreStreaks(i, scoreStreakHolders[i].streak.streakSprite);
        }
    }
    public void DoScoreStreak()
    {
        if (!hasScorestreaks)
            return;

        if (!scoreStreakHolders[selectedScoreStreak].unlocked)
            return;

        scoreStreakHolders[selectedScoreStreak].unlocked = false;
        for (int i = 0; i < scoreStreakHolders.Length; i++)
        {
            if(scoreStreakHolders[i].streak.streakId == scoreStreakHolders[selectedScoreStreak].streak.streakId)
            {
                HudManager.Instance.DisableScoreStreak(i);
            }
        }

        scoreStreakHolders[selectedScoreStreak].addedPoints += scoreStreakHolders[selectedScoreStreak].streak.streakWantedPoints;

        switch (scoreStreakHolders[selectedScoreStreak].streak.streakId)
        {
            case 0:
                //uav
                GameObject currentUAV = PhotonNetwork.Instantiate(uavPrefab.name, GameManager.Instance.GetUAVPoint(), Quaternion.Euler(0, Random.Range(0, 360), 0));
                currentUAV.GetComponent<ScorestreakHealth>().team = PhotonManager.team;
                currentUAV.GetComponent<ScorestreakHealth>().SetTeam(PhotonManager.team);
                pv.RPC("RPC_ScoreStreakMessage", RpcTarget.All, PhotonNetwork.NickName, "UAV");
                StartCoroutine(UAVTimer(currentUAV));
                break;
            case 1:
                //counter uav
                GameObject currentCoutnerUAV = PhotonNetwork.Instantiate(counterUAVPrefab.name, GameManager.Instance.GetUAVPoint(), Quaternion.Euler(0, Random.Range(0,360), 0));
                currentCoutnerUAV.GetComponent<ScorestreakHealth>().team = PhotonManager.team;
                currentCoutnerUAV.GetComponent<ScorestreakHealth>().SetTeam(PhotonManager.team);
                pv.RPC("RPC_ScoreStreakMessage", RpcTarget.All, PhotonNetwork.NickName, "Counter UAV");
                StartCoroutine(CounterUAVTimer(currentCoutnerUAV));
                break;
            case 3:
                //chopper
                GameObject currentChopper = PhotonNetwork.Instantiate(chopperPrefab.name, GameManager.Instance.GetChopperPoint(), Quaternion.Euler(0, Random.Range(0, 360), 0));
                currentChopper.GetComponent<Chopper>().yourHealth = player.GetComponent<Health>();
                currentChopper.GetComponent<ScorestreakHealth>().SetTeam(PhotonManager.team);
                pv.RPC("RPC_ScoreStreakMessage", RpcTarget.All, PhotonNetwork.NickName, "Chopper");

                StartCoroutine(ChopperTimer(currentChopper));
                break;
        }
    }
    IEnumerator UAVTimer(GameObject currentUAV)
    {
        yield return new WaitForSeconds(currentUAV.GetComponent<UAV>().aliveTime);
        if (currentUAV != null)
            PhotonNetwork.Destroy(currentUAV);
    }
    IEnumerator CounterUAVTimer(GameObject currentUAV)
    {
        yield return new WaitForSeconds(currentUAV.GetComponent<CounterUAV>().aliveTime);
        if(currentUAV != null)
            PhotonNetwork.Destroy(currentUAV);
    }
    IEnumerator ChopperTimer(GameObject currentChopper)
    {
        yield return new WaitForSeconds(currentChopper.GetComponent<Chopper>().aliveTime);
        if (currentChopper != null)
            PhotonNetwork.Destroy(currentChopper);
    }
    [PunRPC]
    void RPC_ScoreStreakMessage(string playerName, string streakName)
    {
        HudManager.Instance.SpawnMessage(playerName + " called " + streakName);
    }
    #endregion

    public void SpawnDeathCam(Vector3 deathPos, Vector3 shotFrom)
    {
        HudManager.Instance.Death(false);
        HudManager.Instance.HitShowOff();
        GameManager.Instance.StopCaptures();
        HudManager.Instance.StopCapturing();
        currentDeahtCam = Instantiate(deathCamPrefab, deathPos, Quaternion.identity, null);
        currentDeahtCam.transform.LookAt(deathPos);
        currentDeahtCam.GetComponent<DeathCam>().newLoc = shotFrom;
        currentDeahtCam.GetComponent<DeathCam>().deathLoc = deathPos;

        if (killsThisLive >= 5 && killsThisLive < 10)
            PlayerInfoManager.killStreak5s++;
        else if (killsThisLive >= 10 && killsThisLive < 15)
            PlayerInfoManager.killStreak10s++;
        else if (killsThisLive >= 15)
            PlayerInfoManager.killStreak15s++;


        StartCoroutine(Respawn());
    }
    IEnumerator Respawn()
    {
        float timer = respawnTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(currentDeahtCam);
        Destroy(currentFakePlayer);

        Transform spawnPoint = GameManager.Instance.GetRespawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<PlayerShoot>().SetUp(this);
        player.GetComponent<Health>().pm = this;

        pointsThisLive = 0;
        killsThisLive = 0;
        foreach (ScoreStreaksHolder s in scoreStreakHolders)
        {
            s.addedPoints = 0;
        }
        SyncBar();

        HudManager.Instance.Death(true);
    }
}
