using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A food station with a type of food and a name
/// </summary>
public class FoodStation : MonoBehaviour
{
    [SerializeField]
    private Line line;
    [SerializeField]
    private string foodName;

    /// <summary>
    /// Property to get the line object associated with the food station
    /// </summary>
    public Line Line { get { return line; } }

    /// <summary>
    /// Property to get the name of the food for the station
    /// </summary>
    public string FoodName { get { return foodName; } }
}
