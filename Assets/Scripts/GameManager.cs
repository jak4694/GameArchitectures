using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using cakeslice;

/// <summary>
/// A singleton that handles all of the references necessary for the agents to function
/// </summary>
public class GameManager : MonoBehaviour
{
    private Camera mainCamera;
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    [Header("Cafeteria")]
    [SerializeField]
    private Transform cafeteriaEntrance = null;
    [SerializeField]
    private DestinationArea cafeteriaWaitingArea = null;
    [SerializeField]
    private FoodStation[] foodStations;
    [SerializeField]
    private Line paymentLine = null;
    [SerializeField]
    private DestinationArea snackArea = null;
    [SerializeField]
    private DestinationArea drinkArea = null;
    [SerializeField]
    private SeatingArea seatingArea = null;
    [Header("Office")]
    [SerializeField]
    private Transform[] breakAreaDrinks;
    [SerializeField]
    private DestinationArea breakRoom = null;
    [SerializeField]
    private DestinationArea[] outsideAreas = null;
    [SerializeField]
    private Transform[] securityWaypoints;
    [SerializeField]
    private Transform[] warehouseDestinations;
    [SerializeField]
    private MeetingRoom[] meetingRooms;
    [Header("Other")]
    [SerializeField]
    private TextMeshProUGUI infoText = null;
    [SerializeField]
    private GameObject UI = null;
    private Agent currentlySelectedAgent = null;
    private Manager currentlySelectedManager = null;

    /// <summary>
    /// Property to get the destination for the cafeteria entrance
    /// </summary>
    public Vector3 CafeteriaEntrance { get { return cafeteriaEntrance.position; } }

    /// <summary>
    /// Property to get a waiting area destination for the cafeteria
    /// </summary>
    public Vector3 CafeteriaWaitingAreaDestination { get { return cafeteriaWaitingArea.DestinationWithinArea(); } }

    /// <summary>
    /// Property to get a random food station from the cafeteria
    /// </summary>
    public FoodStation RandomFoodStation { get { return foodStations[Random.Range(0, foodStations.Length)]; } }

    /// <summary>
    /// Property to get the payment line
    /// </summary>
    public Line PaymentLine { get { return paymentLine; } }

    /// <summary>
    /// Property to get the snack area destination
    /// </summary>
    public Vector3 SnackAreaDestination { get { return snackArea.DestinationWithinArea(); } }

    /// <summary>
    /// Property to get the drink area destination
    /// </summary>
    public Vector3 DrinkAreaDestination { get { return drinkArea.DestinationWithinArea(); } }

    /// <summary>
    /// Property to get the seating area for the cafeteria
    /// </summary>
    public SeatingArea SeatingArea { get { return seatingArea; } }

    /// <summary>
    /// Property to get the break area drinks
    /// </summary>
    public Transform BreakAreaDrinks
    {
        get { return breakAreaDrinks[Random.Range(0, breakAreaDrinks.Length)]; }
    }

    /// <summary>
    /// Property to get the break room area
    /// </summary>
    public DestinationArea BreakRoom { get { return breakRoom; } }

    /// <summary>
    /// Property to get an outside area
    /// </summary>
    public DestinationArea Outside
    {
        get { return outsideAreas[Random.Range(0, outsideAreas.Length)]; }
    }

    /// <summary>
    /// Property to get a security waypoint
    /// </summary>
    public Transform SecurityWaypoint
    {
        get { return securityWaypoints[Random.Range(0, securityWaypoints.Length)]; }
    }

    /// <summary>
    /// Property to get a warehouse destination
    /// </summary>
    public Transform WarehouseDestination
    {
        get { return warehouseDestinations[Random.Range(0, warehouseDestinations.Length)]; }
    }

    /// <summary>
    /// Property to get the first available meeting room, or none if they're not available
    /// </summary>
    public MeetingRoom MeetingRoom
    {
        get
        {
            for(int i = 0; i < meetingRooms.Length; i++)
            {
                if(!meetingRooms[i].IsTaken)
                {
                    return meetingRooms[i];
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Set up the game manager singleton
    /// </summary>
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    /// <summary>
    /// Set the camera reference
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Update the UI to show the info of the selected agent
    /// </summary>
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            UI.SetActive(!UI.activeSelf);
        }
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            if(Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out raycastHit))
            {
                Agent agentCheck = raycastHit.transform.gameObject.GetComponent<Agent>();
                Manager managerCheck = raycastHit.transform.gameObject.GetComponent<Manager>();
                if(agentCheck || managerCheck)
                {
                    if(currentlySelectedAgent != null)
                    {
                        currentlySelectedAgent.gameObject.GetComponentInChildren<Outline>().enabled = false;
                        currentlySelectedAgent = null;
                    }
                    if(currentlySelectedManager != null)
                    {
                        currentlySelectedManager.gameObject.GetComponentInChildren<Outline>().enabled = false;
                        currentlySelectedManager = null;
                    }
                    if(agentCheck)
                    {
                        currentlySelectedAgent = agentCheck;
                        currentlySelectedAgent.gameObject.GetComponentInChildren<Outline>().enabled = true;
                    }
                    else
                    {
                        currentlySelectedManager = managerCheck;
                        currentlySelectedManager.gameObject.GetComponentInChildren<Outline>().enabled = true;
                    }
                }
            }
        }
        if(currentlySelectedAgent == null && currentlySelectedManager == null)
        {
            infoText.text = "No Agent Selected";
        }
        else if(currentlySelectedAgent != null)
        {
            string text = "Worker type: ";
            if(currentlySelectedAgent is CubicleWorker)
            {
                text += "Cubicle\n";
            }
            else if(currentlySelectedAgent is SecurityWorker)
            {
                text += "Security\n";
            }
            else if(currentlySelectedAgent is WarehouseWorker)
            {
                text += "Warehouse\n";
            }
            else
            {
                text += "Unknown\n";
            }
            text += "Hunger: " + currentlySelectedAgent.Hunger + "\n";
            text += "Thirst: " + currentlySelectedAgent.Thirst + "\n";
            text += "Restlessness: " + currentlySelectedAgent.Restlessness.ToString("0.00") + "\n";
            text += "Status: " + currentlySelectedAgent.CurrentStateString;
            infoText.text = text;
        }
        else if(currentlySelectedManager != null)
        {
            infoText.text = "Worker type: Manager\nStatus: " + currentlySelectedManager.CurrentStateString;
        }
    }
}
