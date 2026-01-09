using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicSlot : MonoBehaviour
{
    public Image iconDisplay;
    public RelicData currentRelic;
    public TextMeshProUGUI descriptionArea;
    public Image conceptArtDisplay;

    public void SetAsEmpty()
    {
        currentRelic = null;

        iconDisplay.sprite = null;
        iconDisplay.color = new Color(0, 0, 0, 0);

        if (conceptArtDisplay != null)
        {
            conceptArtDisplay.sprite = null;
            conceptArtDisplay.color = new Color(0, 0, 0, 0);
        }
    }

    public void SetupSlot(RelicData data, TextMeshProUGUI descBox)
    {
        currentRelic = data;
        descriptionArea = descBox;

        if (currentRelic == null)
        {
            SetAsEmpty();
            return;
        }

        bool collected = currentRelic.IsCollected();

        if (collected)
        {
            iconDisplay.sprite = currentRelic.icon;
            iconDisplay.color = Color.white;
        }
        else
        {
            iconDisplay.sprite = null;
            iconDisplay.color = new Color(0, 0, 0, 0);
        }
    }

    public void OnSelectRelic()
    {
        if (descriptionArea == null || conceptArtDisplay == null)
            return;

        if (currentRelic != null && currentRelic.IsCollected())
        {
            descriptionArea.text =
                $"<b>{currentRelic.displayName}</b>\n\n{currentRelic.description}";

            conceptArtDisplay.sprite = currentRelic.conceptArt;
            conceptArtDisplay.color = Color.white;
        }
        else
        {
            descriptionArea.text = "Select a Relic to see its secrets...";

            conceptArtDisplay.sprite = null;
            conceptArtDisplay.color = new Color(0, 0, 0, 0.5f);
        }
    }
}
