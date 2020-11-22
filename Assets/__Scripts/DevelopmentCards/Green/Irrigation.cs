using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Irrigation : DevelopmentCard
{
    HashSet<int> wheatTilesSettled;


    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        wheatTilesSettled = new HashSet<int>();
        foreach (Tile tile in buildManager.tiles)
        {
            if (tile.Resource == eResources.Wheat)
            {
                foreach (int vertexID in tile.Vertexes)
                {
                    if (buildManager.PlayerBuildings.ContainsKey(vertexID))
                        wheatTilesSettled.Add(tile.photonView.ViewID);
                }
            }
        }
        if (wheatTilesSettled.Count == 0)
        {
            MiniCleanUp();
            return;
        }
        DisplayCard(true);
        Invoke("Activate", 2f);
    }
    protected override void Activate()
    {
        for (int i = 0; i < wheatTilesSettled.Count * 2; i++)
        {
            cardManager.InitCard((int)eResources.Wheat);
        }
        cardManager.SetNumOfCardsInPanel();
        CleanUp();
    }

    public override void CleanUp()
    {
        base.CleanUp();
    }

}
