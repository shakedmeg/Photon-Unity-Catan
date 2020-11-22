using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DisplayCard : MonoBehaviourPun
{

    public eCommodity commodity;
    void Awake()
    {
        object[] initData = photonView.InstantiationData;
        PlayerPanel playerPanel = PhotonView.Find((int)initData[0]).GetComponent<PlayerPanel>();
        if((bool)initData[1])
            transform.SetParent(playerPanel.transform.Find("VictoryCardsPanel/ScrollView/VictoryCardsContentPanel"), false);
        else
            transform.SetParent(playerPanel.transform.Find("DevelopmentCardsPanel/ScrollView/DevelopmentCardsContentPanel"), false);
    }

    [PunRPC]
    public void SetParent(int viewID)
    {
        transform.SetParent(PhotonView.Find(viewID).transform.Find("DevelopmentCardsPanel/ScrollView/DevelopmentCardsContentPanel"));
    }
}
