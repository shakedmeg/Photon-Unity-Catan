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




    private GreenLvl3Players greenLvl3Players = new GreenLvl3Players();

    


    private Vector3 s0, s1;
    private bool scaling = false;
    private float scaleStart;
    private bool loopScale;
    protected Vector3 scaleToFinishAt;



    void Awake() {
        bColl = GetComponent<BoxCollider>();
        yellowDice = transform.Find("YellowDice");
        redDice = transform.Find("RedDice");
        eventDice = transform.Find("EventDice");
        eventDice.gameObject.SetActive(false);
        bColl.enabled = false;
    }

    void Update()
    {
        ScaleUpDown();
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
                    if (!greenLvl3Players.FirstOne)
                        Utils.RaiseEventForPlayer(RaiseEventsCode.SetPlayerPanel, GameManager.instance.CurrentPlayer, new object[] { false });
                    greenLvl3Players.FirstOne = true;
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
                Utils.RaiseEventForPlayer(RaiseEventsCode.SetPlayerPanel, GameManager.instance.CurrentPlayer, new object[] { true });
                FinishPicking(photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.SendDiceScore:
                SendNumber(score);
                break;
        }
    }

    void OnMouseDown() {
        turnManager.ActivateAlchemists(false);
        StopScaling();
        int yellowDiceNum = Random.Range(0, 6);
        int redDiceNum = Random.Range(0, 6);
        int eventDiceNum = Random.Range(0, 6);

        HandleResults(yellowDiceNum, redDiceNum, eventDiceNum);

    }

    public void HandleResults(int yellowDiceNum, int redDiceNum, int eventDiceNum)
    {        
        this.photonView.RPC("SetDice", RpcTarget.AllViaServer, yellowDiceNum, redDiceNum, eventDiceNum);
        score = yellowDiceNum + redDiceNum + 2;

        if (GameManager.instance.state > GameState.Friendly)
        {
            if (eventDiceNum < 3)
                barbarians.photonView.RPC("Advance", RpcTarget.AllBufferedViaServer, score);
            else
                Utils.RaiseEventForAll(RaiseEventsCode.DeserveDevelopmentCard, new object[] { eventDiceNum, redDiceNum + 1 });
        }
        else
            SendNumber(score);

    }

    public void SendNumber(int diceNumber)
    {
        if (diceNumber == 7)
        {
            // COMMENT TO DISABLE ROBBER!
            playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
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
            Utils.RaiseEventForPlayer(RaiseEventsCode.GainTurnControl, GameManager.instance.CurrentPlayer);
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


    public void InitScaleUpDown(Vector3 s0, Vector3 s1)
    {
        bColl.enabled = true;
        this.s0 = s0;
        this.s1 = s1;
        scaleStart = Time.time;
        scaling = true;
        loopScale = true;
        scaleToFinishAt = s0;
    }

    protected void ScaleUpDown()
    {
        bool assign = true;
        if (scaling)
        {
            float u = (Time.time - scaleStart) / Consts.ScaleTime;
            if (u >= 1)
            {
                u = 1;
                if (loopScale)
                {
                    transform.localScale = (1 - u) * s0 + u * s1;
                    assign = false;
                    Vector3 temp = s0;
                    s0 = s1;
                    s1 = temp;
                    scaleStart = Time.time;
                }
                else
                {
                    scaling = false;
                }
            }
            if (assign)
                transform.localScale = (1 - u) * s0 + u * s1;
        }
    }

    public virtual void StopScaling()
    {
        bColl.enabled = false;
        scaling = false;
        transform.localScale = scaleToFinishAt;
    }

    [PunRPC]
    public void SetDice(int yellowDice, int redDice, int eventDice)
    {
        this.yellowDice.localRotation = Quaternion.Euler(Consts.Quats[yellowDice]);
        this.redDice.localRotation = Quaternion.Euler(Consts.Quats[redDice]);
        this.eventDice.localRotation = Quaternion.Euler(Consts.Quats[eventDice]);
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
