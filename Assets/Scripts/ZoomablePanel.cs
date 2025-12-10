using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ZoomablePanel : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minZoom = 1f;
    public float maxZoom = 5f;
    public float zoomStep = 1.3f;
    public float zoomSmoothSpeed = 5f;

    [Header("Navigation")]
    public ZoomablePanel previousPanel;

    RectTransform rect;
    float currentZoom = 1f;
    float targetZoom = 1f;
    Vector2 currentPos = Vector2.zero;
    Vector2 targetPos = Vector2.zero;

    float lastLeftClick = 0f;
    float lastRightClick = 0f;
    float doubleClickTime = 0.3f;

    bool hotspotClaimed = false;
    bool isTransitioning = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isTransitioning)
        {
            HandleInput();
        }
        SmoothZoom();
    }

    void LateUpdate()
    {
        hotspotClaimed = false;
    }

    bool ClickedOnHotspot()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<SimpleHotspot>() != null)
                return true;
        }
        return false;
    }

    void HandleInput()
    {
        // Left click = zoom in
        if (Input.GetMouseButtonDown(0))
        {
            if (ClickedOnHotspot()) return;

            float t = Time.time - lastLeftClick;
            if (t <= doubleClickTime && t > 0)
            {
                if (!hotspotClaimed) ZoomIn();
                lastLeftClick = 0;
            }
            else
            {
                lastLeftClick = Time.time;
            }
        }

        // Right click = zoom out / go back
        if (Input.GetMouseButtonDown(1))
        {
            float t = Time.time - lastRightClick;
            if (t <= doubleClickTime && t > 0)
            {
                GoBack();
                lastRightClick = 0;
            }
            else
            {
                ZoomOut();
                lastRightClick = Time.time;
            }
        }
    }

    void SmoothZoom()
    {
        if (Mathf.Abs(currentZoom - targetZoom) > 0.001f)
        {
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothSpeed);
            rect.localScale = Vector3.one * currentZoom;
        }

        if (Vector2.Distance(currentPos, targetPos) > 0.1f)
        {
            currentPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * zoomSmoothSpeed);
            rect.anchoredPosition = currentPos;
        }
    }

    void ZoomIn()
    {
        if (targetZoom >= maxZoom) return;
        targetZoom = Mathf.Min(targetZoom * zoomStep, maxZoom);
    }

    void ZoomOut()
    {
        if (targetZoom <= minZoom) return;

        float oldZoom = targetZoom;
        targetZoom = Mathf.Max(targetZoom / zoomStep, minZoom);

        float ratio = targetZoom / oldZoom;
        targetPos *= ratio;
    }

    void GoBack()
    {
        Debug.Log("GoBack called on " + gameObject.name + ", targetZoom=" + targetZoom + ", minZoom=" + minZoom);

        if (targetZoom > minZoom + 0.05f)
        {
            Debug.Log("GoBack: Not at min zoom, returning");
            return;
        }
        if (previousPanel == null)
        {
            Debug.Log("GoBack: previousPanel is null, returning");
            return;
        }
        if (PanelManager.Instance == null)
        {
            Debug.Log("GoBack: PanelManager.Instance is null, returning");
            return;
        }

        Debug.Log("GoBack: Calling ZoomOutFromPanel");
        PanelManager.Instance.ZoomOutFromPanel(this);
    }

    public void ClaimDoubleClick()
    {
        hotspotClaimed = true;
    }

    public void SetTransitioning(bool val)
    {
        isTransitioning = val;
    }

    public float GetZoom() => currentZoom;

    public RectTransform GetRectTransform()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        return rect;
    }

    public void ResetZoom()
    {
        currentZoom = 1f;
        targetZoom = 1f;
        currentPos = Vector2.zero;
        targetPos = Vector2.zero;

        if (rect == null) rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchoredPosition = Vector2.zero;

        isTransitioning = false;
    }
}