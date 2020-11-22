using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smith : DevelopmentCard
{
    public bool BuiltOne { get; set; }
    Vertex firstKnightVertex;
    HashSet<int> upgradeableKnights;
    protected override void CheckIfCanActivate()
    {
        bool checkKnightLevel3 = buildManager.CanBuildKnightsLvl3 && buildManager.buildingAmounts[eBuilding.Knight3] > 0;
        if (!(buildManager.buildingAmounts[eBuilding.Knight2] > 0 || checkKnightLevel3))
        {
            MiniCleanUp();
            return;
        }
        upgradeableKnights = buildManager.GetUpgradeableKnights();
        if (upgradeableKnights.Count == 0)
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
        buildManager.knightsToTurnOff = upgradeableKnights;
    }

    public void BuildSecondKnight(Vertex knightVertex)
    {
        firstKnightVertex = knightVertex;
        upgradeableKnights.Remove(knightVertex.ID);
        knightVertex.knight.SetCollider(false);
        buildManager.UpgradeKnightCleanUp();
        upgradeableKnights = buildManager.GetUpgradeableKnights(firstKnightVertex.ID);
        if(upgradeableKnights.Count == 0)
        {
            CleanUp();
            return;
        }

        buildManager.knightsToTurnOff = upgradeableKnights;

    }

    public override void CleanUp()
    {
        base.CleanUp();
        buildManager.UpgradeKnightCleanUp();
        if (firstKnightVertex != null && firstKnightVertex.knight.Useable)
            firstKnightVertex.knight.SetCollider(true);
        turnManager.SetControl(true);

        BuiltOne = false;

    }
}
