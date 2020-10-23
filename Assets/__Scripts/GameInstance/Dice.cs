using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class Dice : MonoBehaviourPun
{


    private BoxCollider bColl;
    private Transform yellowDice;
    private Transform redDice;
    private Transform eventDice;

    private PlayerSetup playerSetup;
    private TurnManager turnManager;

    private Barbarians barbarians;

    private int score;

    /// <summary>
    /// 
    /// </summary>
    private GreenLvl3Players greenLvl3Players = new GreenLvl3Players();

    private List<Vector3> quats = new List<Vector3>() {
        new Vector3(90, 0 , 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0), new Vector3(180, 0, 0), new Vector3(0, 270, 0), new Vector3(270, 0, 0)
    };

    void Awake() {
        bColl = GetComponent<BoxCollider>();
        yellowDice = transform.Find("YellowDice");
        redDice = transform.Find("RedDice");
        eventDice = transform.Find("EventDice");
        eventDice.gameObject.SetActive(false);
        bColl.enabled = false;
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
            case (byte)RaiseEventsCode.AddGreenPlayer:
                greenLvl3Players.Players.Add(new GreenLvl3Player(photonEvent.Sender));
                break;
            case (byte)RaiseEventsCode.GreenPlayerResponse:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                bool needToPick = (bool)data[0];
                if (needToPick)
                {
                    Utils.RaiseEventForPlayer(RaiseEventsCode.PickCard, photonEvent.Sender);
                }
                else
                {
                    Utils.RaiseEventForPlayer(RaiseEventsCode.AddCachedCards, photonEvent.Sender);
                    FinishPicking(photonEvent.Sender);
                }
                break;
            case (byte)RaiseEventsCode.FinishPickCard:
                if (!photonView.IsMine) return;
                FinishPicking(photonEvent.Sender);
                break;
        }
    }

    void OnMouseDown() {
        //bColl.enabled = false;
        int yellowDiceNum = Random.Range(0, 6);
        int redDiceNum = Random.Range(0, 6);
        int eventDiceNum = Random.Range(0, 6);
        score = yellowDiceNum + redDiceNum + 2;

        this.photonView.RPC("SetDice", RpcTarget.AllViaServer, yellowDiceNum, redDiceNum, eventDiceNum);

        if(GameManager.instance.state > GameState.Friendly)
        {
            if(eventDiceNum <3)
                barbarians.photonView.RPC("Advance", RpcTarget.AllBufferedViaServer, score);
            else
                SendNumber(score);
        }
        else
            SendNumber(score);


    }

    public void SendNumber(int diceNumber)
    {
        if (diceNumber == 7)
        {
            // UNCOMMENT TO ENABLE ROBBER!
            Utils.RaiseEventForAll(RaiseEventsCode.SevenRolled);
        }
        else
        {
            if (greenLvl3Players.IsEmpty())
            {
                Utils.RaiseEventForAll(RaiseEventsCode.MatchTilesToDice, new object[] { diceNumber.ToString() });
                turnManager.GainControl();
            }
            else
            {
                HashSet<int> notGreenPlayers = new HashSet<int>(GameManager.instance.players.Select(x=>x.ActorNumber));
                int[] greenPlayers = greenLvl3Players.GetActorIDs().ToArray();
                notGreenPlayers.ExceptWith(greenPlayers);
                Utils.RaiseEventForGroup(RaiseEventsCode.CheckIfNeedToPick, greenPlayers, new object[] { diceNumber.ToString() });
                Utils.RaiseEventForGroup(RaiseEventsCode.MatchTilesToDice, notGreenPlayers.ToArray(), new object[] { diceNumber.ToString() });
            }
        }
    }


    private void FinishPicking(int actorID)
    {
        greenLvl3Players.SetPlayerFinishByID(actorID);
        if (greenLvl3Players.AllFinished())
        {
            greenLvl3Players.Reset();
            Debug.Log("Return Control to Rolling Player");
            turnManager.GainControl();
        }
    }
    public void SetCollider(bool flag)
    {
        bColl.enabled = flag;
    }

    public void ActivateEventDice()
    {
        eventDice.gameObject.SetActive(true);
    }


    [PunRPC]
    public void SetDice(int yellowDice, int redDice, int eventDice)
    {
        this.yellowDice.localRotation = Quaternion.Euler(quats[yellowDice]);
        this.redDice.localRotation = Quaternion.Euler(quats[redDice]);
        this.eventDice.localRotation = Quaternion.Euler(quats[eventDice]);
    }

    [PunRPC]
    public void Init(int barbariansID)
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
        turnManager = PlayerSetup.LocalPlayerInstance.GetComponent<TurnManager>();

        transform.SetParent(playerSetup.canvas.transform);

        barbarians = PhotonView.Find(barbariansID).GetComponent<Barbarians>();
    }
}
