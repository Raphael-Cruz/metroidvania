using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // This saves the file in a hidden folder on your PC/Mac that persists after updates
            savePath = Application.persistentDataPath + "/savegame.dat";
        }
        else
        {
            Destroy(gameObject);
        }
    }
/* ONLY FOR TESTING, REALODS WHEN WE PRESS L
private void Update()
{
    if (Input.GetKeyDown(KeyCode.L))
    {
        GameData data = LoadGame();
        if (data != null)
        {
            // Set the respawn point for the current session
            Vector3 loadedPos = new Vector3(data.respawnPos[0], data.respawnPos[1], data.respawnPos[2]);
            RespawnController.instance.SetRespawnPoint(loadedPos);
            
            // Reload the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(data.lastSceneName);
        }
    }
}*/

public void SaveGame(string sceneName, Vector3 pos)
{
    GameData data = new GameData();
    data.lastSceneName = sceneName;
    data.respawnPos[0] = pos.x;
    data.respawnPos[1] = pos.y;
    data.respawnPos[2] = pos.z;

    string json = JsonUtility.ToJson(data);
    File.WriteAllText(savePath, json);
    
    Debug.Log("Game Saved to: " + savePath);

    // IMPORTANT: Reset enemies here too so they stay dead/alive 
    // correctly based on your save state logic
    if(EnemyStatusManager.instance != null)
    {
        EnemyStatusManager.instance.ResetDefeatedEnemies();
    }
}

    public GameData LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<GameData>(json);
        }
        
        Debug.LogWarning("No save file found!");
        return null;
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }
}