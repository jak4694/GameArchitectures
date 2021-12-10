using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class handling queueing up agents for food or payment
/// </summary>
public class Line : MonoBehaviour
{
    [SerializeField]
    private bool foodLine = true;
    [SerializeField]
    private Transform lineEntrance = null;
    [SerializeField]
    private float timeToCompleteAction = 15f;
    [SerializeField]
    private Transform[] spotsInLine;
    private List<CafeteriaAgentBehaviors> agentsInLine = new List<CafeteriaAgentBehaviors>();
    private List<CafeteriaAgentBehaviors> agentsWaitingForLine = new List<CafeteriaAgentBehaviors>();

    /// <summary>
    /// Property to get whether or not this is a line for food
    /// </summary>
    public bool FoodLine { get { return foodLine; } }

    /// <summary>
    /// Property to get the line entrance position
    /// </summary>
    public Vector3 LineEntrance { get { return lineEntrance.position; } }

    /// <summary>
    /// Determines a spot for an agent to go into the line
    /// </summary>
    /// <param name="agent">The agent that wants to go into the line</param>
    /// <returns>True if the agent could get in line, false if it's full</returns>
    public bool PutAgentInLine(CafeteriaAgentBehaviors agent)
    {
        //Add the agent to the line if there is a spot available
        if(agentsInLine.Count < spotsInLine.Length)
        {
            agentsInLine.Add(agent);
            //If it's the first agent in line, start the service process
            if(agentsInLine.Count == 1)
            {
                Invoke("CompleteLineAction", timeToCompleteAction);
            }
            agent.NavMeshAgent.SetDestination(spotsInLine[agentsInLine.Count - 1].position);
            return true;
        }
        //If the line is full, add the agent to the queue to get into the line
        else
        {
            agentsWaitingForLine.Add(agent);
            return false;
        }
    }

    /// <summary>
    /// When the service has been completed, move up the line and begin serving the next agent
    /// </summary>
    private void CompleteLineAction()
    {
        //Remove the agent at the beginning of the line and inform them the service has been complete
        agentsInLine[0].LineActionCompleted();
        agentsInLine.RemoveAt(0);
        //If there are agents waiting to get into line, put one into the line
        if(agentsWaitingForLine.Count > 0)
        {
            //Determine the closest agent to the line entrance from the waiting area
            float minDistance = float.MaxValue;
            int indexOfClosestAgent = -1;
            for(int i = 0; i < agentsWaitingForLine.Count; i++)
            {
                float distanceSquared = Vector3.SqrMagnitude(lineEntrance.transform.position - agentsWaitingForLine[i].transform.position);
                if(distanceSquared < minDistance)
                {
                    indexOfClosestAgent = i;
                    minDistance = distanceSquared;
                }
            }
            //Put the agent into the line
            CafeteriaAgentBehaviors nextAgent = agentsWaitingForLine[indexOfClosestAgent];
            agentsWaitingForLine.RemoveAt(indexOfClosestAgent);
            nextAgent.WaitForLineOver();
            agentsInLine.Add(nextAgent);
        }
        //If there are any agents in line, move them all up and start the next service
        if(agentsInLine.Count > 0)
        {
            for(int i = 0; i < agentsInLine.Count; i++)
            {
                agentsInLine[i].NavMeshAgent.SetDestination(spotsInLine[i].position);
            }
            Invoke("CompleteLineAction", timeToCompleteAction);
        }
    }
}
