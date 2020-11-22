using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : DevelopmentCard
{
    Dictionary<int, bool> playersRes = new Dictionary<int, bool>();

    protected override void CheckIfCanActivate()
    {
        if (GameManager.instance.state != GameState.Playing)
        {
            MiniCleanUp();
            return;
        }


        Activate();
    }
    protected override void Activate()
    {
        base.Activate();
        turnManager.SetControl(false);
        cardManager.StartRob();
    }

    public void SetPlayers(int[] actorNums)
    {
        foreach(int i in actorNums)
        {
            playersRes.Add(i, false);
        }
    }

    public void SetPlayerRes(int actorNum)
    {
        playersRes[actorNum] = true;
    }

    public override void CleanUp()
    {
        foreach(bool value in playersRes.Values)
        {
            if (!value) return;
        }
        base.CleanUp();
        turnManager.SetControl(true);

        playersRes = new Dictionary<int, bool>();
    }
}
