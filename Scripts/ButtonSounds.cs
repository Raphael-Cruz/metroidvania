using UnityEngine;
using UnityEngine.EventSystems; // MUST HAVE THIS

public class ButtonSounds : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource source;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // This is the specific function for Hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        if (source && hoverSound) 
        {
            source.PlayOneShot(hoverSound);
        }
    }

    public void PlayClick()
    {
        if (source && clickSound) 
        {
            source.PlayOneShot(clickSound);
        }
    }
}