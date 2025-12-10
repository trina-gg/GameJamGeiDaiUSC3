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
    public float zoomOutDuration = 2.0f;    // How long zoom out takes

    [Header("Manual Zoom Settings")]
    public float manualZoomAmount = 0.7f;   // How much to zoom (0.7 = 70% of current size)
    public float manualZoomSpeed = 0.3f;    // How fast manual zoom happens
    public float minManualZoom = 1f;        // Minimum camera size for manual zoom

    [Header("Input Settings")]
    public float doubleClickTime = 0.3f;

    // Private
    float lastRightClickTime = 0f;
    float lastLeftClickTime = 0f;
    bool isTransitioning = false;
    bool hotspotClaimedClick = false;

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

        // Reset hotspot claim each frame
        hotspotClaimedClick = false;
    }

    // Called by SpriteHotspot when it handles a click
    public void ClaimClick()
    {
        hotspotClaimedClick = true;
    }

    void HandleInput()
    {
        // Left click - manual zoom in
        if (Input.GetMouseButtonDown(0))
        {
            float t = Time.time - lastLeftClickTime;
            if (t <= doubleClickTime && t > 0)
            {
                // Double left click - manual zoom (if hotspot didn't claim it)
                if (!hotspotClaimedClick)
                {
                    ManualZoomIn();
                }
                lastLeftClickTime = 0f;
            }
            else
            {
                lastLeftClickTime = Time.time;
            }
        }

        // Right click - zoom out or go back
        if (Input.GetMouseButtonDown(1))
        {
            float t = Time.time - lastRightClickTime;
            if (t <= doubleClickTime && t > 0)
            {
                // Double right click - go back or zoom out
                if (mainCamera.orthographicSize < defaultCameraSize - 0.1f)
                {
                    // We're manually zoomed in, zoom back to default
                    StartCoroutine(ManualZoomOut());
                }
                else
                {
                    // We're at default zoom, go back to parent panel
                    GoBack();
                }
                lastRightClickTime = 0f;
            }
            else
            {
                lastRightClickTime = Time.time;
            }
        }
    }

    void ManualZoomIn()
    {
        // Get mouse position in world space
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = -10;

        // Calculate new zoom level
        float newZoom = mainCamera.orthographicSize * manualZoomAmount;
        newZoom = Mathf.Max(newZoom, minManualZoom);

        // Don't zoom if already at min
        if (mainCamera.orthographicSize <= minManualZoom + 0.1f)
            return;

        StartCoroutine(SmoothManualZoom(mouseWorld, newZoom));
    }

    IEnumerator SmoothManualZoom(Vector3 targetPos, float targetZoom)
    {
        isTransitioning = true;

        Vector3 startPos = mainCamera.transform.position;
        float startZoom = mainCamera.orthographicSize;

        // Move camera toward mouse position (but not all the way)
        Vector3 endPos = Vector3.Lerp(startPos, targetPos, 0.3f);
        endPos.z = -10;

        float timer = 0f;
        while (timer < manualZoomSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / manualZoomSpeed;
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.orthographicSize = targetZoom;

        isTransitioning = false;
    }

    IEnumerator ManualZoomOut()
    {
        isTransitioning = true;

        Vector3 startPos = mainCamera.transform.position;
        float startZoom = mainCamera.orthographicSize;

        Vector3 endPos = new Vector3(0, 0, -10);
        float endZoom = defaultCameraSize;

        float timer = 0f;
        while (timer < manualZoomSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / manualZoomSpeed;
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, endZoom, t);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.orthographicSize = endZoom;

        isTransitioning = false;
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
        float duration = zoomOutDuration;
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