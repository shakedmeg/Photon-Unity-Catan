using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Linq;

public class Tile : MonoBehaviourPun
{

    [Header("Set Dynamically")]

    public MeshRenderer mRend;

    public GameObject tileSpot;

    private CardManager cardManager;
    private BuildManager buildManager;
    private PlayerSetup playerSetup;

    private eCommodity commodity;

    private Color32 color;

    public SphereCollider sColl;

    public Probability probability;

    public bool RobberOn { get; set; }

    public eResources Resource { get; set; }
    public eCommodity Commodity { get { return commodity; } }


    private List<int> vertexes ;
    public List<int> Vertexes { get { return vertexes; } }

    void Awake() {

        mRend = GetComponent<MeshRenderer>();
        sColl = GetComponent<SphereCollider>();
        object[] data = photonView.InstantiationData;
        if (data == null) return;
        Resource = (eResources)data[0];
        SetCommodityFromResource(Resource);
        SetColor(Consts.TILE_COLOR[Resource]);
        vertexes = new List<int>((int[])data[1]);
        transform.SetParent(PhotonView.Find((int)data[2]).gameObject.transform);

    }


    void OnMouseDown()
    {
        if(playerSetup.currentCard == null)
        {
            PlaceRobber();
            cardManager.SelectRob(Vertexes);
            return;
        }

        switch (playerSetup.currentCard.type)
        {
            case eDevelopmentCardsTypes.Inventor:
                Inventor inventor = playerSetup.currentCard as Inventor;
                if (inventor.FirstTile == null)
                {
                    tileSpot.SetActive(false);
                    sColl.enabled = false;
                    inventor.FirstTile = this;

                }
                else
                {
                    Utils.RaiseEventForAll(RaiseEventsCode.SwitchProbs, new object[] { inventor.FirstTile.photonView.ViewID, photonView.ViewID });
                    inventor.CleanUp();
                }
                break;
            case eDevelopmentCardsTypes.Bishop:
                Bishop bishop = playerSetup.currentCard as Bishop;
                PlaceRobber();
                int[] robbablePlayers = cardManager.GetRobbablePlayers(vertexes).ToArray();
                if(robbablePlayers.Length == 0)
                {
                    bishop.CleanUp();
                }
                else
                {
                    bishop.SetPlayers(robbablePlayers);
                    Utils.RaiseEventForGroup(RaiseEventsCode.LoseCard, robbablePlayers);
                }
                break;
            case eDevelopmentCardsTypes.Merchant:
                PlaceMerchant();
                cardManager.SetMerchant(Resource);
                ((Merchant)playerSetup.currentCard).CleanUp();
                break;
        }
    }

    public void SetCommodityFromResource(eResources resource){
        switch (resource)
        {
            case eResources.Wood:
                commodity = eCommodity.Paper;
                return;
                
            case eResources.Ore:
                commodity = eCommodity.Coin;
                return;
                
            case eResources.Wool:
                commodity = eCommodity.Silk;
                return;
          
            default:
                commodity = eCommodity.None;
                return;

        }
    }

    public void SetColor(Color32 newColor){
        color = newColor;
        mRend.material.color = color;
    }


    private void PlaceRobber()
    {
        Robber robber = cardManager.robber;
        robber.photonView.RPC("SetParent", RpcTarget.AllBufferedViaServer, photonView.ViewID);
        Vector3 p0 = transform.position + Consts.RobberLocalPosition;
        Vector3 p1 = p0 + new Vector3(0, Consts.DROP_HIGHET, 0);
        robber.gameObject.SetActive(true);
        robber.InitDrop(p1, p0);
        photonView.RPC("SetRobberOn", RpcTarget.AllBufferedViaServer, true);
        buildManager.TurnOffTileSpots();
    }

    private void PlaceMerchant()
    {
        MerchantPiece merchant = cardManager.merchant;
        merchant.photonView.RPC("SetParent", RpcTarget.AllBufferedViaServer, photonView.ViewID);
        Vector3 p0 = transform.position + Consts.MerchantLocalPosition;
        Vector3 p1 = p0 + new Vector3(0, Consts.DROP_HIGHET, 0);
        merchant.gameObject.SetActive(true);
        merchant.InitDrop(p1, p0);
        buildManager.TurnOffTileSpots();
    }

    [PunRPC]
    public void SetPlayerManagers()
    {
        cardManager = PlayerSetup.LocalPlayerInstance.GetComponent<CardManager>();
        buildManager = PlayerSetup.LocalPlayerInstance.GetComponent<BuildManager>();
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
    }

    [PunRPC]
    public void SetRobberOn(bool flag)
    {
        RobberOn = flag;
    }
}
