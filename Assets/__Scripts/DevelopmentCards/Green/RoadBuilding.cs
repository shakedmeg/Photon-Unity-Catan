using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilding : DevelopmentCard
{
    public bool BuiltOne { get; set; }

    HashSet<int> roads;

    protected override void CheckIfCanActivate()
    {
        if (buildManager.buildingAmounts[eBuilding.Road] == 0)
        {
            MiniCleanUp();
            return;
        }
        roads = buildManager.CalcRoads();
        if (roads.Count == 0)
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
        buildManager.edgesToTurnOff = roads;
    }

    public void BuildSecondRoad(int[] edgesIDs, int[] vertexIDs)
    {
        if (buildManager.buildingAmounts[eBuilding.Road] == 0)
        {
            CleanUp();
            return;
        }

        AddEdges(new List<int>(edgesIDs), vertexIDs);
        if (buildManager.edgesToTurnOff.Count == 0) 
        {
            CleanUp();
            return;
        }

    }


    private void AddEdges(List<int> edgeIDs, int[] vertexIDs)
    {
        
        foreach (int vertexID in vertexIDs)
        {
            if (buildManager.RivalsBuildingVertexes.ContainsKey(vertexID) || buildManager.RivalsKnights.ContainsKey(vertexID))
                continue;
            Vertex vertex = buildManager.GetVertex(vertexID);
            foreach (int edgeID in vertex.Edges)
            {
                if(edgeIDs.Contains(edgeID) && buildManager.FreeEdges.ContainsKey(edgeID))
                {
                    buildManager.FreeEdges[edgeID].gameObject.SetActive(true);
                    buildManager.edgesToTurnOff.Add(edgeID);
                }
            }
        }
    }

    public override void CleanUp()
    {
        base.CleanUp();
        buildManager.RoadCleanUp();
        turnManager.SetControl(true);

        BuiltOne = false;
    }
}
