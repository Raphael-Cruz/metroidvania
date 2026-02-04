using UnityEngine;

public class DoorLever : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject doorLocked1;    
    [SerializeField] private GameObject doorOpen1; 
    [SerializeField] private GameObject doorLocked2; 
    [SerializeField] private GameObject doorOpen2;   

    public enum InitialBehavior { Default, StayOpenIfBossAlive, StayClosedIfBossAlive }

    [Header("Settings")]
    [SerializeField] private bool isBossDoor = false; 
    [SerializeField] private string bossEnemyID; 
    [SerializeField] private InitialBehavior initialBehavior = InitialBehavior.Default;
    
    [Header("Persistence & ID")]
    [Tooltip("ID único desta porta (ex: Entrada_Arena, Saida_Arena).")]
    public string eventID; 
    
    private string OpenKey => eventID + "_Opened";
    private string ClosedKey => eventID + "_Closed";

    void Start()
    {
        if (string.IsNullOrEmpty(eventID)) return;

        // 1. Prioridade Máxima: Persistência Permanente (Porta aberta por chave/alavanca/boss morto)
        if (WorldState.CompletedEvents.Contains(OpenKey))
        {
            SetDoorOpenState();
            this.enabled = false; 
            return;
        }

        // 2. Prioridade de Boss: Se o boss está VIVO, o comportamento inicial manda (corrige respawn)
        if (!string.IsNullOrEmpty(bossEnemyID) && initialBehavior != InitialBehavior.Default)
        {
            bool isBossAlive = !WorldState.PermanentDeadEnemies.Contains(bossEnemyID);
            if (isBossAlive)
            {
                ApplyInitialBehavior_Internal();
                return; 
            }
        }

        // 3. Persistência de estado "Fechada" (após entrar na arena mas antes de vencer)
        if (WorldState.CompletedEvents.Contains(ClosedKey))
        {
            SetDoorClosedState();
        }
        else
        {
            // 4. Estado padrão se nada acima se aplicar
            SetDoorOpenState();
        }
    }

    private void ApplyInitialBehavior_Internal()
    {
        if (initialBehavior == InitialBehavior.StayOpenIfBossAlive)
        {
            SetDoorOpenState();
        }
        else if (initialBehavior == InitialBehavior.StayClosedIfBossAlive)
        {
            SetDoorClosedState();
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    { 
        if (!collision.gameObject.CompareTag("Player")) return; 
        if (WorldState.CompletedEvents.Contains(OpenKey)) return;

        CloseDoor();

        if (isBossDoor) 
        {
            // Só ativa se o boss não estiver na lista de mortos permanentes
            if (!string.IsNullOrEmpty(bossEnemyID) && WorldState.PermanentDeadEnemies.Contains(bossEnemyID))
            {
                return;
            }

            OpenBossHealthPanel();
        }
    } 

    public void OpenDoor()
    {
        WorldState.CompletedEvents.Add(OpenKey);
        WorldState.CompletedEvents.Remove(ClosedKey);
        SetDoorOpenState();
        this.enabled = false; 
    }

    public void CloseDoor()
    {
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
        if (BossHealthUI.instance != null) BossHealthUI.instance.gameObject.SetActive(true);
    }
}