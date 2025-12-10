using UnityEngine;
using System.Collections;

public class SpritePanelManager : MonoBehaviour
{
    public static SpritePanelManager Instance;

    [Header("References")]
    public Camera mainCamera;
    public SpritePanel currentPanel;        // The currently active panel

    [Header("Camera Settings")]
    public float defaultCameraSize = 707f;  // Camera size when viewing a panel at full size

    [Header("Input Settings")]
    public float doubleClickTime = 0.3f;

    // Private
    float lastRightClickTime = 0f;
    bool isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        mainCamera.orthographicSize = defaultCameraSize;
        mainCamera.transform.position = new Vector3(0, 0, -10);
    }

    void Update()
    {
        if (!isTransitioning)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        // Right click - go back
        if (Input.GetMouseButtonDown(1))
        {
            float t = Time.time - lastRightClickTime;
            if (t <= doubleClickTime && t > 0)
            {
                GoBack();
                lastRightClickTime = 0f;
            }
            else
            {
                lastRightClickTime = Time.time;
            }
        }
    }

    public void ZoomToPanel(SpritePanel targetPanel, float duration)
    {
        if (isTransitioning || targetPanel == null) return;
        StartCoroutine(ZoomInCoroutine(targetPanel, duration));
    }

    IEnumerator ZoomInCoroutine(SpritePanel targetPanel, float duration)
    {
        isTransitioning = true;

        // Store the current panel as parent of target
        SpritePanel parentPanel = currentPanel;

        // Get target's current world position
        Vector3 targetWorldPos = targetPanel.transform.position;

        // Get the sprite renderer to find actual bounds
        SpriteRenderer sr = targetPanel.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("No sprite renderer on " + targetPanel.name);
            isTransitioning = false;
            yield break;
        }

        // Get the world-space bounds of the sprite (accounts for scale)
        Bounds bounds = sr.bounds;
        float spriteWorldHeight = bounds.size.y;

        // Camera size needed to fit this sprite = half the height
        float zoomedCameraSize = spriteWorldHeight / 2f;

        Debug.Log("Zoom IN - Target: " + targetPanel.name +
                  ", Bounds height: " + spriteWorldHeight +
                  ", Zoomed camera size: " + zoomedCameraSize);

        // Starting values
        Vector3 startCamPos = mainCamera.transform.position;
        float startCamSize = mainCamera.orthographicSize;

        // End values
        Vector3 endCamPos = new Vector3(targetWorldPos.x, targetWorldPos.y, -10);
        float endCamSize = zoomedCameraSize;

        // Animate zoom in
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t); // Smoothstep

            mainCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startCamSize, endCamSize, t);

            yield return null;
        }

        mainCamera.transform.position = endCamPos;
        mainCamera.orthographicSize = endCamSize;

        // === SWITCH === 
        // Now the target fills the screen visually
        // Make target the new "full size" panel

        // Unparent target and make it full size at origin
        targetPanel.BecomeFullScreen();

        // Hide the parent panel (and everything in it)
        parentPanel.gameObject.SetActive(false);

        // Reset camera to default position/size
        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.orthographicSize = defaultCameraSize;

        // Update current panel
        currentPanel = targetPanel;
        isTransitioning = false;

        Debug.Log("Now viewing: " + currentPanel.name);
    }

    void GoBack()
    {
        if (currentPanel == null || currentPanel.parentPanel == null)
        {
            Debug.Log("No parent panel to go back to");
            return;
        }

        StartCoroutine(ZoomOutCoroutine());
    }

    IEnumerator ZoomOutCoroutine()
    {
        isTransitioning = true;

        SpritePanel parentPanel = currentPanel.parentPanel;

        // Restore current panel to its original size/position inside parent
        currentPanel.RestoreOriginalTransform();

        // Show parent panel
        parentPanel.gameObject.SetActive(true);

        // Get where current panel now is (it's small again, inside parent)
        Vector3 targetWorldPos = currentPanel.transform.position;
        float targetScale = currentPanel.transform.localScale.x;

        // Camera should start zoomed in on the small panel
        float zoomedCameraSize = defaultCameraSize * targetScale;

        mainCamera.transform.position = new Vector3(targetWorldPos.x, targetWorldPos.y, -10);
        mainCamera.orthographicSize = zoomedCameraSize;

        Debug.Log("Zoom OUT - From: " + currentPanel.name + ", Scale: " + targetScale + ", Starting camera size: " + zoomedCameraSize);

        // End values (zoomed out to normal)
        Vector3 endCamPos = new Vector3(0, 0, -10);
        float endCamSize = defaultCameraSize;

        // Animate zoom out
        float duration = 1.0f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, endCamPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, endCamSize, t);

            yield return null;
        }

        mainCamera.transform.position = endCamPos;
        mainCamera.orthographicSize = endCamSize;

        currentPanel = parentPanel;
        isTransitioning = false;

        Debug.Log("Now viewing: " + currentPanel.name);
    }
}