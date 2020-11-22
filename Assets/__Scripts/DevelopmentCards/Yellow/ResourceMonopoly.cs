using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class ResourceMonopoly : DevelopmentCard
{
    public Dictionary<int, bool> playerRes = new Dictionary<int, bool>();

    public int Resource;

    bool activated = false;


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
        object[] data;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.FinishResourceMonopoly:
                if (!photonView.IsMine || !activated) return;
                data = (object[])photonEvent.CustomData;
                int cards = (int)data[0];
                for(int i = cards; i > 0; i--)
                {
                    cardManager.InitCard(Resource);
                }
                cardManager.SetNumOfCardsInPanel();
                playerRes[photonEvent.Sender] = true;
                if (AllFinished())
                    CleanUp();
                break;
        }
    }

    public bool AllFinished()
    {
        foreach (bool res in playerRes.Values)
        {
            if (!res) return false;
        }
        return true;
    }

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();

        foreach (Player player in GameManager.instance.players)
        {
            if (player.ActorNumber != GameManager.instance.CurrentPlayer)
            {
                playerRes.Add(player.ActorNumber, false);
            }
        }

        Activate();
    }

    protected override void Activate()
    {
        base.Activate();
        activated = true;
        turnManager.SetControl(false);

        playerSetup.resourceMonopolyPanel.SetActive(true);
    }


    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);

        playerSetup.resourceMonopolyPanel.SetActive(false);

        playerRes.Clear();
    }
}
