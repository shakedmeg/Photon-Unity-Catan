using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;


public class Diplomat : DevelopmentCard
{
    public HashSet<Edge> interactableRoad;
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
            case (byte)RaiseEventsCode.FinishDiplomat:
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
        foreach (Player actor in GameManager.instance.players)
            playerRes.Add(actor.ActorNumber, false);
    }

    public void StopScalingRoads()
    {
        foreach (Edge edge in interactableRoad)
            edge.road.StopScaling();
    }

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        interactableRoad = buildManager.GetInteractableRoads();
        if (interactableRoad.Count == 0)
        {
            MiniCleanUp();
            return;
        }

        Activate();
    }


    protected override void Activate()
    {
        base.Activate();
        InitRes();
        activated = true;
        turnManager.SetControl(false);
    }


    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);
    }
}
