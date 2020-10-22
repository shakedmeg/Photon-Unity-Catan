using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Diagnostics.Tracing;

public class Vertex : MonoBehaviourPun
{

    #region Prefabs


    [Header("Game Pieces")]
    public GameObject settlementPrefab;
    public GameObject cityPrefab;
    public GameObject wallPrefab;
    public GameObject knightLvl1Prefab;
    public GameObject knightLvl2Prefab;
    public GameObject knightLvl3Prefab;

    #endregion


    #region Private Fields


    private CardManager cardManager;

    private TurnManager turnManager;

    private Settlement settlement;

    private City city;
    public City City { get { return city; } }

    private Wall wall;


    private Vector3 p0;
    private Vector3 p1;

    private List<object> data;

    #endregion

    #region Public Fields
    public BuildManager buildManager;
    
    public int owner;

    public Knight knight;

    #endregion

    #region Properties

    public int ID { get; private set; }

    public int[] NeighborVertexes { get; private set; }

    public int[] Edges { get; private set; }

    public int[] Tiles { get; private set; }

    public eBuilding Building { get; set; }

    public bool HasWall { get; private set; }

    #endregion


    #region Unity Methods

    void Awake()
    {
        object[] data = photonView.InstantiationData;
        transform.position = (Vector3)data[0];
        ID = (int)data[1];
        NeighborVertexes = (int[])data[2];
        Edges = (int[])data[3];
        gameObject.SetActive((bool)data[4]);
        name = (string)data[5];
        transform.SetParent(PhotonView.Find((int)data[6]).gameObject.transform);
        Tiles = (int[])data[7];
    }



