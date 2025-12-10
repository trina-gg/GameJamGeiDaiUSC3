using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SimpleHotspot : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public ZoomablePanel myPanel;           // The panel this hotspot is on (GameDevDeskIntro)
    public RectTransform zoomTarget;        // What to zoom into (Mars RectTransform)
    public ZoomablePanel targetPanel;       // The panel to switch to after zoom (Mars ZoomablePanel)

    [Header("Settings")]
    public float doubleClickThreshold = 0.35f;
    public float zoomDuration = 1.0f;

    float lastClickTime = 0f;

    // Cached values calculated at Start
    Vector2 cachedTargetCenter;
    float cachedScaleToFill;

    // Store original Mars transform for zoom out
    Vector2 originalMarsAnchorMin;
    Vector2 originalMarsAnchorMax;
    Vector2 originalMarsOffsetMin;
    Vector2 originalMarsOffsetMax;
    Vector3 originalMarsScale;
    Vector2 originalMarsAnchoredPos;

    void Awake()
    {
        Image img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    void Start()
    {
        // Cache these values at start when hierarchy is correct
        cachedTargetCenter = CalculateTargetCenterInPanel();
        cachedScaleToFill = CalculateScaleToFillScreen();

        // Store original Mars transform
        if (zoomTarget != null)
        {
            originalMarsAnchorMin = zoomTarget.anchorMin;
            originalMarsAnchorMax = zoomTarget.anchorMax;
            originalMarsOffsetMin = zoomTarget.offsetMin;
            originalMarsOffsetMax = zoomTarget.offsetMax;
            originalMarsScale = zoomTarget.localScale;
            originalMarsAnchoredPos = zoomTarget.anchoredPosition;
        }

        Debug.Log("SimpleHotspot cached - Center: " + cachedTargetCenter + ", Scale: " + cachedScaleToFill);
    }

    public void RestoreMarsTransform()
    {
        if (zoomTarget != null)
        {
            zoomTarget.anchorMin = originalMarsAnchorMin;
            zoomTarget.anchorMax = originalMarsAnchorMax;
            zoomTarget.offsetMin = originalMarsOffsetMin;
            zoomTarget.offsetMax = originalMarsOffsetMax;
            zoomTarget.localScale = originalMarsScale;
            zoomTarget.anchoredPosition = originalMarsAnchoredPos;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (PanelManager.Instance == null || myPanel == null || zoomTarget == null || targetPanel == null) return;
        if (PanelManager.Instance.currentPanel != myPanel) return;

        float timeSinceLast = Time.time - lastClickTime;
        bool isDoubleClick = (timeSinceLast <= doubleClickThreshold) && (timeSinceLast > 0f);
        lastClickTime = Time.time;

        if (isDoubleClick)
        {
            myPanel.ClaimDoubleClick();
            PanelManager.Instance.ZoomIntoTarget(this);
        }
    }

    // Get the center of the zoom target (Mars) in the panel's local space
    Vector2 CalculateTargetCenterInPanel()
    {
        Vector3[] corners = new Vector3[4];
        zoomTarget.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) / 2f;

        RectTransform panelRect = myPanel.GetComponent<RectTransform>();
        Vector3 localPos = panelRect.InverseTransformPoint(worldCenter);
        return new Vector2(localPos.x, localPos.y);
    }

    // Calculate how much to zoom so the target fills the screen
    float CalculateScaleToFillScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().rect.size;

        // Get the target's ACTUAL displayed size (rect size * scale)
        Vector2 targetSize = zoomTarget.rect.size;
        Vector3 targetScale = zoomTarget.localScale;

        // Actual visible size is rect size multiplied by scale
        float actualWidth = targetSize.x * targetScale.x;
        float actualHeight = targetSize.y * targetScale.y;

        // We need to scale up until this small Mars fills the whole screen
        float scaleX = canvasSize.x / actualWidth;
        float scaleY = canvasSize.y / actualHeight;

        // Use the LARGER scale to ensure Mars fills the screen completely
        float scale = Mathf.Max(scaleX, scaleY);

        Debug.Log("Canvas: " + canvasSize + ", Target actual size: " + actualWidth + "x" + actualHeight + ", Scale needed: " + scale);

        return scale;
    }

    // Public getters return cached values
    public Vector2 GetTargetCenterInPanel() => cachedTargetCenter;
    public float GetScaleToFillScreen() => cachedScaleToFill;
}