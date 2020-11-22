using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Knight : VertexGamePiece
{
    public GameObject head;

    private Color playerColor;

    private MeshRenderer headMeshRenderer;

    private BuildManager buildManager;
    private TurnManager turnManager;
    private CardManager cardManager;

    public bool Activated { get; set; }

    public int Level { get; set; }

    public bool Useable { get; set; }

    public override void Awake()
    {
        base.Awake();
        headMeshRenderer = head.GetComponent<MeshRenderer>();
        coll.enabled = false;
        buildManager = GameManager.instance.playerGameObject.GetComponent<BuildManager>();
        turnManager = GameManager.instance.playerGameObject.GetComponent<TurnManager>();
        cardManager = GameManager.instance.playerGameObject.GetComponent<CardManager>();
        Level = 1;

        object[] data = photonView.InstantiationData;
        playerColor = Utils.Name_To_Color((string)data[0]);

        if (data.Length > 1)
        {
            Activated = (bool) data[1];
            Useable = (bool)data[2];
            if(Useable)
                coll.enabled = true;

            if (Activated)
            {
                ChangeHeadColor(true);
                if (photonView.IsMine)
                {
                    turnManager.barbarians.photonView.RPC("ActivateKnight", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, 1);
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (Vertex.playerSetup.currentCard != null)
        {
            switch (Vertex.playerSetup.currentCard.type)
            {
                case eDevelopmentCardsTypes.Smith:
                    HandleSmith();
                    break;
                case eDevelopmentCardsTypes.Intrigue:
                    HandleIntrigue();
                    break;
            }
            return;
        }

        if(Vertex.playerSetup.currentCardType == eDevelopmentCardsTypes.Deserter)
        {
            Utils.RaiseEventForAll(RaiseEventsCode.RemoveKnight, new object[] { Vertex.ID });
            buildManager.StopScalingKnights();
            Vertex.playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
            Utils.RaiseEventForPlayer(RaiseEventsCode.BuildDesertedKnight, GameManager.instance.CurrentPlayer, new object[] { Level, Activated });
            Vertex.DestroyKnight();
            return;
        }

        if (buildManager.Build == eBuildAction.UpgradeKnight)
        {
            Vertex.UpgradeKnight();
            buildManager.StopScalingKnights();
            
        }
        else
        {
            if (buildManager.Build == eBuildAction.ActivateKnight && Activated) return;
            if (buildManager.Build == eBuildAction.ActivateKnight)
            {
                cardManager.Pay(Consts.Prices[buildManager.Build]);
                TurnOnKnight();
                buildManager.CleanUp();
            }
            else
            {
                object[] data;
                switch (buildManager.KnightAction)
                {
                    case eKnightActions.None:
                        if (!Useable) return;
                        buildManager.knightToMove = this;
                        HashSet<int> vertexes = buildManager.CalcKnightsActionsOptions(buildManager.PlayerKnights[Vertex.ID], new HashSet<int>(), new HashSet<int>(), true);
                        bool canMoveRobber = buildManager.CheckIfKnightCanMoveRobber(Vertex.Tiles);
                        

                        if (vertexes.Count == 0 && !canMoveRobber) 
                            return;

                        SetCollider(false);
                        buildManager.KnightAction = eKnightActions.TakeAction;
                        if (vertexes.Count != 0)
                            buildManager.ShowKnightsActionsOptions(vertexes);
                        if (canMoveRobber)
                        {
                            cardManager.robber.InitScaleUpDown(Consts.RobberRegularScale, Consts.ScaleRobber);
                        }

                        buildManager.cancelButton.SetActive(true);
                        turnManager.SetControl(false);
                        break;

                    case eKnightActions.TakeAction:
                        buildManager.cancelButton.SetActive(false);
                        //buildManager.knightToMove.TurnOffKnight();
                        StopScaling();


                        buildManager.TurnOffKnightOptions();

                        // Verify here robber may not have been set!

                        cardManager.robber.StopScaling();
                        turnManager.SetControl(false);


                        data = new object[] { Vertex.ID };
                        Vertex.playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
                        Utils.RaiseEventForPlayer(RaiseEventsCode.DisplaceKnight, this.photonView.Owner.ActorNumber, data);
                        break;


                }
            }
        }
    }


    public void SetCollider(bool flag)
    {
        coll.enabled = flag;
    }

    [PunRPC]
    public void ChangeHeadColor(bool flag)
    {
        headMeshRenderer.material.color = flag? Consts.KnightHeadActivated : playerColor;
    }

    [PunRPC]
    public void SetLevel(int lvl)
    {
        Level = lvl;
    }

    public void TurnOnKnight()
    {
        Activated = true;
        this.photonView.RPC("ChangeHeadColor", RpcTarget.AllBufferedViaServer, true);
        turnManager.barbarians.photonView.RPC("ActivateKnight", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, Level);
        Vertex.playerSetup.playerPanel.photonView.RPC("SetActivatedKnightsText", RpcTarget.AllBufferedViaServer, Level);
    }

    public void TurnOffKnight()
    {
        Activated = false;
        Useable = false;
        SetCollider(false);
        this.photonView.RPC("ChangeHeadColor", RpcTarget.AllBufferedViaServer, false);
        turnManager.barbarians.photonView.RPC("DeactivateKnight", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, Level);
        Vertex.playerSetup.playerPanel.photonView.RPC("SetActivatedKnightsText", RpcTarget.AllBufferedViaServer, Level*(-1));

    }

    public override void StopScaling()
    {
        base.StopScaling();
        transform.localScale = Level == 3 ? Consts.Knight3RegularScale : Consts.KnightRegularScale;
    }



    #region Development Cards Functions
    private void HandleSmith()
    {
        Smith smith = Vertex.playerSetup.currentCard as Smith;
        Vertex.BuildUpgradedKnight();
        if (!smith.BuiltOne)
        {
            smith.BuiltOne = true;
            smith.BuildSecondKnight(Vertex);
        }
        else
        {
            smith.CleanUp();
        }
    }

    private void HandleIntrigue()
    {
        Intrigue intrigue = Vertex.playerSetup.currentCard as Intrigue;
        intrigue.StopScalingKnights();
        object [] data = new object[] { Vertex.ID };
        Vertex.playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, false);
        Utils.RaiseEventForPlayer(RaiseEventsCode.DisplaceKnight, this.photonView.Owner.ActorNumber, data);
    }
    #endregion
}
