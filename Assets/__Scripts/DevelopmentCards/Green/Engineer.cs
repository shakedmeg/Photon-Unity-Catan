using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engineer : DevelopmentCard
{
    List<Vertex> cities;

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        if (buildManager.buildingAmounts[eBuilding.Wall] == 0)
        {
            MiniCleanUp();
            return;
        }
        cities = buildManager.GetCitiesWithoutWalls();
        if (cities.Count == 0)
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
        buildManager.regularCities = cities;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);

        //cardManager.developmentHand.Remove(this);
        // SEND RPC TO RETURN CARD TO DECK!!!!!!!!!!!!
        //Destroy(gameObject);
    }
}
