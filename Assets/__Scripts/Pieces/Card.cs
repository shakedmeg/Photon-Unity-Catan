using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler
{

    private CardManager cardManager;
    private bool toThorw = false;
    public bool toTrade = false;
    public bool IsInGetList { get; set; } = false;

    void Awake ()
    {
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        switch (cardManager.state)
        {
            case eCardsState.None:
                toTrade = true;
                cardManager.state = eCardsState.Trade;
                cardManager.StartTrade(this);
                transform.SetParent(cardManager.tradeGivePanel.transform);
                break;
            case eCardsState.Throw:
                toThorw = !toThorw;
                if (toThorw)
                {
                    if (Utils.IsFull(cardManager.cardsToThorw))
                    {
                        toThorw = !toThorw;
                        return;
                    }
                    cardManager.cardsToThorw.Add(this);
                    transform.SetParent(cardManager.throwCardsContent.transform);
                }
                else
                {
                    cardManager.cardsToThorw.Remove(this);
                    cardManager.AddCardToHandPanel(this);
                }
                break;
            case eCardsState.Trade:
                if (IsInGetList)
                {
                    cardManager.cardsToGet.Remove(this);
                    if (cardManager.cardsToGive.Count == 0 && cardManager.cardsToGet.Count == 0)
                        cardManager.CloseTrade();
                    else
                        cardManager.CheckBankButton();
                    Destroy(gameObject);
                    return;
                }

                toTrade = !toTrade;
                if (toTrade) 
                {
                    cardManager.cardsToGive.Add(this);
                    cardManager.AddCardToTrade(this);
                    transform.SetParent(cardManager.tradeGivePanel.transform);
                    cardManager.CheckBankButton();
                }
                else
                {
                    cardManager.cardsToGive.Remove(this);
                    cardManager.RemoveCardFromTrade(this);
                    if (cardManager.cardsToGive.Count == 0 && cardManager.cardsToGet.Count == 0)
                        cardManager.CloseTrade();
                    else
                        cardManager.CheckBankButton();
                    cardManager.AddCardToHandPanel(this);
                }
                break;
        }
    }

    public void SetClicks(bool flag)
    {
        enabled = flag;
    }

}
