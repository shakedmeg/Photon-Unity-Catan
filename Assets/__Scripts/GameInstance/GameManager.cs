using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


public class GameManager : MonoBehaviourPun
{
    public static GameManager instance;

    public GameObject PlayerPrefab;

    public GameState state = GameState.SetupSettlement;

    public GameObject playerGameObject;

    public Player[] players;

    public bool[] finishedThrowing;

    private int currentPlayer;

    private int preGameCounter = 0;
    private int gameCounter;


    public int CurrentPlayer { get { return currentPlayer; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            players = PhotonNetwork.PlayerList;
            finishedThrowing = new bool[players.Length];
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (PlayerSetup.LocalPlayerInstance == null)
        {
            playerGameObject = PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, Quaternion.identity);
        }
    }



    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
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
                            Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupSettlement, currentPlayer);
                            break;

                        case GameState.SetupCity:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupCity, currentPlayer);
                            break;
                        default:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.StartTurn, currentPlayer);
                            break;
                    }
                }
                else
                {
                    switch (state)
                    {
                        case GameState.SetupCity:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupCity, currentPlayer);
                            break;

                        case GameState.Friendly:
                            Utils.RaiseEventForPlayer(RaiseEventsCode.StartTurn, currentPlayer);
                            break;
                    }
                }
                break;
            case (byte)RaiseEventsCode.SevenRolled:
                for (int i = 0; i < finishedThrowing.Length; i++)
                {
                    finishedThrowing[i] = false;
                }
                break;
            case (byte)RaiseEventsCode.FinishedThrowing:
                //Debug.LogFormat("Current Player = {0}, Sender = {1}, Finished Throwing: {2} {3} {4}", CurrentPlayer, photonEvent.Sender, finishedThrowing[0], finishedThrowing[1], finishedThrowing[2]);
                finishedThrowing[photonEvent.Sender - 1] = true;
                foreach (bool finish in finishedThrowing)
                {
                    if (!finish) return;
                }
                if (PhotonNetwork.LocalPlayer.ActorNumber == photonEvent.Sender)
                {
                    //Debug.LogFormat("{0} got here", PhotonNetwork.LocalPlayer.ActorNumber);
                    Utils.RaiseEventForPlayer(RaiseEventsCode.FinishRollSeven, currentPlayer);
                }
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
                currentPlayer = (currentPlayer % PhotonNetwork.CurrentRoom.PlayerCount) + 1;
            }
            else if (preGameCounter > PhotonNetwork.CurrentRoom.PlayerCount)
            {
                currentPlayer -= 1;
                if (currentPlayer == 0) currentPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
            }
        }
        else
        {
            gameCounter += 1;
            currentPlayer = (currentPlayer % PhotonNetwork.CurrentRoom.PlayerCount) + 1;
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

        Utils.RaiseEventForPlayer(RaiseEventsCode.PreSetupSettlement, currentPlayer);
    }



    public void SetStartingPlayer()
    {
        currentPlayer = Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount) + 1;

        Utils.RaiseEventForAll(RaiseEventsCode.SetRandomPlayer, new object[] { currentPlayer });
    }
}
