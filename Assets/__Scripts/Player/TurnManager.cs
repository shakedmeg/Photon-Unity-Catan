using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class TurnManager : MonoBehaviourPun
{
    #region Private Fields

    private BuildManager buildManager;

    private CardManager cardManager;

    [SerializeField]
    private Button endTurnButton;
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

        if (photonView.IsMine)
        {
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
                Dice.SetCollider(true);
                break;
            case (byte)RaiseEventsCode.ActivateBarbarians:
                if (!photonView.IsMine) return;
                Dice.ActivateEventDice();
                barbarians.gameObject.SetActive(true);
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
        ButtonsPanel.gameObject.SetActive(true);
        endTurnButton.gameObject.SetActive(true);
        SetKnightsCollider(true);
        cardManager.setCardsColliders(true);
    }

    public void PassTurn()
    {
        if (!photonView.IsMine) return;
        Dice.SetCollider(false);
        endTurnButton.gameObject.SetActive(false);
        SetKnightsCollider(false);

        cardManager.setCardsColliders(false);
        cardManager.CloseTrade();
        cardManager.ClearOffers();
        ButtonsPanel.gameObject.SetActive(false);
        Utils.RaiseEventForAll(RaiseEventsCode.PassTurn);

    }


    public void SetButtonsAndKnightsControl(bool flag)
    {
        SetButtons(flag);
        SetKnightsCollider(flag);
    }


    public void SetKnightsCollider (bool flag)
    {
        foreach (Vertex vertex in buildManager.PlayerKnights.Values)
        {
            Knight knight = vertex.knight;
            if (knight.Activated)
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

}
