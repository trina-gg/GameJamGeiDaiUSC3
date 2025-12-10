using UnityEngine;

/// <summary>
/// 挂在场景里的“插槽/接收点”上：
/// - 需要 Collider2D（用于 OverlapPoint）
/// - acceptsItemId 决定接收哪一个物品
/// - 可以配置放下后物体的局部位置/缩放
/// - 如果配置了 nextPanel，则在成功放下后调用 ZoomToPanel 进入下一个画面
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemReceiver : MonoBehaviour
{
    [Header("Receiver Settings")]
    public string acceptsItemId;          // 要求的 InventoryItem.itemId

    [Tooltip("把物体放下时在本地坐标中的位置（相对于本对象）")]
    public Vector3 localPlaceOffset = Vector3.zero;

    [Tooltip("是否覆盖物体原来的 localScale")]
    public bool overrideLocalScale = false;
    public Vector3 localScaleOverride = Vector3.one;

    [Header("Optional: 解谜后跳转到下一个画面")]
    public SpritePanel nextPanel;         // 把东西放对后要 zoom 进去的 panel（比如火星画面）
    public float zoomDuration = 1.0f;

    public bool TryPlaceItem(InventoryItem item)
    {
        if (item == null) return false;

        // 判断物品是否正确
        if (!string.IsNullOrEmpty(acceptsItemId) &&
            !string.Equals(item.itemId, acceptsItemId))
        {
            return false;
        }

        // 把物体放到自己下面，这样之后 zoom 到对应 SpritePanel 的时候会一起缩放
        Vector3 targetLocalPos = localPlaceOffset;
        Vector3? scale = overrideLocalScale ? (Vector3?)localScaleOverride : null;

        item.ShowInWorld(transform, targetLocalPos, scale);

        // 如果有配置下一个 panel，就像 SpriteHotspot 一样调用 ZoomToPanel
        if (nextPanel != null && SpritePanelManager.Instance != null)
        {
            SpritePanelManager.Instance.ZoomToPanel(nextPanel, zoomDuration);
        }

        return true;
    }
}
