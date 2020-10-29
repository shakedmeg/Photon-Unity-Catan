using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;


public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public static GameObject LocalPlayerInstance;

    public Canvas canvas;

    public GameObject playersDataPanel;

    public GameObject playerPanelPrefab;

    public PlayerPanel playerPanel;

    //public GameObject gameOverPanel;

    //public Text winnerNameText;

    //public Button EndGameButton;


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
        // DontDestroyOnLoad(gameObject);
    }



    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
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
            case (byte)RaiseEventsCode.AddPoints:
                if (!photonView.IsMine) return;
                data = (object[])photonEvent.CustomData;
                playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, (int)data[0]);
                break;
            case (byte)RaiseEventsCode.GameOver:
                if (!photonView.IsMine) return;
                canvas.gameObject.SetActive(false);
                break;
        }
    }
}
