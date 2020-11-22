using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;


public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public static GameObject LocalPlayerInstance;

    private TurnManager turnManager;
    private CardManager cardManager;
    private BuildManager buildManager;

    public Canvas canvas;

    public GameObject playersDataPanel;

    public GameObject playerPanelPrefab;

    public PlayerPanel playerPanel;

    public List<GameObject> buttonIcons;

    #region Development Cards Fields

    public GameObject developmentCardImagePanel;
    public Image developmentCardImage;
    public List<Sprite> images;

    public GameObject cardDescriptionPanel;
    public Text cardDescriptionText;
    public Text cardNameText;

    public DevelopmentCard currentCard = null;
    public eDevelopmentCardsTypes currentCardType = eDevelopmentCardsTypes.None;

    [Header("Alchemist Fields")]
    public GameObject dicePanel;

    public GameObject redDice;
    public GameObject yellowDice;

    public Dropdown redDiceDropDown;
    public Dropdown yellowDiceDropDown;

    [Header("Crane Fields")]
    public GameObject upgradeCommodityPanel;
    public List<Button> commodityButtons;
    public List<Text> commodityButtonsText;

    [Header("Deserter Fields")]
    public GameObject deserterPanel;

    [Header("Spy Fields")]
    public GameObject spyPanel;
    public GameObject spyShowPanel;

    [Header("Commerical Harbor Fields")]
    public GameObject commercialHarborPanel;
    public GameObject declineCommercial;

    [Header("Master Merchant Fields")]
    public GameObject masterMerchantPanel;
    public GameObject viewPanel;
    public GameObject takePanel;

    [Header("Merchant Fleet Fields")]
    public GameObject merchantFleetPanel;
    public List<GameObject> merchantFleetOptions;
    
    [Header("Resource Monopoly Fields")]
    public GameObject resourceMonopolyPanel;

    [Header("Trade Monopoly Fields")]
    public GameObject tradeMonopolyPanel;
    
    [Header("Victory Points Cards")]
    public GameObject defenderCard;

    #endregion

    void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            canvas.gameObject.SetActive(true);
            canvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

            object color;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out color);
            string playerColor = (string)color;
            playerPanel = PhotonNetwork.Instantiate(playerPanelPrefab.name, playerPanelPrefab.transform.position, playerPanelPrefab.transform.rotation, 0, new object[] { playerColor }).GetComponent<PlayerPanel>();
            playerPanel.photonView.RPC("SetParent", RpcTarget.All);
            turnManager = GetComponent<TurnManager>();
            cardManager = GetComponent<CardManager>();
            buildManager = GetComponent<BuildManager>();

            foreach (GameObject go in buttonIcons)
                SetColor(Utils.Name_To_Color(playerColor), go.GetComponent<MeshRenderer>(), go);
        }
    }

    void SetColor(Color color, MeshRenderer mRend, GameObject go)
    {
        foreach (Renderer mr in go.GetComponentsInChildren<Renderer>())
        {
            mr.material.color = new Color(color.r, color.g, color.b, mr.material.color.a);
        }
        if (mRend != null)
            mRend.material.color = color;
    }

    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        object[] data;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.PreSetupSettlement:
            case (byte)RaiseEventsCode.PreSetupCity:
            case (byte)RaiseEventsCode.StartTurn:
                if (!photonView.IsMine) return;
                playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
                break;
            case (byte)RaiseEventsCode.SetPlayerPanel:
                data = (object[])photonEvent.CustomData;
                if (!photonView.IsMine) return;
                playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, (bool)data[0]);
                break;
            case (byte)RaiseEventsCode.ActivateSpyPanel:
                if (!photonView.IsMine) return;
                playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.SpyVictim);
                break;
            case (byte)RaiseEventsCode.AddPoints:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, (int)data[0]);
                if(data.Length > 1)
                {
                    PhotonNetwork.Instantiate(Consts.DisplayCardsPath + defenderCard.name, Vector3.zero, Quaternion.identity, 0, new object[] { playerPanel.photonView.ViewID, true });
                }
                break;
            case (byte)RaiseEventsCode.SetDevelopmentCard:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                currentCardType = (eDevelopmentCardsTypes)((int)data[0]);
                break;
            case (byte)RaiseEventsCode.SetDisplayCard:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                bool flag = (bool)data[0];
                if (flag)
                {
                    developmentCardImage.sprite = images[(int)data[1]];
                }
                developmentCardImagePanel.SetActive(flag);

                break;
            case (byte)RaiseEventsCode.ActivateLongestRoad:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                playerPanel.photonView.RPC("SetLongestRoadTextBold", RpcTarget.AllBufferedViaServer, (bool)data[0]);
                break;
            case (byte)RaiseEventsCode.GameOver:
                if (!photonView.IsMine) return;
                canvas.gameObject.SetActive(false);
                break;
        }
    }


    #region Alchemist Functions

    public void SetDice(bool changeRedDice)
    {
        if (changeRedDice)
            redDice.transform.localRotation = Quaternion.Euler(Consts.Quats[redDiceDropDown.value]);
        else
            yellowDice.transform.localRotation = Quaternion.Euler(Consts.Quats[yellowDiceDropDown.value]);
    }

    public void SendScore()
    {
        dicePanel.SetActive(false);
        Alchemist alchemist = currentCard as Alchemist;
        alchemist.CleanUp();
        turnManager.Dice.HandleResults(yellowDiceDropDown.value, redDiceDropDown.value, Random.Range(0, 6));
    }

    #endregion


    #region Crane Functions
    public void ImproveCity(int commodityType)
    {
        eCommodity commodity = (eCommodity)commodityType;
        for (int i = 0; i < cardManager.commodityPrices[commodity] - 1; i++)
            cardManager.RemoveCardFromHandByType(commodityType);

        string level = cardManager.commodityPrices[commodity].ToString();
        cardManager.improveButtons[commodityType - 5].text = string.Format("Level: {0}", level);
        commodityButtonsText[commodityType - 5].text = string.Format("Level: {0}", level);
        playerPanel.photonView.RPC("SetCommodityText", RpcTarget.AllBufferedViaServer, commodityType - 5, level);

        cardManager.commodityPrices[commodity] += 1;
        switch (cardManager.commodityPrices[commodity])
        {
            case 4:
                cardManager.OpenPerk(commodity);
                turnManager.SetControl(true);
                break;
            case 5:
                Utils.RaiseEventForAll(RaiseEventsCode.CheckImporveCity, new object[] { commodityType, 4 });
                break;
            case 6:
                Utils.RaiseEventForAll(RaiseEventsCode.CheckImporveCity, new object[] { commodityType, 5 });
                break;
            default:
                turnManager.SetControl(true);
                break;
        }

        currentCard.CleanUp();
    }
    #endregion

    #region Master Merchant Functions
    public void AddCardToHandViewPanel(Card card)
    {
        card.transform.SetParent(viewPanel.transform);
    }
    #endregion
}
