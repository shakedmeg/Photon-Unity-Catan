using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MerchantPiece : TileGamePiece
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
        transform.position = tile.transform.position + Consts.MerchantLocalPosition;
        Tile = tile.GetComponent<Tile>();
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
