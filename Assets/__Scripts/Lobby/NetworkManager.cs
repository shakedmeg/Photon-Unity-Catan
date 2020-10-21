using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("Login UI")]
    public GameObject LoginUIPanel;
    public TMP_InputField playerNameInputField;


    [Header("Connecting Info Panel")]
    public GameObject ConnectingInfoUIPanel;

    [Header("Creating Room Info Panel")]
    public GameObject CreatingRoomInfoUIPanel;

    [Header("RoomOptions Panel")]
    public GameObject RoomOptionsUIPanel;
    public GameObject roomListEntryPrefab;
    public GameObject roomListParentGameobject;


    [Header("Create Room Panel")]
    public GameObject CreateRoomUIPanel;
    public TMP_InputField RoomNameInputField;
    public TMP_Dropdown dropdown;
    public string GameMode;


    [Header("Inside Room Panel")]
    public GameObject InsideRoomUIPanel;
    public TMP_Text roomInfoText;
    public GameObject PlayerListPrefab;
    public GameObject PlayerListContent;
    public GameObject StartGameButton;
    public TMP_Text GameModeText;







    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameObjects;
    private Dictionary<int, GameObject> playerListGameObjects;



    #region UNITY Methods


    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(LoginUIPanel.name);

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {

    }


    #endregion



    #region UI Callback Methods
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInputField.text;

        if (!string.IsNullOrEmpty(playerName))
        {

            ActivatePanel(ConnectingInfoUIPanel.name);


            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("Player name is invalid");
        }
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(RoomOptionsUIPanel.name);
    }

    public void OnCreateRoomButtonClicked()
    {
        if (GameMode == null) return;
        ActivatePanel(CreatingRoomInfoUIPanel.name);
        string roomName = RoomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room" + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        string[] roomPropsInLobby = { "gm" }; //gm = game mode

        // two game modes
        // 1. base = "b"
        // 2. knights = "k"

        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", GameMode }, { Consts.ROOM_COLORS, Consts.COLORS_NAMES }, { Consts.COLORS_OWNER, -1 } };

        roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
        roomOptions.CustomRoomProperties = customRoomProperties;

        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnStartGameButtonClicked()
    {
        GameObject.Find("Canvas/MainPanel/InsideRoomPanel/StartGameButton").gameObject.GetComponent<Button>().interactable = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("k"))
            {
                // Knights game mode
                PhotonNetwork.LoadLevel("Knights_Mode");
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("b"))
            {
                // Basic game mode
                //PhotonNetwork.LoadLevel("DeathRaceScene");
            }
        }
    }

    #endregion


    #region Photon Callbacks


    public override void OnConnected()
    {
        Debug.Log("We connected to internet");
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(RoomOptionsUIPanel.name);
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
    }


    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);

        ActivatePanel(InsideRoomUIPanel.name);
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))
        {
            roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                " Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " +
                PhotonNetwork.CurrentRoom.MaxPlayers;



            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("k"))
            {
                // Knights game mode
                GameModeText.text = "KNIGHTS";
            }
            else
            {
                // Basic game mode
                GameModeText.text = "BASIC";
            }


            if (playerListGameObjects == null)
            {
                playerListGameObjects = new Dictionary<int, GameObject>();

            }

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                GameObject playerListGameObject = Instantiate(PlayerListPrefab);
                playerListGameObject.transform.SetParent(PlayerListContent.transform);
                playerListGameObject.transform.localScale = Vector3.one;
                playerListGameObject.GetComponent<PlayerListEntryInitializer>().Initialize(player.ActorNumber, player.NickName);


                object isPlayerReady;
                if (player.CustomProperties.TryGetValue(Consts.PLAYER_READY, out isPlayerReady))
                {
                    playerListGameObject.GetComponent<PlayerListEntryInitializer>().SetPlayerReady((bool)isPlayerReady);
                }

                playerListGameObjects.Add(player.ActorNumber, playerListGameObject);


            }
        }
        StartGameButton.SetActive(false);

    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();


        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                //update cachedRoomlist
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                else
                {
                    //add the new room to the cached room list
                    cachedRoomList.Add(room.Name, room);
                }
            }
        }

        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryPrefab);
            roomListEntryGameObject.transform.SetParent(roomListParentGameobject.transform);
            roomListEntryGameObject.transform.localScale = Vector3.one;


            object gameModeName;
            string type;

            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;

            room.CustomProperties.TryGetValue("gm", out gameModeName);
            if (gameModeName.ToString().Equals("k"))
            {
                type = "Knights";
            }
            else
            {
                type = "Basic";
            }
            roomListEntryGameObject.transform.Find("RoomTypeText").GetComponent<Text>().text = type;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            roomListGameObjects.Add(room.Name, roomListEntryGameObject);
        }

    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
        " Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " +
        PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerListGameObject = Instantiate(PlayerListPrefab);
        playerListGameObject.transform.SetParent(PlayerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;
        playerListGameObject.GetComponent<PlayerListEntryInitializer>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);

        StartGameButton.SetActive(CheckPlayersReady());

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
        " Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " +
        PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        StartGameButton.SetActive(CheckPlayersReady());

        AddColor(otherPlayer);

    }

    public override void OnLeftRoom()
    {
        ActivatePanel(RoomOptionsUIPanel.name);
        foreach (GameObject playerListGameObject in playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        GameObject playerListGameObject;
        if (playerListGameObjects.TryGetValue(targetPlayer.ActorNumber, out playerListGameObject))
        {
            //object isPlayerReady;
            object changedData;
            if (changedProps.TryGetValue(Consts.PLAYER_READY, out changedData))
            {
                playerListGameObject.GetComponent<PlayerListEntryInitializer>().SetPlayerReady((bool)changedData);
            }

            if(changedProps.TryGetValue(Consts.PLAYER_COLOR, out changedData))
            {
                playerListGameObject.GetComponent<PlayerListEntryInitializer>().SetColorForOthers((string)changedData);
            }
        }
        StartGameButton.SetActive(CheckPlayersReady());

    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.SetActive(CheckPlayersReady());
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.OpResponseReceived += NetworkingClientOnOpResponseReceived;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.OpResponseReceived -= NetworkingClientOnOpResponseReceived;
    }


    #endregion



    #region Private Methods
    void OnJoinRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName);
    }



    void ClearRoomListView()
    {
        foreach (var roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }
        roomListGameObjects.Clear();
    }



    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object isPlayerReday;
            if (player.CustomProperties.TryGetValue(Consts.PLAYER_READY, out isPlayerReday))
            {
                if (!(bool)isPlayerReday)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;

    }



    private void NetworkingClientOnOpResponseReceived(OperationResponse opResponse)
    {
        if (opResponse.OperationCode == OperationCode.SetProperties &&
            opResponse.ReturnCode == ErrorCode.InvalidOperation)
        {
            Debug.Log(opResponse.DebugMessage);
            // CAS failure
        }
    }

    /// <summary>
    /// This will add a Color back from a user to the room custom properties
    /// </summary>
    /// <param name="otherPlayer"> player that left the room </param>
    private void AddColor(Player otherPlayer)
    {
        // Get the room colors
        object data;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Consts.ROOM_COLORS, out data);
        List<string> allowedColors = new List<string>((string[])data);

        // Get the player color
        object playerData;
        otherPlayer.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out playerData);
        string colorName = (string)playerData;

        // Insert the player color back to the room colors
        allowedColors.Insert(0, colorName);
        int currentOwner = (int)PhotonNetwork.CurrentRoom.CustomProperties[Consts.COLORS_OWNER];
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.ROOM_COLORS, allowedColors.ToArray() }, { Consts.COLORS_OWNER, PhotonNetwork.LocalPlayer.ActorNumber } };
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.COLORS_OWNER, currentOwner } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties, expectedCustomRoomProperties);
    }

    #endregion







    #region Public Methods
    public void ActivatePanel(string panelNameToBeActivated)
    {
        LoginUIPanel.SetActive(LoginUIPanel.name.Equals(panelNameToBeActivated));
        ConnectingInfoUIPanel.SetActive(ConnectingInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreatingRoomInfoUIPanel.SetActive(CreatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreateRoomUIPanel.SetActive(CreateRoomUIPanel.name.Equals(panelNameToBeActivated));
        RoomOptionsUIPanel.SetActive(RoomOptionsUIPanel.name.Equals(panelNameToBeActivated));
        InsideRoomUIPanel.SetActive(InsideRoomUIPanel.name.Equals(panelNameToBeActivated));
    }


    public void SetGameMode()
    {
        if (dropdown.value == 0)
        {
            GameMode = "k";
        }
        else
        {
            GameMode = "b";
        }

    }

    #endregion


}