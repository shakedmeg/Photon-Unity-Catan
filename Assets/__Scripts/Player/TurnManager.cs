using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Linq;

public class TurnManager : MonoBehaviourPun
{
    #region Private Fields
    private PlayerSetup playerSetup;

    private BuildManager buildManager;

    private CardManager cardManager;

    [SerializeField]
    private Button endTurnButton = null;

    public bool[] finishedThrowing;

    #endregion


    #region Public Fields

    public GameObject ButtonsPanel;


    #endregion


    #region Properties

    public Dice Dice { get; set; }
    public Barbarians barbarians {get; set;}


    #endregion


    #region Unity Methods


    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
            finishedThrowing = new bool[GameManager.instance.players.Length];

        if (photonView.IsMine)
        {
            playerSetup = GetComponent<PlayerSetup>();
            buildManager = GetComponent<BuildManager>();
            cardManager = GetComponent<CardManager>();
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


    #endregion


    #region Raise Events Handlers

    void OnEvent(EventData photonEvent)
    {
        object[] data;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.SendMapData:
                data = (object[])photonEvent.CustomData;
                Init((int)data[0], (int)data[5]);
                break;
            case (byte)RaiseEventsCode.StartTurn:
                if (!photonView.IsMine) return;
                ActivateAlchemists(true);
                Dice.InitScaleUpDown(Consts.DiceRegularScale, Consts.ScaleDice);
                buildManager.SetUsableKnights();
                break;
            case (byte)RaiseEventsCode.ActivateBarbarians:
                if (!photonView.IsMine) return;
                Dice.ActivateEventDice();
                barbarians.gameObject.SetActive(true);
                break;
            case (byte)RaiseEventsCode.GainTurnControl:
                if (!photonView.IsMine) return;
                GainControl();
                break;
            case (byte)RaiseEventsCode.SevenRolled:
                if (!PhotonNetwork.IsMasterClient) return;
                for (int i = 0; i < finishedThrowing.Length; i++)
                    finishedThrowing[i] = false;
                break;
            case (byte)RaiseEventsCode.FinishedThrowing:
                if (!photonView.IsMine) return;
                finishedThrowing[photonEvent.Sender - 1] = true;
                foreach (bool finish in finishedThrowing)
                    if (!finish) return;
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishRollSeven, GameManager.instance.CurrentPlayer);
                break;
        }
    }


    void Init(int diceID, int barbariansID)
    {
        Dice = PhotonView.Find(diceID).gameObject.GetComponent<Dice>();
        barbarians = PhotonView.Find(barbariansID).gameObject.GetComponent<Barbarians>();
    }


    public void GainControl()
    {
        if (!photonView.IsMine) return;
        SetControl(true);
        cardManager.SetDevelopmentCardsPanelActive(true);
    }

    public void PassTurn()
    {
        if (!photonView.IsMine) return;
        endTurnButton.gameObject.SetActive(false);
        SetKnightsCollider(false);

        cardManager.SetMainPanelActive(false);
        cardManager.CloseTrade();
        cardManager.ClearOffers();
        cardManager.RemoveMerchantFleets();
        ButtonsPanel.gameObject.SetActive(false);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        cardManager.SetDevelopmentCardsPanelActive(false);
        Utils.RaiseEventForAll(RaiseEventsCode.PassTurn);

    }


    public void SetControl(bool flag)
    {
        endTurnButton.gameObject.SetActive(flag);
        ButtonsPanel.gameObject.SetActive(flag);
        SetKnightsCollider(flag);
        cardManager.SetMainPanelActive(flag);
        cardManager.SetDevelopmentCardsPanelActive(flag);

    }


    public void SetKnightsCollider (bool flag)
    {
        foreach (Vertex vertex in buildManager.PlayerKnights.Values)
        {
            Knight knight = vertex.knight;
            if (knight.Useable)
                knight.SetCollider(flag);
        }
    }

    public void SetButtons(bool flag)
    {
        foreach (Button button in buildManager.buttons)
        {
            button.gameObject.SetActive(flag);
        }
    }

    #endregion


    public void ActivateAlchemists(bool flag)
    {
        foreach(CanvasGroup alchemist in cardManager.alchemists.Values)
        {
            alchemist.interactable = flag;
        }
    }
}
