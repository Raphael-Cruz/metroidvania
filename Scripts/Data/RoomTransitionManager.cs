using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class RoomTransitionManager : MonoBehaviour
{
    public static RoomTransitionManager instance;

    [Header("Transition Settings")]
    public float fadeDuration = 0.8f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToRoom(string sceneName, string targetDoorID)
    {
        StartCoroutine(TransitionRoutine(sceneName, targetDoorID));
    }

    private IEnumerator TransitionRoutine(string sceneName, string targetDoorID)
    {
        if (RespawnController.instance != null)
            RespawnController.instance.isTransitioningBetweenRooms = true;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
            player.canMove = false;

        if (UIController.instance != null)
            UIController.instance.StartFadeToBlack();

        yield return new WaitForSeconds(fadeDuration);

        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null;

        DoorController targetDoor = null;
        foreach (var door in FindObjectsOfType<DoorController>())
        {
            if (door.thisDoorID == targetDoorID)
            {
                targetDoor = door;
                break;
            }
        }

        player = FindObjectOfType<PlayerMovement>();

       if (player != null && targetDoor != null)
{
    Vector3 spawnPos = targetDoor.GetSpawnPosition();

    if (IsNaN(spawnPos))
    {
        Debug.LogError("SpawnPosition contém NaN!");
        yield break;
    }

    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.simulated = false;
    }

    SetupCinemachine(player, spawnPos);
    player.transform.position = spawnPos;
    player.GetComponent<PlayerMovement>()?.SetInitialDirection(targetDoor.GetFacingX());

    // Snap para o chão via raycast
    RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 5f, LayerMask.GetMask("Ground"));
    if (hit.collider != null)
    {
        // 1.2f = Ajusta posição Y para exatamente em cima do chão
        
        player.transform.position = new Vector3(spawnPos.x, hit.point.y + 1.2f, spawnPos.z);
    }

    yield return new WaitForFixedUpdate();

    if (rb != null)
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;
    }
}
        else
        {
            if (targetDoor == null)
                Debug.LogError($"Door ID '{targetDoorID}' não encontrada!");
        }

        if (UIController.instance != null)
            UIController.instance.StartFadeFromBlack();

        if (player != null)
            player.canMove = true;

        if (RespawnController.instance != null)
            RespawnController.instance.isTransitioningBetweenRooms = false;
    }

    private void SetupCinemachine(PlayerMovement player, Vector3 targetPos)
    {
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam == null) return;

        vcam.Follow = player.transform;

        Vector3 delta = targetPos - player.transform.position;

        if (!IsNaN(delta))
            vcam.OnTargetObjectWarped(player.transform, delta);
    }

    private bool IsNaN(Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }
}