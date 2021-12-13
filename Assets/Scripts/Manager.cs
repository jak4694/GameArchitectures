using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using cakeslice;

/// <summary>
/// Manager that can call meetings and check on subordinates
/// </summary>
public class Manager : MonoBehaviour
{
    public enum ManagerState { GoingToOffice, InOffice, CheckingOnSubordinates, GoingToMeeting, WaitingToStartMeeting, InMeeting }

    private ManagerState currentState = ManagerState.GoingToOffice;
    [SerializeField]
    private Agent[] subordinates;
    [SerializeField]
    private Transform officeLocation;
    private NavMeshAgent agent;
    private List<Agent> subordinatesInMeeting = new List<Agent>();
    private MeetingRoom currentMeetingRoom;
    private int subordinatesToCheckOnCount;
    private Agent subordinateToCheckOn;
    private Coroutine subordinateTargetCoroutine;

    /// <summary>
    /// Property to get the current state in string form
    /// </summary>
    public string CurrentStateString
    {
        get { return currentState.ToString(); }
    }

    /// <summary>
    /// Send the manager to their office at the start
    /// </summary>
    private void Start()
    {
        OutlineEffect.Instance?.RemoveOutline(GetComponentInChildren<Outline>());
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(CheckIfEndedPath());
        GoToOffice();
    }

    /// <summary>
    /// Handle arriving at the destination based on the state
    /// </summary>
    private void PathArrival()
    {
        switch(currentState)
        {
            case ManagerState.GoingToOffice:
                if(Vector3.SqrMagnitude(transform.position - officeLocation.position) <= 4)
                {
                    currentState = ManagerState.InOffice;
                    StartCoroutine(DetermineWhatToDo());
                }
                break;
            case ManagerState.CheckingOnSubordinates:
                if (Vector3.SqrMagnitude(transform.position - agent.destination) <= 4)
                {
                    subordinatesToCheckOnCount--;
                    StopCoroutine(subordinateTargetCoroutine);
                    if (subordinatesToCheckOnCount <= 0)
                    {
                        GoToOffice();
                    }
                    else
                    {
                        PickNewSubordinateToCheckOn();
                    }
                }
                break;
            case ManagerState.GoingToMeeting:
                if(CheckToStartMeeting())
                {
                    currentState = ManagerState.InMeeting;
                }
                else
                {
                    currentState = ManagerState.WaitingToStartMeeting;
                    StartCoroutine(WaitingForMeetingToStart());
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Sets up an event for checking if the path has ended
    /// </summary>
    /// <returns>An enumerator</returns>
    private IEnumerator CheckIfEndedPath()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while(true)
        {
            if(!agent.pathPending)
            {
                if(agent.remainingDistance <= agent.stoppingDistance)
                {
                    if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        PathArrival();
                    }
                }
            }
            yield return wait;
        }
    }

    /// <summary>
    /// Coroutine to wait until deciding to do something
    /// </summary>
    /// <returns>An enumerator</returns>
    private IEnumerator DetermineWhatToDo()
    {
        bool determinedWhatToDo = false;
        while(!determinedWhatToDo)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            if(Random.Range(0f, 1f) <= 0.3f)
            {
                determinedWhatToDo = true;
                CheckOnSubordinates();
            }
            else if(Random.Range(0f, 1f) <= 0.1f)
            {
                determinedWhatToDo = true;
                CallMeeting();
            }
        }
    }

    /// <summary>
    /// Get the manager to go to the office
    /// </summary>
    private void GoToOffice()
    {
        currentState = ManagerState.GoingToOffice;
        agent.destination = officeLocation.position;
    }

    /// <summary>
    /// Start the process of checking on subordinates
    /// </summary>
    private void CheckOnSubordinates()
    {
        currentState = ManagerState.CheckingOnSubordinates;
        subordinatesToCheckOnCount = Random.Range(2, 4);
        PickNewSubordinateToCheckOn();
    }

    /// <summary>
    /// Pick a new subordinate to check on
    /// </summary>
    private void PickNewSubordinateToCheckOn()
    {
        subordinateToCheckOn = subordinates[Random.Range(0, subordinates.Length)];
        agent.destination = subordinateToCheckOn.transform.position;
        subordinateTargetCoroutine = StartCoroutine(UpdateSubordinateTarget());
    }

    /// <summary>
    /// Every half second, update the path to the subordinate
    /// </summary>
    /// <returns>An enumerator</returns>
    private IEnumerator UpdateSubordinateTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        while(true)
        {
            yield return wait;
            agent.destination = subordinateToCheckOn.transform.position;
        }
    }

    /// <summary>
    /// Call a meeting for all of those not in the cafeteria
    /// </summary>
    private void CallMeeting()
    {
        currentMeetingRoom = GameManager.Instance.MeetingRoom;
        if(currentMeetingRoom == null)
        {
            GoToOffice();
            return;
        }
        currentState = ManagerState.GoingToMeeting;
        currentMeetingRoom.IsTaken = true;
        agent.destination = currentMeetingRoom.Seats.GetSeat().transform.position;
        for(int i = 0; i < subordinates.Length; i++)
        {
            if(subordinates[i].GoToMeeting(currentMeetingRoom))
            {
                subordinatesInMeeting.Add(subordinates[i]);
            }
        }
    }

    /// <summary>
    /// Check if everyone is in the meeting, and if so start the meeting
    /// </summary>
    /// <returns>True if the meeting is started; false otherwise</returns>
    private bool CheckToStartMeeting()
    {
        if(currentMeetingRoom.NumberOfAgentsArrived == subordinatesInMeeting.Count)
        {
            currentState = ManagerState.InMeeting;
            Invoke("EndMeeting", Random.Range(20f, 30f));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check while we're waiting if we can start the meeting
    /// </summary>
    /// <returns>An enumerator</returns>
    private IEnumerator WaitingForMeetingToStart()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        bool startedMeeting = false;
        while(!startedMeeting)
        {
            yield return wait;
            startedMeeting = CheckToStartMeeting();
        }
    }

    /// <summary>
    /// Send the subordinates back to work and go back to the office
    /// </summary>
    private void EndMeeting()
    {
        for(int i = 0; i < subordinatesInMeeting.Count; i++)
        {
            subordinatesInMeeting[i].ReturnToWork();
        }
        subordinatesInMeeting.Clear();
        currentMeetingRoom.Seats.ResetSeats();
        currentMeetingRoom.IsTaken = false;
        currentMeetingRoom.NumberOfAgentsArrived = 0;
        GoToOffice();
    }
}
