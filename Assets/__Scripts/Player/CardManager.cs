using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;

public class CardManager : MonoBehaviourPunCallbacks
{
    PlayerSetup playerSetup;
    BuildManager buildManager;
    TurnManager turnManager;

    public eCardsState state;
    public Robber robber;

    #region Panels
    public GameObject buttonsPanel;
    public List<Text> improveButtons;


    public GameObject resourceCardsPanel;
    public GameObject commodityCardsPanel;


    public GameObject throwCardsPanel;
    public GameObject throwCardsContent;

    public GameObject selectPlayerPanel;

    public GameObject tradePanel;
    public List<Text> portTexts;

    public GameObject tradeGivePanel;
    public GameObject tradeGetPanel;

    public Button bankButton;
    public Button makeOfferButton;

    public List<Text> portsTexts;

    public GameObject TradeOffersPanel;

    [SerializeField]
    private GameObject pickCardPanel;

    #endregion

    #region Prefabs
    public List<GameObject> cardsPrefabs;
    public GameObject playerIconPrefab;
    public GameObject OfferPanelPrefab;

    #endregion

    #region City Improvement Fields

    private Dictionary<eCommodity, int> commodityPrices = new Dictionary<eCommodity, int>()
    {
        { eCommodity.Paper, 1 },
        { eCommodity.Coin, 1 },
        { eCommodity.Silk, 1 },
    };

    #endregion

    #region Hand

    public List<int> cachedRollCards = new List<int>(); 

    public int allowedCards = 7;

    public List<ResourceCard> resourcesHand = new List<ResourceCard>();

    private Dictionary<eResources, int> resourceCount = new Dictionary<eResources, int>()
    {
        { eResources.Brick, 0},
        { eResources.Wood, 0 },
        { eResources.Ore, 0},
        { eResources.Wool, 0 },
        { eResources.Wheat, 0 }
    };

    private Dictionary<eCommodity, int> commodityCount = new Dictionary<eCommodity, int>()
    {
        { eCommodity.Paper, 0},
        { eCommodity.Coin, 0},
        { eCommodity.Silk, 0}
    };
    public List<CommodityCard> commodityHand = new List<CommodityCard>();

    #endregion

    #region Port Fields

    private Dictionary<eResources, ePorts> ports = new Dictionary<eResources, ePorts>()
    {
        { eResources.Brick, ePorts.p4To1},
        { eResources.Wood, ePorts.p4To1 },
        { eResources.Ore, ePorts.p4To1},
        { eResources.Wool, ePorts.p4To1 },
        { eResources.Wheat, ePorts.p4To1 }
    };

    private ePorts commodityPort = ePorts.p4To1;
    
    #endregion

    #region Throw Cards Fields

    public int numOfCardToThorw;
    public List<Card> cardsToThorw;

    #endregion

    #region Trade Cards Fields

    public List<Card> cardsToGive = new List<Card>();
    public List<Card> cardsToGet = new List<Card>();

    
    public Dictionary<eResources, int> currentTradeResources = new Dictionary<eResources, int>()
    {
        { eResources.Brick, 0},
        { eResources.Wood, 0 },
        { eResources.Ore, 0},
        { eResources.Wool, 0 },
        { eResources.Wheat, 0 }
    };
    
    
    private Dictionary<eCommodity, int> currentTradeCommodities = new Dictionary<eCommodity, int>() 
    {
        { eCommodity.Paper, 0},
        { eCommodity.Coin, 0},
        { eCommodity.Silk, 0}
    };


    public List<GameObject> offers = new List<GameObject>();

    #endregion


