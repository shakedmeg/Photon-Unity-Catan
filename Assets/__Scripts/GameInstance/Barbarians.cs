﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class Barbarians : MonoBehaviourPun
{
    PlayerSetup playerSetup;
    BuildManager buildManager;
    TurnManager turnManager;
    Dice dice;
    int diceNumber;

    public Slider slider;
    public Text rollsTillAttack;

    private int strength;
    public Dictionary<int, int> knightsPower;

    List<KeyValuePair<int, int>> mightNeedToLoseCity;
    List<int> needToLoseCity;
    int needToLoseCurrent;

    List<KeyValuePair<int, int>> biggestDefenders;

    private const string RollsTillAttackFormat = "Rolls Till Attack: {0}";


    List<int> winners;
    private int startHandoutFrom;

    void Awake()
    {
        gameObject.SetActive(false);
    }




    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.FinishDevelopmentCardWinHandout:
                ActivateWinPanelTo(winners);
                break;
        }
    }


    void Attack()
    {
        int playersStrength = 0;
        foreach (int str in knightsPower.Values)
            playersStrength += str;

        Utils.RaiseEventForPlayer(RaiseEventsCode.SetPlayerPanel, GameManager.instance.CurrentPlayer, new object[] { false });
        if (playersStrength < strength)
            AttackLost();
        else
            AttackWon();
    }

    void FinishAttack()
    {
        Utils.RaiseEventForAll(RaiseEventsCode.DeactivateAllKnights);
        Utils.RaiseEventForPlayer(RaiseEventsCode.SetPlayerPanel, GameManager.instance.CurrentPlayer, new object[] { true });
        Utils.RaiseEventForPlayer(RaiseEventsCode.SendDiceScore, GameManager.instance.CurrentPlayer);
        photonView.RPC("Reset", RpcTarget.All);
    }


    #region Defeat
    void AttackLost()
    {
        mightNeedToLoseCity = new List<KeyValuePair<int, int>>(knightsPower.ToList());
        mightNeedToLoseCity.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        needToLoseCurrent = 0;
        Utils.RaiseEventForPlayer(RaiseEventsCode.CheckIfCanLoseCity, mightNeedToLoseCity[0].Key);
    }

    [PunRPC]
    void ContinueLostCheck(bool response)
    {
        if (response)
            needToLoseCity.Add(mightNeedToLoseCity[needToLoseCurrent].Key);

        if (needToLoseCurrent + 1 == mightNeedToLoseCity.Count)
        {
            if (needToLoseCity.Count == 0)
                FinishAttack();

            else
                Utils.RaiseEventForGroup(RaiseEventsCode.LoseCity, needToLoseCity.ToArray());

            return;
        }

        if (mightNeedToLoseCity[needToLoseCurrent].Value == mightNeedToLoseCity[needToLoseCurrent + 1].Value || needToLoseCity.Count == 0)
        {
            needToLoseCurrent += 1;
            Utils.RaiseEventForPlayer(RaiseEventsCode.CheckIfCanLoseCity, mightNeedToLoseCity[needToLoseCurrent].Key);
        }
        else
            Utils.RaiseEventForGroup(RaiseEventsCode.LoseCity, needToLoseCity.ToArray());
    }

    #endregion

    #region Victory
    
    void AttackWon()
    {
        biggestDefenders = new List<KeyValuePair<int, int>>(knightsPower.ToList());
        biggestDefenders.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        biggestDefenders.Reverse();
        winners = new List<int>();
        int max = 0;
        foreach (KeyValuePair<int,int> playerEntry in biggestDefenders)
        {
            if(playerEntry.Value >= max)
            {
                max = playerEntry.Value;
                winners.Add(playerEntry.Key);
            }
            else
                break;
        }
        if (winners.Count == 1) 
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.AddPoints, winners[0], new object[] { 1, true });
            FinishAttack();
        }
        else
        {
            startHandoutFrom = GameManager.instance.CurrentPlayer;
            ActivateWinPanelTo(winners);
            
        }
    }


    private void ActivateWinPanelTo(List<int> winners)
    {
        if(winners.Count == 0)
        {
            FinishAttack();
            return;
        }

        if (winners.Contains(startHandoutFrom))
        {
            winners.Remove(startHandoutFrom);
            Utils.RaiseEventForPlayer(RaiseEventsCode.ChooseDevelopmentCard, startHandoutFrom);
        }
        else
        {
            bool startLooking = false;
            for(int i = 0; i < GameManager.instance.players.Length * 2; i++)
            {
                int curr = GameManager.instance.players[i % GameManager.instance.players.Length].ActorNumber;
                if (!startLooking)
                {
                    if(curr == startHandoutFrom)
                    {
                        startLooking = true;
                    }
                }
                else
                {
                    if (winners.Contains(curr))
                    {
                        startHandoutFrom = curr;
                        winners.Remove(startHandoutFrom);
                        Utils.RaiseEventForPlayer(RaiseEventsCode.ChooseDevelopmentCard, startHandoutFrom);
                        break;
                    }
                }
            }
        }
    }
    #endregion



    [PunRPC]
    public void Init(int diceID)
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
        buildManager = PlayerSetup.LocalPlayerInstance.GetComponent<BuildManager>();
        turnManager = PlayerSetup.LocalPlayerInstance.GetComponent<TurnManager>();

        dice = PhotonView.Find(diceID).GetComponent<Dice>();

        

        transform.SetParent(playerSetup.canvas.transform, false);
        if (!PhotonNetwork.IsMasterClient) return;
        knightsPower = new Dictionary<int, int>();
        needToLoseCity = new List<int>();
        foreach (Player player in GameManager.instance.players)
        {
            knightsPower.Add(player.ActorNumber, 0);
        }
    }

    [PunRPC]
    public void Reset()
    {
        slider.value = 0;
        rollsTillAttack.text = string.Format(RollsTillAttackFormat, 7);
    }

    [PunRPC]
    public void Advance(int num)
    {
        diceNumber = num;
        slider.value += 1;
        if(slider.value == slider.maxValue)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (GameManager.instance.state != GameState.Playing)
                Utils.RaiseEventForAll(RaiseEventsCode.ActivateRobber);
            Attack();
        }
        else
        {
            rollsTillAttack.text = string.Format(RollsTillAttackFormat, 7 - slider.value);
            if (PhotonNetwork.LocalPlayer.ActorNumber != GameManager.instance.CurrentPlayer) return;
            dice.SendNumber(diceNumber);
        }
    }

    [PunRPC]
    public void ActivateKnight(int sender, int power)
    {
        knightsPower[sender] += power;
    }

    [PunRPC]
    public void DeactivateKnight(int sender, int power)
    {
        knightsPower[sender] -= power;
    }

    [PunRPC]
    public void BuildCity()
    {
        strength += 1;
    }

    [PunRPC]
    public void CityDestroyed(int actor)
    {
        strength -= 1;
        if (!PhotonNetwork.IsMasterClient) return;
        needToLoseCity.Remove(actor);

        if (needToLoseCity.Count == 0)
            FinishAttack();
    }

}
