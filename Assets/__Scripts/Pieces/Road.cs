using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Road : GamePiece
{

    public Edge edge;

    void OnMouseDown()
    {
        Diplomat diplomat = edge.playerSetup.currentCard as Diplomat;
        diplomat.interactableRoad.Remove(edge);
        StopScaling();
        if (edge.owner != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Utils.RaiseEventForPlayer(RaiseEventsCode.LoseRoad, edge.owner, new object[] { edge.Id });
        }
        else
        {
            edge.DestroyRoad();
        }
        diplomat.StopScalingRoads();
    }

}
