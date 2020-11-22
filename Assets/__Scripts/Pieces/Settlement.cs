using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Settlement : VertexGamePiece
{


    void OnMouseDown()
    {
        StopScaling();
        switch (Vertex.buildManager.Build)
        {
            case eBuildAction.City:
                BuildCity();
                Vertex.AfterBuild();

                break;
        }

        if (Vertex.playerSetup.currentCard != null)
        {
            BuildCity();
            Medicine medicine = Vertex.playerSetup.currentCard as Medicine;
            Vertex.cardManager.Pay(medicine.Price);
            Vertex.buildManager.CityCleanUp();
            medicine.CleanUp();
        }

    }


    private void BuildCity()
    {
        Vertex.buildManager.StopScalingBuildings(Vertex.buildManager.settlements, eBuilding.Settlement);
        Vertex.p1 = Vertex.transform.position + new Vector3(-1, Consts.DROP_HIGHET, 0);
        Vertex.p0 = Vertex.transform.position + new Vector3(-1, 1.25f, 0);
        Vertex.BuildCity();
        Vertex.playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 1);

        Vertex.buildManager.cityCount += 1;
        Vertex.turnManager.barbarians.photonView.RPC("BuildCity", RpcTarget.AllBufferedViaServer);
    }
}
