using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Anchor : MonoBehaviourPun
{

    void Awake()
    {
        name = (string)photonView.InstantiationData[0];
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
