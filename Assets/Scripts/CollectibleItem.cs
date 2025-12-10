using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CollectibleItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Info")]
    public string itemId;      // 比如 "rocket"
    public Sprite itemIcon;    // 背包里显示的图标（可以用自己的 sprite）

    [Header("Panel Gate")]
    [Tooltip("这个物品所在的 Panel（比如 Toybox）——只有当它是 PanelManager.currentPanel 时才能被拾取")]
    public ZoomablePanel myPanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[CollectibleItem] Clicked {itemId} on {gameObject.name}");

        // 必须是左键
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // 必须当前 panel 正好是自己所在的 panel
        if (PanelManager.Instance != null && myPanel != null)
        {
            if (PanelManager.Instance.currentPanel != myPanel)
            {
                Debug.Log($"[CollectibleItem] Click ignored, currentPanel = {PanelManager.Instance.currentPanel?.name}, myPanel = {myPanel.name}");
                return;
            }
        }

        var inv = InventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("[CollectibleItem] No InventoryManager.Instance in scene!");
            return;
        }

        // 单物品背包：已经有东西就不再捡
        if (inv.HasItem)
        {
            Debug.Log($"[CollectibleItem] Inventory already has item {inv.CurrentItemId}, ignore picking {itemId}");
            return;
        }

        // 如果没指定 icon，就从自己的 Image 里拿
        if (itemIcon == null)
        {
            Image img = GetComponent<Image>();
            if (img != null)
                itemIcon = img.sprite;
        }

        if (itemIcon == null)
        {
            Debug.LogWarning($"[CollectibleItem] itemIcon for {itemId} is null (no sprite to show in inventory)");
        }

        inv.AddItem(itemId, itemIcon);

        Debug.Log($"[CollectibleItem] Picked {itemId}, disable world object {gameObject.name}");
        gameObject.SetActive(false);
    }
}