    void OnMouseDown()
    {
        gameObject.SetActive(false);
        data = new List<object>() { buildManager.playerColor };
        p1 = transform.position;
        p0 = transform.position;
        switch (buildManager.Build)
        {
            case eBuilding.Settlement:
                p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
                p0 = transform.position + Vector3.up;
                photonView.RPC("SetOwner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
                UpdateVertexes();
                if (GameManager.instance.state == GameState.SetupSettlement || GameManager.instance.state == GameState.SetupCity)
                    PreGameBuildSettlement();
                else
                    BuildSettlement();
                // todo add buildings under player game anchor
                break;

            case eBuilding.City:
                p1 = transform.position + new Vector3(-1, Consts.DROP_HIGHET, 0);
                p0 = transform.position + new Vector3(-1, 1.25f, 0);
                if (GameManager.instance.state == GameState.SetupSettlement || GameManager.instance.state == GameState.SetupCity)
                {
                    photonView.RPC("SetOwner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
                    PreGameBuildCity();
                }
                else
                    BuildCity();
                buildManager.cityCount += 1;
                turnManager.barbarians.photonView.RPC("BuildCity", RpcTarget.AllBufferedViaServer);
                break;

            case eBuilding.Wall:
                HasWall = true;
                p1 = new Vector3(transform.position.x + Consts.DropWall, transform.position.y+0.25f, transform.position.z);
                p0 = transform.position + new Vector3(0, 0.25f, 0);
                Vector3 cityP1 = city.transform.position;
                cityP1 += new Vector3(0, Consts.RaiseHighetCity, 0);
                city.InitDrop(city.transform.position, cityP1);

                Build(ref wall, wallPrefab.name, Quaternion.identity,data);
                AfterBuild();
                cardManager.allowedCards += 2;
                //buildManager.WallCleanUp();
                break;

            case eBuilding.Knight:
                BuildKnight();
                //buildManager.KnightCleanUp();
                AddKnight();
                break;

        }

        switch (buildManager.KnightAction)
        {
            case eKnightActions.TakeAction:
                buildManager.knightToMove.TurnOffKnight();
                buildManager.TurnOffKnightOptions();
                MoveKnight();

                break;
            case eKnightActions.MoveKnight:
                buildManager.TurnOffKnightOptions();
                MoveKnight();
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishMoveKnight, buildManager.notifyToPlayer, new object[]{ buildManager.displacedKnightPervId });

                break;
        }
    }


    #endregion


    #region Send Events

    void UpdateVertexes()
    {
        object[] data = new object[] { NeighborVertexes, ID };

        Utils.RaiseEventForAll(RaiseEventsCode.UpdateVertexes, data);

    }
    void AddKnight()
    {
        object[] data = new object[] { ID };

        Utils.RaiseEventForAll(RaiseEventsCode.AddKnight, data);
    }
    void SendMoveKnight(int knightToRemoveID, eKnightActions action)
    {
        object[] data = new object[] { ID, knightToRemoveID, action };
        Utils.RaiseEventForAll(RaiseEventsCode.MoveKnight, data);
    }

    #endregion


    #region Build Buildings

    private void PreGameBuildSettlement()
    {
        Build(ref settlement, settlementPrefab.name, Quaternion.identity, data, eBuilding.Settlement);
        buildManager.Build = eBuilding.None;
        buildManager.PreSetupRoad(Edges);
    }


    private void BuildSettlement()
    {
        buildManager.vertexesToTurnOff.Remove(ID);
        Build(ref settlement, settlementPrefab.name, Quaternion.identity, data, eBuilding.Settlement);
        AfterBuild();
    }


    private void PreGameBuildCity()
    {
        Build(ref city, cityPrefab.name, Quaternion.Euler(270,0,0), data, eBuilding.City);
        UpdateVertexes();
        buildManager.TilesNumsToResource(Tiles);
        buildManager.Build = eBuilding.None;
        buildManager.PreSetupRoad(Edges);

    }

    private void BuildCity()
    {

        PhotonNetwork.Destroy(settlement.gameObject);
        settlement = null;


        buildManager.vertexesToTurnOff.Remove(ID);
        Build(ref city, cityPrefab.name, Quaternion.Euler(270,0,0), data, eBuilding.City);
        AfterBuild();
    }


    public void DestroyCity()
    {
        PhotonNetwork.Destroy(city.gameObject);
        p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
        p0 = transform.position + Vector3.up;
        Build(ref settlement, settlementPrefab.name, Quaternion.identity, data, eBuilding.Settlement);
        Building = eBuilding.Settlement;
        city = null;
        buildManager.Build = eBuilding.None;
        buildManager.cityCount -= 1;
        turnManager.barbarians.photonView.RPC("CityDestroyed", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    #endregion


    #region Common Piece Helpers

    //private void Build<T>(ref T t, string name, Quaternion rotation, List<object> data, eBuilding building = eBuilding.None) where T : VertexGamePiece
    //{
    //    if (building != eBuilding.None) Building = building;
    //    t = PhotonNetwork.Instantiate(name, p1, rotation, 0, data.ToArray()).GetComponent<T>();
    //    t.InitDrop(p1, p0);
    //    t.Vertex = this;
    //    if (!(GameManager.instance.state == GameState.SetupSettlement || GameManager.instance.state == GameState.SetupCity))
    //    {   
    //        cardManager.Pay();
    //        buildManager.CleanUp();
    //    }
    //}


    private void Build<T>(ref T t, string name, Quaternion rotation, List<object> data, eBuilding building = eBuilding.None) where T : VertexGamePiece
    {
        if (building != eBuilding.None) Building = building;
        t = PhotonNetwork.Instantiate(name, p1, rotation, 0, data.ToArray()).GetComponent<T>();
        t.InitDrop(p1, p0);
        t.Vertex = this;
    }

    private void AfterBuild()
    {
        cardManager.Pay();
        buildManager.CleanUp();
    }

    #endregion


    #region Knights
    public void BuildKnight()
    {
        p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
        p0 = transform.position + Vector3.up;
        buildManager.vertexesToTurnOff.Remove(ID);
        buildManager.knightsToTurnOff.Remove(ID);
        Build(ref knight, knightLvl1Prefab.name, knightLvl1Prefab.transform.rotation, data, eBuilding.Knight);
        AfterBuild();

        PhotonView knightPhotonView = knight.GetComponent<PhotonView>();
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, knightPhotonView.ViewID);

    }


    public void UpgradeKnight()
    {
        data.AddRange(new object[] { knight.Activated, knight.Level +1 });
        p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
        p0 = transform.position + Vector3.up;
        PhotonNetwork.Destroy(knight.gameObject);
        knight = null;
        if (Building == eBuilding.Knight)
            Build(ref knight, knightLvl2Prefab.name, knightLvl2Prefab.transform.rotation, data, eBuilding.Knight2);
        else
            Build(ref knight, knightLvl3Prefab.name, knightLvl3Prefab.transform.rotation, data, eBuilding.Knight3);
        PhotonView knightPhotonView = knight.GetComponent<PhotonView>();
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, knightPhotonView.ViewID);


        data = new List<object>() { buildManager.playerColor };
        p1 = transform.position;
        p0 = transform.position;

    }

    public void MoveKnight()
    {
        SendMoveKnight(buildManager.knightToMove.Vertex.ID, eKnightActions.Move);


        // Add Knight data to this vertex
        knight = buildManager.knightToMove;
        knight.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        Building = knight.Vertex.Building;

        RemoveKnightFromVertex();

        PhotonView knightPhotonView = knight.GetComponent<PhotonView>();
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, knightPhotonView.ViewID);

        buildManager.KnightAction = eKnightActions.None;
    }


    public void RemoveKnightFromVertex()
    {
        knight.Vertex.Building = eBuilding.None;
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, -1);
    }





    #endregion







    #region RPCs


    [PunRPC]
    void SetPlayerManagers()
    {
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        turnManager = PlayerSetup.LocalPlayerInstance.GetComponent<TurnManager>();
    }

    [PunRPC]
    void SetKnight(int knightViewID)
    {
        if (knightViewID == -1)
        {
            knight = null;
            return;
        }
        knight = PhotonView.Find(knightViewID).gameObject.GetComponent<Knight>();
        knight.Vertex = this;
    }

    [PunRPC]
    void SetOwner(int ownerID)
    {
        owner = ownerID;
    }

    #endregion

}
