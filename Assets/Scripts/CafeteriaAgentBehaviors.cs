using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class handling all the behaviors
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CafeteriaAgentBehaviors : MonoBehaviour
{
    public enum AgentState { EnteringCafeteria, WaitingForFood, InLineForFood, ObtainingSnack, ObtainingDrink, CheckingLine,
        WaitingToPay, InLineToPay, MovingToSeat, Eating}

    private int id;
    private int initialHunger;
    private int hunger;
    private int initialThirst;
    private int thirsty;
    private string desiredFood;
    private Line desiredLine;
    private Seat currentSeat;
    private List<string> itemsObtained = new List<string>();
    [SerializeField]
    private AgentState currentState = AgentState.EnteringCafeteria;
    private NavMeshAgent agent;
    private Agent agentBehaviors;

    /// <summary>
    /// Property to get the nav mesh agent component
    /// </summary>
    public NavMeshAgent NavMeshAgent { get { return agent; } }

    /// <summary>
    /// Property to get or set the ID
    /// </summary>
    public int ID
    {
        get { return id; }
        set { id = value; }
    }

    /// <summary>
    /// Property to get the state of the agent
    /// </summary>
    public string CurrentState { get { return currentState.ToString(); } }

    /// <summary>
    /// Property to get a string of the obtained items
    /// </summary>
    public string ItemsObtained
    {
        get
        {
            if(itemsObtained.Count == 0)
            {
                return "None\n";
            }
            string items = "";
            for(int i = 0; i < itemsObtained.Count; i++)
            {
                items += itemsObtained[i] + "\n";
            }
            return items;
        }
    }

    /// <summary>
    /// Set up the parameters for the agent
    /// </summary>
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentBehaviors = GetComponent<Agent>();
    }

    /// <summary>
    /// Go to the cafeteria and begin the process
    /// </summary>
    public void StartCafeteriaJourney()
    {
        initialHunger = hunger = agentBehaviors.Hunger;
        initialThirst = thirsty = agentBehaviors.Thirst;
        //Start the agent walking into the cafeteria
        currentState = AgentState.EnteringCafeteria;
        agent.SetDestination(GameManager.Instance.CafeteriaEntrance);
    }

    /// <summary>
    /// When arriving at the destination, determine any actions or state transitions necessary
    /// </summary>
    public void PathArrival()
    {
        switch(currentState)
        {
            case AgentState.EnteringCafeteria:
                CheckWhichItemToObtain();
                break;
            case AgentState.WaitingForFood:
            case AgentState.WaitingToPay:
                GetNewWanderSpot();
                break;
            case AgentState.ObtainingSnack:
                hunger--;
                itemsObtained.Add("Snack");
                CheckWhichItemToObtain();
                break;
            case AgentState.ObtainingDrink:
                thirsty--;
                itemsObtained.Add("Drink");
                CheckWhichItemToObtain();
                break;
            case AgentState.CheckingLine:
                ArrivedToCheckLine();
                break;
            case AgentState.MovingToSeat:
                currentState = AgentState.Eating;
                Invoke("LeaveCafeteria", (initialHunger * 10) + ((initialThirst > 0) ? 5 : 0));
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Method for the line to inform the agent that they can get into line
    /// </summary>
    public void WaitForLineOver()
    {
        if(currentState == AgentState.WaitingForFood)
        {
            currentState = AgentState.InLineForFood;
        }
        else if(currentState == AgentState.WaitingToPay)
        {
            currentState = AgentState.InLineToPay;
        }
    }

    /// <summary>
    /// Method for the line to inform the agent that they've been served
    /// </summary>
    public void LineActionCompleted()
    {
        if(currentState == AgentState.InLineForFood)
        {
            hunger -= 2;
            itemsObtained.Add(desiredFood);
            CheckWhichItemToObtain();
        }
        else if(currentState == AgentState.InLineToPay)
        {
            Seat seat = GameManager.Instance.SeatingArea.GetSeat();
            if(seat != null)
            {
                currentState = AgentState.MovingToSeat;
                currentSeat = seat;
                seat.IsTaken = true;
                agent.SetDestination(seat.gameObject.transform.position);
                return;
            }
            LeaveCafeteria();
        }
    }

    /// <summary>
    /// Checks whether to get a main food item, a snack, a drink, or to go pay if all are done
    /// </summary>
    private void CheckWhichItemToObtain()
    {
        //If the agent is hungry enough for an entree, try to get an entree
        if(hunger >= 2)
        {
            FoodStation desiredFoodStation = GameManager.Instance.RandomFoodStation;
            desiredFood = desiredFoodStation.FoodName;
            desiredLine = desiredFoodStation.Line;
            agent.SetDestination(desiredLine.LineEntrance);
            currentState = AgentState.CheckingLine;
        }
        //If the agent is only hungry enough for a snack, go get a snack
        else if(hunger == 1)
        {
            currentState = AgentState.ObtainingSnack;
            agent.SetDestination(GameManager.Instance.SnackAreaDestination);
        }
        //If the agent wants a drink, go get a drink
        else if(initialThirst == thirsty && thirsty > 0)
        {
            currentState = AgentState.ObtainingDrink;
            agent.SetDestination(GameManager.Instance.DrinkAreaDestination);
        }
        //If the agent has gotten everything, try to go pay
        else
        {
            desiredLine = GameManager.Instance.PaymentLine;
            agent.SetDestination(desiredLine.LineEntrance);
            currentState = AgentState.CheckingLine;
        }
    }

    /// <summary>
    /// Check the line once we've arrived close enough to check
    /// </summary>
    private void ArrivedToCheckLine()
    {
        bool isGettingInLine = desiredLine.PutAgentInLine(this);
        if(isGettingInLine)
        {
            if(desiredLine.FoodLine)
            {
                currentState = AgentState.InLineForFood;
            }
            else
            {
                currentState = AgentState.InLineToPay;
            }
        }
        else
        {
            if(desiredLine.FoodLine)
            {
                currentState = AgentState.WaitingForFood;
            }
            else
            {
                currentState = AgentState.WaitingToPay;
            }
            GetNewWanderSpot();
        }
    }

    /// <summary>
    /// Get a new wander spot and start moving there
    /// </summary>
    private void GetNewWanderSpot()
    {
        agent.SetDestination(GameManager.Instance.CafeteriaWaitingAreaDestination);
    }

    /// <summary>
    /// Method to tell the agent to leave the cafeteria
    /// </summary>
    private void LeaveCafeteria()
    {
        if(currentState == AgentState.Eating && currentSeat != null)
        {
            currentSeat.IsTaken = false;
        }
        agentBehaviors.Hunger = hunger;
        agentBehaviors.Thirst = thirsty;
        agentBehaviors.Restlessness = Mathf.Max(agentBehaviors.Restlessness - 2, 0);
        agentBehaviors.ReturnToWork();
    }
}
