using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Edge : MonoBehaviourPun
{

    #region Prefabs


    [Header("Game Pieces")]
    public GameObject roadPrefab;


    #endregion


    #region Private Fields


    private Quaternion angle;


    public PlayerSetup playerSetup;
    public BuildManager buildManager;
    private CardManager cardManager;


    #endregion


    public int owner;


    #region Properties

    public Road road;

    public int Id { get; private set; }

    public int[] NeighborEdges { get; private set; }

    public int[] Vertexes { get; private set; }




    #endregion


    #region Unity Methods


    void Awake()
    {
        object[] data = photonView.InstantiationData;
        transform.position = (Vector3)data[0];
        Id = (int)data[1];
        NeighborEdges = (int[])data[2];
        Vertexes = (int[])data[3];
        gameObject.SetActive((bool)data[4]);
        name = (string)data[5];
        transform.SetParent(PhotonView.Find((int)data[6]).gameObject.transform);
        Vector3 angles = (Vector3)data[7];
        angle = Quaternion.Euler(angles.x, angles.y, angles.z);

    }



    void OnMouseDown()
    {
        gameObject.SetActive(false);
        buildManager.edgesToTurnOff.Remove(Id);

        object[] data = new object[] { buildManager.playerColor };
        switch (buildManager.Build)
        {
            case eBuildAction.Road:

                BuildRoad(data);
                
                if (GameManager.instance.state == GameState.SetupSettlement || GameManager.instance.state == GameState.SetupCity)
                {
                    buildManager.Build = eBuildAction.None;
                    buildManager.RoadCleanUp();
                    playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
                    Utils.RaiseEventForAll(RaiseEventsCode.PassTurn);
                }
                else
                {
                    cardManager.Pay(Consts.Prices[buildManager.Build]);
                    buildManager.CleanUp();
                }
                break;
        }

        if(playerSetup.currentCard != null)
        {
            switch (playerSetup.currentCard.type)
            {
                case eDevelopmentCardsTypes.RoadBuilding:
                    RoadBuilding roadBuilding = playerSetup.currentCard as RoadBuilding;
                    BuildRoad(data);
                    if (!roadBuilding.BuiltOne)
                    {
                        buildManager.edgesToTurnOff.Remove(Id);
                        roadBuilding.BuiltOne = true;
                        roadBuilding.BuildSecondRoad(NeighborEdges, Vertexes);
                    }
                    else
                    {
                        roadBuilding.CleanUp();
                    }
                    break;
                case eDevelopmentCardsTypes.Diplomat:
                    Diplomat diplomat = playerSetup.currentCard as Diplomat;
                    BuildRoad(data);
                    buildManager.RoadCleanUp();
                    diplomat.playerRes[PhotonNetwork.LocalPlayer.ActorNumber] = true;
                    if (diplomat.AllFinished())
                        diplomat.CleanUp();
                    break;
            }
        }
    }


    #endregion


    private void BuildRoad(object[] data)
    {
        Vector3 p1 = new Vector3(transform.position.x, Consts.DROP_HIGHET, transform.position.z);
        Vector3 p0 = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        GameObject roadGameObject = PhotonNetwork.Instantiate(roadPrefab.name, p1, angle, 0, data);
        road = roadGameObject.GetComponent<Road>();
        road.InitDrop(p1, p0);
        UpdateEdges();
        photonView.RPC("SetRoad", RpcTarget.AllBufferedViaServer, road.photonView.ViewID);
        photonView.RPC("SetOwner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);

        buildManager.buildingAmounts[eBuilding.Road] -= 1;
        buildManager.playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, 0, buildManager.buildingAmounts[eBuilding.Road].ToString());
    }


    public void DestroyRoad()
    {
        Utils.RaiseEventForAll(RaiseEventsCode.RemoveRoad, new object[] { Id });
        buildManager.buildingAmounts[eBuilding.Road] += 1;
        playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)eBuilding.Road - 1, buildManager.buildingAmounts[eBuilding.Road].ToString());
        PhotonNetwork.Destroy(road.gameObject);
        photonView.RPC("SetRoad", RpcTarget.AllBufferedViaServer, -1);
    }

    #region Send Events

    void UpdateEdges()
    {
        object[] data = new object[] { Id };

        Utils.RaiseEventForAll(RaiseEventsCode.UpdateEdges, data);
    }

    #endregion


    #region RPCs


    [PunRPC]
    void SetPlayerManagers()
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
    }


    [PunRPC]
    void SetOwner(int ownerID)
    {
        owner = ownerID;
    }


    [PunRPC]
    void SetRoad(int roadViewID)
    {
        if (roadViewID == -1)
        {
            road = null;
            return;
        }
        road = PhotonView.Find(roadViewID).gameObject.GetComponent<Road>();
        road.edge = this;
    }

    #endregion

}
