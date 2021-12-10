using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Worker that wanders around the building
/// </summary>
public class SecurityWorker : Agent
{
    /// <summary>
    /// Forward this to the work action
    /// </summary>
    public override void GoingToWorkAction()
    {
        currentState = AgentState.Working;
        WorkingAction(true);
    }

    /// <summary>
    /// Move to a random security waypoint
    /// </summary>
    /// <param name="firstTime">True if this is the first time, false if not</param>
    public override void WorkingAction(bool firstTime)
    {
        agent.SetDestination(GameManager.Instance.SecurityWaypoint.position);
        if(!firstTime)
        {
            if(Random.Range(0f, 1f) < 0.15f)
            {
                hunger++;
            }
            if(Random.Range(0f, 1f) < 0.15f)
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
