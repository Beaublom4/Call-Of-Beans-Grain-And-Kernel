using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayerManager : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] GameObject joinLobbyPanel;
    string lobbyName;

    [Header("Starting")]
    [SerializeField] TMP_Text StartButtonText;
    [SerializeField] TMP_Text CountDownText;
    float countDownTimer;
    bool starting;

    [Header("Match settings")]
    [SerializeField] int gamemodeId;
    [SerializeField] int mapId;
    [SerializeField] int mapIdOffset;
    [SerializeField] TMP_Text gamemodeName;
    [SerializeField] TMP_Text mapName;
    [SerializeField] Image mapIcon;

    [SerializeField] TMP_Text gamemodeLobbyText;
    [SerializeField] Image mapLobbyIcon;

    public static int staticGamemodeId;
    public static int staticMapId;
    public static bool staticUav;
    public static bool hasPerks;

    [Header("Setting infos")]
    [SerializeField] GamemodeScrObj[] gamemodes;
    [SerializeField] string[] mapNames;
    [SerializeField] Sprite[] mapSprites;

    [Header("Teams")]
    Player[] playerTeams;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        SyncGamemode();
        SyncMap();

        if (PhotonNetwork.IsMasterClient)
        {
            ChangeGamemode(0);
        }
    }
    private void Update()
    {
        if(starting)
        {
            if(countDownTimer > 0)
            {
                countDownTimer -= Time.deltaTime;
                CountDownText.text = "Starting in: " + countDownTimer.ToString("F0");
            }
            else
            {
                starting = false;

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                    {
                        pv.RPC("RPC_SyncPlayerId", PhotonNetwork.PlayerList[i], (byte)i);
                    }
                    if (gamemodes[gamemodeId].hasTeams)
                        MakeTeams();
                    else
                        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                        {
                            pv.RPC("RPC_SyncTeam", PhotonNetwork.PlayerList[i], i, 0);
                        }
                    staticMapId = mapId;
                    PhotonNetwork.LoadLevel(mapId + mapIdOffset);
                }
                hasPerks = gamemodes[gamemodeId].hasPerks;
            }
        }
    }
    public void DoJoinPanel(bool activate)
    {
        joinLobbyPanel.SetActive(activate);
    }
    public void OnChangeJoinField(string _lobbyName)
    {
        lobbyName = _lobbyName;
    }
    public void JoinRandomLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void JoinLobby()
    {
        joinLobbyPanel.SetActive(false);
        PhotonManager.Instance.JoinLobby(lobbyName);
    }
    public void ChangeGamemode(int change)
    {
        gamemodeId += change;
        if (gamemodeId < 0)
            gamemodeId = gamemodes.Length - 1;
        else if (gamemodeId >= gamemodes.Length)
            gamemodeId = 0;

        pv.RPC("RPC_SyncGamemode", RpcTarget.AllBuffered, gamemodeId);
    }
    public void ChangeMap(int change)
    {
        mapId += change;
        if (mapId < 0)
            mapId = mapSprites.Length - 1;
        else if (mapId >= mapSprites.Length)
            mapId = 0;
        SyncMap();
    }
    public void ToggleUAV(bool toggle)
    {
        staticUav = toggle;
        pv.RPC("RPC_ToggleUAV", RpcTarget.Others, toggle);
    }
    [PunRPC]
    void RPC_ToggleUAV(bool toggle)
    {
        staticUav = toggle;
    }
    [PunRPC]
    void RPC_SyncGamemode(int _id)
    {
        gamemodeId = _id;
        staticGamemodeId = gamemodeId;
        SyncGamemode();
    }
    void SyncGamemode()
    {
        gamemodeName.text = gamemodes[gamemodeId].gamemodeName;
        gamemodeLobbyText.text = gamemodes[gamemodeId].gamemodeName;
    }
    void SyncMap()
    {
        mapName.text = mapNames[mapId];
        mapIcon.sprite = mapSprites[mapId];
        mapLobbyIcon.sprite = mapSprites[mapId];
        pv.RPC("RPC_SyncMap", RpcTarget.OthersBuffered, mapId);
    }
    [PunRPC]
    void RPC_SyncMap(int mapId)
    {
        mapLobbyIcon.sprite = mapSprites[mapId];
    }
    public void StartMatch()
    {
        pv.RPC("RPC_StartMatch", RpcTarget.All);
    }
    [PunRPC]
    void RPC_StartMatch()
    {
        if (!starting)
        {
            countDownTimer = 5;
            starting = true;
            CountDownText.gameObject.SetActive(true);
            StartButtonText.text = "Cancel";
        }
        else
        {
            CountDownText.gameObject.SetActive(false);
            starting = false;
            StartButtonText.text = "Start Match";
        }
    }
    void MakeTeams()
    {
        playerTeams = new Player[PhotonNetwork.CurrentRoom.PlayerCount];

        foreach(Player p in PhotonNetwork.PlayerList)
        {
            MakeTeam(p);
        }
        int halve = Mathf.CeilToInt(PhotonNetwork.CurrentRoom.PlayerCount / 2);
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int team = 0;
            int placeInTeam = i;
            if (i >= halve)
            {
                team = 1;
                placeInTeam -= halve;
            }
            pv.RPC("RPC_SyncTeam", playerTeams[i], team, placeInTeam);
        }
    }
    void MakeTeam(Player currentPlayer)
    {
        int randomPlace = Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount);
        if (playerTeams[randomPlace] == null)
            playerTeams[randomPlace] = currentPlayer;
        else
            MakeTeam(currentPlayer);
    }
    [PunRPC]
    void RPC_SyncTeam(int team, int gotTeamNum)
    {
        PhotonManager.team = team;
        PhotonManager.teamPlace = gotTeamNum;
    }
    [PunRPC]
    void RPC_SyncPlayerId(byte _id)
    {
        PhotonManager.playerId = _id;
    }
}
