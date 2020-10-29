using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OfferPanel : MonoBehaviourPun
{
    CardManager cardManager;
    
    public int[] offeredCards;
    public int[] requestedCards;

    List<int> iconIDs;
    List<GameObject> icons;
    List<eResponses> responses;

    public GameObject networkPlayeIconPrefab;
    public GameObject[] networkCards;
    public GameObject choosePanelPrefab;
    public GameObject networkAcceptIcon;
    public GameObject networkDeclineIcon;

    public GameObject cancelPanel;
    public CancelIcon cancelIcon;

    public void Awake()
    {
        object[] initData = photonView.InstantiationData;
        offeredCards = (int[])initData[0];
        requestedCards = (int[])initData[1];
    }


    private void InitChoosePanel(Color color)
    {

        ChoosePanel choosePanel = Instantiate(choosePanelPrefab, transform.Find(Consts.PlayerButtonsPanel), false).GetComponent<ChoosePanel>();
        if (cardManager.CheckIfCanAcceptOffer(requestedCards))
            choosePanel.acceptIcon.gameObject.SetActive(true);
        choosePanel.SetColor(color);
        choosePanel.CacheCreator(this);
    }

    private void InitPlayerIcons()
    {
        object[] data;
        iconIDs = new List<int>();
        icons = new List<GameObject>();
        responses = new List<eResponses>();
        foreach (Player player in GameManager.instance.players)
        {
            if (player != photonView.Owner)
            {
                iconIDs.Add(player.ActorNumber);
                object color;
                player.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out color);
                data = new object[] { color, photonView.ViewID };
                icons.Add(PhotonNetwork.Instantiate(networkPlayeIconPrefab.name, networkPlayeIconPrefab.transform.position, networkPlayeIconPrefab.transform.rotation, 0, data));
                responses.Add(eResponses.None);
            }
        }
    }

    private void InitCards()
    {
        object[] data;
        for (int i = 0; i < offeredCards.Length; i++)
        {
            data = new object[] { photonView.ViewID, Consts.OfferedContent };
            PhotonNetwork.Instantiate(Consts.CardsPath + networkCards[offeredCards[i]].name, Vector3.zero, Quaternion.identity, 0, data);
        }
        for (int i = 0; i < requestedCards.Length; i++)
        {
            data = new object[] { photonView.ViewID, Consts.RequestedContent };
            PhotonNetwork.Instantiate(Consts.CardsPath + networkCards[requestedCards[i]].name, Vector3.zero, Quaternion.identity, 0, data);
        }

    }

    private void SwitchIcons(GameObject responsePrefab, int sender, bool response)
    {
        int id = iconIDs.IndexOf(sender);
        eResponses eResponse = response ? eResponses.True : eResponses.False;
        responses[id] = eResponse;

        if (!CheckIfAllDeclined())
        {
            GameObject responseGO =  PhotonNetwork.Instantiate(responsePrefab.name, responsePrefab.transform.position, responsePrefab.transform.rotation);
        
            NetworkPlayerIcon netPlayerIcon = icons[id].GetComponent<NetworkPlayerIcon>();
            string color = netPlayerIcon.colorName;
            
            icons[id] = responseGO;
            PhotonNetwork.Destroy(netPlayerIcon.gameObject);
            
            responseGO.GetComponent<PhotonView>().RPC("Init", RpcTarget.AllBufferedViaServer, photonView.ViewID, id, color, sender);
        }
        else
        {
            cardManager.offers.Remove(this.gameObject);
            if (cardManager.offers.Count == 0)
                cancelPanel.SetActive(false);
            PhotonNetwork.Destroy(gameObject);
        }


    }


    private bool CheckIfAllDeclined()
    {
        foreach(eResponses response in responses)
        {
            if(response != eResponses.False)
            {
                return false;
            }
        }
        return true;
    }

    public void CancelIconPressed()
    {
        cardManager.offers.Remove(gameObject);
        if(cardManager.offers.Count == 0)
            cancelPanel.SetActive(false);
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public void Init()
    {
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        transform.SetParent(cardManager.TradeOffersPanel.transform, false);
        object colorName;
        PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out colorName);
        Color color = Utils.Name_To_Color((string)colorName);
        if (!photonView.IsMine)
        {
            InitChoosePanel(color);
        }
        else
        {
            cancelIcon.SetColor(color);
            cancelIcon.gameObject.SetActive(true);
            cancelPanel = transform.parent.Find(Consts.CancelPanel).gameObject;
            cancelPanel.SetActive(true);
            InitPlayerIcons();
            InitCards();
        }

    }

    [PunRPC]
    public void DeclinePressed(int sender)
    {
        if (!photonView.IsMine) return;
        SwitchIcons(networkDeclineIcon, sender, false);
    }

    [PunRPC]
    public void AcceptPressed(int sender)
    {
        if (!photonView.IsMine) return;
        SwitchIcons(networkAcceptIcon, sender, true);
    }


    [PunRPC]
    public void SetPlayerManagers()
    {
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
    }

    [PunRPC]
    public void SetParent()
    {
        transform.SetParent(cardManager.TradeOffersPanel.transform, false);
    }


}
