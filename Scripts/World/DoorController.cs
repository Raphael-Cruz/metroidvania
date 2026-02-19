using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Configuração da Porta")]
    public string sceneToLoad;      
    public string targetDoorID;     
    public string thisDoorID;       

    [Header("Spawn")]
    [Tooltip("Coloque um GameObject filho posicionado no chão onde o player deve aparecer")]
    public Transform exitPoint;

    public enum FacingDirection { Left, Right }
    [Tooltip("Para qual direção o player olha ao entrar por esta porta")]
    public FacingDirection playerFacing = FacingDirection.Right;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (RoomTransitionManager.instance != null)
                RoomTransitionManager.instance.TransitionToRoom(sceneToLoad, targetDoorID);
        }

    }

    public Vector3 GetSpawnPosition()
    {
        if (exitPoint != null)
            return exitPoint.position;

        // Fallback: mesma posição da porta (avisa no editor)
        Debug.LogWarning($"[DoorController] '{thisDoorID}' sem exitPoint definido! Usando posição da porta.");
        return transform.position;
    }

    public float GetFacingX()
    {
        return playerFacing == FacingDirection.Right ? 1f : -1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, exitPoint.position);

            // Desenha seta indicando facing
            Gizmos.color = Color.blue;
            Vector3 dir = playerFacing == FacingDirection.Right ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(exitPoint.position, exitPoint.position + dir * 0.5f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}