using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpriteHotspot : MonoBehaviour
{
    [Header("What to zoom into")]
    public SpritePanel targetPanel;         // The panel to zoom into (Mars)

    [Header("Settings")]
    public float doubleClickTime = 0.35f;
    public float zoomDuration = 1.0f;

    float lastClickTime = 0f;

    void OnMouseDown()
    {
        // Check if this hotspot's parent panel is the current active panel
        SpritePanel myPanel = GetComponentInParent<SpritePanel>();
        if (myPanel != null && SpritePanelManager.Instance.currentPanel != myPanel)
        {
            return; // Not on the active panel, ignore click
        }

        float timeSinceLast = Time.time - lastClickTime;
        bool isDoubleClick = (timeSinceLast <= doubleClickTime) && (timeSinceLast > 0f);
        lastClickTime = Time.time;

        if (isDoubleClick)
        {
            Debug.Log("Double-click on " + gameObject.name + " -> zooming to " + targetPanel.name);
            SpritePanelManager.Instance.ZoomToPanel(targetPanel, zoomDuration);
        }
    }
}