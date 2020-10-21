using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class PlayerListEntryInitializer : MonoBehaviour
{
    [Header("UI Refernces")]
    public Text PlayerNameText;
    public ColorDropdown PlayerColorDropdown;
    public Button PlayerReadyButton;
    public Image PlayerReadyImage;

    private bool isPlayerReady = false;


    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.OpResponseReceived += NetworkingClientOnOpResponseReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.OpResponseReceived -= NetworkingClientOnOpResponseReceived;
    }

    private void NetworkingClientOnOpResponseReceived(OperationResponse opResponse)
    {
        if (opResponse.OperationCode == OperationCode.SetProperties &&
            opResponse.ReturnCode == ErrorCode.InvalidOperation)
        {
            Debug.Log(opResponse.DebugMessage);
            // CAS failure
            // we will assign color again
        }
    }


    public void Initialize(int playerID, string playerName)
    {
        PlayerNameText.text = playerName;
        PlayerColorDropdown.AddOptions(Consts.COLORS_DROPDOWN);

        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID)
        {
            PlayerReadyButton.gameObject.SetActive(false);
            PlayerColorDropdown.interactable = false;
        }
        else
        {
            // I am the local player
            SetColor();

            PlayerColorDropdown.onValueChanged.AddListener((int x) =>
            {
                DropDownSetColor();
            });




            ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable() { { Consts.PLAYER_READY, isPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);

            PlayerReadyButton.onClick.AddListener(() =>
            {
                isPlayerReady = !isPlayerReady;
                SetPlayerReady(isPlayerReady);

                ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable() { { Consts.PLAYER_READY, isPlayerReady } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);
            });




        }
    }

    public void SetPlayerReady(bool playerReady)
    {
        PlayerReadyImage.enabled = playerReady;

        if (playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready!";
        }
        else
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready?";
        }
    }



    /// <summary>
    /// Whenever a player joins a room the player will be assigned with the first color from the room properties.
    /// </summary>
    private void SetColor(){
        // Get room colors
        object data;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Consts.ROOM_COLORS, out data);
        List<string> allowedColors = new List<string>((string[])data);
        
        // Parse color
        string colorName = allowedColors[0];
        Color color = Consts.COLOR_NAME_TO_COLOR[colorName];
        
        // Remove color from room colors
        allowedColors.RemoveAt(0);
        object owner;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Consts.COLORS_OWNER,out owner);
        int currentOwner = (int)owner;
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.ROOM_COLORS, allowedColors.ToArray() }, { Consts.COLORS_OWNER, PhotonNetwork.LocalPlayer.ActorNumber } };
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.COLORS_OWNER, currentOwner } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties, expectedCustomRoomProperties);


        // assign color to the player custom properties and dropdown value
        int value = -1;
        switch (colorName)
        {
            case Consts.YELLOW:
                value = 0;
                break;
            case Consts.RED:
                value = 1;
                break;
            case Consts.BLUE:
                value = 2;
                break;
            case Consts.WHITE:
                value = 3;
                break;
            case Consts.BLACK:
                value = 4;
                break;
        }

        ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable() { { Consts.PLAYER_COLOR, colorName } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);

        PlayerColorDropdown.value = value;
    }



    public void SetColorForOthers(string color)
    {
        PlayerColorDropdown.transform.GetChild(0).GetComponent<Text>().text = color;
        PlayerColorDropdown.transform.GetChild(3).GetComponent<Image>().color = Utils.Name_To_Color(color);
    }




    /// <summary>
    /// Whenever a Player will select a color it checks the room properties to see if the color is assignable.
    /// if so the player will add its color to the room color properties and will change his color.
    /// if not the DropDown will change its allowChange attribute to false so that once it assigns the value,
    /// it wont go into and endless loop.
    /// </summary>
    public void DropDownSetColor()
    {
        if (!PlayerColorDropdown.allowChange)
        {
            PlayerColorDropdown.allowChange = true;
            return;
        }

        string colorName = "";
        Color color = Color.clear;
        Image image = PlayerColorDropdown.transform.GetChild(3).GetComponent<Image>();
        Utils.Set_Color_And_Name(PlayerColorDropdown.value, ref colorName, ref color);
        
        //switch (PlayerColorDropdown.value)
        //{
        //    case 0:
        //        colorName = Consts.YELLOW;
        //        color = Color.yellow ;
        //        break;
        //    case 1:
        //        colorName = Consts.RED;
        //        color = Color.red ;
        //        break;
        //    case 2:
        //        colorName = Consts.BLUE;
        //        color = Color.blue ;
        //        break;
        //    case 3:
        //        colorName = Consts.WHITE;
        //        color = Color.white ;
        //        break;
        //    case 4:
        //        colorName = Consts.BLACK;
        //        color = Color.black ;
        //        break;
        //}


        object playerData;
        PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Consts.PLAYER_COLOR, out playerData);

        object data;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Consts.ROOM_COLORS, out data);
        List<string> allowedColors = new List<string>((string[])data);
        if (allowedColors.Contains(colorName))
        {
            allowedColors.Remove(colorName);
            allowedColors.Insert(0, (string)playerData);
            object owner;
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Consts.COLORS_OWNER, out owner);
            int currentOwner = (int)owner;
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.ROOM_COLORS, allowedColors.ToArray() }, { Consts.COLORS_OWNER, PhotonNetwork.LocalPlayer.ActorNumber } };
            ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.COLORS_OWNER, currentOwner } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties, expectedCustomRoomProperties);

            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() { { Consts.PLAYER_COLOR, colorName } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
            image.color = color;
        }
        else
        {
            PlayerColorDropdown.allowChange = false;
            //PlayerColorDropdown.transform.GetChild(0).GetComponent<Text>().text = (string)playerData;
            PlayerColorDropdown.value = Utils.Name_To_Index((string)playerData);
        }


    }





    
}
