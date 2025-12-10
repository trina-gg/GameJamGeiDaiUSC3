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
    public float zoomOutDuration = 1.5f;  // Faster zoom out

    [Header("Manual Zoom Settings")]
    public float manualZoomAmount = 0.7f;
    public float manualZoomSpeed = 0.3f;
    public float minManualZoom = 1f;

    [Header("Input Settings")]
    public float doubleClickTime = 0.3f;

    [Header("Bounce Effect")]
    public bool enableBounce = true;
    public float bounceAmount = 0.02f;     // How much overshoot (2%)
    public float bounceDuration = 0.15f;   // How long the bounce takes

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
            t = EaseOutQuart(t);

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
            t = EaseOutQuart(t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, endZoom, t);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.orthographicSize = endZoom;

        isTransitioning = false;
    }

    // Easing functions for more natural feel
    float EaseOutQuart(float t)
    {
        return 1f - Mathf.Pow(1f - t, 4f);
    }

    float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public void ZoomToPanel(SpritePanel targetPanel, float duration)
    {
        if (isTransitioning || targetPanel == null) return;
        StartCoroutine(ZoomInCoroutine(targetPanel, duration));
    }

    IEnumerator ZoomInCoroutine(SpritePanel targetPanel, float duration)
    {
        isTransitioning = true;

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

        // Start target sprite invisible using radial fade
        Material mat = sr.material;
        bool hasRadialFade = mat.HasProperty("_FadeProgress");

        if (hasRadialFade)
        {
            mat.SetFloat("_FadeProgress", 0f);
            mat.SetFloat("_FadeSoftness", 0.9f);
        }
        else
        {
            Color originalColor = sr.color;
            Color fadeColor = originalColor;
            fadeColor.a = 0f;
            sr.color = fadeColor;
        }

        // Fade starts earlier at 75% of zoom, completes at 99%
        float fadeStartPercent = 0.75f;
        float fadeEndPercent = 0.99f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Use easing for more natural zoom feel
            float smoothT = EaseInOutCubic(t);

            // Camera zoom
            mainCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startCamSize, endCamSize, smoothT);

            // Radial fade from center
            if (t >= fadeStartPercent)
            {
                float fadeT = (t - fadeStartPercent) / (fadeEndPercent - fadeStartPercent);
                fadeT = Mathf.Clamp01(fadeT);
                fadeT = EaseOutQuart(fadeT); // Smooth fade

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

        // Switch panels
        targetPanel.BecomeFullScreen();
        parentPanel.gameObject.SetActive(false);

        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.orthographicSize = defaultCameraSize;

        currentPanel = targetPanel;

        // Landing bounce effect
        if (enableBounce)
        {
            yield return StartCoroutine(LandingBounce());
        }

        isTransitioning = false;
    }

    IEnumerator LandingBounce()
    {
        // Get current panel's transform for bounce
        Transform panelTransform = currentPanel.transform;
        Vector3 normalScale = panelTransform.localScale;
        Vector3 bounceScale = normalScale * (1f + bounceAmount);

        // Bounce out (scale up slightly)
        float timer = 0f;
        float halfBounce = bounceDuration / 2f;

        while (timer < halfBounce)
        {
            timer += Time.deltaTime;
            float t = timer / halfBounce;
            t = EaseOutQuart(t);
            panelTransform.localScale = Vector3.Lerp(normalScale, bounceScale, t);
            yield return null;
        }

        // Bounce back (scale back to normal)
        timer = 0f;
        while (timer < halfBounce)
        {
            timer += Time.deltaTime;
            float t = timer / halfBounce;
            t = EaseInOutCubic(t);
            panelTransform.localScale = Vector3.Lerp(bounceScale, normalScale, t);
            yield return null;
        }

        panelTransform.localScale = normalScale;
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

        // Get sprite renderer for radial fade
        SpriteRenderer sr = currentPanel.GetComponent<SpriteRenderer>();
        Material mat = null;
        bool hasRadialFade = false;

        if (sr != null)
        {
            mat = sr.material;
            hasRadialFade = mat.HasProperty("_FadeProgress");
        }

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

        // Fade out completes at 25% of zoom out
        float fadeEndPercent = 0.25f;

        float timer = 0f;
        while (timer < zoomOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / zoomOutDuration;

            // Use easing for more natural zoom feel
            float smoothT = EaseInOutCubic(t);

            mainCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startCamSize, endCamSize, smoothT);

            // Radial fade out at the START of zoom out
            if (t <= fadeEndPercent)
            {
                float fadeT = t / fadeEndPercent;
                fadeT = Mathf.Clamp01(fadeT);
                fadeT = EaseOutQuart(fadeT);
                float fadeValue = 1f - fadeT;

                if (hasRadialFade)
                {
                    mat.SetFloat("_FadeProgress", fadeValue);
                }
                else if (sr != null)
                {
                    Color c = sr.color;
                    c.a = fadeValue;
                    sr.color = c;
                }
            }

            yield return null;
        }

        mainCamera.transform.position = endCamPos;
        mainCamera.orthographicSize = endCamSize;

        // Ensure fully faded out
        if (hasRadialFade)
        {
            mat.SetFloat("_FadeProgress", 0f);
        }
        else if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        currentPanel = parentPanel;
        isTransitioning = false;
    }
}