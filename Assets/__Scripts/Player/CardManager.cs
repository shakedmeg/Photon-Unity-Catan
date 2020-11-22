using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;
using UnityEngine.UI;

public class CardManager : MonoBehaviourPunCallbacks
{
    PlayerSetup playerSetup;
    BuildManager buildManager;
    TurnManager turnManager;

    public eCardsState state;
    public Robber robber;
    public MerchantPiece merchant;

    #region Panels
    public GameObject buttonsPanel;
    public List<Text> improveButtons;




    [SerializeField]
    private CanvasGroup mainPanel = null;

    [SerializeField]
    private GameObject resourceCardsPanel = null;

    [SerializeField]
    private GameObject commodityCardsPanel = null;

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
    private GameObject pickCardPanel = null;

    #endregion

    #region Prefabs
    public List<GameObject> cardsPrefabs;
    public GameObject playerIconPrefab;
    public GameObject OfferPanelPrefab;

    #endregion

    #region City Improvement Fields

    public Dictionary<eCommodity, int> commodityPrices = new Dictionary<eCommodity, int>()
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

    public Dictionary<eCommodity, int> commodityCount = new Dictionary<eCommodity, int>()
    {
        { eCommodity.Paper, 0},
        { eCommodity.Coin, 0},
        { eCommodity.Silk, 0}
    };
    public List<CommodityCard> commodityHand = new List<CommodityCard>();

    #endregion

    #region Port Fields

    public Dictionary<eResources, ePorts> ports = new Dictionary<eResources, ePorts>()
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

    #region Development Cards Fields

    public List<GameObject> developmentCardsPrefabs;

    public GameObject constitution;

    public GameObject printer;

    public List<GameObject> backgrounds;

    public CanvasGroup developmentCardsPanel;

    public GameObject developmentCardsContentPanel;

    public GameObject chooseDevelopmentCardPanel;

    private bool winDevFromAtt = false;
    public bool ThrowDevCards { get; set; } = false;

    public List<DevelopmentCard> developmentHand = new List<DevelopmentCard>();

    public GameObject givePanel;

    public GameObject giveCardsContent;

    public GameObject exchangePanel;

    public GameObject exchangeCardsContent;

    public List<Card> forcedCardsToGive;

    public Card exchangeCard;

    public List<Card> cardsToTake;
    public GameObject takeCardsContent;

    public int merchantPort = -1;

    public List<eResources> merchantFleetPorts = new List<eResources>();

