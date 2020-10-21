﻿using System.Collections;
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


    private BuildManager buildManager;
    private CardManager cardManager;


    #endregion


    #region Properties


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
        Vector3 p1;
        Vector3 p0;
        switch (buildManager.Build)
        {
            case eBuilding.Road:
                p1 = new Vector3(transform.position.x, Consts.DROP_HIGHET, transform.position.z);
                p0 = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                GameObject roadGameObject = PhotonNetwork.Instantiate(roadPrefab.name, p1, angle, 0, data);
                Road road = roadGameObject.GetComponent<Road>();
                road.InitDrop(p1, p0);
                UpdateEdges();
                
                if (GameManager.instance.state == GameState.SetupSettlement || GameManager.instance.state == GameState.SetupCity)
                {
                    buildManager.Build = eBuilding.None;
                    buildManager.RoadCleanUp();
                    Utils.RaiseEventForAll(RaiseEventsCode.PassTurn);
                    //buildManager.PassSetupTurn();
                }
                else
                {
                    cardManager.Pay();
                    buildManager.CleanUp();
                }
                break;
        }
    }


    #endregion



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
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
    }

    #endregion

}