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
        // Singleton Robusto
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void TransitionToRoom(string sceneName, string targetDoorID)
    {
        StartCoroutine(TransitionRoutine(sceneName, targetDoorID));
    }

    private IEnumerator TransitionRoutine(string sceneName, string targetDoorID)
    {
        // 1. Avisa o RespawnController para ignorar o Start() do player na nova cena
        if (RespawnController.instance != null)
        {
            RespawnController.instance.isTransitioningBetweenRooms = true;
        }

        // 2. Trava o movimento do player
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        // 3. Fade Out
        if (UIController.instance != null)
        {
            UIController.instance.StartFadeToBlack();
        }
        yield return new WaitForSeconds(fadeDuration);

        // 4. Carrega a cena de forma assíncrona
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // 5. Pequena espera para os objetos da nova cena carregarem
        yield return null;

        // 6. Procura a porta de destino pelo ID
        DoorController[] allDoors = FindObjectsOfType<DoorController>();
        DoorController targetDoor = null;

        foreach (DoorController door in allDoors)
        {
            if (door.thisDoorID == targetDoorID)
            {
                targetDoor = door;
                break;
            }
        }

// 7. Reposiciona o Player e a Câmera (ONLY TELEPORT, NO SAVING)
        player = FindObjectOfType<PlayerMovement>();
        if (player != null && targetDoor != null)
        {
            Vector3 spawnPos = targetDoor.GetSpawnPosition();
            
            // This physically moves the player to the door
            player.transform.position = spawnPos;

            // This tells the camera to jump there instantly
            SetupCinemachine(player, spawnPos);
            
            Debug.Log("Player moved to door: " + targetDoorID);
        }
        else
        {
            if (targetDoor == null) Debug.LogError("Door ID '" + targetDoorID + "' not found in this scene!");
        }

        yield return new WaitForFixedUpdate();
        // 8. Fade In e libera o player
        if (UIController.instance != null)
        {
            UIController.instance.StartFadeFromBlack();
        }

        if (player != null) player.canMove = true;

        // 9. Libera o RespawnController
        if (RespawnController.instance != null)
        {
            RespawnController.instance.isTransitioningBetweenRooms = false;
        }
    }

    private void SetupCinemachine(PlayerMovement player, Vector3 newPos)
    {
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = player.transform;
            // Força a câmera a teleportar instantaneamente para o player
            vcam.OnTargetObjectWarped(player.transform, newPos - player.transform.position);
        }
    }
}