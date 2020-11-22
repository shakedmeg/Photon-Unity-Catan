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
    private Text activatedKnightsText = null;

    [SerializeField]
    private GameObject activatedKnightHead = null;
    
    [SerializeField]
    private GameObject activatedKnightBody = null;

    [SerializeField]
    private Text numOfCards = null;
    
    [SerializeField]
    private Text longestRoad = null;    
    [SerializeField]
    private Text longestRoadText = null;

    [SerializeField]
    private GameObject longestRoadImage = null;

    [SerializeField]
    private Image[] paperLevels = null;
    [SerializeField]
    private Image[] coinLevels = null;
    [SerializeField]
    private Image[] silkLevels = null;

    
    [SerializeField]
    private Text statusText = null;
    


    private const string PickAResource = "Picking a Resource";
    private const string ThrowingCards = "Throwing Cards";
    private const string LosingCity = "Losing City";
    private const string LosingKnight = "Losing Knight";
    private const string Displace = "Displacing Knight";
    private const string GivingCards = "Giving Cards";
    private const string ExchangeCards = "Exchanging Cards";
    private const string ChoosingCards = "Choosing Cards";
    private const string LosingCards = "Losing Cards";
    private const string ChoosingDevCards = "Choosing Development Card";
    private const string LosingDevCards = "Losing Development Card";


    private Color32 goodColor = new Color32(54, 166, 0, 255);
    private Color32 badColor = new Color32(205,0,0,255);
    private Color32 defaultColor = new Color(1,1,1,1);

    public int points = 0;

    private const string pointsString = "Points {0}";
    private const string numOfCardsInHand = "Cards: {0}";

    private string playerColor; 

    void Awake()
    {
        object[] data = photonView.InstantiationData;
        playerColor = (string)data[0];
        foreach (GameObject go in buildings)
            SetColor(Utils.Name_To_Color(playerColor), go.GetComponent<MeshRenderer>(), go);
        SetColor(Consts.KnightHeadActivated, activatedKnightHead.GetComponent<MeshRenderer>(), activatedKnightHead);
        SetColor(Utils.Name_To_Color(playerColor), activatedKnightBody.GetComponent<MeshRenderer>(), activatedKnightBody);

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
            case Consts.LoseKnight:
                statusText.gameObject.SetActive(true);
                statusText.text = LosingKnight;
                image.color = Consts.CoinDevelopmentColor;
                break;
            case Consts.DisplaceKnightIntrigue:
                statusText.gameObject.SetActive(true);
                statusText.text = Displace;
                image.color = Consts.CoinDevelopmentColor;
                break;
            case Consts.DisplaceKnight:
                statusText.gameObject.SetActive(true);
                statusText.text = Displace;
                image.color = badColor;
                break;
            case Consts.Saboteur:
                statusText.gameObject.SetActive(true);
                statusText.text = ThrowingCards;
                image.color = Consts.CoinDevelopmentColor;
                break;
            case Consts.Wedding:
                statusText.gameObject.SetActive(true);
                statusText.text = GivingCards;
                image.color = Consts.CoinDevelopmentColor;
                break;
            case Consts.CommercialHarbor:
                statusText.gameObject.SetActive(true);
                statusText.text = ExchangeCards;
                image.color = Consts.SilkDevelopmentColor;
                break;
            case Consts.MasterMerchantTaker:
                statusText.gameObject.SetActive(true);
                statusText.text = ChoosingCards;
                image.color = Consts.SilkDevelopmentColor;
                break;
            case Consts.MasterMerchantVictim:
                statusText.gameObject.SetActive(true);
                statusText.text = LosingCards;
                image.color = new Color32(Consts.SilkDevelopmentColor.r, Consts.SilkDevelopmentColor.g, Consts.SilkDevelopmentColor.b, 100);
                break;
            case Consts.Spy:
                statusText.gameObject.SetActive(true);
                statusText.text = ChoosingDevCards;
                image.color = Consts.CoinDevelopmentColor;
                break;
            case Consts.SpyVictim:
                statusText.gameObject.SetActive(true);
                statusText.text = LosingDevCards;
                image.color = new Color32(Consts.CoinDevelopmentColor.r, Consts.CoinDevelopmentColor.g, Consts.CoinDevelopmentColor.b, 100);
                break;
            case Consts.ChooseDevCard:
                statusText.gameObject.SetActive(true);
                statusText.text = Consts.ChooseDevCard;
                image.color = goodColor;
                break;
            case Consts.ThrowDevCard:
                statusText.gameObject.SetActive(true);
                statusText.text = Consts.ThrowDevCard;
                image.color = badColor;
                break;
        }
    }


    [PunRPC]
    public void AddVictoryPoints(int vp)
    {
        points += vp;
        victoryPoints.text = string.Format(pointsString, points);
        if (photonView.IsMine)
        {
            Utils.RaiseEventForAll(RaiseEventsCode.UpdatePointsForAll, new object[] { vp });
        }
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
        switch (type)
        {
            case 0:
                coinLevels[int.Parse(amount) - 1].gameObject.SetActive(true);
                break;
            case 1:
                paperLevels[int.Parse(amount) - 1].gameObject.SetActive(true);
                break;
            case 2:
                silkLevels[int.Parse(amount) - 1].gameObject.SetActive(true);
                break;
        }
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

    [PunRPC]
    public void SetLongestRoadTextBold(bool flag)
    {
        FontStyle fs = flag ? FontStyle.Bold : FontStyle.Normal;
        longestRoad.fontStyle = fs;
        longestRoadText.fontStyle = fs;
        longestRoadImage.SetActive(flag);
    }

    [PunRPC]
    public void SetActivatedKnightsText(int add)
    {
        if (add == 0)
        {
            activatedKnightsText.text = "0";
        }
        else
        {
            int amount = int.Parse(activatedKnightsText.text);
            amount += add;
            activatedKnightsText.text = amount.ToString();
        }
    }
}
