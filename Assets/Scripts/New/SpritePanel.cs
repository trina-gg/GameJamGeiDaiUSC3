using UnityEngine;

public class SpritePanel : MonoBehaviour
{
    [Header("Navigation")]
    public SpritePanel parentPanel;        // The panel this one is inside of (null for root)

    [Header("Original Transform (set automatically at Start)")]
    public Vector3 originalPosition;
    public Vector3 originalScale;
    public Transform originalParent;

    bool hasStoredOriginals = false;

    void Start()
    {
        // Store original transform values at Start (not Awake)
        StoreOriginalTransform();
    }

    void StoreOriginalTransform()
    {
        if (!hasStoredOriginals)
        {
            originalPosition = transform.localPosition;
            originalScale = transform.localScale;
            originalParent = transform.parent;
            hasStoredOriginals = true;

            Debug.Log(gameObject.name + " stored originals - Pos: " + originalPosition + ", Scale: " + originalScale);
        }
    }

    public void RestoreOriginalTransform()
    {
        if (originalParent != null)
            transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;

        Debug.Log(gameObject.name + " restored to - Pos: " + originalPosition + ", Scale: " + originalScale);
    }

    public void BecomeFullScreen()
    {
        // Make sure we've stored originals before changing
        StoreOriginalTransform();

        // Move to root level, scale to 1, position at center
        transform.SetParent(null);
        transform.localScale = Vector3.one;
        transform.position = Vector3.zero;

        Debug.Log(gameObject.name + " became full screen");
    }
}