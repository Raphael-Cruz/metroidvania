using UnityEngine;

public class DoorLever : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject doorLocked1;    
    [SerializeField] private GameObject doorOpen1; 
    [SerializeField] private GameObject doorLocked2; 
    [SerializeField] private GameObject doorOpen2;   
    [SerializeField] private GameObject BossHealthPanel;   

    [Header("Persistence Settings")]
    public string eventID; 
    
    private string OpenKey => eventID + "_Opened";
    private string ClosedKey => eventID + "_Closed";

    void Start()
    {
          if (BossHealthPanel) BossHealthPanel.SetActive(false);
        if (string.IsNullOrEmpty(eventID)) return;

            {
   
            // Initial state: open
            if (doorLocked1 != null) doorLocked1.SetActive(false);
            if (doorOpen1 != null) doorOpen1.SetActive(true);
            if (doorLocked2 != null) doorLocked2.SetActive(false);
            if (doorOpen2 != null) doorOpen2.SetActive(true);           
    }

        // PRIORITY 1: If the Boss is dead/Door is permanently opened, it stays open.
        if (WorldState.CompletedEvents.Contains(OpenKey))
        {
            SetDoorOpenState();
            this.enabled = false; 
        }
        // PRIORITY 2: If it was triggered to close and boss isn't dead yet.
        else if (WorldState.CompletedEvents.Contains(ClosedKey))
        {
            SetDoorClosedState();
            // We DON'T disable the script here because the boss will open it later!
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    { 
        if (!collision.gameObject.CompareTag("Player")) return; 

        // If the door is already permanently opened by boss, do nothing.
        if (WorldState.CompletedEvents.Contains(OpenKey)) return;

        // Otherwise, close it when player walks through trigger.
        CloseDoor();
        OpenBossHealthPanel();
    } 

    public void OpenDoor()
    {
        // Update WorldState to OPEN
        WorldState.CompletedEvents.Add(OpenKey);
        
        // Remove CLOSED state so it doesn't conflict on reload
        WorldState.CompletedEvents.Remove(ClosedKey);

        SetDoorOpenState();
        Debug.Log($"{eventID} is now permanently opened by Boss.");
        
        // Disable this script/trigger forever
        this.enabled = false; 
    }

    public void CloseDoor()
    {
        // Only close if it hasn't been permanently opened yet
        if (WorldState.CompletedEvents.Contains(OpenKey)) return;

        SetDoorClosedState();
        
        if (!WorldState.CompletedEvents.Contains(ClosedKey))
        {
            WorldState.CompletedEvents.Add(ClosedKey);
        }
    }

    private void SetDoorOpenState()
    {
        if (doorLocked1 != null) doorLocked1.SetActive(false); 
        if (doorOpen1 != null) doorOpen1.SetActive(true);
        if (doorLocked2 != null) doorLocked2.SetActive(false); 
        if (doorOpen2 != null) doorOpen2.SetActive(true);
    }

    private void SetDoorClosedState()
    {
        if (doorLocked1 != null) doorLocked1.SetActive(true); 
        if (doorOpen1 != null) doorOpen1.SetActive(false);
        if (doorLocked2 != null) doorLocked2.SetActive(true); 
        if (doorOpen2 != null) doorOpen2.SetActive(false);
    }
    
  public void OpenBossHealthPanel()
    {
        if (BossHealthPanel) BossHealthPanel.SetActive(true);
    }

    

}