using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An area of destinations that can be picked from randomly
/// </summary>
public class DestinationArea : MonoBehaviour
{
    [SerializeField]
    private float width;
    [SerializeField]
    private float length;

    /// <summary>
    /// Get a random destination within the area
    /// </summary>
    /// <returns>Vector3 position of the chosen destination</returns>
    public Vector3 DestinationWithinArea()
    {
        return new Vector3(transform.position.x + Random.Range(-width / 2, width / 2), 
            transform.position.y, transform.position.z + Random.Range(-length / 2, length / 2));
    }

    /// <summary>
    /// Show the area of possible destinations when selected
    /// </summary>
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, 1, length));
    }
}
