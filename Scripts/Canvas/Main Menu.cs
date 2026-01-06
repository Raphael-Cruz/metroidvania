using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneFader fader; 
    [SerializeField] private string gameSceneName = "Scene 1";

    [Header("Continue Settings")]
    [SerializeField] private Button continueButton;

private void Start()
{
    // Direct check as a backup to the Singleton
    string path = Application.persistentDataPath + "/savegame.dat";
    
    if (continueButton != null)
    {
        // This checks the physical disk directly, bypassing script timing issues
        continueButton.interactable = System.IO.File.Exists(path);
        
        Debug.Log("Checking for save at: " + path + " | Found: " + continueButton.interactable);
    }
}

    public void StartGame()
    {
        Debug.Log("Start Button was Clicked!");
        if (fader != null) 
        {
            StartCoroutine(fader.FadeOut(gameSceneName));
        }
        else 
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

public void ContinueGame()
{
    Debug.Log("Continue Button Clicked!"); // 1. Did the click work?

    if (SaveManager.instance != null)
    {
        GameData data = SaveManager.instance.LoadGame();
        
        if (data != null)
        {
            Debug.Log("Loaded Scene Name from file: " + data.lastSceneName); // 2. What's in the file?
            
            if (!string.IsNullOrEmpty(data.lastSceneName))
            {
                StartCoroutine(fader.FadeOut(data.lastSceneName));
            }
            else
            {
                Debug.LogError("Save file found, but lastSceneName is EMPTY!");
            }
        }
        else
        {
            Debug.LogError("SaveManager failed to load GameData!");
        }
    }
    else
    {
        Debug.LogError("SaveManager.instance is NULL in the Main Menu!");
    }
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