using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Worker that sits in the cubicle and does work on a computer
/// </summary>
public class CubicleWorker : Agent
{
    [SerializeField]
    private Transform cubicleSpot = null;
    private Coroutine continuingWorkCoroutine;

    /// <summary>
    /// When we go to work, get to the cubicle spot
    /// </summary>
    public override void GoingToWorkAction()
    {
        agent.SetDestination(cubicleSpot.position);
    }

    /// <summary>
    /// While working, continue to get more hungry, tired, and restless
    /// </summary>
    /// <param name="firstTime">True if it's the first working action; false otherwise</param>
    public override void WorkingAction(bool firstTime)
    {
        if(firstTime)
        {
            if(Random.Range(0f, 1f) < 0.1f)
            {
                hunger++;
            }
            if(Random.Range(0f, 1f) < 0.1f)
            {
                thirst++;
            }
            if(Random.Range(0f, 1f) < 0.15f)
            {
                restlessness++;
            }
            bool stoppedWork = CheckActionsToDo();
            if(!stoppedWork)
            {
                continuingWorkCoroutine = StartCoroutine(ContinuingWork());
            }
        }
    }

    /// <summary>
    /// The agent waits a certain amount of time before potentially becoming restless/hungry/thirsty
    /// </summary>
    /// <returns></returns>
    private IEnumerator ContinuingWork()
    {
        yield return new WaitForSeconds(Random.Range(5f, 10f));
        WorkingAction(true);
    }

    /// <summary>
    /// If we're called to a meeting, stop work
    /// </summary>
    protected override void RespondToMeeting()
    {
        StopCoroutine(continuingWorkCoroutine);
    }
}
