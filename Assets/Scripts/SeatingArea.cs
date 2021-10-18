using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that allows agents to determine a seat to sit at and keeps track of the seats
/// </summary>
public class SeatingArea : MonoBehaviour
{
    private Seat[] seats;

    /// <summary>
    /// Get all the seats in the seating area at the start
    /// </summary>
    private void Start()
    {
        seats = GetComponentsInChildren<Seat>();
    }

    /// <summary>
    /// Picks a random seat for the agent to "sit" at or lets the agent know no seats are available
    /// </summary>
    /// <returns>A seat that's available, or null if no seat is available</returns>
    public Seat GetSeat()
    {
        int randomIndex = Random.Range(0, seats.Length);
        for(int i = 0; i < seats.Length; i++)
        {
            Seat seat = seats[(i + randomIndex) % seats.Length];
            if(!seat.IsTaken)
            {
                seat.IsTaken = true;
                return seat;
            }
        }
        return null;
    }
}
