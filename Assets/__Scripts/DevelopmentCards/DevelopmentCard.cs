using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public abstract class DevelopmentCard : MonoBehaviourPun, IPointerEnterHandler, IPointerExitHandler
{

    // ONCE DONE CHANGE TO PROTECTED

    public BuildManager buildManager;
    public CardManager cardManager;
    public TurnManager turnManager;
    public PlayerSetup playerSetup;

    public eDevelopmentCardsTypes type;
    public eCommodity color;

    public Image image;
    public GameObject Background { get; set; }
    public string cardName;
    public string description;

    protected virtual void Awake()
    {
        cardManager = PlayerSetup.LocalPlayerInstance.GetComponent<CardManager>();
        buildManager = PlayerSetup.LocalPlayerInstance.GetComponent<BuildManager>();
        turnManager = PlayerSetup.LocalPlayerInstance.GetComponent<TurnManager>();
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
    }

    public void Init()
    {
        if (cardManager.ThrowDevCards)
        {
            cardManager.ThrowDevCards = false;
            cardManager.DiscardDevelopmentCard(this);
            return;
        }else if (playerSetup.currentCard != null)
        {
            playerSetup.spyShowPanel.SetActive(false);
            cardManager.SpyTake(this);
            return;
        }
        playerSetup.currentCard = this;
        CheckIfCanActivate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        playerSetup.cardNameText.text = cardName;
        playerSetup.cardDescriptionText.text = description;
        playerSetup.cardDescriptionPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        playerSetup.cardDescriptionPanel.SetActive(false);
    }

    protected void DisplayCard(bool flag)
    {
        Utils.RaiseEventForAll(RaiseEventsCode.SetDisplayCard, new object[] { flag, (int)type });
        
    }

    protected virtual void Activate()
    {
        DisplayCard(true);
    }

    protected virtual void CheckIfCanActivate()
    {
        cardManager.CloseTrade();
        cardManager.SetDevelopmentCardsPanelActive(false);
    }

    protected void MiniCleanUp()
    {
        playerSetup.currentCard = null;
        cardManager.SetDevelopmentCardsPanelActive(true);
    }

    public virtual void CleanUp()
    {
        MiniCleanUp();
        PhotonNetwork.Destroy(Background);
        Utils.RaiseEventForMaster(RaiseEventsCode.ReturnDevelopmentCardAfterUsage, new object[] { (int)type, (int)color });
        cardManager.developmentHand.Remove(this);
        DisplayCard(false);
        Destroy(gameObject);
    }
}
