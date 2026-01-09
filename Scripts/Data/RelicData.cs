using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "Inventory/Relic")]
public class RelicData : ScriptableObject
{
  
   [Header("Persistence ID")]
    public string relicID;
    [Header("UI")]
    public string displayName;
    public Sprite icon;

    [TextArea]
    public string description;

    public Sprite conceptArt;

    public bool IsCollected()
    {
        return WorldState.CollectedRelics.Contains(relicID);
    }
}


