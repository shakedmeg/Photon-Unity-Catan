using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;


public class Card : MonoBehaviour, IPointerDownHandler
{

    private CardManager cardManager;
    private PlayerSetup playerSetup;
    private bool toThorw = false;
    public bool toTrade = false;
    public bool toGive = false;
    public bool toExchange = false;
    public bool toTake = false;
    public bool IsInGetList { get; set; } = false;


    void Awake ()
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
        cardManager = PlayerSetup.LocalPlayerInstance.GetComponent<CardManager>();
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
            case eCardsState.Give:
                toGive = !toGive;
                if (toGive)
                {
                    if (Utils.IsFull(cardManager.forcedCardsToGive))
                    {
                        toGive = !toGive;
                        return;
                    }
                    cardManager.forcedCardsToGive.Add(this);
                    transform.SetParent(cardManager.giveCardsContent.transform);
                }
                else
                {
                    cardManager.forcedCardsToGive.Remove(this);
                    cardManager.AddCardToHandPanel(this);
                }
                break;
            case eCardsState.Exchange:
                toExchange = !toExchange;
                if (toExchange)
                {
                    if (GameManager.instance.CurrentPlayer == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        ResourceCard resourceCard = this as ResourceCard;
                        if (resourceCard == null)
                        {
                            toExchange = !toExchange;
                            return;
                        }
                        else
                        {
                            if (cardManager.exchangeCard != null)
                            {
                                toExchange = !toExchange;
                                return;
                            }
                            else
                            {
                                cardManager.exchangeCard = this;
                                transform.SetParent(cardManager.exchangeCardsContent.transform);
                            }
                        }
                    }
                    else
                    {
                        CommodityCard commodityCard = this as CommodityCard;
                        if (commodityCard == null)
                        {
                            toExchange = !toExchange;
                            return;
                        }
                        else
                        {
                            if (cardManager.exchangeCard != null)
                            {
                                toExchange = !toExchange;
                                return;
                            }
                            else
                            {
                                cardManager.exchangeCard = this;
                                transform.SetParent(cardManager.exchangeCardsContent.transform);
                            }
                        }

                    }
                }
                else
                {
                    cardManager.exchangeCard = null;
                    cardManager.AddCardToHandPanel(this);
                }
                break;

            case eCardsState.Take:
                toTake = !toTake;
                MasterMerchant masterMerchant = playerSetup.currentCard as MasterMerchant;
                if (toTake)
                {
                    if (Utils.IsFull(cardManager.cardsToTake))
                    {
                        toTake = !toTake;
                        return;
                    }
                    cardManager.cardsToTake.Add(this);
                    masterMerchant.RivalCards.Remove(gameObject);
                    transform.SetParent(cardManager.takeCardsContent.transform);
                }
                else
                {
                    cardManager.cardsToTake.Remove(this);
                    masterMerchant.RivalCards.Remove(gameObject);
                    playerSetup.AddCardToHandViewPanel(this);
                }
                break;
        }
    }
}
