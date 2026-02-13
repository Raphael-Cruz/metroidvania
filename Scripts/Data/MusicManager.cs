using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;        
using System.Collections.Generic; 

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Settings")]
    public AudioSource audioSource;
    public float fadeSpeed = 0.5f;

    [System.Serializable]
    public class SceneTrack {
        public string sceneName;
        public AudioClip track;
    }

private Coroutine fadeCoroutine;

    public List<SceneTrack> playlist;

    void Awake() {
        // Singleton pattern to prevent duplicates
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Find the track for the current scene
        foreach (var item in playlist) {
            if (item.sceneName == scene.name) {
                PlayTrack(item.track);
                break;
            }
        }
    }

public void PlayTrack(AudioClip newTrack)
{
    if (audioSource.clip == newTrack)
        return;

    if (fadeCoroutine != null)
        StopCoroutine(fadeCoroutine);

    fadeCoroutine = StartCoroutine(Crossfade(newTrack));
}

private IEnumerator Crossfade(AudioClip newTrack)
{
    float startVolume = audioSource.volume;

    // Fade OUT
    while (audioSource.volume > 0f)
    {
        audioSource.volume -= Time.deltaTime * fadeSpeed;
        yield return null;
    }

    audioSource.volume = 0f;

    audioSource.clip = newTrack;
    audioSource.Play();

    // Fade IN
    while (audioSource.volume < startVolume)
    {
        audioSource.volume += Time.deltaTime * fadeSpeed;
        yield return null;
    }

    audioSource.volume = startVolume;
}


}