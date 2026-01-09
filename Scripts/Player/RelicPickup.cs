using UnityEngine;
using UnityEngine.SceneManagement;

public class RelicPickup : MonoBehaviour
{
    public RelicData relicData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[RelicPickup] relicData is NULL? {relicData == null}");

         if (!other.CompareTag("Player"))
            return;

        PlayerAbilityTracker player = other.GetComponent<PlayerAbilityTracker>();
        if (player == null)
            player = other.GetComponentInParent<PlayerAbilityTracker>();

        if (player == null)
            return;

        Debug.Log($"[RelicPickup] Collected relic: {relicData.relicID}");

        //  Register relic
        WorldState.CollectedRelics.Add(relicData.relicID);

        //  Apply abilities immediately
        WorldState.ApplyAbilitiesToPlayer(player);

        //  Save
        SaveManager.instance?.SaveGame(
            SceneManager.GetActiveScene().name,
            other.transform.position
        );

        //  Persistence
        GetComponent<PersistenceObject>()?.MarkAsCollected();

        Destroy(gameObject);
    }
}
