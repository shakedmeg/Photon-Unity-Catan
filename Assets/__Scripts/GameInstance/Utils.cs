using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
public class Utils : MonoBehaviour
{
    public static Color Name_To_Color(string name)
    {
        switch (name)
        {
            case Consts.YELLOW:
                return Color.yellow;
            case Consts.RED:
                return Color.red;
            case Consts.BLUE:
                return Color.blue;
            case Consts.WHITE:
                return Color.white;
            case Consts.BLACK:
                return Color.black;
        }
        return Color.clear;
    }

    public static int Name_To_Index(string name)
    {
        switch (name)
        {
            case Consts.YELLOW:
                return 0;
            case Consts.RED:
                return 1;
            case Consts.BLUE:
                return 2;
            case Consts.WHITE:
                return 3;
            case Consts.BLACK:
                return 4;
        }
        return -1;
    }

    public static void Set_Color_And_Name(int value, ref string colorName, ref Color color)
    {
    switch (value)
        {
            case 0:
                colorName = Consts.YELLOW;
                color = Color.yellow ;
                return;
            case 1:
                colorName = Consts.RED;
                color = Color.red ;
                return;
            case 2:
                colorName = Consts.BLUE;
                color = Color.blue ;
                return;
            case 3:
                colorName = Consts.WHITE;
                color = Color.white ;
                return;
            case 4:
                colorName = Consts.BLACK;
                color = Color.black ;
                return;
        }
        
    }

    public static string ERsourceToString(eResources resource)
    {
        switch (resource)
        {
            case eResources.Brick:
                return Consts.Brick;
            case eResources.Wheat:
                return Consts.Wheat;
            case eResources.Wood:
                return Consts.Wood;
            case eResources.Ore:
                return Consts.Ore;
            case eResources.Wool:
                return Consts.Wool;

            default:
                return "";


        }
    }


    public static string ECommodityToString(eCommodity commodity)
    {
        switch (commodity)
        {
            case eCommodity.Paper:
                return Consts.Paper;
            case eCommodity.Coin:
                return Consts.Coin;
            case eCommodity.Silk:
                return Consts.Silk;
            default:
                return "";


        }
    }


    public static int GetCardType(Card card)
    {
        if (card is ResourceCard)
            return (int)((ResourceCard)card).resource;
        else
            return (int)((CommodityCard)card).commodity;
    }



    public static void RaiseEventForAll(RaiseEventsCode code, object[] data = null)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte)code, data, raiseEventOptions, sendOptions);
    }
    

    public static void RaiseEventForGroup(RaiseEventsCode code, int[] actors, object[] data = null)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            TargetActors = actors,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte)code, data, raiseEventOptions, sendOptions);
    }


    public static void RaiseEventForPlayer(RaiseEventsCode code, int actor, object[] data = null)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            TargetActors = new int[] { actor },
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte)code, data, raiseEventOptions, sendOptions);
    }



    public static bool IsFull<T>(List<T> list) where T:Component
    {
        return list.Capacity == list.Count;
    }

    public static bool IsResourceCard(Card card)
    {
        return card is ResourceCard;
    }

}
