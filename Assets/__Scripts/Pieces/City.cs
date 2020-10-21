using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : VertexGamePiece
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Drop();
        ScaleUpDown();
    }

    void OnMouseDown()
    {
        StopScaling();
        switch (Vertex.buildManager.Build)
        {
            case eBuilding.Destroy:
                Vertex.buildManager.StopScalingCities(Vertex.buildManager.possibleCitiesToLose);
                Vertex.DestroyCity();
                break;
        }
    }
}
