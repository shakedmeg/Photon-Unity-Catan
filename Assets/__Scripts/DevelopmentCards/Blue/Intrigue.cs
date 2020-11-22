using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class Intrigue : DevelopmentCard
{

    HashSet<Knight> knights;
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
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.FinishIntrigue:
                if (!photonView.IsMine || !activated) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
                CleanUp();
                break;
        }
    }


    public void StopScalingKnights()
    {
        foreach (Knight knight in knights)
        {
            knight.StopScaling();
        }
    }


    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        knights = new HashSet<Knight>();
        foreach(Edge edge in buildManager.PlayerRoads.Values)
        {
            foreach(int vertexID in edge.Vertexes)
            {
                if (buildManager.RivalsKnights.ContainsKey(vertexID))
                {
                    Knight knight = buildManager.RivalsKnights[vertexID].knight;
                    knights.Add(knight);
                }
            }
        }

        if (knights.Count == 0)
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
        Utils.RaiseEventForAll(RaiseEventsCode.SetDevelopmentCard, new object[] { (int)type });

        turnManager.SetControl(false);

        foreach (Knight knight in knights)
        {
            Vector3 s1 = knight.Level == 3 ? Consts.ScaleKnight3 : Consts.ScaleKnight;
            knight.InitScaleUpDown(knight.transform.localScale, s1);
        }
    }

    public override void CleanUp()
    {
        base.CleanUp();
        Utils.RaiseEventForAll(RaiseEventsCode.SetDevelopmentCard, new object[] { (int)eDevelopmentCardsTypes.None });
        turnManager.SetControl(true);
    }
}
