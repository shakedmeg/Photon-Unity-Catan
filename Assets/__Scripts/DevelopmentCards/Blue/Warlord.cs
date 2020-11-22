using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warlord : DevelopmentCard
{
    List<Knight> knights;

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();        
        knights = new List<Knight>();
        if(buildManager.PlayerKnights.Values.Count == 0)
        {
            MiniCleanUp();
            return;
        }
        foreach (Vertex vertex in buildManager.PlayerKnights.Values)
        {
            Knight knight = vertex.knight;
            if (!knight.Activated)
            {
                knights.Add(knight);
            }
        }
        DisplayCard(true);
        Invoke("Activate", 2f);
    }


    protected override void Activate()
    {
        foreach(Knight knight in knights)
        {
            knight.TurnOnKnight();
        }
        CleanUp();
    }

    public override void CleanUp()
    {
        base.CleanUp();
    }
}
