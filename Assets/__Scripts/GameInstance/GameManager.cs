using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public GameObject PlayerPrefab;

    public GameState state = GameState.SetupSettlement;

    public GameObject playerGameObject;

    public Player[] players;

    public GameObject canvas;

    public Text winnerNameText;


    private int currentPlayer;

    private int[] playerActorIDs;

    private int preGameCounter = 0;
    private int gameCounter;

    private Dictionary<eCommodity, int[]> cityImprovementHolder = new Dictionary<eCommodity, int[]>()
    {
        { eCommodity.Coin, new int[]{-1, 4} },
        { eCommodity.Paper, new int[]{-1, 4} },
        { eCommodity.Silk, new int[]{-1, 4} },
    };

    private Dictionary<int,int> playerLongestRoads = new Dictionary<int, int>();
    private int playerHoldingLongestRoad = -1;

    private eLongestRoadState longestRoadState = eLongestRoadState.Game;
    private List<int> tiedRoadPlayers = new List<int>();
    private int tiedRoadsToPass;

    public int CurrentPlayer { get { return playerActorIDs[currentPlayer]; } }

    void Awake()
    {
        instance = this;
        players = PhotonNetwork.PlayerList;
        playerActorIDs = new int[players.Length];
        for (int i = 0; i<players.Length; i++)
        {
            playerLongestRoads.Add(players[i].ActorNumber, 0);
            playerActorIDs[i] = players[i].ActorNumber;
        }
    }

    void Start()
    {
        if (PlayerSetup.LocalPlayerInstance == null)
        {
            playerGameObject = PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, Quaternion.identity);
        }
    }



    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }




    void OnEvent(EventData photonEvent)
    {
        object[] data;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.SetRandomPlayer:
                data = (object[])photonEvent.CustomData;
                currentPlayer = (int)data[0];
                break;
            case (byte)RaiseEventsCode.PassTurn:
                int previousPlayer = currentPlayer;
                SetCurrentPlayer();
                if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;
                if (currentPlayer != previousPlayer)
                {
                    switch (state)
                    {
                        case GameState.SetupSettlement:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupSettlement, CurrentPlayer);
                            break;

                        case GameState.SetupCity:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupCity, CurrentPlayer);
                            break;
                        default:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.StartTurn, CurrentPlayer);
                            break;
                    }
                }
                else
                {
                    switch (state)
                    {
                        case GameState.SetupCity:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupCity, CurrentPlayer);
                            break;

                        case GameState.Friendly:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.StartTurn, CurrentPlayer);
                            break;
                    }
                }
                break;
            case (byte)RaiseEventsCode.CheckImporveCity:
                data = (object[])photonEvent.CustomData;
                eCommodity commodity = (eCommodity)data[0];
                int improvementLevel = (int)data[1];
                if(cityImprovementHolder[commodity][0] == photonEvent.Sender)
                {
                    cityImprovementHolder[commodity][1] = improvementLevel;
                    return;
                }
                else if (cityImprovementHolder[commodity][0] == -1)
                {
                    cityImprovementHolder[commodity][0] = photonEvent.Sender;
                    Utils.RaiseEventForPlayer(RaiseEventsCode.ImproveCity, photonEvent.Sender, new object[] { data[0] });
                }
                else if(cityImprovementHolder[commodity][1] < improvementLevel)
                {
                    Utils.RaiseEventForPlayer(RaiseEventsCode.LoseImproveCity, cityImprovementHolder[commodity][0], new object[] { data[0] });
                    cityImprovementHolder[commodity][0] = photonEvent.Sender;
                    cityImprovementHolder[commodity][1] = improvementLevel;
                    Utils.RaiseEventForPlayer(RaiseEventsCode.TakeImproveCity, photonEvent.Sender, new object[] { data[0] });
                }
                break;
            case (byte)RaiseEventsCode.ActivateRobber:
                state = GameState.Playing;
                break;
            case (byte)RaiseEventsCode.SetLongestRoad:
                data = (object[])photonEvent.CustomData;
                HandleLongestRoad((int)data[0], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.GameOver:
                data = (object[])photonEvent.CustomData;
                GameOver((string)data[0], (string)data[1]);
                break;

        }

    }


    private void SetCurrentPlayer()
    {
        if (state == GameState.SetupSettlement || state == GameState.SetupCity)
        {
            preGameCounter += 1;
            if (preGameCounter == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                state = GameState.SetupCity;
            }
            else if (preGameCounter == 2 * PhotonNetwork.CurrentRoom.PlayerCount)
            {
                state = GameState.Friendly;
            }
            else if (preGameCounter < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                currentPlayer = ( (currentPlayer + 1) % PhotonNetwork.CurrentRoom.PlayerCount);
            }
            else if (preGameCounter > PhotonNetwork.CurrentRoom.PlayerCount)
            {
                currentPlayer -= 1;
                if (currentPlayer == -1) currentPlayer = PhotonNetwork.CurrentRoom.PlayerCount - 1;
            }
        }
        else
        {
            gameCounter += 1;
            currentPlayer = ((currentPlayer + 1) % PhotonNetwork.CurrentRoom.PlayerCount);
            if (gameCounter == 2 * PhotonNetwork.CurrentRoom.PlayerCount)
            {
                state = GameState.PreAttack;
                Utils.RaiseEventForAll(RaiseEventsCode.ActivateBarbarians);
            }
        }
    }


    public void StartGameSetup()
    {
        SetStartingPlayer();

        Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupSettlement, CurrentPlayer);
    }



    public void SetStartingPlayer()
    {
        currentPlayer = Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount);

        Utils.RaiseEventForAll(RaiseEventsCode.SetRandomPlayer, new object[] { currentPlayer });
    }


    public void HandleLongestRoad(int roadLength, int sender)
    {
        int oldLength = playerLongestRoads[sender];
        playerLongestRoads[sender] = roadLength;
        if (roadLength <= 4)
            return;

        switch (longestRoadState)
        {
            case eLongestRoadState.Game:
                longestRoadState = eLongestRoadState.Player;
                playerHoldingLongestRoad = sender;
                Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
                break;

            case eLongestRoadState.Player:
                if (playerHoldingLongestRoad == sender)
                {
                    if (roadLength < oldLength)
                        RoadShorten(sender);
                }
                else
                {
                    if (roadLength > playerLongestRoads[playerHoldingLongestRoad])
                    {
                        Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, playerHoldingLongestRoad, new object[] { -2 });
                        playerHoldingLongestRoad = sender;
                        Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
                    }
                }
                break;
            case eLongestRoadState.Tie:
                if(roadLength > tiedRoadsToPass)
                {
                    playerHoldingLongestRoad = sender;
                    longestRoadState = eLongestRoadState.Player;
                    Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
                }
                break;
        }
    }


    private void RoadShorten(int sender)
    {
        int maxLength = 5;
        foreach (KeyValuePair<int, int> entry in playerLongestRoads)
        {
            if (entry.Value > maxLength)
            {
                tiedRoadPlayers.Clear();
                tiedRoadPlayers.Add(entry.Key);
            }
            else if (entry.Value == maxLength)
            {
                tiedRoadPlayers.Add(entry.Key);
            }
        }
        if(tiedRoadPlayers.Count == 0)
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, playerHoldingLongestRoad, new object[] { -2 });
            longestRoadState = eLongestRoadState.Game;
        }
        else if (tiedRoadPlayers.Count == 1)
        {
            if (tiedRoadPlayers[0] != playerHoldingLongestRoad)
            {
                Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, playerHoldingLongestRoad, new object[] { -2 });

                playerHoldingLongestRoad = tiedRoadPlayers[0];
                Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
            }
            tiedRoadPlayers.Clear();
        }
        else
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, playerHoldingLongestRoad, new object[] { -2 });
            longestRoadState = eLongestRoadState.Tie;
            tiedRoadsToPass = maxLength;
            tiedRoadPlayers.Clear();
        }
    }



    public void GameOver(string playerName, string playerColor)
    {
        winnerNameText.text = playerName;
        winnerNameText.color = Utils.Name_To_Color(playerColor);
        canvas.SetActive(true);

    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

}
