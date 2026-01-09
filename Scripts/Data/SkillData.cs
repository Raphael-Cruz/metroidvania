using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
public class SkillData : ScriptableObject
{
    [Header("ID (MUST MATCH PersistenceObject.uniqueID)")]
    public string skillID;   // e.g. "Missile", "Dash"

    public Sprite icon;

    [Header("Ammo")]
    public int maxQuantity;
     public int currentQuantity;

    public Sprite conceptArt;

    [HideInInspector] public bool isCollected;

    public void ResetRuntimeData()
    {
        isCollected = false;
        currentQuantity = 0;
    }
}