    void Awake()
    {
        if (photonView.IsMine)
        {
            playerSetup = GetComponent<PlayerSetup>();
            buildManager = GetComponent<BuildManager>();
            turnManager = GetComponent<TurnManager>();
            state = eCardsState.None;
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
                data = (object[])photonEvent.CustomData;
                InitRobber((int)data[4]);
                break;
            case (byte)RaiseEventsCode.SevenRolled:
                if (!photonView.IsMine) return;
                int numOfCards = resourcesHand.Count + commodityHand.Count;
                if (numOfCards > allowedCards)
                {
                    state = eCardsState.Throw;
                    setCardsColliders(true);
                    numOfCardToThorw = numOfCards / 2;
                    cardsToThorw = new List<Card>(numOfCardToThorw);
                    playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.Bad);
                    throwCardsPanel.SetActive(true);
                }
                else
                {
                    Utils.RaiseEventForAll(RaiseEventsCode.FinishedThrowing);
                }
                break;
            case (byte)RaiseEventsCode.FinishRollSeven:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
                if (GameManager.instance.state == GameState.Playing)
                {
                    StartRob();
                }
                else
                {
                    turnManager.GainControl();
                }
                break;
            case (byte)RaiseEventsCode.LoseCard:
                if (!photonView.IsMine) return;
                int cardLost = LoseRandomCard();
                Utils.RaiseEventForPlayer(RaiseEventsCode.TakeCard, photonEvent.Sender, new object[] { cardLost });
                break;
            case (byte)RaiseEventsCode.TakeCard:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                int cardName = (int)data[0];
                if (cardName != -1)
                {
                    InitCard(cardName);
                }
                EndRobberPlacement();
                break;
            case (byte)RaiseEventsCode.CompleteTrade:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                CompleteTrade((int[])data[0], (int[])data[1]);
                break;
            case (byte)RaiseEventsCode.AddCachedCards:
                if (!photonView.IsMine) return;
                InitCards(cachedRollCards);
                break;
            case (byte)RaiseEventsCode.PickCard:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.Good);
                pickCardPanel.SetActive(true);
                break;
        }
    }

    private void InitRobber(int viewID)
    {
        robber = PhotonView.Find(viewID).gameObject.GetComponent<Robber>();
    }

    public void InitCards(List<int> resources)
    {
        foreach (int resource in resources)
        {
            InitCard(resource);
        }

    }

    public void InitCard(int type)
    {
        if (type <= 4)
            AddResourceCardToHand(Instantiate(cardsPrefabs[type], resourceCardsPanel.transform).GetComponent<ResourceCard>(), false);
        else
            AddCommodityCardToHand(Instantiate(cardsPrefabs[type], commodityCardsPanel.transform).GetComponent<CommodityCard>(), false);        
    }


    public void setCardsColliders(bool flag)
    {
        foreach(Card card in resourcesHand)
            card.SetClicks(flag);
        foreach(Card card in commodityHand)
            card.SetClicks(flag);
    }


    #region Build Related

    public void AllowBuild(int buildOption)
    {
        eBuildAction buildAction = (eBuildAction)buildOption + 1;
        eBuilding building = (eBuilding)buildOption + 1;
        if (buildManager.buildingAmounts[building] == 0) return;
        if (CheckPriceInHand(Consts.Prices[buildAction]))
        {
            buildManager.ButtonHandler(buildOption);
        }
    }

    public void ImproveCity(int commodityNum)
    {
        eCommodity commodity = (eCommodity)commodityNum;
        if (buildManager.cityCount == 0 || commodityCount[commodity] < commodityPrices[commodity] || commodityPrices[commodity] == 6) return;
        
        int unimprovedCitiesCount = buildManager.CountUnimprovedCities();
        bool canImproveCity = commodityPrices[commodity] >= 4 && unimprovedCitiesCount != 0;
        
        if (canImproveCity|| commodityPrices[commodity] < 4)
        {
            for(int i=0; i< commodityPrices[commodity]; i++)
                RemoveCardFromHandByType(commodityNum);
            

            improveButtons[commodityNum - 5].text = string.Format("Level: {0}", commodityPrices[commodity].ToString());
            
            commodityPrices[commodity] += 1;
            switch (commodityPrices[commodity])
            {
                case 4:
                    OpenPerk(commodity);
                    break;
                case 5:
                    Utils.RaiseEventForMaster(RaiseEventsCode.CheckImporveCity, new object[] { commodityNum, 4 });
                    break;
                case 6:
                    Utils.RaiseEventForMaster(RaiseEventsCode.CheckImporveCity, new object[] { commodityNum, 5 });
                    break;
            }
        }

    }


    void OpenPerk(eCommodity commodity)
    {
        switch (commodity)
        {
            case eCommodity.Paper:
                Utils.RaiseEventForAll(RaiseEventsCode.AddGreenPlayer);
                break;
            case eCommodity.Coin:
                buildManager.CanBuildKnightsLvl3 = true;
                break;
            case eCommodity.Silk:
                commodityPort = ePorts.p2To1;
                UpdateCommodityPortsText(Consts.p2to1);
                break;
        }
    }

    public bool CheckPriceInHand(Dictionary<eResources, int> price)
    {
        foreach (KeyValuePair<eResources, int> entry in price)
        {
            if (resourceCount[entry.Key]-currentTradeResources[entry.Key] < entry.Value)
                return false;
        }
        return true;
    }

    public void Pay()
    {
        foreach (KeyValuePair<eResources, int> entry in Consts.Prices[buildManager.Build])
        {
            resourceCount[entry.Key] -= entry.Value;
            for (int i = 0; i < entry.Value; i++)
            {
                ResourceCard card = resourcesHand.First(x => x.resource == entry.Key);
                resourcesHand.Remove(card);
                Destroy(card.gameObject);
            }
        }
    }

    #endregion


    #region Port Text Related

    public void UpdateCommodityPortsText(string text)
    {

        for(int i = 5; i<8; i++)
        {
            UpdatePortText(i, text);
        }
    }

    public void UpdatePortText(int port, string portText)
    {
        if (port == 1)
            portTexts[3].text = portText;
        else if (port == 3)
            portTexts[1].text = portText;
        else
            portTexts[port].text = portText;
    }

    public void UpatePort(int port)
    {
        if(port == 5)
        {
            List<eResources> keys = new List<eResources>(ports.Keys);
            foreach(eResources key in keys)
            {
                if(ports[key] == ePorts.p4To1)
                {
                    ports[key] = ePorts.p3To1;
                    UpdatePortText((int) key, Consts.p3to1);
                }
            }
            if (commodityPort == ePorts.p4To1)
            {
                commodityPort = ePorts.p3To1;
                UpdateCommodityPortsText(Consts.p3to1);

            }
        }
        else
        {
            ports[(eResources)port] = ePorts.p2To1;
            UpdatePortText(port, Consts.p2to1);
        }

    }

    #endregion


    #region Throw Related
    public void Thorw()
    {
        if (!Utils.IsFull(cardsToThorw)) return;
        for (int i = 0; i < cardsToThorw.Count; i++)
        {
            Card card = cardsToThorw[i];
            ResourceCard resourceCard = card as ResourceCard;

            if (resourceCard != null)
            {
                resourceCount[resourceCard.resource] -= 1;
                resourcesHand.Remove(resourceCard);
            }
            else
            {
                CommodityCard commodityCard = card as CommodityCard;
                commodityCount[commodityCard.commodity] -= 1;
                commodityHand.Remove(card as CommodityCard);
            }
            Destroy(card.gameObject);
        }
        cardsToThorw.Clear();
        state = eCardsState.None;
        throwCardsPanel.SetActive(false);
        setCardsColliders(false);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        Utils.RaiseEventForAll(RaiseEventsCode.FinishedThrowing);

    }

    #endregion

    #region Maintain Hands

    public void AddCardsFromRoll(Tile tile, int vertexID)
    {
        int resource = (int)tile.Resource;
        InitCard(resource);
        if (buildManager.PlayerBuildings[vertexID].Building == eBuilding.City)
        {
            if (tile.Commodity != eCommodity.None)
                InitCard((int)tile.Commodity);
            else
                InitCard(resource);
        }
    }

    public void AddCardsToCache(Tile tile, int vertexID)
    {
        int resource = (int)tile.Resource;
        cachedRollCards.Add(resource);
        if (buildManager.PlayerBuildings[vertexID].Building == eBuilding.City)
        {
            if (tile.Commodity != eCommodity.None)
                cachedRollCards.Add((int)tile.Commodity);
            else
                cachedRollCards.Add(resource);
        }
    }

    public void AddCardToHand(Card card)
    {
        if (Utils.IsResourceCard(card))
        {
            ResourceCard resourceCard = card as ResourceCard;
            AddResourceCardToHand(resourceCard, true);
        }
        else
        {
            CommodityCard commodityCard = card as CommodityCard;
            AddCommodityCardToHand(commodityCard, true);
        }
    }

    public void AddResourceCardToHand(ResourceCard resourceCard, bool setParent)
    {
        resourcesHand.Add(resourceCard);
        resourceCount[resourceCard.resource] += 1;
        if (setParent)
            resourceCard.transform.SetParent(resourceCardsPanel.transform);
    }

    public void AddCommodityCardToHand(CommodityCard commodityCard, bool setParent)
    {
        commodityHand.Add(commodityCard);
        commodityCount[commodityCard.commodity] += 1;
        if (setParent)
            commodityCard.transform.SetParent(commodityCardsPanel.transform);
    }

    public void RemoveResourceCardFromHand(ResourceCard resourceCard)
    {
        resourcesHand.Remove(resourceCard);
        resourceCount[resourceCard.resource] -= 1;
        Destroy(resourceCard.gameObject);
    }

    public void RemoveCommodityCardFromHand(CommodityCard commodityCard)
    {
        commodityHand.Remove(commodityCard);
        commodityCount[commodityCard.commodity] -= 1;
        Destroy(commodityCard.gameObject);
    }


    public void RemoveCardFromHandByType(int cardType)
    {
        if (cardType <= 4)
        {
            ResourceCard cardToRemove = null;
            foreach (ResourceCard card in resourcesHand)
            {
                if ((int)card.resource == cardType)
                {
                    cardToRemove = card;
                    break;
                }
            }
            RemoveResourceCardFromHand(cardToRemove);
        }
        else
        {
            CommodityCard cardToRemove = null;
            foreach (CommodityCard card in commodityHand)
            {
                if ((int)card.commodity == cardType)
                {
                    cardToRemove = card;
                    break;
                }
            }
            RemoveCommodityCardFromHand(cardToRemove);
        }
    }


    public void AddCardToHandPanel(Card card)
    {
        if (Utils.IsResourceCard(card))
            card.transform.SetParent(resourceCardsPanel.transform);
        else
            card.transform.SetParent(commodityCardsPanel.transform);
    }

    public void PickCard(int resource)
    {
        pickCardPanel.SetActive(false);
        InitCard(resource);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        Utils.RaiseEventForMaster(RaiseEventsCode.FinishPickCard);
    }

    #endregion

    #region Robber Functions
    public void StartRob()
    {
        state = eCardsState.Robber;
        robber.gameObject.SetActive(false);
        if (robber.photonView.Owner != PhotonNetwork.LocalPlayer)
            robber.photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        robber.Tile.photonView.RPC("SetRobberOn", RpcTarget.AllBufferedViaServer, false);
        foreach (Tile tile in buildManager.tiles)
        {
            if (tile != robber.Tile)
            {
                tile.tileSpot.SetActive(true);
                tile.sColl.enabled = true;
            }
        }
    }


    public void SelectRob(List<int> vertexes)
    {
        HashSet<int> possiblePlayersToRob = new HashSet<int>();
        foreach (int vertex in vertexes)
        {
            if (buildManager.RivalsBuildingVertexes.ContainsKey(vertex))
            {
                possiblePlayersToRob.Add(buildManager.RivalsBuildingVertexes[vertex].owner);
            }
        }
        if (possiblePlayersToRob.Count != 0)
        {
            selectPlayerPanel.SetActive(true);
            foreach (int player in possiblePlayersToRob)
            {
                GameObject playerIconGO = Instantiate(playerIconPrefab, selectPlayerPanel.transform);
                foreach (Player playerObject in GameManager.instance.players)
                {
                    if (playerObject.ActorNumber == player)
                    {
                        object color;
                        playerObject.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out color); ;
                        string playerColor = (string)color;
                        PlayerIcon playerIcon = playerIconGO.GetComponent<PlayerIcon>();
                        playerIcon.SetColor(Utils.Name_To_Color(playerColor));
                        playerIcon.Owner = player;
                        break;
                    }
                }
            }
        }
        else
        {
            EndRobberPlacement();
        }
    }

    public void FinishSelect()
    {
        selectPlayerPanel.SetActive(false);
        foreach (PlayerIcon pi in selectPlayerPanel.GetComponentsInChildren<PlayerIcon>())
        {
            Destroy(pi.gameObject);
        }
    }


    int LoseRandomCard()
    {
        int numOfCards = resourcesHand.Count + commodityHand.Count;
        if (numOfCards == 0) return -1;
        int chosen = Random.Range(0, numOfCards);
        if (chosen < resourcesHand.Count)
        {
            ResourceCard resourceCard = resourcesHand[chosen];
            int resource = (int)resourceCard.resource;
            RemoveResourceCardFromHand(resourceCard);
            return resource;
        }
        else
        {
            chosen -= resourcesHand.Count;
            CommodityCard commodityCard = commodityHand[chosen];
            int commodity = (int)commodityCard.commodity;
            RemoveCommodityCardFromHand(commodityCard);
            return commodity;
        }
    }


    public void EndRobberPlacement()
    {

        //robber.photonView.TransferOwnership(0);
        turnManager.GainControl(); // change this once robber placement is good
        state = eCardsState.None;
        buildManager.KnightAction = eKnightActions.None;
    }

    #endregion

    #region Trade Related

    public void StartTrade(Card card)
    {
        buttonsPanel.SetActive(false);
        tradePanel.SetActive(true);
        cardsToGive.Add(card);
        AddCardToTrade(card);
        card.transform.SetParent(tradeGivePanel.transform);
    }

    public void CloseTrade()
    {
        state = eCardsState.None;
        tradePanel.SetActive(false);
        buttonsPanel.SetActive(true);
        bankButton.gameObject.SetActive(false);
        makeOfferButton.gameObject.SetActive(false);
        foreach (Card card in cardsToGive)
        {
            card.toTrade = false;
            AddCardToHandPanel(card);
        }
        cardsToGive.Clear();
        foreach (Card card in cardsToGet)
        {
            Destroy(card.gameObject);
        }
        cardsToGet.Clear();
        ClearCurrentTrade();
    }

    public void AddToGet(int type)
    {
        Card card = Instantiate(cardsPrefabs[type], tradeGetPanel.transform).GetComponent<Card>();
        card.IsInGetList = true;
        cardsToGet.Add(card);
        CheckBankButton();
    }

    public void AddCardToTrade(Card card)
    {
        if (Utils.IsResourceCard(card)){
            ResourceCard resourceCard = card as ResourceCard;
            currentTradeResources[resourceCard.resource] += 1;
        }
        else
        {
            CommodityCard commodityCard = card as CommodityCard;
            currentTradeCommodities[commodityCard.commodity] += 1;
        }
    }
    public void RemoveCardFromTrade(Card card)
    {
        if (Utils.IsResourceCard(card)){
            ResourceCard resourceCard = card as ResourceCard;
            currentTradeResources[resourceCard.resource] -= 1;
        }
        else
        {
            CommodityCard commodityCard = card as CommodityCard;
            currentTradeCommodities[commodityCard.commodity] -= 1;
        }
    }

    public void CheckBankButton()
    {
        int numOfEntitledCards = 0;
        foreach(KeyValuePair<eResources, int> entry in currentTradeResources)
        {
            if (entry.Value % (int)ports[entry.Key] != 0)
            {
                CheckMakeOfferButton();
                return;
            }
            numOfEntitledCards += entry.Value / (int)ports[entry.Key];
        }

        foreach(int value in currentTradeCommodities.Values)
        {
            if (value % (int)commodityPort != 0)
            {
                CheckMakeOfferButton();
                return;
            }
            numOfEntitledCards += value / (int)commodityPort;
        }
        if (numOfEntitledCards != cardsToGet.Count)
        {
            CheckMakeOfferButton();
            return;
        }
        bankButton.gameObject.SetActive(true);
        makeOfferButton.gameObject.SetActive(false);
    }


    public void CheckMakeOfferButton()
    {
        bankButton.gameObject.SetActive(false);
        if(cardsToGive.Count != 0 && cardsToGet.Count != 0)
            makeOfferButton.gameObject.SetActive(true);
        else
            makeOfferButton.gameObject.SetActive(false);
    }

    public void ClearCurrentTrade()
    {
        for (int i = 0; i<5; i++)
            currentTradeResources[(eResources)i] = 0;
        for (int i = 0; i < 3; i++)
            currentTradeCommodities[(eCommodity)i+5] = 0;
    }

    public void BankTrade()
    {
        foreach (Card card in cardsToGive)
        {
            if (Utils.IsResourceCard(card))
                RemoveResourceCardFromHand(card as ResourceCard);
            else
                RemoveCommodityCardFromHand(card as CommodityCard);
        }
        cardsToGive.Clear();

        foreach(Card card in cardsToGet)
        {
            card.IsInGetList = false;
            AddCardToHand(card);
        }
        cardsToGet.Clear();

        ClearCurrentTrade();
    }


    public void MakeOffer()
    {
        List<int> offeredCards = new List<int>();
        List<int> requestedCards = new List<int>();

        foreach (Card card in cardsToGive)
            offeredCards.Add(Utils.GetCardType(card));
        foreach (Card card in cardsToGet)
            requestedCards.Add(Utils.GetCardType(card));

        GameObject offer = PhotonNetwork.Instantiate(OfferPanelPrefab.name, OfferPanelPrefab.transform.position, Quaternion.identity, 0, new object[] { offeredCards.ToArray(), requestedCards.ToArray() });

        offers.Add(offer);

        PhotonView offerView = offer.GetComponent<PhotonView>();

        offerView.RPC("Init", RpcTarget.AllBufferedViaServer);

    }

    public bool CheckIfCanAcceptOffer(int[] cardTypes)
    {
        Dictionary<int, int> cardTypesCount = new Dictionary<int, int>();
        foreach(int cardType in cardTypes)
        {
            if (cardTypesCount.ContainsKey(cardType))
                cardTypesCount[cardType] += 1;
            else
                cardTypesCount.Add(cardType, 1);
        }

        foreach(int key in cardTypesCount.Keys)
        {
            if (key <= 4)
            {
                if (cardTypesCount[key] > resourceCount[(eResources)key])
                    return false;
            }
            else
            {
                if (cardTypesCount[key] > commodityCount[(eCommodity)key])
                    return false;
            }
        }

        return true;

    }

    public void CompleteTrade(int[] cardsToRemove, int[] cardsToAdd)
    {
        foreach (int cardType in cardsToRemove)
        {
            RemoveCardFromHandByType(cardType);
        }

        InitCards(new List<int>(cardsToAdd));
    }


    public void ClearOffers()
    {
        foreach(GameObject offer in offers)
        {
            PhotonNetwork.Destroy(offer);
        }

        offers.Clear();
    }




    #endregion
}
