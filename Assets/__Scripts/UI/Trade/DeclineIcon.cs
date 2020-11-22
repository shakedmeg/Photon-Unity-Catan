using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeclineIcon : MonoBehaviourPun
{
    [SerializeField]
    private ChoosePanel choosePanel = null;

    private PlayerSetup playerSetup;

    void Start()
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
    }

    void OnMouseDown()
    {
        if(playerSetup.currentCard == null)
        {
            choosePanel.offerPanel.photonView.RPC("DeclinePressed", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
            Destroy(choosePanel.gameObject);
        }
        else
        {
            CommercialHarbor commercialHarbor = playerSetup.currentCard as CommercialHarbor;
            commercialHarbor.CleanUp();
        }
    }

    public void SetColor(Color color)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
    }

}
