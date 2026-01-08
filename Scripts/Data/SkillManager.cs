using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;


public class SkillManager : MonoBehaviour
{

    [Header("UI References")]
    public Transform gridParent;
    public Image SkillDisplayImage; 

    [Header("Data")]
    public List<SkillData> allSkills = new List<SkillData>();

private void OnEnable()
{
   
 if (SkillDisplayImage != null)
        {
            SkillDisplayImage.sprite = null;
            SkillDisplayImage.color = new Color(0, 0, 0, 0.6f); 
        }


    // Auto-select the first collected skill 
    SkillSlot[] allSlots = gridParent.GetComponentsInChildren<SkillSlot>();

    foreach (var slot in allSlots)
    {
        if (slot.currentSkill != null && slot.currentSkill.isCollected)
        {
            slot.OnSelectSkill(); // Show this skill immediately
            break; 
        }
    }
     RefreshGrid();
}

public void RefreshGrid()
{
  // if (gridParent == null) return;

    SkillSlot[] allSlots = gridParent.GetComponentsInChildren<SkillSlot>(true);

    for (int i = 0; i < allSlots.Length; i++)
    {
        allSlots[i].conceptArtDisplay = SkillDisplayImage;

        if (i < allSkills.Count && allSkills[i] != null)
        {
            allSlots[i].SetupSlot(allSkills[i]);

        }
        else
        {
            allSlots[i].SetAsEmpty();
        }
    }
}
}