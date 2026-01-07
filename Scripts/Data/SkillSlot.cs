using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SkillSlot : MonoBehaviour
{
    public Image iconDisplay;
    public SkillData currentSkill;
 
    public UnityEngine.UI.Image conceptArtDisplay;

public void SetAsEmpty()
{
    currentSkill = null;
    
    // Make the icon invisible
    iconDisplay.sprite = null;
    iconDisplay.color = new Color(0, 0, 0, 0); 
    
    // GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
}

public void SetupSlot(SkillData data )
{
    currentSkill = data;
    
    // Only show the icon if it's collected; otherwise stay hidden/dark
    if (currentSkill.isCollected)
    {
        iconDisplay.sprite = currentSkill.icon;
        iconDisplay.color = Color.white;
    }
    else
    {

        iconDisplay.color = new Color(0, 0, 0, 0); 
    }
}

    
public void OnSelectSkill()
{
    if ( conceptArtDisplay == null) return;

    if (currentSkill != null && currentSkill.isCollected)
    {
       
        // Update the big image
        conceptArtDisplay.sprite = currentSkill.conceptArt;
        conceptArtDisplay.color = Color.white; // Make it visible
    }
    else
    {
        
        // Clear the art and make it dark/opaque
        conceptArtDisplay.sprite = null;
        conceptArtDisplay.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
    }
}

}