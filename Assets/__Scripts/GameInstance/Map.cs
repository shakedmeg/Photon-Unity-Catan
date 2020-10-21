using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;
using System;

public class Map : MonoBehaviourPun
{

    private List<eResources> resources;

    #region Prefabs


    [Header("Set in Inspector")]
    public GameObject anchorPrefab;
    public GameObject tilePrefab;
    public GameObject probabilityPrefab;
    public GameObject dicePrefab;
    public GameObject barbariansPrefab;
    public GameObject openSpotVertexPrefab;
    public GameObject openSpotEdgePrefab;
    public GameObject robberPrefab;
    public TextAsset layoutJSON;


    #endregion


    #region Public Fields


    [Header("Set Dynamically")]
    public Layout layout;
    public Tile[] tiles;


    #endregion



    #region Private fields


    private GameObject tilesAnchorGO;
    private int tilesAnchorID;

    private GameObject freeVertexesAnchorGO;
    private int freeVertexesAnchorID;
    
    
    private GameObject freeEdgesAnchorGO;
    private int freeEdgesAnchorID;

    private GameObject barbariansGO;

    private GameObject diceGameObject;

    private int[] tilesViewIDs;
    private int[] openSpotVertexesViewIDs;
    private int[] openSpotEdgesViewIDs;

    private int robberViewID;
    private int barbariansViewID;

    #endregion


    #region Unity Methods


    void Awake()
    {
        resources = new List<eResources>() { eResources.Brick, eResources.Brick, eResources.Brick,
                                                eResources.Wood, eResources.Wood, eResources.Wood, eResources.Wood,
                                                eResources.Ore, eResources.Ore, eResources.Ore,
                                                eResources.Wool, eResources.Wool, eResources.Wool, eResources.Wool,
                                                eResources.Wheat, eResources.Wheat, eResources.Wheat, eResources.Wheat,
                                                eResources.Desert
            };

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutJSON.text);
        if (!PhotonNetwork.IsMasterClient) return;

        barbariansGO = PhotonNetwork.Instantiate(barbariansPrefab.name, barbariansPrefab.transform.position, barbariansPrefab.transform.rotation);
        PhotonView barbariansView = barbariansGO.GetComponent<PhotonView>();
        barbariansViewID = barbariansView.ViewID;

        diceGameObject = PhotonNetwork.InstantiateRoomObject(dicePrefab.name, dicePrefab.transform.position, dicePrefab.transform.rotation);
        PhotonView diceView = diceGameObject.GetComponent<PhotonView>();


        barbariansView.RPC("Init", RpcTarget.AllBufferedViaServer, diceView.ViewID);
        diceGameObject.GetComponent<PhotonView>().RPC("Init", RpcTarget.AllBufferedViaServer, barbariansViewID);
        
        
        LayoutGame();
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


    #region Init Board

    void LayoutGame()
    {

        InitAnchors();
        Shuffle(ref resources);
        InitTiles();
        InitVertexes();
        InitEdges();
        SendMapViewIDs();
        GameManager.instance.StartGameSetup();
    }

    void InitAnchors()
    {
        if (tilesAnchorGO == null)
        {
            object[] data = new object[] { "_TilesAnchor" };
            tilesAnchorGO = PhotonNetwork.InstantiateRoomObject(anchorPrefab.name, Vector3.zero, Quaternion.identity, 0, data);
            tilesAnchorID = tilesAnchorGO.gameObject.GetComponent<PhotonView>().ViewID;
        }

        if (freeVertexesAnchorGO == null)
        {
            object[] data = new object[] { "_FreeVertexesAnchor" };
            freeVertexesAnchorGO = PhotonNetwork.InstantiateRoomObject(anchorPrefab.name, Vector3.zero, Quaternion.identity, 0, data);
            freeVertexesAnchorID = freeVertexesAnchorGO.gameObject.GetComponent<PhotonView>().ViewID;
        }
        if (freeEdgesAnchorGO == null)
        {
            object[] data = new object[] { "_FreeEdgesAnchor" };
            freeEdgesAnchorGO  = PhotonNetwork.InstantiateRoomObject(anchorPrefab.name, Vector3.zero, Quaternion.identity, 0, data);
            freeEdgesAnchorID = freeEdgesAnchorGO.gameObject.GetComponent<PhotonView>().ViewID;
        }

    }


