using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Header("Starting Panel")]
    public ZoomablePanel currentPanel;

    SimpleHotspot lastHotspot;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(true);
            currentPanel.ResetZoom();
        }
    }

    public void ZoomIntoTarget(SimpleHotspot hotspot)
    {
        lastHotspot = hotspot;
        StartCoroutine(ZoomInCoroutine(hotspot));
    }

    IEnumerator ZoomInCoroutine(SimpleHotspot hotspot)
    {
        ZoomablePanel fromPanel = hotspot.myPanel;
        ZoomablePanel toPanel = hotspot.targetPanel;
        RectTransform zoomTarget = hotspot.zoomTarget;

        fromPanel.SetTransitioning(true);

        RectTransform panelRect = fromPanel.GetComponent<RectTransform>();

        // Get where Mars is and how much to zoom
        Vector2 targetCenter = hotspot.GetTargetCenterInPanel();
        float targetScale = hotspot.GetScaleToFillScreen();

        Vector3 startScale = panelRect.localScale;
        Vector2 startPos = panelRect.anchoredPosition;

        // End values - zoom so Mars fills the screen
        Vector3 endScale = Vector3.one * targetScale;
        Vector2 endPos = -targetCenter * targetScale;

        float duration = hotspot.zoomDuration;
        float timer = 0f;

        // Zoom animation
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t); // Smoothstep

            panelRect.localScale = Vector3.Lerp(startScale, endScale, t);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        panelRect.localScale = endScale;
        panelRect.anchoredPosition = endPos;

        // Now Mars fills the screen - do the switch
        // Reparent Mars to canvas level
        Canvas canvas = fromPanel.GetComponentInParent<Canvas>();
        toPanel.transform.SetParent(canvas.transform, true);

        // Hide the old panel
        fromPanel.gameObject.SetActive(false);

        // Reset Mars to fill screen properly
        RectTransform toRect = toPanel.GetComponent<RectTransform>();
        toRect.anchorMin = Vector2.zero;
        toRect.anchorMax = Vector2.one;
        toRect.offsetMin = Vector2.zero;
        toRect.offsetMax = Vector2.zero;
        toRect.localScale = Vector3.one;
        toRect.anchoredPosition = Vector2.zero;

        currentPanel = toPanel;
        toPanel.ResetZoom();
        fromPanel.ResetZoom();

        Debug.Log("Zoom in complete: " + fromPanel.name + " -> " + toPanel.name);
    }

    public void ZoomOutFromPanel(ZoomablePanel fromPanel)
    {
        Debug.Log("ZoomOutFromPanel called, fromPanel=" + fromPanel.name);

        if (lastHotspot == null)
        {
            Debug.Log("No hotspot to go back through");
            return;
        }

        StartCoroutine(ZoomOutCoroutine(fromPanel, lastHotspot));
    }

    IEnumerator ZoomOutCoroutine(ZoomablePanel fromPanel, SimpleHotspot hotspot)
    {
        ZoomablePanel toPanel = hotspot.myPanel;

        fromPanel.SetTransitioning(true);

        // Get the cached values (calculated at Start when hierarchy was correct)
        Vector2 targetCenter = hotspot.GetTargetCenterInPanel();
        float zoomScale = hotspot.GetScaleToFillScreen();

        // Reparent Mars back into GameDevDeskIntro
        fromPanel.transform.SetParent(toPanel.transform, false);

        // Restore Mars to its original transform
        hotspot.RestoreMarsTransform();

        // Show the parent panel, starting zoomed in
        toPanel.gameObject.SetActive(true);
        RectTransform toRect = toPanel.GetComponent<RectTransform>();

        // Start zoomed in (Mars filling screen)
        Vector3 startScale = Vector3.one * zoomScale;
        Vector2 startPos = -targetCenter * zoomScale;

        toRect.localScale = startScale;
        toRect.anchoredPosition = startPos;

        // End at normal zoom
        Vector3 endScale = Vector3.one;
        Vector2 endPos = Vector2.zero;

        float duration = hotspot.zoomDuration;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t);

            toRect.localScale = Vector3.Lerp(startScale, endScale, t);
            toRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        toRect.localScale = endScale;
        toRect.anchoredPosition = endPos;

        currentPanel = toPanel;
        toPanel.ResetZoom();

        Debug.Log("Zoom out complete: " + fromPanel.name + " -> " + toPanel.name);
    }

    // Legacy methods
    public void SwitchToPanel(ZoomablePanel newPanel)
    {
        if (newPanel == null || newPanel == currentPanel) return;
        if (currentPanel != null) { currentPanel.gameObject.SetActive(false); currentPanel.ResetZoom(); }
        currentPanel = newPanel;
        currentPanel.gameObject.SetActive(true);
        currentPanel.ResetZoom();
    }

    public void SwitchWithAutoZoom(ZoomablePanel fromPanel, ZoomablePanel toPanel)
    {
        StartCoroutine(AutoZoom(fromPanel, toPanel));
    }

    public void SwitchWithAutoZoom(ZoomablePanel toPanel)
    {
        if (currentPanel != null) StartCoroutine(AutoZoom(currentPanel, toPanel));
    }

    // Backwards compatibility - same as SwitchWithAutoZoom
    public void PlaySeamlessTransition(ZoomablePanel fromPanel, ZoomablePanel toPanel, RectTransform hotspotRect = null)
    {
        StartCoroutine(AutoZoom(fromPanel, toPanel));
    }

    IEnumerator AutoZoom(ZoomablePanel from, ZoomablePanel to)
    {
        from.SetTransitioning(true);
        RectTransform rect = from.GetComponent<RectTransform>();
        Vector3 start = rect.localScale;
        Vector3 end = start * 2f;
        float t = 0f;
        while (t < 0.5f) { t += Time.deltaTime; rect.localScale = Vector3.Lerp(start, end, t / 0.5f); yield return null; }
        from.gameObject.SetActive(false); from.ResetZoom();
        currentPanel = to; to.gameObject.SetActive(true); to.ResetZoom();
    }
}