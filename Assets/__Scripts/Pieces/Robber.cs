using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Robber : TileGamePiece
{

    private CardManager cardManager;
    private BuildManager buildManager;

    //public CapsuleCollider cColl;

    public override void Awake()
    {
        coll = GetComponent<CapsuleCollider>();
        object[] data = photonView.InstantiationData;
        GameObject tile = PhotonView.Find((int)data[0]).gameObject;
        transform.SetParent(tile.transform);
        transform.position = tile.transform.position + Consts.RobberLocalPosition;
        Tile = tile.GetComponent<Tile>();
    }

    void OnMouseDown() 
    {
        buildManager.cancelButton.SetActive(false);
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        coll.enabled = false;
        StopScaling();
        if (buildManager.knightToMove != null)
        {
            buildManager.knightToMove.TurnOffKnight();
            buildManager.TurnOffKnightOptions();
            buildManager.knightToMove = null;
        }

        cardManager.StartRob();
    }


    public override void StopScaling()
    {
        base.StopScaling();
        transform.localScale = Consts.RobberRegularScale;
    }

    public void SetCollider(bool flag)
    {
        coll.enabled = flag;
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