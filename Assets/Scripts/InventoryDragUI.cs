using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂在 Inventory UI 的 Image 上，负责：
/// - 鼠标按下/拖拽时让一个“拖拽图标”跟随鼠标
/// - 松手时，用屏幕坐标 → 世界坐标做 2D 射线检测，
///   看是否碰到 ItemReceiver
/// </summary>
public class InventoryDragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Icon")]
    public Image dragIcon;                // 一个单独的 Image（可以是 inventorySlotImage 的克隆）

    Canvas _canvas;
    bool _hasItem = false;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
        }
    }

    public void SetHasItem(bool hasItem)
    {
        _hasItem = hasItem;
        if (!hasItem && dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_hasItem || dragIcon == null) return;

        dragIcon.sprite = GetComponent<Image>().sprite;
        dragIcon.gameObject.SetActive(true);
        UpdateDragIconPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_hasItem || dragIcon == null) return;
        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_hasItem || dragIcon == null) return;

        dragIcon.gameObject.SetActive(false);

        // 松手时，用屏幕坐标做 2D 射线检测，看有没有 ItemReceiver
        TryDropOnWorld(eventData.position);
    }

    void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (_canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out Vector2 localPos);

        dragIcon.rectTransform.localPosition = localPos;
    }

    void TryDropOnWorld(Vector2 screenPos)
    {
        if (!InventoryManager.Instance || !InventoryManager.Instance.HasItem) return;

        // 使用 SpritePanelManager 的摄像机
        Camera cam = SpritePanelManager.Instance.mainCamera;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        // 用 OverlapPoint 或 Raycast 检测 ItemReceiver
        Collider2D hit = Physics2D.OverlapPoint(worldPos2D);
        if (hit == null) return;

        ItemReceiver receiver = hit.GetComponent<ItemReceiver>();
        if (receiver == null) return;

        // 尝试在这个接收点使用当前物品
        InventoryManager.Instance.UseItemAtReceiver(receiver);
    }
}
