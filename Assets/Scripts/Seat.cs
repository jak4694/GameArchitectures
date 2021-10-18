using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that handles a location for a seat and whether or not the seat is available
/// </summary>
public class Seat : MonoBehaviour
{
    [SerializeField]
    private Transform location;
    private bool isTaken;

    /// <summary>
    /// Property to get the location of the seat
    /// </summary>
    public Transform Location { get { return location; } }

    /// <summary>
    /// Property to get or set whether or not the seat is taken
    /// </summary>
    public bool IsTaken
    {
        get { return isTaken; }
        set { isTaken = value; }
    }
}
