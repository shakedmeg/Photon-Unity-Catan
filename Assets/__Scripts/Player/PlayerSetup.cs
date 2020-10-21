using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    public static GameObject LocalPlayerInstance;

    public Canvas canvas;

    void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            canvas.gameObject.SetActive(true);
            canvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);
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
