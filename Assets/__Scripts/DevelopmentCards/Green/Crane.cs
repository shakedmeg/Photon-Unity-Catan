using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Crane : DevelopmentCard
{

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();

        bool res = false;
        int unimprovedCitiesCount = buildManager.CountUnimprovedCities();
        for (int i = 5; i<8; i++)
        {
            bool ans = CheckCommodity(i, unimprovedCitiesCount);
            res |= ans;
            playerSetup.commodityButtons[i - 5].gameObject.SetActive(ans);
        }
        if (!res)
        {
            MiniCleanUp();
            return;
        }

        Activate();
    }
    protected override void Activate()
    {
        base.Activate();
        turnManager.SetControl(false);
        playerSetup.upgradeCommodityPanel.SetActive(true);       
    }

    /// <summary>
    /// Checks if the player has enough commodities after the discount.
    /// </summary>
    /// <param name="commodityType">commodity type to convert to eCommodity</param>
    /// <returns></returns>    
    private bool CheckCommodity(int commodityType, int unimprovedCitiesCount)
    {
        eCommodity commodity = (eCommodity)commodityType;
        if (buildManager.cityCount == 0 || cardManager.commodityCount[commodity] < cardManager.commodityPrices[commodity] - 1 || cardManager.commodityPrices[commodity] == 6) return false;
        bool canImproveCity = cardManager.commodityPrices[commodity] >= 4 && unimprovedCitiesCount != 0;
        if (!canImproveCity)
        {
            if (GameManager.instance.cityImprovementHolder[commodity][0] == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                canImproveCity = true;
            }
            else
            {
                if (GameManager.instance.cityImprovementHolder[commodity][0] != -1)
                    canImproveCity = GameManager.instance.cityImprovementHolder[commodity][1] >= cardManager.commodityPrices[commodity];
            }
        }
        if (canImproveCity || cardManager.commodityPrices[commodity] < 4)
            return true;
        else
            return false;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        playerSetup.upgradeCommodityPanel.SetActive(false);

    }

}
