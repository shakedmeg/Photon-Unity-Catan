using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;


public class City : VertexGamePiece
{
    [SerializeField]
    GameObject cityImprovementPrefab;

    CityImprovement cityImprovement;

    bool wall = false;
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
                break;
        }
    }

    void Update()
    {
        Drop();
        ScaleUpDown();
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
                Debug.Log(cityImprovement.photonView.ViewID);
                Utils.RaiseEventForAll(RaiseEventsCode.SetImproveCityID, new object[] { (int)improveType, cityImprovement.photonView.ViewID });
                cityImprovement.InitDrop(p1, p0);

                CleanUp();
                break;
            case eBuildAction.TakeImprovedCity:
                Setup(ref position, ref p0, ref p1);

                Debug.Log(improveType);
                Debug.Log(Vertex.buildManager.improveCityViewID[improveType]);
                Debug.Log(PhotonView.Find(Vertex.buildManager.improveCityViewID[improveType]));


                cityImprovement = PhotonView.Find(Vertex.buildManager.improveCityViewID[improveType]).GetComponent<CityImprovement>();
                cityImprovement.photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                cityImprovement.InitDrop(p1, p0);
                
                CleanUp();
                break;
            case eBuildAction.Destroy:
                Vertex.buildManager.StopScalingCities(Vertex.buildManager.regularCities);
                Vertex.DestroyCity();
                break;
        }
    }

    private void Setup(ref Vector3 position, ref Vector3 p0, ref Vector3 p1)
    {
        position = transform.transform.position;
        p0 = wall ? new Vector3(position.x + 2, 3, position.z) : new Vector3(position.x + 2, 2.5f, position.z);
        p1 = new Vector3(position.x + 2, Consts.DROP_HIGHET, position.z);
        improveType = Vertex.buildManager.ImproveCommodity;
    }

    private void CleanUp()
    {
        Improved = true;
        Vertex.buildManager.Build = eBuildAction.None;
        Vertex.buildManager.ImproveCommodity = eCommodity.None;
        Vertex.buildManager.StopScalingCities(Vertex.buildManager.regularCities);
    }
}
