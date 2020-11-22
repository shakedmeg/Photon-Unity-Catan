using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantFleet : DevelopmentCard
{
    protected override void CheckIfCanActivate()
    {
        base.CheckIfCanActivate();

        bool canActivate = false;
        foreach(KeyValuePair<eResources, ePorts> entry in cardManager.ports)
        {
            if (entry.Value == ePorts.p3To1 || entry.Value == ePorts.p4To1)
            {
                if (cardManager.merchantFleetPorts.Contains(entry.Key)) continue;

                if (cardManager.merchantPort == -1 || cardManager.merchantPort == 100)
                {
                    canActivate = true;
                    break;
                }
                else
                {
                    if ((eResources)cardManager.merchantPort != entry.Key )
                    {
                        canActivate = true;
                        break;
                    }
                }
            }
        }

        if (canActivate)
        {
            Activate();
        }
        else
        {
            MiniCleanUp();
            return;
        }
    }


    protected override void Activate()
    {
        base.Activate();
        turnManager.SetControl(false);

        foreach (KeyValuePair<eResources, ePorts> entry in cardManager.ports)
        {
            if (entry.Value == ePorts.p3To1 || entry.Value == ePorts.p4To1)
            {
                if (cardManager.merchantFleetPorts.Contains(entry.Key))
                {
                    playerSetup.merchantFleetOptions[(int)entry.Key].SetActive(false);
                    continue;
                }
                
                if (cardManager.merchantPort == -1 || cardManager.merchantPort == 100)
                {
                    playerSetup.merchantFleetOptions[(int)entry.Key].SetActive(true);
                }
                else
                {
                    if ((eResources)cardManager.merchantPort != entry.Key)
                    {
                        playerSetup.merchantFleetOptions[(int)entry.Key].SetActive(true);
                    }
                    else
                    {
                        playerSetup.merchantFleetOptions[(int)entry.Key].SetActive(false);
                    }
                }
            }
            else
            {
                playerSetup.merchantFleetOptions[(int)entry.Key].SetActive(false);
            }
        }

        playerSetup.merchantFleetPanel.SetActive(true);
    }

    public override void CleanUp()
    {
        base.CleanUp();
        playerSetup.merchantFleetPanel.SetActive(false);
        turnManager.SetControl(true);
    }
}
