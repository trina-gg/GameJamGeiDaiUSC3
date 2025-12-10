// InventoryDragUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂在 “物品图标那张 Image”（InventoryItemIcon）上，负责：
/// - 鼠标拖拽时，让 dragIcon 跟随鼠标
/// - 拖拽开始隐藏槽里的图标，拖拽结束根据是否成功使用物品决定是否恢复
/// </summary>
public class InventoryDragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Icon")]
    [Tooltip("跟随鼠标移动的 Image，一般放在同一个 Canvas 下，默认隐藏。")]
    public Image dragIcon;

    Canvas _canvas;
    Image _slotItemImage;   // 就是挂着这个脚本的 Image
    bool _hasItem = false;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _slotItemImage = GetComponent<Image>();

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 由 InventoryManager 调整：当前是否有物品在格子里。
    /// </summary>
    public void SetHasItem(bool hasItem)
    {
        _hasItem = hasItem;

        // 是否显示格子里的物品图标，由 InventoryManager 控制为主；
        // 这里不强制改 enabled，以免覆盖 Manager 的逻辑。
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_hasItem || dragIcon == null || _slotItemImage == null) return;

        // 拖拽开始：拖拽图标用当前格子的 sprite
        dragIcon.sprite = _slotItemImage.sprite;
        dragIcon.gameObject.SetActive(true);
        UpdateDragIconPosition(eventData);

        // 槽里暂时不显示物品，只留下背景槽
        _slotItemImage.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_hasItem || dragIcon == null) return;
        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
        }

        // 尝试把物品放到世界里的接收点
        TryDropOnWorld(eventData.position);

        // 根据 Inventory 里是否还存在物品决定要不要恢复槽里的图标
        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.HasItem &&
            _slotItemImage != null)
        {
            // 物品还在 inventory 里，说明刚才没用成功 → 把图标恢复
            _slotItemImage.enabled = true;
        }
        // 否则：物品已经被正确使用，InventoryManager 会清空 icon，
        // 这里就保持空槽（只有背景）。
    }

    void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (_canvas == null || dragIcon == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            _canvas.worldCamera,
            out Vector2 localPos);

        dragIcon.rectTransform.localPosition = localPos;
    }

    void TryDropOnWorld(Vector2 screenPos)
    {
        if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem)
            return;

        // 使用 SpritePanelManager 的摄像机
        Camera cam = SpritePanelManager.Instance != null
            ? SpritePanelManager.Instance.mainCamera
            : Camera.main;

        if (cam == null) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        // 用 OverlapPoint 检测 ItemReceiver
        Collider2D hit = Physics2D.OverlapPoint(worldPos2D);
        if (hit == null) return;

        ItemReceiver receiver = hit.GetComponent<ItemReceiver>();
        if (receiver == null) return;

        // 尝试在这个接收点使用当前物品
        InventoryManager.Instance.UseItemAtReceiver(receiver);
    }
}
