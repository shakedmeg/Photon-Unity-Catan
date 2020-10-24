using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerSetup : MonoBehaviourPun
{
    public static GameObject LocalPlayerInstance;

    public Canvas canvas;

    public GameObject playersDataPanel;

    public GameObject playerPanelPrefab;

    public PlayerPanel playerPanel;

    void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            canvas.gameObject.SetActive(true);
            canvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

            object color;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out color);
            string playerColor = (string)color;
            playerPanel = PhotonNetwork.Instantiate(playerPanelPrefab.name, playerPanelPrefab.transform.position, playerPanelPrefab.transform.rotation, 0, new object[] { playerColor }).GetComponent<PlayerPanel>();
            playerPanel.photonView.RPC("SetParent", RpcTarget.All);
        }

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);
    }



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
            case (byte)RaiseEventsCode.PreSetupSettlement:
            case (byte)RaiseEventsCode.PreSetupCity:
            case (byte)RaiseEventsCode.StartTurn:
                if (!photonView.IsMine) return;
                playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
                break;
            case (byte)RaiseEventsCode.SetPlayerPanel:
                data = (object[])photonEvent.CustomData;
                if (!photonView.IsMine) return;
                playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, (bool)data[0]);
                break;
        }
    }
}
