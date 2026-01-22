using UnityEngine;

public class DoorLever : MonoBehaviour
{
    [SerializeField] private GameObject doorLocked; 
    [SerializeField] private GameObject doorOpen;   
    public string eventID; // Match this with your PersistenceObject uniqueID

    void Start()
    {
        // 1. Check if this door was ALREADY opened in a previous session
        if (!string.IsNullOrEmpty(eventID) && WorldState.CompletedEvents.Contains(eventID))
        {
            SetDoorOpenState();
        }
        else
        {
            // Initial state: Locked
            if (doorLocked != null) doorLocked.SetActive(true);
            if (doorOpen != null) doorOpen.SetActive(false);
        }
    }

    public void OpenDoor()
    {
        Debug.Log("Door is opening and saving to WorldState!");
        
        SetDoorOpenState();

        // 2. Save the event so it stays open forever
        if (!string.IsNullOrEmpty(eventID))
        {
            // Use your existing Persistence logic if you have a component attached,
            // or add it directly to WorldState here:
            if (!WorldState.CompletedEvents.Contains(eventID))
            {
                WorldState.CompletedEvents.Add(eventID);
            }
        }
    }

    private void SetDoorOpenState()
    {
        if (doorLocked != null) doorLocked.SetActive(false); 
        if (doorOpen != null) doorOpen.SetActive(true);
    }
}