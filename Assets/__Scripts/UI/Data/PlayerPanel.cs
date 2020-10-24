using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;


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
    private Text[] commoditysTexts = null;
    
    [SerializeField]
    private Text statusText = null;
    



    private const string PickAResource = "Picking a Resource";
    private const string ThrowingCards = "Throwing Cards";
    private const string LosingCity = "Choosing City To Loose";

    private Color32 goodColor = new Color32(54, 166, 0, 255);
    private Color32 badColor = new Color32(205,0,0,255);
    private Color32 defaultColor = new Color(1,1,1,1);

    private int points = 0;
    private int coinLevel = 0;
    private int paperLevel = 0;
    private int silkLevel = 0;

    private const string pointsString = "Points {0}";
    private const string level = "Level: {0}";

    void Awake()
    {
        object[] data = photonView.InstantiationData;
        foreach(GameObject go in buildings)
            SetColor(Utils.Name_To_Color((string)data[0]), go.GetComponent<MeshRenderer>(), go);
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
            Debug.Log("We Have a Winner!!");
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
        commoditysTexts[type].text = amount;
    }

    [PunRPC]
    public void SetNumOfCardsText(string amount)
    {
        numOfCards.text = amount;
    }
}
