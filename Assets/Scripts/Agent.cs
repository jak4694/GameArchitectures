using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using cakeslice;

/// <summary>
/// Abstract agent class to handle the agent classes
/// </summary>
public abstract class Agent : MonoBehaviour
{
    public enum AgentState { GoingToWork, Working, GoingToBreakRoom, InBreakRoom, GoingToMeeting, InMeeting, GoingOutside, Outside, TalkingToAgent, Cafeteria}

    protected AgentState currentState = AgentState.GoingToWork;
    protected CafeteriaAgentBehaviors cafeteriaBehaviors;
    protected Agent agentTalkingTo;
    protected NavMeshAgent agent;
    protected DestinationArea currentOutsideArea;
    protected int hunger = 0;
    protected int thirst = 0;
    protected float restlessness = 0;
    private Seat destinationSeat;
    private MeetingRoom destinationMeeting;

    /// <summary>
    /// Property to get the nav mesh agent component
    /// </summary>
    public NavMeshAgent NavMeshAgent { get { return agent; } }

    /// <summary>
    /// Property to get the current agent state
    /// </summary>
    public AgentState CurrentState { get { return currentState; } }

    /// <summary>
    /// Property to get the current state in string form
    /// </summary>
    public string CurrentStateString
    {
        get
        {
            if(currentState == AgentState.Cafeteria)
            {
                return cafeteriaBehaviors.CurrentState;
            }
            else
            {
                return currentState.ToString();
            }
        }
    }

    /// <summary>
    /// Property to get or set the hunger
    /// </summary>
    public int Hunger
    {
        get { return hunger; }
        set { hunger = value; }
    }

    /// <summary>
    /// Property to get or set the thirst
    /// </summary>
    public int Thirst
    {
        get { return thirst; }
        set { thirst = value; }
    }

    /// <summary>
    /// Property to get or set the restlessness value
    /// </summary>
    public float Restlessness
    {
        get { return restlessness; }
        set { restlessness = value; }
    }

    /// <summary>
    /// Set up the references for the behaviors
    /// </summary>
    protected void Start()
    {
        OutlineEffect.Instance?.RemoveOutline(GetComponentInChildren<Outline>());
        agent = GetComponent<NavMeshAgent>();
        cafeteriaBehaviors = GetComponent<CafeteriaAgentBehaviors>();
        ReturnToWork();
        StartCoroutine(CheckIfEndedPath());
    }

    //Each worker type acts differently going to work
    public abstract void GoingToWorkAction();

    //Each worker type has a special action
    public abstract void WorkingAction(bool firstTime);

    /// <summary>
    /// If an agent needs to respond to a meeting in a custom way, a method is provided
    /// </summary>
    protected virtual void RespondToMeeting() { }

    /// <summary>
    /// Set the agent to return to work again
    /// </summary>
    public void ReturnToWork()
    {
        currentState = AgentState.GoingToWork;
        GoingToWorkAction();
        CancelInvoke("ReturnToWork");
    }

