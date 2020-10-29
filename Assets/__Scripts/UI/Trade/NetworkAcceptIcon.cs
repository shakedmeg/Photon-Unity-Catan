using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkAcceptIcon : MonoBehaviourPun
{
    int playerID;
    CapsuleCollider cColl;
    OfferPanel offerPanel;

    CardManager cardManager;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.SetActive(false);
        cColl = GetComponent<CapsuleCollider>();
    }


    void OnMouseDown()
    {
        Utils.RaiseEventForPlayer(RaiseEventsCode.CompleteTrade, playerID, new object[] { offerPanel.requestedCards, offerPanel.offeredCards });
        cardManager.CompleteTrade(offerPanel.offeredCards, offerPanel.requestedCards);
        cardManager.CloseTrade();
    }


    // Replaces the player icon in the responses
    [PunRPC]
    public void Init(int parentViewID, int siblindIndx, string colorName, int sender)
    {
        playerID = sender;
        offerPanel = PhotonView.Find(parentViewID).GetComponent<OfferPanel>();
        
        transform.SetParent(offerPanel.transform.Find(Consts.ResponsesPanel), false);
        transform.SetSiblingIndex(siblindIndx);
        gameObject.SetActive(true);
        Color color = Utils.Name_To_Color(colorName);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);

        if (photonView.IsMine)
        {
            cColl.enabled = true;
            cardManager = PlayerSetup.LocalPlayerInstance.GetComponent<CardManager>();
        }
    }
}
