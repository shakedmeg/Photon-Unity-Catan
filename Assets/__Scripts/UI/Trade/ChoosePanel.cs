using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChoosePanel : MonoBehaviour
{

    public AcceptIcon acceptIcon;
    public DeclineIcon declineIcon;

    public OfferPanel offerPanel;

    void Awake()
    {
        acceptIcon.gameObject.SetActive(false);
    }

    public void SetColor(Color color)
    {
        acceptIcon.SetColor(color);
        declineIcon.SetColor(color);
    }

    public void CacheCreator(OfferPanel op)
    {
        offerPanel = op;
    }
}
