using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class BuildManager : MonoBehaviourPun
{

    public string playerColor;

    public Button[] buttons;
    public GameObject cancelButton;


    #region Private Fields


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



    public HashSet<int> edgesToTurnOff;

    public HashSet<int> vertexesToTurnOff;

    public HashSet<int> knightsToTurnOff;

    public HashSet<int> knightsOptionsToTurnOff;




    #region Cached Data Fields


    public List<Tile> tiles = new List<Tile>();

    public Dictionary<int, Vertex> FreeVertexes { get; private set; } = new Dictionary<int, Vertex>();
    public Dictionary<int, Edge> FreeEdges { get; private set; } = new Dictionary<int, Edge>();
    public Dictionary<int, Vertex> FreeKnights { get; private set; } = new Dictionary<int, Vertex>();

    public Dictionary<int, Vertex> PlayerBuildings { get; private set; } = new Dictionary<int, Vertex>();
    public int cityCount = 0;

    public Dictionary<int, Edge> PlayerRoads { get; private set; } = new Dictionary<int, Edge>();
    public Dictionary<int, Vertex> PlayerKnights { get; private set; } = new Dictionary<int, Vertex>();


    public Dictionary<int, Vertex> RivalsBuildingVertexes { get; private set; } = new Dictionary<int, Vertex>();
    public Dictionary<int, Vertex> RivalsKnights { get; private set; } = new Dictionary<int, Vertex>();

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


    public List<Vertex> regularCities = new List<Vertex>();

    #endregion


    #region Properties

    public bool CanBuildKnightsLvl3 { get; set; } = false;
    public eBuilding Build { get; set; }
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
                Build = eBuilding.Settlement;
                SetDictActive(FreeVertexes, true);
                break;
            case (byte)RaiseEventsCode.PreSetupCity:
                Build = eBuilding.City;
                SetDictActive(FreeVertexes, true);
                break;
            case (byte)RaiseEventsCode.MatchTilesToDice:
                data = (object[])photonEvent.CustomData;
                GetTilesWithDiceNumber((string)data[0]);
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
                DisplaceKnight((int) data[0]);
                break;
            case (byte)RaiseEventsCode.FinishMoveKnight:
                if (!photonView.IsMine) return;
                turnManager.RegainControl();
                data = (object[])photonEvent.CustomData;
                if (FreeVertexes.ContainsKey((int)data[0])) FreeVertexes[(int)data[0]].MoveKnight();
                if (FreeKnights.ContainsKey((int)data[0])) FreeKnights[(int)data[0]].MoveKnight();
                break;
            case (byte)RaiseEventsCode.CheckIfCanLoseCity:
                if (!photonView.IsMine) return;
                turnManager.barbarians.photonView.RPC("ContinueLostCheck", RpcTarget.MasterClient, CountUnimprovedCities() != 0);
                break;
            case (byte)RaiseEventsCode.LoseCity:
                if (!photonView.IsMine) return;
                Build = eBuilding.Destroy;
                foreach (Vertex vertex in regularCities)
                    vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
                break;
            case (byte)RaiseEventsCode.ImproveCity:
                data = (object[])photonEvent.CustomData;
                Build = eBuilding.ImproveCity;
                ImproveCommodity = (eCommodity)data[0];
                foreach (Vertex vertex in regularCities)
                    vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
                break;
            case (byte)RaiseEventsCode.TakeImproveCity:
                data = (object[])photonEvent.CustomData;
                Build = eBuilding.TakeImprovedCity;
                ImproveCommodity = (eCommodity)data[0];
                foreach (Vertex vertex in regularCities)
                    vertex.City.InitScaleUpDown(Consts.CityRegularScale, Consts.ScaleCity);
                break;
            case (byte)RaiseEventsCode.SetImproveCityID:
                data = (object[])photonEvent.CustomData;
                SetImprovementViewID((eCommodity)data[0], (int)data[1]);
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
        for(int i = 0; i<Consts.NumOfTiles; i++)
        {
            Tile tile = PhotonView.Find(tilesIDs[i]).gameObject.GetComponent<Tile>();
            tiles.Add(tile);
            if (tile.Resource == eResources.Desert) {
                desertFound = true;
                continue;
            }

            key = desertFound? Consts.Probabilitiys[i - 1] : Consts.Probabilitiys[i];
            
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
            PlayerRoads.Add(edgeID, edge);

    }


    void AddKnight(int knightID, int sender)
    {
        if (!photonView.IsMine) return;
        Vertex knight;

        knight = RemoveIfExist(FreeKnights, knightID);
        if (knight == null) knight = RemoveIfExist(FreeVertexes, knightID);

        if (PhotonNetwork.LocalPlayer.ActorNumber == sender)
            PlayerKnights.Add(knightID, knight);
        else
            RivalsKnights.Add(knightID, knight);
    }

    void RemoveKnight(int knightIDToRemove, int sender)
    {
        if (!photonView.IsMine) return;
        Vertex knightToRemove;
        
        if (PhotonNetwork.LocalPlayer.ActorNumber == sender)
            knightToRemove = RemoveIfExist(PlayerKnights, knightIDToRemove);
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
            knightToMove.Vertex.RemoveKnightFromVertex();
            Utils.RaiseEventForAll(RaiseEventsCode.RemoveKnight, new object[] { displacedKnightPervId });
            PhotonNetwork.Destroy(knightToMove.gameObject);
            Utils.RaiseEventForPlayer(RaiseEventsCode.FinishMoveKnight, notifyToPlayer, new object[] { displacedKnightPervId });
        }
        else
        {
            KnightAction = eKnightActions.MoveKnight;
            ShowKnightsActionsOptions(vertexes);
        }
    }

    public void GetTilesWithDiceNumber(string number)
    {
        if (!photonView.IsMine) return;
        foreach (Tile tile in probNumberTile[number])
        {
            if (tile.RobberOn) continue;
            if (tile.Resource != eResources.Desert)
            {
                foreach (int vertex in tile.Vertexes)
                {
                    if (PlayerBuildings.ContainsKey(vertex))
                    {
                        int resource = (int)tile.Resource;
                        cardManager.InitCard(resource);
                        if (PlayerBuildings[vertex].Building == eBuilding.City)
                        {
                            if (tile.Commodity != eCommodity.None)
                            {
                                cardManager.InitCard((int)tile.Commodity);
                            }
                            else
                            {
                                cardManager.InitCard(resource);
                            }
                        }
                    }
                }
            }
        }
    }


    public void SetImprovementViewID(eCommodity commodityType, int viewID)
    {
        improveCityViewID[commodityType] = viewID;
    }

    #endregion






    #region Functions Called From Tiles, Vertexs and Edges


    public void TurnOffValuesFromDict<T>(Dictionary<int, T> d, IEnumerable<int> valuesToTurnOff) where T: Component
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

        Build = eBuilding.Road;
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
    /// <param name="actorID"> Player to receive the cards</param>
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
        cardManager.setCardsColliders(false);

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
                foreach(int optinalVertex in edge.Vertexes)
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
        //this.photonView.RPC("SetKnightAction", RpcTarget.All, eKnightActions.TakeAction);
        foreach (int knightOptionsID in knightOptionsIDs)
        {
            if (FreeVertexes.ContainsKey(knightOptionsID)) FreeVertexes[knightOptionsID].gameObject.SetActive(true);
            if (FreeKnights.ContainsKey(knightOptionsID)) FreeKnights[knightOptionsID].gameObject.SetActive(true);
            if (RivalsKnights.ContainsKey(knightOptionsID))
            {
                Knight rivalKnight = RivalsKnights[knightOptionsID].knight;
                rivalKnight.InitScaleUpDown(rivalKnight.transform.localScale, Consts.ScaleKnight);
                rivalKnight.SetCollider(true);            
            }
        }

        knightsOptionsToTurnOff = knightOptionsIDs;
    }


    public void StopScalingCities(List<Vertex> cities)
    {
        foreach (Vertex vertex in cities)
            vertex.City.StopScaling();
        cities.Clear();
    }

    public void StopScalingKnights(bool turnOffCollider)
    {
        foreach (int knightID in knightsToTurnOff)
        {
            Knight knight = PlayerKnights[knightID].knight;
            if (turnOffCollider)
                if(!knight.Activated)
                    knight.SetCollider(false);
            knight.StopScaling();
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
        if(Build != eBuilding.None)
        {
            int oldBuild = (int)Build - 1;
            buttons[oldBuild].enabled = true;
            Invoke(buttonCleanUps[oldBuild], 0);
        }

        Build = (eBuilding)buildOption+1;
        buttons[buildOption].enabled = false;
        cancelButton.gameObject.SetActive(true);
        Invoke(buttonHandlers[buildOption], 0);


    }


    public void BuildRoad()
    {
        Build = eBuilding.Road;
        HashSet<int> openEdgesToDisplay = new HashSet<int>();

        foreach (Vertex vertex in PlayerBuildings.Values)
        {
            openEdgesToDisplay.UnionWith(vertex.Edges);
        }

        foreach (Edge edge in PlayerRoads.Values)
        {
            int[] vertexNeighbors = edge.Vertexes;
            HashSet<int> neighborEdges = new HashSet<int>(edge.NeighborEdges);
            for (int i = 0; i<vertexNeighbors.Length; i++)
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

        edgesToTurnOff = openEdgesToDisplay;

    }

    public void BuildSettlement()
    {
        Build = eBuilding.Settlement;

        HashSet<int> openVertexesToDisplay = new HashSet<int>();

        foreach(Edge edge in PlayerRoads.Values)
        {
            openVertexesToDisplay.UnionWith(edge.Vertexes);
        }

        openVertexesToDisplay.IntersectWith(FreeVertexes.Keys);

        SetActiveValuesInDict(FreeVertexes, openVertexesToDisplay);

        vertexesToTurnOff = openVertexesToDisplay;
    }

    public void BuildCity()
    {
        Build = eBuilding.City;
        List<int> settlements = new List<int>();

        foreach (KeyValuePair<int, Vertex> entry in PlayerBuildings)
        {
            if (entry.Value.Building == eBuilding.Settlement)
            {
                settlements.Add(entry.Key);
            }
        }

        SetActiveValuesInDict(PlayerBuildings, settlements);

        vertexesToTurnOff = new HashSet<int>(settlements);

    }

    public void BuildWall()
    {
        Build = eBuilding.Wall;
        List<int> cities = new List<int>();

        foreach (KeyValuePair<int, Vertex> entry in PlayerBuildings)
        {
            Vertex vertex = entry.Value;
            if (vertex.Building == eBuilding.City && !vertex.HasWall)
            {
                cities.Add(entry.Key);
            }
        }

        SetActiveValuesInDict(PlayerBuildings, cities);

        vertexesToTurnOff = new HashSet<int>(cities);
    }

    public void BuildKnight()
    {
        Build = eBuilding.Knight;
        HashSet<int> freeVertexesToDisplay = new HashSet<int>();
        HashSet<int> knightsVertexesToDisplay = new HashSet<int>();

        foreach (Edge edge in PlayerRoads.Values)
        {
            foreach (int vertex in edge.Vertexes)
            {
                if (FreeVertexes.ContainsKey(vertex))
                {
                    freeVertexesToDisplay.Add(vertex);
                }else if (FreeKnights.ContainsKey(vertex))
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
        Build = eBuilding.UpgradeKnight;
        HashSet<int> optionalKnights = new HashSet<int>();
        foreach (KeyValuePair<int, Vertex> entry in PlayerKnights)
        {
            Knight knight = entry.Value.knight;
            if (entry.Value.Building == eBuilding.Knight || ( entry.Value.Building == eBuilding.Knight2 && CanBuildKnightsLvl3))
            {

                knight.InitScaleUpDown(knight.transform.localScale, Consts.ScaleKnight);
                knight.SetCollider(true);
                optionalKnights.Add(entry.Key);
            }
            knightsToTurnOff = optionalKnights;
        }
    }
    
    public void ActivateKnight()
    {
        Build = eBuilding.ActivateKnight;

        HashSet<int> optionalKnights = new HashSet<int>();
        foreach(KeyValuePair<int, Vertex> entry in PlayerKnights)
        {
            Knight knight = entry.Value.knight;
            if (!knight.Activated)
            {
                Vector3 s1 = knight.Level > 2? Consts.ScaleKnight3 : Consts.ScaleKnight;

                knight.InitScaleUpDown(knight.transform.localScale, s1);
                knight.SetCollider(true);
                optionalKnights.Add(entry.Key);
            }

            knightsToTurnOff = optionalKnights;
        }
    }


    public void CleanUp()
    {
        int buildOption = (int)Build - 1;
        buttons[buildOption].enabled = true;
        Invoke(buttonCleanUps[buildOption], 0);
        Build = eBuilding.None;
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
        TurnOffValuesFromDict(PlayerBuildings, vertexesToTurnOff);
        vertexesToTurnOff.Clear();
    }
    public void WallCleanUp()
    {
        TurnOffValuesFromDict(PlayerBuildings, vertexesToTurnOff);
        vertexesToTurnOff.Clear();
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
        StopScalingKnights(false);
        //TurnOffValuesFromDict(PlayerKnights, knightsToTurnOff);
        knightsToTurnOff.Clear();
    }
    public void ActivateKnightCleanUp()
    {
        if (knightsToTurnOff == null) return;
        StopScalingKnights(true);
        //TurnOffValuesFromDict(PlayerKnights, knightsToTurnOff);
        knightsToTurnOff.Clear();
    }


    #endregion



    #region Barbarians Attack And City Improvements
   
    
    public int CountUnimprovedCities()
    {
        regularCities.Clear();
        foreach(Vertex vertex in PlayerBuildings.Values)
        {
            if(vertex.Building == eBuilding.City)
            {
                if (!vertex.City.Improved)
                    regularCities.Add(vertex);
            }
        }
        return regularCities.Count;
    }
    #endregion



    #region RPC's

    [PunRPC]
    public void SetKnightAction(eKnightActions action)
    {
        KnightAction = action;
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


    private void AddIfAbsent<T>(Dictionary<int, T> d, int key, T go)where T: Component
    {
        if (!d.ContainsKey(key))
        {
            d.Add(key, go);
        }
    }

    private T RemoveIfExist<T>(Dictionary<int, T> d, int key) where T: Component
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
