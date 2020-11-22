using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

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

    public  Dictionary<eCommodity, int[]> cityImprovementHolder = new Dictionary<eCommodity, int[]>()
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


    public Dictionary<int, int> playerPoints = new Dictionary<int, int>();

    public Dictionary<eCommodity, List<eDevelopmentCardsTypes>> developmentCards;

    public int CurrentPlayer { get { return playerActorIDs[currentPlayer]; } }

    public Dictionary<int, bool> playerDevelopmentCardRes = new Dictionary<int, bool>();



    void Awake()
    {
        instance = this;
        players = PhotonNetwork.PlayerList;
        playerActorIDs = new int[players.Length];
        for (int i = 0; i<players.Length; i++)
        {
            playerLongestRoads.Add(players[i].ActorNumber, 0);
            playerPoints.Add(players[i].ActorNumber, 0);
            playerActorIDs[i] = players[i].ActorNumber;
            playerDevelopmentCardRes.Add(players[i].ActorNumber, false);
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
            case (byte)RaiseEventsCode.SendMapData:
                if (!PhotonNetwork.IsMasterClient) return;
                data = (object[])photonEvent.CustomData;
                developmentCards = new Dictionary<eCommodity, List<eDevelopmentCardsTypes>>();
                developmentCards.Add(eCommodity.Coin, new List<eDevelopmentCardsTypes>());
                developmentCards.Add(eCommodity.Paper, new List<eDevelopmentCardsTypes>());
                developmentCards.Add(eCommodity.Silk, new List<eDevelopmentCardsTypes>());

                for(int i = 7; i<10; i++)
                {
                    foreach(int type in (int[])data[i])
                    {
                        developmentCards[(eCommodity)i - 2].Add((eDevelopmentCardsTypes)type);
                    }
                }
                break;
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
                    if(PhotonNetwork.IsMasterClient)
                        Utils.RaiseEventForPlayer(RaiseEventsCode.GainTurnControl, photonEvent.Sender);
                    return;
                }
                else if (cityImprovementHolder[commodity][0] == -1)
                {
                    cityImprovementHolder[commodity][0] = photonEvent.Sender;
                    if (PhotonNetwork.IsMasterClient)
                        Utils.RaiseEventForPlayer(RaiseEventsCode.ImproveCity, photonEvent.Sender, new object[] { data[0] });
                }
                else if(cityImprovementHolder[commodity][1] < improvementLevel)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        Utils.RaiseEventForPlayer(RaiseEventsCode.LoseImproveCity, cityImprovementHolder[commodity][0], new object[] { data[0] });
                        Utils.RaiseEventForPlayer(RaiseEventsCode.TakeImproveCity, photonEvent.Sender, new object[] { data[0] });
                    }
                    cityImprovementHolder[commodity][0] = photonEvent.Sender;
                    cityImprovementHolder[commodity][1] = improvementLevel;
                }
                else
                {
                    Utils.RaiseEventForPlayer(RaiseEventsCode.GainTurnControl, photonEvent.Sender);
                }
                break;
            case (byte)RaiseEventsCode.ActivateRobber:
                state = GameState.Playing;
                break;
            case (byte)RaiseEventsCode.SetLongestRoad:
                data = (object[])photonEvent.CustomData;
                HandleLongestRoad((int)data[0], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.UpdatePointsForAll:
                data = (object[])photonEvent.CustomData;
                playerPoints[photonEvent.Sender] += (int)data[0];
                break;
            case (byte)RaiseEventsCode.GiveDevelopmentCard:
                if (!PhotonNetwork.IsMasterClient) return;
                data = (object[])photonEvent.CustomData;
                if ((bool)data[0])
                {
                    GetAndSendDevelopmentCard((eCommodity)data[1], photonEvent.Sender, RaiseEventsCode.SendDevelopmentCardFromRoll);
                }
                else
                {
                    playerDevelopmentCardRes[photonEvent.Sender] = true;
                    ContinueAfterDevelopmentCardsHandout();
                }
                break;
            case (byte)RaiseEventsCode.ReturnDevelopmentCardAfterUsage:
                data = (object[])photonEvent.CustomData;
                ReturnDevelopmentCard((eDevelopmentCardsTypes)data[0], (eCommodity)data[1]);
                break;
            case (byte)RaiseEventsCode.ReturnDevelopmentCard:
                data = (object[])photonEvent.CustomData;

                ReturnDevelopmentCard((eDevelopmentCardsTypes)data[0], (eCommodity)data[1]);

                if ((bool)data[2])
                {
                    Utils.RaiseEventForMaster(RaiseEventsCode.FinishDevelopmentCardWinHandout);
                }
                else
                {
                    playerDevelopmentCardRes[photonEvent.Sender] = true;
                    ContinueAfterDevelopmentCardsHandout();
                }
                break;
            case (byte)RaiseEventsCode.FinishDevelopmentCardRollHandout:
                playerDevelopmentCardRes[photonEvent.Sender] = true;
                ContinueAfterDevelopmentCardsHandout();
                break;
            case (byte)RaiseEventsCode.WinDevelopmentCard:
                data = (object[])photonEvent.CustomData;
                GetAndSendDevelopmentCard((eCommodity)data[1], photonEvent.Sender, RaiseEventsCode.SendDevelopmentCardFromWin);

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
                Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, sender, new object[] { true });
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
                        Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, playerHoldingLongestRoad, new object[] { false });
                        playerHoldingLongestRoad = sender;
                        Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
                        Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, sender, new object[] { true });

                    }
                }
                break;
            case eLongestRoadState.Tie:
                if(roadLength > tiedRoadsToPass)
                {
                    playerHoldingLongestRoad = sender;
                    longestRoadState = eLongestRoadState.Player;
                    Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
                    Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, sender, new object[] { true });
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
            Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, playerHoldingLongestRoad, new object[] { false });
            longestRoadState = eLongestRoadState.Game;
        }
        else if (tiedRoadPlayers.Count == 1)
        {
            if (tiedRoadPlayers[0] != playerHoldingLongestRoad)
            {
                Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, playerHoldingLongestRoad, new object[] { -2 });
                Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, playerHoldingLongestRoad, new object[] { false });

                playerHoldingLongestRoad = tiedRoadPlayers[0];
                Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, sender, new object[] { 2 });
                Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, sender, new object[] { true });

            }
            tiedRoadPlayers.Clear();
        }
        else
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, playerHoldingLongestRoad, new object[] { -2 });
            Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateLongestRoad, playerHoldingLongestRoad, new object[] { false });
            longestRoadState = eLongestRoadState.Tie;
            tiedRoadsToPass = maxLength;
            tiedRoadPlayers.Clear();
        }
    }


    private void GetAndSendDevelopmentCard(eCommodity stack, int sender, RaiseEventsCode winWay)
    {
        int cardType = (int)developmentCards[stack][0];
        developmentCards[stack].RemoveAt(0);
        Utils.RaiseEventForPlayer(winWay, sender, new object[] { cardType });
    }

    public bool AllFinished()
    {
        foreach (bool res in playerDevelopmentCardRes.Values)
        {
            if (!res) return false;
        }
        return true;
    }


    private void ContinueAfterDevelopmentCardsHandout()
    {
        if (AllFinished())
        {
            int[] actors = playerDevelopmentCardRes.Keys.ToArray();
            foreach (int actor in actors)
            {
                playerDevelopmentCardRes[actor] = false;
            }
            Utils.RaiseEventForPlayer(RaiseEventsCode.SendDiceScore, CurrentPlayer);
        }
    }

    private void ReturnDevelopmentCard(eDevelopmentCardsTypes dCardType, eCommodity stack)
    {
        developmentCards[stack].Add(dCardType);
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
