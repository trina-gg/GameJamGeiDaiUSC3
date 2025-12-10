using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [Header("Animation Frames")]
    public Sprite[] frames;             // Drag all your PNGs here in order

    [Header("Settings")]
    public float frameRate = 8f;        // Frames per second
    public bool loop = true;            // Loop the animation?
    public bool playOnStart = true;     // Start playing automatically?

    SpriteRenderer spriteRenderer;
    int currentFrame = 0;
    float timer = 0f;
    bool isPlaying = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteAnimator needs a SpriteRenderer!");
            return;
        }

        if (frames.Length > 0 && playOnStart)
        {
            Play();
        }
    }

    void Update()
    {
        if (!isPlaying || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    isPlaying = false;
                    return;
                }
            }

            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    public void Play()
    {
        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        if (frames.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        isPlaying = true;
    }

    public void SetFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < frames.Length)
        {
            currentFrame = frameIndex;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}