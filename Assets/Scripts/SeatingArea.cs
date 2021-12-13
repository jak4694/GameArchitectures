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

    /// <summary>
    /// Resets all seats to be not taken
    /// </summary>
    public void ResetSeats()
    {
        for(int i = 0; i < seats.Length; i++)
        {
            seats[i].IsTaken = false;
        }
    }

    /// <summary>
    /// Determine how many seats are taken
    /// </summary>
    /// <returns>The number of seats currently taken</returns>
    public int NumberOfSeatsTaken()
    {
        int count = 0;
        for(int i = 0; i < seats.Length; i++)
        {
            if(seats[i].IsTaken)
            {
                count++;
            }
        }
        return count;
    }
}
