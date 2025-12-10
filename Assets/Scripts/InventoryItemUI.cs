using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Data")]
    [SerializeField] private string itemId;
    public string ItemId => itemId;

    [Header("UI")]
    public Image iconImage;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    public void Setup(string id, Sprite icon)
    {
        itemId = id;
        if (iconImage != null)
            iconImage.sprite = icon;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        originalAnchoredPos = rectTransform.anchoredPosition;

        Debug.Log($"[InventoryItemUI] Setup item {itemId}");
    }

    public void Clear()
    {
        Debug.Log($"[InventoryItemUI] Clear item {itemId}");
        itemId = null;
        if (iconImage != null)
            iconImage.sprite = null;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPos;
        }
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        originalParent = rectTransform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        originalParent = rectTransform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;

        rectTransform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false; // 让 DropTarget 能收到 OnDrop

        Debug.Log($"[InventoryItemUI] Begin drag {itemId}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, eventData.position, eventData.pressEventCamera, out pos))
        {
            rectTransform.anchoredPosition = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 无论是否放置成功，都回到背包原位。
        rectTransform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;

        Debug.Log($"[InventoryItemUI] End drag {itemId}");
    }
}
