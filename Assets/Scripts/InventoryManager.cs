using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的单格 Inventory：同时只允许有一个物体。
/// 负责：
/// - 接收 InventoryItem 的 Pickup 请求
/// - 管理 UI 图标
/// - 在物品成功使用后清空 inventory
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    public Image inventorySlotImage;          // UI 上的图标 Image（Screen Space - Overlay Canvas 下）

    [Header("Runtime State")]
    public InventoryItem currentItem;         // 当前拿着的世界物体（隐藏中）
    public bool HasItem => currentItem != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (inventorySlotImage != null)
        {
            inventorySlotImage.enabled = false;
        }
    }

    /// <summary>
    /// 由 InventoryItem 调用：把物体放进 inventory
    /// </summary>
    public void PickupItem(InventoryItem item)
    {
        if (item == null || HasItem) return;

        currentItem = item;
        currentItem.HideInWorld();

        if (inventorySlotImage != null)
        {
            inventorySlotImage.sprite = item.iconSprite;
            inventorySlotImage.enabled = true;
        }

        // 如果 InventorySlot 上挂了 InventoryDragUI，可以让它知道当前有物品
        var drag = inventorySlotImage.GetComponent<InventoryDragUI>();
        if (drag != null)
        {
            drag.SetHasItem(true);
        }
    }

    /// <summary>
    /// 在物品被成功使用（放到正确位置）时调用。
    /// 让物体回到世界中，并清空 UI。
    /// </summary>
    public void UseItemAtReceiver(ItemReceiver receiver)
    {
        if (!HasItem || receiver == null) return;

        // 让接收点根据逻辑决定如何摆放这个物体/是否接受
        bool accepted = receiver.TryPlaceItem(currentItem);
        if (!accepted) return;

        // UI 清空
        if (inventorySlotImage != null)
        {
            inventorySlotImage.enabled = false;
        }

        var drag = inventorySlotImage.GetComponent<InventoryDragUI>();
        if (drag != null)
        {
            drag.SetHasItem(false);
        }

        currentItem = null;
    }

    /// <summary>
    /// 如果你想丢弃/重置物品
    /// </summary>
    public void DropAndRestoreCurrentItem()
    {
        if (!HasItem) return;

        currentItem.RestoreToOriginal();

        if (inventorySlotImage != null)
        {
            inventorySlotImage.enabled = false;
        }

        var drag = inventorySlotImage.GetComponent<InventoryDragUI>();
        if (drag != null)
        {
            drag.SetHasItem(false);
        }

        currentItem = null;
    }
}
