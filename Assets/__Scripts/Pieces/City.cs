using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;


public class City : VertexGamePiece
{
    [SerializeField]
    GameObject cityImprovementPrefab = null;

    CityImprovement cityImprovement;


    [SerializeField]
    private GameObject wallPrefab = null;

    private Wall wall;

    public bool HasWall { get; private set; }
    public bool Improved { get; internal set; } = false;

    private eCommodity improveType;


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
            case (byte)RaiseEventsCode.LoseImproveCity:
                data = (object[])photonEvent.CustomData;
                if (improveType != (eCommodity)data[0]) return;
                Improved = false;
                cityImprovement = null;
                Vertex.playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, -2);
                break;
        }
    }


    void OnMouseDown()
    {
        StopScaling();
        Vector3 position = Vector3.zero;
        Vector3 p0 = Vector3.zero;
        Vector3 p1 = Vector3.zero;
        switch (Vertex.buildManager.Build)
        {
            case eBuildAction.ImproveCity:
                Setup(ref position, ref p0, ref p1);

                cityImprovement = PhotonNetwork.Instantiate(cityImprovementPrefab.name, p1, cityImprovementPrefab.transform.rotation, 0, new object[] { Utils.ECommodityToString(improveType)}).GetComponent<CityImprovement>();
                Utils.RaiseEventForAll(RaiseEventsCode.SetImproveCityID, new object[] { (int)improveType, cityImprovement.photonView.ViewID });
                cityImprovement.InitDrop(p1, p0);

                Vertex.playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 2);

                CleanUp();
                break;
            case eBuildAction.TakeImprovedCity:
                Setup(ref position, ref p0, ref p1);


                cityImprovement = PhotonView.Find(Vertex.buildManager.improveCityViewID[improveType]).GetComponent<CityImprovement>();
                cityImprovement.photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                cityImprovement.InitDrop(p1, p0);
                
                Vertex.playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 2);
                
                CleanUp();
                break;
            case eBuildAction.Destroy:
                Vertex.buildManager.StopScalingBuildings(Vertex.buildManager.regularCities, eBuilding.City);
                Vertex.DestroyCity();
                if (HasWall)
                {
                    PhotonNetwork.Destroy(wall.gameObject);
                    Vertex.cardManager.allowedCards -= 2;
                    HasWall = false;
                }
                break;

            case eBuildAction.Wall:
                BuildWall(p0, p1);
                Vertex.AfterBuild();
                break;
        }

        if(Vertex.playerSetup.currentCard != null)
        {
            BuildWall(p0, p1);
            Vertex.playerSetup.currentCard.CleanUp();
        }
    }

    private void Setup(ref Vector3 position, ref Vector3 p0, ref Vector3 p1)
    {
        position = transform.transform.position;
        p0 = HasWall ? new Vector3(position.x + 2, 3, position.z) : new Vector3(position.x + 2, 2.5f, position.z);
        p1 = new Vector3(position.x + 2, Consts.DROP_HIGHET, position.z);
        improveType = Vertex.buildManager.ImproveCommodity;
    }

    private void CleanUp()
    {
        Improved = true;
        Vertex.buildManager.Build = eBuildAction.None;
        Vertex.buildManager.ImproveCommodity = eCommodity.None;
        Vertex.buildManager.StopScalingBuildings(Vertex.buildManager.regularCities, eBuilding.City);
        Vertex.turnManager.SetControl(true);
    }

    public void BuildWall(Vector3 p0, Vector3 p1)
    {
        Vertex.buildManager.StopScalingBuildings(Vertex.buildManager.regularCities, eBuilding.City);
        HasWall = true;
        p1 = Vertex.transform.position + new Vector3(Consts.DropWall, 0.25f, 0);
        p0 = Vertex.transform.position + new Vector3(0, 0.25f, 0);
        Vector3 cityP1 = transform.position;
        cityP1 += new Vector3(0, Consts.RaiseHighetCity, 0);
        InitDrop(transform.position, cityP1);

        if (Improved)
        {
            Vector3 improvedP1 = cityImprovement.transform.position;
            improvedP1 += new Vector3(0, Consts.RaiseHighetCity, 0);
            cityImprovement.InitDrop(cityImprovement.transform.position, improvedP1);
        }



        wall = PhotonNetwork.Instantiate(wallPrefab.name, p1, Quaternion.identity, 0, new object[] { Vertex.buildManager.playerColor }).GetComponent<Wall>();
        wall.InitDrop(p1, p0);
        wall.Vertex = Vertex;
        Vertex.buildManager.buildingAmounts[eBuilding.Wall] -= 1;
        Vertex.playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)eBuilding.Wall - 1, Vertex.buildManager.buildingAmounts[eBuilding.Wall].ToString());


        Vertex.cardManager.allowedCards += 2;
    }

}
