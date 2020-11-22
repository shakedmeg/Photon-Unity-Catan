using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using System.Linq;

public class BuildManager : MonoBehaviourPun
{

    public string playerColor;

    public Button[] buttons;
    public GameObject cancelButton;


    #region Private Fields

    public PlayerSetup playerSetup;
    private CardManager cardManager;
    private TurnManager turnManager;

    private Dictionary<int, string> buttonHandlers = new Dictionary<int, string>()
    {
        { 0, Consts.BuildRoad },
        { 1, Consts.BuildSettlement },
        { 2, Consts.BuildCity },
        { 3, Consts.BuildWall },
        { 4, Consts.BuildKnight },
        { 5, Consts.UpgradeKnight },
        { 6, Consts.ActivateKnight },
        { 7, Consts.UpgradeCity }
    };

    private Dictionary<int, string> buttonCleanUps = new Dictionary<int, string>()
    {
        { 0, Consts.RoadCleanUp },
        { 1, Consts.SettlementCleanUp },
        { 2, Consts.CityCleanUp },
        { 3, Consts.WallCleanUp },
        { 4, Consts.KnightCleanUp },
        { 5, Consts.UpgradeKnightCleanUp },
        { 6, Consts.ActivateKnightCleanUp },
        { 7, Consts.UpgradeCityCleanUp }
    };

    #endregion



    public HashSet<int> edgesToTurnOff = new HashSet<int>();

    public HashSet<int> vertexesToTurnOff = new HashSet<int>();

    public HashSet<int> knightsToTurnOff = new HashSet<int>();

    public HashSet<int> knightsOptionsToTurnOff = new HashSet<int>();




    #region Cached Data Fields


    public List<Tile> tiles = new List<Tile>();

    public Dictionary<int, Vertex> FreeVertexes { get; private set; } = new Dictionary<int, Vertex>();
    public Dictionary<int, Edge> FreeEdges { get; private set; } = new Dictionary<int, Edge>();
    public Dictionary<int, Vertex> FreeKnights { get; private set; } = new Dictionary<int, Vertex>();

    public Dictionary<int, Vertex> PlayerBuildings { get; private set; } = new Dictionary<int, Vertex>();
    public int settlementsInGame = 0;
    public int cityCount = 0;

    public Dictionary<int, Edge> PlayerRoads { get; private set; } = new Dictionary<int, Edge>();
    public Dictionary<int, Vertex> PlayerKnights { get; private set; } = new Dictionary<int, Vertex>();


    public Dictionary<int, Vertex> RivalsBuildingVertexes { get; private set; } = new Dictionary<int, Vertex>();
    public Dictionary<int, Vertex> RivalsKnights { get; private set; } = new Dictionary<int, Vertex>();
    public Dictionary<int, Edge> RivalRoads { get; private set; } = new Dictionary<int, Edge>();

    public Dictionary<eCommodity, int> improveCityViewID = new Dictionary<eCommodity, int>()
    {
        { eCommodity.Coin, -1 },
        { eCommodity.Paper, -1 },
        { eCommodity.Silk, -1 },
    };


    private Dictionary<string, List<Tile>> probNumberTile = new Dictionary<string, List<Tile>>();



    public Knight knightToMove;


    public int notifyToPlayer;

    public int displacedKnightPervId;

    public List<Vertex> settlements = new List<Vertex>();
    public List<Vertex> regularCities = new List<Vertex>();


    public Dictionary<eBuilding, int> buildingAmounts = new Dictionary<eBuilding, int>()
    {
        { eBuilding.Road, 15 },
        { eBuilding.Settlement, 5 },
        { eBuilding.City, 4 },
        { eBuilding.Wall, 3 },
        { eBuilding.Knight, 2 },
        { eBuilding.Knight2, 2 },
        { eBuilding.Knight3, 2 },
    };


    #endregion


    #region Properties

    public bool CanBuildKnightsLvl3 { get; set; } = false;
    public eBuildAction Build { get; set; }
    public eKnightActions KnightAction { get; set; }

    public eCommodity ImproveCommodity { get; set; }
    #endregion


