using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for 'Image'
using TMPro;

public class RelicMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform gridParent;
    public TextMeshProUGUI descriptionText;
    public Image RelicDisplayImage; 
    public string defaultDescription = "Select a Relic to see its secrets...";

    [Header("Data")]
    public List<RelicData> allRelics = new List<RelicData>();

    private void OnEnable()
    {
        if (descriptionText != null) 
            descriptionText.text = defaultDescription;

        // Make the big square look 'empty' when the menu opens
        if (RelicDisplayImage != null)
        {
            RelicDisplayImage.sprite = null;
            RelicDisplayImage.color = new Color(0, 0, 0, 0.6f); 
        }

        RefreshGrid();
    }

    public void RefreshGrid()
    {
        if (gridParent == null) return;

        RelicSlot[] allSlots = gridParent.GetComponentsInChildren<RelicSlot>(true);

        for (int i = 0; i < allSlots.Length; i++)
        {
            // Pass the UI references to every slot
            allSlots[i].descriptionArea = descriptionText;
            allSlots[i].conceptArtDisplay = RelicDisplayImage;

            if (i < allRelics.Count && allRelics[i] != null)
            {
                allSlots[i].SetupSlot(allRelics[i], descriptionText);
            }
            else
            {
                allSlots[i].SetAsEmpty();
            }
        }
    }
}