using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Deserter : DevelopmentCard
{

    public int DesertedKnightLvl { get; private set; }
    public bool DesertedKnightActive { get; private set; }
    public int MaxBuildableLvl { get; private set; }

    public GameObject playerIconPrefab;

    private List<GameObject> playerIcons = new List<GameObject>();


    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        if (buildManager.RivalsKnights.Values.Count == 0)
        {
            MiniCleanUp();
            return;
        };

        Activate();
    }

    protected override void Activate()
    {
        base.Activate();
        Utils.RaiseEventForAll(RaiseEventsCode.SetDevelopmentCard, new object[] { (int)type });

        turnManager.SetControl(false);

        HashSet<int> rivalKnightActors = new HashSet<int>();
        foreach (Vertex vertex in buildManager.RivalsKnights.Values)
        {
            rivalKnightActors.Add(vertex.knight.photonView.OwnerActorNr);
        }

        playerSetup.deserterPanel.SetActive(true);
        foreach (int player in rivalKnightActors)
        {
            GameObject playerIconGO = Instantiate(playerIconPrefab, playerSetup.deserterPanel.transform);
            Utils.PaintPlayerIcon(playerIconGO, player);
            playerIcons.Add(playerIconGO);
        }
    }

    public void SetKnightData(int lvl, bool activated)
    {
        DesertedKnightLvl = lvl;
        DesertedKnightActive = activated;
    }

    public int GetMaxBuildableKnight()
    {
        int i;
        for (i = DesertedKnightLvl; i > 0; i--)
        {
            if (buildManager.buildingAmounts[(eBuilding)(i+4)] != 0)
                break;
        }
        MaxBuildableLvl = i;
        return i;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        Utils.RaiseEventForAll(RaiseEventsCode.SetDevelopmentCard, new object[] { (int)eDevelopmentCardsTypes.None });
        turnManager.SetControl(true);

        for(int i = 0; i<playerIcons.Count; i++)
        {
            Destroy(playerIcons[i]);
        }

        playerIcons.Clear();
    }
}
