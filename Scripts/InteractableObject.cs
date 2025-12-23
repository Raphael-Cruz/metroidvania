using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject floatingButton;
    public GameObject interactionPanel;

    public KeyCode interactKey = KeyCode.E;
    private bool playerInside;

    private void Start()
    {
        floatingButton?.SetActive(false);
        interactionPanel?.SetActive(false);
    }

private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerInside = true;

        if (floatingButton != null)
        {
            floatingButton.SetActive(true);

           var follower = floatingButton.GetComponent<UIFollower>();
if (follower != null)
{
    follower.target = transform;
    follower.offset = new Vector3(0, 2, 0);  // ← Move upward on screen
}

            var anim = floatingButton.GetComponent<FloatingButtonAnimator>();
            if (anim != null) anim.Show();
        }

        Debug.Log("Floating button ON");
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerInside = false;

        if (floatingButton != null)
        {
            var anim = floatingButton.GetComponent<FloatingButtonAnimator>();
            if (anim != null) anim.Hide();
        }

        interactionPanel?.SetActive(false);

        Debug.Log("Floating button OFF");
    }
}

    // --- NEW ---
    public void ToggleInteraction()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(!interactionPanel.activeSelf);

        Debug.Log("Interaction toggled");
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(interactKey))
        {
            ToggleInteraction();
        }
    }
}
