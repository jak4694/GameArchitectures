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
    [SerializeField]
    private int maxAgentCount = 60;
    private int currentAgentCount = 0;
    private int newAgentID = 1;
    private bool waitingForAgentsToLeave = false;
    [SerializeField]
    private GameObject agentPrefab = null;
    [SerializeField]
    private Transform agentSpawnLocation;
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
    private Transform cafeteriaExit = null;
    [SerializeField]
    private SeatingArea seatingArea = null;
    [SerializeField]
    private TextMeshProUGUI infoText = null;
    [SerializeField]
    private GameObject UI = null;
    private AgentBehaviors currentlySelectedAgent = null;

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
    /// Property to get the destination for the cafeteria exit
    /// </summary>
    public Vector3 CafeteriaExit { get { return cafeteriaExit.position; } }

    /// <summary>
    /// Property to get the seating area for the cafeteria
    /// </summary>
    public SeatingArea SeatingArea { get { return seatingArea; } }

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
    /// Start spawning agents
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;
        SpawnAgent();
    }

    /// <summary>
    /// Update the UI to show the info of the selected agent
    /// </summary>
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            UI.SetActive(!UI.activeSelf);
        }
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            if(Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out raycastHit))
            {
                AgentBehaviors agentCheck = raycastHit.transform.gameObject.GetComponent<AgentBehaviors>();
                if(agentCheck)
                {
                    if(currentlySelectedAgent != null)
                    {
                        currentlySelectedAgent.gameObject.GetComponentInChildren<Outline>().enabled = false;
                    }
                    currentlySelectedAgent = agentCheck;
                    currentlySelectedAgent.gameObject.GetComponentInChildren<Outline>().enabled = true;
                }
            }
        }
        if(currentlySelectedAgent == null)
        {
            infoText.text = "No Agent Selected";
        }
        else
        {
            string text = "Agent ID: " + currentlySelectedAgent.ID + "\n";
            text += "State: " + currentlySelectedAgent.CurrentState + "\n";
            text += "Items Obtained:" + "\n" + currentlySelectedAgent.ItemsObtained;
            infoText.text = text;
        }
    }

    /// <summary>
    /// Spawn in an agent
    /// </summary>
    private void SpawnAgent()
    {
        GameObject agent = Instantiate(agentPrefab, agentSpawnLocation);
        agent.GetComponent<AgentBehaviors>().ID = newAgentID;
        newAgentID++;
        currentAgentCount++;
        if(currentAgentCount < maxAgentCount)
        {
            Invoke("SpawnAgent", Random.Range(2f, 5f));
        }
        else
        {
            waitingForAgentsToLeave = true;
        }
    }

    /// <summary>
    /// Inform the game manager that an agent has despawned
    /// </summary>
    public void AgentDespawned()
    {
        currentAgentCount--;
        if(waitingForAgentsToLeave)
        {
            waitingForAgentsToLeave = false;
            SpawnAgent();
        }
    }
}