    #region Unity Methods


    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            object color;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out color);
            playerColor = (string)color;
            playerSetup = GetComponent<PlayerSetup>();
            cardManager = GetComponent<CardManager>();
            turnManager = GetComponent<TurnManager>();
        }
    }


    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }



    #endregion



    #region Raise Events Handlers


    void OnEvent(EventData photonEvent)
    {
        object[] data;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.SendMapData:
                data = (object[])photonEvent.CustomData;
                InitMapStructs((int[])data[1], (int[])data[2], (int[])data[3]);
                break;
            case (byte)RaiseEventsCode.PreSetupSettlement:
                Build = eBuildAction.Settlement;
                SetDictActive(FreeVertexes, true);
                break;
            case (byte)RaiseEventsCode.PreSetupCity:
                Build = eBuildAction.City;
                SetDictActive(FreeVertexes, true);
                break;
            case (byte)RaiseEventsCode.MatchTilesToDice:
                data = (object[])photonEvent.CustomData;
                GetTilesWithDiceNumber((string)data[0], true);
                break;
            case (byte)RaiseEventsCode.UpdateVertexes:
                data = (object[])photonEvent.CustomData;
                UpdateVertexes((int[])data[0], (int)data[1], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.UpdateEdges:
                data = (object[])photonEvent.CustomData;
                UpdateEdges((int)data[0], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.AddKnight:
                data = (object[])photonEvent.CustomData;
                AddKnight((int)data[0], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.MoveKnight:
                data = (object[])photonEvent.CustomData;
                MoveKnight((int)data[0], (int)data[1], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.RemoveKnight:
                data = (object[])photonEvent.CustomData;
                RemoveKnight((int)data[0], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.DisplaceKnight:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                notifyToPlayer = photonEvent.Sender;
                if (playerSetup.currentCardType == eDevelopmentCardsTypes.Intrigue)
                {
                    playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.DisplaceKnightIntrigue);
                }
                else
                {
                    playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.DisplaceKnight);
                }
                DisplaceKnight((int)data[0]);
                break;
            case (byte)RaiseEventsCode.FinishMoveKnight:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
                turnManager.SetControl(true);
                data = (object[])photonEvent.CustomData;
                knightToMove.TurnOffKnight();
                if (FreeVertexes.ContainsKey((int)data[0])) FreeVertexes[(int)data[0]].MoveKnight();
                if (FreeKnights.ContainsKey((int)data[0])) FreeKnights[(int)data[0]].MoveKnight();
                break;
            case (byte)RaiseEventsCode.CheckIfCanLoseCity:
                if (!photonView.IsMine) return;
                turnManager.barbarians.photonView.RPC("ContinueLostCheck", RpcTarget.MasterClient, CountUnimprovedCities() != 0);
                break;
            case (byte)RaiseEventsCode.LoseCity:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.LoseCity);
                Build = eBuildAction.Destroy;
                foreach (Vertex vertex in regularCities)
                    vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
                break;
            case (byte)RaiseEventsCode.ImproveCity:
                data = (object[])photonEvent.CustomData;
                Build = eBuildAction.ImproveCity;
                ImproveCommodity = (eCommodity)data[0];
                foreach (Vertex vertex in regularCities)
                    vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
                break;
            case (byte)RaiseEventsCode.TakeImproveCity:
                data = (object[])photonEvent.CustomData;
                Build = eBuildAction.TakeImprovedCity;
                ImproveCommodity = (eCommodity)data[0];
                foreach (Vertex vertex in regularCities)
                    vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
                break;
            case (byte)RaiseEventsCode.SetImproveCityID:
                data = (object[])photonEvent.CustomData;
                SetImprovementViewID((eCommodity)data[0], (int)data[1]);
                break;
            case (byte)RaiseEventsCode.CheckIfNeedToPick:
                data = (object[])photonEvent.CustomData;
                GetTilesWithDiceNumber((string)data[0], false);
                break;
            case (byte)RaiseEventsCode.DeactivateAllKnights:
                if (!photonView.IsMine) return;
                foreach (Vertex vertex in PlayerKnights.Values)
                    if (vertex.knight.Activated)
                        vertex.knight.TurnOffKnight();
                break;
            case (byte)RaiseEventsCode.CheckRoads:
                if (!photonView.IsMine) return;
                CalcLongestRoad();
                break;
            case (byte)RaiseEventsCode.ChooseKnightToLose:
                if (!photonView.IsMine) return;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, Consts.LoseKnight);
                ScaleAllKnight();
                break;
            case (byte)RaiseEventsCode.BuildDesertedKnight:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
                BuildDesertedKnight((int)data[0], (bool)data[1]);
                break;
            case (byte)RaiseEventsCode.LoseRoad:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                PlayerRoads[(int)data[0]].DestroyRoad();
                break;
            case (byte)RaiseEventsCode.RemoveRoad:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                RemoveRoad((int)data[0], photonEvent.Sender);
                break;
            case (byte)RaiseEventsCode.SwitchProbs:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                SwitchProbs((int)data[0], (int)data[1]);
                break;
        }

    }


    private void InitMapStructs(int[] tilesIDs, int[] vertexesIDs, int[] edgesIDs)
    {
        for (int i = 0; i < vertexesIDs.Length; i++)
        {
            FreeVertexes.Add(i, PhotonView.Find(vertexesIDs[i]).gameObject.GetComponent<Vertex>());
        }

        for (int i = 0; i < edgesIDs.Length; i++)
        {
            FreeEdges.Add(i, PhotonView.Find(edgesIDs[i]).gameObject.GetComponent<Edge>());
        }


        bool desertFound = false;
        string key;
        for (int i = 0; i < Consts.NumOfTiles; i++)
        {
            Tile tile = PhotonView.Find(tilesIDs[i]).gameObject.GetComponent<Tile>();
            tiles.Add(tile);
            if (tile.Resource == eResources.Desert) {
                desertFound = true;
                continue;
            }

            key = desertFound ? Consts.Probabilitiys[i - 1] : Consts.Probabilitiys[i];

            if (probNumberTile.ContainsKey(key))
            {
                probNumberTile[key].Add(tile);
            }
            else
            {
                probNumberTile.Add(key, new List<Tile>() { tile });
            }
        }
    }

    public void SetDictActive(Dictionary<int, Vertex> d, bool flag)
    {
        if (!photonView.IsMine) return;
        foreach (KeyValuePair<int, Vertex> entry in d)
        {
            entry.Value.gameObject.SetActive(flag);
        }
    }

    public void UpdateVertexes(int[] toRemove, int id, int sender)
    {
        if (!photonView.IsMine) return;
        Vertex vertex;
        for (int i = 0; i < toRemove.Length; i++)
        {
            vertex = RemoveIfExist(FreeVertexes, toRemove[i]);
            if (vertex != null)
                AddIfAbsent(FreeKnights, toRemove[i], vertex);

        }
        vertex = RemoveIfExist(FreeVertexes, id);
        if (vertex != null)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == sender)
            {
                PlayerBuildings.Add(id, vertex);
            }
            else
            {
                RivalsBuildingVertexes.Add(id, vertex);
            }
        }
    }

    void UpdateEdges(int edgeID, int sender)
    {
        if (!photonView.IsMine) return;
        Edge edge;
        edge = RemoveIfExist(FreeEdges, edgeID);
        if (edge != null && PhotonNetwork.LocalPlayer.ActorNumber == sender)
        {
            PlayerRoads.Add(edgeID, edge);
            CalcLongestRoad();
        }
        else
        {
            RivalRoads.Add(edgeID, edge);
        }

    }

    void AddKnight(int knightID, int sender)
    {
        if (!photonView.IsMine) return;
        Vertex knight;

        knight = RemoveIfExist(FreeKnights, knightID);
        if (knight == null) knight = RemoveIfExist(FreeVertexes, knightID);

        if (PhotonNetwork.LocalPlayer.ActorNumber == sender)
        {
            PlayerKnights.Add(knightID, knight);
            UpdateLongestRoad(knight);
        }
        else
        {
            RivalsKnights.Add(knightID, knight);
        }
    }

    void RemoveKnight(int knightIDToRemove, int sender)
    {
        if (!photonView.IsMine) return;
        Vertex knightToRemove;

        if (PhotonNetwork.LocalPlayer.ActorNumber == sender)
        {
            knightToRemove = RemoveIfExist(PlayerKnights, knightIDToRemove);
            UpdateLongestRoad(knightToRemove);
        }
        else
            knightToRemove = RemoveIfExist(RivalsKnights, knightIDToRemove);


        bool addToFreeVertexes = true;
        foreach (int vertex in knightToRemove.NeighborVertexes)
        {
            if (PlayerBuildings.ContainsKey(vertex) || RivalsBuildingVertexes.ContainsKey(vertex))
            {
                addToFreeVertexes = false;
                break;
            }
        }

        if (addToFreeVertexes)
            FreeVertexes.Add(knightIDToRemove, knightToRemove);
        else
            FreeKnights.Add(knightIDToRemove, knightToRemove);

    }

    void MoveKnight(int knightIDToAdd, int knightIDToRemove, int sender)
    {
        if (!photonView.IsMine) return;
        AddKnight(knightIDToAdd, sender);
        RemoveKnight(knightIDToRemove, sender);
    }

    void DisplaceKnight(int knightID)
    {
        knightToMove = PlayerKnights[knightID].knight;
        displacedKnightPervId = knightToMove.Vertex.ID;
        HashSet<int> vertexes = CalcKnightsActionsOptions(PlayerKnights[knightID], new HashSet<int>(), new HashSet<int>(), false);
        if (vertexes.Count == 0)
        {
            if (knightToMove.Activated)
            {
                turnManager.barbarians.photonView.RPC("DeactivateKnight", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, knightToMove.Level);
                playerSetup.playerPanel.photonView.RPC("SetActivatedKnightsText", RpcTarget.AllBufferedViaServer, (-1) * knightToMove.Level);
            }
            buildingAmounts[(eBuilding)(4 + knightToMove.Level)] += 1;
            playerSetup.playerPanel.photonView.RPC("SetBuildingText", RpcTarget.AllBufferedViaServer, 3 + knightToMove.Level, buildingAmounts[(eBuilding)(4 + knightToMove.Level)].ToString());
            knightToMove.Vertex.RemoveKnightFromVertex();
            Utils.RaiseEventForAll(RaiseEventsCode.RemoveKnight, new object[] { displacedKnightPervId });
            PhotonNetwork.Destroy(knightToMove.gameObject);
            playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
            if (playerSetup.currentCardType == eDevelopmentCardsTypes.Intrigue)
            {
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishIntrigue, notifyToPlayer);
            }
            else
            {
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishMoveKnight, notifyToPlayer, new object[] { displacedKnightPervId });
            }
        }
        else
        {
            KnightAction = eKnightActions.MoveKnight;
            ShowKnightsActionsOptions(vertexes);
        }
    }

    /// <summary>
    /// Compares the tiles probabilities to the rolled number, probabilty with robber on it will be skipped
    /// </summary>
    /// <param name="number"> The number the dice had rolled</param>
    /// <param name="toHand"> If true will add the cards strighat to the hand, if false it will cache them in the card manager class</param>
    public void GetTilesWithDiceNumber(string number, bool toHand)
    {
        if (!photonView.IsMine) return;
        cardManager.cachedRollCards.Clear();
        foreach (Tile tile in probNumberTile[number])
        {
            if (tile.RobberOn || tile.Resource == eResources.Desert) continue;
            foreach (int vertex in tile.Vertexes)
            {
                if (PlayerBuildings.ContainsKey(vertex))
                {
                    if (toHand)
                        cardManager.AddCardsFromRoll(tile, vertex);
                    else
                        cardManager.AddCardsToCache(tile, vertex);
                }
            }
        }
        if (!toHand)
        {
            Utils.RaiseEventForMaster(RaiseEventsCode.GreenPlayerResponse, new object[] { cardManager.cachedRollCards.Count == 0 });
        }
    }

    public void SetImprovementViewID(eCommodity commodityType, int viewID)
    {
        improveCityViewID[commodityType] = viewID;
    }

    #endregion






    #region Functions Called From Tiles, Vertexs and Edges


    public void TurnOffValuesFromDict<T>(Dictionary<int, T> d, IEnumerable<int> valuesToTurnOff) where T : Component
    {
        foreach (int id in valuesToTurnOff)
        {
            d[id].gameObject.SetActive(false);
        }

    }
    public void TurnOffValuesFromDict(Dictionary<int, GameObject> d, IEnumerable<int> valuesToTurnOff)
    {
        foreach (int id in valuesToTurnOff)
        {
            d[id].SetActive(false);
        }

    }

    public void TurnOffKnightOptions()
    {
        foreach (int vertexID in knightsOptionsToTurnOff)
        {
            if (FreeVertexes.ContainsKey(vertexID)) FreeVertexes[vertexID].gameObject.SetActive(false);
            if (FreeKnights.ContainsKey(vertexID)) FreeKnights[vertexID].gameObject.SetActive(false);
        }
    }

    public void PreSetupRoad(int[] roads)
    {

        Build = eBuildAction.Road;
        SetDictActive(FreeVertexes, false);

        for (int i = 0; i < roads.Length; i++)
        {
            FreeEdges[roads[i]].gameObject.SetActive(true);
        }
        edgesToTurnOff = new HashSet<int>(roads);
    }


    /// <summary>
    /// Checks the resource on the given tiles id's and send them back to the player
    /// </summary>
    /// <param name="tilesIds"> Tiles to be checked</param>
    public void TilesNumsToResource(int[] tilesIds)
    {
        List<int> resources = new List<int>();

        for (int i = 0; i < tilesIds.Length; i++)
        {
            int resource = (int)tiles[tilesIds[i]].Resource;
            if (resource != 100)
                resources.Add(resource);
        }

        cardManager.InitCards(resources);

    }


    public bool CheckIfKnightCanMoveRobber(int[] tileIDs)
    {
        if (GameManager.instance.state != GameState.Playing)
            return false;
        foreach (int tileID in tileIDs)
            if (tiles[tileID].RobberOn) return true;
        return false;
    }


    public HashSet<int> CalcKnightsActionsOptions(Vertex vertex, HashSet<int> vertexes, HashSet<int> vistiedVertexes, bool isDisplace)
    {
        vistiedVertexes.Add(vertex.ID);
        foreach (int edgeID in vertex.Edges)
        {
            if (PlayerRoads.ContainsKey(edgeID))
            {
                Edge edge = PlayerRoads[edgeID];
                foreach (int optinalVertex in edge.Vertexes)
                {
                    if (vistiedVertexes.Contains(optinalVertex)) continue;

                    if (RivalsBuildingVertexes.ContainsKey(optinalVertex)) continue;
                    if (RivalsKnights.ContainsKey(optinalVertex))
                    {
                        if (!isDisplace) continue;
                        // Check here if the knight is weaker than mine if so he needs to be added to the knights
                        if (RivalsKnights[optinalVertex].knight.Level < knightToMove.Level)
                            vertexes.Add(optinalVertex);
                        continue;
                    }
                    Vertex nextVertex = null;
                    if (FreeVertexes.ContainsKey(optinalVertex)) nextVertex = FreeVertexes[optinalVertex];
                    if (FreeKnights.ContainsKey(optinalVertex)) nextVertex = FreeKnights[optinalVertex];
                    if (nextVertex != null)
                    {
                        vertexes.Add(optinalVertex);
                    }
                    if (PlayerBuildings.ContainsKey(optinalVertex)) nextVertex = PlayerBuildings[optinalVertex];
                    if (PlayerKnights.ContainsKey(optinalVertex)) nextVertex = PlayerKnights[optinalVertex];
                    CalcKnightsActionsOptions(nextVertex, vertexes, vistiedVertexes, isDisplace);
                }
            }
        }
        return vertexes;

    }


    public void ShowKnightsActionsOptions(HashSet<int> knightOptionsIDs)
    {
        foreach (int knightOptionsID in knightOptionsIDs)
        {
            if (FreeVertexes.ContainsKey(knightOptionsID)) FreeVertexes[knightOptionsID].gameObject.SetActive(true);
            if (FreeKnights.ContainsKey(knightOptionsID)) FreeKnights[knightOptionsID].gameObject.SetActive(true);
            if (RivalsKnights.ContainsKey(knightOptionsID))
            {
                Knight rivalKnight = RivalsKnights[knightOptionsID].knight;
                rivalKnight.InitScaleUpDown(rivalKnight.transform.localScale, Consts.ScaleKnight);
            }
        }

        knightsOptionsToTurnOff = knightOptionsIDs;
    }


    public void StopScalingBuildings(List<Vertex> buildings, eBuilding building)
    {
        foreach (Vertex vertex in buildings)
        {
            if (building == eBuilding.City)
                vertex.City.StopScaling();
            else
                vertex.settlement.StopScaling();
        }
        buildings.Clear();
    }


    public void StopScalingKnights()
    {
        foreach (int knightID in knightsToTurnOff)
        {
            Knight knight = PlayerKnights[knightID].knight;
            knight.StopScaling();
            if (knight.Useable)
                knight.SetCollider(true);
        }
    }


    public void TurnOffTileSpots()
    {
        foreach (Tile tile in tiles)
        {
            tile.tileSpot.SetActive(false);
            tile.sColl.enabled = false;
        }
    }

    #endregion

    #region Functions Called From Buttons

    public void ButtonHandler(int buildOption)
    {
        if (Build != eBuildAction.None)
        {
            int oldBuild = (int)Build - 1;
            buttons[oldBuild].enabled = true;
            Invoke(buttonCleanUps[oldBuild], 0);
        }

        Build = (eBuildAction)buildOption + 1;
        buttons[buildOption].enabled = false;
        cancelButton.gameObject.SetActive(true);
        Invoke(buttonHandlers[buildOption], 0);


    }


    public void BuildRoad()
    {
        Build = eBuildAction.Road;


        edgesToTurnOff = CalcRoads();
        if (edgesToTurnOff.Count == 0)
            CleanUp();
    }

    public HashSet<int> CalcRoads()
    {
        HashSet<int> openEdgesToDisplay = new HashSet<int>();

        foreach (Vertex vertex in PlayerBuildings.Values)
        {
            openEdgesToDisplay.UnionWith(vertex.Edges);
        }

        foreach (Edge edge in PlayerRoads.Values)
        {
            int[] vertexNeighbors = edge.Vertexes;
            HashSet<int> neighborEdges = new HashSet<int>(edge.NeighborEdges);
            for (int i = 0; i < vertexNeighbors.Length; i++)
            {
                if (RivalsBuildingVertexes.ContainsKey(vertexNeighbors[i]))
                {
                    neighborEdges.ExceptWith(RivalsBuildingVertexes[vertexNeighbors[i]].Edges);
                }
                else if (RivalsKnights.ContainsKey(vertexNeighbors[i]))
                {
                    neighborEdges.ExceptWith(RivalsKnights[vertexNeighbors[i]].Edges);
                }
            }
            openEdgesToDisplay.UnionWith(neighborEdges);
        }

        openEdgesToDisplay.IntersectWith(FreeEdges.Keys);

        SetActiveValuesInDict(FreeEdges, openEdgesToDisplay);

        return openEdgesToDisplay;
    }

    public void BuildSettlement()
    {
        Build = eBuildAction.Settlement;

        HashSet<int> openVertexesToDisplay = new HashSet<int>();

        foreach (Edge edge in PlayerRoads.Values)
        {
            openVertexesToDisplay.UnionWith(edge.Vertexes);
        }

        openVertexesToDisplay.IntersectWith(FreeVertexes.Keys);

        SetActiveValuesInDict(FreeVertexes, openVertexesToDisplay);

        vertexesToTurnOff = openVertexesToDisplay;
        if (vertexesToTurnOff.Count == 0)
            CleanUp();
    }

    public void BuildCity()
    {
        Build = eBuildAction.City;
        
        settlements = GetSettlements();
        
        if (this.settlements.Count == 0)
            CleanUp();

    }
    public List<Vertex> GetSettlements()
    {
        List<Vertex> settlements = new List<Vertex>();

        foreach (Vertex vertex in PlayerBuildings.Values)
        {
            if (vertex.Building == eBuilding.Settlement)
            {
                vertex.settlement.InitScaleUpDown(Consts.SettlementRegularScale, Consts.ScaleSettlement);
                settlements.Add(vertex);
            }
        }
        return settlements;
    }



    public void BuildWall()
    {
        Build = eBuildAction.Wall;
        regularCities = GetCitiesWithoutWalls();
        if (regularCities.Count == 0)
            CleanUp();
    }

    public List<Vertex> GetCitiesWithoutWalls()
    {
        List<Vertex> cities = new List<Vertex>();

        foreach (Vertex vertex in PlayerBuildings.Values)
        {
            if (vertex.Building == eBuilding.City && !vertex.City.HasWall)
            {
                cities.Add(vertex);
                vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
            }
        }

        return cities;
    }

    public void BuildKnight()
    {
        Build = eBuildAction.Knight;
        GetKnightBuildSpots();
        if (knightsToTurnOff.Count == 0 && vertexesToTurnOff.Count == 0)
            CleanUp();

    }

    public void GetKnightBuildSpots()
    {
        HashSet<int> freeVertexesToDisplay = new HashSet<int>();
        HashSet<int> knightsVertexesToDisplay = new HashSet<int>();

        foreach (Edge edge in PlayerRoads.Values)
        {
            foreach (int vertex in edge.Vertexes)
            {
                if (FreeVertexes.ContainsKey(vertex))
                {
                    freeVertexesToDisplay.Add(vertex);
                }
                else if (FreeKnights.ContainsKey(vertex))
                {
                    knightsVertexesToDisplay.Add(vertex);
                }
            }
        }

        SetActiveValuesInDict(FreeVertexes, freeVertexesToDisplay);
        SetActiveValuesInDict(FreeKnights, knightsVertexesToDisplay);

        vertexesToTurnOff = freeVertexesToDisplay;
        knightsToTurnOff = knightsVertexesToDisplay;
    }

    public void UpgradeKnight()
    {
        Build = eBuildAction.UpgradeKnight;
        
        knightsToTurnOff = GetUpgradeableKnights();
        if (knightsToTurnOff.Count == 0)
            CleanUp();
    }


    /// <summary>
    /// Joins all the upgradeable knights into one hashset.
    /// </summary>
    /// <param name="vertexID">If this was called from smith this param will indicate the ID of the first knight that was upgraded
    /// so it wont activate him again</param>
    /// <returns></returns>
    public HashSet<int> GetUpgradeableKnights(int vertexID = -1)
    {
        HashSet<int> optionalKnights = new HashSet<int>();
        foreach (KeyValuePair<int, Vertex> entry in PlayerKnights)
        {
            Knight knight = entry.Value.knight;
            if (entry.Key == vertexID) continue;

            if (entry.Value.Building == eBuilding.Knight && buildingAmounts[eBuilding.Knight2] != 0)
            {
                knight.InitScaleUpDown(knight.transform.localScale, Consts.ScaleKnight);
                optionalKnights.Add(entry.Key);
            }
            else if ((entry.Value.Building == eBuilding.Knight2 && CanBuildKnightsLvl3 && buildingAmounts[eBuilding.Knight3]!=0))
            {
                knight.InitScaleUpDown(knight.transform.localScale, Consts.ScaleKnight);
                optionalKnights.Add(entry.Key);
            }
        }
        return optionalKnights;
    }

    public void ActivateKnight()
    {
        Build = eBuildAction.ActivateKnight;

        HashSet<int> optionalKnights = new HashSet<int>();
        foreach (KeyValuePair<int, Vertex> entry in PlayerKnights)
        {
            Knight knight = entry.Value.knight;
            if (!knight.Activated)
            {
                Vector3 s1 = knight.Level > 2 ? Consts.ScaleKnight3 : Consts.ScaleKnight;

                knight.InitScaleUpDown(knight.transform.localScale, s1);
                optionalKnights.Add(entry.Key);
            }
        }
        knightsToTurnOff = optionalKnights;
        if (knightsToTurnOff.Count == 0)
            CleanUp();
    }


    public void CleanUp()
    {
        if (KnightAction == eKnightActions.TakeAction)
        {
            TurnOffKnightOptions();
            cardManager.robber.StopScaling();
            knightToMove.SetCollider(true);
            KnightAction = eKnightActions.None;
            cancelButton.gameObject.SetActive(false);
            turnManager.SetControl(true);
        }

        if (Build == eBuildAction.None) return;
        int buildOption = (int)Build - 1;
        buttons[buildOption].enabled = true;
        Invoke(buttonCleanUps[buildOption], 0);
        Build = eBuildAction.None;
        cancelButton.gameObject.SetActive(false);
    }

    public void RoadCleanUp()
    {
        TurnOffValuesFromDict(FreeEdges, edgesToTurnOff);
    }

    public void SettlementCleanUp()
    {
        TurnOffValuesFromDict(FreeVertexes, vertexesToTurnOff);
    }

    public void CityCleanUp()
    {
        StopScalingBuildings(settlements, eBuilding.Settlement);
    }
    public void WallCleanUp()
    {
        StopScalingBuildings(regularCities, eBuilding.City);
    }
    public void KnightCleanUp()
    {
        TurnOffValuesFromDict(FreeVertexes, vertexesToTurnOff);
        vertexesToTurnOff.Clear();
        TurnOffValuesFromDict(FreeKnights, knightsToTurnOff);
        knightsToTurnOff.Clear();
    }
    public void UpgradeKnightCleanUp()
    {
        if (knightsToTurnOff == null) return;
        StopScalingKnights();
        knightsToTurnOff.Clear();
    }
    public void ActivateKnightCleanUp()
    {
        if (knightsToTurnOff == null) return;
        StopScalingKnights();
        knightsToTurnOff.Clear();
    }


    #endregion

    #region Barbarians Attack And City Improvements


    public int CountUnimprovedCities()
    {
        regularCities.Clear();
        foreach (Vertex vertex in PlayerBuildings.Values)
        {
            if (vertex.Building == eBuilding.City)
            {
                if (!vertex.City.Improved)
                    regularCities.Add(vertex);
            }
        }
        return regularCities.Count;
    }
    #endregion

    #region Make Knights Useable
    public void SetUsableKnights()
    {
        foreach (Vertex vertex in PlayerKnights.Values)
        {
            if (vertex.knight.Activated && !vertex.knight.Useable)
            {
                vertex.knight.Useable = true;
            }
        }
    }
    #endregion

    #region LongestRoad

    private List<HashSet<int>> DivideRoadsToSets()
    {
        List<HashSet<int>> roadSets = new List<HashSet<int>>();
        HashSet<int> seenEdges = new HashSet<int>();
        foreach (Edge edge in PlayerRoads.Values)
        {
            if (seenEdges.Contains(edge.Id)) continue;
            HashSet<int> newRoadSet = new HashSet<int>();
            newRoadSet.Add(edge.Id);
            seenEdges.Add(edge.Id);
            BranchFromEdge(edge, seenEdges, newRoadSet);
            roadSets.Add(newRoadSet);

        }
        return roadSets;
    }

    private void BranchFromEdge(Edge edge, HashSet<int> seenEdges, HashSet<int> newRoadSet)
    {
        foreach(int vertexID in edge.Vertexes)
        {
            if (RivalsBuildingVertexes.ContainsKey(vertexID) || RivalsKnights.ContainsKey(vertexID))
                continue;
            
            foreach(int edgeID in GetVertex(vertexID).Edges)
            {
                if(PlayerRoads.ContainsKey(edgeID) && !seenEdges.Contains(edgeID))
                {
                    newRoadSet.Add(edgeID);
                    seenEdges.Add(edgeID);
                    BranchFromEdge(PlayerRoads[edgeID], seenEdges, newRoadSet);
                }
            }
        }
    }

    private HashSet<int> GetOneWayRoads(HashSet<int> roadSet)
    {
        HashSet<int> oneWayRoads = new HashSet<int>();
        foreach(int edgeID in roadSet)
        {
            bool foundOneSide = false;
            bool foundTwoSide = false;
            foreach(int vertexID in PlayerRoads[edgeID].Vertexes)
            {
                if (RivalsBuildingVertexes.ContainsKey(vertexID) || RivalsKnights.ContainsKey(vertexID))
                {
                    oneWayRoads.Add(edgeID);
                    break;
                }
                foreach(int vertexEdgeID in GetVertex(vertexID).Edges)
                {
                    if (vertexEdgeID == edgeID) continue;

                    if(PlayerRoads.ContainsKey(vertexEdgeID))
                    {
                        if (foundOneSide)
                        {
                            foundTwoSide = true;
                            break;
                        }
                        else
                        {
                            foundOneSide = true;
                            break;
                        }
                    }
                }
            }

            if(!foundOneSide || (foundOneSide && !foundTwoSide))
                oneWayRoads.Add(edgeID);
        }
        return oneWayRoads;
    }

    public void CalcLongestRoad()
    {
        int longest = 0;
        List<HashSet<int>> roadSets = DivideRoadsToSets();
        foreach(HashSet<int> roadSet in roadSets)
        {
            HashSet<int> oneWayRoads = GetOneWayRoads(roadSet);
            int curr = oneWayRoads.Count == 0 ? CalcLongestPath(roadSet) : CalcLongestPath(oneWayRoads);
            if (curr > longest)
                longest = curr;
        }
        playerSetup.playerPanel.photonView.RPC("SetLongestRoadText", RpcTarget.AllBufferedViaServer, longest.ToString());
        Utils.RaiseEventForMaster(RaiseEventsCode.SetLongestRoad, new object[] { longest });
    }

    private int CalcLongestPath(HashSet<int> roads)
    {
        int longest = 0;
        foreach (int edgeID in roads)
        {
            HashSet<int> seenEdges = new HashSet<int>() { edgeID };
            int curr = CalcLongestPathHelper(edgeID, ref seenEdges);
            if (curr > longest)
                longest = curr;
        }
        return longest;
    }

    private int CalcLongestPathHelper(int edgeID, ref HashSet<int> seenEdges, int cameFromVertex =-1)
    {
        int longest = 1;
        foreach (int vertexID in PlayerRoads[edgeID].Vertexes)
        {
            if (cameFromVertex != -1 && cameFromVertex == vertexID) continue;
            if (!RivalsBuildingVertexes.ContainsKey(vertexID) && !RivalsKnights.ContainsKey(vertexID))
            {
                foreach (int vertexEdgeID in GetVertex(vertexID).Edges)
                {
                    if (seenEdges.Contains(vertexEdgeID) || !PlayerRoads.ContainsKey(vertexEdgeID)) continue;

                    seenEdges.Add(vertexEdgeID);
                    int curr = 1 + CalcLongestPathHelper(vertexEdgeID, ref seenEdges, vertexID);
                    if (curr > longest)
                        longest = curr;
                }
            }
        }
        return longest;
    }


    public Vertex GetVertex(int vertexID)
    {
        if (FreeVertexes.ContainsKey(vertexID))
            return FreeVertexes[vertexID];
        else if (FreeKnights.ContainsKey(vertexID))
            return FreeKnights[vertexID];
        else if (PlayerBuildings.ContainsKey(vertexID))
            return PlayerBuildings[vertexID];
        else
            return PlayerKnights[vertexID];
    }




    private void UpdateLongestRoad(Vertex knight)
    {
        HashSet<int> playersNear = new HashSet<int>();
        foreach (int edgeID in knight.Edges)
        {
            if (RivalRoads.ContainsKey(edgeID))
            {
                playersNear.Add(RivalRoads[edgeID].owner);
            }
        }
        Utils.RaiseEventForGroup(RaiseEventsCode.CheckRoads, playersNear.ToArray());
    }

    #endregion


    #region Development Cards Function

    // Deserter Related Functions
    private void ScaleAllKnight()
    {
        knightsToTurnOff = new HashSet<int>(PlayerKnights.Keys);
        foreach (Vertex vertex in PlayerKnights.Values)
        {
            Knight knight = vertex.knight;
            Vector3 s1 = knight.Level > 2 ? Consts.ScaleKnight3 : Consts.ScaleKnight;
            knight.InitScaleUpDown(knight.transform.localScale, s1);
        }
    }

    private void BuildDesertedKnight(int lvl, bool active)
    {
        Deserter deserter = playerSetup.currentCard as Deserter;
        deserter.SetKnightData(lvl, active);
        int maxLvl = deserter.GetMaxBuildableKnight();
        if (maxLvl == 0)
        {
            deserter.CleanUp();
        }
        else
        {
            GetKnightBuildSpots();
            if (knightsToTurnOff.Count == 0 && vertexesToTurnOff.Count == 0)
                deserter.CleanUp();
        }
    }


    // Diplomat Related Functions
    public HashSet<Edge> GetInteractableRoads()
    {
        HashSet<Edge> interactableRoads = new HashSet<Edge>();
        GetMyInteractableRoads(interactableRoads);
        GetRivalsInteractableRoads(interactableRoads);
        return interactableRoads;
    }

    private void GetMyInteractableRoads(HashSet<Edge> interactableRoads)
    {
        foreach (Edge edge in PlayerRoads.Values)
        {
            foreach (int vertexID in edge.Vertexes)
            {
                if (PlayerBuildings.ContainsKey(vertexID) || PlayerKnights.ContainsKey(vertexID))
                {
                    continue;
                }
                else if (RivalsBuildingVertexes.ContainsKey(vertexID) || RivalsKnights.ContainsKey(vertexID))
                {
                    interactableRoads.Add(edge);
                    edge.road.InitScaleUpDown(Consts.RoadRegularScale, Consts.ScaleRoad);
                    break;
                }
                else
                {
                    Vertex vertex;
                    if (FreeVertexes.ContainsKey(vertexID))
                    {
                        vertex = FreeVertexes[vertexID];
                    }
                    else
                    {
                        vertex = FreeKnights[vertexID];
                    }
                    bool canAdd = true;
                    foreach (int edgeID in vertex.Edges)
                    {
                        if (edgeID == edge.Id) continue;
                        if (PlayerRoads.ContainsKey(edgeID))
                        {
                            canAdd = false;
                            break;
                        }
                    }
                    if (canAdd)
                    {
                        interactableRoads.Add(edge);
                        edge.road.InitScaleUpDown(Consts.RoadRegularScale, Consts.ScaleRoad);
                        break;
                    }
                }
            }
        }
    }


    private void GetRivalsInteractableRoads(HashSet<Edge> interactableRoads)
    {
        foreach (Edge edge in RivalRoads.Values)
        {
            int owner = edge.owner;
            foreach (int vertexID in edge.Vertexes)
            {
                if (PlayerBuildings.ContainsKey(vertexID) || PlayerKnights.ContainsKey(vertexID))
                {
                    interactableRoads.Add(edge);
                    edge.road.InitScaleUpDown(Consts.RoadRegularScale, Consts.ScaleRoad);
                    break;
                }
                else if (RivalsBuildingVertexes.ContainsKey(vertexID) || RivalsKnights.ContainsKey(vertexID))
                {
                    Vertex vertex;
                    if (RivalsBuildingVertexes.ContainsKey(vertexID))
                        vertex = RivalsBuildingVertexes[vertexID];
                    else
                        vertex = RivalsKnights[vertexID];
                    
                    if (vertex.owner == owner) continue;
                    
                    interactableRoads.Add(edge);
                    edge.road.InitScaleUpDown(Consts.RoadRegularScale, Consts.ScaleRoad);
                    break;
                }
                else
                {
                    Vertex vertex;
                    if (FreeVertexes.ContainsKey(vertexID))
                    {
                        vertex = FreeVertexes[vertexID];
                    }
                    else
                    {
                        vertex = FreeKnights[vertexID];
                    }
                    bool canAdd = true;
                    foreach (int edgeID in vertex.Edges)
                    {
                        if (edgeID == edge.Id) continue;
                        if (RivalRoads.ContainsKey(edgeID))
                        {
                            if(RivalRoads[edgeID].owner == owner)
                            {
                                canAdd = false;
                                break;
                            }
                        }
                    }
                    if (canAdd)
                    {
                        interactableRoads.Add(edge);
                        edge.road.InitScaleUpDown(Consts.RoadRegularScale, Consts.ScaleRoad);
                        break;
                    }
                }
            }
        }
    }


    private void RemoveRoad(int edgeID, int sender)
    {

        if (!photonView.IsMine) return;
        Edge edgeToRemove;

        if (PhotonNetwork.LocalPlayer.ActorNumber == sender)
        {
            edgeToRemove = RemoveIfExist(PlayerRoads, edgeID);
            FreeEdges.Add(edgeID, edgeToRemove);
            if(PhotonNetwork.LocalPlayer.ActorNumber == GameManager.instance.CurrentPlayer)
            {
                CalcLongestRoad();
                edgesToTurnOff = CalcRoads();
            }
            else
            {
                Utils.RaiseEventForPlayer(RaiseEventsCode.FinishDiplomat, GameManager.instance.CurrentPlayer);    
            }
        }
        else
        {
            edgeToRemove = RemoveIfExist(RivalRoads, edgeID);
            FreeEdges.Add(edgeID, edgeToRemove);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishDiplomat, GameManager.instance.CurrentPlayer);
        }


    }
    public void SwitchProbs(int tileID1, int tileID2)
    {
        Tile tile1 = PhotonView.Find(tileID1).GetComponent<Tile>();
        Tile tile2 = PhotonView.Find(tileID2).GetComponent<Tile>();

        string tile1Numebr = tile1.probability.tNumber.text;
        string tile2Numebr = tile2.probability.tNumber.text;

        probNumberTile[tile1Numebr].Remove(tile1);
        probNumberTile[tile1Numebr].Add(tile2);

        probNumberTile[tile2Numebr].Remove(tile2);
        probNumberTile[tile2Numebr].Add(tile1);


        tile1.probability.photonView.RPC("SetProb", RpcTarget.AllBufferedViaServer, tile2Numebr);
        tile2.probability.photonView.RPC("SetProb", RpcTarget.AllBufferedViaServer, tile1Numebr);

    }

    #endregion


    #region Private Functions

    private void SetActiveValuesInDict<T>(Dictionary<int, T> d, IEnumerable<int> keysToActivate) where T : Component
    {
        foreach (int key in keysToActivate)
        {
            d[key].gameObject.SetActive(true);
        }
    }


    private void AddIfAbsent<T>(Dictionary<int, T> d, int key, T go) where T : Component
    {
        if (!d.ContainsKey(key))
        {
            d.Add(key, go);
        }
    }

    private T RemoveIfExist<T>(Dictionary<int, T> d, int key) where T : Component
    {
        if (d.ContainsKey(key))
        {
            T go = d[key];
            d.Remove(key);
            return go;
        }
        return null;
    }

    #endregion
}
