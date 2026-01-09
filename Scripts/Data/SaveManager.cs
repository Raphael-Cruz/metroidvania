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


public void SaveGame(string sceneName, Vector3 pos)
{
    GameData data = new GameData();
    data.lastSceneName = sceneName;
    data.respawnPos[0] = pos.x;
    data.respawnPos[1] = pos.y;
    data.respawnPos[2] = pos.z;

    // Grab the global progress
    data.collectedRelics = new System.Collections.Generic.List<string>(WorldState.CollectedRelics);
    data.collectedSkills = new System.Collections.Generic.List<string>(WorldState.UnlockedSkills);
    data.permanentDeadEnemies = new System.Collections.Generic.List<string>(WorldState.PermanentDeadEnemies);   

    string json = JsonUtility.ToJson(data);
    File.WriteAllText(savePath, json);
}

public GameData LoadGame()
{
    if (File.Exists(savePath))
    {
        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);
        
     
        WorldState.LoadFromData(data);
        
        return data;
    }
    
    Debug.LogWarning("No save file found!");
    return null;
}
    
    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }
}