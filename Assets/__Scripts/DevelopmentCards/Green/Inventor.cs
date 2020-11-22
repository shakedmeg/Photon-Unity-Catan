using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Photon.Pun;
using UnityEngine;

public class Inventor : DevelopmentCard
{
    public Tile FirstTile { get; set; }

    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        Activate();
    }

    protected override void Activate()
    {
        base.Activate();
        turnManager.SetControl(false);
        SetInventorTiles(true);
    }


    private void SetInventorTiles(bool flag)
    {
        foreach (Tile tile in buildManager.tiles)
        {
            if (tile.probability == null || tile.probability.tNumber.text == "6" || tile.probability.tNumber.text == "8" || tile.probability.tNumber.text == "2" || tile.probability.tNumber.text == "12")
                continue;

            tile.tileSpot.SetActive(flag);
            tile.sColl.enabled = flag;
        }
    }

    public override void CleanUp()
    {
        base.CleanUp();
        SetInventorTiles(false);
        turnManager.SetControl(true);
    }
}
