using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIcon : MonoBehaviour
{

    CardManager cardManager;
    CapsuleCollider cColl;
    public int Owner { get; set; }

    void Awake()
    {
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        cColl = GetComponent<CapsuleCollider>();
    }
    void OnMouseDown()
    {
        cColl.enabled = false;
        cardManager.FinishSelect();
        Utils.RaiseEventForPlayer(RaiseEventsCode.LoseCard, Owner);
    }

    public void SetColor(Color color)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = new Color(color.r, color.g, color.b, r.material.color.a);
        }
    }


}
