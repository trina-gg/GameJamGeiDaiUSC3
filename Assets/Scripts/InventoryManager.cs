// InventoryManager.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的单格 Inventory：
/// - 同时只允许有一个物体
/// - itemIconImage：只负责显示物品图标（背景槽请用单独的 Image）
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    [Tooltip("只负责显示物品图标的 Image，不要把背景槽拖进来。")]
    public Image itemIconImage;

    [Header("Runtime State")]
    [Tooltip("当前被拿在 inventory 里的世界物体（本体隐藏在场景中）。")]
    public InventoryItem currentItem;
    public bool HasItem => currentItem != null;

    void Awake()
    {
        // 单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 游戏开始时，物品图标是隐藏的（背景槽由别的 Image 负责显示）
        if (itemIconImage != null)
        {
            itemIconImage.enabled = false;
            itemIconImage.sprite = null;
        }
    }

    /// <summary>
    /// 由 InventoryItem 调用：把物体放进 inventory。
    /// </summary>
    public void PickupItem(InventoryItem item)
    {
        if (item == null || HasItem) return;

        currentItem = item;
        currentItem.HideInWorld();

        if (itemIconImage != null)
        {
            itemIconImage.sprite = item.iconSprite;
            itemIconImage.enabled = true;   // 显示物品图标
        }

        var drag = itemIconImage != null
            ? itemIconImage.GetComponent<InventoryDragUI>()
            : null;

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

        // 物品已经被正确使用，UI 清空
        ClearIconAndState();
    }

    /// <summary>
    /// 如果你想丢弃/重置物品（比如玩家取消使用），
    /// 会把物品放回原来的 panel 位置。
    /// </summary>
    public void DropAndRestoreCurrentItem()
    {
        if (!HasItem) return;

        currentItem.RestoreToOriginal();
        ClearIconAndState();
    }

    void ClearIconAndState()
    {
        if (itemIconImage != null)
        {
            itemIconImage.enabled = false;  // 图标隐藏（背景槽仍然在）
            itemIconImage.sprite = null;
        }

        var drag = itemIconImage != null
            ? itemIconImage.GetComponent<InventoryDragUI>()
            : null;

        if (drag != null)
        {
            drag.SetHasItem(false);
        }

        currentItem = null;
    }
}
