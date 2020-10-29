using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelIcon : MonoBehaviour
{
    [SerializeField]
    private OfferPanel offerPanel = null;

    void OnMouseDown()
    {
        offerPanel.CancelIconPressed();
    }

    public void SetColor(Color color)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
    }

}
