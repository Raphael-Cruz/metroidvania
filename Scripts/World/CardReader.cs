using UnityEngine;

public class CardReader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DoorLever linkedDoor;
    [SerializeField] private GameObject floatingButton;

    [Header("Settings")]
    [SerializeField] private string requiredRelicID = "AccessCard";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInside = false;
    private bool isUsed = false;

    private void Start()
    {
        if (floatingButton != null)
            floatingButton.SetActive(false);
            
        // If the door is already opened, we should mark this as used
        if (linkedDoor != null && linkedDoor.eventID != "")
        {
            string openKey = linkedDoor.eventID + "_Opened";
            if (WorldState.CompletedEvents.Contains(openKey))
            {
                isUsed = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isUsed) return;

        if (other.CompareTag("Player"))
        {
            playerInside = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            HidePrompt();
        }
    }

    private void Update()
    {
        if (isUsed) return;

        if (playerInside && InputManager.instance.GetInteractDown())
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        // Check if player has the card
        if (WorldState.CollectedRelics.Contains(requiredRelicID))
        {
            if (linkedDoor != null)
            {
                linkedDoor.OpenDoor();
                isUsed = true;
                HidePrompt();
                Debug.Log($"[CardReader] Access granted! Opening door: {linkedDoor.eventID}");
            }
            else
            {
                Debug.LogWarning("[CardReader] Access granted but no DoorLever linked!");
            }
        }
        else
        {
            // Optional: Add a "Locked" sound or UI feedback here
            Debug.Log($"[CardReader] Access denied. Need relic: {requiredRelicID}");
        }
    }

    private void ShowPrompt()
    {
        if (floatingButton != null)
        {
            floatingButton.SetActive(true);
            
            var follower = floatingButton.GetComponent<UIFollower>();
            if (follower != null)
            {
                follower.target = transform;
                follower.offset = new Vector3(0, 2, 0); 
            }

            var anim = floatingButton.GetComponent<FloatingButtonAnimator>();
            if (anim != null) anim.Show();
        }
    }

    private void HidePrompt()
    {
        if (floatingButton != null)
        {
            var anim = floatingButton.GetComponent<FloatingButtonAnimator>();
            if (anim != null) 
                anim.Hide();
            else
                floatingButton.SetActive(false);
        }
    }
}
