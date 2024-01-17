using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ActionDelay
{
    bool Canceled = false;
    Action Action;

    public ActionDelay(Action action)
    {
        Action = action;
    }

    public IEnumerator WaitThenDoAction(float Seconds)
    {
        float internalTimer = 0;
        Debug.LogWarning($"Waiting {Seconds} seconds...");
        while (!Canceled && internalTimer < Seconds)
        {
            internalTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        if (!Canceled)
        {
            Debug.LogWarning($"Waiting done!");
            Action();
        }
        else
        {
            Debug.LogWarning($"Action canceled");
        }
    }

    public void Cancel()
    {
        Canceled = true;
    }
}