    private void InitTiles()
    {
        tilesViewIDs = new int[Consts.NumOfTiles];
        GameObject tileGO;
        bool desert_assigned = false;
        for (int i = 0; i < Consts.NumOfTiles; i++)
        {
            object[] tileData = new object[] { resources[i], layout.tiles[i].vertexes.ToArray(), tilesAnchorID };
            object[] probData = new object[2];
            tileGO = PhotonNetwork.InstantiateRoomObject(tilePrefab.name, Vector3.zero, tilePrefab.transform.rotation, 0, tileData);
            PhotonView tilePhotonView = tileGO.GetComponent<PhotonView>();
            tilePhotonView.RPC("SetPlayerManagers", RpcTarget.AllBufferedViaServer);
            int tileViewID = tilePhotonView.ViewID;
            tilesViewIDs[i] = tileViewID;
            tileGO.GetComponent<Tile>().transform.localPosition = layout.tiles[i].pos;
            if (resources[i] == eResources.Desert)
            {
                PhotonView robberPhotonView = PhotonNetwork.InstantiateRoomObject(robberPrefab.name, robberPrefab.transform.position,
                                                                    robberPrefab.transform.rotation, 0, new object[] { tileViewID }).GetComponent<PhotonView>();
                robberPhotonView.RPC("SetPlayerManagers", RpcTarget.AllBufferedViaServer);
                robberViewID = robberPhotonView.ViewID;
                desert_assigned = true;
                continue;
            }

            probData[0] = desert_assigned ? Consts.Probabilitiys[i - 1] : Consts.Probabilitiys[i];
            probData[1] = tileViewID;
            PhotonNetwork.InstantiateRoomObject(probabilityPrefab.name, probabilityPrefab.transform.position, probabilityPrefab.transform.rotation, 0, probData);

            
        }
    }


    private void Shuffle(ref List<eResources> resources)
    {
        // Create a temporary List to hold the new shuffle order
        List<eResources> tResources = new List<eResources>();
        int ndx; // This will hold the index of the card to be moved
        // Repeat as long as there are cards in the original List
        while (resources.Count > 0)
        {
            // Pick the index of a random card
            ndx = UnityEngine.Random.Range(0, resources.Count);
            // Add that card to the temporary List
            tResources.Add(resources[ndx]);
            // And remove that card from the original List
            resources.RemoveAt(ndx);
        }
        // Replace the original List with the temporary List
        resources = tResources;
    }




    public void InitVertexes()
    {
        openSpotVertexesViewIDs = new int[layout.vertexes.Count];
        GameObject openSpotVertexGO;
        PhotonView openSpotVertexView;
        for (int i = 0; i < layout.vertexes.Count; i++)
        {
            object[] data = new object[] { layout.vertexes[i].pos,
                                           layout.vertexes[i].id,
                                           layout.vertexes[i].neighbors.ToArray(),
                                           layout.vertexes[i].edges.ToArray(),
                                           false,
                                           "Open_Spot_Vertex" + i.ToString(),
                                           freeVertexesAnchorID,
                                           layout.vertexes[i].tiles.ToArray(),
                };
            openSpotVertexGO = PhotonNetwork.Instantiate(openSpotVertexPrefab.name, Vector3.zero, Quaternion.identity, 0, data);
            openSpotVertexView = openSpotVertexGO.GetComponent<PhotonView>();
            openSpotVertexView.RPC("SetPlayerManagers", RpcTarget.AllBufferedViaServer);
            openSpotVertexesViewIDs[i] = openSpotVertexView.ViewID;
        }
    }


    public void InitEdges()
    {
        openSpotEdgesViewIDs = new int[layout.edges.Count];
        GameObject openSpotEdgeGO;
        PhotonView openSpotEdgeView;

        for (int i = 0; i < layout.edges.Count; i++)
        {
            object[] data = new object[] { layout.edges[i].pos,
                                            layout.edges[i].id,
                                            layout.edges[i].neighbors.ToArray(),
                                            layout.edges[i].vertexes.ToArray(),
                                            false,
                                            "Open_Spot_Edge" + i.ToString(),
                                            freeEdgesAnchorID,
                                            layout.edges[i].angle
            };
            openSpotEdgeGO = PhotonNetwork.Instantiate(openSpotEdgePrefab.name, Vector3.zero, Quaternion.identity, 0, data);
            openSpotEdgeView = openSpotEdgeGO.GetComponent<PhotonView>();
            openSpotEdgeView.RPC("SetPlayerManagers", RpcTarget.AllBufferedViaServer);

            openSpotEdgesViewIDs[i] = openSpotEdgeView.ViewID;

        }
    }



    #endregion


    #region Send Events


    void SendMapViewIDs()
    {
        object[] data = new object[] { diceGameObject.GetComponent<PhotonView>().ViewID, tilesViewIDs, openSpotVertexesViewIDs, openSpotEdgesViewIDs, robberViewID, barbariansViewID };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.SendMapData, data, raiseEventOptions, sendOptions);

    }


    #endregion

    #region Raise Events Handlers



    void OnEvent(EventData photonEvent)
    {
    }

    #endregion
}
