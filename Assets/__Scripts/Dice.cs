using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Dice : MonoBehaviourPun
{


    private BoxCollider bColl;
    private Transform yellowDice;
    private Transform redDice;
    private Transform eventDice;

    private PlayerSetup playerSetup;
    private TurnManager turnManager;

    private Barbarians barbarians;
        

    private List<Vector3> quats = new List<Vector3>() {
        new Vector3(90, 0 , 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0), new Vector3(180, 0, 0), new Vector3(0, 270, 0), new Vector3(270, 0, 0)
    };

    void Awake() {
        bColl = GetComponent<BoxCollider>();
        yellowDice = transform.Find("YellowDice");
        redDice = transform.Find("RedDice");
        eventDice = transform.Find("EventDice");
        eventDice.gameObject.SetActive(false);
        bColl.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown() {
        //bColl.enabled = false;
        int yellowDiceNum = Random.Range(0, 6);
        int redDiceNum = Random.Range(0, 6);
        int eventDiceNum = Random.Range(0, 6);
        int score = yellowDiceNum + redDiceNum + 2;

        this.photonView.RPC("SetDice", RpcTarget.AllViaServer, yellowDiceNum, redDiceNum, eventDiceNum);

        if(GameManager.instance.state > GameState.Friendly)
        {
            if(eventDiceNum <3)
                barbarians.photonView.RPC("Advance", RpcTarget.AllBufferedViaServer, score);
            else
                SendNumber(score);
        }
        else
            SendNumber(score);


    }

    public void SendNumber(int diceNumber)
    {

        if (diceNumber != 7)
        {
            Utils.RaiseEventForAll(RaiseEventsCode.MatchTilesToDice, new object[] { diceNumber.ToString() });
        }
        else
        {
            // UNCOMMENT TO ENABLE ROBBER!

            //turnManager.GiveControl();
            //Utils.RaiseEventForAll(RaiseEventsCode.SevenRolled);
        }
    }

    public void SetCollider(bool flag)
    {
        bColl.enabled = flag;
    }

    public void ActivateEventDice()
    {
        eventDice.gameObject.SetActive(true);
    }


    [PunRPC]
    public void SetDice(int yellowDice, int redDice, int eventDice)
    {
        this.yellowDice.localRotation = Quaternion.Euler(quats[yellowDice]);
        this.redDice.localRotation = Quaternion.Euler(quats[redDice]);
        this.eventDice.localRotation = Quaternion.Euler(quats[eventDice]);
    }

    [PunRPC]
    public void Init(int barbariansID)
    {
        playerSetup = PlayerSetup.LocalPlayerInstance.GetComponent<PlayerSetup>();
        turnManager = PlayerSetup.LocalPlayerInstance.GetComponent<TurnManager>();

        transform.SetParent(playerSetup.canvas.transform);

        barbarians = PhotonView.Find(barbariansID).GetComponent<Barbarians>();
    }
}