    /// <summary>
    /// When the agent arrives to their destination
    /// </summary>
    protected void PathArrival()
    {
        switch(currentState)
        {
            case AgentState.GoingToWork:
                currentState = AgentState.Working;
                WorkingAction(true);
                break;
            case AgentState.Working:
                WorkingAction(false);
                break;
            case AgentState.GoingToBreakRoom:
                currentState = AgentState.InBreakRoom;
                WanderRoom(GameManager.Instance.BreakRoom);
                thirst = Mathf.Max(thirst - 1, 0);
                Invoke("ReturnToWork", Random.Range(10f, 30f));
                break;
            case AgentState.InBreakRoom:
                WanderRoom(GameManager.Instance.BreakRoom);
                restlessness -= 0.1f;
                if(restlessness <= 0)
                {
                    restlessness = 0;
                    CancelInvoke("ReturnToWork");
                    ReturnToWork();
                }
                break;
            case AgentState.GoingOutside:
                Invoke("ReturnToWork", Random.Range(15f, 40f));
                WanderRoom(currentOutsideArea);
                break;
            case AgentState.Outside:
                WanderRoom(currentOutsideArea);
                restlessness -= 0.1f;
                if(restlessness <= 0)
                {
                    restlessness = 0;
                    CancelInvoke("ReturnToWork");
                    ReturnToWork();
                }
                break;
            case AgentState.GoingToMeeting:
                if(Vector3.SqrMagnitude(transform.position - destinationSeat.transform.position) <= 4f)
                {
                    currentState = AgentState.InMeeting;
                    destinationMeeting.NumberOfAgentsArrived++;
                }
                break;
            case AgentState.TalkingToAgent:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Check if we need to react to hunger, thirst, or restlessness
    /// </summary>
    /// <returns>True if we did something that will stop us from our actions</returns>
    protected bool CheckActionsToDo()
    {
        //If we're in a meeting or at the cafeteria, ignore all of these
        if(currentState == AgentState.InMeeting || currentState == AgentState.GoingToMeeting || currentState == AgentState.Cafeteria)
        {
            return false;
        }
        //If we're hungry, see if we are hungry enough to go to the cafeteria
        if(hunger > 0)
        {
            if(Random.Range(0f, 1f) < hunger * 0.07f)
            {
                currentState = AgentState.Cafeteria;
                cafeteriaBehaviors.StartCafeteriaJourney();
                return true;
            }
        }
        //If we're thirsty, see if we want to go to the break room
        if(thirst > 0)
        {
            if(Random.Range(0f, 1f) < thirst * 0.08f)
            {
                currentState = AgentState.GoingToBreakRoom;
                agent.SetDestination(GameManager.Instance.BreakAreaDrinks.position);
                return true;
            }
        }
        //If we're restless, see if we want to go outside or to the break room
        if(restlessness > 0)
        {
            if(Random.Range(0f, 1f) < restlessness * 0.1f)
            {
                if(Random.Range(0f, 1f) <= 0.3f)
                {
                    currentState = AgentState.GoingToBreakRoom;
                    agent.SetDestination(GameManager.Instance.BreakRoom.DestinationWithinArea());
                }
                else
                {
                    currentState = AgentState.GoingOutside;
                    currentOutsideArea = GameManager.Instance.Outside;
                    agent.SetDestination(currentOutsideArea.DestinationWithinArea());
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Wander the given room
    /// </summary>
    protected void WanderRoom(DestinationArea area)
    {
        agent.destination = area.DestinationWithinArea();
    }

    /// <summary>
    /// Sets up an event for checking if the path has ended
    /// </summary>
    /// <returns>An enumerator</returns>
    protected IEnumerator CheckIfEndedPath()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        //If we're dealing with the cafeteria, let the cafeteria behaviors know that we arrived at a path
                        if (currentState == AgentState.Cafeteria)
                        {
                            cafeteriaBehaviors.PathArrival();
                        }
                        //Otherwise, let the agent deal with its own behaviors
                        else
                        {
                            PathArrival();
                        }
                    }
                }
            }
            yield return wait;
        }
    }

    /// <summary>
    /// Set the agent to be talking to another agent
    /// </summary>
    /// <param name="otherAgent">The other agent</param>
    public void TalkingToAgent(Agent otherAgent)
    {
        currentState = AgentState.TalkingToAgent;
        agentTalkingTo = otherAgent;
        agent.SetDestination(transform.position);
        CancelInvoke("ReturnToWork");
        Invoke("ReturnToWork", 15f);
    }

    /// <summary>
    /// Go to a meeting if the agent isn't going to or in the cafeteria
    /// </summary>
    /// <param name="meetingRoom">The meeting room to go to</param>
    /// <returns>True if the agent is going to the meeting; false otherwise</returns>
    public bool GoToMeeting(MeetingRoom meetingRoom)
    {
        if(currentState != AgentState.Cafeteria)
        {
            Seat seat = meetingRoom.Seats.GetSeat();
            if(seat == null)
            {
                return false;
            }
            currentState = AgentState.GoingToMeeting;
            destinationMeeting = meetingRoom;
            destinationSeat = seat;
            CancelInvoke("ReturnToWork");
            agent.destination = seat.transform.position;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Use trigger colliders to determine if the agent should talk to another or if they should stop talking when a manager passes by
    /// </summary>
    /// <param name="other">The other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Manager" && currentState == AgentState.TalkingToAgent)
        {
            agentTalkingTo.ReturnToWork();
            ReturnToWork();
        }
        Agent otherAgent = other.gameObject.GetComponent<Agent>();
        if(otherAgent && (currentState == AgentState.InBreakRoom || currentState == AgentState.Outside) &&
            (otherAgent.CurrentState == AgentState.InBreakRoom || otherAgent.CurrentState == AgentState.Outside))
        {
            if(Random.Range(0f, 1f) <= 1f)
            {
                TalkingToAgent(otherAgent);
                otherAgent.TalkingToAgent(this);
            }
        }
    }
}
