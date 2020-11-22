using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class MasterMerchant : DevelopmentCard
{
    List<int> masterMerchanPlayers;

    public Dictionary<int, bool> checkResponses;
    public Dictionary<int, bool> responses;

    public GameObject playerIconPrefab;

    private List<PlayerIcon> playerIcons = new List<PlayerIcon>();

    public List<GameObject> cards;

    public int Rival { get; set; }

    public List<GameObject> RivalCards { get; set; } = new List<GameObject>();

    bool activated = false;


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
            case (byte)RaiseEventsCode.CardsCount:
                if (!photonView.IsMine || !activated) return;
                data = (object[])photonEvent.CustomData;
                checkResponses[photonEvent.Sender] = true;
                responses[photonEvent.Sender] = (int)data[0] != 0 ? true : false;

                if (AllResponses())
                    Activate();
                break;
            case (byte)RaiseEventsCode.FinishMasterMerchant:
                if (!photonView.IsMine || !activated) return;
                data = (object[])photonEvent.CustomData;
                int[] cardTypes = (int[])data[0];
                if (cardTypes.Length == 1)
                    cardManager.cardsToTake = new List<Card>(1);
                else
                    cardManager.cardsToTake = new List<Card>(2);
                foreach (int type in cardTypes)
                    RivalCards.Add(Instantiate(cards[type], playerSetup.viewPanel.transform));
                cardManager.SetNumOfCardsInPanel();
                playerSetup.viewPanel.SetActive(true);
                playerSetup.takePanel.SetActive(true);
                cardManager.state = eCardsState.Take;
                break;
        }
    }

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        activated = true;
        masterMerchanPlayers = new List<int>();
        checkResponses = new Dictionary<int, bool>();
        responses = new Dictionary<int, bool>();
        foreach (KeyValuePair<int, int> entry in GameManager.instance.playerPoints)
        {
            if (entry.Key != GameManager.instance.CurrentPlayer && entry.Value > playerSetup.playerPanel.points)
            {
                masterMerchanPlayers.Add(entry.Key);
                checkResponses.Add(entry.Key, false);
                responses.Add(entry.Key, false);
            }
        }

        if (masterMerchanPlayers.Count == 0)
        {
            MiniCleanUp();
            return;
        }

        Utils.RaiseEventForGroup(RaiseEventsCode.CountCards, masterMerchanPlayers.ToArray());

    }

    protected override void Activate()
    {
        bool start = false;

        foreach (KeyValuePair<int,bool> entry in responses)
        {
            if (!entry.Value) continue;

            start = true;
            GameObject playerIconGO = Instantiate(playerIconPrefab, playerSetup.masterMerchantPanel.transform);
            playerIcons.Add(playerIconGO.GetComponent<PlayerIcon>());
            Utils.PaintPlayerIcon(playerIconGO, entry.Key);
        }

        if (start)
        {
            DisplayCard(true);
            turnManager.SetControl(false);
            cardManager.SetDevelopmentCardsPanelActive(false);

            playerSetup.masterMerchantPanel.SetActive(true);
        }
        else
        {
            MiniCleanUp();
            activated = false;
        }
    }


    public bool AllResponses()
    {
        foreach (bool res in checkResponses.Values)
        {
            if (!res) return false;
        }
        return true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);

        for (int i = 0; i < playerIcons.Count; i++)
        {
            Destroy(playerIcons[i].gameObject);
        }

        playerIcons.Clear();

        for(int i = 0; i< RivalCards.Count; i++)
        {
            Destroy(RivalCards[i].gameObject);
        }

        RivalCards.Clear();

        playerSetup.takePanel.SetActive(false);
        playerSetup.viewPanel.SetActive(false);


        checkResponses = new Dictionary<int, bool>();
        responses = new Dictionary<int, bool>();
    }
}
