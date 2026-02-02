using UnityEngine; 

public class DoorEvent : MonoBehaviour { 

    [Header("Door")] 
    [SerializeField] private DoorLever cyberDoor; 
    [SerializeField] private GameObject doorLocked; 
    [SerializeField] private GameObject doorOpen; 

 void Start()
    {
   
            // Initial state: Locked
            if (doorLocked != null) doorLocked.SetActive(false);
            if (doorOpen != null) doorOpen.SetActive(true);
           
    }

    void OnTriggerEnter2D(Collider2D collision) { 
        if (!collision.gameObject.CompareTag("Player")) return; 
        SwitchDoorState(); 

    } 
   

    private void SwitchDoorState() {
        if (doorLocked != null) doorLocked.SetActive(true); 
        if (doorOpen != null) doorOpen.SetActive(false);
    }

    }

