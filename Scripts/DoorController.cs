using UnityEngine;

public class DoorController : MonoBehaviour
{
    public enum ExitDirection { Left, Right, Up, Down }

    [Header("Configuração da Porta")]
    public string sceneToLoad;      
    public string targetDoorID;     
    public string thisDoorID;       

    [Header("Direção da Saída")]
    public ExitDirection exitDirection = ExitDirection.Right;
    public float distancePath = 1.5f; // Distância da porta

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (RoomTransitionManager.instance != null)
            {
                RoomTransitionManager.instance.TransitionToRoom(sceneToLoad, targetDoorID);
            }
        }
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 offset = Vector3.zero;

        // Calcula o offset baseado na direção escolhida
        switch (exitDirection)
        {
            case ExitDirection.Left:  offset = Vector3.left * distancePath; break;
            case ExitDirection.Right: offset = Vector3.right * distancePath; break;
            case ExitDirection.Up:    offset = Vector3.up * distancePath; break;
            case ExitDirection.Down:  offset = Vector3.down * distancePath; break;
        }

        return transform.position + offset;
    }

    // Desenha uma seta no Editor para você ver para onde o player vai sair
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = GetSpawnPosition();
        Gizmos.DrawWireSphere(spawnPos, 0.3f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
}