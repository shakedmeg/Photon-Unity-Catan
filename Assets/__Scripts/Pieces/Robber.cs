using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Robber : TileGamePiece
{

    private CardManager cardManager;
    private BuildManager buildManager;
    private TurnManager turnManager;

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
        turnManager.SetControl(false);
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
        cardManager = PlayerSetup.LocalPlayerInstance.GetComponent<CardManager>();
        buildManager = PlayerSetup.LocalPlayerInstance.GetComponent<BuildManager>();
        turnManager = PlayerSetup.LocalPlayerInstance.GetComponent<TurnManager>();
    }

    [PunRPC]
    public void SetParent(int parentViewID)
    {
        Tile tile = PhotonView.Find(parentViewID).GetComponent<Tile>();
        transform.SetParent(tile.gameObject.transform);
        Tile = tile;
    }
}