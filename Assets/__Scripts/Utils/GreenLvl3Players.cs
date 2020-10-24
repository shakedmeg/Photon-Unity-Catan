using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GreenLvl3Player
{

    public int ActorID { get; set; }
    public bool Finished { get; set; } = false;
    
    public GreenLvl3Player(int actorID)
    {
        ActorID = actorID;
    }

}
public class GreenLvl3Players
{
    public List<GreenLvl3Player> Players { get; set; } = new List<GreenLvl3Player>();

    public bool FirstOne { get; set; }

    public bool IsEmpty()
    {
        return Players.Count == 0;
    }


    public List<int> GetActorIDs()
    {
        return new List<int>(Players.Select(x => x.ActorID));
    }

    public bool AllFinished()
    {
        return Players.Aggregate(true, (acc, curr) => acc && curr.Finished);
    }

    public void SetPlayerFinishByID(int id)
    {
        foreach (GreenLvl3Player player in Players)
            if (id == player.ActorID)
                player.Finished = true;
    }

    public void Reset()
    {
        FirstOne = false;
        foreach (GreenLvl3Player player in Players)
            player.Finished = false;
    }
}
