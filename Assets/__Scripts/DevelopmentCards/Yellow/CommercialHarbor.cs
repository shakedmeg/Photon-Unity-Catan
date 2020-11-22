using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class CommercialHarbor : DevelopmentCard
{

    public GameObject playerIconPrefab;

    public Dictionary<int, bool> checkCommodityResponses;
    public Dictionary<int, bool> commodityResponses;

    bool activated = false;


    public int PlayerReady { get; set; }
    public int RivalReady { get; set; }

    public int Rival { get; set; }

    private List<PlayerIcon> playerIcons = new List<PlayerIcon>();


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
            case (byte)RaiseEventsCode.CommoditiesCount:
                if (!photonView.IsMine || !activated) return;
                data = (object[])photonEvent.CustomData;
                checkCommodityResponses[photonEvent.Sender] = true;
                commodityResponses[photonEvent.Sender] = (int)data[0] != 0 ? true : false;

                if (AllCommodityResponses())
                {
                    turnManager.SetControl(false);
                    Invoke("Activate", 1f);
                }
                break;
            case (byte)RaiseEventsCode.FinishCommercialHarbor:
                if (!photonView.IsMine || !activated) return;
                data = (object[])photonEvent.CustomData;
                RivalReady = (int)data[0];
                if (ExchangeReady())
                    MakeExchange();
                break;
        }
    }

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        if (cardManager.resourcesHand.Count == 0)
        {
            MiniCleanUp();
            return;
        }
        DisplayCard(true);

        activated = true;
        checkCommodityResponses = new Dictionary<int, bool>();
        commodityResponses = new Dictionary<int, bool>();
        foreach (Player player in GameManager.instance.players)
        {
            if (player.ActorNumber == GameManager.instance.CurrentPlayer) continue;
            checkCommodityResponses.Add(player.ActorNumber, false);
            commodityResponses.Add(player.ActorNumber, false);
        }

        Utils.RaiseEventForGroup(RaiseEventsCode.CountCommodities, checkCommodityResponses.Keys.ToArray());

    }

    protected override void Activate()
    {

        bool start = false;
        foreach (KeyValuePair<int, bool> entry in commodityResponses)
        {
            if (!entry.Value) continue;

            start = true;
            GameObject playerIconGO = Instantiate(playerIconPrefab, playerSetup.commercialHarborPanel.transform);
            playerIconGO.transform.SetAsFirstSibling();
            playerIcons.Add(playerIconGO.GetComponent<PlayerIcon>());
            Utils.PaintPlayerIcon(playerIconGO, entry.Key);

        }
        if (!start)
        {
            CleanUp();
            return;
        }

        playerSetup.declineCommercial.SetActive(false);
        playerSetup.commercialHarborPanel.SetActive(true);

    }


    public void BeginExchange(int rival)
    {
        PlayerReady = -1;
        RivalReady = -1;
        Rival = rival;
        Utils.RaiseEventForPlayer(RaiseEventsCode.CommercialHarbor, rival);
        cardManager.HandleCommercialHarbor();
    }

    public bool ExchangeReady()
    {
        return PlayerReady != -1 && RivalReady != -1;
    }

    public void MakeExchange()
    {
        Utils.RaiseEventForPlayer(RaiseEventsCode.CompleteCommercialHarborExchange, Rival, new object[] { PlayerReady });
        cardManager.RemoveResourceCardFromHand((ResourceCard)cardManager.exchangeCard);
        cardManager.InitCard(RivalReady);
        cardManager.exchangeCard = null;
        RemovePlayerIcon(Rival);
        if (playerIcons.Count == 0 || cardManager.resourcesHand.Count == 0)
        {
            CleanUp();
        }
        else
        {
            playerSetup.commercialHarborPanel.SetActive(true);
        }
    }


    private void RemovePlayerIcon(int owner)
    {
        PlayerIcon playerIcon = null;
        foreach(PlayerIcon pi in playerIcons)
        {
            if(pi.Owner == owner)
            {
                playerIcon = pi;
                break;
            }
        }

        playerIcons.Remove(playerIcon);
        Destroy(playerIcon.gameObject);

    }

    public bool AllCommodityResponses()
    {
        foreach (bool res in checkCommodityResponses.Values)
        {
            if (!res) return false;
        }
        return true;
    }


    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);

        playerSetup.declineCommercial.SetActive(false);
        playerSetup.commercialHarborPanel.SetActive(false);

        for(int i = 0; i<playerIcons.Count; i++)
        {
            Destroy(playerIcons[i].gameObject);
        }

        playerIcons.Clear();


        playerIcons = new List<PlayerIcon>();
        checkCommodityResponses = new Dictionary<int, bool>();
        commodityResponses = new Dictionary<int, bool>();
    }
}
