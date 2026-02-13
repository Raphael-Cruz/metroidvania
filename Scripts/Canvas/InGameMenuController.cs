using UnityEngine;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject relicsPanel;
    public GameObject skillsPanel;
    public GameObject mapPanel;
    public GameObject systemPanel;

    [Header("Default State")]
    public GameObject menuObject; // The actual UI parent to toggle on/off

    public static bool isGamePaused = false;

    void Start()
    {
        isGamePaused = false;
        menuObject.SetActive(false);
    }

    void Update()
    {
        if (InputManager.instance.GetPauseDown()) // Toggle menu
        {
            ToggleMenu();
        }
    }

public void ToggleMenu()
    {
        bool isActive = !menuObject.activeSelf;
        menuObject.SetActive(isActive);
        
        isGamePaused = isActive; // Update the static variable
        Time.timeScale = isActive ? 0f : 1f; 

        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        
        if (isActive) OpenRelics();
    }

    public void OpenRelics() => SwitchTab(relicsPanel);
    public void OpenSkills() => SwitchTab(skillsPanel);
    public void OpenMap() => SwitchTab(mapPanel);
    public void OpenSystem() => SwitchTab(systemPanel);

    private void SwitchTab(GameObject activePanel)
    {
        relicsPanel.SetActive(false);
        skillsPanel.SetActive(false);
        mapPanel.SetActive(false);
        systemPanel.SetActive(false);

        activePanel.SetActive(true);
    }
}