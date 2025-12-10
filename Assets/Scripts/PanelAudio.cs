using UnityEngine;

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
    static PanelAudio currentlyPlaying;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = music;
        audioSource.loop = loop;
        audioSource.volume = 0f;
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        // When this panel becomes active, play its music
        if (music != null)
        {
            // Fade out previous panel's music
            if (currentlyPlaying != null && currentlyPlaying != this)
            {
                currentlyPlaying.FadeOut();
            }

            currentlyPlaying = this;
            FadeIn();
        }
    }

    void OnDisable()
    {
        // Stop audio when panel is hidden
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void FadeIn()
    {
        if (audioSource == null || music == null) return;

        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeCoroutine(0f, volume, fadeInDuration));
    }

    public void FadeOut()
    {
        if (audioSource == null) return;

        StartCoroutine(FadeCoroutine(audioSource.volume, 0f, fadeOutDuration));
    }

    System.Collections.IEnumerator FadeCoroutine(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            audioSource.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }
        audioSource.volume = to;

        if (to == 0f)
        {
            audioSource.Stop();
        }
    }
}