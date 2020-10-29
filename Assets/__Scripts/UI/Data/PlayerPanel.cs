using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class PlayerPanel : MonoBehaviourPun
{
    public GameObject[] buildings;
    
    [SerializeField]
    private Image image = null;

    [SerializeField]
    private Text playerName = null;

    [SerializeField]
    private Text victoryPoints = null;

    [SerializeField]
    private Text[] buildingTexts = null;

    [SerializeField]
    private Text numOfCards = null;
    
    [SerializeField]
    private Text longestRoad = null;

    [SerializeField]
    private Text[] commoditysTexts = null;
    
    [SerializeField]
    private Text statusText = null;
    



    private const string PickAResource = "Picking a Resource";
    private const string ThrowingCards = "Throwing Cards";
    private const string LosingCity = "Losing City";

    private Color32 goodColor = new Color32(54, 166, 0, 255);
    private Color32 badColor = new Color32(205,0,0,255);
    private Color32 defaultColor = new Color(1,1,1,1);

    private int points = 0;

    private const string pointsString = "Points {0}";
    private const string level = "Level: {0}";
    private const string numOfCardsInHand = "#Cards: {0}";

    private string playerColor; 

    void Awake()
    {
        object[] data = photonView.InstantiationData;
        playerColor = (string)data[0];
        foreach (GameObject go in buildings)
            SetColor(Utils.Name_To_Color(playerColor), go.GetComponent<MeshRenderer>(), go);
        playerName.text = photonView.Owner.NickName;
    }


    void SetColor(Color color, MeshRenderer mRend, GameObject go)
    {
        foreach (Renderer mr in go.GetComponentsInChildren<Renderer>())
        {
            mr.material.color = new Color(color.r, color.g, color.b, mr.material.color.a);
        }
        if(mRend != null)
            mRend.material.color = color;
    }

    [PunRPC]
    public void SetParent()
    {
        transform.SetParent(PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>().playersDataPanel.transform, false);
    }

    [PunRPC]
    public void MakeActive(bool flag)
    {
        statusText.gameObject.SetActive(false);
        float a = flag? 1 : 100f/255f;
        image.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, a);
    }

    [PunRPC]
    public void MakeActive(string colorKey)
    {
        switch (colorKey)
        {
            case Consts.Good:
                statusText.gameObject.SetActive(true);
                statusText.text = PickAResource;
                image.color = goodColor;
                break;
            case Consts.Bad:
                statusText.gameObject.SetActive(true);
                statusText.text = ThrowingCards;
                image.color = badColor;
                break;
            case Consts.LoseCity:
                statusText.gameObject.SetActive(true);
                statusText.text = LosingCity;
                image.color = badColor;
                break;
        }
    }


    [PunRPC]
    public void AddVictoryPoints(int vp)
    {
        points += vp;
        victoryPoints.text = string.Format(pointsString, points);

        if (points >= 13)
        {
            if (photonView.IsMine)
                Utils.RaiseEventForAll(RaiseEventsCode.GameOver, new object[] { PhotonNetwork.LocalPlayer.NickName, playerColor } );
        }
    }

    [PunRPC]
    public void SetBuildingText(int type, string amount)
    {
        buildingTexts[type].text = amount;
    }

    [PunRPC]
    public void SetCommodityText(int type, string amount)
    {
        commoditysTexts[type].text = string.Format(level, amount);
    }

    [PunRPC]
    public void SetNumOfCardsText(string amount)
    {
        numOfCards.text = string.Format(numOfCardsInHand, amount);
    }

    [PunRPC]
    public void SetLongestRoadText(string amount)
    {
        longestRoad.text = amount;
    }
}
