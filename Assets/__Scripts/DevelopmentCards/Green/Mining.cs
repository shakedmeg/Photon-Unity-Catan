using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mining : DevelopmentCard
{
    HashSet<int> oreTilesSettled;

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        oreTilesSettled = new HashSet<int>();
        foreach (Tile tile in buildManager.tiles)
        {
            if (tile.Resource == eResources.Ore)
            {
                foreach (int vertexID in tile.Vertexes)
                {
                    if (buildManager.PlayerBuildings.ContainsKey(vertexID))
                        oreTilesSettled.Add(tile.photonView.ViewID);
                }
            }
        }

        if (oreTilesSettled.Count == 0)
        {
            MiniCleanUp();
            return;
        }
        DisplayCard(true);
        Invoke("Activate", 2f);
    }

    protected override void Activate()
    {
        for (int i = 0; i < oreTilesSettled.Count * 2; i++)
        {
            cardManager.InitCard((int)eResources.Ore);
        }
        cardManager.SetNumOfCardsInPanel();
        CleanUp();
    }

    public override void CleanUp()
    {
        base.CleanUp();
    }
}
