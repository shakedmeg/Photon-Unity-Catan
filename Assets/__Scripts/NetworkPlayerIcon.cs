using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerIcon : MonoBehaviourPun
{
    public string colorName; 
    void Awake()
    {
        object[] data = photonView.InstantiationData;
        colorName = (string)data[0];
        Color color =  Utils.Name_To_Color(colorName);
        SetColor(color);
        transform.SetParent(PhotonView.Find((int)data[1]).transform.Find(Consts.ResponsesPanel),false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color color)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
        }
    }
}
