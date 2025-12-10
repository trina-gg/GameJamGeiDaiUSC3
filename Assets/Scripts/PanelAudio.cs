using UnityEngine;
using System.Collections;

public class PanelAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip music;
    public bool loop = true;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Fade Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    AudioSource audioSource;
    SpritePanel myPanel;
    bool isPlaying = false;

    static PanelAudio currentlyPlaying;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = music;
        audioSource.loop = loop;
        audioSource.volume = 0f;
        audioSource.playOnAwake = false;

        myPanel = GetComponent<SpritePanel>();
    }

    void Start()
    {
        // Check if this is the starting panel
        StartCoroutine(CheckInitialPanel());
    }

    IEnumerator CheckInitialPanel()
    {
        // Wait one frame for SpritePanelManager to initialize
        yield return null;

        if (SpritePanelManager.Instance != null &&
            SpritePanelManager.Instance.currentPanel == myPanel)
        {
            PlayMusic();
        }
    }

    void Update()
    {
        if (SpritePanelManager.Instance == null || myPanel == null) return;

        bool isCurrentPanel = (SpritePanelManager.Instance.currentPanel == myPanel);

        // Started being current panel
        if (isCurrentPanel && !isPlaying)
        {
            PlayMusic();
        }
        // Stopped being current panel
        else if (!isCurrentPanel && isPlaying)
        {
            StopMusic();
        }
    }

    // Call this to start music early (before panel switch completes)
    public void StartMusicEarly()
    {
        if (!isPlaying)
        {
            PlayMusic();
        }
    }

    void PlayMusic()
    {
        if (music == null || isPlaying) return;

        // Stop previous panel's music
        if (currentlyPlaying != null && currentlyPlaying != this)
        {
            currentlyPlaying.StopMusic();
        }

        currentlyPlaying = this;
        isPlaying = true;

        StopAllCoroutines();
        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeCoroutine(0f, volume, fadeInDuration));

        Debug.Log("Playing music for: " + gameObject.name);
    }

    void StopMusic()
    {
        if (!isPlaying) return;

        isPlaying = false;
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop());

        Debug.Log("Stopping music for: " + gameObject.name);
    }

    IEnumerator FadeOutAndStop()
    {
        float startVol = audioSource.volume;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, timer / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    IEnumerator FadeCoroutine(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        audioSource.volume = to;
    }
}