using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class Saboteur : DevelopmentCard
{

    List<int> sabotagedPlayers;
    public Dictionary<int, bool> playerRes = new Dictionary<int, bool>();
    bool activated = false;


    protected override void Awake()
    {
        base.Awake();
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
            case (byte)RaiseEventsCode.FinishSabotuer:
                if (!photonView.IsMine || !activated) return;
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

    private void InitRes()
    {
        playerRes = new Dictionary<int, bool>();
        foreach (int actor in sabotagedPlayers)
            playerRes.Add(actor, false);
    }

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        sabotagedPlayers = new List<int>();
        foreach(KeyValuePair<int,int> entry in GameManager.instance.playerPoints)
        {
            if (entry.Key != GameManager.instance.CurrentPlayer && entry.Value >= playerSetup.playerPanel.points)
            {
                sabotagedPlayers.Add(entry.Key);
            }
        }

        if (sabotagedPlayers.Count == 0)
        {
            MiniCleanUp();
            return;
        }

        Activate();
    }


    protected override void Activate()
    {
        base.Activate();
        activated = true;
        InitRes();

        Utils.RaiseEventForAll(RaiseEventsCode.SetDevelopmentCard, new object[] { (int)type });

        turnManager.SetControl(false);

        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);

        Utils.RaiseEventForGroup(RaiseEventsCode.Sabotage, sabotagedPlayers.ToArray());
    }


    public override void CleanUp()
    {
        base.CleanUp();
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
        turnManager.SetControl(true);
        Utils.RaiseEventForAll(RaiseEventsCode.SetDevelopmentCard, new object[] { (int)eDevelopmentCardsTypes.None });

    }
}
