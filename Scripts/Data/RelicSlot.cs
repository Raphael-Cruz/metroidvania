using UnityEngine;
using UnityEngine.UI;
using TMPro; 


public class RelicSlot : MonoBehaviour
{
    public Image iconDisplay;
    public RelicData currentRelic;
    public TextMeshProUGUI descriptionArea; 
    public UnityEngine.UI.Image conceptArtDisplay;

public void SetAsEmpty()
{
    currentRelic = null;
    
    // Make the icon invisible
    iconDisplay.sprite = null;
    iconDisplay.color = new Color(0, 0, 0, 0); 
    
    // Optional: Change the background frame alpha to look 'empty'
    // GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
}

public void SetupSlot(RelicData data, TextMeshProUGUI descBox)
{
    currentRelic = data;
    descriptionArea = descBox;
    
    // Only show the icon if it's collected; otherwise stay hidden/dark
    if (currentRelic.isCollected)
    {
        iconDisplay.sprite = currentRelic.icon;
        iconDisplay.color = Color.white;
    }
    else
    {
        // This shows the 'Empty' square even if the relic exists but isn't found
        iconDisplay.color = new Color(0, 0, 0, 0); 
    }
}

    
public void OnSelectRelic()
{
    if (descriptionArea == null || conceptArtDisplay == null) return;

    if (currentRelic != null && currentRelic.isCollected)
    {
        // Update Text
        descriptionArea.text = $"<b>{currentRelic.relicName}</b>\n\n{currentRelic.description}";
        
        // Update the big image
        conceptArtDisplay.sprite = currentRelic.conceptArt;
        conceptArtDisplay.color = Color.white; // Make it visible
    }
    else
    {
        descriptionArea.text = "Select a Relic to see its secrets...";
        
        // Clear the art and make it dark/opaque
        conceptArtDisplay.sprite = null;
        conceptArtDisplay.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
    }
}

}