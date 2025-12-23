using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    // 1. ADD THIS LINE so the menu knows which fader to use
    [SerializeField] private SceneFader fader; 

    [SerializeField] private string gameSceneName = "scene 1";

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        
        // 2. Change 'SceneFader' to 'fader' (the variable name)
        // 3. REMOVE the SceneManager.LoadScene line from here. 
        // The FadeOut script already handles loading the scene once it turns black!
        StartCoroutine(fader.FadeOut(gameSceneName));
    }

    public void OpenOptions()
    {
        Debug.Log("Opening Options...");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}