    public Dictionary<Alchemist, CanvasGroup> alchemists = new Dictionary<Alchemist, CanvasGroup>();

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
        int numOfCards;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.SendMapData:
                data = (object[])photonEvent.CustomData;
                InitRobber((int)data[4]);
                InitMerchant((int)data[6]);
                break;
            case (byte)RaiseEventsCode.SevenRolled:
                if (!photonView.IsMine) return;
                numOfCards = resourcesHand.Count + commodityHand.Count;
                if (numOfCards > allowedCards)
                {
                    state = eCardsState.Throw;
                    SetMainPanelActive(true);
                    numOfCardToThorw = numOfCards / 2;
                    cardsToThorw = new List<Card>(numOfCardToThorw);
                    playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.Bad);
                    throwCardsPanel.SetActive(true);
                }
                else
                {
                    Utils.RaiseEventForMaster(RaiseEventsCode.FinishedThrowing);
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
                SetNumOfCardsInPanel();
                if (playerSetup.currentCard == null)
                    EndRobberPlacement();
                else 
                {
                    Bishop bishop = playerSetup.currentCard as Bishop;
                    bishop.SetPlayerRes(photonEvent.Sender);
                    bishop.CleanUp();
                }
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
            case (byte)RaiseEventsCode.Sabotage:
                if (!photonView.IsMine) return;
                HandleSaboteur();
                break;
            case (byte)RaiseEventsCode.CountDevCards:
                if (!photonView.IsMine) return;
                Utils.RaiseEventForPlayer(RaiseEventsCode.DevCardsCount, GameManager.instance.CurrentPlayer, new object[] { developmentHand.Select(x => new int[] { (int)x.type, x.Background.GetPhotonView().ViewID }).ToArray() });
                break;
            case (byte)RaiseEventsCode.Spy:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
                for (int i = 0; i<developmentHand.Count; i++)
                {
                    if((int)developmentHand[i].type == (int)data[0])
                    {
                        Destroy(developmentHand[i].gameObject);
                        developmentHand.RemoveAt(i);
                        break;
                    }
                }
                break;
            case (byte)RaiseEventsCode.Wedding:
                if (!photonView.IsMine) return;
                HandleWedding();
                break;
            case (byte)RaiseEventsCode.CountCommodities:
                if (!photonView.IsMine) return;
                Utils.RaiseEventForPlayer(RaiseEventsCode.CommoditiesCount, photonEvent.Sender, new object[] { commodityHand.Count });
                break;
            case (byte)RaiseEventsCode.CommercialHarbor:
                if (!photonView.IsMine) return;
                HandleCommercialHarbor();
                break;
            case (byte)RaiseEventsCode.CompleteCommercialHarborExchange:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                InitCard((int)data[0]);
                RemoveCommodityCardFromHand((CommodityCard)exchangeCard);
                exchangeCard = null;
                break;
            case (byte)RaiseEventsCode.CountCards:
                if (!photonView.IsMine) return;
                Utils.RaiseEventForPlayer(RaiseEventsCode.CardsCount, photonEvent.Sender, new object[] { resourcesHand.Count + commodityHand.Count });
                break;
            case (byte)RaiseEventsCode.MasterMerchant:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.MasterMerchantVictim);
                List<int> cards = new List<int>();
                foreach (ResourceCard resourceCard in resourcesHand)
                    cards.Add((int)resourceCard.resource);
                foreach (CommodityCard commodityCard in commodityHand )
                    cards.Add((int)commodityCard.commodity);
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishMasterMerchant, photonEvent.Sender, new object[] { cards.ToArray() });
                break;
            case (byte)RaiseEventsCode.CompleteMasterMerchant:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                foreach (int i in (int[])data[0])
                    RemoveCardFromHandByType(i);
                SetNumOfCardsInPanel();
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
                break;
            case (byte)RaiseEventsCode.LoseMerchant:
                if (!photonView.IsMine) return;
                RemoveMerchant();
                playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, -1);
                break;
            case (byte)RaiseEventsCode.ResourceMonopoly:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                HandleResourceMonopoly((int)data[0]);
                break;
            case (byte)RaiseEventsCode.TradeMonopoly:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                HandleTradeMonopoly((int)data[0]);
                break;
            case (byte)RaiseEventsCode.DeserveDevelopmentCard:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                CheckDevelopmentCardEntitledment((int)data[0], (int)data[1]);
                break;
            case (byte)RaiseEventsCode.SendDevelopmentCardFromRoll:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                AddDevelopmentCard((int)data[0], RaiseEventsCode.FinishDevelopmentCardRollHandout);                
                break;
            case (byte)RaiseEventsCode.ChooseDevelopmentCard:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.ChooseDevCard);
                chooseDevelopmentCardPanel.SetActive(true);
                break;
            case (byte)RaiseEventsCode.SendDevelopmentCardFromWin:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                winDevFromAtt = true;
                AddDevelopmentCard((int)data[0], RaiseEventsCode.FinishDevelopmentCardWinHandout);                
                break;
        }
    }

    private void InitRobber(int viewID)
    {
        robber = PhotonView.Find(viewID).gameObject.GetComponent<Robber>();
    }

    private void InitMerchant(int viewID)
    {
        merchant = PhotonView.Find(viewID).gameObject.GetComponent<MerchantPiece>();
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


    #region Build Related

    public void AllowBuild(int buildOption)
    {
        eBuildAction buildAction = (eBuildAction)buildOption + 1;
        eBuilding building = (eBuilding)buildOption + 1;
        if(buildManager.buildingAmounts[building] > 0)
            CheckPrice(buildOption, buildAction);
        else
            buildManager.CleanUp();
    }

    public void AllowUpgrde()
    {
        eBuildAction buildAction = eBuildAction.UpgradeKnight;
        bool checkKnightLevel3 = buildManager.CanBuildKnightsLvl3 && buildManager.buildingAmounts[eBuilding.Knight3] > 0;
        if ((buildManager.buildingAmounts[eBuilding.Knight2] > 0) || checkKnightLevel3)
            CheckPrice(5, buildAction);
        else
            buildManager.CleanUp();
    }
    public void AllowKnightActivation()
    {
        eBuildAction buildAction = eBuildAction.ActivateKnight;
        CheckPrice(6, buildAction);
    }

    private void CheckPrice(int buildOption, eBuildAction buildAction)
    {
        if (CheckPriceInHand(Consts.Prices[buildAction]))
        {
            buildManager.ButtonHandler(buildOption);
        }
        else
        {
            buildManager.CleanUp();
        }
    }

    public void ImproveCity(int commodityNum)
    {
        eCommodity commodity = (eCommodity)commodityNum;
        if (buildManager.cityCount == 0 || commodityCount[commodity] < commodityPrices[commodity] || commodityPrices[commodity] == 6) return;

        int unimprovedCitiesCount = buildManager.CountUnimprovedCities();
        bool canImproveCity = commodityPrices[commodity] >= 4 && unimprovedCitiesCount != 0;
        if (!canImproveCity)
        {
            if(GameManager.instance.cityImprovementHolder[commodity][0] == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                canImproveCity = true;
            }
            else
            {
                if (GameManager.instance.cityImprovementHolder[commodity][0] != -1)
                    canImproveCity = GameManager.instance.cityImprovementHolder[commodity][1] >= commodityPrices[commodity];
            }
        }        
        if (canImproveCity|| commodityPrices[commodity] < 4)
        {
            for(int i=0; i< commodityPrices[commodity]; i++)
                RemoveCardFromHandByType(commodityNum);

            string level = commodityPrices[commodity].ToString();
            improveButtons[commodityNum - 5].text = string.Format("Level: {0}", level);
            playerSetup.commodityButtonsText[commodityNum - 5].text = string.Format("Level: {0}", level);
            playerSetup.playerPanel.photonView.RPC("SetCommodityText", RpcTarget.AllBufferedViaServer, commodityNum - 5, level);
            
            commodityPrices[commodity] += 1;
            switch (commodityPrices[commodity])
            {
                case 4:
                    OpenPerk(commodity);
                    break;
                case 5:
                    turnManager.SetControl(false);
                    Utils.RaiseEventForAll(RaiseEventsCode.CheckImporveCity, new object[] { commodityNum, 4 });
                    break;
                case 6:
                    turnManager.SetControl(false);
                    Utils.RaiseEventForAll(RaiseEventsCode.CheckImporveCity, new object[] { commodityNum, 5 });
                    break;
            }
        }
    }


    public void OpenPerk(eCommodity commodity)
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

    public void Pay(Dictionary<eResources, int> price)
    {
        foreach (KeyValuePair<eResources, int> entry in price)
        {
            resourceCount[entry.Key] -= entry.Value;
            for (int i = 0; i < entry.Value; i++)
            {
                ResourceCard card = resourcesHand.First(x => x.resource == entry.Key);
                resourcesHand.Remove(card);
                Destroy(card.gameObject);
            }
        }
        SetNumOfCardsInPanel();
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
        SetMainPanelActive(false);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        SetNumOfCardsInPanel();
        if(playerSetup.currentCardType == eDevelopmentCardsTypes.Saboteur)
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishSabotuer, GameManager.instance.CurrentPlayer);
        else
            Utils.RaiseEventForMaster(RaiseEventsCode.FinishedThrowing);

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
        SetNumOfCardsInPanel();
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
        SetNumOfCardsInPanel();
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
        SortResourceHand();
    }

    public void AddCommodityCardToHand(CommodityCard commodityCard, bool setParent)
    {
        commodityHand.Add(commodityCard);
        commodityCount[commodityCard.commodity] += 1;
        if (setParent)
            commodityCard.transform.SetParent(commodityCardsPanel.transform);
        SortCommodityHand();
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
        {
            card.transform.SetParent(resourceCardsPanel.transform);
            SortResourceHand();
        }
        else
        {
            card.transform.SetParent(commodityCardsPanel.transform);
            SortCommodityHand();
        }
    }

    public void PickCard(int resource)
    {
        pickCardPanel.SetActive(false);
        InitCard(resource);
        SetNumOfCardsInPanel();
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        Utils.RaiseEventForMaster(RaiseEventsCode.FinishPickCard);
    }

    private void SortResourceHand()
    {
        resourcesHand.Sort((r1, r2) => r1.resource.CompareTo(r2.resource));
        for (int i = 0; i < resourcesHand.Count; i++)
            resourcesHand[i].transform.SetSiblingIndex(i);
    }

    private void SortCommodityHand()
    {
        commodityHand.Sort((c1, c2) => c1.commodity.CompareTo(c2.commodity));
        for (int i = 0; i < commodityHand.Count; i++)
            commodityHand[i].transform.SetSiblingIndex(i);
    }

    public void SetMainPanelActive(bool flag)
    {
        mainPanel.interactable = flag;
        mainPanel.blocksRaycasts = flag;
    }

    public void SetNumOfCardsInPanel()
    {
        playerSetup.playerPanel.photonView.RPC("SetNumOfCardsText", RpcTarget.AllBufferedViaServer, (resourcesHand.Count + commodityHand.Count).ToString());
    }

    #endregion

    #region Robber Functions
    public void StartRob()
    {
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
        HashSet<int> possiblePlayersToRob = GetRobbablePlayers(vertexes);
        if (possiblePlayersToRob.Count != 0)
        {
            selectPlayerPanel.SetActive(true);
            foreach (int player in possiblePlayersToRob)
            {
                GameObject playerIconGO = Instantiate(playerIconPrefab, selectPlayerPanel.transform);
                Utils.PaintPlayerIcon(playerIconGO, player);
            }
        }
        else
        {
            EndRobberPlacement();
        }
    }

    public HashSet<int> GetRobbablePlayers(IEnumerable<int> vertexes)
    {
        HashSet<int> possiblePlayersToRob = new HashSet<int>();
        foreach (int vertex in vertexes)
        {
            if (buildManager.RivalsBuildingVertexes.ContainsKey(vertex))
            {
                possiblePlayersToRob.Add(buildManager.RivalsBuildingVertexes[vertex].owner);
            }
        }
        return possiblePlayersToRob;
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
            SetNumOfCardsInPanel();
            return resource;
        }
        else
        {
            chosen -= resourcesHand.Count;
            CommodityCard commodityCard = commodityHand[chosen];
            int commodity = (int)commodityCard.commodity;
            RemoveCommodityCardFromHand(commodityCard);
            SetNumOfCardsInPanel();
            return commodity;
        }
    }


    public void EndRobberPlacement()
    {
        turnManager.GainControl(); // change this once robber placement is good
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
        ClearOffers();
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
            if (merchantPort == (int)entry.Key || merchantFleetPorts.Contains(entry.Key))
            {
                if(entry.Value % 2 != 0)
                {
                    CheckMakeOfferButton();
                    return;
                }
                numOfEntitledCards += entry.Value / 2;
            }
            else
            {
                if (entry.Value % (int)ports[entry.Key] != 0)
                {
                    CheckMakeOfferButton();
                    return;
                }
                numOfEntitledCards += entry.Value / (int)ports[entry.Key];
            }
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
        ClearOffers();
        SetNumOfCardsInPanel();

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
        SetNumOfCardsInPanel();
    }


    public void ClearOffers()
    {
        if(offers.Count > 0)
            offers[0].GetComponent<OfferPanel>().cancelPanel.SetActive(false);

        foreach (GameObject offer in offers)
        {
            PhotonNetwork.Destroy(offer);
        }

        offers.Clear();
    }




    #endregion

    #region Development Cards Hand

    public void SetDevelopmentCardsPanelActive(bool flag)
    {
        developmentCardsPanel.interactable = flag;
        //developmentCardsPanel.blocksRaycasts = flag;
    }

    private void CheckDevelopmentCardEntitledment(int eventDice, int redDice)
    {
        eCommodity cardType = eCommodity.None;
        switch (eventDice)
        {
            case 3:
                cardType = eCommodity.Paper;
                break;
            case 4:
                cardType = eCommodity.Silk;
                break;
            case 5:
                cardType = eCommodity.Coin;
                break;
        }
        int commodityLevel;
        if (commodityPrices[cardType] == 1)
            commodityLevel = 0;
        else
            commodityLevel = commodityPrices[cardType];

        if (commodityLevel >= redDice)
            Utils.RaiseEventForMaster(RaiseEventsCode.GiveDevelopmentCard, new object[] { true, (int)cardType });
        else
            Utils.RaiseEventForMaster(RaiseEventsCode.GiveDevelopmentCard, new object[] { false });
    }

    public void ChooseDevelopmentCard(int stack)
    {
        chooseDevelopmentCardPanel.SetActive(false);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        Utils.RaiseEventForMaster(RaiseEventsCode.WinDevelopmentCard, new object[] { true, stack });
    }


    public void AddDevelopmentCard(int developmentCardType, RaiseEventsCode reportTo)
    {
        if (developmentCardType != (int)eDevelopmentCardsTypes.Constitution && developmentCardType != (int)eDevelopmentCardsTypes.Printer)
        {
            DevelopmentCard developmentCard = Instantiate(developmentCardsPrefabs[developmentCardType], developmentCardsContentPanel.transform).GetComponent<DevelopmentCard>();
            developmentHand.Add(developmentCard);
            SetDevelopmentCardsPanelActive(!developmentCardsPanel.interactable);
            SetDevelopmentCardsPanelActive(!developmentCardsPanel.interactable);


            GameObject background = PhotonNetwork.Instantiate(Consts.DisplayCardsPath + backgrounds[(int)developmentCard.color - 5].name, Vector3.zero, Quaternion.identity, 0, new object[] { playerSetup.playerPanel.photonView.ViewID, false });
            developmentCard.Background = background;
            if(developmentCardType == 0)
            {
                Alchemist alchemist = developmentCard as Alchemist;
                alchemists.Add(alchemist, alchemist.GetComponent<CanvasGroup>());
            }
            if (developmentHand.Count == 5)
            {
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.ThrowDevCard);
                ThrowDevCards = true;
                SetDevelopmentCardsPanelActive(true);
                turnManager.ActivateAlchemists(true);
            }
            else
            {
                winDevFromAtt = false;
                Utils.RaiseEventForMaster(reportTo);
            }
        }
        else
        {
            playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 1);
            if (developmentCardType == (int)eDevelopmentCardsTypes.Constitution)
            {
                PhotonNetwork.Instantiate(Consts.DisplayCardsPath + constitution.name, Vector3.zero, Quaternion.identity, 0, new object[] { playerSetup.playerPanel.photonView.ViewID, true });

            }
            else
            {
                PhotonNetwork.Instantiate(Consts.DisplayCardsPath + printer.name, Vector3.zero, Quaternion.identity, 0, new object[] { playerSetup.playerPanel.photonView.ViewID, true });
            }
            winDevFromAtt = false;
            Utils.RaiseEventForMaster(reportTo);
        }
    }

    public void DiscardDevelopmentCard(DevelopmentCard card)
    {
        SetDevelopmentCardsPanelActive(false);
        playerSetup.cardDescriptionPanel.SetActive(false);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        turnManager.ActivateAlchemists(false);
        PhotonNetwork.Destroy(card.Background);
        Utils.RaiseEventForMaster(RaiseEventsCode.ReturnDevelopmentCard, new object[] { (int)card.type, (int)card.color, winDevFromAtt });
        winDevFromAtt = false;
        developmentHand.Remove(card);
        if( card.type == eDevelopmentCardsTypes.Alchemist)
        {
            Alchemist alchemist = card as Alchemist;
            alchemists.Remove(alchemist);
        }
        Destroy(card.gameObject);
    }

    #endregion

    #region Development Cards Handlers

    private void HandleSaboteur()
    {
        int numOfCards = resourcesHand.Count + commodityHand.Count;
        state = eCardsState.Throw;
        SetMainPanelActive(true);
        numOfCardToThorw = numOfCards / 2;
        cardsToThorw = new List<Card>(numOfCardToThorw);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.Saboteur);
        throwCardsPanel.SetActive(true);
    }

    public void SpyTake(DevelopmentCard card)
    {
        card.transform.SetParent(developmentCardsContentPanel.transform);
        card.Background.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        card.Background.GetPhotonView().RPC("SetParent", RpcTarget.AllBufferedViaServer, playerSetup.playerPanel.photonView.ViewID);
        developmentHand.Add(card);
        Spy spy = playerSetup.currentCard as Spy;
        spy.Steal(card);
    }

    private void HandleWedding()
    {
        int cards = resourcesHand.Count + commodityHand.Count;
        if (cards == 0)
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishWedding, GameManager.instance.CurrentPlayer, new object[] { new int[] { } });
            return;
        }
        else if (cards == 1)
        {
            forcedCardsToGive = new List<Card>(cards);
        }
        else
        {
            forcedCardsToGive = new List<Card>(2);
        }
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.Wedding);

        SetMainPanelActive(true);
        state = eCardsState.Give;
        givePanel.SetActive(true);
    }




    public void HandleCommercialHarbor()
    {
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.CommercialHarbor);
        SetMainPanelActive(true);
        state = eCardsState.Exchange;
        exchangePanel.SetActive(true);
    }

    private void HandleResourceMonopoly(int type)
    {
        int cards = resourceCount[(eResources)type];
        if (cards == 0)
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishResourceMonopoly, GameManager.instance.CurrentPlayer, new object[] { 0 });
            return;
        }
        else if (cards == 1)
        {
            RemoveCardFromHandByType(type);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishResourceMonopoly, GameManager.instance.CurrentPlayer, new object[] { 1 });
            SetNumOfCardsInPanel();
        }
        else
        {
            RemoveCardFromHandByType(type);
            RemoveCardFromHandByType(type);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishResourceMonopoly, GameManager.instance.CurrentPlayer, new object[] { 2 });
            SetNumOfCardsInPanel();
            
        }

    }

    private void HandleTradeMonopoly(int type)
    {
        int cards = commodityCount[(eCommodity)type];
        if (cards == 0)
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishTradeMonopoly, GameManager.instance.CurrentPlayer, new object[] { 0 });
            return;
        }
        else
        {
            RemoveCardFromHandByType(type);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishTradeMonopoly, GameManager.instance.CurrentPlayer, new object[] { 1 });
            SetNumOfCardsInPanel(); ;
        }
    }

    public void GiveCards()
    {
        if (!Utils.IsFull(forcedCardsToGive)) return;
        int[] types = new int [forcedCardsToGive.Count]; 
        for (int i = 0; i < forcedCardsToGive.Count; i++)
        {
            Card card = forcedCardsToGive[i];
            ResourceCard resourceCard = card as ResourceCard;

            if (resourceCard != null)
            {
                types[i] = (int)resourceCard.resource;
                RemoveResourceCardFromHand(resourceCard);
            }
            else
            {
                CommodityCard commodityCard = card as CommodityCard;
                types[i] = (int)commodityCard.commodity;
                RemoveCommodityCardFromHand(commodityCard);
            }
        }
        forcedCardsToGive.Clear();
        state = eCardsState.None;
        givePanel.SetActive(false);
        SetMainPanelActive(false);
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        SetNumOfCardsInPanel();
        switch (playerSetup.currentCardType)
        {
            case eDevelopmentCardsTypes.Wedding:
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishWedding, GameManager.instance.CurrentPlayer, new object[] { types });
                break;
        }
    }


    public void ExchangeCards()
    {
        if (exchangeCard == null) return;
        int type;
        ResourceCard resourceCard = exchangeCard as ResourceCard;
        if (resourceCard != null)
        {
            type = (int)resourceCard.resource;
        }
        else
        {
            CommodityCard commodityCard = exchangeCard as CommodityCard;
            type = (int)commodityCard.commodity;
        }

        state = eCardsState.None;
        exchangePanel.SetActive(false);
        SetMainPanelActive(false);
        if(GameManager.instance.CurrentPlayer == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
            CommercialHarbor commercialHarbor = playerSetup.currentCard as CommercialHarbor;
            commercialHarbor.PlayerReady = type;
            if (commercialHarbor.ExchangeReady())
                commercialHarbor.MakeExchange();
        }
        else
        {
            playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishCommercialHarbor, GameManager.instance.CurrentPlayer, new object[] { type });

        }
    }

    public void TakeCards()
    {
        if (!Utils.IsFull(cardsToTake)) return;
        int[] types = new int[cardsToTake.Count];
        for (int i = 0; i < cardsToTake.Count; i++)
        {
            Card card = cardsToTake[i];
            ResourceCard resourceCard = card as ResourceCard;
            if (resourceCard != null)
            {
                types[i] = (int)resourceCard.resource;
                AddResourceCardToHand(resourceCard, true);
            }
            else
            {
                CommodityCard commodityCard = card as CommodityCard;
                types[i] = (int)commodityCard.commodity;
                AddCommodityCardToHand(commodityCard, true);
            }
        }
        cardsToTake.Clear();
        state = eCardsState.None;
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
        SetNumOfCardsInPanel();
        
        
        MasterMerchant masterMerchant = playerSetup.currentCard as MasterMerchant;
        
        Utils.RaiseEventForPlayer(RaiseEventsCode.CompleteMasterMerchant, masterMerchant.Rival, new object[] { types });
        masterMerchant.CleanUp();

    }


    public void SetMerchant(eResources resource)
    {
        if(merchantPort != -1)
        {
            RemoveMerchant();
        }
        merchantPort = (int)resource;
        if(resource != eResources.Desert)
            UpdatePortText((int)resource, Consts.p2to1);
    }

    public void RemoveMerchant()
    {
        if(merchantPort != 100)
        {
            RemovePort((eResources)merchantPort);
        }
        merchantPort = -1;
    }


    public void AddMerchantFleet(int type)
    {
        merchantFleetPorts.Add((eResources)type);
        ((MerchantFleet)playerSetup.currentCard).CleanUp();
        UpdatePortText(type, Consts.p2to1);
    }

    public void RemovePort(eResources resource)
    {
        ePorts port = ports[resource];
        switch (port)
        {
            case ePorts.p4To1:
                UpdatePortText((int)resource, Consts.p4to1);
                break;
            case ePorts.p3To1:
                UpdatePortText((int)resource, Consts.p3to1);
                break;
            case ePorts.p2To1:
                UpdatePortText((int)resource, Consts.p2to1);
                break;
        }
    }

    public void RemoveMerchantFleets()
    {
        foreach(eResources resource in merchantFleetPorts)
        {
            RemovePort(resource);
        }
        merchantFleetPorts.Clear();
    }


    public void StartMonopoly(int type)
    {
        int[] rivals = GameManager.instance.players.Select((x) => x.ActorNumber).Where((x) => x != PhotonNetwork.LocalPlayer.ActorNumber).ToArray();
        if(type <= 4)
        {
            ResourceMonopoly rm = playerSetup.currentCard as ResourceMonopoly;
            rm.Resource = type;
            Utils.RaiseEventForGroup(RaiseEventsCode.ResourceMonopoly, rivals, new object[] { type });

        }
        else
        {
            TradeMonopoly tm = playerSetup.currentCard as TradeMonopoly;
            tm.Commodity = type;
            Utils.RaiseEventForGroup(RaiseEventsCode.TradeMonopoly, rivals, new object[] { type });
        }

    }

    #endregion
}
