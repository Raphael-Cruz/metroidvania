using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public int maxQuantity;
    public int currentQuantity;
    public Sprite conceptArt;
    public bool isCollected;
}