using UnityEngine;
using System.Collections;

public class SpritePanelManager : MonoBehaviour
{
    public static SpritePanelManager Instance;

    [Header("References")]
    public Camera mainCamera;
    public SpritePanel currentPanel;

    [Header("Camera Settings")]
    public float defaultCameraSize = 707f;
    public float zoomOutDuration = 2.0f;

    [Header("Manual Zoom Settings")]
    public float manualZoomAmount = 0.7f;
    public float manualZoomSpeed = 0.3f;
    public float minManualZoom = 1f;

    [Header("Input Settings")]
    public float doubleClickTime = 0.3f;

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
        hotspotClaimedClick = false;
    }

    public void ClaimClick()
    {
        hotspotClaimedClick = true;
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float t = Time.time - lastLeftClickTime;
            if (t <= doubleClickTime && t > 0)
            {
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

        if (Input.GetMouseButtonDown(1))
        {
            float t = Time.time - lastRightClickTime;
            if (t <= doubleClickTime && t > 0)
            {
                if (mainCamera.orthographicSize < defaultCameraSize - 0.1f)
                {
                    StartCoroutine(ManualZoomOut());
                }
                else
                {
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
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = -10;

        float newZoom = mainCamera.orthographicSize * manualZoomAmount;
        newZoom = Mathf.Max(newZoom, minManualZoom);

        if (mainCamera.orthographicSize <= minManualZoom + 0.1f)
            return;

        StartCoroutine(SmoothManualZoom(mouseWorld, newZoom));
    }

    IEnumerator SmoothManualZoom(Vector3 targetPos, float targetZoom)
    {
        isTransitioning = true;

        Vector3 startPos = mainCamera.transform.position;
        float startZoom = mainCamera.orthographicSize;

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

        Debug.Log("ZoomInCoroutine started with duration: " + duration);

        SpritePanel parentPanel = currentPanel;
        Vector3 targetWorldPos = targetPanel.transform.position;

        SpriteRenderer sr = targetPanel.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("No sprite renderer on " + targetPanel.name);
            isTransitioning = false;
            yield break;
        }

        Bounds bounds = sr.bounds;
        float spriteWorldHeight = bounds.size.y;
        float zoomedCameraSize = spriteWorldHeight / 2f;

        Vector3 startCamPos = mainCamera.transform.position;
        float startCamSize = mainCamera.orthographicSize;

        Vector3 endCamPos = new Vector3(targetWorldPos.x, targetWorldPos.y, -10);
        float endCamSize = zoomedCameraSize;

        Debug.Log("Start cam size: " + startCamSize + ", End cam size: " + endCamSize);

        // Start target sprite invisible using radial fade
        Material mat = sr.material;
        bool hasRadialFade = mat.HasProperty("_FadeProgress");

        if (hasRadialFade)
        {
            mat.SetFloat("_FadeProgress", 0f);
            mat.SetFloat("_FadeSoftness", 0.9f); // Almost full softness - very subtle radial
        }
        else
        {
            Color originalColor = sr.color;
            Color fadeColor = originalColor;
            fadeColor.a = 0f;
            sr.color = fadeColor;
        }

        // Fade starts at 90% of zoom, completes at 99.9%
        float fadeStartPercent = 0.90f;
        float fadeEndPercent = 0.999f;

        float timer = 0f;
        int frameCount = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            frameCount++;
            float t = timer / duration;
            float smoothT = t * t * (3f - 2f * t);

            // Camera zoom
            mainCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startCamSize, endCamSize, smoothT);

            // Radial fade from center - only in final moments
            if (t >= fadeStartPercent)
            {
                float fadeT = (t - fadeStartPercent) / (fadeEndPercent - fadeStartPercent);
                fadeT = Mathf.Clamp01(fadeT);

                if (hasRadialFade)
                {
                    mat.SetFloat("_FadeProgress", fadeT);
                }
                else
                {
                    Color c = sr.color;
                    c.a = fadeT;
                    sr.color = c;
                }
            }

            yield return null;
        }

        Debug.Log("While loop completed after " + frameCount + " frames");

        // Ensure fully visible
        if (hasRadialFade)
        {
            mat.SetFloat("_FadeProgress", 1f);
        }
        else
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        mainCamera.transform.position = endCamPos;
        mainCamera.orthographicSize = endCamSize;

        // Switch
        targetPanel.BecomeFullScreen();
        parentPanel.gameObject.SetActive(false);

        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.orthographicSize = defaultCameraSize;

        currentPanel = targetPanel;
        isTransitioning = false;
    }

    void GoBack()
    {
        if (currentPanel == null || currentPanel.parentPanel == null)
        {
            return;
        }

        StartCoroutine(ZoomOutCoroutine());
    }

    IEnumerator ZoomOutCoroutine()
    {
        isTransitioning = true;

        SpritePanel parentPanel = currentPanel.parentPanel;

        currentPanel.RestoreOriginalTransform();
        parentPanel.gameObject.SetActive(true);

        Vector3 targetWorldPos = currentPanel.transform.position;
        float targetScale = currentPanel.transform.localScale.x;

        float zoomedCameraSize = defaultCameraSize * targetScale;

        mainCamera.transform.position = new Vector3(targetWorldPos.x, targetWorldPos.y, -10);
        mainCamera.orthographicSize = zoomedCameraSize;

        Vector3 startCamPos = mainCamera.transform.position;
        float startCamSize = mainCamera.orthographicSize;

        Vector3 endCamPos = new Vector3(0, 0, -10);
        float endCamSize = defaultCameraSize;

        float timer = 0f;
        while (timer < zoomOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / zoomOutDuration;
            float smoothT = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startCamSize, endCamSize, smoothT);

            yield return null;
        }

        mainCamera.transform.position = endCamPos;
        mainCamera.orthographicSize = endCamSize;

        currentPanel = parentPanel;
        isTransitioning = false;
    }
}