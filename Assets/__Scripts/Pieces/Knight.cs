using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Knight : VertexGamePiece
{
    public GameObject head;

    private Color playerColor;

    private MeshRenderer headMeshRenderer;
    private BoxCollider bColl;

    private BuildManager buildManager;
    private TurnManager turnManager;
    private CardManager cardManager;

    public bool Activated { get; set; }

    public int Level { get; private set; }

    public override void Awake()
    {
        base.Awake();
        headMeshRenderer = head.GetComponent<MeshRenderer>();
        bColl = GetComponent<BoxCollider>();
        bColl.enabled = false;
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
        turnManager = GameManager.instance.playerGameObject.GetComponent<TurnManager>();
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        Level = 1;

        object[] data = photonView.InstantiationData;
        playerColor = Utils.Name_To_Color((string)data[0]);

        if (data.Length > 1)
        {
            Activated = (bool) data[1];
            if (Activated)
            {
                bColl.enabled = true;
                ChangeHeadColor(true);
            }
            Level = (int)data[2];

        }
    }

    // Update is called once per frame
    void Update()
    {
        Drop();
        ScaleUpDown();
    }


    void OnMouseDown()
    {
        if (buildManager.Build == eBuildAction.UpgradeKnight)
        {
            Vertex.UpgradeKnight();
            buildManager.StopScalingKnights(false);
        }
        else
        {
            if (buildManager.Build == eBuildAction.ActivateKnight && Activated) return;
            if (buildManager.Build == eBuildAction.ActivateKnight)
            {
                Activated = true;
                this.photonView.RPC("ChangeHeadColor", RpcTarget.AllBufferedViaServer, true);
                cardManager.Pay();
                buildManager.CleanUp();
                turnManager.barbarians.photonView.RPC("ActivateKnight", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, Level);
            }
            else
            {
                object[] data;
                switch (buildManager.KnightAction)
                {
                    case eKnightActions.None:
                        buildManager.knightToMove = this;
                        HashSet<int> vertexes = buildManager.CalcKnightsActionsOptions(buildManager.PlayerKnights[Vertex.ID], new HashSet<int>(), new HashSet<int>(), true);
                        bool canMoveRobber = buildManager.CheckIfKnightCanMoveRobber(Vertex.Tiles);
                        

                        if (vertexes.Count == 0 && !canMoveRobber) 
                            return;
                        buildManager.KnightAction = eKnightActions.TakeAction;
                        if (vertexes.Count != 0)
                            buildManager.ShowKnightsActionsOptions(vertexes);
                        if (canMoveRobber)
                        {
                            cardManager.robber.cColl.enabled = true;
                            cardManager.robber.InitScaleUpDown(Consts.RobberRegularScale, Consts.ScaleRobber);
                        }
                        break;

                    case eKnightActions.TakeAction:
                        buildManager.knightToMove.TurnOffKnight();
                        SetCollider(false);
                        StopScaling();
                        data = new object[] { Vertex.ID };
                        buildManager.TurnOffKnightOptions();
                        turnManager.SetButtonsAndKnightsControl(false);
                        Utils.RaiseEventForPlayer(RaiseEventsCode.DisplaceKnight, this.photonView.Owner.ActorNumber, data);
                        break;


                }
            }
        }
    }


    public void SetCollider(bool flag)
    {
        bColl.enabled = flag;
    }

    [PunRPC]
    public void ChangeHeadColor(bool flag)
    {
        headMeshRenderer.material.color = flag? Consts.KnightHeadActivated : playerColor;
    }

    public void TurnOffKnight()
    {
        Activated = false;
        SetCollider(false);
        this.photonView.RPC("ChangeHeadColor", RpcTarget.AllBufferedViaServer, false);
        //ChangeHeadColor(playerColor);
    }

    public override void StopScaling()
    {
        base.StopScaling();
        transform.localScale = Level == 3 ? Consts.Knight3RegularScale : Consts.KnightRegularScale;
    }
}
