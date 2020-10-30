using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Tile : MonoBehaviourPun
{

    [Header("Set Dynamically")]

    public MeshRenderer mRend;

    public GameObject tileSpot;

    private CardManager cardManager;
    private BuildManager buildManager;

    private eCommodity commodity;

    private Color32 color;

    public SphereCollider sColl;

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
        Robber robber = cardManager.robber;
        robber.photonView.RPC("SetParent", RpcTarget.AllBufferedViaServer, photonView.ViewID);
        Vector3 p0 = transform.position + Consts.RobberLocalPosition;
        Vector3 p1 = p0 + new Vector3(0, Consts.DROP_HIGHET, 0);
        robber.gameObject.SetActive(true);
        robber.InitDrop(p1, p0);
        photonView.RPC("SetRobberOn", RpcTarget.AllBufferedViaServer, true);
        buildManager.TurnOffTileSpots();
        cardManager.SelectRob(Vertexes);
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


    [PunRPC]
    public void SetPlayerManagers()
    {
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
    }

    [PunRPC]
    public void SetRobberOn(bool flag)
    {
        RobberOn = flag;
    }
}
