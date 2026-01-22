[System.Serializable]
public class GameData
{
    public string lastSceneName;
    public float[] respawnPos = new float[3];

    public System.Collections.Generic.List<string> collectedRelics = new();
    public System.Collections.Generic.List<string> collectedSkills = new();
    public System.Collections.Generic.List<string> permanentDeadEnemies = new();
    public System.Collections.Generic.List<string> completedEvents = new();
}
