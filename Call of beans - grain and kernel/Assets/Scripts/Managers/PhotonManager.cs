using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    public static bool startUp;

    public static PhotonManager Instance;
    public static byte playerId;
    public static int team;
    public static int teamPlace;

    [SerializeField] TMP_Text playersOnlineText, currentVersionText;
    public TMP_InputField playerNameInput;

    [Header("Multiplayer")]
    [SerializeField] TMP_Text lobbyNameText;
    [SerializeField] GameObject startGameButton, settingsGameButton;

    [SerializeField] Transform playerList;
    [SerializeField] GameObject playerInfoPrefab;

    [Header("Zombies")]
    public static bool zombies;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        MenuManager.Instance.SwitchMenu("loading");
    }
    private void Start()
    {
        currentVersionText.text = "version: " + PhotonNetwork.AppVersion;
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startGameButton.SetActive(true);
                settingsGameButton.SetActive(true);
                PhotonNetwork.CurrentRoom.IsOpen = true;
            }

            pv.RPC("RPC_SpawnPlayerHolder", RpcTarget.AllBuffered, PhotonNetwork.NickName, PlayerInfoManager.Instance.level, PhotonNetwork.LocalPlayer);

            MenuManager.Instance.SwitchMenu("multiplayerLobby");
            lobbyNameText.text = "Lobby " + PhotonNetwork.CurrentRoom.Name;
            return;
        }
        PhotonNetwork.ConnectUsingSettings();
    }
    private void Update()
    {
        if (PhotonNetwork.IsConnected)
            playersOnlineText.text = "Players online: " + PhotonNetwork.CountOfPlayersOnMaster.ToString();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
        if (PhotonNetwork.NickName == "")
        {
            PhotonNetwork.NickName = "Player" + Random.Range(0, 999).ToString("000");
        }
        playerNameInput.text = PhotonNetwork.NickName;

        MenuManager.Instance.SwitchMenu("main");
    }
    public void CreateLobby(int mode)
    {
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties["Mode"] = 0;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomProperties = customProperties;

        PhotonNetwork.CreateRoom(Random.Range(1, 9999).ToString("0000"), roomOptions);
        MenuManager.Instance.SwitchMenu("loading");
    }
    public override void OnJoinedRoom()
    {
        startGameButton.SetActive(false);
        settingsGameButton.SetActive(false);

        if (!zombies)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startGameButton.SetActive(true);
                settingsGameButton.SetActive(true);
            }

            pv.RPC("RPC_SpawnPlayerHolder", RpcTarget.AllBuffered, PhotonNetwork.NickName, PlayerInfoManager.Instance.level, PhotonNetwork.LocalPlayer);

            MenuManager.Instance.SwitchMenu("multiplayerLobby");
            lobbyNameText.text = "Lobby " + PhotonNetwork.CurrentRoom.Name;
            playerId = PhotonNetwork.CurrentRoom.PlayerCount;
            playerId--;
        }
        else
        {
            PhotonNetwork.LoadLevel(5);
        }
    }
    [PunRPC]
    void RPC_SpawnPlayerHolder(string _name, int _level, Player _p)
    {
        PlayerHolder holder = Instantiate(playerInfoPrefab, playerList).GetComponent<PlayerHolder>();
        holder.SetUp(_name, _level, _p);
    }
    public void JoinLobby(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        MenuManager.Instance.SwitchMenu("loading");
    }
    public void QuitLobby()
    {
        MenuManager.Instance.SwitchMenu("loading");
        PhotonNetwork.LeaveRoom();
        foreach(Transform t in playerList)
        {
            Destroy(t.gameObject);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (Transform t in playerList)
        {
            if (t.GetComponent<PlayerHolder>().player == otherPlayer)
                Destroy(t.gameObject);
        }
    }
    public void LoadZombies()
    {
        zombies = true;
        PhotonNetwork.CreateRoom("Zombies");
    }
    public void ChangeName(string _name)
    {
        string newName = _name.Replace(" ", "-");

        PhotonNetwork.NickName = newName;
        PlayerPrefs.SetString("PlayerName", newName);
    }
    public void ChangeLevel()
    {
        pv.RPC("RPC_ChangeLevel", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, PhotonNetwork.NickName, PlayerInfoManager.Instance.level);
    }
    [PunRPC]
    void RPC_ChangeLevel(Player p, string name ,int newLevel)
    {
        foreach(Transform t in playerList)
        {
            if(t.GetComponent<PlayerHolder>().player == p)
            {
                t.GetComponent<PlayerHolder>().SetUp(name, newLevel, p);
            }
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
