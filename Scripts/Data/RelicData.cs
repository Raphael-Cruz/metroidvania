using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "Inventory/Relic")]
public class RelicData : ScriptableObject
{
    public string relicName;
    public Sprite icon;
    [TextArea]
    public string description;
    public Sprite conceptArt;
    public bool isCollected;
}