using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Merchant : DevelopmentCard
{
    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        Activate();
    }

    protected override void Activate()
    {
        base.Activate();
        turnManager.SetControl(false);

        MerchantPiece merchant = cardManager.merchant;
        merchant.gameObject.SetActive(false);
        bool allowSameTile = false;
        if (cardManager.merchant.photonView.Owner != PhotonNetwork.LocalPlayer)
        {
            if (cardManager.merchant.photonView.Owner != null)
            {
                Utils.RaiseEventForPlayer(RaiseEventsCode.LoseMerchant, cardManager.merchant.photonView.OwnerActorNr);
            }
            allowSameTile = true;
            cardManager.merchant.photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            playerSetup.playerPanel.photonView.RPC("AddVictoryPoints", RpcTarget.AllBufferedViaServer, 1);
        }

        
        foreach (Tile tile in buildManager.tiles)
        {
            if (tile != merchant.Tile || allowSameTile)
            {
                foreach(int vertexID in tile.Vertexes)
                {
                    if (buildManager.PlayerBuildings.ContainsKey(vertexID))
                    {
                        tile.tileSpot.SetActive(true);
                        tile.sColl.enabled = true;
                        break;
                    }
                }
            }
        }
    }

    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);
    }
}
