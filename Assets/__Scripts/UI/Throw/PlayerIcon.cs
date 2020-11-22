using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerIcon : MonoBehaviour
{
    PlayerSetup playerSetup;
    CardManager cardManager;
    CapsuleCollider cColl;
    public int Owner { get; set; }

    void Awake()
    {
        cardManager = PlayerSetup.LocalPlayerInstance.GetComponent<CardManager>();
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
        cColl = GetComponent<CapsuleCollider>();
    }
    void OnMouseDown()
    {
        if(playerSetup.currentCard == null)
        {
            cColl.enabled = false;
            cardManager.FinishSelect();
            Utils.RaiseEventForPlayer(RaiseEventsCode.LoseCard, Owner);
            return ;
        }

        switch (playerSetup.currentCard.type)
        {
            case eDevelopmentCardsTypes.Deserter:
                playerSetup.deserterPanel.SetActive(false);
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
                Utils.RaiseEventForPlayer(RaiseEventsCode.ChooseKnightToLose, Owner);
                break;
            case eDevelopmentCardsTypes.Spy:
                playerSetup.spyPanel.SetActive(false);
                Spy spy = playerSetup.currentCard as Spy;
                Utils.RaiseEventForPlayer(RaiseEventsCode.ActivateSpyPanel, Owner);
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.Spy);
                spy.ShowCards(Owner);
                break;
            case eDevelopmentCardsTypes.CommercialHarbor:
                playerSetup.commercialHarborPanel.SetActive(false);
                playerSetup.declineCommercial.SetActive(true);
                CommercialHarbor commercialHarbor = playerSetup.currentCard as CommercialHarbor;
                commercialHarbor.BeginExchange(Owner);

                Utils.RaiseEventForPlayer(RaiseEventsCode.CommercialHarbor, Owner);
                break;
            case eDevelopmentCardsTypes.MasterMerchant:
                playerSetup.masterMerchantPanel.SetActive(false);
                MasterMerchant masterMerchant = playerSetup.currentCard as MasterMerchant;
                masterMerchant.Rival = Owner;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.MasterMerchantTaker);
                Utils.RaiseEventForPlayer(RaiseEventsCode.MasterMerchant, Owner);
                break;
        }

    }

    public void SetColor(Color color)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
        }
    }


}
