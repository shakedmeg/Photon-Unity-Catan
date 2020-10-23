using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkDeclineIcon : MonoBehaviourPun
{

    void Awake()
    {
        gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        
    // Replaces the player icon in the responses
    [PunRPC]
    public void Init(int parentViewID, int siblindIndx, string colorName, int sender)
    {
        transform.SetParent(PhotonView.Find(parentViewID).transform.Find(Consts.ResponsesPanel), false);
        transform.SetSiblingIndex(siblindIndx);
        gameObject.SetActive(true);
        Color color = Utils.Name_To_Color(colorName);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
    }
}
