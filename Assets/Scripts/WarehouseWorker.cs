using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Worker that "moves things" between spots in the warehouse
/// </summary>
public class WarehouseWorker : Agent
{
    /// <summary>
    /// Forward this to the working action
    /// </summary>
    public override void GoingToWorkAction()
    {
        currentState = AgentState.Working;
        WorkingAction(true);
    }

    /// <summary>
    /// Move between the different locations in the warehouse
    /// </summary>
    /// <param name="firstTime">True if it's the first time doing the working action</param>
    public override void WorkingAction(bool firstTime)
    {
        agent.SetDestination(GameManager.Instance.WarehouseDestination.position);
        if(!firstTime)
        {
            if(Random.Range(0f, 1f) < 0.2f)
            {
                hunger++;
            }
            if(Random.Range(0f, 1f) < 0.2f)
            {
                thirst++;
            }
            if(Random.Range(0f, 1f) < 0.05f)
            {
                restlessness++;
            }
            CheckActionsToDo();
        }
    }
}
