using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Probability : MonoBehaviourPun
{
    public TextMesh tNumber;


    void Awake()
    {
        object[] data = photonView.InstantiationData;
        string number = (string)data[0];
        tNumber = transform.Find("Number").GetComponent<TextMesh>();
        tNumber.text = number;
        if(number == "6" || number == "8")
        {
            tNumber.color = new Color32(176, 41, 41, 255);
        }

        transform.SetParent(PhotonView.Find((int)data[1]).gameObject.transform);
        transform.localPosition = Vector3.back;

    }
    
}
