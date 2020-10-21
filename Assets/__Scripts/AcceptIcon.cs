using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AcceptIcon : MonoBehaviour
{
    [SerializeField]
    private ChoosePanel choosePanel;

    void OnMouseDown()
    {
        choosePanel.offerPanel.photonView.RPC("AcceptPressed", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
        Destroy(choosePanel.gameObject);
    }

    public void SetColor(Color color)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
    }
}
