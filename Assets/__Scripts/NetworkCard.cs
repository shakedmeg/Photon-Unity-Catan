using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkCard : MonoBehaviourPun
{
    CardManager cardManager;

    void Awake()
    {
        object[] data = photonView.InstantiationData;
        int parentViewID = (int)data[0];
        string parentName = (string)data[1];
        transform.SetParent(PhotonView.Find(parentViewID).transform.Find(parentName), false);
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        }
    }

    [PunRPC]
    public void SetParent()
    {

    }
}
