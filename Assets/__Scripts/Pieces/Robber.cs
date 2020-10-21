using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Robber : TileGamePiece
{

    private CardManager cardManager;
    private BuildManager buildManager;

    public CapsuleCollider cColl;

    public override void Awake()
    {
        cColl = GetComponent<CapsuleCollider>();
        object[] data = photonView.InstantiationData;
        GameObject tile = PhotonView.Find((int)data[0]).gameObject;
        transform.SetParent(tile.transform);
        transform.position = tile.transform.position + Consts.RobberLocalPosition;
        Tile = tile.GetComponent<Tile>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Drop();
        ScaleUpDown();
    }

    void OnMouseDown() 
    {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        cColl.enabled = false;
        StopScaling();
        buildManager.knightToMove.TurnOffKnight();
        cardManager.StartRob();
    }


    public override void StopScaling()
    {
        base.StopScaling();
        transform.localScale = Consts.RobberRegularScale;
    }


    [PunRPC]
    public void SetPlayerManagers()
    {
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
    }

    [PunRPC]
    public void SetParent(int parentViewID)
    {
        transform.SetParent(PhotonView.Find(parentViewID).gameObject.transform);
    }
}