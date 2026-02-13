using UnityEngine;

public class BossMusicTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip boss1;
    [SerializeField] private string bossEnemyID; // same ID used in DoorLever
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Already triggered once
        if (hasTriggered)
            return;

        // If boss is permanently dead → never trigger
        if (!string.IsNullOrEmpty(bossEnemyID) &&
            WorldState.PermanentDeadEnemies.Contains(bossEnemyID))
        {
            return;
        }

        hasTriggered = true;

        if (MusicManager.instance != null && boss1 != null)
        {
            MusicManager.instance.PlayTrack(boss1);
        }

        // Optional: disable trigger completely
        gameObject.SetActive(false);
    }
}
