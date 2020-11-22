using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class Spy : DevelopmentCard
{

    public GameObject playerIconPrefab;
    private List<DevelopmentCard> showedDevCards = new List<DevelopmentCard>();

    public Dictionary<int, bool> checkDevCardsResponses;
    public Dictionary<int, int[][]> devCardsResponses;
    bool activated = false;



    private List<PlayerIcon> playerIcons = new List<PlayerIcon>();

    private int rival;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        object[] data;
        switch (photonEvent.Code)
        {
            case (byte)RaiseEventsCode.DevCardsCount:
                if (!photonView.IsMine || !activated) return;
                data = (object[])photonEvent.CustomData;
                int[][] cards = (int[][])data[0];
                devCardsResponses[photonEvent.Sender] = cards;
                checkDevCardsResponses[photonEvent.Sender] = true;
                if (AllFinished())
                    VerifyActivationCheck();
                break;
        }
    }

    public bool AllFinished()
    {
        foreach (bool res in checkDevCardsResponses.Values)
        {
            if (!res) return false;
        }
        return true;
    }



    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();
        activated = true;
        checkDevCardsResponses = new Dictionary<int, bool>();
        devCardsResponses = new Dictionary<int, int[][]>();
        foreach (Player player in GameManager.instance.players)
        {
            if (player.ActorNumber == GameManager.instance.CurrentPlayer) continue;
            checkDevCardsResponses.Add(player.ActorNumber, false);
            devCardsResponses.Add(player.ActorNumber, new int[0][]);
        }

        Utils.RaiseEventForGroup(RaiseEventsCode.CountDevCards, checkDevCardsResponses.Keys.ToArray());
    }

    private void VerifyActivationCheck()
    {
        bool canActivate = false;
        foreach(int[][] cards in devCardsResponses.Values)
        {
            if(cards.Length != 0)
            {
                canActivate = true;
                break;
            }
        }

        if (canActivate)
        {
            Activate();
        }
        else
        {
            MiniCleanUp();
            activated = false;
        }
    }

    protected override void Activate()
    {
        base.Activate();
        turnManager.SetControl(false);


        foreach (KeyValuePair<int, int[][]> entry in devCardsResponses)
        {
            if (entry.Value.Length != 0)
            {
                GameObject playerIconGO = Instantiate(playerIconPrefab, playerSetup.spyPanel.transform);
                playerIcons.Add(playerIconGO.GetComponent<PlayerIcon>());
                Utils.PaintPlayerIcon(playerIconGO, entry.Key);
            }
        }
        playerSetup.spyPanel.SetActive(true);
    }

    public void ShowCards(int owner) 
    {
        rival = owner;
        foreach(int[] cardData in devCardsResponses[owner])
        {
            DevelopmentCard card = Instantiate(cardManager.developmentCardsPrefabs[cardData[0]], playerSetup.spyShowPanel.transform).GetComponent<DevelopmentCard>();
            card.Background = PhotonView.Find(cardData[1]).gameObject;
            showedDevCards.Add(card);
        }
        playerSetup.spyShowPanel.SetActive(true);
    }


    public void Steal(DevelopmentCard card)
    {
        showedDevCards.Remove(card);
        Utils.RaiseEventForPlayer(RaiseEventsCode.Spy, rival, new object[] { (int)card.type });
        playerSetup.playerPanel.photonView.RPC("MakeActive", RpcTarget.AllBufferedViaServer, true);
        CleanUp();
    }

    public override void CleanUp()
    {
        base.CleanUp();
        turnManager.SetControl(true);

        for (int i = 0; i < playerIcons.Count; i++)
        {
            Destroy(playerIcons[i].gameObject);
        }
        playerIcons.Clear();

        for (int i = 0; i < showedDevCards.Count; i++)
        {
            Destroy(showedDevCards[i].gameObject);
        }

        showedDevCards.Clear();
    }
}
