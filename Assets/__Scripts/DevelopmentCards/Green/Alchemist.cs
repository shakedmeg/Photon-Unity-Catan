using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Alchemist : DevelopmentCard
{
    public CanvasGroup canvasGroup;

    protected override void CheckIfCanActivate()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Activate();
    }

    protected override void Activate()
    {
        base.Activate();
        turnManager.ActivateAlchemists(false);
        turnManager.Dice.StopScaling();
        playerSetup.dicePanel.SetActive(true);
    }

    public override void CleanUp()
    {
        playerSetup.currentCard = null;
        Utils.RaiseEventForMaster(RaiseEventsCode.ReturnDevelopmentCardAfterUsage, new object[] { (int)type, (int)color });
        PhotonNetwork.Destroy(Background);
        cardManager.alchemists.Remove(this);
        cardManager.developmentHand.Remove(this);
        DisplayCard(false);
        Destroy(gameObject);
    }
}
