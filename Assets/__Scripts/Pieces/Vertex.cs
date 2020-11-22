using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Vertex : MonoBehaviourPun
{

    #region Prefabs


    [Header("Game Pieces")]
    public GameObject settlementPrefab;
    public GameObject cityPrefab;
    public GameObject knightLvl1Prefab;
    public GameObject knightLvl2Prefab;
    public GameObject knightLvl3Prefab;

    #endregion


    #region Private Fields

    public PlayerSetup playerSetup;

    public CardManager cardManager;

    public TurnManager turnManager;

    public Settlement settlement;

    private City city;
    public City City { get { return city; } }



    public Vector3 p0;
    public Vector3 p1;

    private List<object> data;

    private int port;

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
        port = (int)data[8];

    }



    void OnMouseDown()
    {
        gameObject.SetActive(false);
        data = new List<object>() { buildManager.playerColor };
        p1 = transform.position;
        p0 = transform.position;

        if(playerSetup.currentCard != null)
        {
            Deserter deserter = playerSetup.currentCard as Deserter;
            buildManager.KnightCleanUp();
            GameObject knightPrefab;
            int lvl = deserter.MaxBuildableLvl;
            eBuilding knightLvl = (eBuilding)(lvl + 4);

            if(lvl == 3)
                knightPrefab = knightLvl3Prefab;
            else if(lvl == 2)
                knightPrefab = knightLvl2Prefab;
            else
                knightPrefab = knightLvl1Prefab;

            BuildKnight(knightPrefab, knightLvl);
            knight.SetLevel(lvl);
            knight.photonView.RPC("SetLevel", RpcTarget.AllBufferedViaServer, lvl);
            AddKnight();

            if (deserter.DesertedKnightActive)
            {
                knight.Useable = true;
                knight.TurnOnKnight();
            }
            deserter.CleanUp();
            return;
        }

        if(playerSetup.currentCardType == eDevelopmentCardsTypes.Intrigue)
        {
            buildManager.TurnOffKnightOptions();
            MoveKnight();
            playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishIntrigue, buildManager.notifyToPlayer);
            return;
        }




        switch (buildManager.Build)
        {
            case eBuildAction.Settlement:
                p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
                p0 = transform.position + Vector3.up;
                photonView.RPC("SetOwner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
                UpdateVertexes();
                if (GameManager.instance.state == GameState.SetupSettlement || GameManager.instance.state == GameState.SetupCity)
                    PreGameBuildSettlement();
                else
                    BuildSettlement();
                AddPort();
                buildManager.settlementsInGame += 1;
                playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 1);

                // todo add buildings under player game anchor
                break;

            case eBuildAction.City:
                p1 = transform.position + new Vector3(-1, Consts.DROP_HIGHET, 0);
                p0 = transform.position + new Vector3(-1, 1.25f, 0);

                photonView.RPC("SetOwner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
                PreGameBuildCity();
                AddPort();
                playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 2);
                
                buildManager.cityCount += 1;
                turnManager.barbarians.photonView.RPC("BuildCity", RpcTarget.AllBufferedViaServer);
                break;




            case eBuildAction.Knight:
                BuildKnight(knightLvl1Prefab, eBuilding.Knight);
                AfterBuild();
                AddKnight();
                break;

        }

        switch (buildManager.KnightAction)
        {
            case eKnightActions.TakeAction:
                buildManager.cancelButton.SetActive(false);
                turnManager.SetControl(true);
                buildManager.knightToMove.TurnOffKnight();
                buildManager.TurnOffKnightOptions();
                MoveKnight();

                break;
            case eKnightActions.MoveKnight:
                buildManager.TurnOffKnightOptions();
                MoveKnight();
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
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
        buildManager.Build = eBuildAction.None;
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
        buildManager.Build = eBuildAction.None;
        buildManager.PreSetupRoad(Edges);

    }

    public void BuildCity()
    {

        PhotonNetwork.Destroy(settlement.gameObject);
        settlement = null;


        Build(ref city, cityPrefab.name, Quaternion.Euler(270,0,0), data, eBuilding.City);

        buildManager.settlementsInGame -= 1;
        buildManager.buildingAmounts[eBuilding.Settlement] = buildManager.settlementsInGame >= 5 ? 0 : 5 - buildManager.settlementsInGame;
        playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)eBuilding.Settlement - 1, buildManager.buildingAmounts[eBuilding.Settlement].ToString());

    }


    public void DestroyCity()
    {
        PhotonNetwork.Destroy(city.gameObject);
        p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
        p0 = transform.position + Vector3.up;
        buildManager.buildingAmounts[eBuilding.Settlement] = buildManager.buildingAmounts[eBuilding.Settlement] == 0? 1: buildManager.buildingAmounts[eBuilding.Settlement];
        Build(ref settlement, settlementPrefab.name, Quaternion.identity, data, eBuilding.Settlement);

        buildManager.settlementsInGame += 1;


        Building = eBuilding.Settlement;
        city = null;

        buildManager.buildingAmounts[eBuilding.City] += 1;
        playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)eBuilding.City - 1, buildManager.buildingAmounts[eBuilding.City].ToString());

        buildManager.Build = eBuildAction.None;
        buildManager.cityCount -= 1;
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, -1);
        
        turnManager.barbarians.photonView.RPC("CityDestroyed", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    #endregion


    #region Common Piece Helpers

    public void Build<T>(ref T t, string name, Quaternion rotation, List<object> data, eBuilding building = eBuilding.None) where T : VertexGamePiece
    {
        if (building != eBuilding.None) Building = building;
        t = PhotonNetwork.Instantiate(name, p1, rotation, 0, data.ToArray()).GetComponent<T>();
        t.InitDrop(p1, p0);
        t.Vertex = this;
        buildManager.buildingAmounts[Building] -= 1;
        playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)Building-1, buildManager.buildingAmounts[Building].ToString());
    }

    public void AfterBuild()
    {
        cardManager.Pay(Consts.Prices[buildManager.Build]);
        buildManager.CleanUp();
    }

    #endregion


    #region Knights
    public void BuildKnight(GameObject knightPrefab, eBuilding knightLvl)
    {
        p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
        p0 = transform.position + Vector3.up;
        buildManager.vertexesToTurnOff.Remove(ID);
        buildManager.knightsToTurnOff.Remove(ID);
        Build(ref knight, knightPrefab.name, knightPrefab.transform.rotation, data, knightLvl);

        PhotonView knightPhotonView = knight.GetComponent<PhotonView>();
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, knightPhotonView.ViewID);

    }


    public void UpgradeKnight()
    {
        BuildUpgradedKnight();
        AfterBuild();

        data = new List<object>() { buildManager.playerColor };
        p1 = transform.position;
        p0 = transform.position;

    }

    public void BuildUpgradedKnight()
    {
        data.AddRange(new object[] { knight.Activated, knight.Useable });
        p1 = transform.position + new Vector3(0, Consts.DROP_HIGHET, 0);
        p0 = transform.position + Vector3.up;
        PhotonNetwork.Destroy(knight.gameObject);
        knight = null;
        if (Building == eBuilding.Knight)
        {
            Build(ref knight, knightLvl2Prefab.name, knightLvl2Prefab.transform.rotation, data, eBuilding.Knight2);
            knight.photonView.RPC("SetLevel", RpcTarget.AllBufferedViaServer, 2);
            buildManager.buildingAmounts[eBuilding.Knight] += 1;
            playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)eBuilding.Knight - 1, buildManager.buildingAmounts[eBuilding.Knight].ToString());
        }
        else
        {
            Build(ref knight, knightLvl3Prefab.name, knightLvl3Prefab.transform.rotation, data, eBuilding.Knight3);
            knight.photonView.RPC("SetLevel", RpcTarget.AllBufferedViaServer, 3);
            buildManager.buildingAmounts[eBuilding.Knight2] += 1;
            playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)eBuilding.Knight2 - 1, buildManager.buildingAmounts[eBuilding.Knight2].ToString());
        }
        if (knight.Activated)
        {
            playerSetup.playerPanel.photonView.RPC("SetActivatedKnightsText", RpcTarget.AllBufferedViaServer, 1);
        }
        PhotonView knightPhotonView = knight.GetComponent<PhotonView>();
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, knightPhotonView.ViewID);
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
        buildManager.knightToMove = null;
    }


    public void RemoveKnightFromVertex()
    {
        knight.Vertex.Building = eBuilding.None;
        photonView.RPC("SetKnight", RpcTarget.AllBufferedViaServer, -1);
    }


    public void DestroyKnight()
    {
        if (knight.Activated)
        {
            turnManager.barbarians.photonView.RPC("DeactivateKnight", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, knight.Level);
            playerSetup.playerPanel.photonView.RPC("SetActivatedKnightsText", RpcTarget.AllBufferedViaServer, (-1)*knight.Level);
        }
        buildManager.buildingAmounts[Building] += 1;
        playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, (int)Building - 1, buildManager.buildingAmounts[Building].ToString());
        PhotonNetwork.Destroy(knight.gameObject);
        RemoveKnightFromVertex();
    }



    #endregion



    private void AddPort()
    {
        if (port == -1) return;
        cardManager.UpatePort(port);
    }



    #region RPCs


    [PunRPC]
    void SetPlayerManagers()
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
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
