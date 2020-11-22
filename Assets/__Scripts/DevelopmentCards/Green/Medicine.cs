using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medicine : DevelopmentCard
{
    public Dictionary<eResources, int> Price { get; } = new Dictionary<eResources, int>()
    {
        { eResources.Ore, 2 },
        { eResources.Wheat, 1 }
    };

    List<Vertex> settlements;

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        if (buildManager.buildingAmounts[eBuilding.City] == 0 || !cardManager.CheckPriceInHand(Price))
        {
            MiniCleanUp();
            return;
        }
        settlements = buildManager.GetSettlements();
        if (settlements.Count == 0)
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
        buildManager.settlements = settlements;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);
    }
}
