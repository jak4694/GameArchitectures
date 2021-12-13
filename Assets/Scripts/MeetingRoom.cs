using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing a meeting room
/// </summary>
public class MeetingRoom : MonoBehaviour
{
    [SerializeField]
    private SeatingArea seats;
    private bool isTaken = false;
    private int numberOfAgentsArrived;
    
    /// <summary>
    /// Property to get the seating area data structure
    /// </summary>
    public SeatingArea Seats { get { return seats; } }

    /// <summary>
    /// Property to get or set if the meeting room is taken
    /// </summary>
    public bool IsTaken
    {
        get { return isTaken; }
        set { isTaken = value; }
    }

    /// <summary>
    /// Property to get or set the number of agents that have arrived
    /// </summary>
    public int NumberOfAgentsArrived
    {
        get { return numberOfAgentsArrived; }
        set { numberOfAgentsArrived = value; }
    }
}
